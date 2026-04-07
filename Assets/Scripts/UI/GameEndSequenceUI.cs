using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

/// <summary>
/// Handles the game end sequence - shows 3 images in order:
/// 1. Manifesto_Gangster (dismissable with C)
/// 2. Game_End (dismissable with C)
/// 3. Credits (dismissable with C)
/// </summary>
public class GameEndSequenceUI : MonoBehaviour
{
    public static GameEndSequenceUI Instance { get; private set; }
    
    [Header("References")]
    [SerializeField] private Image displayImage;
    
    [Header("Sequence Sprites")]
    [SerializeField] private Sprite manifestoGangsterSprite;
    [SerializeField] private Sprite gameEndSprite;
    [SerializeField] private Sprite creditsSprite;
    
    [Header("Panels to Hide")]
    [SerializeField] private GameObject missionPanel;
    [SerializeField] private GameObject maskTrackerPanel;
    
    private int currentStep = 0;
    private bool isActive = false;
    
    public bool IsActive => isActive;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        // Auto-find Image component if not assigned
        if (displayImage == null)
        {
            displayImage = GetComponent<Image>();
        }
        
        // Hide by default
        if (displayImage != null)
        {
            displayImage.enabled = false;
        }
    }
    
    private void Start()
    {
        // Auto-find panels if not assigned
        if (missionPanel == null)
        {
            missionPanel = GameObject.Find("MissionPanel");
        }
        if (maskTrackerPanel == null)
        {
            maskTrackerPanel = GameObject.Find("MaskTrackerPanel");
        }
    }
    
    private void Update()
    {
        if (!isActive) return;
        
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.cKey.wasPressedThisFrame)
        {
            AdvanceSequence();
        }
    }
    
    /// <summary>
    /// Start the game end sequence
    /// </summary>
    public void StartSequence()
    {
        if (displayImage == null)
        {
            Debug.LogError("[GameEndSequenceUI] No Image component found!");
            return;
        }
        
        isActive = true;
        currentStep = 0;
        
        // Hide UI panels
        if (missionPanel != null)
        {
            missionPanel.SetActive(false);
        }
        if (maskTrackerPanel != null)
        {
            maskTrackerPanel.SetActive(false);
        }
        
        // Play club music
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayClubMusic();
        }
        
        ShowCurrentStep();
        Debug.Log("[GameEndSequenceUI] Game end sequence started. Press C to continue.");
    }
    
    private void ShowCurrentStep()
    {
        Sprite spriteToShow = currentStep switch
        {
            0 => manifestoGangsterSprite,
            1 => gameEndSprite,
            2 => creditsSprite,
            _ => null
        };
        
        if (spriteToShow != null)
        {
            displayImage.sprite = spriteToShow;
            displayImage.SetNativeSize();
            displayImage.enabled = true;
            
            string stepName = currentStep switch
            {
                0 => "Manifesto_Gangster",
                1 => "Game_End",
                2 => "Credits",
                _ => "Unknown"
            };
            Debug.Log($"[GameEndSequenceUI] Showing: {stepName}. Press C to continue.");
        }
        else
        {
            Debug.LogWarning($"[GameEndSequenceUI] No sprite for step {currentStep}!");
            EndSequence();
        }
    }
    
    private void AdvanceSequence()
    {
        currentStep++;
        
        if (currentStep > 2)
        {
            EndSequence();
        }
        else
        {
            ShowCurrentStep();
        }
    }
    
    private void EndSequence()
    {
        isActive = false;
        
        if (displayImage != null)
        {
            displayImage.enabled = false;
        }
        
        Debug.Log("[GameEndSequenceUI] Game end sequence complete. Thanks for playing!");
        
        // Optionally: Return to main menu, quit game, or just leave player in the club
        // For now, just show UI again
        if (missionPanel != null)
        {
            missionPanel.SetActive(true);
        }
        if (maskTrackerPanel != null)
        {
            maskTrackerPanel.SetActive(true);
        }
    }
}
