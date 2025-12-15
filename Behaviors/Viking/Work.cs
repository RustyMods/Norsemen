using System;
using UnityEngine;

namespace Norsemen;

public partial class Viking
{
    public bool StartWork(ItemDrop.ItemData item)
    {
        if (InAttack() && !HaveQueuedChain() || InDodge() || !CanMove() || IsKnockedBack() || IsStaggering() || InMinorAction())
            return false;
        if (m_currentAttack != null)
        {
            m_currentAttack.Stop();
            m_previousAttack = m_currentAttack;
            m_currentAttack = null;
        }

        Attack attack = item.m_shared.m_attack.Clone();
        attack.m_damageMultiplier = 0f;
        attack.m_hitTerrain = false;
        
        if (!attack.Start(this, m_body, m_zanim, m_animEvent, m_visEquipment, item, m_previousAttack, m_timeSinceLastAttack, 1f))
        {
            return false;
        }
        ClearActionQueue();
        StartAttackGroundCheck();
        m_currentAttack = attack;
        m_currentAttackIsSecondary = false;
        m_lastCombatTimer = 0.0f;
        return true;
    }

    public void CheckLastWork()
    {
        string item = m_nview.GetZDO().GetString(VikingVars.lastWorkTargetResource);
        if (string.IsNullOrEmpty(item)) return;
        
        long lastWorkTime = m_nview.GetZDO().GetLong(VikingVars.lastWorkTime, ZNet.instance.GetTime().Ticks);
        
        DateTime dateTime = new DateTime(lastWorkTime);
        double difference = (ZNet.instance.GetTime() - dateTime).TotalSeconds;
        int increment = (int)(difference % 10);
        if (increment <= 0)
        {
            increment = 1;
        }
        
        GetInventory().AddItem(item, increment, 1, 0, 0L, "");
        
        NorsemenPlugin.LogDebug($"[{GetName()}] received {item} x{increment}");
        
        ResetLastWork();
    }

    public void ResetLastWork()
    {
        m_nview.GetZDO().Set(VikingVars.lastWorkTime, ZNet.instance.GetTime().Ticks);
        m_nview.GetZDO().Set(VikingVars.lastWorkTargetResource, string.Empty);
    }
    
    
}