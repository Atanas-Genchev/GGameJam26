using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

/// <summary>
/// Displays success screen when all mask fragments are collected.
/// Press F to dismiss the screen and equip the mask.
/// </summary>
public class SuccessScreenUI : MonoBehaviour
{
    public static SuccessScreenUI Instance { get; private set; }
    
    [Header("References")]
    [SerializeField] private Image successImage;
    [SerializeField] private Sprite mask1SuccessSprite; // Success_Mask_Worker.png
    [SerializeField] private Sprite mask2SuccessSprite; // Success_Mask_Gangster.png
    
    private bool isShowing = false;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        // Auto-find Image component if not assigned
        if (successImage == null)
        {
            successImage = GetComponent<Image>();
        }
        
        // Hide by default
        if (successImage != null)
        {
            successImage.enabled = false;
        }
    }
    
    private void Update()
    {
        // Check for F key to dismiss and equip mask
        if (isShowing)
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.fKey.wasPressedThisFrame)
            {
                Hide();
            }
        }
    }
    
    /// <summary>
    /// Show the success screen
    /// </summary>
public void Show()
    {
        if (successImage == null)
        {
            Debug.LogError("[SuccessScreenUI] No Image component found!");
            return;
        }
        
        // Get the appropriate sprite based on current mask set
        Sprite spriteToShow = mask1SuccessSprite; // Default to mask 1
        if (MaskTrackerUI.Instance != null)
        {
            int currentMaskSet = MaskTrackerUI.Instance.CurrentMaskSet;
            spriteToShow = currentMaskSet switch
            {
                1 => mask1SuccessSprite,
                2 => mask2SuccessSprite,
                _ => mask1SuccessSprite
            };
            Debug.Log($"[SuccessScreenUI] Showing success screen for mask set {currentMaskSet}");
        }
        
        if (spriteToShow != null)
        {
            successImage.sprite = spriteToShow;
            successImage.SetNativeSize();
        }
        else
        {
            Debug.LogWarning("[SuccessScreenUI] No sprite assigned for current mask set!");
        }
        
        successImage.enabled = true;
        isShowing = true;
        
        Debug.Log("[SuccessScreenUI] Success screen displayed. Press F to equip mask and continue.");
    }
    
    /// <summary>
    /// Hide the success screen and equip the mask
    /// </summary>
    public void Hide()
    {
        if (successImage != null)
        {
            successImage.enabled = false;
        }
        isShowing = false;
        
        // Equip the mask when dismissing the success screen
        MaskInventory.Instance?.EquipMask();
        
        Debug.Log("[SuccessScreenUI] Success screen dismissed. Mask equipped!");
    }
    
    public bool IsShowing => isShowing;
}
