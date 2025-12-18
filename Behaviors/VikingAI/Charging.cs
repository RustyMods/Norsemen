namespace Norsemen;

public partial class VikingAI
{
    public void UpdateChargeAttack(float dt, bool hasItem, bool hasTarget, bool doAttack, ItemDrop.ItemData? itemData)
    {
        bool canCharge = hasItem && hasTarget && doAttack;
        bool notAttacking = !m_character.InAttack();
        bool hasValidAttack = itemData?.m_shared.m_attack != null && 
                              !itemData.m_shared.m_attack.IsDone() &&
                              !string.IsNullOrEmpty(itemData.m_shared.m_attack.m_drawAnimationState);
        
        if ((IsCharging() || canCharge) && notAttacking && hasValidAttack)
        {
            ChargeStart(itemData?.m_shared.m_attack.m_drawAnimationState);
        }
    }
}