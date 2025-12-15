using HarmonyLib;
using UnityEngine;

namespace Norsemen;

public partial class VikingAI
{
    public Fish? m_fish;
    public ItemDrop.ItemData? m_bait;
    
    public bool UpdateFishing(float dt, ItemDrop.ItemData fishingRod)
    {
        if (m_fish == null) return false;

        if (m_fish.gameObject == null)
        {
            ResetWorkTargets();
            return false;
        }

        if (m_bait == null || !m_viking.GetInventory().ContainsItem(m_bait))
        {
            ResetWorkTargets();
            m_viking.UnequipItem(fishingRod);
            return false;
        }

        if (!MoveTo(dt, m_fish.transform.position, 15f, false)) return true;
        StopMoving();
        LookAt(m_fish.transform.position);
        CastFishingRod(fishingRod, m_fish);
        m_lastWorkActionTime = Time.time;
        m_nview.GetZDO().Set(VikingVars.lastWorkTime, ZNet.instance.GetTime().Ticks);
        return true;
    }

    public void CastFishingRod(ItemDrop.ItemData fishingRod, Fish fish)
    {
        if (m_viking.StartWork(fishingRod))
        {
            if (!m_viking.GetInventory().CanAddItem(fish.m_itemDrop.m_itemData, 1)) return;
            m_viking.GetInventory().AddItem(fish.m_itemDrop.m_itemData.m_dropPrefab.name, 1, fish.m_itemDrop.m_itemData.m_quality, 0, 0L, "");
            m_nview.GetZDO().Set(VikingVars.lastWorkTargetResource, fish.m_itemDrop.m_itemData.m_dropPrefab.name);
        }
    }
    
    [HarmonyPatch(typeof(FishingFloat), nameof(FishingFloat.GetOwner))]
    private static class FishingFloat_GetOwner_Patch
    {
        private static void Postfix(FishingFloat __instance, ref Character __result)
        {
            if (__result != null) return;
            long ownerID = __instance.m_nview.GetZDO().GetLong(ZDOVars.s_rodOwner);
            foreach (Viking viking in Viking.instances)
            {
                if (viking.m_nview.GetZDO().m_uid.UserID != ownerID) continue;
                __result = viking;
                return;
            }
        }
    }
}