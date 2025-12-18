using BepInEx.Configuration;

namespace Norsemen;

public class Faction
{
    public readonly string name;
    public int hash;
    private readonly bool friendly;
    public readonly string hoverColor = "white";
    public readonly bool targetTames = false;
    public readonly Character.Faction faction;

    public Faction(string name, bool friendly)
    {
        this.name = name;
        this.friendly = friendly;
        hash = name.GetStableHashCode();
        faction = FactionManager.GetFaction(name);
        isFriendly = ConfigManager.config($"{name} Faction", "Friendly", friendly ? Toggle.On : Toggle.Off, $"If on, {name} are friendly unless aggravated");
        FactionManager.customFactions[faction] = this;
    }

    public readonly ConfigEntry<Toggle> isFriendly;

    public bool IsFriendly() => isFriendly.Value is Toggle.On;
}