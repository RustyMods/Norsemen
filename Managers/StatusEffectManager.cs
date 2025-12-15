using System.Collections.Generic;
using HarmonyLib;

namespace Norsemen;

public static class StatusEffectManager
{
    // private static readonly List<StatusEffect> statusEffects = new();
    // public static void Register(this StatusEffect statusEffect) => statusEffects.Add(statusEffect);
    //
    // [HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.Awake))]
    // private static class ObjectDB_Awake_Patch
    // {
    //     private static void Prefix(ObjectDB __instance)
    //     {
    //         __instance.m_StatusEffects.AddRange(statusEffects);
    //     }
    // }
}