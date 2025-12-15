using System.Collections.Generic;
using UnityEngine;

namespace Norsemen;

public static class CloneManager
{
    public static GameObject root;
    internal static readonly Dictionary<string, GameObject> clones = new();
    internal static readonly Dictionary<string, GameObject> norsemen = new();

    static CloneManager()
    {
        root = new GameObject($"{NorsemenPlugin.ModName}_prefab_root");
        UnityEngine.Object.DontDestroyOnLoad(root);
        root.SetActive(false);
    }

    public static Transform GetRootTransform()
    {
        return root.transform;
    }
}