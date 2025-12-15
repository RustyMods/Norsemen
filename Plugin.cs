using System.Collections.Generic;
using System.Reflection;
using BepInEx;
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
        
        public void Awake()
        {
            
            instance = this;

            Localizer.Load();
            
            SetupTombstone();
            SetupNorsemen();
            SetupCommands();
            
            ConditionalRandomItem.Setup();
            ConditionalRandomSet.Setup();
            
            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmony.PatchAll(assembly);
        }

        private static void SetupNorsemen()
        {
            Faction norsemen = new Faction("Norsemen", true);

            ConditionalRandomSet MeadowTorchSet = new ("Torch", "", 0.1f, "ArmorRagsChest", "ArmorRagsLegs", "Torch");
            ConditionalRandomSet MeadowFlintKnifeSet = new("Flint", "defeated_eikthyr", 0.2f, "ArmorLeatherChest", "ArmorLeatherLegs", "CapeDeerHide", "KnifeFlint");
            ConditionalRandomSet MeadowClubSet = new("Club", "", 0.1f, "ArmorRagsChest", "ArmorRagsLegs", "Club");
            ConditionalRandomSet BlackForestTroll = new("Troll", "defeated_gdking", 0.3f, "HelmetTrollLeather",
                "ArmorTrollLeatherChest", "ArmorTrollLeatherLegs", "CapeTrollLeather", "SwordBronze",
                "ShieldBronzeBuckler");
            ConditionalRandomSet BlackForestBronze = new("Bronze", "defeated_gdking", 0.3f, "HelmetBronze",
                "ArmorBronzeChest", "ArmorBronzeLegs", "CapeTrollLeather", "AtgeirBronze");
            ConditionalRandomSet BlackForestBear = new("Bjorn", "defeated_gdking", 0.3f, "HelmetBerserkerHood",
                "ArmorBerserkerChest", "ArmorBerserkerLegs", "FistBjornClaw");
            
            ConditionalRandomSet SwampIron = new("Iron", "defeated_bonemass", 0.4f, "HelmetIron", "ArmorIronChest", "ArmorIronLegs",
                "CapeDeerHide", "BowHuntsman");
            ConditionalRandomSet SwampRoot = new("Root", "defeated_bonemass", 0.4f, "HelmetRoot", "ArmorRootChest", "ArmorRootLegs",
                "CapeTrollHide", "BowHuntsman");

            ConditionalRandomSet MountainSilver = new("Silver", "defeated_dragon", 0.5f, "HelmetDrake", "ArmorWolfChest",
                "ArmorWolfLegs", "CapeWolf", "BattleAxeCrystal");
            ConditionalRandomSet MountainFenrir = new("Fenrir", "defeated_dragon", 0.5f, "HelmetFenring", "ArmorFenringChest",
                "ArmorFenringLegs", "CapeWolf", "KnifeSilver", "ShieldSilver");

            ConditionalRandomSet PlainsPadded = new("Padded", "defeated_goblingking", 0.6f, "HelmetPadded",
                "ArmorPaddedGreaves", "ArmorPaddedCuirass", "CapeLinen", "MaceNeedle");
            ConditionalRandomSet PlainsVile = new("Vile", "defeated_goblinking", 0.6f, "HelmetBerserkerUndead",
                "ArmorBerserkerUndeadChest", "ArmorBerserkerUndeadLegs", "FistBjornUndeadClaw");

            ConditionalRandomSet MistlandsCarapace = new("Carapace", "defeated_queen", 0.7f, "HelmetCarapace",
                "ArmorCarapaceChest", "ArmorCarapaceLegs", "ShieldCarapace", "SpearCarapace", "Demister");

            ConditionalRandomSet MistlandsMage = new("Mage", "defeated_queen", 0.7f, "HelmetMageHood", "ArmorMageChest",
                "ArmorMageLegs", "StaffFireball", "CapeFeather", "Demister");

            ConditionalRandomSet Harvester = new("Harvester", "defeated_dragon", 1f, "ArmorHarvester1", "ArmorLeatherLegs", "Cultivator", "AxeBronze");

            ConditionalRandomSet Ask = new("Askvin", "defeated_queen", 0.8f, "HelmetAshlandsMediumHood",
                "ArmorAshlandsMediumChest", "ArmorAshlandsMediumlegs", "CapeAsksvin", "BowAshlands");

            ConditionalRandomSet Flametal = new("Flametal", "defeated_queen", 0.8f, "HelmetFlametal",
                "ArmorFlametalChest", "ArmorFlametalLegs", "CapeAsh", "SwordNiedhogg", "ShieldFlametalTower");

            ConditionalRandomSet MageAsh = new("MageAshlands", "defeated_queen", 0.8f, "HelmetMage_Ashlands",
                "ArmorMageChest_Ashlands", "ArmorMageLegs_Ashlands", "StaffRoot", "StaffGreenRoots");
            
            ConditionalRandomItem Coins = new("Coins", "Coins", 1, 100);
            ConditionalRandomItem Raspberries = new("Raspberry_50", "Raspberry", 1, 50, 0.5f);
            ConditionalRandomItem Wood = new("Wood_50", "Wood", 25, 50);
            ConditionalRandomItem LeatherScraps = new("LeatherScrap_20", "LeatherScrap", 1, 20, 0.5f);
            ConditionalRandomItem Flint = new("Flint_20", "Flint", 1, 20, 0.5f, "defeated_eikthry");
            ConditionalRandomItem deerHide = new("DeerHide", "DeerHide", 1, 10, 0.5f, "defeated_eikthyr");
            ConditionalRandomItem surtlingCore = new("SurtlingCore", "SurtlingCore", 1, 3, 0.4f, "defeated_eikthyr");
            ConditionalRandomItem pickaxeAntler = new("PickaxeAntler", "PickaxeAntler", 1, 1, 0.6f, "defeated_eikthyr");
            ConditionalRandomItem tinOre = new("TinOre", "TinOre", 1, 5, 0.5f, "defeated_eikthyr");
            ConditionalRandomItem copperOre = new("CopperOre", "CopperOre", 1, 5, 0.5f, "defeated_eikthyr");
            ConditionalRandomItem deerStew = new("DeerStew", "DeerStew", 1, 5, 0.5f, "defeated_eikthyr");
            ConditionalRandomItem tin = new("Tin", "Tin", 1, 5, 0.3f, "defeated_gdking");
            ConditionalRandomItem copper = new("Copper", "Copper", 1, 5, 0.3f, "defeated_gdking");
            ConditionalRandomItem ironScrap = new("IronScrap", "IronScrap", 1, 5, 0.3f, "defeated_gdking");
            ConditionalRandomItem chain = new("Chain", "Chain", 1, 2, 0.5f, "defeated_gdking");
            ConditionalRandomItem iron = new("Iron", "Iron", 1, 5, 0.3f, "defeated_bonemass");
            ConditionalRandomItem fineWood = new("FineWood_50", "FineWood", 25, 50, 0.5f, "defeated_bonemass");
            ConditionalRandomItem silverOre = new ("SilverOre", "SilverOre", 1, 10, 0.33f, "defeated_bonemass");
            ConditionalRandomItem obsidian = new("Obsidian", "Obsidian", 1, 20, 0.4f, "defeated_bonemass");
            ConditionalRandomItem crystal = new("Crystal", "Crystal", 10, 15, 0.5f, "defeated_bonemass");
            ConditionalRandomItem silver = new("Silver", "Silver", 1, 5, 0.3f, "defeated_dragon");
            ConditionalRandomItem tissue = new("SoftTissue", "SoftTissue", 1, 5, 0.3f, "defeated_goblinking");
            
            Norseman meadows = new Norseman(Heightmap.Biome.Meadows, "Meadows_Norseman_RS", norsemen);
            meadows.conditionalRandomSets.Add(MeadowTorchSet, MeadowFlintKnifeSet, MeadowClubSet);
            meadows.conditionalRandomItems.Add(Raspberries, LeatherScraps, Flint, Wood);

            Norseman blackforest = new Norseman(Heightmap.Biome.BlackForest, "BlackForest_Norseman_RS", norsemen);
            blackforest.conditionalRandomSets.Add(MeadowClubSet, MeadowFlintKnifeSet, BlackForestTroll, BlackForestBear, BlackForestBear, BlackForestBronze);
            blackforest.conditionalRandomItems.Add(Flint, Wood, deerStew, tin, copper, tinOre, copperOre, deerHide, surtlingCore, pickaxeAntler, Coins);
            blackforest.baseHealth = 100f;
            blackforest.baseArmor = 5f;

            Norseman swamp = new Norseman(Heightmap.Biome.Swamp, "Swamp_Norseman_RS", norsemen);
            swamp.conditionalRandomSets.Add(MeadowClubSet, BlackForestBronze, SwampRoot, SwampIron);
            swamp.conditionalRandomItems.Add(Flint, tin, copper, surtlingCore, iron, ironScrap, deerStew, chain, Coins);
            swamp.baseHealth = 150f;
            swamp.baseArmor = 10f;
            
            Norseman mountains = new (Heightmap.Biome.Mountain, "Mountains_Norseman_RS", norsemen);
            mountains.conditionalRandomSets.Add(MeadowClubSet, BlackForestTroll, SwampIron, MountainFenrir, MountainSilver);
            mountains.conditionalRandomItems.Add(Coins, tin, copper, iron, silverOre, silver, obsidian, crystal, fineWood);
            mountains.baseHealth = 200f;
            mountains.baseArmor = 15f;
            
            Norseman plains = new(Heightmap.Biome.Plains, "Plains_Norseman_RS", norsemen);
            plains.conditionalRandomSets.Add(MeadowClubSet, BlackForestBear, SwampRoot, MountainFenrir, PlainsPadded, PlainsVile);
            plains.conditionalRandomItems.Add(Coins, fineWood, ironScrap, iron, silver, obsidian, chain);
            plains.baseHealth = 250f;
            plains.baseArmor = 20f;
            
            Norseman mistlands = new(Heightmap.Biome.Mistlands, "Mistlands_Norseman_RS", norsemen);
            mistlands.conditionalRandomSets.Add(BlackForestBear, SwampIron, PlainsPadded, MistlandsCarapace, MistlandsMage);
            mistlands.conditionalRandomItems.Add(Coins, fineWood, silver, iron, surtlingCore);
            mistlands.baseHealth = 300f;
            mistlands.baseArmor = 25f;
            
            Norseman ashlands = new(Heightmap.Biome.AshLands, "AshLands_Norseman_RS", norsemen);
            ashlands.conditionalRandomSets.Add(MistlandsCarapace, MistlandsMage, Flametal, Ask, MageAsh);
            ashlands.conditionalRandomItems.Add(Coins);
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
        }

        private static void SetupCommands()
        {
            NorseCommand tameAll = new NorseCommand("tame", "tames all nearby norsemen", _ =>
            {
                List<Viking> vikings = Viking.GetAllVikings();
                int count = 0;
                foreach (Viking? viking in vikings)
                {
                    if (viking.IsTamed()) continue;
                    ++count;
                    viking.SetTamed(true);
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