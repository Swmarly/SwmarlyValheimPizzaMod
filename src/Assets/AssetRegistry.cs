using System;
using System.Collections.Generic;
using UnityEngine;
using Jotunn;
using Jotunn.Managers;

namespace SwmarlyValheimPizzaMod.Assets;

internal static class AssetRegistry
{
    private static readonly Dictionary<string, GameObject> Prefabs = new(StringComparer.Ordinal);

    public static GameObject? Find(string prefabName)
    {
        if (Prefabs.TryGetValue(prefabName, out GameObject? cached) && cached)
            return cached;

        GameObject? prefab = PrefabManager.Instance.GetPrefab(prefabName);
        if (prefab)
            Prefabs[prefabName] = prefab;
        return prefab;
    }

    public static GameObject Require(string prefabName)
    {
        GameObject? prefab = Find(prefabName);
        if (!prefab)
            throw new InvalidOperationException($"Required Valheim prefab '{prefabName}' was not found.");
        return prefab;
    }

    public static void LogMissing(string prefabName)
    {
        PizzaPlugin.Log.LogWarning($"Optional/required asset '{prefabName}' is missing; the affected content will be skipped.");
    }
}
