using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using ServerSync;

namespace Norsemen;

public static class NameGenerator
{
    [Serializable]
    public class Names
    {
        private static readonly Random rng = new Random();

        public List<string> MaleNames = new();
        public List<string> FemaleNames = new();
        
        public string GenerateMaleName()
        {
            string baseName = names.MaleNames[rng.Next(names.MaleNames.Count)];
            return baseName;
        }

        public string GenerateFemaleName()
        {
            string baseName = names.FemaleNames[rng.Next(names.FemaleNames.Count)];
            return baseName;
        }
    }

    public static Names names = new();

    public static CustomSyncedValue<string> sync = new(ConfigManager.ConfigSync, "RustyMods.Norsemen.Names.Sync", "");

    public static string FileName = "Names.yml";
    public static string FilePath = Path.Combine(ConfigManager.DirectoryPath, FileName);
    public static void Setup()
    {
        GetOrSerialize();
        SetupFileWatcher();
        sync.ValueChanged += OnConfigChange;
    }

    public static void SetupFileWatcher()
    {
        FileSystemWatcher watcher = new(ConfigManager.DirectoryPath, FileName);
        watcher.Changed += ReadConfigValues;
        watcher.Created += ReadConfigValues;
        watcher.Renamed += ReadConfigValues;
        watcher.IncludeSubdirectories = true;
        watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
        watcher.EnableRaisingEvents = true;
    }

    public static void OnConfigChange()
    {
        if (!ZNet.instance || ZNet.instance.IsServer()) return;
        string text = sync.Value;
        if (string.IsNullOrEmpty(text)) return;
        Names data = ConfigManager.deserializer.Deserialize<Names>(text);
        if (data.MaleNames.Count > 0)
        {
            names.MaleNames = data.MaleNames;
        }

        if (data.FemaleNames.Count > 0)
        {
            names.FemaleNames = data.FemaleNames;
        }
    }

    public static void ReadConfigValues(object sender, FileSystemEventArgs e)
    {
        if (!ZNet.instance || !ZNet.instance.IsServer()) return;
        Read();
        UpdateSync(ZNet.instance);
        NorsemenPlugin.LogDebug($"{FileName} changed");
    }

    public static void UpdateSync(ZNet net)
    {
        if (!net.IsServer()) return;
        string text = ConfigManager.serializer.Serialize(names);
        sync.Value = text;
    }

    public static void GetOrSerialize()
    {
        if (File.Exists(FilePath))
        {
            Read();
        }
        else
        {
            string text = EmbeddedResourceManager.GetFile("Names.yml");
            names = ConfigManager.deserializer.Deserialize<Names>(text);
            File.WriteAllText(FilePath, text);
        }
    }

    public static void Read()
    {
        try
        {
            string text = File.ReadAllText(FilePath);
            Names data = ConfigManager.deserializer.Deserialize<Names>(text);
            if (data.MaleNames.Count > 0)
            {
                names.MaleNames = data.MaleNames;
            }

            if (data.FemaleNames.Count > 0)
            {
                names.FemaleNames = data.FemaleNames;
            }
            
        }
        catch
        {
            NorsemenPlugin.LogError($"Failed to deserialize {FileName}");
        }
    }
    
    
}