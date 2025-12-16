using UnityEngine;

namespace Norsemen;

public partial class VikingAI
{
    public Player? m_followPlayer;
    public bool UpdateFollow(float dt)
    {
        float distanceFromFollow = Vector3.Distance(transform.position, m_follow.transform.position);
        bool run = distanceFromFollow > 5.0f;

        if (m_followPlayer != null)
        {
            run |= m_followPlayer.IsRunning();
        }

        if (distanceFromFollow < 3.0f)
        {
            return false;
        }

        if (distanceFromFollow > 15f)
        {
            TeleportToFollow();
            return true;
        }
        
        if (IsAlerted())
        {
            return false;
        }

        MoveTo(dt, m_follow.transform.position, 0.0f, run);
        return true;
    }

    public void TeleportToFollow()
    {
        m_viking.TeleportTo(m_follow.transform.position + m_follow.transform.forward * -2f, transform.rotation, false);
    }
}