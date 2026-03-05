using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using UnityEngine;

namespace Norsemen;

public class RequirementList
{
    public readonly List<Requirement> requirements;
    public RequirementList() => requirements = new List<Requirement>();
    
    public RequirementList(List<Requirement> reqs) => requirements = reqs;

    public RequirementList(string cfg)
    {
        requirements = new List<Requirement>();
        if (string.IsNullOrEmpty(cfg)) return;
        string[] reqs = cfg.Split(';');
        for (int i = 0; i < reqs.Length; ++i)
        {
            string req = reqs[i];
            string[] parts = req.Split(':');
            if (parts.Length < 2) continue;
            string item = parts[0].Trim();
            int amount = int.TryParse(parts[1].Trim(), out int count) ? count : 1;
            Add(item, amount);
        }
    }

    public void Add(string item, int amount) => requirements.Add(new Requirement 
        { item = item, amount = amount });
    
    public override string ToString() => string.Join(";", requirements);

    public struct Requirement
    {
        public string item;
        public int amount;
        
        public override string ToString() => $"{item}:{amount}";
    }

    public Piece.Requirement[] ToPieceRequirements() => requirements.Select(r => new Piece.Requirement
    {
        m_resItem = ObjectDB.instance.GetItemPrefab(r.item)?.GetComponent<ItemDrop>(),
        m_amount = r.amount,
    })
        .Where(r => r.m_resItem != null)
        .ToArray();

    public static void Draw(ConfigEntryBase cfg)
    {
        bool locked = cfg.Description.Tags
            .Select(a =>
                a.GetType().Name == "ConfigurationManagerAttributes"
                    ? (bool?)a.GetType().GetField("ReadOnly")?.GetValue(a)
                    : null).FirstOrDefault(v => v != null) ?? false;
        bool wasUpdated = false;
        List<Requirement> requirements = new List<Requirement>();
        GUILayout.BeginVertical();
        List<Requirement> cfgReqs = new RequirementList((string)cfg.BoxedValue).requirements;
        if (cfgReqs.Count == 0)
        {
            cfgReqs.Add(new Requirement { item = "", amount = 0 });
        }
        for (int i = 0; i < cfgReqs.Count; ++i)
        {
            GUILayout.BeginHorizontal();

            Requirement req = cfgReqs[i];
            var item = req.item;
            var amount = req.amount;

            var itemField = GUILayout.TextField(item);
            if (itemField != req.item && !locked)
            {
                wasUpdated = true;
                item = itemField;
            }

            var amountField = GUILayout.TextField(amount.ToString());
            var amountValue = int.TryParse(amountField, out int count) ? count : 0;
            if (amountValue != amount && !locked)
            {
                wasUpdated = true;
                amount = count;
            }
            if (GUILayout.Button("x", new GUIStyle(GUI.skin.button) { fixedWidth = 21 }) && !locked)
            {
                wasUpdated = true;
            }
            else
            {
                requirements.Add(new Requirement {item = item, amount = amount});
            }
            if (GUILayout.Button("+", new GUIStyle(GUI.skin.button) { fixedWidth = 21 }) && !locked)
            {
                requirements.Add(new Requirement {item = "", amount = 0});
                wasUpdated = true;
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();
        if (wasUpdated)
        {
            cfg.BoxedValue = new RequirementList(requirements).ToString();
        }
    }

    public static readonly ConfigurationManagerAttributes attributes = new ConfigurationManagerAttributes()
    {
        CustomDrawer = Draw
    };
}