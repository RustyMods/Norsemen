using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace Norsemen;

public partial class VikingAI
{
    public TreeBase? m_tree;
    
    public bool UpdateLogging(float dt, ItemDrop.ItemData axe, bool isFollowing)
    {
        if (m_tree == null) return false;
        
        if (m_tree.gameObject == null)
        {
            ResetWorkTargets();
            return false;
        }

        if (isFollowing)
        {
            float distance = Vector3.Distance(m_tree.transform.position, transform.position);

            if (distance > 20f)
            {
                ResetWorkTargets();
                return false;
            }
        }
        
        if (!MoveTo(dt, m_tree.transform.position, axe.m_shared.m_attack.m_attackRange, false)) return true;
        StopMoving();
        LookAt(m_tree.transform.position);
        DoChopping(axe, m_tree);
        m_lastWorkActionTime = Time.time;
        m_nview.GetZDO().Set(VikingVars.lastWorkTime, ZNet.instance.GetTime().Ticks);
        return true;
    }
    public void DoChopping(ItemDrop.ItemData axe, TreeBase tree)
    {
        if (m_viking.StartWork(axe))
        {
            if (tree.m_logPrefab != null)
            {
                TreeLog? treeLog = tree.m_logPrefab.GetComponent<TreeLog>();
                if (treeLog != null)
                {
                    if (treeLog.m_subLogPrefab != null)
                    {
                        treeLog = treeLog.m_subLogPrefab.GetComponent<TreeLog>();
                    }
                    List<DropTable.DropData>? resources = treeLog.m_dropWhenDestroyed.m_drops;
                    AddResources(resources);
                }
            }
        }
        if (tree.m_logPrefab != null)
        {
            TreeLog? treeLog = tree.m_logPrefab.GetComponent<TreeLog>();
            if (treeLog != null)
            {
                if (treeLog.m_subLogPrefab != null)
                {
                    treeLog = treeLog.m_subLogPrefab.GetComponent<TreeLog>();
                }
                List<DropTable.DropData>? resources = treeLog.m_dropWhenDestroyed.m_drops;
                AddResources(resources);
            }
        }
    }

    [HarmonyPatch(typeof(TreeBase), nameof(TreeBase.RPC_Damage))]
    private static class TreeBase_RPC_Damage_Patch
    {
        private static void Postfix(TreeBase __instance, HitData hit)
        {
            if (!__instance.m_nview.IsOwner()) return;
            if (hit.GetAttacker() is not Viking) return;
            if (hit.GetTotalDamage() <= 0)
            {
                __instance.Shake();
            }
        }
    }
}