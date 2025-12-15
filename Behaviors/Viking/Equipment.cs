using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Norsemen;

public partial class Viking
{
    public ItemDrop.ItemData? m_pickaxe;
    public ItemDrop.ItemData? m_axe;
    public ItemDrop.ItemData? m_fishingRod;
    
    public ConditionalItemSet[]? m_conditionalItemSets;
    public ConditionalRandomItem[]? m_conditionalRandomItems;

    public void AddDefaultItems()
    {
        List<string> sets = new StringList(configs.conditionalSets.Value).list;
        List<ConditionalItemSet> randomSets = new();
        foreach (string? set in sets)
        {
            if (!ConditionalRandomSet.sets.TryGetValue(set, out var data)) continue;
            randomSets.Add(data.set);
        }
        m_conditionalItemSets = randomSets.ToArray();

        List<string> items = new StringList(configs.conditionalItems.Value).list;
        List<ConditionalRandomItem> randomItems = new();
        foreach (string? item in items)
        {
            if (!Norsemen.ConditionalRandomItem.items.TryGetValue(item, out var data)) continue;
            randomItems.Add(data.item);
        }
        
        m_conditionalRandomItems = randomItems.ToArray();
        
        bool addedDefaultItems = m_nview.GetZDO().GetBool(ZDOVars.s_addedDefaultItems);
        if (addedDefaultItems) return;
        
        m_inventory.m_onChanged -= OnInventoryChanged;

        if (m_randomShield is { Length: > 0 })
        {
            int index = UnityEngine.Random.Range(0, m_randomShield.Length);
            GameObject? shield = m_randomShield[index];
            if (shield != null) GiveDefaultItem(shield);
        }

        if (m_randomWeapon is { Length: > 0 })
        {
            int index = UnityEngine.Random.Range(0, m_randomWeapon.Length);
            GameObject? weapon = m_randomWeapon[index];
            if (weapon != null) GiveDefaultItem(weapon);
        }

        if (m_randomArmor is { Length: > 0 })
        {
            int index = UnityEngine.Random.Range(0, m_randomArmor.Length);
            GameObject? armorItem = m_randomArmor[index];
            if (armorItem != null) GiveDefaultItem(armorItem);
        }

        bool addedSet = false;
        if (m_conditionalItemSets is { Length: > 0 })
        {
            List<ConditionalItemSet> availableSets = new List<ConditionalItemSet>();
            foreach (ConditionalItemSet set in m_conditionalItemSets)
            {
                if (!configs.AddSet(set.m_name)) continue;
                if (!set.HasKey()) continue;
                availableSets.Add(set);
            }

            if (availableSets.Count > 0)
            {
                float totalWeight = availableSets.Sum(x => x.m_weight);
                float chance = 0f;
                float roll = UnityEngine.Random.Range(0f, totalWeight);

                List<ConditionalItemSet> sorted = availableSets.OrderBy(x => x.m_weight).ToList();
                ConditionalItemSet? set = null;

                for (int i = 0; i < sorted.Count; ++i)
                {
                    ConditionalItemSet? possibleSet = sorted[i];
                    chance += possibleSet.m_weight;
                    if (roll < chance)
                    {
                        set = possibleSet;
                        break;
                    }
                }

                if (set == null)
                {
                    int index = sorted.Count - 1;
                    set = availableSets[index];
                }
                
                foreach (GameObject? item in set.m_items)
                {
                    if (item == null) continue;
                    GiveDefaultItem(item);
                }

                addedSet = true;
            }
        }

        if (!addedSet)
        {
            if (m_defaultItems is { Length: > 0 })
            {
                foreach (GameObject? item in m_defaultItems)
                {
                    if (item == null) continue;
                    GiveDefaultItem(item);
                }
            }
        }
        
        if (m_randomItems != null || m_conditionalRandomItems != null)
        {
            int amount = (int)Enum.GetValues(typeof(ItemDrop.ItemData.ItemType)).Cast<ItemDrop.ItemData.ItemType>().Max();
            m_randomItemSlotFilled = new bool[amount];

            if (m_randomItems is { Length: > 0 })
            {
                foreach (RandomItem? randomItem in m_randomItems)
                {
                    if (randomItem.m_prefab == null) continue;
                    float roll = UnityEngine.Random.value;
                    if (roll <= randomItem.m_chance) continue;
                    
                    int itemType = (int)randomItem.m_prefab.GetComponent<ItemDrop>().m_itemData.m_shared.m_itemType;
                    if (m_randomItemSlotFilled[itemType]) continue;
                    m_randomItemSlotFilled[itemType] = true;
                    GiveDefaultItem(randomItem.m_prefab);
                }
            }

            if (m_conditionalRandomItems is { Length: > 0 })
            {
                foreach (ConditionalRandomItem randomItem in m_conditionalRandomItems)
                {
                    if (randomItem.m_prefab == null) continue;
                    if (!configs.AddItem(randomItem.m_name)) continue;
                    if (!randomItem.HasKey()) continue;
                    float roll = UnityEngine.Random.value;
                    if (roll < randomItem.m_chance) continue;
                    
                    int stack = UnityEngine.Random.Range(randomItem.m_min, randomItem.m_max);
                    GetInventory().AddItem(randomItem.m_prefab.name, stack, 1, 0, 0L, "");
                }
            }
        }

        m_nview.GetZDO().Set(ZDOVars.s_addedDefaultItems, true);
        Save();
        m_inventory.m_onChanged += OnInventoryChanged;
    }
    
