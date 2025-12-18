namespace Norsemen;

public partial class VikingAI
{
    public bool UpdateInventory(bool isTamed)
    {
        bool inUse = m_viking.IsInUse();

        if (!inUse) return false;

        if (isTamed)
        {
            if (m_viking.m_currentPlayer)
            {
                LookAt(m_viking.m_currentPlayer.GetTopPoint());
            }
            StopMoving();
            return true;
        }

        if (m_viking.m_currentPlayer)
        {
            float roll = UnityEngine.Random.value;
            float chance = 0.5f;

            if (roll < chance)
            {
                LookAt(m_viking.m_currentPlayer.GetEyePoint());
            }
            
            bool canSeeThief = CanSeeTarget(m_viking.m_currentPlayer);
            if (canSeeThief)
            {
                SetAggravated(true, AggravatedReason.Theif);
            }
        }

        return false;
    }
}