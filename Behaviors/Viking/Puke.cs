using System.Collections.Generic;
using HarmonyLib;

namespace Norsemen;

public partial class Viking
{
    private static readonly EffectListRef pukeFX = new()
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
            viking.QueueSay(TalkManager.GetTalk(TalkManager.TalkType.Puke), "", "emote_despair", pukeFX);
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