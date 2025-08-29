// Editor/FindMissingAndNullRefs.cs
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class FindMissingAndNullRefs
{
    [MenuItem("Tools/Find Missing Scripts & Null Beatmaps")]
    static void Find()
    {
        int missingCount = 0;
        foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            var comps = go.GetComponents<Component>();
            for (int i = 0; i < comps.Length; i++)
            {
                if (comps[i] == null)
                {
                    Debug.LogError($"Missing script in GameObject: {GetFullPath(go)}", go);
                    missingCount++;
                }
            }
        }

        int nullBeatmaps = 0;
        foreach (var bm in Resources.FindObjectsOfTypeAll<Beatmap>())
        {
            if (bm.clip == null || bm.beatsCSV == null)
            {
                Debug.LogError($"Invalid Beatmap asset: {AssetDatabase.GetAssetPath(bm)} - clip:{bm.clip} csv:{bm.beatsCSV}");
                nullBeatmaps++;
            }
        }

        Debug.Log($"Done. Missing scripts: {missingCount}. Bad Beatmaps: {nullBeatmaps}");
    }

    static string GetFullPath(GameObject go)
    {
        string path = go.name;
        while (go.transform.parent != null)
        {
            go = go.transform.parent.gameObject;
            path = go.name + "/" + path;
        }
        return path;
    }
}
