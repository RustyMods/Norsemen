using UnityEngine;

namespace Norsemen;

public partial class VikingAI
{
    public bool UpdateCircleTarget(float dt)
    {
        if (m_circleTargetInterval > 0.0 && m_targetCreature)
        {
            m_pauseTimer += dt;
            if (m_pauseTimer > (double)m_circleTargetInterval)
            {
                if (m_pauseTimer > m_circleTargetInterval + (double)m_circleTargetDuration) m_pauseTimer = Random.Range(0.0f, m_circleTargetInterval / 10f);
                RandomMovementArroundPoint(dt, m_targetCreature.transform.position, m_circleTargetDistance, IsAlerted());
                return true;
            }
        }

        return false;
    }
}