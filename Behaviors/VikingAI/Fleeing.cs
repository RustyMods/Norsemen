using UnityEngine;

namespace Norsemen;

public partial class VikingAI
{
    public bool UpdateFlee(float dt)
    {
        if (m_fleeIfNotAlerted && !HuntPlayer() && m_targetCreature && !IsAlerted() && Vector3.Distance(m_targetCreature.transform.position, transform.position) - m_targetCreature.GetRadius() > m_alertRange)
        {
            Flee(dt, m_targetCreature.transform.position);
            return true;
        }
        
        if (m_fleeIfLowHealth > 0.0 && m_timeSinceHurt < m_fleeTimeSinceHurt && m_targetCreature != null &&
            m_character.GetHealthPercentage() < (double)m_fleeIfLowHealth)
        {
            Flee(dt, m_targetCreature.transform.position);
            return true;
        }
        
        if (m_fleeInLava && m_character.InLava() && (m_targetCreature == null || m_targetCreature.AboveOrInLava()))
        {
            Flee(dt, m_character.transform.position - m_character.transform.forward);
            return true;
        }

        return false;
    }
}