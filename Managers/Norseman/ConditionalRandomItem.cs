using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using ServerSync;
using YamlDotNet.Serialization;

namespace Norsemen;

[Serializable]
public class ConditionalRandomItem
{
    public static string FolderPath = Path.Combine(ConfigManager.DirectoryPath, "Random Items");
    public static Dictionary<string, ConditionalRandomItem> items = new();
    public static CustomSyncedValue<string> sync = new(ConfigManager.ConfigSync, "RustyMods.ConditionalRandomItems.Sync", "");
    public static Dictionary<string, ConditionalRandomItem> configMap = new();
    
    public string Name = "";
    public string PrefabName = "";
    public string RequiredDefeatKey = "";
    public float Chance = 0.5f;
    public int Min = 1;
    public int Max = 1;

    [YamlIgnore]
    public Viking.ConditionalRandomItem? _item;
    [YamlIgnore]
    public Viking.ConditionalRandomItem item
    {
        get
        {
            if (_item != null) return _item;
            _item = new Viking.ConditionalRandomItem()
            {
                m_name = Name,
                m_prefab = Helpers.GetPrefab(PrefabName),
                m_requiredDefeatKey = RequiredDefeatKey,
                m_chance = Chance,
                m_min = Min,
                m_max = Max
            };
            return _item;
        }
    }

    public ConditionalRandomItem(string name, string prefab, int min = 1, int max = 1, float chance = 0.5f, string requiredDefeatKey = "")
    {
        Name = name;
        PrefabName = prefab;
        RequiredDefeatKey = requiredDefeatKey;
        Chance = chance;
        Min = min;
        Max = max;
        items[name] = this;
    }

    public ConditionalRandomItem()
    {
        if (string.IsNullOrEmpty(Name)) return;
        items[Name] = this;
    }

    public void GetOrSerialize()
    {
        string filePath = Path.Combine(FolderPath, $"{Name}.yml");
        if (File.Exists(filePath))
        {
            Read(filePath);
        }
        else
        {
            if (!Directory.Exists(FolderPath)) Directory.CreateDirectory(FolderPath);
            string data = ConfigManager.serializer.Serialize(this);
            File.WriteAllText(filePath, data);
        }
        
        configMap[filePath] = this;
    }

    public void Read(string filePath)
    {
        try
        {
            string text = File.ReadAllText(filePath);
            ConditionalRandomItem data = ConfigManager.deserializer.Deserialize<ConditionalRandomItem>(text);
            this.Copy(data);
            _item = null;
        }
        catch
        {
            NorsemenPlugin.LogError($"Failed to deserialize file: {Path.GetFileName(filePath)}");
        }
    }

    public static void Setup()
    {
        foreach (ConditionalRandomItem? item in items.Values)
        {
            item.GetOrSerialize();
        }
        
        string[] files = Directory.GetFiles(FolderPath, "*.yml");
        foreach (string filePath in files)
        {
            if (configMap.ContainsKey(filePath)) continue;
            ConditionalRandomItem item = new ConditionalRandomItem();
            item.Read(filePath);
            items[item.Name] = item;
            configMap[filePath] = item;
        }

        SetupWatcher();
        
        sync.ValueChanged += OnConfigChange;
    }

    public static void SetupWatcher()
    {
        FileSystemWatcher watcher = new(FolderPath, "*.yml");
        watcher.Changed += ReadConfigValues;
        watcher.Created += ReadConfigValues;
        watcher.Renamed += ReadConfigValues;
        watcher.IncludeSubdirectories = true;
        watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
        watcher.EnableRaisingEvents = true;
    }

    public static void ReadConfigValues(object sender, FileSystemEventArgs e)
    {
        string? filePath = e.FullPath;
        string? fileName = Path.GetFileName(filePath);
        if (!configMap.TryGetValue(filePath, out ConditionalRandomItem? item))
        {
            NorsemenPlugin.LogError($"{fileName} changed, but missing from config map");
        }
        else
        {
            item.Read(filePath);
        }
    }
    
    public static void OnZNetAwake(ZNet net)
    {
        if (!net.IsServer()) return;

        string data = ConfigManager.serializer.Serialize(items);
        sync.Value = data;
    }

    public static void OnConfigChange()
    {
        if (!ZNet.instance || ZNet.instance.IsServer()) return;
        string text = sync.Value;
        if (string.IsNullOrEmpty(text)) return;
        try
        {
            Dictionary<string, ConditionalRandomItem> data = ConfigManager.deserializer.Deserialize<Dictionary<string, ConditionalRandomItem>>(text);
            items.Clear();
            items.AddRange(data);
        }
        catch
        {
            NorsemenPlugin.LogError("Failed to deserialize server random sets");
        }
    }
    
}