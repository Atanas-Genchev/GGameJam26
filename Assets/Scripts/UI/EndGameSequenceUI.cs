using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

/// <summary>
/// Handles the end game sequence - displays a series of images in order.
/// Each image is dismissed with C key, then the next one shows.
/// Sequence: Manifesto_Gangster -> Game_End -> Credits
/// </summary>
public class EndGameSequenceUI : MonoBehaviour
{
    public static EndGameSequenceUI Instance { get; private set; }
    
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
    private Sprite[] sequenceSprites;
    
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
        // Build the sequence array
        sequenceSprites = new Sprite[] { manifestoGangsterSprite, gameEndSprite, creditsSprite };
        
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
        if (isActive)
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.cKey.wasPressedThisFrame)
            {
                AdvanceSequence();
            }
        }
    }
    
    /// <summary>
    /// Start the end game sequence
    /// </summary>
    public void StartSequence()
    {
        if (displayImage == null)
        {
            Debug.LogError("[EndGameSequenceUI] No Image component found!");
            return;
        }
        
        currentStep = 0;
        isActive = true;
        
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
        
        // Set mission to 4
        if (MissionPanelUI.Instance != null)
        {
            MissionPanelUI.Instance.SetMission(4);
        }
        
        ShowCurrentStep();
        Debug.Log("[EndGameSequenceUI] End game sequence started!");
    }
    
    private void ShowCurrentStep()
    {
        if (currentStep >= sequenceSprites.Length)
        {
            EndSequence();
            return;
        }
        
        Sprite currentSprite = sequenceSprites[currentStep];
        if (currentSprite != null)
        {
            displayImage.sprite = currentSprite;
            displayImage.SetNativeSize();
            displayImage.enabled = true;
            
            string stepName = currentStep switch
            {
                0 => "Manifesto Gangster",
                1 => "Game End",
                2 => "Credits",
                _ => "Unknown"
            };
            Debug.Log($"[EndGameSequenceUI] Showing: {stepName}. Press C to continue.");
        }
        else
        {
            Debug.LogWarning($"[EndGameSequenceUI] No sprite for step {currentStep}!");
            AdvanceSequence();
        }
    }
    
    private void AdvanceSequence()
    {
        currentStep++;
        ShowCurrentStep();
    }
    
    private void EndSequence()
    {
        isActive = false;
        displayImage.enabled = false;
        
        Debug.Log("[EndGameSequenceUI] End game sequence complete!");
        
        // Optionally: Show panels again or keep them hidden
        // For now, keep them hidden since game is over
    }
}
