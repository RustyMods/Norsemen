namespace Norsemen;

public partial class Viking
{
    public override void ApplyArmorDamageMods(ref HitData.DamageModifiers mods)
    {
        if (m_chestItem != null)
        {
            mods.Apply(m_chestItem.m_shared.m_damageModifiers);
        }

        if (m_legItem != null)
        {
            mods.Apply(m_legItem.m_shared.m_damageModifiers);
        }

        if (m_helmetItem != null)
        {
            mods.Apply(m_helmetItem.m_shared.m_damageModifiers);
        }

        if (m_shoulderItem != null)
        {
            mods.Apply(m_shoulderItem.m_shared.m_damageModifiers);
        }
    }
}