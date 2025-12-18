using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Norsemen;

public partial class Norseman
{
    private static GameObject? elfEars = AssetBundleManager.LoadAsset<GameObject>("norsemen_bundle", "ElvenEars");
    public static GameObject? tombstone;
    
    public GameObject Prefab = null!;
    public readonly string name;
    private bool Loaded;
    public event Action<Norseman>? OnCreated;
    public Viking m_viking = null!;
    public VikingAI m_ai = null!;
    public readonly Faction faction;
    public List<string> defaultItems = new();
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
        configs.biome = biome;
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
        m_ai.m_randomMoveInterval = 15f;
        m_ai.m_randomMoveRange = 20f;
        m_ai.m_skipLavaTargets = true;
        m_ai.m_avoidLava = true;
        m_ai.m_avoidLavaFlee = true;
        m_ai.m_avoidFire = true;
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
        m_ai.m_minAttackInterval = 1.5f;
        m_ai.m_circleTargetInterval = 8f;
        m_ai.m_circleTargetDuration = 6f;
        m_ai.m_circleTargetDistance = 8f;
        m_ai.m_consumeRange = 1f;
        m_ai.m_consumeSearchRange = 10f;
        m_ai.m_consumeSearchInterval = 10f;
        m_ai.m_patrol = true;

        if (elfEars != null)
        {
            GameObject attach_skin = elfEars.transform.Find("attach_skin").gameObject;
            GameObject? instance = UnityEngine.Object.Instantiate(attach_skin);
            instance.name = "elf_ears";
            VisEquipment.CleanupInstance(instance);
            VisEquipment? visEq = Prefab.GetComponent<VisEquipment>();
            SkinnedMeshRenderer? body = visEq.m_bodyModel;
            
            instance.transform.SetParent(body.transform.parent);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;

            List<Material> earMats = new();
            
            foreach (SkinnedMeshRenderer? renderer in instance.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                renderer.rootBone = body.rootBone;
                renderer.bones = body.bones;
                if (renderer.name.CustomStartsWith("007"))
                {
                    earMats.Add(renderer.materials);
                }
            }

            m_viking.m_elfEars = instance;
            m_viking.m_elfEarMats = earMats.ToArray();
        }
        
        SetupConfigs();
        OnCreated?.Invoke(this);
        CloneManager.norsemen[Prefab.name] = Prefab;
        PrefabManager.RegisterPrefab(Prefab);
        Loaded = true;
    }
}