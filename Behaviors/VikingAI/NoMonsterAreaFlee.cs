namespace Norsemen;

public partial class VikingAI
{
    public bool UpdateNoMonsterArea(float dt)
    {
        if (!m_character.IsTamed())
        {
            if (m_targetCreature != null)
            {
                if (EffectArea.IsPointInsideNoMonsterArea(m_targetCreature.transform.position))
                {
                    Flee(dt, m_targetCreature.transform.position);
                    return true;
                }
            }
            else
            {
                var noMonsterArea = EffectArea.IsPointCloseToNoMonsterArea(transform.position);
                if (noMonsterArea != null)
                {
                    Flee(dt, noMonsterArea.transform.position);
                    return true;
                }
            }
        }

        return false;
    }
}