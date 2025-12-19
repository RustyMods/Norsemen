using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using ServerSync;
using UnityEngine;

namespace Norsemen;

public static class CustomizationManager
{
    public static readonly List<string> beards = new();
    public static readonly List<string> hairs = new();
    
    public static readonly List<Color> hairColors = new List<Color>()
    {
        Color.black,                                    // Black
        new (0.98f, 0.94f, 0.75f, 1f),                 // Platinum Blonde
        new (0.63f, 0.36f, 0f, 1f),                    // Brown
        new (0.15f, 0.08f, 0.05f, 1f),                 // Dark Brown
        new (0.35f, 0.25f, 0.15f, 1f),                 // Medium Brown
        new (0.55f, 0.45f, 0.35f, 1f),                 // Light Brown
        new (0.95f, 0.87f, 0.51f, 1f),                 // Golden Blonde
        new (0.85f, 0.75f, 0.45f, 1f),                 // Dirty Blonde
        new (0.72f, 0.65f, 0.52f, 1f),                 // Sandy Blonde
        new (0.45f, 0.18f, 0.08f, 1f),                 // Auburn
        new (0.55f, 0.12f, 0.05f, 1f),                 // Dark Red
        new (0.72f, 0.25f, 0.12f, 1f),                 // Ginger
        new (0.40f, 0.40f, 0.40f, 1f),                 // Dark Gray
        new (0.65f, 0.65f, 0.65f, 1f),                 // Silver Gray
        new (0.88f, 0.88f, 0.90f, 1f),                 // White/Silver
        new (0.25f, 0.25f, 0.28f, 1f),                 // Charcoal
    };
    
    public static readonly List<Color> skinColors = new List<Color>()
    {
        new (1f, 1f, 1f, 1f),                          // Base/Pale (no tint)
        new (1f, 0.95f, 0.92f, 1f),                    // Very Fair
        new (1f, 0.92f, 0.86f, 1f),                    // Fair with Pink undertone
        new (1f, 0.94f, 0.88f, 1f),                    // Fair
        new (1f, 0.90f, 0.82f, 1f),                    // Light
        new (1f, 0.88f, 0.78f, 1f),                    // Light Medium
        new (0.98f, 0.85f, 0.72f, 1f),                 // Medium
        new (0.95f, 0.82f, 0.68f, 1f),                 // Medium Tan
        new (0.92f, 0.78f, 0.62f, 1f),                 // Tan
        new (0.88f, 0.72f, 0.56f, 1f),                 // Deep Tan
        new (0.85f, 0.68f, 0.52f, 1f),                 // Light Brown
        new (0.78f, 0.62f, 0.48f, 1f),                 // Medium Brown
        new (1f, 0.88f, 0.75f, 1f),                    // Warm Beige
        new (0.98f, 0.90f, 0.80f, 1f),                 // Peachy Fair
        new (0.95f, 0.86f, 0.76f, 1f),                 // Golden Light
    };

    public static Color GetRandomSkinColor()
    {
        int index = UnityEngine.Random.Range(0, skinColors.Count);
        return skinColors[index];
    }

    public static void GetHairAndBeards(ObjectDB db)
    {
        foreach (GameObject? prefab in db.m_items)
        {
            ItemDrop component = prefab.GetComponent<ItemDrop>();
            if (component.m_itemData.m_shared.m_itemType is not ItemDrop.ItemData.ItemType.Customization) continue;
            if (prefab.name.CustomStartsWith("Beard"))
            {
                beards.Add(prefab.name);
            }
            else if (prefab.name.CustomStartsWith("Hair"))
            {
                hairs.Add(prefab.name);
            }
        }

        beards.RemoveAll(x => x.Contains("_"));
        hairs.RemoveAll(x => x.Contains("_"));
    }


    public static readonly string FileName = "Equipment.yml";
    public static readonly string FilePath = Path.Combine(ConfigManager.DirectoryPath, FileName);

