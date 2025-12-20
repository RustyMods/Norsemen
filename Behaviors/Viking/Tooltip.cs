using System;
using System.Collections.Generic;
using System.Text;

namespace Norsemen;

public partial class Viking
{
    public string GetTooltip()
    {
        StringBuilder sb = new();
        sb.AppendFormat("$se_health: <color=#ff8080ff>{0:0}</color> ($item_total: <color=yellow>{1:0}</color>)", GetHealth(), GetMaxHealth());
        sb.AppendFormat("\n$item_armor: <color=orange>{0:0}</color>", GetArmor());
        sb.Append($"\n$norseman_level: <color=orange>{GetLevel()}</color>");
        string owner = m_nview.GetZDO().GetString(ZDOVars.s_ownerName);
        if (!string.IsNullOrEmpty(owner))
        {
            sb.Append($"\n$norseman_owner: <color=orange>{owner}</color>");
        }

        sb.Append($"\n$norseman_state: <color=orange>$norseman_{m_vikingAI.m_behaviour.ToString().ToLower()}</color>");
        sb.Append($"\n$norseman_movement: <color=orange>$norseman_{m_vikingAI.m_moveType.ToString().ToLower()}</color>");
        sb.AppendFormat("\n$item_weight: <color=orange>{0:0}</color> ($item_total: <color=yellow>{1:0}</color>)", m_inventory.GetTotalWeight(), GetMaxCarryWeight());

        HitData.DamageModifiers modifiers = GetDamageModifiers();
        foreach (HitData.DamageType damage in Enum.GetValues(typeof(HitData.DamageType)))
        {
            if (damage is
                HitData.DamageType.Chop or
                HitData.DamageType.Pickaxe or
                HitData.DamageType.Physical or
                HitData.DamageType.Elemental)
            {
                continue;
            }

            HitData.DamageModifier modifier = modifiers.GetModifier(damage);
            if (modifier is HitData.DamageModifier.Normal)
            {
                continue;
            }

            string damageType = $"$inventory_{damage.ToString().ToLower()}";
            string mod = $"$inventory_{modifier.ToString().ToLower()}";
            sb.Append($"\n{damageType}: <color=orange>{mod}</color>");
        }
        
        List<StatusEffect> statusEffects = GetSEMan().GetStatusEffects();
        if (statusEffects.Count > 0)
        {
            sb.Append("\n\n$norseman_status");
            for (int i = 0; i < statusEffects.Count; ++i)
            {
                StatusEffect? effect = statusEffects[i];
                sb.Append($"\n- {effect.m_name}");
            }
        }
        
        return sb.ToString();
    }
}