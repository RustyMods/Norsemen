using System.Collections.Generic;
using System.Text;

namespace Norsemen;

public partial class Viking
{
    public string GetTooltip()
    {
        StringBuilder sb = new();
        sb.AppendFormat("$se_health: <color=orange>{0:0}/{1:0}</color>", GetHealth(), GetMaxHealth());
        sb.AppendFormat("\n$item_armor: <color=orange>{0:0}</color>", GetArmor());
        sb.Append($"\n$norseman_level: <color=orange>{GetLevel()}</color>");
        string owner = m_nview.GetZDO().GetString(ZDOVars.s_ownerName);
        if (!string.IsNullOrEmpty(owner))
        {
            sb.Append($"\n$norseman_owner: <color=orange>{owner}</color>");
        }

        sb.Append($"\n$norseman_state: <color=orange>$norseman_{m_vikingAI.m_behaviour.ToString().ToLower()}</color>");
        sb.Append(
            $"\n$norseman_movement: <color=orange>$norseman_{m_vikingAI.m_moveType.ToString().ToLower()}</color>)");
        
        List<StatusEffect> statusEffects = GetSEMan().GetStatusEffects();
        if (statusEffects.Count > 0)
        {
            sb.Append("\nStatus Effects:");
            for (int i = 0; i < statusEffects.Count; ++i)
            {
                StatusEffect? effect = statusEffects[i];
                sb.Append($"\n- {effect.m_name}");
            }
        }
        
        return sb.ToString();
    }
}