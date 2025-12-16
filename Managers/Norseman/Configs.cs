using System.Linq;
using BepInEx.Configuration;

namespace Norsemen;

public partial class Norseman
{
    public void SetupConfigs()
    {
        configs.baseHealth = ConfigManager.config(name, "Base Health", baseHealth, new ConfigDescription("Set base health", new AcceptableValueRange<float>(1f, 1000f)));
        m_viking.m_health = configs.baseHealth.Value;

        configs.canMine = ConfigManager.config(name, "Can Mine", Toggle.On, "If on, will mine ores if has pickaxe");
        configs.canLumber = ConfigManager.config(name, "Can Lumber", Toggle.On, "If on, will lumber if has axe");
        configs.canFish = ConfigManager.config(name, "Can Fish", Toggle.On, "If on, will fish if has fishing rod and bait");
        configs.workRequiresFood = ConfigManager.config(name, "Work Requires Food", Toggle.On, "If on, will only mine and lumber if not hungry");
        
        spawnInfo.SetupConfigs();
        
        configs.conditionalSets = ConfigManager.config(name, "Conditional Sets",
            new StringList(conditionalRandomSets.Select(x => x.Name).ToList()).ToString(),
            new ConfigDescription("Set conditional random sets by unique name", null, StringList.attributes));
        configs.conditionalItems = ConfigManager.config(name, "Conditional Items",
            new StringList(conditionalRandomItems.Select(x => x.Name).ToList()).ToString(),
            new ConfigDescription("Set conditional items by unique name", null, StringList.attributes));

        configs.conditionalWeapons = ConfigManager.config(name, "Conditional Weapons",
            new StringList(conditionalRandomWeapons.Select(x => x.Name).ToList()).ToString(),
            new ConfigDescription("Set conditional weapons by unique name", null, StringList.attributes));

        configs.tamingTime = ConfigManager.config(name, "Taming Duration", 1800f, "Set time it take to tame, in seconds");
        configs.baseArmor = ConfigManager.config(name, "Base Armor", baseArmor, "Set base armor");

        configs.tameable = ConfigManager.config(name, "Tameable", Toggle.On, "If on, norseman is tameable");
    }
}