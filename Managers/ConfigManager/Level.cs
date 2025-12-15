using System;
using System.Globalization;
using System.Linq;
using BepInEx.Configuration;
using UnityEngine;

namespace Norsemen;

public class Level
{
    public static readonly ConfigurationManagerAttributes attributes = new()
    {
        CustomDrawer = DrawTable
    };
    
    public readonly int Min;
    public readonly int Max;
    public readonly float Chance;

    public Level(int min, int max, float chance)
    {
        Min = min;
        Max = max;
        Chance = chance;
    }

    public Level(string level)
    {
        var parts = level.Split(':');
        Min = int.TryParse(parts[0], out int min) ? min : 1;
        Max = int.TryParse(parts[1], out int max) ? max : 1;
        Chance = float.TryParse(parts[2], out float chance) ? chance : 0.5f;
    }

    public static void DrawTable(ConfigEntryBase cfg)
    {
        bool locked = cfg.Description.Tags.Select(a => a.GetType().Name == "ConfigurationManagerAttributes" ? (bool?)a.GetType().GetField("ReadOnly")?.GetValue(a) : null).FirstOrDefault(v => v != null) ?? false;
        Level data = new Level((string)cfg.BoxedValue);
        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Min: ");
        int newMin = !locked ? int.TryParse(GUILayout.TextField(data.Min.ToString(), new GUIStyle(GUI.skin.textField)), out int newMinimum) ? newMinimum : data.Min : data.Min;
        GUILayout.Label("Max: ");
        int newMax = !locked ?int.TryParse(GUILayout.TextField(data.Max.ToString(), new GUIStyle(GUI.skin.textField)), out int newMaximum) ? newMaximum : data.Max : data.Max;
        GUILayout.Label("LevelUp Chance: ");
        float newChance = !locked
            ? float.TryParse(
                GUILayout.TextField(data.Chance.ToString(CultureInfo.InvariantCulture)), out float newCha)
                    ? newCha
                    : data.Chance : data.Chance;
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();

        if (newMin != data.Min || newMax != data.Max || Math.Abs(newChance - data.Chance) > 0.01f)
        {
            cfg.BoxedValue = new Level(newMin, newMax, newChance).ToString();
        }
    }
    public override string ToString() => $"{Min}:{Max}:{Chance}";
}