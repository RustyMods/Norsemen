using System;
using YamlDotNet.Serialization;

namespace Norsemen;

[Serializable]
public class ConditionalRandomWeapon
{
    public string PrefabName;
    public string RequiredDefeatKey;
    public float Weight;

    [YamlIgnore] private Viking.ConditionalRandomWeapon? _weapon;

    [YamlIgnore] public Viking.ConditionalRandomWeapon weapon
    {
        get
        {
            if (_weapon != null) return _weapon;
            _weapon = new Viking.ConditionalRandomWeapon()
            {
                m_requiredDefeatKey = RequiredDefeatKey,
                m_weight = Weight,
                m_prefab = Helpers.GetPrefab(PrefabName),
            };
            return _weapon;
        }
    }
    public ConditionalRandomWeapon(string prefabName, string requiredDefeatKey, float weight)
    {
        PrefabName = prefabName;
        RequiredDefeatKey = requiredDefeatKey;
        Weight = weight;
    }
}