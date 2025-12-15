using System;

namespace Norsemen;

public partial class Viking
{
    public float armor;
    
    public float GetArmor()
    {
        armor = configs.BaseArmor;
        if (m_chestItem != null) armor += m_chestItem.GetArmor();
        if (m_legItem != null) armor += m_legItem.GetArmor();
        if (m_helmetItem != null) armor += m_helmetItem.GetArmor();
        if (m_shoulderItem != null) armor += m_shoulderItem.GetArmor();
        m_seman.ApplyArmorMods(ref armor);
        return armor;
    }

    public override float GetBodyArmor() => armor;
}