using System.Collections.Generic;
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

        if (elfEars != null)
        {
            var extra = clone.AddComponent<ExtraRagdoll>();
            if (clone.TryGetComponent(out VisEquipment visEq))
            {
                GameObject attach_skin = elfEars.transform.Find("attach_skin").gameObject;
                GameObject? instance = UnityEngine.Object.Instantiate(attach_skin);
                instance.name = "elf_ears";
                VisEquipment.CleanupInstance(instance);

                SkinnedMeshRenderer? body = visEq.m_bodyModel;
                instance.transform.SetParent(body.transform.parent);
                instance.transform.localPosition = Vector3.zero;
                instance.transform.localRotation = Quaternion.identity;

                List<Material> earMats = new();
            
                foreach (SkinnedMeshRenderer? renderer in instance.GetComponentsInChildren<SkinnedMeshRenderer>())
                {
                    renderer.rootBone = body.rootBone;
                    renderer.bones = body.bones;
                    if (renderer.name.CustomStartsWith("007"))
                    {
                        earMats.Add(renderer.materials);
                    }
                }

                extra.m_elfEars = instance;
                extra.m_elfEarMats = earMats.ToArray();
            }
        }


        PrefabManager.RegisterPrefab(clone);
        return clone;
    }

    public class ExtraRagdoll : MonoBehaviour
    {
        public GameObject m_elfEars;
        public Material[] m_elfEarMats;
        
        public void SetElfEars(bool isElf, Color color)
        {
            if (m_elfEars == null) return;
            m_elfEars.SetActive(isElf);
            SetElfEarColor(color);
        }

        public void SetElfEarColor(Color color)
        {
            if (m_elfEarMats == null) return;

            foreach (Material material in m_elfEarMats)
            {
                material.color = color;
            }
        }
    }
}