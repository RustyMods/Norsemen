using System;
using System.Collections.Generic;

namespace Norsemen;

[Serializable]
public class Equipment
{
    public List<ConditionalChanceItem> RandomItems = new();
    public List<ConditionalWeightedSet> RandomSets = new();
    public List<ConditioanlWeightedItem> RandomWeapons = new();
}