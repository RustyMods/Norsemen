using System;
using System.Collections.Generic;
using UnityEngine;

namespace Norsemen;

public partial class VikingAI
{
    public bool UpdateConsume(float dt, Viking character, bool hasTarget, bool isTamed)
    {
        if (m_consumeItems == null || m_consumeItems.Count == 0)
        {
            return false;
        }
        
        bool shouldSearchForFood = !IsAlerted() && !hasTarget;
        
        if (shouldSearchForFood && UpdateConsumeSearch(character, isTamed, dt))
        {
            return true;
        }

        return false;
    }
    
    public bool IsFoodItem(ItemDrop.ItemData item)
    {
        return m_consumeItems.Exists(i => i.m_itemData.m_shared.m_name == item.m_shared.m_name);
    }

    public bool EatInventoryFood(Viking viking)
    {
        Inventory? inventory = viking.GetInventory();
        List<ItemDrop.ItemData>? consumables = inventory.GetAllItemsOfType(ItemDrop.ItemData.ItemType.Consumable);
        
        ItemDrop.ItemData? targetItem = null;
        if (consumables.Count > 0)
        {
            foreach (ItemDrop.ItemData? consumable in consumables)
            {
                if (IsFoodItem(consumable))
                {
                    targetItem = consumable;
                    break;
                }
            }
        }

        if (targetItem == null) return false;
        viking.OnConsumedItem(targetItem);
        inventory.RemoveItem(targetItem, 1);
        return true;
    }

    public bool UpdateConsumeSearch(Viking viking, bool isTamed, float dt)
    {
        m_consumeSearchTimer += dt;
        if (m_consumeSearchTimer > m_consumeSearchInterval)
        {
            m_consumeSearchTimer = 0.0f;
            if (!viking.IsHungry())
            {
                return false;
            }

            if (isTamed && EatInventoryFood(viking))
            {
                return false;
            }

            if (isTamed && m_moveType is Movement.Guard)
            {
                return false;
            }
            
            m_consumeTarget = FindClosestConsumableItem(m_consumeSearchRange);
        }
        else
        {
            return false;
        }

        if (!m_consumeTarget)
        {
            return false;
        }
        if (!MoveTo(dt, m_consumeTarget.transform.position, m_consumeRange, false))
        {
            return true;
        }
        
        LookAt(m_consumeTarget.transform.position);

        if (!IsLookingAt(m_consumeTarget.transform.position, 20f) || !m_consumeTarget.RemoveOne())
        {
            return true;
        }

        viking.OnConsumedItem(m_consumeTarget);
        m_consumeTarget = null;

        return false;
    }
}