using UnityEngine;

namespace Norsemen;

public partial class Viking
{
    private float m_dodgeTimer = 0f;
    private float m_dodgeCooldown = 5f;  
    private float m_dodgeCheckInterval = 0.2f;  
    private float m_dodgeCheckTimer = 0f;
    private float m_dodgeDistance = 15f; 
    private float m_dodgeThreatDistance = 5f;

    public void Dodge(Vector3 dir)
    {
        transform.rotation = Quaternion.LookRotation(dir);
        m_body.rotation = transform.rotation;
        m_zanim.SetTrigger("dodge");
        AddNoise(5f);
        m_dodgeEffects.Create(transform.position, Quaternion.identity, transform);
        m_dodgeTimer = m_dodgeCooldown;
    }

    public void UpdateDodge(float dt)
    {
        if (m_dodgeTimer > 0f)
        {
            m_dodgeTimer -= dt;
        }
        
        m_dodgeCheckTimer -= dt;
        if (m_dodgeCheckTimer > 0f) return;
        m_dodgeCheckTimer = m_dodgeCheckInterval;
        
        if (!m_vikingAI.IsAlerted()) return;
        if (IsBlocking() || IsRunning() || IsSwimming()) return;
        if (m_dodgeTimer > 0f) return; 
        
        Character? target = m_vikingAI.GetTargetCreature();
        if (target == null) return;
        
        float distance = Vector3.Distance(target.transform.position, transform.position);
        if (distance > m_dodgeDistance) return;
        
        
        bool shouldDodge = ShouldDodgeTarget(target, distance);
        
        if (shouldDodge)
        {
            Vector3 toTarget = target.transform.position - transform.position;
            Vector3 dodgeDir = CalculateDodgeDirection(toTarget.normalized, distance);
            Dodge(dodgeDir);
        }
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