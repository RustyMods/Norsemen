using UnityEngine;

namespace Norsemen;

public partial class VikingAI
{
    public bool UpdateAttack(float dt, ItemDrop.ItemData? itemData, bool doAttack, bool canHearTarget, bool canSeeTarget)
    {
        if (itemData == null) return false;
        
        switch (itemData.m_shared.m_aiTargetType)
        {
            case ItemDrop.ItemData.AiTarget.Enemy:
                return UpdateEnemyAttack(dt, itemData, doAttack, canHearTarget, canSeeTarget);
            
            case ItemDrop.ItemData.AiTarget.FriendHurt:
            case ItemDrop.ItemData.AiTarget.Friend:
                return UpdateFriendAttack(dt, itemData, doAttack);
            
            default:
                return false;
        }
    }
    
    
    private bool UpdateEnemyAttack(float dt, ItemDrop.ItemData itemData, bool doAttack, bool canHearTarget, bool canSeeTarget)
    {
        if (m_targetStatic != null)
        {
            return HandleStaticTarget(dt, itemData, doAttack);
        }
        
        if (m_targetCreature != null)
        {
            return HandleCreatureTarget(dt, itemData, doAttack, canHearTarget, canSeeTarget);
        }
        
        return false;
    }
    
    private bool HandleStaticTarget(float dt, ItemDrop.ItemData itemData, bool doAttack)
    {
        Vector3 closestPoint = m_targetStatic.FindClosestPoint(transform.position);
        float distance = Vector3.Distance(closestPoint, transform.position);
        bool inRange = distance < itemData.m_shared.m_aiAttackRange;
        bool canSee = CanSeeTarget(m_targetStatic);
        
        if (inRange && canSee)
        {
            Vector3 targetCenter = m_targetStatic.GetCenter();
            LookAt(targetCenter);
            
            if (itemData.m_shared.m_aiAttackMaxAngle == 0.0)
            {
                ZLog.LogError($"AI Attack Max Angle for {itemData.m_shared.m_name} is 0!");
            }
            
            bool lookingAtTarget = IsLookingAt(targetCenter, itemData.m_shared.m_aiAttackMaxAngle, itemData.m_shared.m_aiInvertAngleCheck);
            
            if (lookingAtTarget && doAttack)
            {
                DoWeaponAttack(null, false);
            }
            else
            {
                StopMoving();
            }
        }
        else
        {
            MoveTo(dt, closestPoint, 0.0f, IsAlerted());
            ChargeStop();
        }
        
        return false;
    }
    
    private bool HandleCreatureTarget(float dt, ItemDrop.ItemData itemData, bool doAttack, bool canHearTarget, bool canSeeTarget)
    {
        bool canDetectTarget = canHearTarget || canSeeTarget || (HuntPlayer() && m_targetCreature.IsPlayer());
        
        if (canDetectTarget)
        {
            return HandleDetectedCreature(dt, itemData, doAttack, canSeeTarget);
        }
        
        return HandleLostCreature(dt);
    }
    
