using UnityEngine;

namespace Norsemen;

public partial class VikingAI
{
    public void UpdateTargets(Viking viking, bool isTamed, float dt, out bool canHearTarget, out bool canSeeTarget)
    {
        m_unableToAttackTargetTimer -= dt;
        m_updateTargetTimer -= dt;

        if (m_updateTargetTimer <= 0.0 && !m_character.InAttack())
        {
            UpdateTargetSearch(isTamed);
        }

        if (m_targetCreature != null && isTamed)
        {
            CheckTamedTargetDistance();
        }

        ValidateCurrentTarget();

        UpdateTargetSensing(dt, out canHearTarget, out canSeeTarget);

        CheckGiveUpChase(dt);
    }

    private void UpdateTargetSearch(bool isTamed)
    {
        m_updateTargetTimer = Player.IsPlayerInRange(transform.position, 50f) ? 2f : 6f;

        if (isTamed && m_behaviour is Emotion.Passive)
        {
            m_targetCreature = null;
        }
        else
        {
            Character? enemy = FindEnemy();
            if (enemy)
            {
                m_targetCreature = enemy;
                m_targetStatic = null;
            }
        }

        bool hasPlayerTarget = m_targetCreature != null && m_targetCreature.IsPlayer();
        bool cannotReachTarget = m_targetCreature != null &&
                                 m_unableToAttackTargetTimer > 0.0 &&
                                 !HavePath(m_targetCreature.transform.position);
        
        bool shouldAttackObjects = m_attackPlayerObjects &&
                                   (!m_aggravatable || IsAggravated()) &&
                                   !ZoneSystem.instance.GetGlobalKey(GlobalKeys.PassiveMobs);

        bool needsStaticTarget = (m_targetCreature == null || cannotReachTarget) && !isTamed;

        if (shouldAttackObjects && needsStaticTarget)
        {
            FindStaticTargets(hasPlayerTarget);
        }
    }

    private void FindStaticTargets(bool hasPlayerTarget)
    {
        StaticTarget? priorityTarget = FindClosestStaticPriorityTarget();
        if (priorityTarget != null)
        {
            m_targetStatic = priorityTarget;
            m_targetCreature = null;
        }

        bool canPathToStatic = false;
        if (m_targetStatic != null)
        {
            Vector3 closestPoint = m_targetStatic.FindClosestPoint(m_character.transform.position);
            canPathToStatic = HavePath(closestPoint);
        }

        bool needsRandomTarget = (m_targetStatic == null || !canPathToStatic) && IsAlerted() && hasPlayerTarget;

        if (needsRandomTarget)
        {
            var randomTarget = FindRandomStaticTarget(10f);
            if (randomTarget != null)
            {
                m_targetStatic = randomTarget;
                m_targetCreature = null;
            }
        }
    }

    private void CheckTamedTargetDistance()
    {
        if (m_targetCreature == null) return;

        float maxDistance = m_alertRange;

        if (GetPatrolPoint(out Vector3 checkPoint))
        {
            if (Vector3.Distance(m_targetCreature.transform.position, checkPoint) > maxDistance)
            {
                m_targetCreature = null;
            }
        }
        else if (m_follow != null)
        {
            if (Vector3.Distance(m_targetCreature.transform.position, m_follow.transform.position) > maxDistance)
            {
                m_targetCreature = null;
            }
        }
    }

    private void ValidateCurrentTarget()
    {
        if (m_targetCreature == null)
        {
            return;
        }

        if (m_targetCreature.IsDead())
        {
            m_targetCreature = null;
        }
        else if (!IsEnemy(m_targetCreature))
        {
            m_targetCreature = null;
        }
        else if (m_skipLavaTargets && m_targetCreature.AboveOrInLava())
        {
            m_targetCreature = null;
        }
    }

    private void UpdateTargetSensing(float dt, out bool canHearTarget, out bool canSeeTarget)
    {
        canHearTarget = false;
        canSeeTarget = false;

        if (!hasWorkTarget && m_targetCreature != null)
        {
            canHearTarget = CanHearTarget(m_targetCreature);
            canSeeTarget = CanSeeTarget(m_targetCreature);

            if (canSeeTarget || canHearTarget) m_timeSinceSensedTargetCreature = 0.0f;

            if (m_targetCreature.IsPlayer())
            {
                m_targetCreature.OnTargeted(canSeeTarget || canHearTarget, IsAlerted());
            }

            SetTargetInfo(m_targetCreature.GetZDOID());
        }
        else
        {
            SetTargetInfo(ZDOID.None);
        }

        m_timeSinceSensedTargetCreature += dt;
    }

    private void CheckGiveUpChase(float dt)
    {
        if (!IsAlerted() && m_targetCreature == null) return;

        m_timeSinceAttacking += dt;

        bool isHuntingPlayer = HuntPlayer() && m_targetCreature != null && m_targetCreature.IsPlayer();

        // Don't give up if hunting player
        if (isHuntingPlayer) return;

        // Check if we've lost track of target for too long
        bool lostTrackTooLong = m_timeSinceSensedTargetCreature > 30.0;

        // Check if we've been unable to attack for too long or gone too far
        float attackTimeout = 60f;
        bool attackingTooLong = m_timeSinceAttacking > attackTimeout;
        bool tooFarFromSpawn = m_maxChaseDistance > 0.0 &&
                               m_timeSinceSensedTargetCreature > 1.0 &&
                               Vector3.Distance(m_spawnPoint, transform.position) > m_maxChaseDistance;

        bool shouldGiveUp = lostTrackTooLong || (attackingTooLong && tooFarFromSpawn);

        if (shouldGiveUp)
        {
            SetAlerted(false);
            m_targetCreature = null;
            m_targetStatic = null;
            m_timeSinceAttacking = 0.0f;
            m_updateTargetTimer = 5f;
        }
    }
}