    public static Dictionary<Heightmap.Biome, Equipment> equipment = new()
    {
        [Heightmap.Biome.Meadows] = new Equipment(),
        [Heightmap.Biome.BlackForest] = new Equipment(),
        [Heightmap.Biome.Swamp] = new Equipment(),
        [Heightmap.Biome.Mountain] = new Equipment(),
        [Heightmap.Biome.Plains] = new Equipment(),
        [Heightmap.Biome.Mistlands] = new Equipment(),
        [Heightmap.Biome.AshLands] = new Equipment(),
        [Heightmap.Biome.DeepNorth] = new Equipment(),
        [Heightmap.Biome.Ocean] = new Equipment()
    };

    public static readonly CustomSyncedValue<string> sync = new(ConfigManager.ConfigSync, "RustyMods.Norsemen.Equipment.Sync", "");
    
    public static void Setup()
    {
        GetOrSerialize();
        SetupFileWatcher();
        sync.ValueChanged += OnConfigChange;
    }

    public static void OnConfigChange()
    {
        if (!ZNet.instance || ZNet.instance.IsServer()) return;
        string text = sync.Value;
        if (string.IsNullOrEmpty(text)) return;
        try
        {
            Dictionary<Heightmap.Biome, Equipment> data = ConfigManager.deserializer.Deserialize<Dictionary<Heightmap.Biome, Equipment>>(text);
            equipment.Clear();
            equipment.AddRange(data);
        }
        catch
        {
            NorsemenPlugin.LogError("Failed to deserialize server's equipments");
        }
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

    public static void ReadConfigValues(object sender, FileSystemEventArgs e)
    {
        if (!ZNet.instance || !ZNet.instance.IsServer()) return;
        try
        {
            string text = File.ReadAllText(FilePath);
            Dictionary<Heightmap.Biome, Equipment> data = ConfigManager.deserializer.Deserialize<Dictionary<Heightmap.Biome, Equipment>>(text);
            equipment = data;
            UpdateSync(ZNet.instance);
            NorsemenPlugin.LogDebug($"{FileName} changed");
        }
        catch (Exception ex)
        {
            NorsemenPlugin.LogError($"Failed to deserialize {FileName}");
            Debug.LogError(ex.Message);
        }
    }

    public static void UpdateSync(ZNet net)
    {
        if (!net.IsServer()) return;
        string text = ConfigManager.serializer.Serialize(equipment);
        sync.Value = text;
    }

    public static void GetOrSerialize()
    {
        if (!File.Exists(FilePath))
        {
            string data = ConfigManager.serializer.Serialize(equipment);
            File.WriteAllText(FilePath, data);
        }
        else
        {
            Read();
        }
    }

    public static void Read()
    {
        try
        {
            string text = File.ReadAllText(FilePath);
            Dictionary<Heightmap.Biome, Equipment> data = ConfigManager.deserializer.Deserialize<Dictionary<Heightmap.Biome, Equipment>>(text);
            equipment = data;
        }
        catch (Exception ex)
        {
            NorsemenPlugin.LogError($"Failed to deserialize: {FileName}");
            Debug.LogError(ex.Message);
        }
    }

    public static void Add(Heightmap.Biome biome, params ConditionalRandomSet[] set)
    {
        if (!equipment.ContainsKey(biome)) equipment[biome] = new Equipment();
        equipment[biome].RandomSets.Add(set);
    }

    public static void Add(Heightmap.Biome biome, params ConditionalRandomItem[] item)
    {
        if (!equipment.ContainsKey(biome)) equipment[biome] = new Equipment();
        equipment[biome].RandomItems.Add(item);
    }

    public static void Add(Heightmap.Biome biome, params ConditionalRandomWeapon[] weapon)
    {
        if (!equipment.ContainsKey(biome)) equipment[biome] = new Equipment();
        equipment[biome].RandomWeapons.Add(weapon);
    }

    public static List<ConditionalRandomSet> GetSets(Heightmap.Biome biome)
    {
        if (!equipment.TryGetValue(biome, out Equipment? data)) return new();
        return data.RandomSets;
    }

    public static List<ConditionalRandomItem> GetItems(Heightmap.Biome biome)
    {
        if (!equipment.TryGetValue(biome, out Equipment? data)) return new();
        return data.RandomItems;
    }

    public static List<ConditionalRandomWeapon> GetWeapons(Heightmap.Biome biome)
    {
        if (!equipment.TryGetValue(biome, out Equipment? data)) return new();
        return data.RandomWeapons;
    }
}