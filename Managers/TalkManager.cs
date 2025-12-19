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
    public static Dictionary<TalkType, List<string>> talks;
    public static CustomSyncedValue<string> sync;

    static TalkManager()
    {
        FileName = "RandomTalks.yml";
        FilePath = Path.Combine(ConfigManager.DirectoryPath, FileName);
        talks = new Dictionary<TalkType, List<string>>();
        foreach (TalkType type in Enum.GetValues(typeof(TalkType)))
        {
            talks[type] = new();
        }
        sync = new(ConfigManager.ConfigSync, "RustyMods.Norseman.RandomTalk.Sync");
        sync.ValueChanged += OnConfigChanged;
    }

    public static List<string> GetTalk(TalkType type)
    {
        return talks[type];
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
            foreach (TalkType type in Enum.GetValues(typeof(TalkType)))
            {
                if (!talks.ContainsKey(type))
                {
                    talks[type] = new List<string>();
                }
            }
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
    
    public static void SetupTalks()
    {
        talks[TalkType.Generic].Add("Hail, traveler! Did you bring ale or just stories?");
        talks[TalkType.Generic].Add("The mist of the fjords cloaks us like a shroud... or maybe it is just the smell of troll sweat.");
        talks[TalkType.Generic].Add("Another dawn in Midgard, land of mortals... and mediocre mead.");
        talks[TalkType.Generic].Add("Beware the lurking fulings, creatures of the dark... and terrible table manners.");
        talks[TalkType.Generic].Add("Hast thou witnessed that marvel? The ale mug that never empties!");
        talks[TalkType.Generic].Add("A fleet-footed deer flees to the woods... probably to avoid hearing my bawdy jokes.");
        talks[TalkType.Generic].Add("Approach, wayfarer from distant lands! Let us swap tales and insults.");
        talks[TalkType.Generic].Add("Your hands are nimble as a troll's, friend... but can you hold your drink like one?");

        talks[TalkType.PlayerBase].Add("Who dares enter these hallowed halls? Hope you brought more than just an appetite.");
        talks[TalkType.PlayerBase].Add("Did I forget to extinguish the hearth fire? Or was it the heat from our last feast?");
        talks[TalkType.PlayerBase].Add("A scent of mischief lingers in the air... or maybe it is just the smell of unwashed socks.");
        talks[TalkType.PlayerBase].Add("Is that where you'll rest your head, under this roof of shields? Watch out for the loose thatch, it's a hazard.");
        talks[TalkType.PlayerBase].Add("Behold, a stronghold fit for a warrior! Just mind the cobwebs and the occasional rat.");
        talks[TalkType.PlayerBase].Add("What business have you within these walls? Looking for trouble or just lost your way to the outhouse?");
        talks[TalkType.PlayerBase].Add("Pay me no mind, I merely observe the surroundings... and occasionally partake in questionable activities.");
        talks[TalkType.PlayerBase].Add("By the Norns, what curious artifact is this? Or is it just a misplaced undergarment?");

        talks[TalkType.Greets].Add("Skal, fellow warrior");
        talks[TalkType.Greets].Add("Hail and well met, comrade");
        talks[TalkType.Greets].Add("By the beard of Odin, greetings");
        talks[TalkType.Greets].Add("May the hammer of Thor protect you");
        talks[TalkType.Greets].Add("Valhalla awaits, traveler");
        talks[TalkType.Greets].Add("Fair winds to you, friend");
        talks[TalkType.Greets].Add("In the name of the Allfather, greetings");
        talks[TalkType.Greets].Add("Walk the path of the shieldmaiden, traveler");

        talks[TalkType.Farewells].Add("May the Valkyries guide you");
        talks[TalkType.Farewells].Add("Safe travels on the road to Valhalla");
        talks[TalkType.Farewells].Add("Fair winds and following seas");
        talks[TalkType.Farewells].Add("Until we meet in the halls of Asgard");
        talks[TalkType.Farewells].Add("May your path be clear, warrior");
        talks[TalkType.Farewells].Add("Skal, until we cross paths again");
        talks[TalkType.Farewells].Add("May the gods watch over you");
        talks[TalkType.Farewells].Add("Farewell, may your axe stay sharp");

        talks[TalkType.Damaged].Add("By the beard of Odin, enough!");
        talks[TalkType.Damaged].Add("Back off, you insolent whelp!");
        talks[TalkType.Damaged].Add("Do not test my patience, mortal!");
        talks[TalkType.Damaged].Add("Thor's thunder, cease your folly!");
        talks[TalkType.Damaged].Add("Out of my sight, before I unleash Fenrir!");
        talks[TalkType.Damaged].Add("You tread on thin ice, beware!");
        talks[TalkType.Damaged].Add("By Yggdrasil's roots, you try my temper!");
        talks[TalkType.Damaged].Add("Enough! The wrath of the gods is not to be trifled with!");

        talks[TalkType.Puke].Add("By Thor's hammer, this feast has turned foul!");
        talks[TalkType.Puke].Add("May Loki's trickery be the cause of this wretched meal!");
        talks[TalkType.Puke].Add("Odin's ravens would not touch such a putrid offering!");
        talks[TalkType.Puke].Add("Skál to the gods for this cursed sustenance!");
        talks[TalkType.Puke].Add("The mead may be sweet, but this meal is a sour betrayal!");
        talks[TalkType.Puke].Add("A feast fit for the depths of Niflheim!");
        talks[TalkType.Puke].Add("Valhalla awaits, but this dish belongs in the depths of Hel!");
        talks[TalkType.Puke].Add("The taste of Jormungandr's venom lingers in this fare!");
        talks[TalkType.Puke].Add("Even Fenrir would turn his nose up at this spoiled repast!");
        talks[TalkType.Puke].Add("May Freyja guide us to better provisions, for this is a dish of misfortune!");

        talks[TalkType.Eat].Add("{0} tastes better than a shieldmaiden's attention");
        talks[TalkType.Eat].Add("{0} smells as good as a roasting greydwarf");
        talks[TalkType.Eat].Add("{0} makes my entrails burn with joy");
        talks[TalkType.Eat].Add("{0} melts on my tongue like a surtling in water");
        talks[TalkType.Eat].Add("{0} is fit for a last meal");

        talks[TalkType.Thieved].Add("Oi! That was mine, you thieving rat!");
        talks[TalkType.Thieved].Add("By Loki’s lies—return what you stole!");
        talks[TalkType.Thieved].Add("Hands off, sneak! I was saving that.");
        talks[TalkType.Thieved].Add("Thief! Even a greydwarf has better manners.");
        talks[TalkType.Thieved].Add("You dare rob a Norseman? Bold… and foolish.");
        talks[TalkType.Thieved].Add("I felt that! My purse grows lighter—and my mood darker.");
        talks[TalkType.Thieved].Add("By Odin’s missing eye, you’ll pay for that!");
        talks[TalkType.Thieved].Add("Steal from me again and you’ll be eating dirt.");
        
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
            string data = ConfigManager.serializer.Serialize(talks);
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
            foreach (TalkType type in Enum.GetValues(typeof(TalkType)))
            {
                if (!talks.ContainsKey(type))
                {
                    talks[type] = new List<string>();
                }
            }
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