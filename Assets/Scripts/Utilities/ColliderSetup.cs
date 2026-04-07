using UnityEngine;

/// <summary>
/// Utility script to add MeshColliders to all child objects with MeshRenderers.
/// Attach to a parent object and click the context menu to add colliders.
/// </summary>
public class ColliderSetup : MonoBehaviour
{
    [ContextMenu("Add MeshColliders to All Children")]
    public void AddCollidersToChildren()
    {
        int count = 0;
        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
        
        foreach (MeshRenderer renderer in renderers)
        {
            // Skip if already has a collider
            if (renderer.GetComponent<Collider>() != null) continue;
            
            MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                MeshCollider collider = renderer.gameObject.AddComponent<MeshCollider>();
                count++;
            }
        }
        
        Debug.Log($"[ColliderSetup] Added {count} MeshColliders");
    }
    
    [ContextMenu("Add BoxColliders to All Children")]
    public void AddBoxCollidersToChildren()
    {
        int count = 0;
        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
        
        foreach (MeshRenderer renderer in renderers)
        {
            // Skip if already has a collider
            if (renderer.GetComponent<Collider>() != null) continue;
            
            renderer.gameObject.AddComponent<BoxCollider>();
            count++;
        }
        
        Debug.Log($"[ColliderSetup] Added {count} BoxColliders");
    }
    
    [ContextMenu("Remove All Colliders from Children")]
    public void RemoveCollidersFromChildren()
    {
        int count = 0;
        Collider[] colliders = GetComponentsInChildren<Collider>();
        
        foreach (Collider col in colliders)
        {
            DestroyImmediate(col);
            count++;
        }
        
        Debug.Log($"[ColliderSetup] Removed {count} colliders");
    }
}
