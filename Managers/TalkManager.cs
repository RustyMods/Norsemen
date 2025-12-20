using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using ServerSync;

namespace Norsemen;

public static class TalkManager
{
    public enum TalkType
    {
        Generic, PlayerBase, Greets, Farewells, Damaged, Thieved, Puke, Eat
    }

    public static string FileName;
    public static string FilePath;
    public static Dictionary<TalkType, List<string>> talks = new();
    public static CustomSyncedValue<string> sync;

    static TalkManager()
    {
        FileName = "RandomTalks.yml";
        FilePath = Path.Combine(ConfigManager.DirectoryPath, FileName);
        sync = new(ConfigManager.ConfigSync, "RustyMods.Norseman.RandomTalk.Sync");
        sync.ValueChanged += OnConfigChanged;
    }

    public static List<string> GetTalk(TalkType type)
    {
        return talks.TryGetValue(type, out var list) ? list : new List<string>();
    }

    public static void OnConfigChanged()
    {
        if (!ZNet.instance || ZNet.instance.IsServer()) return;
        string text = sync.Value;
        if (string.IsNullOrEmpty(text)) return;
        try
        {
            Dictionary<TalkType, List<string>> data = ConfigManager.deserializer.Deserialize<Dictionary<TalkType, List<string>>>(text);
            talks = data;
        }
        catch
        {
            NorsemenPlugin.LogError("Failed to deserialize server's random talks");
        }
    }

    public static void UpdateSync(ZNet net)
    {
        if (!net.IsServer()) return;
        string text = ConfigManager.serializer.Serialize(talks);
        sync.Value = text;
    }
    
    public static void Setup()
    {
        GetOrSerialize();
    }

    public static void GetOrSerialize()
    {
        if (File.Exists(FilePath))
        {
            Read(FilePath);
        }
        else
        {
            string data = EmbeddedResourceManager.GetFile(FileName);
            talks = ConfigManager.deserializer.Deserialize<Dictionary<TalkType, List<string>>>(data);
            File.WriteAllText(FilePath, data);
        }
    }

    public static void Read(string filePath)
    {
        try
        {
            string text = File.ReadAllText(filePath);
            Dictionary<TalkType, List<string>> data = ConfigManager.deserializer.Deserialize<Dictionary<TalkType, List<string>>>(text);
            talks = data;
        }
        catch
        {
            NorsemenPlugin.LogError("Failed to deserialize random talks");
        }
    }

    public static void SetupWatcher()
    {
        FileSystemWatcher watcher = new(ConfigManager.DirectoryPath, FileName);
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
        Read(FilePath);
        UpdateSync(ZNet.instance);
        NorsemenPlugin.LogInfo($"{FileName} file changed");
    }
}