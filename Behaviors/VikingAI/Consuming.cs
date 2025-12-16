using System;
using System.Collections.Generic;
using UnityEngine;

namespace Norsemen;

public partial class VikingAI
{
    public bool UpdateConsume(float dt, Viking character, bool hasTarget)
    {
        if (m_consumeItems == null || m_consumeItems.Count == 0)
        {
            return false;
        }
        
        bool shouldSearchForFood = !IsAlerted() && !hasTarget;
        
        if (shouldSearchForFood && UpdateFoodItem(character, dt))
        {
            return true;
        }

        return false;
    }

    public void OnConsumedItem(ItemDrop.ItemData item)
    {
        Viking.m_sootheEffect.Create(m_viking.GetCenterPoint(), Quaternion.identity);
        m_viking.ResetFeedingTimer();
        List<string> consumedTalk = Viking.GetConsumeTalk(item.m_shared.m_name);
        m_viking.m_queuedTexts.Clear();
        m_viking.QueueSay(consumedTalk);
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
                if (m_consumeItems.Exists(x => x.m_itemData.m_shared.m_name == consumable.m_shared.m_name))
                {
                    targetItem = consumable;
                    break;
                }
            }
        }

        if (targetItem == null) return false;

        viking.m_consumeItemEffects.Create(transform.position, Quaternion.identity);
        OnConsumedItem(targetItem);
        string trigger = targetItem.m_shared.m_isDrink ? "drink" : "eat";
        m_animator.SetTrigger(trigger);
        inventory.RemoveOneItem(targetItem);
        
        return true;
    }

    public bool UpdateFoodItem(Viking viking, float dt)
    {
        m_consumeSearchTimer += dt;
        if (m_consumeSearchTimer > m_consumeSearchInterval)
        {
            NorsemenPlugin.LogInfo("Update search food");
            m_consumeSearchTimer = 0.0f;
            if (!viking.IsHungry())
            {
                return false;
            }

            if (viking.IsTamed() && EatInventoryFood(viking))
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
        
        NorsemenPlugin.LogDebug($"Found consumable item: {m_consumeTarget.name}");

        if (!MoveTo(dt, m_consumeTarget.transform.position, m_consumeRange, false))
        {
            return true;
        }
        
        LookAt(m_consumeTarget.transform.position);

        if (!IsLookingAt(m_consumeTarget.transform.position, 20f) || !m_consumeTarget.RemoveOne())
        {
            return true;
        }
        
        if (m_onConsumedItem != null)
        {
            m_onConsumedItem(m_consumeTarget);
        }
        viking.m_consumeItemEffects.Create(transform.position, Quaternion.identity);
        string trigger = m_consumeTarget.m_itemData.m_shared.m_isDrink ? "drink" : "eat";
        m_animator.SetTrigger(trigger);
                
        m_consumeTarget = null;

        return false;
    }
}