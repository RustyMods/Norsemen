using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using ServerSync;
using YamlDotNet.Serialization;

namespace Norsemen;

[Serializable]
public class ConditionalRandomSet
{
    public static string FolderPath = Path.Combine(ConfigManager.DirectoryPath, "Random Sets");
    public static Dictionary<string, ConditionalRandomSet> sets = new();
    public static CustomSyncedValue<string> sync = new(ConfigManager.ConfigSync, "RustyMods.ConditionalRandomSets.Sync", "");
    public static Dictionary<string, ConditionalRandomSet> configMap = new();
    
    public string Name = "";
    public List<string> PrefabNames = new();
    public string RequiredDefeatKey = "";
    public float Weight = 1f;

    [YamlIgnore]
    public Viking.ConditionalItemSet? _set;
    [YamlIgnore]
    public Viking.ConditionalItemSet set
    {
        get
        {
            if (_set != null) return _set;
            _set = new Viking.ConditionalItemSet()
            {
                m_name = Name,
                m_requiredDefeatKey = RequiredDefeatKey,
                m_weight = Weight,
                m_items = PrefabNames.Select(Helpers.GetPrefab).ToArray()
            };
            return _set;
        }
    }

    public ConditionalRandomSet()
    {
        if (string.IsNullOrEmpty(Name)) return;
        sets[Name] = this;
    }

    public ConditionalRandomSet(string name, string requiredDefeatKey, float weight, params string[] prefabNames)
    {
        Name = name;
        RequiredDefeatKey = requiredDefeatKey;
        PrefabNames.Add(prefabNames);
        Weight = weight;
        sets[name] = this;
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
            ConditionalRandomSet data = ConfigManager.deserializer.Deserialize<ConditionalRandomSet>(text);
            this.Copy(data);
            _set = null;
        }
        catch
        {
            NorsemenPlugin.LogError($"Failed to deserialize file: {Path.GetFileName(filePath)}");
        }
    }
    public static void Setup()
    {
        foreach (ConditionalRandomSet? set in sets.Values)
        {
            set.GetOrSerialize();
        }
        
        string[] files = Directory.GetFiles(FolderPath, "*.yml");
        foreach (string filePath in files)
        {
            if (configMap.ContainsKey(filePath)) continue;
            ConditionalRandomSet set = new ConditionalRandomSet();
            set.Read(filePath);
            sets[set.Name] = set;
            configMap[filePath] = set;
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
        if (!ZNet.instance || !ZNet.instance.IsServer()) return;
        string? filePath = e.FullPath;
        string? fileName = Path.GetFileName(filePath);
        if (!configMap.TryGetValue(filePath, out ConditionalRandomSet? set))
        {
            set = new ConditionalRandomSet();
            set.Read(filePath);
            sets[set.Name] = set;
            configMap[filePath] = set;
            NorsemenPlugin.LogInfo($"{fileName} registered");
        }
        else
        {
            set.Read(filePath);
            NorsemenPlugin.LogInfo($"{fileName} changed");
        }
        UpdateSyncedFiles(ZNet.instance);
    }

    public static void UpdateSyncedFiles(ZNet net)
    {
        if (!net.IsServer()) return;

        string data = ConfigManager.serializer.Serialize(sets);
        sync.Value = data;
    }

    public static void OnConfigChange()
    {
        if (!ZNet.instance || ZNet.instance.IsServer()) return;
        string text = sync.Value;
        if (string.IsNullOrEmpty(text)) return;
        try
        {
            Dictionary<string, ConditionalRandomSet> data = ConfigManager.deserializer.Deserialize<Dictionary<string, ConditionalRandomSet>>(text);
            sets.Clear();
            sets.AddRange(data);
        }
        catch
        {
            NorsemenPlugin.LogError("Failed to deserialize server random sets");
        }
    }
}