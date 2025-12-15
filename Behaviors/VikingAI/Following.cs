namespace Norsemen;

public partial class VikingAI
{
    public bool UpdateFollowOrIdle(float dt, bool hasTarget, bool hasItem)
    {
        if (!hasTarget || !hasItem)
        {
            if (m_follow)
            {
                Follow(m_follow, dt);
            }
            else
            {
                IdleMovement(dt);
            }
            ChargeStop();
            return true;
        }

        return false;
    }
}