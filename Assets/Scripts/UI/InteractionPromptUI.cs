using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Singleton UI for showing interaction prompts using sprite images.
/// Positioned in the bottom-left corner.
/// </summary>
public class InteractionPromptUI : MonoBehaviour
{
    public static InteractionPromptUI Instance { get; private set; }
    
    [Header("References")]
    [SerializeField] private Image promptImage;
    
    [Header("Prompt Sprites")]
    [SerializeField] private Sprite pickFragmentSprite;   // e_to_pick_fragment
    [SerializeField] private Sprite useVendingSprite;     // e_to_use_vending
    [SerializeField] private Sprite equipMaskSprite;      // f_to_equip_mask
    
    public enum PromptType
    {
        None,
        PickFragment,
        UseVending,
        EquipMask
    }
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        // Hide by default
        if (promptImage != null)
        {
            promptImage.enabled = false;
        }
    }
    
    /// <summary>
    /// Show a specific prompt type
    /// </summary>
    public void ShowPrompt(PromptType type)
    {
        if (promptImage == null) return;
        
        Sprite sprite = type switch
        {
            PromptType.PickFragment => pickFragmentSprite,
            PromptType.UseVending => useVendingSprite,
            PromptType.EquipMask => equipMaskSprite,
            _ => null
        };
        
        if (sprite != null)
        {
            promptImage.sprite = sprite;
            promptImage.SetNativeSize();
            promptImage.enabled = true;
        }
        else
        {
            promptImage.enabled = false;
        }
    }
    
    /// <summary>
    /// Legacy support - parse text to determine prompt type
    /// </summary>
    public void ShowPrompt(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            HidePrompt();
            return;
        }
        
        // Determine type from text content
        string lower = text.ToLower();
        
        if (lower.Contains("fragment") || lower.Contains("piece") || lower.Contains("част") || lower.Contains("фрагмент"))
        {
            ShowPrompt(PromptType.PickFragment);
        }
        else if (lower.Contains("vending") || lower.Contains("машина") || lower.Contains("вендинг"))
        {
            ShowPrompt(PromptType.UseVending);
        }
        else if (lower.Contains("equip") || lower.Contains("mask") || lower.Contains("маск") || lower.Contains("сложи"))
        {
            ShowPrompt(PromptType.EquipMask);
        }
        else
        {
            // Default to fragment for unknown prompts
            ShowPrompt(PromptType.PickFragment);
        }
    }
    
    public void HidePrompt()
    {
        if (promptImage != null)
        {
            promptImage.enabled = false;
        }
    }
}