    private bool HandleDetectedCreature(float dt, ItemDrop.ItemData itemData, bool doAttack, bool canSeeTarget)
    {
        m_beenAtLastPos = false;
        m_lastKnownTargetPos = m_targetCreature.transform.position;
        
        float distanceToTarget = Vector3.Distance(m_lastKnownTargetPos, transform.position) - m_targetCreature.GetRadius();
        float alertDistance = m_alertRange * m_targetCreature.GetStealthFactor();
        
        if (canSeeTarget && distanceToTarget < alertDistance)
        {
            SetAlerted(true);
        }

        float range = itemData.m_shared.m_skillType is Skills.SkillType.Bows or Skills.SkillType.Crossbows
            ? 10f
            : itemData.m_shared.m_attack.m_attackRange;
        
        bool inAttackRange = distanceToTarget < range;
        bool shouldMove = !inAttackRange || !canSeeTarget || !IsAlerted();
        
        if (shouldMove)
        {
            Vector3 interceptPosition = CalculateInterceptPosition(distanceToTarget);
            MoveTo(dt, interceptPosition, 0.0f, IsAlerted());
            
            if (m_timeSinceAttacking > 15.0)
            {
                m_unableToAttackTargetTimer = 15f;
            }
        }
        else
        {
            StopMoving();
        }

        bool isAlerted = IsAlerted();
        
        if (inAttackRange && canSeeTarget && isAlerted)
        {
            if (PheromoneFleeCheck(m_targetCreature))
            {
                Flee(dt, m_targetCreature.transform.position);
                m_updateTargetTimer = Random.Range(m_fleePheromoneMin, m_fleePheromoneMax);
                m_targetCreature = null;
            }
            else
            {
                LookAt(m_targetCreature.GetTopPoint());
                bool lookingAtTarget = IsLookingAt(m_lastKnownTargetPos, itemData.m_shared.m_aiAttackMaxAngle, itemData.m_shared.m_aiInvertAngleCheck);
                
                if (doAttack && lookingAtTarget)
                {
                    DoWeaponAttack(m_targetCreature, false);
                }
            }
        }
        else if (isAlerted)
        {
            UpdateDodge(dt, m_targetCreature);
        }
        
        return false;
    }
    
    private Vector3 CalculateInterceptPosition(float distanceToTarget)
    {
        Vector3 velocity = m_targetCreature.GetVelocity();
        Vector3 intercept = velocity * m_interceptTime;
        Vector3 position = m_lastKnownTargetPos;
        
        if (distanceToTarget > intercept.magnitude / 4.0)
        {
            position += intercept;
        }
        
        return position;
    }
    
    private bool HandleLostCreature(float dt)
    {
        ChargeStop();
        
        if (m_beenAtLastPos)
        {
            RandomMovement(dt, m_lastKnownTargetPos);
            
            if (m_timeSinceAttacking > 15.0)
            {
                m_unableToAttackTargetTimer = 15f;
            }
        }
        else if (MoveTo(dt, m_lastKnownTargetPos, 0.0f, IsAlerted()))
        {
            m_beenAtLastPos = true;
        }
        
        return false;
    }

    private bool UpdateFriendAttack(float dt, ItemDrop.ItemData itemData, bool doAttack)
    {
        bool lookingForHurt = itemData.m_shared.m_aiTargetType == ItemDrop.ItemData.AiTarget.FriendHurt;
        Character? target = lookingForHurt ? HaveHurtFriendInRange(m_viewRange) : HaveFriendInRange(m_viewRange);
        
        if (target == null)
        {
            RandomMovement(dt, transform.position, true);
            return false;
        }
        
        float distance = Vector3.Distance(target.transform.position, transform.position);
        float range = itemData.m_shared.m_skillType is Skills.SkillType.Bows or Skills.SkillType.Crossbows
            ? 10f
            : itemData.m_shared.m_attack.m_attackRange;
        
        bool inRange = distance < range;
        
        if (inRange)
        {
            if (doAttack)
            {
                StopMoving();
                LookAt(target.transform.position);
                DoWeaponAttack(target, true);
            }
            else
            {
                RandomMovement(dt, target.transform.position);
            }
        }
        else
        {
            MoveTo(dt, target.transform.position, 0.0f, IsAlerted());
        }
        
        return false;
    }
    
    public bool DoWeaponAttack(Character? target, bool isFriend)
    {
        ItemDrop.ItemData? currentWeapon = m_viking.GetCurrentWeapon();
        if (currentWeapon == null || !CanUseAttack(currentWeapon)) return false;
        bool secondary = !string.IsNullOrEmpty(currentWeapon.m_shared.m_secondaryAttack.m_attackAnimation) && UnityEngine.Random.value > 0.5f;
        if (!m_viking.StartAttack(target, secondary))
        {
            return false;
        }
        m_timeSinceAttacking = 0.0f;
        return true;
    }
}