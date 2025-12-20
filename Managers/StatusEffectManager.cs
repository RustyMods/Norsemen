using System.Collections.Generic;
using HarmonyLib;

namespace Norsemen;

public static class StatusEffectManager
{
    private static readonly List<StatusEffect> statusEffects = new();
    public static void Register(this StatusEffect statusEffect) => statusEffects.Add(statusEffect);

    static StatusEffectManager()
    {
        Harmony harmony = NorsemenPlugin.instance._harmony;
        harmony.Patch(AccessTools.Method(typeof(ObjectDB), nameof(ObjectDB.Awake)),
            prefix: new HarmonyMethod(AccessTools.Method(typeof(StatusEffectManager), nameof(Patch_ObjectDB_Awake))));
    }

    public static void Patch_ObjectDB_Awake(ObjectDB __instance)
    {
        __instance.m_StatusEffects.AddRange(statusEffects);
    }
}