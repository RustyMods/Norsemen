using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Norsemen;

public static class SpawnManager
{
    public static readonly SpawnSystemList SpawnList;

    static SpawnManager()
    {
        SpawnList = NorsemenPlugin.instance.gameObject.AddComponent<SpawnSystemList>();
        Harmony harmony = NorsemenPlugin.instance._harmony;
        harmony.Patch(AccessTools.Method(typeof(SpawnSystem), nameof(SpawnSystem.Awake)),
            postfix: new HarmonyMethod(AccessTools.Method(typeof(SpawnManager), nameof(Patch_SpawnSystem_Awake))));
    }

    public static void Patch_SpawnSystem_Awake(SpawnSystem __instance)
    {
        __instance.m_spawnLists.Add(SpawnList);
    }
    
    public class SpawnInfo : SpawnSystem.SpawnData
    {
        private readonly string PrefabName;
        public readonly Heightmap.Biome Biome;
        public readonly Configs configs;
        public class Configs
        {
            public ConfigEntry<Toggle> Enabled = null!;
            public ConfigEntry<Heightmap.Biome> Biome = null!;
            public ConfigEntry<Heightmap.BiomeArea> Area = null!;
            public ConfigEntry<int> MaxSpawned = null!;
            public ConfigEntry<float> Interval = null!;
            public ConfigEntry<float> Chance = null!;
            public ConfigEntry<float> Distance = null!;
            public ConfigEntry<string> RequiredKey = null!;
            public ConfigEntry<string> RequiredEnvs = null!;
            public ConfigEntry<TimeOfDay> TOD = null!;
            public ConfigEntry<string>? Altitude;
            public ConfigEntry<Region>? Forest;
            public ConfigEntry<string>? Level;
        }

        private void ConfigChanged(object? o, EventArgs? e)
        {
            m_enabled = configs.Enabled.Value is Toggle.On;
            m_biome = configs.Biome.Value;
            m_biomeArea = configs.Area.Value;
            m_maxSpawned = configs.MaxSpawned.Value;
            m_spawnInterval = configs.Interval.Value;
            m_spawnChance = configs.Chance.Value;
            m_spawnDistance = configs.Distance.Value;
            m_requiredGlobalKey = configs.RequiredKey.Value;
            m_requiredEnvironments = new EnvList(configs.RequiredEnvs.Value).GetValidatedList();
            m_spawnAtDay = configs.TOD.Value is TimeOfDay.Both or TimeOfDay.Day;
            m_spawnAtNight = configs.TOD.Value is TimeOfDay.Both or TimeOfDay.Night;
            m_minAltitude = configs.Altitude != null ? new MinMax(configs.Altitude.Value).Min : -1000f;
            m_maxAltitude = configs.Altitude != null ? new MinMax(configs.Altitude.Value).Max : 1000f;
            m_inForest = configs.Forest == null || configs.Forest.Value is Region.Both or Region.InForest;
            m_outsideForest = configs.Forest == null || configs.Forest.Value is Region.Both or Region.OutForest;
            m_minLevel = configs.Level != null ? new Level(configs.Level.Value).Min : 1;
            m_maxLevel = configs.Level != null ? new Level(configs.Level.Value).Max : 1;
            m_overrideLevelupChance = configs.Level != null ? new Level(configs.Level.Value).Chance : 0f;
        }
        public void SetupConfigs()
        {
            configs.Enabled = ConfigManager.config(PrefabName, "Enabled", Toggle.On, "If on, viking can spawn");
            configs.Enabled.SettingChanged += ConfigChanged;
            configs.Biome = ConfigManager.config(PrefabName, "Biome", Biome, "Set biomes viking can spawn in");
            configs.Biome.SettingChanged += ConfigChanged;
            configs.Area = ConfigManager.config(PrefabName, "Biome Area", Heightmap.BiomeArea.Everything, "Set particular part of biome viking can spawn in");
            configs.Area.SettingChanged += ConfigChanged;
            configs.MaxSpawned = ConfigManager.config(PrefabName, "Max Spawned", m_maxSpawned, "Set maximum amount allowed spawned in a zone");
            configs.MaxSpawned.SettingChanged += ConfigChanged;
            configs.Interval = ConfigManager.config(PrefabName, "Spawn Interval", m_spawnInterval, "Set how often vikings will try to spawn");
            configs.Interval.SettingChanged += ConfigChanged;
            configs.Chance = ConfigManager.config(PrefabName, "Spawn Chance", m_spawnChance, new ConfigDescription("Set chance to spawn", new AcceptableValueRange<float>(0f, 100f)));
            configs.Chance.SettingChanged += ConfigChanged;
            configs.Distance = ConfigManager.config(PrefabName, "Spawn Distance", m_spawnDistance, "Spawn range, 0 = use global settings");
            configs.Distance.SettingChanged += ConfigChanged;
            configs.RequiredKey = ConfigManager.config(PrefabName, "Required Key", "", "Only spawn if this key is present");
            configs.RequiredKey.SettingChanged += ConfigChanged;
            configs.RequiredEnvs = ConfigManager.config(PrefabName, "Required Envs",
                new EnvList().ToString(), new ConfigDescription(
                    "List of required environments for viking to spawn", null,
                    EnvList.attributes));
            configs.RequiredEnvs.SettingChanged += ConfigChanged;
            configs.TOD = ConfigManager.config(PrefabName, "Spawn Time Of Day", TimeOfDay.Both, "Set time of day requirement");
            configs.TOD.SettingChanged += ConfigChanged;
            configs.Altitude = ConfigManager.config(PrefabName, "Spawn Altitude",
                new MinMax(m_minAltitude, m_maxAltitude).ToString(), new ConfigDescription("Set [min]-[max] altitude", null,
                    MinMax.attributes));
            configs.Altitude.SettingChanged += ConfigChanged;
            configs.Forest = ConfigManager.config(PrefabName, "Spawn Region", Region.Both, "Set which region viking can spawn in");
            configs.Forest.SettingChanged += ConfigChanged;
            configs.Level = ConfigManager.config(PrefabName, "Spawn Level",
                new Level(m_minLevel, m_maxLevel, m_overrideLevelupChance).ToString(), new ConfigDescription("Set [min]:[max]:[chanceToLevel]",
                    null, Level.attributes));
            configs.Level.SettingChanged += ConfigChanged;
            ConfigChanged(null, null);
        }
        
        public enum TimeOfDay { Both, Night, Day}
        public enum Region {Both, InForest, OutForest}
        
        public SpawnInfo(Norseman viking)
        {
            PrefabName = viking.name;
            m_name = viking.name;
            Biome = viking.biome;
            configs = new Configs();
            SpawnList.enabled = true;
            m_groupSizeMin = 0;
            m_groupSizeMax = 1;
            m_levelUpMinCenterDistance = 1f;
            m_minTilt = 0f;
            m_maxTilt = 50f;
            m_groupRadius = 50f;
            SpawnList.m_spawners.Add(this);
        }
    }
}