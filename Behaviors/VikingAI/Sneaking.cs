using UnityEngine;

namespace Norsemen;

public partial class VikingAI
{
    public float m_crouchTimer;
    private float m_crouchCheckInterval = 2f;
    
    public void UpdateCrouch(float dt, Character? target, bool canSeeTarget)
    {
        m_crouchTimer += dt;
        
        if (m_crouchTimer > m_crouchCheckInterval)
        {
            UpdateCrouchDecision(target, canSeeTarget);
            m_crouchTimer = 0f;
        }
        
        bool shouldCrouch = m_viking.m_crouchToggled && m_viking.CanCrouch();
        m_animator.SetBool(Player.s_crouching, shouldCrouch);
    }

    private void UpdateCrouchDecision(Character? target, bool canSeeTarget)
    {
        if (target == null)
        {
            m_viking.SetCrouch(false);
            return;
        }
        
        if (!IsAlerted())
        {
            m_viking.SetCrouch(false);
            return;
        }
        
        float distance = Vector3.Distance(target.transform.position, transform.position);
        
        if (canSeeTarget)
        {
            if (distance > 20f && distance < 40f)
            {
                m_viking.SetCrouch(UnityEngine.Random.value < 0.7f);
            }
            else if (distance >= 40f)
            {
                m_viking.SetCrouch(UnityEngine.Random.value < 0.8f);
            }
            else
            {
                m_viking.SetCrouch(UnityEngine.Random.value < 0.2f);
            }
        }
        else
        {
            m_viking.SetCrouch(UnityEngine.Random.value < 0.6f);
        }
    }
}