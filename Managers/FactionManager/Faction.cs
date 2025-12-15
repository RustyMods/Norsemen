namespace Norsemen;

public class Faction
{
    public readonly string name;
    public int hash;
    public readonly bool friendly;
    public readonly bool targetTames = false;
    public readonly Character.Faction faction;

    public Faction(string name, bool friendly)
    {
        this.name = name;
        this.friendly = friendly;
        hash = name.GetStableHashCode();
        faction = FactionManager.GetFaction(name);
        FactionManager.customFactions[faction] = this;
    }
}