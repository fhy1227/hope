using Godot;
using GodotArray = Godot.Collections.Array;
using GodotDictionary = Godot.Collections.Dictionary;
using System;
using System.Collections.Generic;

namespace Hope.Config;

/// <summary>
/// 配置反序列化接口 - 所有配置数据类实现此接口
/// </summary>
public interface IConfigData
{
	int Id { get; set; }
	void FromDict(GodotDictionary dict);
}

/// <summary>
/// 统一配置管理器
/// 自动加载 assets/config/ 下所有 JSON 配置并缓存
/// </summary>
public static class ConfigManager
{
	private static readonly Dictionary<string, GodotArray> _tableLists = new();
	private static readonly Dictionary<string, GodotDictionary> _tableDicts = new();
	private static readonly Dictionary<string, object> _typedLists = new();
	private static bool _loaded = false;

	public static bool IsLoaded => _loaded;

	/// <summary>
	/// 加载所有配置表（游戏启动时由 ConfigBootstrap 调用）
	/// </summary>
	public static void LoadAll()
	{
		if (_loaded) return;

		using var dir = DirAccess.Open(ParamsConfig.PathConfigDir);
		if (dir == null)
		{
			GD.PrintErr($"[ConfigManager] 无法打开配置目录: {ParamsConfig.PathConfigDir}");
			return;
		}

		dir.ListDirBegin();
		int count = 0;
		while (true)
		{
			var fileName = dir.GetNext();
			if (string.IsNullOrEmpty(fileName)) break;
			if (!fileName.EndsWith(".json")) continue;

			var configName = fileName.Replace(".json", "");
			var path = ParamsConfig.PathConfigDir + fileName;
			using var file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Read);
			if (file == null)
			{
				GD.PrintErr($"[ConfigManager] 无法加载: {path}");
				continue;
			}

			var parseResult = Json.ParseString(file.GetAsText());
			if (parseResult.VariantType != Variant.Type.Dictionary) continue;

			var root = parseResult.AsGodotDictionary();
			if (root.TryGetValue("_dict", out var dictVar))
			{
				var dict = dictVar.AsGodotDictionary();
				_tableDicts[configName] = dict;
				_tableLists[configName] = root.TryGetValue("_list", out var listVar)
					? listVar.AsGodotArray()
					: BuildListFromDict(dict);
			}
			count++;
		}
		dir.ListDirEnd();

		_loaded = true;
		GD.Print($"[ConfigManager] 加载完成: {count} 个配置表");
	}

	private static void EnsureLoaded()
	{
		if (!_loaded) LoadAll();
	}

	/// <summary>
	/// 从 _dict 的值构建列表（保持 JSON 键插入顺序）
	/// </summary>
	private static GodotArray BuildListFromDict(GodotDictionary dict)
	{
		var list = new GodotArray();
		foreach (var kv in dict)
			list.Add(kv.Value);
		return list;
	}

	/// <summary>
	/// 获取配置表列表（运行时由 _dict 生成，或兼容旧版 _list）
	/// </summary>
	public static GodotArray GetList(string tableName)
	{
		EnsureLoaded();
		return _tableLists.GetValueOrDefault(tableName, new GodotArray());
	}

	/// <summary>
	/// 获取配置表原始字典 (_dict)
	/// </summary>
	public static GodotDictionary GetDict(string tableName)
	{
		EnsureLoaded();
		return _tableDicts.GetValueOrDefault(tableName, new GodotDictionary());
	}

	/// <summary>
	/// 根据 Id 获取单行数据
	/// </summary>
	public static GodotDictionary GetRow(string tableName, int id)
	{
		EnsureLoaded();
		var key = id.ToString();
		if (_tableDicts.TryGetValue(tableName, out var dict)
			&& dict.TryGetValue(key, out var row))
		{
			return row.AsGodotDictionary();
		}
		return new GodotDictionary();
	}

	/// <summary>
	/// 获取泛型配置列表 (通过 IConfigData.FromDict 反序列化)
	/// </summary>
	public static List<T> GetAll<T>() where T : IConfigData, new()
	{
		EnsureLoaded();
		var tableName = GetTableName<T>();
		if (_typedLists.TryGetValue(tableName, out var cached))
			return (List<T>)cached;

		var list = GetList(tableName);
		var result = new List<T>(list.Count);
		foreach (var item in list)
		{
			var config = new T();
			config.FromDict(item.AsGodotDictionary());
			result.Add(config);
		}
		_typedLists[tableName] = result;
		return result;
	}

	/// <summary>
	/// 根据 Id 获取单条泛型配置
	/// </summary>
	public static T? Get<T>(int id) where T : IConfigData, new()
	{
		EnsureLoaded();
		var tableName = GetTableName<T>();

		var row = GetRow(tableName, id);
		if (row.Count == 0) return default;

		var config = new T();
		config.FromDict(row);
		return config;
	}

	private static string GetTableName<T>() where T : IConfigData
	{
		var name = typeof(T).Name;
		var tableName = name.EndsWith("Config")
			? name.Substring(0, name.Length - 6)
			: name;
		return ToSnakeCase(tableName);
	}

	/// <summary>
	/// PascalCase -> snake_case
	/// </summary>
	private static string ToSnakeCase(string input)
	{
		if (string.IsNullOrEmpty(input)) return input;
		var sb = new System.Text.StringBuilder();
		for (int i = 0; i < input.Length; i++)
		{
			if (char.IsUpper(input[i]))
			{
				if (sb.Length > 0) sb.Append('_');
				sb.Append(char.ToLower(input[i]));
			}
			else
			{
				sb.Append(input[i]);
			}
		}
		return sb.ToString();
	}
}
