using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using HarmonyLib;

namespace Norsemen;

public static class RaidManager
{
    internal static readonly List<Raid> raids;
    
    static RaidManager()
    {
        raids = new();
        Harmony harmony = NorsemenPlugin.instance._harmony;
        harmony.Patch(AccessTools.Method(typeof(RandEventSystem), nameof(RandEventSystem.Awake)),
            postfix: new HarmonyMethod(typeof(RaidManager), nameof(Patch_RandEvenSystem)));
        harmony.Patch(AccessTools.Method(typeof(MonsterAI), nameof(MonsterAI.SetEventCreature)),
            postfix: new HarmonyMethod(typeof(RaidManager), nameof(Patch_MonsterAI_SetEventCreature)));
    }

    private static void Patch_RandEvenSystem(RandEventSystem __instance)
    {
        __instance.m_events.AddRange(raids);
    }

    private static void Patch_MonsterAI_SetEventCreature(MonsterAI __instance, bool despawn)
    {
        if (!despawn || __instance is not VikingAI vikingAI) return;
        vikingAI.SetAggravated(true, BaseAI.AggravatedReason.Theif);
        vikingAI.m_attackPlayerObjects = true;
    }
}

public class Raid : RandomEvent
{
    private ConfigEntry<float> duration;
    private ConfigEntry<Toggle> baseOnly;
    private ConfigEntry<Toggle> pauseNoPlayers;
    private ConfigEntry<float> range;
    private ConfigEntry<Toggle> enabled;
    private ConfigEntry<string> requiredKeys;
    private ConfigEntry<string> notRequiredKeys;

    private readonly Norseman viking;
    
    public Raid(Norseman viking)
    {
        this.viking = viking;
        SpawnSystem.SpawnData info = viking.spawnInfo.Clone();
        info.m_enabled = true;
        info.m_canSpawnCloseToPlayer = true;
        info.m_spawnInterval = 10f;
        info.m_huntPlayer = true;
        info.m_spawnAtDay = true;
        info.m_spawnAtNight = true;
        info.m_spawnChance = 100f;
        info.m_spawnDistance = 15f;
        info.m_maxSpawned = 10;
        info.m_groupSizeMax = 3;
        info.m_groupRadius = 3f;
        info.m_requiredEnvironments.Clear();
        
        m_spawn.Add(info);
        RaidManager.raids.Add(this);
    }

    public void SetupConfigs(params string[] keys)
    {
        enabled = ConfigManager.config(viking.name, "Raid Enabled", m_enabled ? Toggle.On : Toggle.Off,
            $"If on, {viking.name} raids are enabled");
        duration = ConfigManager.config(viking.name, "Raid Duration", m_duration,
            $"Set raid duration for {viking.name}");
        baseOnly = ConfigManager.config(viking.name, "Raid Base Only", m_nearBaseOnly ? Toggle.On : Toggle.Off,
            $"If on, {viking.name} raids are near base only");
        pauseNoPlayers = ConfigManager.config(viking.name, "Raid Pause No Players", m_pauseIfNoPlayerInArea ? Toggle.On : Toggle.Off,
            $"If on, {viking.name} raids pause if no players are in area.");
        range = ConfigManager.config(viking.name, "Raid Range", m_eventRange, $"Set {viking.name} event radius");
        requiredKeys = ConfigManager.config(viking.name, "Raid Required Keys", new StringList(keys).ToString(),
            new ConfigDescription($"List of required global keys for {viking.name} raid", null, StringList.attributes));
        notRequiredKeys = ConfigManager.config(viking.name, "Raid Not Required Keys", "",
            new ConfigDescription($"List of required keys to be false for {viking.name} raid", null,
                StringList.attributes));
        
        enabled.SettingChanged += OnConfigChange;
        duration.SettingChanged += OnConfigChange;
        baseOnly.SettingChanged += OnConfigChange;
        range.SettingChanged += OnConfigChange;
        pauseNoPlayers.SettingChanged += OnConfigChange;
        requiredKeys.SettingChanged += OnConfigChange;
        notRequiredKeys.SettingChanged += OnConfigChange;
        
        OnConfigChange();
    }

    private void OnConfigChange(object sender, EventArgs e) => OnConfigChange();

    public void OnConfigChange()
    {
        m_enabled = enabled?.Value is Toggle.On;
        m_duration = duration?.Value ?? m_duration;
        m_nearBaseOnly = baseOnly?.Value is Toggle.On;
        m_pauseIfNoPlayerInArea = pauseNoPlayers?.Value is Toggle.On;
        m_eventRange = range?.Value ?? m_eventRange;
        m_requiredGlobalKeys = new StringList(requiredKeys?.Value ?? "").list;
        m_notRequiredGlobalKeys = new StringList(notRequiredKeys?.Value ?? "").list;
    }
}