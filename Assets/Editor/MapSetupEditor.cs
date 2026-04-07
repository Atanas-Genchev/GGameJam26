using UnityEngine;
using UnityEditor;

public class MapSetupEditor : EditorWindow
{
    [MenuItem("Tools/Setup Map Colliders")]
    static void SetupMapColliders()
    {
        GameObject map = GameObject.Find("GGJ_Map01_Preview");
        if (map == null)
        {
            map = GameObject.Find("Map");
        }
        
        if (map == null)
        {
            Debug.LogError("Could not find Map or GGJ_Map01_Preview!");
            return;
        }
        
        int count = 0;
        MeshRenderer[] renderers = map.GetComponentsInChildren<MeshRenderer>();
        
        foreach (MeshRenderer renderer in renderers)
        {
            // Skip if already has a collider
            if (renderer.GetComponent<Collider>() != null) continue;
            
            MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                Undo.AddComponent<MeshCollider>(renderer.gameObject);
                count++;
            }
        }
        
        Debug.Log($"[MapSetup] Added {count} MeshColliders to map objects");
    }
    
    [MenuItem("Tools/Rename Map")]
    static void RenameMap()
    {
        GameObject map = GameObject.Find("GGJ_Map01_Preview");
        if (map != null)
        {
            Undo.RecordObject(map, "Rename Map");
            map.name = "Map";
            Debug.Log("Renamed GGJ_Map01_Preview to Map");
        }
    }
}
