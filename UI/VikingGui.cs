using HarmonyLib;
using UnityEngine;

namespace Norsemen;

public static partial class VikingGui
{
    private static readonly int visible = Animator.StringToHash("visible");
    public static Viking? m_currentViking;

    public static void Show(this InventoryGui gui, Viking viking)
    {
        m_currentViking = viking;
        gui.Show(null);
        
        string name = viking.GetName();
        string tooltip = viking.GetTooltip();
        
        armor.Show(viking.GetArmor().ToString("0"));
        armor.tooltip.Set(name, tooltip);
        health.Show($"{viking.GetHealth():0}/{viking.GetMaxHealth():0}");
        health.tooltip.Set(name, tooltip);

        NorseGui.instance?.Show();
    }
    
    public static void CloseVikingInventory()
    {
        m_currentViking?.SetInUse(null);
        m_currentViking = null;
    }

    [HarmonyPatch(typeof(Player), nameof(Player.SetLocalPlayer))]
    private static class Player_SetLocalPlayer
    {
        private static void Postfix()
        {
            m_currentViking = null;
        }
    }

    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.OnSelectedItem))]
    private static class InventoryGui_OnSelectedItem
    {
        private static bool Prefix(InventoryGui __instance, InventoryGrid grid, ItemDrop.ItemData? item,
            InventoryGrid.Modifier mod)
        {
            if (!Player.m_localPlayer || item == null || item.m_shared.m_questItem || mod is not InventoryGrid.Modifier.Move || __instance.m_dragGo || __instance.m_currentContainer != null || m_currentViking == null) return true;

            if (Player.m_localPlayer.IsTeleporting()) return false;

            Player.m_localPlayer.RemoveEquipAction(item);
            Player.m_localPlayer.UnequipItem(item);
            
            Inventory playerInventory = Player.m_localPlayer.GetInventory();
            Inventory vikingInventory = m_currentViking.GetInventory();
            
            if (grid.GetInventory() == vikingInventory)
            {
                playerInventory.MoveItemToThis(vikingInventory, item);
            }
            else
            {
                vikingInventory.MoveItemToThis(playerInventory, item);
            }

            __instance.m_moveItemEffects.Create(__instance.transform.position, Quaternion.identity);
            return false;
        }
    }

    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.UpdateContainer))]
    private static class InventoryGUI_UpdateContainer
    {
        private static bool Prefix(InventoryGui __instance, Player player)
        {
            if (!__instance.m_animator.GetBool(visible) || __instance.m_currentContainer != null || m_currentViking == null)
            {
                NorseGui.instance?.Hide();
                return true;
            }

            m_currentViking.SetInUse(player);
            __instance.m_container.gameObject.SetActive(true);
            __instance.m_containerGrid.UpdateInventory(m_currentViking.m_inventory, null, __instance.m_dragItem);
            __instance.m_containerName.text = m_currentViking.GetName();

            if (__instance.m_firstContainerUpdate)
            {
                __instance.m_containerGrid.ResetView();
                __instance.m_firstContainerUpdate = false;
                __instance.m_containerHoldTime = 0.0f;
                __instance.m_containerHoldState = 0;
            }
            
            float distance = Vector3.Distance(m_currentViking.transform.position, player.transform.position);

            if (distance > __instance.m_autoCloseDistance)
            {
                CloseVikingInventory();
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.OnTakeAll))]
    private static class InventoryGui_OnTakeAll
    {
        private static bool Prefix(InventoryGui __instance)
        {
            if (Player.m_localPlayer.IsTeleporting() || __instance.m_currentContainer != null ||
                m_currentViking == null) return true;
            
            __instance.SetupDragItem(null, null, 1);
            Inventory inventory = m_currentViking.GetInventory();
            Player.m_localPlayer.GetInventory().MoveAll(inventory);

            return false;
        }
    }

    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.OnStackAll))]
    private static class InventoryGui_OnStackAll
    {
        private static bool Prefix(InventoryGui __instance)
        {
            if (Player.m_localPlayer.IsTeleporting() || __instance.m_currentContainer != null || m_currentViking == null) return true;

            __instance.SetupDragItem(null, null, 1);
            m_currentViking.GetInventory().StackAll(Player.m_localPlayer.GetInventory());
            
            return false;
        }
    }

    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.Hide))]
    private static class InventoryGui_Hide
    {
        private static void Postfix()
        {
            CloseVikingInventory();
            armor.Hide();
            health.Hide();
            NorseGui.instance?.Hide();
        }
    }

    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.IsContainerOpen))]
    private static class InventoryGui_IsContainerOpen
    {
        private static void Postfix(ref bool __result)
        {
            __result |= m_currentViking != null;
        }
    }

    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.UpdateContainerWeight))]
    private static class InventoryGui_UpdateContainerWeight
    {
        private static void Postfix(InventoryGui __instance)
        {
            if (__instance.m_currentContainer != null || m_currentViking == null) return;
            int totalWeight = Mathf.CeilToInt(m_currentViking.GetInventory().GetTotalWeight());
            __instance.m_containerWeight.text = totalWeight.ToString();
        }
    }
}