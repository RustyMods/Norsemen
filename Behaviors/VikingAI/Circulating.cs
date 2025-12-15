namespace Norsemen;

public partial class VikingAI
{
    public bool UpdateCirculate(float dt, bool hasItem, bool doAttack)
    {
        bool shouldCirculate = m_character.IsFlying() ? m_circulateWhileChargingFlying : m_circulateWhileCharging;

        if (shouldCirculate && hasItem && !doAttack && !m_character.InAttack())
        {
            if (m_targetCreature != null)
            {
                RandomMovementArroundPoint(dt, m_targetCreature.transform.position, m_randomMoveRange, IsAlerted());
                return true;
            }
            if (m_targetStatic != null)
            {
                RandomMovementArroundPoint(dt, m_targetStatic.transform.position, m_randomMoveRange, IsAlerted());
                return true;
            }
        }

        return false;
    }
}