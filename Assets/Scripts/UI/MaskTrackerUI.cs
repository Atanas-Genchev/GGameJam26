using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI panel that tracks mask fragment collection progress.
/// Supports multiple mask sets (Mask 1: Papka/Riza/Badge, Mask 2: Bandana/Spray/Hoodie).
/// </summary>
public class MaskTrackerUI : MonoBehaviour
{
    public static MaskTrackerUI Instance { get; private set; }
    
    [Header("Text Display")]
    [SerializeField] private TextMeshProUGUI progressText;
    
    [Header("Background")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Sprite mask1Background;  // mask_fragments_progression_rectangle (current)
    [SerializeField] private Sprite mask2Background;  // mask_fragments_progression_rectangle_2
    
    [Header("Icon Images")]
    [SerializeField] private Image icon1;  // Left icon
    [SerializeField] private Image icon2;  // Middle icon
    [SerializeField] private Image icon3;  // Right icon
    
    [Header("Mask 1 - Not Taken Sprites")]
    [SerializeField] private Sprite papkaNotTaken;
    [SerializeField] private Sprite rizaNotTaken;
    [SerializeField] private Sprite badgeNotTaken;
    
    [Header("Mask 1 - Taken Sprites")]
    [SerializeField] private Sprite papkaTaken;
    [SerializeField] private Sprite rizaTaken;
    [SerializeField] private Sprite badgeTaken;
    
    [Header("Mask 2 - Not Taken Sprites")]
    [SerializeField] private Sprite bandanaNotTaken;
    [SerializeField] private Sprite sprayNotTaken;
    [SerializeField] private Sprite hoodieNotTaken;
    
    [Header("Mask 2 - Taken Sprites")]
    [SerializeField] private Sprite bandanaTaken;
    [SerializeField] private Sprite sprayTaken;
    [SerializeField] private Sprite hoodieTaken;
    
    private int currentMaskSet = 1;
    private int totalPieces = 3;
    private int collectedCount = 0;
    
    // Track which specific items are collected for each mask
    private bool item1Collected = false;  // Papka / Bandana
    private bool item2Collected = false;  // Riza / Spray
    private bool item3Collected = false;  // Badge / Hoodie
    
    public int CurrentMaskSet => currentMaskSet;
    public bool AllCollected => collectedCount >= totalPieces;
    public int CollectedCount => collectedCount;
    public int TotalPieces => totalPieces;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        // Initialize display
        UpdateDisplay();
    }
    
    /// <summary>
    /// Switch to a different mask set (1 or 2)
    /// </summary>
    public void SetMaskSet(int maskSet)
    {
        currentMaskSet = maskSet;
        ResetProgress();
        
        // Update background
        if (backgroundImage != null)
        {
            Sprite bgSprite = maskSet switch
            {
                1 => mask1Background,
                2 => mask2Background,
                _ => mask1Background
            };
            if (bgSprite != null)
            {
                backgroundImage.sprite = bgSprite;
            }
        }
        
        // Update icons to not taken state for new mask set
        UpdateIconsForMaskSet();
        
        Debug.Log($"[MaskTrackerUI] Switched to mask set {currentMaskSet}");
    }
    
    /// <summary>
    /// Update all icons based on current mask set
    /// </summary>
private void UpdateIconsForMaskSet()
    {
        if (currentMaskSet == 1)
        {
            if (icon1 != null) icon1.sprite = item1Collected ? papkaTaken : papkaNotTaken;
            if (icon2 != null) icon2.sprite = item2Collected ? rizaTaken : rizaNotTaken;
            if (icon3 != null) icon3.sprite = item3Collected ? badgeTaken : badgeNotTaken;
        }
        else if (currentMaskSet == 2)
        {
            if (icon1 != null) icon1.sprite = item1Collected ? bandanaTaken : bandanaNotTaken;
            if (icon2 != null) icon2.sprite = item2Collected ? sprayTaken : sprayNotTaken;
            if (icon3 != null) icon3.sprite = item3Collected ? hoodieTaken : hoodieNotTaken;
        }
    }
    
    /// <summary>
    /// Called when a specific collectible is picked up.
    /// Mask 1: "Papka", "Riza", "Badge"
    /// Mask 2: "Bandana", "Spray", "Hoodie"
    /// </summary>
    public void CollectItem(string itemType)
    {
        bool wasCollected = false;
        string lowerType = itemType.ToLower();
        
        // Mask 1 items
        if (currentMaskSet == 1)
        {
            switch (lowerType)
            {
                case "papka":
                    if (!item1Collected)
                    {
                        item1Collected = true;
                        wasCollected = true;
                        if (icon1 != null && papkaTaken != null)
                            icon1.sprite = papkaTaken;
                    }
                    break;
                case "riza":
                    if (!item2Collected)
                    {
                        item2Collected = true;
                        wasCollected = true;
                        if (icon2 != null && rizaTaken != null)
                            icon2.sprite = rizaTaken;
                    }
                    break;
                case "badge":
                    if (!item3Collected)
                    {
                        item3Collected = true;
                        wasCollected = true;
                        if (icon3 != null && badgeTaken != null)
                            icon3.sprite = badgeTaken;
                    }
                    break;
            }
        }
        // Mask 2 items
        else if (currentMaskSet == 2)
        {
            switch (lowerType)
            {
                case "bandana":
                    if (!item1Collected)
                    {
                        item1Collected = true;
                        wasCollected = true;
                        if (icon1 != null && bandanaTaken != null)
                            icon1.sprite = bandanaTaken;
                    }
                    break;
                case "spray":
                    if (!item2Collected)
                    {
                        item2Collected = true;
                        wasCollected = true;
                        if (icon2 != null && sprayTaken != null)
                            icon2.sprite = sprayTaken;
                    }
                    break;
                case "hoodie":
                    if (!item3Collected)
                    {
                        item3Collected = true;
                        wasCollected = true;
                        if (icon3 != null && hoodieTaken != null)
                            icon3.sprite = hoodieTaken;
                    }
                    break;
            }
        }
        
        if (wasCollected)
        {
            collectedCount++;
            UpdateDisplay();
            Debug.Log($"[MaskTrackerUI] Collected {itemType}. Progress: {collectedCount}/{totalPieces}");
            
            // Check if all pieces collected - show success screen
            if (AllCollected)
            {
                SuccessScreenUI.Instance?.Show();
            }
        }
        else if (!wasCollected)
        {
            Debug.LogWarning($"[MaskTrackerUI] Unknown or already collected item: {itemType} (current mask set: {currentMaskSet})");
        }
    }
    
    /// <summary>
    /// Updates the progress text display
    /// </summary>
    private void UpdateDisplay()
    {
        if (progressText != null)
        {
            progressText.text = $"{collectedCount} / {totalPieces}  Ч А С Т И  С Ъ Б Р А Н И";
        }
    }
    
    /// <summary>
    /// Reset all collection progress (for new game/level or switching masks)
    /// </summary>
    public void ResetProgress()
    {
        collectedCount = 0;
        item1Collected = false;
        item2Collected = false;
        item3Collected = false;
        
        UpdateIconsForMaskSet();
        UpdateDisplay();
    }
}
