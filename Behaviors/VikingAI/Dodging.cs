using UnityEngine;

namespace Norsemen;

public partial class VikingAI
{
    private float m_dodgeTimer = 0f;
    private float m_dodgeCooldown = 5f;  
    private float m_dodgeCheckInterval = 0.2f;  
    private float m_dodgeCheckTimer = 0f;
    private float m_dodgeDistance = 15f; 
    private float m_dodgeThreatDistance = 5f;
    
    public bool UpdateDodge(float dt, Character? target)
    {
        if (m_dodgeTimer > 0f)
        {
            m_dodgeTimer -= dt;
        }
        
        m_dodgeCheckTimer -= dt;
        if (m_dodgeCheckTimer > 0f) return false;
        m_dodgeCheckTimer = m_dodgeCheckInterval;

        if (target == null) return false;
        
        if (!IsAlerted() || !m_viking.CanDodge()) return false;
        if (m_dodgeTimer > 0f) return false; 
        
        float distance = Vector3.Distance(target.transform.position, transform.position);
        if (distance > m_dodgeDistance) return false;
        
        bool shouldDodge = ShouldDodgeTarget(target, distance);
        
        if (shouldDodge)
        {
            Vector3 toTarget = target.transform.position - transform.position;
            Vector3 dodgeDir = CalculateDodgeDirection(toTarget.normalized, distance);
            m_viking.Dodge(dodgeDir);
            m_dodgeTimer = m_dodgeCooldown;
            return true;
        }

        return false;
    }

    private bool ShouldDodgeTarget(Character target, float distance)
    {
        bool isAttacking = target.InAttack() || target.IsDrawingBow();
        
        if (!isAttacking) return false;
        
        float dodgeChance;
        if (distance < m_dodgeThreatDistance)
        {
            dodgeChance = Mathf.Lerp(0.8f, 0.6f, distance / m_dodgeThreatDistance);
        }
        else
        {
            float t = (distance - m_dodgeThreatDistance) / (m_dodgeDistance - m_dodgeThreatDistance);
            dodgeChance = Mathf.Lerp(0.6f, 0.1f, t);
        }
        
        return UnityEngine.Random.value < dodgeChance;
    }

    private Vector3 CalculateDodgeDirection(Vector3 toTargetNormalized, float distance)
    {
        if (distance < m_dodgeThreatDistance)
        {
            return -toTargetNormalized;
        }
        
        Vector3 right = Vector3.Cross(Vector3.up, toTargetNormalized);
        float sideDirection = UnityEngine.Random.value > 0.5f ? 1f : -1f;
        
        Vector3 dodgeDir = (-toTargetNormalized + right * sideDirection).normalized;
        
        return dodgeDir;
    }
}