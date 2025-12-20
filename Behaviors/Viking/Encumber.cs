using System;

namespace Norsemen;

public partial class Viking
{
    public void UpdateEncumber()
    {
        if (!IsTamed()) return;
        
        if (!IsEncumbered())
        {
            m_seman.RemoveStatusEffect(SEMan.s_statusEffectEncumbered);
        }
        else
        {
            if (!NorsemenPlugin.CanBecomeEncumbered) return;
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
        float baseValue = NorsemenPlugin.BaseCarryWeight;
        float levelAddedValue = Math.Max(m_level - 1, 0) * 50;
        baseValue += levelAddedValue;
        m_seman.ModifyMaxCarryWeight(baseValue, ref baseValue);
        return baseValue;
    }
}