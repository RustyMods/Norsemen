using UnityEngine;

namespace Norsemen;

public partial class VikingAI
{
    public bool UpdateAttack(float dt, ItemDrop.ItemData? itemData, bool doAttack, bool canHearTarget, bool canSeeTarget, bool isTamed)
    {
        if (itemData == null) return false;
        
        switch (itemData.m_shared.m_aiTargetType)
        {
            case ItemDrop.ItemData.AiTarget.Enemy:
                return UpdateEnemyAttack(dt, itemData, doAttack, canHearTarget, canSeeTarget, isTamed);
            
            case ItemDrop.ItemData.AiTarget.FriendHurt:
            case ItemDrop.ItemData.AiTarget.Friend:
                return UpdateFriendAttack(dt, itemData, doAttack, isTamed);
            
            default:
                return false;
        }
    }
    
    
    private bool UpdateEnemyAttack(float dt, ItemDrop.ItemData itemData, bool doAttack, bool canHearTarget, bool canSeeTarget, bool isTamed)
    {
        if (m_targetStatic != null)
        {
            return HandleStaticTarget(dt, itemData, doAttack, isTamed);
        }
        
        if (m_targetCreature != null)
        {
            return HandleCreatureTarget(dt, itemData, doAttack, canHearTarget, canSeeTarget, isTamed);
        }
        
        return false;
    }

    public static float GetWeaponRange(ItemDrop.ItemData itemData)
    {
        switch (itemData.m_shared.m_skillType)
        {
            case Skills.SkillType.Bows:
            case Skills.SkillType.Crossbows:
            case Skills.SkillType.ElementalMagic:
            case Skills.SkillType.BloodMagic:
                return UnityEngine.Random.Range(15f, 30f);
            default:
                return itemData.m_shared.m_attack.m_attackRange;
        }
    }
    
    private bool HandleStaticTarget(float dt, ItemDrop.ItemData itemData, bool doAttack, bool isTamed)
    {
        Vector3 closestPoint = m_targetStatic.FindClosestPoint(transform.position);
        float distance = Vector3.Distance(closestPoint, transform.position);
        bool inRange = distance < GetWeaponRange(itemData);
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
                return true;
            }
            StopMoving();
            return true;
        }
        
        if (isTamed && m_moveType is Movement.Guard)
        {
            return false;
        }
        
        MoveTo(dt, closestPoint, 0.0f, IsAlerted());
        ChargeStop();
        
        return true;
    }
    
    private bool HandleCreatureTarget(float dt, ItemDrop.ItemData itemData, bool doAttack, bool canHearTarget, bool canSeeTarget, bool isTamed)
    {
        bool canDetectTarget = canHearTarget || canSeeTarget || (HuntPlayer() && m_targetCreature.IsPlayer());
        
        if (canDetectTarget)
        {
            return HandleDetectedCreature(dt, itemData, doAttack, canSeeTarget, isTamed);
        }
        
        return HandleLostCreature(dt, isTamed);
    }
    
    private bool HandleDetectedCreature(float dt, ItemDrop.ItemData itemData, bool doAttack, bool canSeeTarget, bool isTamed)
    {
        m_beenAtLastPos = false;
        m_lastKnownTargetPos = m_targetCreature.transform.position;
        
        float distanceToTarget = Vector3.Distance(m_lastKnownTargetPos, transform.position) - m_targetCreature.GetRadius();
        float alertDistance = m_alertRange * m_targetCreature.GetStealthFactor();
        
        if (canSeeTarget && distanceToTarget < alertDistance)
        {
            SetAlerted(true);
        }

        float range = GetWeaponRange(itemData);
        
        bool inAttackRange = distanceToTarget < range;
        bool shouldMove = !inAttackRange || !canSeeTarget || !IsAlerted();

        if (shouldMove && isTamed && m_moveType is Movement.Guard)
        {
            return true;
        }
        
        if (shouldMove)
        {
            Vector3 interceptPosition = CalculateInterceptPosition(distanceToTarget);
            MoveTo(dt, interceptPosition, 0.0f, IsAlerted());
            
            if (m_timeSinceAttacking > 15.0)
            {
                m_unableToAttackTargetTimer = 15f;
            }

            return true;
        }
        
        StopMoving();

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
                    return true;
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
    
    private bool HandleLostCreature(float dt, bool isTamed)
    {
        ChargeStop();
        bool shouldMove = !isTamed || m_moveType is Movement.Patrol;

        if (m_beenAtLastPos)
        {
            if (shouldMove)
            {
                RandomMovement(dt, m_lastKnownTargetPos);
            }
            
            if (m_timeSinceAttacking > 15.0)
            {
                m_unableToAttackTargetTimer = 15f;
            }

            return true;
        }
        
        if (shouldMove && MoveTo(dt, m_lastKnownTargetPos, 0.0f, IsAlerted()))
        {
            m_beenAtLastPos = true;
            return true;
        }

        return false;
    }

    private bool UpdateFriendAttack(float dt, ItemDrop.ItemData itemData, bool doAttack, bool isTamed)
    {
        bool lookingForHurt = itemData.m_shared.m_aiTargetType == ItemDrop.ItemData.AiTarget.FriendHurt;
        Character? target = lookingForHurt ? HaveHurtFriendInRange(m_viewRange) : HaveFriendInRange(m_viewRange);
        bool shouldMove = !isTamed || m_moveType is Movement.Patrol;
        if (target == null)
        {
            if (shouldMove) RandomMovement(dt, transform.position, true);
            return false;
        }
        
        float distance = Vector3.Distance(target.transform.position, transform.position);
        float range = GetWeaponRange(itemData);
        
        bool inRange = distance < range;
        
        if (inRange)
        {
            if (doAttack)
            {
                StopMoving();
                LookAt(target.transform.position);
                DoWeaponAttack(target, true);
                return true;
            }

            if (shouldMove)
            {
                RandomMovement(dt, target.transform.position);
                return true;
            }
        }
        else
        {
            if (shouldMove)
            {
                MoveTo(dt, target.transform.position, 0.0f, IsAlerted());
                return true;
            }
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