using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace Norsemen
{
    public enum Toggle
    {
        On = 1,
        Off = 0
    }
    
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class NorsemenPlugin : BaseUnityPlugin
    {
        internal const string ModName = "Norsemen";
        internal const string ModVersion = "0.3.0";
        internal const string Author = "RustyMods";
        public const string ModGUID = Author + "." + ModName;
        internal static string ConnectionError = "";
        public readonly Harmony _harmony = new(ModGUID);
        public static readonly ManualLogSource NorsemenLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);
        public static NorsemenPlugin instance = null!;

        private static ConfigEntry<Toggle> canSteal = null!;
        private static ConfigEntry<Toggle> removeEquipment = null!;
        private static ConfigEntry<Toggle> canBeEncumbered = null!;
        private static ConfigEntry<int> baseCarryWeight = null!;
        
        public static bool CanSteal => canSteal.Value is Toggle.On;
        public static bool RemoveEquipment => removeEquipment.Value is Toggle.On;
        public static bool CanBecomeEncumbered => canBeEncumbered.Value is Toggle.On;
        public static int BaseCarryWeight => baseCarryWeight.Value;
        
        public void Awake()
        {
            instance = this;

            Localizer.Load();
            
            SetupTombstone();
            SetupNorsemen();
            SetupCommands();
            
            
            NameGenerator.Setup();
            TalkManager.Setup();
            CustomizationManager.Setup();

            canSteal = ConfigManager.config("Settings", "Steal", Toggle.On, "If on, players can steal from norsemen");
            removeEquipment = ConfigManager.config("Settings", "Naked on Tamed", Toggle.Off, "If on, tamed norsemen will lose equipment on tamed");
            canBeEncumbered = ConfigManager.config("Settings", "Encumbers", Toggle.On, "If on, tamed norsemen can become encumbered");
            baseCarryWeight = ConfigManager.config("Settings", "Base Carry Weight", 300, "Set nrosemens base carry weight");
            
            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmony.PatchAll(assembly);
        }

        private static void SetupNorsemen()
        {
            Faction norsemen = new Faction("Norsemen", true);
            
            Norseman meadows = new Norseman(Heightmap.Biome.Meadows, "Meadows_Norseman_RS", norsemen);
            meadows.baseHealth = 50f;
            meadows.baseArmor = 0f;

            Norseman blackforest = new Norseman(Heightmap.Biome.BlackForest, "BlackForest_Norseman_RS", norsemen);
            blackforest.baseHealth = 100f;
            blackforest.baseArmor = 5f;

            Norseman swamp = new Norseman(Heightmap.Biome.Swamp, "Swamp_Norseman_RS", norsemen);
            swamp.baseHealth = 150f;
            swamp.baseArmor = 10f;
            
            Norseman mountains = new (Heightmap.Biome.Mountain, "Mountains_Norseman_RS", norsemen);
            mountains.baseHealth = 200f;
            mountains.baseArmor = 15f;
            
            Norseman plains = new(Heightmap.Biome.Plains, "Plains_Norseman_RS", norsemen);
            plains.baseHealth = 250f;
            plains.baseArmor = 20f;
            
            Norseman mistlands = new(Heightmap.Biome.Mistlands, "Mistlands_Norseman_RS", norsemen);
            mistlands.baseHealth = 300f;
            mistlands.baseArmor = 25f;
            
            Norseman ashlands = new(Heightmap.Biome.AshLands, "AshLands_Norseman_RS", norsemen);
            ashlands.baseHealth = 350f;
            ashlands.baseArmor = 30f;
        }

        private static void SetupTombstone()
        {
            Clone vikingTombstone = new Clone("Player_tombstone", "Norseman_tombstone_RS");
            vikingTombstone.OnCreated += prefab =>
            {
                MeshRenderer? renderer = prefab.GetComponentInChildren<MeshRenderer>();
                if (renderer != null)
                {
                    List<Material> newMaterials = new();
                    foreach (Material? material in renderer.sharedMaterials)
                    {
                        Material mat = new Material(material);
                        newMaterials.Add(mat);
                        mat.SetColor("_EmissionColor", new Color(0f, 0.8f, 1f) * 4f);
                    }

                    Material[] mats = newMaterials.ToArray();
                    renderer.sharedMaterials = mats;
                    renderer.materials = mats;
                    
                    prefab.AddComponent<VikingTomb>();
                }

                Transform? ringParticles = prefab.transform.Find("Particle System");
                if (ringParticles != null && ringParticles.TryGetComponent(out ParticleSystem ps))
                {
                    ParticleSystem.MainModule main = ps.main;
                    main.startColor = new Color(0f, 0.8f, 1f, 0.54f);
                }
                
                Norseman.tombstone = prefab;
            };

            Clone fx_revive = new Clone("fx_summon_twitcher_spawn", "fx_revive_norseman");
            fx_revive.OnCreated += prefab =>
            {
                var swirls = prefab.transform.Find("Swirls");
                var dust = prefab.transform.Find("Swirls/Dust");
                var spores = prefab.transform.Find("Swirls/Swirly Spores");
                var light = prefab.transform.Find("Point light");
                var light1 = prefab.transform.Find("Point light (1)");
                
                if (swirls != null)
                {
                    var ps = swirls.GetComponent<ParticleSystem>();
                    var main = ps.main;
                    main.startColor = new Color(0f, 0.8f, 1f, 1f);
                }

                if (dust != null)
                {
                    var ps = dust.GetComponent<ParticleSystem>();
                    var main = ps.main;
                    main.startColor = new Color(0.4f, 0.8f, 1f, 1f);
                }

                if (spores != null)
                {
                    var ps = spores.GetComponent<ParticleSystem>();
                    Gradient gradient1 = new();
                    gradient1.SetKeys(new []
                    {
                        new GradientColorKey(new Color(0.4f, 0.8f, 0.8f, 1f), 0.0f), 
                        new GradientColorKey(new Color(0f, 0.8f, 1f, 1f), 1.0f)
                    }, new GradientAlphaKey[]
                    {
                        new (1f, 0f), new (0f, 1f)
                    });
                    ps.customData.SetColor(ParticleSystemCustomData.Custom1, gradient1);
                }

                if (light != null)
                {
                    var l = light.GetComponent<Light>();
                    l.color = new Color(0.1f, 0.8f, 0.8f, 1f);
                }

                if (light1 != null)
                {
                    var l = light1.GetComponent<Light>();
                    l.color = new Color(0.1f, 0.8f, 0.8f, 1f);
                }

                VikingTomb.reviveEffects.m_effectPrefabs = new[]
                {
                    new EffectList.EffectData()
                    {
                        m_prefab = prefab,
                    }
                };
            };
        }

        private static void SetupCommands()
        {
            NorseCommand tameAll = new NorseCommand("tame", "tames all nearby norsemen", _ =>
            {
                List<Viking> vikings = Viking.GetAllVikings();
                int count = 0;
                foreach (Viking? viking in vikings)
                {
                    if (viking.IsTamed() || !viking.configs.Tameable) continue;
                    ++count;
                    viking.SetTamed(true);
                    viking.m_nview.GetZDO().Set(VikingVars.lastLevelUpTime, ZNet.instance.GetTime().Ticks);
                }

                if (Player.m_localPlayer)
                {
                    Player.m_localPlayer.Message(MessageHud.MessageType.Center, $"Tamed {count} norsemen");
                }
                return true;
            }, adminOnly: true);
            
            NorseCommand removeTombs = new ("clear_tombs", "removes all nearby norsemen tombstones", _ =>
            {
                VikingTomb.RemoveAll();
                return true;
            }, adminOnly: true);
        }

        private void OnDestroy()
        {
            Config.Save();
        }

        public static void LogDebug(string msg)
        {
            if (!ConfigManager.ShouldLog(ConfigManager.LogLevel.Debug)) return;
            NorsemenLogger.LogDebug(msg);
        }

        public static void LogError(string msg)
        {
            if (!ConfigManager.ShouldLog(ConfigManager.LogLevel.Error)) return;
            NorsemenLogger.LogError(msg);
        }

        public static void LogWarning(string msg)
        {
            if (!ConfigManager.ShouldLog(ConfigManager.LogLevel.Warning)) return;
            NorsemenLogger.LogWarning(msg);
        }

        public static void LogInfo(string msg)
        {
            if (!ConfigManager.ShouldLog(ConfigManager.LogLevel.Info)) return;
            NorsemenLogger.LogInfo(msg);
        }
    }
}