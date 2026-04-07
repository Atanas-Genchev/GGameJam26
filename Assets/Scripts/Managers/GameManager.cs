using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Central game manager that handles game state, mask equipping, and world appearance changes.
/// This is a singleton - only one should exist in the scene.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("References")]
    [SerializeField] private MaskInventory playerInventory;
    
    [Header("Swappable Objects")]
    [Tooltip("Renderers that will have their materials swapped when a mask is equipped")]
    [SerializeField] private List<Renderer> buildingRenderers = new List<Renderer>();
    [SerializeField] private List<Renderer> groundRenderers = new List<Renderer>();
    
    [Header("Default Materials (No Mask)")]
    [SerializeField] private Material defaultBuildingMaterial;
    [SerializeField] private Material defaultGroundMaterial;
    
    [Header("Debug/Testing Materials")]
    [Tooltip("Used when MaskData doesn't have a material assigned")]
    [SerializeField] private Material debugMaskBuildingMaterial; // Black material for testing
    
    private MaskData currentEquippedMask = null;
    
    /// <summary>
    /// Event fired when world appearance changes
    /// </summary>
    public event System.Action<MaskData> OnWorldChanged;
    
    public MaskData CurrentEquippedMask => currentEquippedMask;
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    private void Start()
    {
        // Subscribe to inventory events
        if (playerInventory != null)
        {
            playerInventory.OnMaskEquipped += HandleMaskEquipped;
        }
        else
        {
            Debug.LogError("[GameManager] Player Inventory not assigned!");
        }
        
        // Auto-find building renderers if not assigned
        if (buildingRenderers.Count == 0)
        {
            AutoFindBuildingRenderers();
        }
    }
    
    private void OnDestroy()
    {
        if (playerInventory != null)
        {
            playerInventory.OnMaskEquipped -= HandleMaskEquipped;
        }
    }
    
    private void AutoFindBuildingRenderers()
    {
        // Find all objects under "Environment" parent
        GameObject envParent = GameObject.Find("Environment");
        if (envParent != null)
        {
            Renderer[] renderers = envParent.GetComponentsInChildren<Renderer>();
            buildingRenderers.AddRange(renderers);
            Debug.Log($"[GameManager] Auto-found {renderers.Length} building renderers");
        }
    }
    
    private void HandleMaskEquipped(MaskData maskData)
    {
        currentEquippedMask = maskData;
        
        string maskName = maskData != null ? maskData.maskName : "Unknown";
        Debug.Log($"[GameManager] Mask equipped: {maskName}. Applying world changes...");
        
        ApplyMaskToWorld(maskData);
    }
    
    /// <summary>
    /// Apply the mask's visual effects to the world
    /// </summary>
    public void ApplyMaskToWorld(MaskData maskData)
    {
        Material buildingMat = null;
        Material groundMat = null;
        
        if (maskData != null)
        {
            // Use mask-specific materials if available
            buildingMat = maskData.buildingMaterial;
            groundMat = maskData.groundMaterial;
            
            // Apply lighting/fog settings
            if (maskData.enableFog)
            {
                RenderSettings.fog = true;
                RenderSettings.fogColor = maskData.fogColor;
            }
            RenderSettings.ambientLight = maskData.ambientLightColor;
        }
        
        // Fall back to debug material if mask doesn't have materials
        if (buildingMat == null)
        {
            buildingMat = debugMaskBuildingMaterial;
        }
        
        // Apply to buildings
        if (buildingMat != null)
        {
            foreach (var renderer in buildingRenderers)
            {
                if (renderer != null)
                {
                    renderer.material = buildingMat;
                }
            }
            Debug.Log($"[GameManager] Applied material to {buildingRenderers.Count} buildings");
        }
        
        // Apply to ground
        if (groundMat != null)
        {
            foreach (var renderer in groundRenderers)
            {
                if (renderer != null)
                {
                    renderer.material = groundMat;
                }
            }
        }
        
        OnWorldChanged?.Invoke(maskData);
    }
    
    /// <summary>
    /// Reset world to default appearance (no mask)
    /// </summary>
    public void ResetWorldToDefault()
    {
        currentEquippedMask = null;
        
        // Apply default materials
        if (defaultBuildingMaterial != null)
        {
            foreach (var renderer in buildingRenderers)
            {
                if (renderer != null)
                {
                    renderer.material = defaultBuildingMaterial;
                }
            }
        }
        
        if (defaultGroundMaterial != null)
        {
            foreach (var renderer in groundRenderers)
            {
                if (renderer != null)
                {
                    renderer.material = defaultGroundMaterial;
                }
            }
        }
        
        // Reset lighting
        RenderSettings.fog = false;
        RenderSettings.ambientLight = Color.white;
        
        OnWorldChanged?.Invoke(null);
    }
    
    /// <summary>
    /// Register a building renderer at runtime
    /// </summary>
    public void RegisterBuildingRenderer(Renderer renderer)
    {
        if (!buildingRenderers.Contains(renderer))
        {
            buildingRenderers.Add(renderer);
        }
    }
    
    /// <summary>
    /// Register a ground renderer at runtime
    /// </summary>
    public void RegisterGroundRenderer(Renderer renderer)
    {
        if (!groundRenderers.Contains(renderer))
        {
            groundRenderers.Add(renderer);
        }
    }
}
