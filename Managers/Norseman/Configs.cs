using System;
using System.Collections.Generic;
using BepInEx.Configuration;

namespace Norsemen;

public class NorsemanConfigs
{
    private static readonly Dictionary<string, NorsemanConfigs> configs = new();

    public Heightmap.Biome biome = Heightmap.Biome.None;

    private ConfigEntry<float> baseHealth = null!;
    private ConfigEntry<Toggle> canMine = null!;
    private ConfigEntry<Toggle> canLumber = null!;
    private ConfigEntry<Toggle> canFish = null!;
    private ConfigEntry<Toggle> workRequiresFood = null!;
    private ConfigEntry<float> tamingTime = null!;
    private ConfigEntry<float> baseArmor = null!;
    private ConfigEntry<Toggle> tameable = null!;
    private ConfigEntry<float> workResourceInterval = null!;

    public NorsemanConfigs(string vikingName)
    {
        configs[vikingName] = this;
    }
    
    public static bool TryGetConfig(string prefab, out NorsemanConfigs config) => configs.TryGetValue(prefab, out config);

    public void Setup(Norseman norseman)
    {
        baseHealth = ConfigManager.config(norseman.name, 
            "Base Health", 
            norseman.baseHealth, 
            new ConfigDescription("Set base health", new AcceptableValueRange<float>(1f, 1000f)));
        norseman.m_viking.m_health = baseHealth.Value;

        canMine = ConfigManager.config(norseman.name, 
            "Can Mine", 
            Toggle.On, 
            "If on, will mine ores if has pickaxe");
        canLumber = ConfigManager.config(norseman.name, 
            "Can Lumber", Toggle.On, 
            "If on, will lumber if has axe");
        canFish = ConfigManager.config(norseman.name, 
            "Can Fish", Toggle.On, 
            "If on, will fish if has fishing rod and bait");
        workRequiresFood = ConfigManager.config(norseman.name, 
            "Work Requires Food", Toggle.On, 
            "If on, will only mine and lumber if not hungry");
        
        norseman.spawnInfo.SetupConfigs();
        
        tamingTime = ConfigManager.config(norseman.name, 
            "Taming Duration", 
            1800f, 
            "Set time it take to tame, in seconds");
        baseArmor = ConfigManager.config(norseman.name, 
            "Base Armor", 
            norseman.baseArmor, 
            "Set base armor");

        tameable = ConfigManager.config(norseman.name, 
            "Tameable", 
            Toggle.On, 
            "If on, norseman is tameable");
        workResourceInterval = ConfigManager.config(norseman.name, 
            "Work Resource Interval", 
            10f,
            "Set minimum required time to receive resource from working");
        
        Subscribe();
    }

    private void Subscribe()
    {
        baseHealth.SettingChanged += OnHealthChange;
        canMine.SettingChanged += OnWorkConfigChange;
        canFish.SettingChanged += OnWorkConfigChange;
        canLumber.SettingChanged += OnWorkConfigChange;
        tamingTime.SettingChanged += OnTameTimeConfigChanged;
    }

    private void OnHealthChange(object sender, EventArgs e)
    {
        List<Viking> vikings = Viking.GetAllVikings();
        for (int i = 0; i < vikings.Count; ++i)
        {
            Viking viking = vikings[i];
            if (!viking.m_nview.IsValid() || !viking.m_nview.IsOwner()) continue;
            viking.m_health = baseHealth.Value;
            viking.SetupMaxHealth();
        }
    }

    private static void OnWorkConfigChange(object sender, EventArgs e)
    {
        List<Viking> vikings = Viking.GetAllVikings();
        for (int i = 0; i < vikings.Count; ++i)
        {
            Viking viking = vikings[i];
            if (!viking.m_nview.IsValid() || !viking.m_nview.IsOwner()) continue;
            viking.m_vikingAI.ResetWorkTargets();
        }
    }

    private void OnTameTimeConfigChanged(object sender, EventArgs args)
    {
        List<Viking> vikings = Viking.GetAllVikings();
        for (int i = 0; i < vikings.Count; ++i)
        {
            Viking viking = vikings[i];
            if (!viking.m_nview.IsValid() || !viking.m_nview.IsOwner()) continue;
            viking.m_tamingTime = tamingTime.Value;
            viking.m_nview.GetZDO().Set(ZDOVars.s_tameTimeLeft, viking.m_tamingTime);
        }
    }
    
    public static float GetBaseArmor(string prefabName, float defaultValue = 0f) => 
        TryGetConfig(prefabName, out NorsemanConfigs config) ? config.baseArmor.Value : defaultValue;
    public static float GetTameTime(string prefabName, float defaultValue = 1800f) => 
        TryGetConfig(prefabName, out NorsemanConfigs config) ? config.tamingTime.Value : defaultValue;
    public static float GetBaseHealth(string prefabName, float defaultValue = 50f) => 
        TryGetConfig(prefabName, out NorsemanConfigs config) ? config.baseHealth.Value : defaultValue;
    public static bool CanVikingFish(string prefabName) => 
        TryGetConfig(prefabName, out NorsemanConfigs config) && config.canFish.Value is Toggle.On;
    public static bool CanVikingLumber(string prefabName) => 
        TryGetConfig(prefabName, out NorsemanConfigs config) && config.canLumber.Value is Toggle.On;
    public static bool CanVikingMine(string prefabName) => 
        TryGetConfig(prefabName, out NorsemanConfigs config) && config.canMine.Value is Toggle.On;
    public static bool DoesWorkRequireFood(string prefabName) => 
        TryGetConfig(prefabName, out NorsemanConfigs config) && config.workRequiresFood.Value is Toggle.On;
    public static float GetWorkResourceInterval(string prefabName) =>
        TryGetConfig(prefabName, out NorsemanConfigs config) ? config.workResourceInterval.Value : 1f;
    public static bool IsTameable(string prefabName) =>
        TryGetConfig(prefabName, out NorsemanConfigs config) && config.tameable.Value is Toggle.On;
}