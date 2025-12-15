using System;
using System.Globalization;
using System.Linq;
using BepInEx.Configuration;
using UnityEngine;

namespace Norsemen;

public class MinMax
{
    public static readonly ConfigurationManagerAttributes attributes = new ConfigurationManagerAttributes()
    {
        CustomDrawer = DrawTable
    };
    
    public readonly float Min;
    public readonly float Max;

    public MinMax(float min, float max)
    {
        Min = min;
        Max = max;
    }

    public MinMax(string altitude)
    {
        var parts = altitude.Split('-');
        Min = float.TryParse(parts[0], out float min) ? min : -1000f;
        Max = float.TryParse(parts[1], out float max) ? max : 1000f;
    }

    public static void DrawTable(ConfigEntryBase cfg)
    {
        bool locked = cfg.Description.Tags.Select(a => a.GetType().Name == "ConfigurationManagerAttributes" ? (bool?)a.GetType().GetField("ReadOnly")?.GetValue(a) : null).FirstOrDefault(v => v != null) ?? false;
        var data = new MinMax((string)cfg.BoxedValue);
        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Min: ");
        float newMin = !locked ? float.TryParse(GUILayout.TextField(data.Min.ToString(CultureInfo.InvariantCulture), new GUIStyle(GUI.skin.textField)), out float newMinimum) ? newMinimum : data.Min : data.Min;
        GUILayout.Label("Max: ");
        float newMax = !locked ?float.TryParse(GUILayout.TextField(data.Max.ToString(CultureInfo.InvariantCulture), new GUIStyle(GUI.skin.textField)), out float newMaximum) ? newMaximum : data.Max : data.Max;
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();

        if (Math.Abs(newMin - data.Min) > 0.1f || Math.Abs(newMax - data.Max) > 0.1f)
        {
            cfg.BoxedValue = new MinMax(newMin, newMax).ToString();
        }
    }

    public override string ToString() => $"{Min}-{Max}";
}