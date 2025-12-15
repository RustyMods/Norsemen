using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Norsemen;

public partial class VikingAI
{
    private float m_workTargetSearchTimer;
    private float m_workTargetSearchInterval = 30f;

    private bool hasWorkTarget;

    public void OnWorkConfigChanged(object sender, EventArgs args)
    {
        ResetWorkTargets();
    }

    public void ResetWorkTargets()
    {
        m_mineRock = null;
        m_mineRock5 = null;
        m_destructible = null;
        m_tree = null;
        m_lastMineRock5Point = Vector3.zero;
        m_fish = null;
        m_bait = null;
        hasWorkTarget = false;
    }
    
    public void FindWorkTargets(float dt)
    {
        if (hasWorkTarget || m_viking.IsInUse()) return;
        
        m_workTargetSearchTimer += dt;
        if (m_workTargetSearchTimer < m_workTargetSearchInterval) return;
        m_workTargetSearchTimer = 0.0f;

        bool canMine = m_viking.configs.CanMine;
        bool canLumber = m_viking.configs.CanLumber;
        bool canFish = m_viking.configs.CanFish;
        bool canWork = m_viking.configs.RequireFood || !m_viking.IsHungry();
        bool shouldSearch = canWork && (canMine || canLumber || canFish);
        
        // Reset targets
        ResetWorkTargets();
        
        if (!shouldSearch) return;
        
        if (m_viking.m_pickaxe == null && m_viking.m_axe == null && m_viking.m_fishingRod == null) return;
        
        float mineRockDistance = float.MaxValue;
        float mineRock5Distance = float.MaxValue;
        float treeDistance = float.MaxValue;
        float destructibleDistance = float.MaxValue;
        float fishDistance = float.MaxValue;
        
        MineRock? selectedMineRock = null;
        MineRock5? selectedMineRock5 = null;
        TreeBase? selectedTree = null;
        Destructible? selectedDestructible = null;
        Fish? selectedFish = null;

        List<ZNetView> prefabs = ZNetScene.instance.m_instances.Values.ToList();
        
        for (int i = 0; i < prefabs.Count; ++i)
        {
            ZNetView? prefab = prefabs[i];
            float distance = Vector3.Distance(transform.position, prefab.transform.position);
            if (distance > 50f) continue;
            
            if (m_viking.m_pickaxe != null && canMine)
            {
                MineRock? mineRock = prefab.GetComponent<MineRock>();
                if (mineRock != null)
                {
                    if (m_viking.m_pickaxe.m_shared.m_toolTier < mineRock.m_minToolTier) continue;
                    if (!mineRock.m_dropItems.m_drops.IsOreVein()) continue;
                    if (distance < mineRockDistance)
                    {
                        mineRockDistance = distance;
                        selectedMineRock = mineRock;
                    }

                    continue;
                }
                
                MineRock5? mineRock5 = prefab.GetComponent<MineRock5>();
                if (mineRock5 != null)
                {
                    if (m_viking.m_pickaxe.m_shared.m_toolTier < mineRock5.m_minToolTier) continue;
                    if (!mineRock5.m_dropItems.m_drops.IsOreVein()) continue;
                    if (distance < mineRock5Distance)
                    {
                        mineRock5Distance = distance;
                        selectedMineRock5 = mineRock5;
                    }

                    continue;
                }

                Destructible? destructible = prefab.GetComponent<Destructible>();
                if (destructible != null)
                {
                    if (m_viking.m_pickaxe.m_shared.m_toolTier < destructible.m_minToolTier) continue;
                    
                    if (destructible.m_spawnWhenDestroyed != null)
                    {
                        mineRock5 = destructible.m_spawnWhenDestroyed.GetComponent<MineRock5>();
                        if (!mineRock5.m_dropItems.m_drops.IsOreVein()) continue;
                        if (mineRock5 != null)
                        {
                            if (distance < destructibleDistance)
                            {
                                selectedDestructible = destructible;
                                destructibleDistance = distance;
                            }

                            continue;
                        }
                    }
                    
                    DropOnDestroyed? dropOnDestroyed = destructible.GetComponent<DropOnDestroyed>();
                    if (dropOnDestroyed != null)
                    {
                        if (!dropOnDestroyed.m_dropWhenDestroyed.m_drops.IsOreVein()) continue;
                        if (distance < destructibleDistance)
                        {
                            selectedDestructible = destructible;
                            destructibleDistance = distance;
                        }

                        continue;
                    }
                }
            }
            
            // Check for Tree
            if (m_viking.m_axe != null && canLumber)
            {
                TreeBase? tree = prefab.GetComponent<TreeBase>();
                if (tree != null)
                {
                    if (m_viking.m_axe.m_shared.m_toolTier < tree.m_minToolTier) continue;
                    if (distance < treeDistance)
                    {
                        treeDistance = distance;
                        selectedTree = tree;
                    }

                    continue;
                }
            }
            
            // Check for fish
            if (m_viking.m_fishingRod != null && canFish)
            {
                Fish? fish = prefab.GetComponent<Fish>();
                if (fish != null)
                {
                    bool hasBait = false;
                    foreach (Fish.BaitSetting? bait in fish.m_baits)
                    {
                        if (m_viking.GetInventory().HaveItem(bait.m_bait.m_itemData.m_shared.m_name))
                        {
                            hasBait = true;
                            m_bait = m_viking.GetInventory().GetItem(bait.m_bait.m_itemData.m_shared.m_name);
                            break;
                        }
                    }
                    
                    if (!hasBait) continue;

                    if (distance < fishDistance)
                    {
                        fishDistance = distance;
                        selectedFish = fish;
                    }
                }
            }
        }
        
        // Keep only the single nearest target
        float nearest = Mathf.Min(mineRockDistance, mineRock5Distance, treeDistance, destructibleDistance, fishDistance);
        
        if (Math.Abs(nearest - mineRockDistance) < 1f && selectedMineRock != null)
        {
            m_mineRock = selectedMineRock;
            hasWorkTarget = true;
            m_viking.EquipItem(m_viking.m_pickaxe);
            NorsemenPlugin.LogDebug($"[{m_viking.GetName()}] found an ore deposit: {m_mineRock.name}");
        }
        else if (Math.Abs(nearest - mineRock5Distance) < 1f && selectedMineRock5 != null)
        {
            m_mineRock5 = selectedMineRock5;
            hasWorkTarget = true;
            m_viking.EquipItem(m_viking.m_pickaxe);
            NorsemenPlugin.LogDebug($"[{m_viking.GetName()}] found an ore deposit: {m_mineRock5.name}");
        }
        else if (Math.Abs(nearest - destructibleDistance) < 1f && selectedDestructible != null)
        {
            m_destructible = selectedDestructible;
            hasWorkTarget = true;
            m_viking.EquipItem(m_viking.m_pickaxe);
            NorsemenPlugin.LogDebug($"[{m_viking.GetName()}] found an ore deposit: {m_destructible.name}");
        }
        else if (Math.Abs(nearest - treeDistance) < 1f && selectedTree != null)
        {
            m_tree = selectedTree;
            hasWorkTarget = true;
            m_viking.EquipItem(m_viking.m_axe);
            NorsemenPlugin.LogDebug($"[{m_viking.GetName()}] found a tree: {m_tree.name}");
        }
        else if (Math.Abs(nearest - fishDistance) < 1f && selectedFish != null)
        {
            m_fish = selectedFish;
            hasWorkTarget = true;
            m_viking.EquipItem(m_viking.m_fishingRod);
            NorsemenPlugin.LogDebug($"[{m_viking.GetName()}] found a fish: {m_fish.name}");
        }
    }
}