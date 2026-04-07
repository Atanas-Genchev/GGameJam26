using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Registers all child renderers with the GameManager for texture swapping.
/// Attach to the Map parent object.
/// </summary>
public class MapRendererRegistration : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool registerOnStart = true;
    [SerializeField] private bool excludeGround = true;
    [SerializeField] private string groundObjectName = "Plane";
    
    private void Start()
    {
        if (registerOnStart)
        {
            RegisterAllRenderers();
        }
    }
    
    public void RegisterAllRenderers()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("[MapRendererRegistration] GameManager not found!");
            return;
        }
        
        int count = 0;
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        
        foreach (Renderer renderer in renderers)
        {
            // Optionally skip ground
            if (excludeGround && renderer.gameObject.name == groundObjectName)
            {
                continue;
            }
            
            GameManager.Instance.RegisterBuildingRenderer(renderer);
            count++;
        }
        
        Debug.Log($"[MapRendererRegistration] Registered {count} renderers with GameManager");
    }
}
