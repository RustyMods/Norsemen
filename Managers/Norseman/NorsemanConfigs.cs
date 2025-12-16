using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;

namespace Norsemen;

public class NorsemanConfigs
{
    public static readonly Dictionary<string, NorsemanConfigs> configs = new();
        
    public ConfigEntry<float> baseHealth = null!;
    public ConfigEntry<Toggle> canMine = null!;
    public ConfigEntry<Toggle> canLumber = null!;
    public ConfigEntry<Toggle> canFish = null!;
    public ConfigEntry<Toggle> workRequiresFood = null!;
    public ConfigEntry<string> conditionalSets = null!;
    public ConfigEntry<string> conditionalItems = null!;
    public ConfigEntry<string> conditionalWeapons = null!;
    public ConfigEntry<float> tamingTime = null!;
    public ConfigEntry<float> baseArmor = null!;
    public ConfigEntry<Toggle> tameable = null!;

    public NorsemanConfigs(string vikingName)
    {
        configs[vikingName] = this;
    }

    public float BaseHealth => baseHealth.Value;
    public bool CanMine => canMine.Value is Toggle.On;
    public bool CanLumber => canLumber.Value is Toggle.On;
    public bool CanFish => canFish.Value is Toggle.On;
    public bool RequireFood => workRequiresFood.Value is Toggle.On;
    public bool AddSet(string name) => new StringList(conditionalSets.Value).list.Contains(name);
    public bool AddItem(string name) => new StringList(conditionalItems.Value).list.Contains(name);
    public bool AddWeapon(string name) => new StringList(conditionalWeapons.Value).list.Contains(name);
    public float TamingTime => tamingTime.Value;
    public float BaseArmor => baseArmor.Value;
    public bool Tameable => tameable.Value is Toggle.On;
}