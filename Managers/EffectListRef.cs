using System.Collections.Generic;
using UnityEngine;

namespace Norsemen;

public class EffectListRef
{
    public List<EffectDataRef> dataRefs = new();
    private readonly List<EffectList.EffectData> data = new();
    private EffectList? _effects;

    public EffectList Effects
    {
        get
        {
            if (_effects != null) return _effects;
            foreach (EffectDataRef? dataRef in dataRefs)
            {
                data.Add(dataRef.ToEffectData());
            }
            _effects = new EffectList();
            _effects.m_effectPrefabs = data.ToArray();
            return _effects;
        }
    }

    public GameObject[] Create(Vector3 basePos, Quaternion baseRot, Transform? baseParent = null, float scale = 1f,
        int variant = -1) => Effects.Create(basePos, baseRot, baseParent, scale, variant);

    public EffectListRef(params string[] effects) => Add(effects);
    public EffectListRef(params EffectDataRef[] refs) => dataRefs.AddRange(refs);
    public EffectListRef(){}

    public void Add(params string[] effects)
    {
        foreach (string name in effects)
        {
            dataRefs.Add(new EffectDataRef(name));
        }
    }

    public void Add(params EffectDataRef[] refs) => dataRefs.AddRange(refs);

    public class EffectDataRef
    {
        public readonly string prefabName;
        public int variant;
        public bool attach;
        public bool follow;
        public bool inheritParentRotation;
        public bool inheritParentScale;
        public bool multiplyParentVisualScale;
        public bool randomRotation;
        public bool scale;
        public string childTransform = string.Empty;

        public EffectDataRef(string prefabName)
        {
            this.prefabName = prefabName;
        }

        private EffectList.EffectData? _data;

        public EffectList.EffectData ToEffectData()
        {
            if (_data != null) return _data;
            if (Helpers.GetPrefab(prefabName) is not { } prefab)
            {
                NorsemenPlugin.LogError("Effect Data Reference invalid: " + prefabName);
                return new();
            }
            _data = new EffectList.EffectData()
            {
                m_prefab = prefab,
                m_variant = variant,
                m_attach = attach,
                m_follow = follow,
                m_inheritParentRotation = inheritParentRotation,
                m_inheritParentScale = inheritParentScale,
                m_multiplyParentVisualScale = multiplyParentVisualScale,
                m_randomRotation = randomRotation,
                m_scale = scale,
                m_childTransform = childTransform
            };
            return _data;
        }
    }
}