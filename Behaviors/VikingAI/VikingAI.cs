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

        bool isTamed = m_viking.IsTamed();

        if (isTamed)
        {
            UpdateBehaviour(dt);
        }

        if (isTamed)
        {
            UpdateAttachShip(dt);
            
            if (UpdateAttach())
            {
                return true;
            }
        }

        if (UpdateInventory(isTamed))
        {
            return true;
        }
        
        // if (UpdateSleeping(dt))
        // {
        //     return true;
        // }

        if (!isTamed)
        {
            if (HuntPlayer())
            {
                SetAlerted(true);
            }
        }
        
        UpdateTargets(m_viking, isTamed, dt, out bool canHearTarget, out bool canSeeTarget);
        
        // if (m_avoidLand && !m_character.IsSwimming())
        // {
        //     MoveToWater(dt, 20f);
        //     return true;
        // }

        if (!isTamed)
        {
            if (UpdateDespawn(dt, canSeeTarget))
            {
                return true;
            }
        }

        if (UpdateFlee(dt, isTamed, IsAlerted()))
        {
            return true;
        }

        if (UpdateAvoidFire(dt, isTamed))
        {
            return true;
        }

        if (UpdateNoMonsterArea(dt, isTamed))
        {
            return true;
        }
        
        bool isFollowing = m_follow != null;

        if (isFollowing)
        {
            if (UpdateFollow(dt))
            {
                return true;
            }
        }
        
        if (UpdateHurt(dt, isTamed))
        {
            return true;
        }
        
        bool hasTarget = m_targetStatic != null || m_targetCreature != null;
        
        if (UpdateConsume(dt, m_viking, hasTarget, isTamed))
        {
            return true;
        }

        bool shouldWork = !isTamed || m_moveType != Movement.Guard;
        
        if (!hasTarget && shouldWork)
        {
            FindWorkTargets(dt);
            if (hasWorkTarget && UpdateWork(dt, m_viking.m_pickaxe, m_viking.m_axe, m_viking.m_fishingRod, isFollowing))
            {
                return true;
            }
        }
        
        if (UpdateCircleTarget(dt, isTamed))
        {
            return true;
        }

        ItemDrop.ItemData? itemData = SelectBestAttack(m_viking, dt);
        bool hasItem = itemData != null;
        bool canAttack = itemData != null && Time.time - itemData.m_lastAttackTime > itemData.m_shared.m_aiAttackInterval;
        bool shouldAttack = m_character.GetTimeSinceLastAttack() >= m_minAttackInterval;
        bool doAttack = (itemData != null) & canAttack & shouldAttack;
        
        UpdateChargeAttack(dt, hasItem, hasTarget, doAttack, itemData);
        
        if (UpdateCirculate(dt, hasItem, doAttack, isTamed))
        {
            return true;
        }
        
        UpdateCrouch(dt, m_targetCreature, canSeeTarget);

        if (UpdateAttack(dt, itemData, doAttack, canHearTarget, canSeeTarget, isTamed))
        {
            return true;
        }

        if (!isFollowing && (!isTamed || m_moveType is Movement.Patrol))
        {
            IdleMovement(dt);
        }
        else
        {
            StopMoving();
        }
        
        if (!hasItem || !hasTarget)
        {
            ChargeStop();
        }
        return true;
    }
}