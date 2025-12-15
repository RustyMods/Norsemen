using System.Collections.Generic;
using UnityEngine;

namespace Norsemen;

public partial class VikingAI : MonsterAI
{
    public Viking m_viking = null!;

    public override void Awake()
    {
        base.Awake();
        m_viking = GetComponent<Viking>();

        bool isTamed = m_viking.IsTamed();
        m_aggravatable = m_aggravatable && !isTamed;
    }
    
    public override bool UpdateAI(float dt)
    {
        if (!UpdateBaseAI(dt))
        {
            return false;
        }
        
        if (UpdateSleeping(dt))
        {
            return true;
        }

        if (m_viking.IsAttached())
        {
            return true;
        }
        
        if (HuntPlayer())
        {
            SetAlerted(true);
        }
        
        UpdateTargets(m_viking, dt, out bool canHearTarget, out bool canSeeTarget);
        
        if (m_avoidLand && !m_character.IsSwimming())
        {
            MoveToWater(dt, 20f);
            return true;
        }

        if (UpdateDespawn(dt, canSeeTarget))
        {
            return true;
        }

        if (UpdateFlee(dt))
        {
            return true;
        }

        if (UpdateAvoidFire(dt))
        {
            return true;
        }

        if (UpdateNoMonsterArea(dt))
        {
            return true;
        }

        if (UpdateHurt(dt))
        {
            return true;
        }
        
        bool hasTarget = m_targetStatic != null || m_targetCreature != null;
        
        if (UpdateConsume(dt, m_viking, hasTarget))
        {
            return true;
        }
        
        if (!hasTarget)
        {
            FindWorkTargets(dt);
            if (hasWorkTarget && UpdateWork(dt, m_viking.m_pickaxe, m_viking.m_axe, m_viking.m_fishingRod))
            {
                return true;
            }
        }
        
        if (UpdateCircleTarget(dt))
        {
            return true;
        }

        ItemDrop.ItemData? itemData = SelectBestAttack(m_viking, dt);
        bool hasItem = itemData != null;
        bool canAttack = itemData != null && Time.time - itemData.m_lastAttackTime > itemData.m_shared.m_aiAttackInterval;
        bool shouldAttack = m_character.GetTimeSinceLastAttack() >= m_minAttackInterval;
        bool doAttack = (itemData != null) & canAttack & shouldAttack;
        
        UpdateChargeAttack(dt, hasItem, hasTarget, doAttack, itemData);
        
        if (UpdateCirculate(dt, hasItem, doAttack))
        {
            return true;
        }

        if (UpdateFollowOrIdle(dt, hasTarget, hasItem))
        {
            return true;
        }

        if (UpdateAttack(dt, itemData, doAttack, canHearTarget, canSeeTarget))
        {
            return true;
        }
        return true;
    }
}