    public List<ItemDrop.ItemData?> GetEquipment()
    {
        List<ItemDrop.ItemData?> result = new()
        {
            m_helmetItem, m_legItem, m_shoulderItem, m_chestItem, m_utilityItem, m_leftItem, m_rightItem, m_ammoItem, m_trinketItem
        };
        return result;
    }
    
    public void EquipItems()
    {
        UnequipIfNotInInventory();
        List<ItemDrop.ItemData> items = m_inventory.GetAllItemsInGridOrder();
        for (int i = 0; i < items.Count; ++i)
        {
            ItemDrop.ItemData? item = items[i];
            if (!item.IsEquipable()) continue;
            
            EquipIfBetter(item);
            
            switch (item.m_shared.m_skillType)
            {
                case Skills.SkillType.Pickaxes:
                    if (m_pickaxe == null || m_pickaxe.IsItemBetter(item))
                    {
                        m_pickaxe = item;
                    }
                    break;
                case Skills.SkillType.Axes:
                    if (m_axe == null || m_axe.IsItemBetter(item))
                    {
                        m_axe = item;
                    }
                    break;
                default:
                    if (m_fishingRod == null && item.m_shared.m_animationState is ItemDrop.ItemData.AnimationState.FishingRod)
                    {
                        m_fishingRod = item;
                    }
                    break;
            }
        }
        SetupVisEquipment(m_visEquipment, false);
        GetArmor();
    }

    public override void SetupVisEquipment(VisEquipment visEq, bool isRagdoll)
    {
        if (!isRagdoll)
        {
            visEq.SetLeftItem(m_leftItem != null ? m_leftItem.m_dropPrefab.name : "", m_leftItem != null ? m_leftItem.m_variant : 0);
            visEq.SetRightItem(m_rightItem != null ? m_rightItem.m_dropPrefab.name : "");
            visEq.SetLeftBackItem(m_hiddenLeftItem != null ? m_hiddenLeftItem.m_dropPrefab.name : "", m_hiddenLeftItem != null ? m_hiddenLeftItem.m_variant : 0);
            visEq.SetRightBackItem(m_hiddenRightItem != null ? m_hiddenRightItem.m_dropPrefab.name : "");
        }

        visEq.SetChestItem(m_chestItem != null ? m_chestItem.m_dropPrefab.name : "");
        visEq.SetLegItem(m_legItem != null ? m_legItem.m_dropPrefab.name : "");
        visEq.SetHelmetItem(m_helmetItem != null ? m_helmetItem.m_dropPrefab.name : "");
        visEq.SetShoulderItem(m_shoulderItem != null ? m_shoulderItem.m_dropPrefab.name : "", m_shoulderItem != null ? m_shoulderItem.m_variant : 0);
        visEq.SetUtilityItem(m_utilityItem != null ? m_utilityItem.m_dropPrefab.name : "");
        visEq.SetTrinketItem(m_trinketItem != null ? m_trinketItem.m_dropPrefab.name : "");
        
        visEq.SetBeardItem(m_beardItem);
        visEq.SetHairItem(m_hairItem);
        visEq.SetHairColor(m_hairColor);
        visEq.SetSkinColor(m_skinColor);
    }
    
