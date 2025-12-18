using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace Norsemen;

public partial class Viking
{
    public static readonly List<ItemDrop> consumableItems = new();
    public ItemDrop.ItemData? m_lastFoodItem;
    
    [HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.Awake))]
    private static class ObjectDB_Awake
    {
        private static void Postfix(ObjectDB __instance)
        {
            foreach (GameObject? item in __instance.m_items)
            {
                if (!item.TryGetComponent(out ItemDrop component)) continue;
                if (component.m_itemData.m_shared.m_itemType != ItemDrop.ItemData.ItemType.Consumable) continue;
                if (component.m_itemData.m_shared.m_consumeStatusEffect != null) continue;
                consumableItems.Add(component);
            }
        }
    }
    
    public void OnConsumedItem(ItemDrop item)
    {
        OnConsumedItem(item.m_itemData);
    }

    public void UpdateHealth()
    {
        float baseHealth = configs.BaseHealth;
        float health = baseHealth * m_level;
        if (m_lastFoodItem != null && !IsHungry())
        {
            health += m_lastFoodItem.m_shared.m_food;
        }

        SetMaxHealth(health);
    }

    public void OnConsumedItem(ItemDrop.ItemData item)
    {
        if (IsHungry())
        {
            m_sootheEffect.Create(GetCenterPoint(), Quaternion.identity);
            ResetFeedingTimer();
            m_queuedTexts.Clear();
            QueueSay(TalkManager.GetTalk(TalkManager.TalkType.Eat), context: item.m_shared.m_name);
            string trigger = item.m_shared.m_isDrink ? "drink" : "eat";
            m_zanim.SetTrigger(trigger);
        }
        
        if (item.m_shared.m_consumeStatusEffect != null)
        {
            bool isPukeEffect = item.m_shared.m_consumeStatusEffect is SE_Puke;
            bool isTamed = IsTamed();
            bool canAdd = item.m_shared.m_consumeStatusEffect.CanAdd(this);
            bool shouldAdd = canAdd && (isPukeEffect || isTamed);

            if (shouldAdd)
            {
                m_seman.AddStatusEffect(item.m_shared.m_consumeStatusEffect.NameHash());
                if (isPukeEffect && !isTamed)
                {
                    m_vikingAI.SetAggravated(true, BaseAI.AggravatedReason.Damage);
                }
            }
        }
    }

    public void SetupFood()
    {
        m_vikingAI.m_consumeItems.AddRange(consumableItems);
    }

    public override bool CanConsumeItem(ItemDrop.ItemData item, bool checkWorldLevel = false)
    {
        if (item.m_shared.m_itemType != ItemDrop.ItemData.ItemType.Consumable) return false;
        if (item.m_shared.m_consumeStatusEffect != null && item.m_shared.m_consumeStatusEffect.CanAdd(this)) return true;
        return IsHungry();
    }
}