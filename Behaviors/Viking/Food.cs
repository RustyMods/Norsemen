using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace Norsemen;

public partial class Viking
{
    public static readonly List<ItemDrop> consumableItems = new();
    
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

    public void SetupFood()
    {
        m_vikingAI.m_consumeItems.AddRange(consumableItems);
    }

    public override bool CanConsumeItem(ItemDrop.ItemData item, bool checkWorldLevel = false)
    {
        if (item.m_shared.m_itemType != ItemDrop.ItemData.ItemType.Consumable) return false;
        return IsHungry();
    }

    public void EatFood(ItemDrop.ItemData item)
    {
        if (IsHungry())
        {
            m_sootheEffect.Create(GetCenterPoint(), Quaternion.identity);
        }
        ResetFeedingTimer();
        
        List<string> consumedTalk = GetConsumeTalk(item.m_shared.m_name);
        m_queuedTexts.Clear();
        QueueSay(consumedTalk);
    }
    
    public static List<string> GetItemSay(ItemDrop.ItemData item)
    {
        string say = $"{item.m_shared.m_name} $item_type_say_{item.m_shared.m_skillType.ToString().ToLower()}";
        if (say.Contains("[")) return new List<string>();
        return new List<string>()
        {
            say
        };
    }
    
    public static List<string> GetConsumeTalk(string sharedName)
    {
        string say = $"{sharedName} {sharedName}_say";
        string? localized = Localization.instance.Localize(say);
    
        return localized.Contains("[") ? new List<string>()
        {
            $"{sharedName} $norseman_consume_1",
            $"{sharedName} $norseman_consume_2",
            $"{sharedName} $norseman_consume_3",
            $"{sharedName} $norseman_consume_4",
            $"{sharedName} $norseman_consume_5"
        } : new List<string>() { say };
    }
}