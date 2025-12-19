using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

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
        
        public static bool CanSteal => canSteal.Value is Toggle.On;
        public static bool RemoveEquipment => removeEquipment.Value is Toggle.On;
        
        public void Awake()
        {
            instance = this;

            Localizer.Load();
            
            SetupTombstone();
            SetupNorsemen();
            SetupCommands();
            SetupEquipment();
            NameGenerator.Setup();
            TalkManager.SetupTalks();
            CustomizationManager.Setup();

            canSteal = ConfigManager.config("Settings", "Steal", Toggle.On, "If on, players can steal from norsemen");
            removeEquipment = ConfigManager.config("Settings", "Naked on Tamed", Toggle.Off, "If on, tamed norsemen will lose equipment on tamed");
            
            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmony.PatchAll(assembly);
        }

        public static void SetupEquipment()
        {
            ConditionalRandomSet MeadowTorchSet = new ("", 0.1f, "ArmorRagsChest", "ArmorRagsLegs", "Torch");
            ConditionalRandomSet MeadowFlintKnifeSet = new("defeated_eikthyr", 0.2f, "ArmorLeatherChest", "ArmorLeatherLegs", "CapeDeerHide", "KnifeFlint");
            ConditionalRandomSet MeadowClubSet = new("", 0.1f, "ArmorRagsChest", "ArmorRagsLegs", "Club");
            ConditionalRandomSet BlackForestTroll = new("defeated_gdking", 0.3f, "HelmetTrollLeather",
                "ArmorTrollLeatherChest", "ArmorTrollLeatherLegs", "CapeTrollLeather", "SwordBronze",
                "ShieldBronzeBuckler");
            ConditionalRandomSet BlackForestBronze = new("defeated_gdking", 0.3f, "HelmetBronze",
                "ArmorBronzeChest", "ArmorBronzeLegs", "CapeTrollLeather", "AtgeirBronze");
            ConditionalRandomSet BlackForestBear = new("defeated_gdking", 0.3f, "HelmetBerserkerHood",
                "ArmorBerserkerChest", "ArmorBerserkerLegs", "FistBjornClaw");
            
            ConditionalRandomSet SwampIron = new("defeated_bonemass", 0.4f, "HelmetIron", "ArmorIronChest", "ArmorIronLegs",
                "CapeDeerHide", "BowHuntsman");
            ConditionalRandomSet SwampRoot = new("defeated_bonemass", 0.4f, "HelmetRoot", "ArmorRootChest", "ArmorRootLegs",
                "CapeTrollHide", "BowHuntsman");

            ConditionalRandomSet MountainSilver = new("defeated_dragon", 0.5f, "HelmetDrake", "ArmorWolfChest",
                "ArmorWolfLegs", "CapeWolf", "BattleAxeCrystal");
            ConditionalRandomSet MountainFenrir = new("defeated_dragon", 0.5f, "HelmetFenring", "ArmorFenringChest",
                "ArmorFenringLegs", "CapeWolf", "KnifeSilver", "ShieldSilver");

            ConditionalRandomSet PlainsPadded = new("defeated_goblingking", 0.6f, "HelmetPadded",
                "ArmorPaddedGreaves", "ArmorPaddedCuirass", "CapeLinen", "MaceNeedle");
            ConditionalRandomSet PlainsVile = new("defeated_goblinking", 0.6f, "HelmetBerserkerUndead",
                "ArmorBerserkerUndeadChest", "ArmorBerserkerUndeadLegs", "FistBjornUndeadClaw");

            ConditionalRandomSet MistlandsCarapace = new("defeated_queen", 0.7f, "HelmetCarapace",
                "ArmorCarapaceChest", "ArmorCarapaceLegs", "ShieldCarapace", "SpearCarapace", "Demister");

            ConditionalRandomSet MistlandsMage = new("defeated_queen", 0.7f, "HelmetMageHood", "ArmorMageChest",
                "ArmorMageLegs", "StaffFireball", "CapeFeather", "Demister");

            ConditionalRandomSet Harvester = new("defeated_dragon", 1f, "ArmorHarvester1", "ArmorLeatherLegs", "Cultivator", "AxeBronze");

            ConditionalRandomSet Ask = new("defeated_queen", 0.8f, "HelmetAshlandsMediumHood",
                "ArmorAshlandsMediumChest", "ArmorAshlandsMediumlegs", "CapeAsksvin", "BowAshlands");

            ConditionalRandomSet Flametal = new("defeated_queen", 0.8f, "HelmetFlametal",
                "ArmorFlametalChest", "ArmorFlametalLegs", "CapeAsh", "SwordNiedhogg", "ShieldFlametalTower");

            ConditionalRandomSet MageAsh = new("defeated_queen", 0.8f, "HelmetMage_Ashlands",
                "ArmorMageChest_Ashlands", "ArmorMageLegs_Ashlands", "StaffRoot", "StaffGreenRoots");

            CustomizationManager.Add(Heightmap.Biome.Meadows, MeadowTorchSet, MeadowFlintKnifeSet, MeadowClubSet);
            CustomizationManager.Add(Heightmap.Biome.BlackForest, MeadowClubSet, BlackForestBear, BlackForestBronze, BlackForestTroll);
            CustomizationManager.Add(Heightmap.Biome.Swamp, BlackForestTroll, SwampIron, SwampRoot);
            CustomizationManager.Add(Heightmap.Biome.Mountain, SwampIron, MountainFenrir, MountainSilver);
            CustomizationManager.Add(Heightmap.Biome.Plains, MountainSilver, PlainsPadded, PlainsVile);
            CustomizationManager.Add(Heightmap.Biome.Mistlands, PlainsPadded, MistlandsCarapace, MistlandsMage);
            CustomizationManager.Add(Heightmap.Biome.AshLands, MistlandsCarapace, Ask, Flametal, MageAsh);
            
            ConditionalRandomItem Coins = new("Coins", 1, 100);
            ConditionalRandomItem Raspberries = new("Raspberry", 1, 50, 0.5f);
            ConditionalRandomItem Wood = new("Wood", 25, 50);
            ConditionalRandomItem LeatherScraps = new("LeatherScrap", 1, 20, 0.5f);
            ConditionalRandomItem Flint = new("Flint", 1, 20, 0.5f, "defeated_eikthry");
            ConditionalRandomItem deerHide = new("DeerHide", 1, 10, 0.5f, "defeated_eikthyr");
            ConditionalRandomItem surtlingCore = new("SurtlingCore", 1, 3, 0.4f, "defeated_eikthyr");
            ConditionalRandomItem pickaxeAntler = new("PickaxeAntler", 1, 1, 0.6f, "defeated_eikthyr");
            ConditionalRandomItem tinOre = new("TinOre", 1, 5, 0.5f, "defeated_eikthyr");
            ConditionalRandomItem copperOre = new("CopperOre", 1, 5, 0.5f, "defeated_eikthyr");
            ConditionalRandomItem deerStew = new("DeerStew", 1, 5, 0.5f, "defeated_eikthyr");
            ConditionalRandomItem tin = new( "Tin", 1, 5, 0.3f, "defeated_gdking");
            ConditionalRandomItem copper = new("Copper", 1, 5, 0.3f, "defeated_gdking");
            ConditionalRandomItem ironScrap = new("IronScrap", 1, 5, 0.3f, "defeated_gdking");
            ConditionalRandomItem chain = new("Chain", 1, 2, 0.5f, "defeated_gdking");
            ConditionalRandomItem iron = new("Iron", 1, 5, 0.3f, "defeated_bonemass");
            ConditionalRandomItem fineWood = new("FineWood", 25, 50, 0.5f, "defeated_bonemass");
            ConditionalRandomItem silverOre = new ("SilverOre", 1, 10, 0.33f, "defeated_bonemass");
            ConditionalRandomItem obsidian = new("Obsidian", 1, 20, 0.4f, "defeated_bonemass");
            ConditionalRandomItem crystal = new("Crystal", 10, 15, 0.5f, "defeated_bonemass");
            ConditionalRandomItem silver = new("Silver", 1, 5, 0.3f, "defeated_dragon");
            ConditionalRandomItem tissue = new("SoftTissue", 1, 5, 0.3f, "defeated_goblinking");
            
            CustomizationManager.Add(Heightmap.Biome.Meadows, Raspberries, Wood, LeatherScraps, Flint, deerHide);
            CustomizationManager.Add(Heightmap.Biome.BlackForest, Raspberries, Wood, deerHide, surtlingCore, pickaxeAntler, tinOre, copperOre);
            CustomizationManager.Add(Heightmap.Biome.Swamp, Coins, deerStew, copperOre, ironScrap, iron, surtlingCore, chain);
            CustomizationManager.Add(Heightmap.Biome.Mountain, Coins, deerStew, tin, copper, silverOre, obsidian, crystal, silver, ironScrap, chain, iron);
            CustomizationManager.Add(Heightmap.Biome.Plains, Coins, deerStew, fineWood, silver);
            CustomizationManager.Add(Heightmap.Biome.Plains, Coins, deerStew, fineWood);
            CustomizationManager.Add(Heightmap.Biome.Mistlands, Coins, deerStew, tissue, fineWood);
            CustomizationManager.Add(Heightmap.Biome.AshLands, Coins, deerStew, tissue);

            ConditionalRandomWeapon bow = new("Bow", "defeated_eikthyr", 0.1f);
            ConditionalRandomWeapon bowfine = new("BowFineWood", "defeated_gdking", 0.2f);
            ConditionalRandomWeapon knifeChitin = new("KnifeChitin", "defeated_bonemass", 0.3f);
            ConditionalRandomWeapon frostner = new("MaceSilver", "defeated_dragon", 0.4f);
            
            CustomizationManager.Add(Heightmap.Biome.Meadows, bow);
            CustomizationManager.Add(Heightmap.Biome.BlackForest, bowfine);
            CustomizationManager.Add(Heightmap.Biome.Mountain, bowfine);
            CustomizationManager.Add(Heightmap.Biome.Swamp, knifeChitin);
            CustomizationManager.Add(Heightmap.Biome.Mountain, frostner);
        }

        private static void SetupNorsemen()
        {
            Faction norsemen = new Faction("Norsemen", true);
            
            Norseman meadows = new Norseman(Heightmap.Biome.Meadows, "Meadows_Norseman_RS", norsemen);

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