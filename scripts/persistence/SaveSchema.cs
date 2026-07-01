using Hope.Config;

namespace Hope.Persistence;

/// <summary>
/// 存档格式版本与路径常量。
/// </summary>
public static class SaveSchema
{
    public const int CurrentVersion = 1;
    public static int MaxProfileSlots => (int)ParamsConfig.SaveMaxProfileSlots;
    public const string SavesRoot = "user://saves/";
    public const string MetaFileName = "meta";
    public const string CharacterFileName = "character";
    public const string CharacterTempFileName = "character.tmp";

    public static string GetSlotDirectory(int slotIndex) => $"{SavesRoot}slot_{slotIndex}/";

    public static string GetCharacterPath(int slotIndex) =>
        $"{GetSlotDirectory(slotIndex)}{CharacterFileName}";

    public static string GetCharacterTempPath(int slotIndex) =>
        $"{GetSlotDirectory(slotIndex)}{CharacterTempFileName}";
}
