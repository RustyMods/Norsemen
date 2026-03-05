using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Norsemen;

public partial class VikingAI
{
    public float m_lastWorkActionTime;
    public float m_workInterval = 10f;
    public float m_lastResourceGainedTime;
    public bool UpdateWork(float dt, ItemDrop.ItemData pickaxe, ItemDrop.ItemData axe, ItemDrop.ItemData fishingRod, bool isFollowing)
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
        
        bool canWork = !NorsemanConfigs.DoesWorkRequireFood(m_viking.prefabName) || !m_viking.IsHungry() || !m_viking.IsTamed();

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

        bool canMine = NorsemanConfigs.CanVikingMine(m_viking.prefabName);
        
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

        bool canLumber = NorsemanConfigs.CanVikingLumber(m_viking.prefabName);
        if (axe != null && canLumber)
        {
            if (UpdateLogging(dt, axe, isFollowing))
            {
                return true;
            }
        }

        bool canFish = NorsemanConfigs.CanVikingFish(m_viking.prefabName);
        
        if (fishingRod != null && canFish)
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
        
        float difference = Time.time - m_lastResourceGainedTime;
        if (difference < NorsemanConfigs.GetWorkResourceInterval(m_viking.prefabName)) return;
        
        DropTable.DropData randomResource = resources[Random.Range(0, resources.Count)];
        string prefabName = randomResource.m_item.name;
        m_nview.GetZDO().Set(VikingVars.lastWorkTargetResource, prefabName);
        if (!m_viking.GetInventory().CanAddItem(randomResource.m_item, 1)) return;
        m_viking.GetInventory().AddItem(prefabName, 1, 1, 0, 0L, "");
        m_lastResourceGainedTime = Time.time;
    }
}