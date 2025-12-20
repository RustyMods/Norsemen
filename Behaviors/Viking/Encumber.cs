namespace Norsemen;

public partial class Viking
{
    public float m_maxCarryWeight = 300f;
    
    public void UpdateEncumber()
    {
        if (!IsTamed()) return;

        if (!IsEncumbered())
        {
            m_seman.RemoveStatusEffect(SEMan.s_statusEffectEncumbered);
        }
        else
        {
            m_seman.AddStatusEffect(SEMan.s_statusEffectEncumbered);
        }
    }

    public override bool IsEncumbered()
    {
        if (!IsTamed()) return false;
        return m_inventory.GetTotalWeight() > GetMaxCarryWeight();
    }

    public float GetMaxCarryWeight()
    {
        float baseValue = m_maxCarryWeight;
        m_seman.ModifyMaxCarryWeight(baseValue, ref baseValue);
        return baseValue;
    }
}