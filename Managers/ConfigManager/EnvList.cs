using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;

namespace Norsemen;

public class EnvList
{
    public static readonly ConfigurationManagerAttributes attributes = new ConfigurationManagerAttributes()
    {
        CustomDrawer = DrawTable
    };
    
    public readonly List<string> Envs;
    public EnvList(List<string> envs)
    {
        Envs = envs;
        if (Envs.Count == 0) Envs.Add("");
    }
    public EnvList(string envs) => Envs = envs.Split(',').ToList();
    public EnvList() => Envs = new List<string>() { "" };
    public void Add(string env) => Envs.Add(env);
    public List<string> GetValidatedList() => EnvMan.instance ? Envs.Where(env => EnvMan.instance.GetEnv(env) is not null).ToList() : Envs.Where(env => !env.IsNullOrWhiteSpace()).ToList();
    public override string ToString() => string.Join(",", Envs);
    public static void DrawTable(ConfigEntryBase cfg)
    {
        bool locked = cfg.Description.Tags.Select(a => a.GetType().Name == "ConfigurationManagerAttributes" ? (bool?)a.GetType().GetField("ReadOnly")?.GetValue(a) : null).FirstOrDefault(v => v != null) ?? false;
        List<string> newEnvs = new();
        bool wasUpdated = false;

        GUILayout.BeginVertical();
        foreach (string env in new EnvList((string)cfg.BoxedValue).Envs)
        {
            GUILayout.BeginHorizontal();
            string newEnv = GUILayout.TextField(env, new GUIStyle(GUI.skin.textField));
            string environment = locked ? env : newEnv;
            bool wasDeleted = GUILayout.Button("x", new GUIStyle(GUI.skin.button) { fixedWidth = 21 });
            bool wasAdded = GUILayout.Button("+", new GUIStyle(GUI.skin.button) { fixedWidth = 21 });
            GUILayout.EndHorizontal();
            if (wasDeleted && !locked)
            {
                wasUpdated = true;
            }
            else
            {
                newEnvs.Add(environment);
            }

            if (wasAdded && !locked)
            {
                wasUpdated = true;
                newEnvs.Add("");
            }
        }
        GUILayout.EndVertical();
        if (wasUpdated)
        {
            cfg.BoxedValue = new EnvList(newEnvs).ToString();
        }
    }
}