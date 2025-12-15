namespace Norsemen;

public partial class VikingAI
{
    public bool UpdateBaseAI(float dt)
    {
        if (!m_nview.IsValid())
        {
            return false;
        }
        
        if (!m_nview.IsOwner())
        {
            m_alerted = m_nview.GetZDO().GetBool(ZDOVars.s_alert);
            return false;
        }

        if (m_jumpInterval > 0.0)
        {
            m_jumpTimer += dt;
        }

        if (m_randomMoveUpdateTimer > 0.0)
        {
            m_randomMoveUpdateTimer -= dt;
        }

        UpdateRegeneration(dt);
        m_timeSinceHurt -= dt;
        return true;
    }
}