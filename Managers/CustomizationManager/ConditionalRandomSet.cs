using System;
using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Serialization;

namespace Norsemen;

[Serializable]
public class ConditionalRandomSet
{
    public List<string> PrefabNames = new();
    public string RequiredDefeatKey;
    public float Weight;

    [YamlIgnore] private Viking.ConditionalItemSet? _set;
    [YamlIgnore] public Viking.ConditionalItemSet set
    {
        get
        {
            if (_set != null) return _set;
            _set = new Viking.ConditionalItemSet()
            {
                m_requiredDefeatKey = RequiredDefeatKey,
                m_weight = Weight,
                m_items = PrefabNames.Select(Helpers.GetPrefab).ToArray()
            };
            return _set;
        }
    }
    public ConditionalRandomSet(string requiredDefeatKey, float weight, params string[] prefabNames)
    {
        RequiredDefeatKey = requiredDefeatKey;
        PrefabNames.Add(prefabNames);
        Weight = weight;
    }
}