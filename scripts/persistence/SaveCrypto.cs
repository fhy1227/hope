using System;
using System.Security.Cryptography;
using System.Text;
using Godot;

namespace Hope.Persistence;

/// <summary>
/// 存档 AES-GCM 加解密；密钥由设备 ID 与固定盐派生，防止随手改 JSON。
/// 磁盘格式：<c>HOPE\x01</c> + nonce(12) + tag(16) + ciphertext。
/// </summary>
public static class SaveCrypto
{
    private const int KeySizeBytes = 32;
    private const int NonceSizeBytes = 12;
    private const int TagSizeBytes = 16;
    private const int Pbkdf2Iterations = 100_000;

    private static readonly byte[] FileMagic = "HOPE\x01"u8.ToArray();
    private static readonly byte[] KeySalt = "Hope.Save.v1"u8.ToArray();

    private static byte[]? _cachedKey;

    /// <summary>将明文 JSON 加密为写盘字节流。</summary>
    public static byte[] Encrypt(string plaintext)
    {
        var plainBytes = Encoding.UTF8.GetBytes(plaintext);
        var nonce = new byte[NonceSizeBytes];
        System.Security.Cryptography.RandomNumberGenerator.Fill(nonce);

        var cipherBytes = new byte[plainBytes.Length];
        var tag = new byte[TagSizeBytes];

        using (var aes = new AesGcm(GetKey(), TagSizeBytes))
        {
            aes.Encrypt(nonce, plainBytes, cipherBytes, tag);
        }

        var payload = new byte[FileMagic.Length + nonce.Length + tag.Length + cipherBytes.Length];
        var offset = 0;
        Buffer.BlockCopy(FileMagic, 0, payload, offset, FileMagic.Length);
        offset += FileMagic.Length;
        Buffer.BlockCopy(nonce, 0, payload, offset, nonce.Length);
        offset += nonce.Length;
        Buffer.BlockCopy(tag, 0, payload, offset, tag.Length);
        offset += tag.Length;
        Buffer.BlockCopy(cipherBytes, 0, payload, offset, cipherBytes.Length);
        return payload;
    }

    /// <summary>将磁盘字节流解密为明文 JSON；失败时 <paramref name="error"/> 说明原因。</summary>
    public static string? Decrypt(byte[] payload, out string? error)
    {
        error = null;
        var minSize = FileMagic.Length + NonceSizeBytes + TagSizeBytes;
        if (payload.Length < minSize)
        {
            error = "存档文件过短或已损坏";
            return null;
        }

        for (var i = 0; i < FileMagic.Length; i++)
        {
            if (payload[i] != FileMagic[i])
            {
                error = "存档格式无效";
                return null;
            }
        }

        var offset = FileMagic.Length;
        var nonce = payload.AsSpan(offset, NonceSizeBytes);
        offset += NonceSizeBytes;
        var tag = payload.AsSpan(offset, TagSizeBytes);
        offset += TagSizeBytes;
        var cipherBytes = payload.AsSpan(offset);

        var plainBytes = new byte[cipherBytes.Length];
        try
        {
            using var aes = new AesGcm(GetKey(), TagSizeBytes);
            aes.Decrypt(nonce, cipherBytes, tag, plainBytes);
        }
        catch (CryptographicException)
        {
            error = "存档解密失败（可能已篡改或非本机存档）";
            return null;
        }

        return Encoding.UTF8.GetString(plainBytes);
    }

    private static byte[] GetKey()
    {
        if (_cachedKey != null)
        {
            return _cachedKey;
        }

        var seed = GetDeviceSeed();
        _cachedKey = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(seed),
            KeySalt,
            Pbkdf2Iterations,
            HashAlgorithmName.SHA256,
            KeySizeBytes);
        return _cachedKey;
    }

    /// <summary>设备绑定种子：<see cref="OS.GetUniqueId"/> 不可用时回退到 user:// 绝对路径。</summary>
    private static string GetDeviceSeed()
    {
        var deviceId = OS.GetUniqueId();
        if (!string.IsNullOrEmpty(deviceId))
        {
            return deviceId;
        }

        return ProjectSettings.GlobalizePath("user://");
    }
}
