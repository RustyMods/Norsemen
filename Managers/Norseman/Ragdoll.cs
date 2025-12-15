using UnityEngine;

namespace Norsemen;

public partial class Norseman
{
    private static GameObject? ragdoll;

    private static GameObject? CreateRagdoll()
    {
        GameObject? prefab = Helpers.GetPrefab("Player_ragdoll");
        if (prefab is null) return null;
        GameObject clone = UnityEngine.Object.Instantiate(prefab, CloneManager.root.transform, false);
        clone.name = "Norseman_ragdoll";
        if (clone.TryGetComponent(out Ragdoll component))
        {
            component.m_ttl = 8f;
            component.m_removeEffect.m_effectPrefabs = new[]
            {
                new EffectList.EffectData()
                {
                    m_prefab = Helpers.GetPrefab("vfx_corpse_destruction_small")
                }
            };
            component.m_float = true;
            component.m_dropItems = false;
        }

        PrefabManager.RegisterPrefab(clone);
        return clone;
    }
}