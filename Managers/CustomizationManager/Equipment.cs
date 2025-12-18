using System;
using System.Collections.Generic;

namespace Norsemen;

[Serializable]
public class Equipment
{
    public List<ConditionalRandomItem> RandomItems = new();
    public List<ConditionalRandomSet> RandomSets = new();
    public List<ConditionalRandomWeapon> RandomWeapons = new();
}