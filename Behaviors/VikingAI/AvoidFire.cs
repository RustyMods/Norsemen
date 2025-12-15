namespace Norsemen;

public partial class VikingAI
{
    public bool UpdateAvoidFire(float dt)
    {
        bool shouldAvoidFire = m_afraidOfFire || m_avoidFire;

        if (shouldAvoidFire && AvoidFire(dt, m_targetCreature, m_afraidOfFire))
        {
            if (m_afraidOfFire)
            {
                ResetWorkTargets();
            }
            return true;
        }

        return false;
    }
}