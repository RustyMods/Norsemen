namespace Norsemen;

public partial class VikingAI
{
    public bool UpdateDespawn(float dt, bool canSeeTarget)
    {
        if (DespawnInDay() && EnvMan.IsDay() && (m_targetCreature == null || !canSeeTarget))
        {
            MoveAwayAndDespawn(dt, true);
            return true;
        }

        if (IsEventCreature() && !RandEventSystem.HaveActiveEvent())
        {
            SetHuntPlayer(false);
            if (m_targetCreature == null && !IsAlerted())
            {
                MoveAwayAndDespawn(dt, false);
                return true;
            }
        }

        return false;
    }
}