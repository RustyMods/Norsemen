using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

namespace Norsemen;

public static class FactionManager
{
    public static readonly Dictionary<Character.Faction, Faction> customFactions = new();
    public static readonly Dictionary<string, Character.Faction> factions = new();

    static FactionManager()
    {
        Harmony harmony = NorsemenPlugin.instance._harmony;
        harmony.Patch(AccessTools.Method(typeof(Enum), nameof(Enum.GetValues)),
            postfix: new HarmonyMethod(AccessTools.Method(typeof(FactionManager), nameof(Patch_Enum_GetValues))));
        harmony.Patch(AccessTools.Method(typeof(Enum), nameof(Enum.GetNames)),
            postfix: new HarmonyMethod(AccessTools.Method(typeof(FactionManager), nameof(Patch_Enum_GetNames))));
        harmony.Patch(AccessTools.Method(typeof(BaseAI), nameof(BaseAI.IsEnemy), new Type[]{typeof(Character), typeof(Character)}),
            prefix: new HarmonyMethod(AccessTools.Method(typeof(FactionManager), nameof(Patch_BaseAI_IsEnemy))));
    }

    private static bool Patch_BaseAI_IsEnemy(Character a, Character b, ref bool __result)
    {
        if (a is not Viking && b is not Viking) return true;
        __result = IsEnemy(a, b);
        return false;
    }

    public static bool IsEnemy(Character a, Character b)
    {
        if (a == b) return false;
        if (a is Viking vikingA) return IsEnemy(vikingA, b);
        if (b is Viking vikingB) return IsEnemy(vikingB, a);
        return a.GetFaction() != b.GetFaction();
    }

    public static bool IsEnemy(Viking viking, Character character)
    {
        if (character is Viking vikingB)
        {
            return IsEnemyToOtherViking(viking, vikingB);
        }

        if (character.GetFaction() == Character.Faction.Players)
        {
            return IsEnemyToPlayers(viking);
        }

        return IsEnemyToCreatures(viking, character);
    }

    public static bool IsEnemyToCreatures(Viking viking, Character character)
    {
        if (!customFactions.TryGetValue(viking.GetFaction(), out Faction faction)) return true;

        bool isVikingTamed = viking.IsTamed();
        bool isCharacterTamed = character.IsTamed();
        
        if (isVikingTamed && isCharacterTamed)
        {
            return false;
        }

        switch (character.GetFaction())
        {
            case Character.Faction.Boss:
                return isVikingTamed;
            case Character.Faction.AnimalsVeg:
                return !isVikingTamed;
            case Character.Faction.Dverger:
                return character.m_baseAI.IsAggravated();
            case Character.Faction.PlayerSpawned:
                return false;
            default:
                bool isCharacterTameable = character.GetComponent<Tameable>();
                bool isCharacterAlerted = character.m_baseAI.IsAlerted();
                if (isCharacterTameable)
                {
                    return faction.targetTames || isCharacterAlerted;
                }
                return true;
        }
    }
    public static bool IsEnemyToPlayers(Viking viking)
    {
        if (!customFactions.TryGetValue(viking.GetFaction(), out Faction faction)) return true;
        if (viking.IsTamed()) return false;
        if (!faction.IsFriendly()) return true;
        
        return viking.m_vikingAI.m_aggravated && viking.m_aggravatedReason != BaseAI.AggravatedReason.Building;
    }

    public static bool IsEnemyToOtherViking(Viking a, Viking b)
    {
        if (!customFactions.TryGetValue(a.GetFaction(), out Faction factionA)) return true;
        if (!customFactions.TryGetValue(b.GetFaction(), out Faction factionB)) return true;
        if (a.IsTamed() && b.IsTamed()) return false;
        return factionA != factionB;
    }

    private static void Patch_Enum_GetValues(Type enumType, ref Array __result)
    {
        if (enumType != typeof(Character.Faction)) return;
        if (factions.Count == 0) return;
        Character.Faction[] f = new Character.Faction[__result.Length + factions.Count];
        __result.CopyTo(f, 0);
        factions.Values.CopyTo(f, __result.Length);
        __result = f;
    }

    private static void Patch_Enum_GetNames(Type enumType, ref string[] __result)
    {
        if (enumType != typeof(Character.Faction)) return;
        if (factions.Count == 0) return;
        __result = __result.AddRangeToArray(factions.Keys.ToArray());
    }


    public static Character.Faction GetFaction(string name)
    {
        if (Enum.TryParse(name, true, out Character.Faction faction))
        {
            return faction;
        }

        if (factions.TryGetValue(name, out faction))
        {
            return faction;
        }

        Dictionary<Character.Faction, string> map = GetFactionMap();
        foreach (KeyValuePair<Character.Faction, string> kvp in map)
        {
            if (kvp.Value == name)
            {
                faction = kvp.Key;
                factions[name] = faction;
                return faction;
            }
        }

        faction = (Character.Faction)name.GetStableHashCode();
        factions[name] = faction;
        return faction;

    }

    private static Dictionary<Character.Faction, string> GetFactionMap()
    {
        Array values = Enum.GetValues(typeof(Character.Faction));
        string[] names = Enum.GetNames(typeof(Character.Faction));
        Dictionary<Character.Faction, string> map = new();
        for (int i = 0; i < values.Length; ++i)
        {
            map[(Character.Faction)values.GetValue(i)] = names[i];
        }
        return map;
    }
}