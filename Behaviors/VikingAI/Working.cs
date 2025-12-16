using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Norsemen;

public partial class VikingAI
{
    public float m_lastWorkActionTime;
    public float m_workInterval = 10f;

    public bool UpdateWork(float dt, ItemDrop.ItemData? pickaxe, ItemDrop.ItemData? axe, ItemDrop.ItemData? fishingRod, bool isFollowing)
    {
        if (m_viking.IsInUse())
        {
            return false;
        }
        
        if (IsAlerted())
        {
            ResetWorkTargets();
            return false;
        }
        
        if (pickaxe == null && axe == null && fishingRod == null)
        {
            return false;
        }
        
        bool canWork = m_viking.configs.workRequiresFood.Value is Toggle.Off || !m_viking.IsHungry() || !m_viking.IsTamed();

        if (!canWork)
        {
            return false;
        }
        
        bool hasEnoughTimePassed = Time.time - m_lastWorkActionTime > m_workInterval;

        if (!hasEnoughTimePassed)
        {
            StopMoving();
            return true;
        }

        bool canMine = m_viking.configs.canMine.Value is Toggle.On;
        
        if (pickaxe != null && canMine)
        {
            if (UpdateMineRockMining(dt, pickaxe, isFollowing))
            {
                return true;
            }

            if (UpdateMineRock5Mining(dt, pickaxe, isFollowing))
            {
                return true;
            }

            if (UpdateDestructibleMining(dt, pickaxe, isFollowing))
            {
                return true;
            }
        }

        bool canLumber = m_viking.configs.canLumber.Value is Toggle.On;
        if (axe != null && canLumber)
        {
            if (UpdateLogging(dt, axe, isFollowing))
            {
                return true;
            }
        }

        if (fishingRod != null)
        {
            if (UpdateFishing(dt, fishingRod, isFollowing))
            {
                return true;
            }
        }
        return false;
    }
    
    public void AddResources(List<DropTable.DropData> resources)
    {
        if (resources.Count <= 0) return;
        DropTable.DropData randomResource = resources[Random.Range(0, resources.Count)];
        string prefabName = randomResource.m_item.name;
        m_nview.GetZDO().Set(VikingVars.lastWorkTargetResource, prefabName);
        if (!m_viking.GetInventory().CanAddItem(randomResource.m_item, 1)) return;
        m_viking.GetInventory().AddItem(prefabName, 1, 1, 0, 0L, "");
    }
}