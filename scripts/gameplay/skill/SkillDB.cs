using Godot;
using System.Collections.Generic;
using System.Linq;

namespace Hope.SkillSystem;

/// <summary>
/// 技能数据库 Autoload：加载 <c>res://data/skills/</c> 下的 .tres 并注册代码目录中的技能。
/// </summary>
public partial class SkillDB : Node
{
    public static SkillDB? Instance { get; private set; }

    private readonly Dictionary<string, SkillDefinition> _definitions = new();
    private readonly Dictionary<ESkillTag, List<SkillDefinition>> _byTag = new();

    public override void _EnterTree()
    {
        Instance = this;
        ProcessMode = ProcessModeEnum.Always;
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
        LoadFromDirectory("res://data/skills");
        SkillCatalog.RegisterBarbarianSkills(this);
        GD.Print($"[SkillDB] 已加载 {_definitions.Count} 个技能定义");
    }

    /// <summary>注册技能定义（代码或 .tres 加载后调用）。</summary>
    public void Register(SkillDefinition def)
    {
        if (string.IsNullOrEmpty(def.SkillId))
        {
            return;
        }

        _definitions[def.SkillId] = def;

        if (!_byTag.ContainsKey(def.Tag))
        {
            _byTag[def.Tag] = [];
        }

        if (!_byTag[def.Tag].Any(s => s.SkillId == def.SkillId))
        {
            _byTag[def.Tag].Add(def);
        }
    }

    private void LoadFromDirectory(string basePath)
    {
        if (!DirAccess.DirExistsAbsolute(basePath))
        {
            return;
        }

        using var dir = DirAccess.Open(basePath);
        if (dir == null)
        {
            return;
        }

        CollectAndLoad(dir, basePath);
    }

    private void CollectAndLoad(DirAccess dir, string currentPath)
    {
        dir.ListDirBegin();
        var fileName = dir.GetNext();
        while (!string.IsNullOrEmpty(fileName))
        {
            if (fileName is "." or "..")
            {
                fileName = dir.GetNext();
                continue;
            }

            var fullPath = $"{currentPath}/{fileName}";
            if (dir.CurrentIsDir())
            {
                using var subDir = DirAccess.Open(fullPath);
                if (subDir != null)
                {
                    CollectAndLoad(subDir, fullPath);
                }
            }
            else if (fileName.EndsWith(".tres") || fileName.EndsWith(".res"))
            {
                var def = ResourceLoader.Load<SkillDefinition>(fullPath);
                if (def != null)
                {
                    Register(def);
                }
            }

            fileName = dir.GetNext();
        }

        dir.ListDirEnd();
    }

    public static SkillDefinition? GetDefinition(string skillId)
    {
        return Instance?._definitions.GetValueOrDefault(skillId);
    }

    public static List<SkillDefinition> GetSkillsByTag(ESkillTag tag)
    {
        if (Instance == null || !Instance._byTag.TryGetValue(tag, out var list))
        {
            return [];
        }

        return list.OrderBy(s => s.TreePositionY).ThenBy(s => s.TreePositionX).ToList();
    }

    public static List<SkillDefinition> GetAllSkills()
    {
        return Instance?._definitions.Values.ToList() ?? [];
    }

    public static bool Exists(string skillId)
    {
        return Instance != null && Instance._definitions.ContainsKey(skillId);
    }
}