    public void EquipIfBetter(ItemDrop.ItemData item)
    {
        switch (item.m_shared.m_itemType)
        {
            case ItemDrop.ItemData.ItemType.Helmet:
                if (m_helmetItem == null || m_helmetItem.IsItemBetter(item))
                {
                    EquipItem(item);
                }
                break;
            case ItemDrop.ItemData.ItemType.Chest:
                if (m_chestItem == null || m_chestItem.IsItemBetter(item))
                {
                    EquipItem(item);
                }
                break;
            case ItemDrop.ItemData.ItemType.Shoulder:
                if (m_shoulderItem == null || m_shoulderItem.IsItemBetter(item))
                {
                    EquipItem(item);
                }
                break;
            case ItemDrop.ItemData.ItemType.Legs:
                if (m_legItem == null || m_legItem.IsItemBetter(item))
                {
                    EquipItem(item);
                }
                break;
            case ItemDrop.ItemData.ItemType.Utility:
                if (m_utilityItem == null)
                {
                    EquipItem(item);
                }
                break;
            case ItemDrop.ItemData.ItemType.Shield:
                if (m_leftItem == null || m_leftItem.IsItemBetter(item))
                {
                    EquipItem(item);
                }
                break;
            case ItemDrop.ItemData.ItemType.Ammo:
                if (m_ammoItem == null || m_ammoItem.IsItemBetter(item))
                {
                    EquipItem(item);
                }
                break;
            default:
                if (m_rightItem == null || m_rightItem.IsItemBetter(item))
                {
                    EquipItem(item);
                }
                break;
        }
    }
    
    public void UnequipIfNotInInventory()
    {
        List<ItemDrop.ItemData?> equipment = GetEquipment();
        for (int i = 0; i < equipment.Count; ++i)
        {
            ItemDrop.ItemData? item = equipment[i];
            if (item == null || m_inventory.ContainsItem(item)) continue;
            UnequipItem(item);
        }

        if (m_pickaxe != null && !m_inventory.ContainsItem(m_pickaxe))
        {
            m_pickaxe = null;
        }

        if (m_axe != null && !m_inventory.ContainsItem(m_axe))
        {
            m_axe = null;
        }

        if (m_fishingRod != null && !m_inventory.ContainsItem(m_fishingRod))
        {
            m_fishingRod = null;
        }
    }

    [Serializable]
    public class ConditionalItemSet
    {
        public string m_name = "";
        public GameObject?[] m_items = Array.Empty<GameObject>();
        public string m_requiredDefeatKey = "";
        public float m_weight = 1f;
        public bool HasKey() => string.IsNullOrEmpty(m_requiredDefeatKey) ||
                                ZoneSystem.instance.GetGlobalKey(m_requiredDefeatKey);
    }

    [Serializable]
    public class ConditionalRandomItem
    {
        public string m_name = "";
        public GameObject? m_prefab;
        public string m_requiredDefeatKey = "";
        public float m_chance = 0.5f;
        public int m_min = 1;
        public int m_max = 1;
        
        public bool HasKey() => string.IsNullOrEmpty(m_requiredDefeatKey) || ZoneSystem.instance.GetGlobalKey(m_requiredDefeatKey);
    }
}