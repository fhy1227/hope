using Godot;
using Hope.Config;

namespace Hope;

/// <summary>
/// 轻量音频管理：一条 BGM 通道 + SFX 对象池。
/// </summary>
public partial class AudioManager : Node
{
    public static AudioManager? Instance { get; private set; }

    private AudioStreamPlayer? _musicPlayer;
    private AudioStreamPlayer[] _sfxPool = [];
    private int _sfxIndex;

    public override void _EnterTree()
    {
        Instance = this;
    }

    public override void _ExitTree()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public override void _Ready()
    {
        var bus = ParamsConfig.AudioMasterBus;
        _musicPlayer = new AudioStreamPlayer { Bus = bus };
        AddChild(_musicPlayer);

        var poolSize = (int)ParamsConfig.AudioSfxPoolSize;
        _sfxPool = new AudioStreamPlayer[poolSize];
        for (var i = 0; i < poolSize; i++)
        {
            _sfxPool[i] = new AudioStreamPlayer { Bus = bus };
            AddChild(_sfxPool[i]);
        }
    }

    public void PlayMusic(AudioStream stream, bool loop = true)
    {
        if (stream == null)
        {
            return;
        }

        if (_musicPlayer == null) return;
        _musicPlayer.Stream = stream;
        _musicPlayer.Play();
    }

    public void StopMusic()
    {
        if (_musicPlayer == null) return;
        _musicPlayer.Stop();
    }

    public void PlaySfx(AudioStream stream)
    {
        if (stream == null)
        {
            return;
        }

        var poolSize = _sfxPool.Length;
        if (poolSize == 0)
        {
            return;
        }

        var player = _sfxPool[_sfxIndex];
        _sfxIndex = (_sfxIndex + 1) % poolSize;

        player.Stream = stream;
        player.Play();
    }
}
