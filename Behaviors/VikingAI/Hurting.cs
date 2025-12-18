namespace Norsemen;

public  partial class VikingAI
{
    public bool UpdateHurt(float dt, bool isTamed)
    {
        if (isTamed && m_moveType is Movement.Guard)
        {
            return false;
        }
        
        bool shouldFlee = m_timeSinceAttacking > 30.0 && m_timeSinceHurt < 20.0;
        if (m_fleeIfHurtWhenTargetCantBeReached && m_targetCreature != null && shouldFlee)
        {
            Flee(dt, m_targetCreature.transform.position);
            m_lastKnownTargetPos = transform.position;
            m_updateTargetTimer = 1f;
            return true;
        }

        return false;
    }
}