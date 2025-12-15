using UnityEngine;

namespace Norsemen;

public partial class Viking
{
    public bool m_crouchToggled;
    public float m_crouchTimer;
    private float m_crouchCheckInterval = 2f;

    public override void SetCrouch(bool crouch)
    {
        if (m_crouchToggled != crouch)
        {
            m_crouchToggled = crouch;
            m_crouchTimer = 0f;
        }
    }

    public void UpdateCrouch(float dt)
    {
        m_crouchTimer += dt;
        
        if (m_crouchTimer > m_crouchCheckInterval)
        {
            UpdateCrouchDecision();
            m_crouchTimer = 0f;
        }
        
        bool shouldCrouch = m_crouchToggled && CanCrouch();
        m_zanim.SetBool(Player.s_crouching, shouldCrouch);
    }

    private void UpdateCrouchDecision()
    {
        Character? target = m_vikingAI.GetTargetCreature();
        
        if (target == null)
        {
            SetCrouch(false);
            return;
        }
        
        if (!m_vikingAI.IsAlerted())
        {
            SetCrouch(false);
            return;
        }
        
        float distance = Vector3.Distance(target.transform.position, transform.position);
        
        bool canSeeTarget = m_vikingAI.CanSeeTarget(target);
        
        if (canSeeTarget)
        {
            if (distance > 20f && distance < 40f)
            {
                SetCrouch(UnityEngine.Random.value < 0.7f);
            }
            else if (distance >= 40f)
            {
                SetCrouch(UnityEngine.Random.value < 0.8f);
            }
            else
            {
                SetCrouch(UnityEngine.Random.value < 0.2f);
            }
        }
        else
        {
            SetCrouch(UnityEngine.Random.value < 0.6f);
        }
    }

    private bool CanCrouch()
    {
        if (IsSwimming()) return false;
        if (IsRunning()) return false;
        if (IsBlocking()) return false;
        if (InAttack()) return false;
        if (IsDrawingBow()) return false;
        if (IsDead()) return false;
        if (IsStaggering()) return false;
        return true;
    }
}