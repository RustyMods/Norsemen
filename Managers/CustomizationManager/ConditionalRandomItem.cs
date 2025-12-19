using System;
using YamlDotNet.Serialization;

namespace Norsemen;

[Serializable]
public class ConditionalRandomItem
{
    public string PrefabName = "";
    public string RequiredDefeatKey = "";
    public float Chance = 0.5f;
    public int Min = 1;
    public int Max = 1;

    [YamlIgnore] private Viking.ConditionalRandomItem? _item;
    [YamlIgnore] public Viking.ConditionalRandomItem item
    {
        get
        {
            if (_item != null) return _item;
            _item = new Viking.ConditionalRandomItem()
            {
                m_prefab = Helpers.GetPrefab(PrefabName),
                m_requiredDefeatKey = RequiredDefeatKey,
                m_chance = Chance,
                m_min = Min,
                m_max = Max
            };
            return _item;
        }
    }

    public ConditionalRandomItem(string prefab, int min = 1, int max = 1, float chance = 0.5f, string requiredDefeatKey = "")
    {
        PrefabName = prefab;
        RequiredDefeatKey = requiredDefeatKey;
        Chance = chance;
        Min = min;
        Max = max;
    }
    
    public ConditionalRandomItem(){}
}