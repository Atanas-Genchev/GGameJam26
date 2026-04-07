using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Swaps the world texture based on the equipped mask.
/// Finds all renderers in the Map object and swaps their textures.
/// Also shows/hides decorations based on mask.
/// - No mask: STREET_colors (default)
/// - Worker mask (Set 1): TempWORKER_colors + show posters
/// - Gangster mask (Set 2): TempREBEL_colors + show graffiti
/// </summary>
public class WorldTextureSwapper : MonoBehaviour
{
    public static WorldTextureSwapper Instance { get; private set; }
    
    [Header("Textures")]
    [SerializeField] private Texture2D defaultTexture;    // STREET_colors
    [SerializeField] private Texture2D workerTexture;     // TempWORKER_colors
    [SerializeField] private Texture2D rebelTexture;      // TempREBEL_colors
    
    [Header("Settings")]
    [Tooltip("Name of the Map root object")]
    [SerializeField] private string mapObjectName = "Map";
    
    [Header("Worker Mask Decorations")]
    [Tooltip("Posters object to show when worker mask is equipped")]
    [SerializeField] private GameObject postersObject;
    
    [Header("Gangster Mask Decorations")]
    [Tooltip("Graffiti object to show when gangster mask is equipped")]
    [SerializeField] private GameObject graffitiObject;
    
    // Cache of all materials we need to swap
    private List<Material> targetMaterials = new List<Material>();
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    private void Start()
    {
        // Subscribe to mask equipped event
        if (MaskInventory.Instance != null)
        {
            MaskInventory.Instance.OnMaskEquipped += OnMaskEquipped;
        }
        
        // Find all materials in the Map
        FindAllMapMaterials();
        
        // Set default texture
        SetTextureOnAllMaterials(defaultTexture);
        
        // Hide all decorations by default
        SetPostersVisible(false);
        SetGraffitiVisible(false);
    }
    
    private void OnDestroy()
    {
        if (MaskInventory.Instance != null)
        {
            MaskInventory.Instance.OnMaskEquipped -= OnMaskEquipped;
        }
    }
    
    private void FindAllMapMaterials()
    {
        targetMaterials.Clear();
        
        GameObject map = GameObject.Find(mapObjectName);
        if (map == null)
        {
            Debug.LogError($"[WorldTextureSwapper] Could not find '{mapObjectName}' object!");
            return;
        }
        
        // Get all renderers in Map and children
        Renderer[] renderers = map.GetComponentsInChildren<Renderer>(true);
        
        HashSet<Material> uniqueMaterials = new HashSet<Material>();
        
        foreach (Renderer renderer in renderers)
        {
            // Use sharedMaterials to get all materials on this renderer
            foreach (Material mat in renderer.sharedMaterials)
            {
                if (mat != null && !uniqueMaterials.Contains(mat))
                {
                    // Check if material has a texture property we can modify
                    if (mat.HasProperty("_BaseMap") || mat.HasProperty("_MainTex"))
                    {
                        uniqueMaterials.Add(mat);
                        targetMaterials.Add(mat);
                    }
                }
            }
        }
        
        Debug.Log($"[WorldTextureSwapper] Found {targetMaterials.Count} materials to swap in '{mapObjectName}'");
    }
    
    private void OnMaskEquipped(MaskData maskData)
    {
        if (MaskTrackerUI.Instance == null) return;
        
        int maskSet = MaskTrackerUI.Instance.CurrentMaskSet;
        
        switch (maskSet)
        {
            case 1:
                // Worker mask equipped - show posters
                SetTextureOnAllMaterials(workerTexture);
                SetPostersVisible(true);
                SetGraffitiVisible(false);
                Debug.Log("[WorldTextureSwapper] Switched to Worker texture + Posters visible");
                break;
            case 2:
                // Gangster/Rebel mask equipped - show graffiti
                SetTextureOnAllMaterials(rebelTexture);
                SetPostersVisible(false);
                SetGraffitiVisible(true);
                Debug.Log("[WorldTextureSwapper] Switched to Rebel texture + Graffiti visible");
                break;
            default:
                SetTextureOnAllMaterials(defaultTexture);
                SetPostersVisible(false);
                SetGraffitiVisible(false);
                Debug.Log("[WorldTextureSwapper] Switched to default texture");
                break;
        }
    }
    
    /// <summary>
    /// Show or hide the posters object
    /// </summary>
    private void SetPostersVisible(bool visible)
    {
        if (postersObject != null)
        {
            postersObject.SetActive(visible);
            Debug.Log($"[WorldTextureSwapper] Posters visibility: {visible}");
        }
    }
    
    /// <summary>
    /// Show or hide the graffiti object
    /// </summary>
    private void SetGraffitiVisible(bool visible)
    {
        if (graffitiObject != null)
        {
            graffitiObject.SetActive(visible);
            Debug.Log($"[WorldTextureSwapper] Graffiti visibility: {visible}");
        }
    }
    
    /// <summary>
    /// Set texture on all cached materials
    /// </summary>
    private void SetTextureOnAllMaterials(Texture2D texture)
    {
        if (texture == null)
        {
            Debug.LogWarning("[WorldTextureSwapper] Texture is null!");
            return;
        }
        
        if (targetMaterials.Count == 0)
        {
            Debug.LogWarning("[WorldTextureSwapper] No target materials found!");
            return;
        }
        
        int successCount = 0;
        
        foreach (Material mat in targetMaterials)
        {
            if (mat == null) continue;
            
            // Try URP property first (_BaseMap), then Standard (_MainTex)
            if (mat.HasProperty("_BaseMap"))
            {
                mat.SetTexture("_BaseMap", texture);
                successCount++;
            }
            else if (mat.HasProperty("_MainTex"))
            {
                mat.SetTexture("_MainTex", texture);
                successCount++;
            }
        }
        
        Debug.Log($"[WorldTextureSwapper] Applied texture '{texture.name}' to {successCount}/{targetMaterials.Count} materials");
    }
    
    /// <summary>
    /// Reset to default texture and hide all decorations
    /// </summary>
    public void ResetToDefault()
    {
        SetTextureOnAllMaterials(defaultTexture);
        SetPostersVisible(false);
        SetGraffitiVisible(false);
    }
    
    /// <summary>
    /// Force refresh materials (call if Map was reloaded)
    /// </summary>
    public void RefreshMaterials()
    {
        FindAllMapMaterials();
    }
}
