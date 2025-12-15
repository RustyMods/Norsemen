using System;

namespace Norsemen;

public partial class Viking
{
    public NorsemanConfigs configs = null!;
    
    public void SetupConfigs()
    {
        configs = NorsemanConfigs.configs[name.Replace("(Clone)", string.Empty)];
        configs.baseHealth.SettingChanged += OnBaseHealthChanged;
        configs.canLumber.SettingChanged += m_vikingAI.OnWorkConfigChanged;
        configs.canMine.SettingChanged += m_vikingAI.OnWorkConfigChanged;
        configs.tamingTime.SettingChanged += OnTameTimeConfigChanged;
    }

    public void RemoveConfigSubscriptions()
    {
        configs.baseHealth.SettingChanged -= OnBaseHealthChanged;
        configs.canLumber.SettingChanged -= m_vikingAI.OnWorkConfigChanged;
        configs.canMine.SettingChanged -= m_vikingAI.OnWorkConfigChanged;
        configs.tamingTime.SettingChanged -= OnTameTimeConfigChanged;
    }
    
    public void OnBaseHealthChanged(object sender, EventArgs args)
    {
        m_health = configs.BaseHealth;
        SetupMaxHealth();
    }
}