using System;
using System.Collections.Generic;
using UnityEngine;

namespace Norsemen;

public class Clone
{
    private GameObject? Prefab;
    private readonly string PrefabName;
    private readonly string NewName;
    private bool Loaded;
    public event Action<GameObject>? OnCreated;

    public Clone(string prefabName, string newName)
    {
        PrefabName = prefabName;
        NewName = newName;
        PrefabManager.Clones.Add(this);
    }

    internal void Create()
    {
        if (Loaded) return;
        if (Helpers.GetPrefab(PrefabName) is not { } prefab) return;
        Prefab = UnityEngine.Object.Instantiate(prefab, CloneManager.GetRootTransform(), false);
        Prefab.name = NewName;
        PrefabManager.RegisterPrefab(Prefab);
        OnCreated?.Invoke(Prefab);
        CloneManager.clones[Prefab.name] = Prefab;
        Loaded = true;
    }
}