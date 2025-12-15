using System;
using HarmonyLib;
using UnityEngine;

namespace Norsemen;

public partial class Viking
{
    public ItemDrop.ItemData? m_weaponLoaded;

    public EffectList m_dodgeEffects = new();

    public override bool StartAttack(Character? target, bool secondaryAttack)
    {
        if (InAttack() && !HaveQueuedChain() || InDodge() || !CanMove() || IsKnockedBack() || IsStaggering() || InMinorAction()) return false;
    
        ItemDrop.ItemData currentWeapon = GetCurrentWeapon();
        if (currentWeapon == null || (!currentWeapon.HaveSecondaryAttack() && !currentWeapon.HavePrimaryAttack())) return false;

        bool secondary = currentWeapon.HaveSecondaryAttack() && UnityEngine.Random.value > 0.5;
        if (currentWeapon.m_shared.m_skillType is Skills.SkillType.Spears) secondary = false;
        
        if (m_currentAttack != null)
        {
            m_currentAttack.Stop();
            m_previousAttack = m_currentAttack;
            m_currentAttack = null;
        }
        Attack? attack = !secondary ? currentWeapon.m_shared.m_attack.Clone() : currentWeapon.m_shared.m_secondaryAttack.Clone();
        if (!attack.Start(this, m_body, m_zanim, m_animEvent, m_visEquipment, currentWeapon, m_previousAttack,
                m_timeSinceLastAttack, UnityEngine.Random.Range(0.5f, 1f))) return false;

        if (currentWeapon.m_shared.m_attack.m_requiresReload) SetWeaponLoaded(null);
        if (currentWeapon.m_shared.m_attack.m_bowDraw) currentWeapon.m_shared.m_attack.m_attackDrawPercentage = 0.0f;
        if (currentWeapon.m_shared.m_itemType is not ItemDrop.ItemData.ItemType.Torch) currentWeapon.m_durability -= 1.5f;
        
        ClearActionQueue();
        StartAttackGroundCheck();
        m_currentAttack = attack;
        m_currentAttackIsSecondary = secondary;
        m_lastCombatTimer = 0.0f;
        if (currentWeapon.m_shared.m_name == "$item_stafficeshards")
        {
            Invoke(nameof(StopCurrentAttack), 5f);
        }
        return true;
    }
    
    private void StopCurrentAttack()
    {
        if (m_currentAttack == null) return;
        m_currentAttack.Stop();
        m_previousAttack = m_currentAttack;
        m_currentAttack = null;
    }
    
    private void SetWeaponLoaded(ItemDrop.ItemData? weapon)
    {
        if (weapon == m_weaponLoaded) return;
        m_weaponLoaded = weapon;
        m_nview.GetZDO().Set(ZDOVars.s_weaponLoaded, weapon != null);
    }

    [HarmonyPatch(typeof(Attack), nameof(Attack.HaveAmmo))]
    private static class Attack_HaveAmmo
    {
        private static void Postfix(Humanoid character, ref bool __result)
        {
            if (__result || character is not Viking) return;
            __result = true;
        }
    }

    [HarmonyPatch(typeof(Attack), nameof(Attack.FindAmmo))]
    private static class Attack_FindAmmo_Patch
    {
        private static void Postfix(Humanoid character, ItemDrop.ItemData weapon, ref ItemDrop.ItemData? __result)
        {
            if (__result != null || character is not Viking viking) return;

            if (string.IsNullOrEmpty(weapon.m_shared.m_ammoType)) return;

            switch (weapon.m_shared.m_ammoType)
            {
                case "$ammo_arrows":
                    ItemDrop.ItemData? arrow = viking.GetInventory().AddItem("ArrowWood", 10, 1, 0, 0L, "");
                    if (arrow != null)
                    {
                        __result = arrow;
                    }
                    break;
                case "$ammo_bolts":
                    var bolt = viking.GetInventory().AddItem("BoltBone", 10, 1, 0, 0L, "");
                    if (bolt != null)
                    {
                        __result = bolt;
                    }
                    break;
            }
        }
    }
}