using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using ServerSync;
using YamlDotNet.Serialization;

namespace Norsemen;

[Serializable]
public class ConditionalRandomWeapon
{
    public static string FolderPath = Path.Combine(ConfigManager.DirectoryPath, "Random Weapons");
    public static Dictionary<string, ConditionalRandomWeapon> weapons = new();
    public static CustomSyncedValue<string> sync = new(ConfigManager.ConfigSync, "RustyMods.ConditionalRandomWeapons.Sync", "");
    public static Dictionary<string, ConditionalRandomWeapon> configMap = new();

    public string Name = "";
    public string PrefabName = "";
    public string RequiredDefeatKey = "";
    public float Weight = 1f;

    [YamlIgnore] public Viking.ConditionalRandomWeapon? _weapon;

    [YamlIgnore]
    public Viking.ConditionalRandomWeapon weapon
    {
        get
        {
            if (_weapon != null) return _weapon;
            _weapon = new Viking.ConditionalRandomWeapon()
            {
                m_name = Name,
                m_requiredDefeatKey = RequiredDefeatKey,
                m_weight = Weight,
                m_prefab = Helpers.GetPrefab(PrefabName),
            };
            return _weapon;
        }
    }

    public ConditionalRandomWeapon()
    {
        if (string.IsNullOrEmpty(Name)) return;
        weapons[Name] = this;
    }

    public ConditionalRandomWeapon(string name, string prefabName, string requiredDefeatKey, float weight)
    {
        Name = name;
        PrefabName = prefabName;
        RequiredDefeatKey = requiredDefeatKey;
        Weight = weight;
        weapons[Name] = this;
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
            ConditionalRandomWeapon data = ConfigManager.deserializer.Deserialize<ConditionalRandomWeapon>(text);
            this.Copy(data);
            _weapon = null;
        }
        catch
        {
            NorsemenPlugin.LogError($"Failed to deserialize file: {Path.GetFileName(filePath)}");
        }
    }

    public static void Setup()
    {
        foreach (var weapon in weapons.Values)
        {
            weapon.GetOrSerialize();
        }
        
        string[] files = Directory.GetFiles(FolderPath, "*.yml");
        foreach (string filePath in files)
        {
            if (configMap.ContainsKey(filePath)) continue;
            ConditionalRandomWeapon weapon = new ConditionalRandomWeapon();
            weapon.Read(filePath);
            weapons[weapon.Name] = weapon;
            configMap[filePath] = weapon;
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
        if (!configMap.TryGetValue(filePath, out ConditionalRandomWeapon? weapon))
        {
            weapon = new ConditionalRandomWeapon();
            weapon.Read(filePath);
            weapons[weapon.Name] = weapon;
            configMap[filePath] = weapon;
            NorsemenPlugin.LogInfo($"{fileName} registered");
        }
        else
        {
            weapon.Read(filePath);
            NorsemenPlugin.LogInfo($"{fileName} changed");
        }
        UpdateSyncedFiles(ZNet.instance);
    }
    
    public static void UpdateSyncedFiles(ZNet net)
    {
        if (!net.IsServer()) return;

        string data = ConfigManager.serializer.Serialize(weapons);
        sync.Value = data;
    }
    
    public static void OnConfigChange()
    {
        if (!ZNet.instance || ZNet.instance.IsServer()) return;
        string text = sync.Value;
        if (string.IsNullOrEmpty(text)) return;
        try
        {
            Dictionary<string, ConditionalRandomWeapon> data = ConfigManager.deserializer.Deserialize<Dictionary<string, ConditionalRandomWeapon>>(text);
            weapons.Clear();
            weapons.AddRange(data);
        }
        catch
        {
            NorsemenPlugin.LogError("Failed to deserialize server random sets");
        }
    }
}