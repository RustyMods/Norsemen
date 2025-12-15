using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Norsemen;

public partial class Norseman
{
    public static GameObject? tombstone;
    
    public GameObject Prefab = null!;
    public readonly string name;
    private bool Loaded;
    public event Action<Norseman>? OnCreated;
    public Viking m_viking = null!;
    public VikingAI m_ai = null!;
    public readonly Faction faction;
    public List<string> defaultItems = new();
    public readonly List<ConditionalRandomItem> conditionalRandomItems = new();
    public readonly List<ConditionalRandomSet> conditionalRandomSets = new();
    public readonly Heightmap.Biome biome;
    public readonly NorsemanConfigs configs;
    public readonly SpawnManager.SpawnInfo spawnInfo;
    public float baseHealth = 50f;
    public float baseArmor = 0f;
    
    public Norseman(Heightmap.Biome biome, string name, Faction faction)
    {
        this.biome = biome;
        this.name = name;
        this.faction = faction;
        configs = new NorsemanConfigs(name);
        spawnInfo = new SpawnManager.SpawnInfo(this)
        {
            m_spawnInterval = 1000f,
            m_spawnDistance = 50f,
            m_maxLevel = 3,
            m_minAltitude = 10f,
            m_spawnChance = 5f
        };
        PrefabManager.Norsemen.Add(this);
    }
    
    internal void Create()
    {
        if (Loaded) return;
        GameObject prefab = FejdStartup.instance.m_playerPrefab;
        Player player = prefab.GetComponent<Player>();
        Prefab = UnityEngine.Object.Instantiate(prefab, CloneManager.GetRootTransform(), false);
        spawnInfo.m_prefab = Prefab;
        Prefab.name = name;
        Prefab.Remove<Player>();
        Prefab.Remove<PlayerController>();
        Prefab.Remove<Talker>();
        Prefab.Remove<Skills>();
        
        Prefab.GetComponent<ZNetView>().m_persistent = true;
        
        m_viking = Prefab.AddComponent<Viking>();
        m_viking.CopyFieldsFrom(player);
        m_viking.m_eye = Utils.FindChild(Prefab.transform, "EyePos");
        m_viking.m_faction = faction.faction;
        m_viking.m_health = 50f;
        
        if (defaultItems.Count > 0)
        {
            m_viking.m_defaultItems = defaultItems.Select(Helpers.GetPrefab).ToArray();
        }

        if (tombstone != null)
        {
            m_viking.m_tombstone = tombstone;
        }
        
        ragdoll ??= CreateRagdoll();
        if (ragdoll != null)
        {
            m_viking.m_deathEffects.m_effectPrefabs = new[]
            {
                new EffectList.EffectData()
                {
                    m_prefab = ragdoll
                },
                new EffectList.EffectData()
                {
                    m_prefab = Helpers.GetPrefab("vfx_player_death")
                }
            };
        }

        m_viking.m_name = "$enemy_norseman_rs";
        
        m_ai = Prefab.AddComponent<VikingAI>();
        m_ai.m_attackPlayerObjects = false;
        m_ai.m_aggravatable = true;
        m_ai.m_passiveAggresive = true;
        m_ai.m_avoidWater = true;
        m_ai.m_alertedEffects.m_effectPrefabs = new[]
        {
            new EffectList.EffectData()
            {
                m_prefab = Helpers.GetPrefab("sfx_dverger_vo_alerted")
            }
        };
        m_ai.m_idleSound.m_effectPrefabs = new[]
        {
            new EffectList.EffectData()
            {
                m_prefab = Helpers.GetPrefab("sfx_dverger_vo_idle")
            }
        };
        m_ai.m_idleSoundInterval = 20f;
        m_ai.m_idleSoundChance = 0.5f;
        m_ai.m_pathAgentType = Pathfinding.AgentType.HumanoidAvoidWater;
        m_ai.m_moveMinAngle = 90f;
        m_ai.m_smoothMovement = true;
        m_ai.m_randomCircleInterval = 2f;
        m_ai.m_randomMoveInterval = 30f;
        m_ai.m_randomMoveRange = 3f;
        m_ai.m_skipLavaTargets = true;
        m_ai.m_avoidLava = true;
        m_ai.m_avoidLavaFlee = true;
        m_ai.m_fleeRange = 25f;
        m_ai.m_fleeAngle = 45f;
        m_ai.m_fleeInterval = 2f;
        m_ai.m_alertRange = 20f;
        m_ai.m_fleeIfHurtWhenTargetCantBeReached = true;
        m_ai.m_fleeUnreachableSinceAttacking = 30f;
        m_ai.m_fleeUnreachableSinceHurt = 30f;
        m_ai.m_fleeIfLowHealth = 0.2f;
        m_ai.m_fleeTimeSinceHurt = 20f;
        m_ai.m_fleeInLava = true;
        m_ai.m_circulateWhileCharging = true;
        m_ai.m_privateAreaTriggerTreshold = 4;
        m_ai.m_interceptTimeMax = 2f;
        m_ai.m_interceptTimeMin = 0f;
        m_ai.m_maxChaseDistance = 200f;
        m_ai.m_minAttackInterval = 5f;
        m_ai.m_circleTargetInterval = 8f;
        m_ai.m_circleTargetDuration = 6f;
        m_ai.m_circleTargetDistance = 8f;
        m_ai.m_consumeRange = 1f;
        m_ai.m_consumeSearchRange = 10f;
        m_ai.m_consumeSearchInterval = 10f;
        m_ai.m_patrol = true;
        
        SetupConfigs();
        OnCreated?.Invoke(this);
        CloneManager.norsemen[Prefab.name] = Prefab;
        PrefabManager.RegisterPrefab(Prefab);
        Loaded = true;
    }
}