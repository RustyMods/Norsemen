using System.Collections.Generic;
using HarmonyLib;

namespace Norsemen;

public partial class Viking
{
    public List<string> m_pukeTalk = new List<string>()
    {
        "$norseman_puke_1", 
        "$norseman_puke_2", 
        "$norseman_puke_3", 
        "$norseman_puke_4",
        "$norseman_puke_5",
        "$norseman_puke_6", 
        "$norseman_puke_7", 
        "$norseman_puke_8", 
        "$norseman_puke_9",
        "$norseman_puke_10",
    };

    public static readonly EffectListRef m_pukeEffects = new()
    {
        dataRefs = new()
        {
            new EffectListRef.EffectDataRef("fx_Puke")
            {
                attach = true,
                inheritParentRotation = true,
                childTransform = "Jaw"
            },
            new EffectListRef.EffectDataRef("sfx_Puke_male")
            {
                variant = 0,
                attach = true
            },
            new EffectListRef.EffectDataRef("sfx_Puke_female")
            {
                variant = 1,
                attach = true
            }
        }
    };

    [HarmonyPatch(typeof(SE_Puke), nameof(SE_Puke.Setup))]
    private static class SE_Puke_Setup_Patch
    {
        private static void Postfix(SE_Puke __instance)
        {
            if (__instance.m_character is not Viking viking) return;
            viking.m_queuedTexts.Clear();
            viking.QueueSay(viking.m_pukeTalk, "emote_despair", m_pukeEffects);
        }
    }
    
    [HarmonyPatch(typeof(SE_Puke), nameof(SE_Puke.UpdateStatusEffect))]
    private static class SE_Puke_Update_Patch
    {
        private static bool Prefix(SE_Puke __instance, float dt)
        {
            if (__instance.m_character is not Viking) return true;
            __instance.m_time += dt;
            UpdateStatus(__instance, dt);
            UpdateStats(__instance, dt);
            return false;
        }
        
        private static void UpdateStatus(SE_Puke __instance, float dt)
        {
            __instance.m_removeTimer += dt;
            if (__instance.m_removeTimer <= __instance.m_removeInterval) return;
            __instance.m_removeTimer = 0.0f;
        }

        private static void UpdateStats(SE_Puke __instance, float dt)
        {
            __instance.m_tickTimer += dt;
            if (!(__instance.m_tickTimer >= __instance.m_tickInterval)) return;
            __instance.m_tickTimer = 0.0f;
            __instance.m_character.Damage(new HitData()
            {
                m_damage =
                {
                    m_damage = 1f
                },
                m_point = __instance.m_character.GetTopPoint(),
                m_hitType = HitData.HitType.Self
            });
        }
    }
}