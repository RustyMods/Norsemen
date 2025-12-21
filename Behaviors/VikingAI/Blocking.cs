using UnityEngine;

namespace Norsemen;

public partial class VikingAI
{
    public float m_blockTimer;
    private float m_blockCheckInterval = 2f;
    
    public void UpdateBlock(float dt, Character? target, bool canSeeTarget)
    {
        m_blockTimer += dt;
        if (m_blockTimer > m_blockCheckInterval)
        {
            m_blockTimer = 0.0f;
            UpdateBlockDecision(target, canSeeTarget);
        }
    }

    public void UpdateBlockDecision(Character? target, bool canSeeTarget)
    {
        if (!IsAlerted())
        {
            m_viking.SetBlocking(false);
            return;
        }
        
        if (target == null)
        {
            m_viking.SetBlocking(false);
            return;
        }

        if (m_viking.GetHealthPercentage() < 0.25f)
        {
            m_viking.SetBlocking(true);
            return;
        }
        
        if (target.IsFlying())
        {
            m_viking.SetBlocking(false);
            return;
        }
        
        float distance = Vector3.Distance(target.transform.position, transform.position);
        bool isAlerted = target.m_baseAI.IsAlerted() || target.IsPlayer();
        
        if (canSeeTarget)
        {
            if (distance < 10f)
            {
                if (isAlerted)
                {
                    m_viking.SetBlocking(UnityEngine.Random.value > 0.8f);
                }
                else
                {
                    m_viking.SetBlocking(UnityEngine.Random.value > 0.5f);
                }
            }
            else
            {
                bool isCharging = target.m_baseAI.IsCharging() || target.IsDrawingBow();
                if (isCharging)
                {
                    m_viking.SetBlocking(UnityEngine.Random.value > 0.6f);
                }
                else
                {
                    m_viking.SetBlocking(UnityEngine.Random.value > 0.25f);
                }
            }
        }
        else
        {
            m_viking.SetBlocking(false);
        }
    }
}