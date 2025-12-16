using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Norsemen;

public static class Helpers
{
    internal static ZNetScene? _ZNetScene;
    internal static ObjectDB? _ObjectDB;
    internal static GameObject? GetPrefab(string prefabName)
    {
        if (ZNetScene.instance != null) return ZNetScene.instance.GetPrefab(prefabName);
        if (_ZNetScene == null) return null;
        GameObject? result = _ZNetScene.m_prefabs.Find(prefab => prefab.name == prefabName);
        if (result != null) return result;
        return CloneManager.clones.TryGetValue(prefabName, out GameObject clone) ? clone : result;
    }
    
    public static void Add<T>(this List<T> list, params T[] values) => list.AddRange(values);
    public static void Remove<T>(this GameObject prefab) where T : Component
    {
        if (prefab.TryGetComponent(out T component)) Object.Destroy(component);
    }
    
    public static void AddRange<T, V>(this Dictionary<T, V> dict, Dictionary<T, V> other)
    {
        foreach (KeyValuePair<T, V> kvp in other)
        {
            dict[kvp.Key] = kvp.Value;
        }
    }

    public static void ClearEquipment(this VisEquipment visEq)
    {
        ZDO? zdo = visEq.m_nview.GetZDO();
        if (zdo == null) return;

        zdo.Set(ZDOVars.s_leftItem, 0);
        zdo.Set(ZDOVars.s_rightItem, 0);
        zdo.Set(ZDOVars.s_chestItem, 0);
        zdo.Set(ZDOVars.s_legItem, 0);
        zdo.Set(ZDOVars.s_helmetItem, 0);
        zdo.Set(ZDOVars.s_shoulderItem, 0);
        zdo.Set(ZDOVars.s_utilityItem, 0);
        zdo.Set(ZDOVars.s_trinketItem, 0);

        visEq.m_leftItem = "";
        visEq.m_rightItem = "";
        visEq.m_chestItem = "";
        visEq.m_legItem = "";
        visEq.m_helmetItem = "";
        visEq.m_shoulderItem = "";
        visEq.m_utilityItem = "";
        visEq.m_trinketItem = "";
    }
    
    public static void CopyFieldsFrom<T, V>(this T target, V source)
        where T : Humanoid
        where V : Humanoid
    {
        Dictionary<string, FieldInfo> targetFields = typeof(T)
            .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .ToDictionary(f => f.Name);

        FieldInfo[] sourceFields = typeof(V).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        foreach (FieldInfo sourceField in sourceFields)
        {
            if (!targetFields.TryGetValue(sourceField.Name, out FieldInfo targetField))
                continue;
            if (!targetField.FieldType.IsAssignableFrom(sourceField.FieldType))
                continue;

            targetField.SetValue(target, sourceField.GetValue(source));
        }
    }
    
    public static void Copy<T>(this T target, T source)
    {
        foreach (FieldInfo field in typeof(T).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            object? value = field.GetValue(source);
            if (value == null) continue;
            field.SetValue(target, value);
        }
    }


    public static bool IsItemBetter(this ItemDrop.ItemData item, ItemDrop.ItemData other)
    {
        switch (item.m_shared.m_skillType)
        {
            case Skills.SkillType.Pickaxes when other.m_shared.m_skillType == Skills.SkillType.Pickaxes:
            case Skills.SkillType.Axes when other.m_shared.m_skillType == Skills.SkillType.Axes:
                return item.m_shared.m_toolTier < other.m_shared.m_toolTier;
            default:
                switch (item.m_shared.m_itemType)
                {
                    case ItemDrop.ItemData.ItemType.Torch:
                        return false;
                    case ItemDrop.ItemData.ItemType.OneHandedWeapon:
                    case ItemDrop.ItemData.ItemType.TwoHandedWeapon:
                    case ItemDrop.ItemData.ItemType.TwoHandedWeaponLeft:
                    case ItemDrop.ItemData.ItemType.Bow:
                    case ItemDrop.ItemData.ItemType.Hands:
                    case ItemDrop.ItemData.ItemType.Ammo:
                        return item.GetDamage().GetTotalDamage() < other.GetDamage().GetTotalDamage();
                    case ItemDrop.ItemData.ItemType.Shield:
                        return item.m_shared.m_blockPower < other.m_shared.m_blockPower;
                    case ItemDrop.ItemData.ItemType.Trinket:
                        return item.m_shared.m_maxAdrenaline < other.m_shared.m_maxAdrenaline;
                    default:
                        return item.GetArmor() < other.GetArmor();
                }
        }
    }

    public static bool IsOreVein(this List<DropTable.DropData> drops) =>
        drops.Exists(x => !x.m_item.GetComponent<ItemDrop>().m_itemData.m_shared.m_teleportable);
}