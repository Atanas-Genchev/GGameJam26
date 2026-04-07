using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

/// <summary>
/// Displays manifesto screen when player passes through trigger zone.
/// Press C to dismiss the screen.
/// Hides MissionPanel and MaskTrackerPanel while displayed.
/// </summary>
public class ManifestoScreenUI : MonoBehaviour
{
    public static ManifestoScreenUI Instance { get; private set; }
    
    [Header("References")]
    [SerializeField] private Image manifestoImage;
    [SerializeField] private Sprite manifestoSprite; // Manifesto_Worker.png
    
    [Header("Panels to Hide")]
    [SerializeField] private GameObject missionPanel;
    [SerializeField] private GameObject maskTrackerPanel;
    
    private bool isShowing = false;
    
    public bool IsShowing => isShowing;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        // Auto-find Image component if not assigned
        if (manifestoImage == null)
        {
            manifestoImage = GetComponent<Image>();
        }
        
        // Hide by default
        if (manifestoImage != null)
        {
            manifestoImage.enabled = false;
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
        // Check for C key to dismiss
        if (isShowing)
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.cKey.wasPressedThisFrame)
            {
                Hide();
            }
        }
    }
    
    /// <summary>
    /// Show the manifesto screen
    /// </summary>
    public void Show()
    {
        if (manifestoImage == null)
        {
            Debug.LogError("[ManifestoScreenUI] No Image component found!");
            return;
        }
        
        if (manifestoSprite != null)
        {
            manifestoImage.sprite = manifestoSprite;
        }
        
        manifestoImage.enabled = true;
        isShowing = true;
        
        // Hide UI panels
        if (missionPanel != null)
        {
            missionPanel.SetActive(false);
        }
        if (maskTrackerPanel != null)
        {
            maskTrackerPanel.SetActive(false);
        }
        
        // Trigger office music (nivo 2 + office_ambi)
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayOfficeMusic();
        }
        
        Debug.Log("[ManifestoScreenUI] Manifesto screen displayed. Press C to dismiss.");
    }

/// <summary>
    /// Show the manifesto screen with a custom sprite (for gangster manifesto)
    /// </summary>
    public void ShowWithSprite(Sprite customSprite)
    {
        if (manifestoImage == null)
        {
            Debug.LogError("[ManifestoScreenUI] No Image component found!");
            return;
        }
        
        if (customSprite != null)
        {
            manifestoImage.sprite = customSprite;
        }
        
        manifestoImage.enabled = true;
        isShowing = true;
        
        // Hide UI panels
        if (missionPanel != null)
        {
            missionPanel.SetActive(false);
        }
        if (maskTrackerPanel != null)
        {
            maskTrackerPanel.SetActive(false);
        }
        
        Debug.Log("[ManifestoScreenUI] Custom manifesto screen displayed. Press C to dismiss.");
    }

    
    /// <summary>
    /// Hide the manifesto screen
    /// </summary>
    public void Hide()
    {
        if (manifestoImage != null)
        {
            manifestoImage.enabled = false;
        }
        isShowing = false;
        
        // Show UI panels again
        if (missionPanel != null)
        {
            missionPanel.SetActive(true);
        }
        if (maskTrackerPanel != null)
        {
            maskTrackerPanel.SetActive(true);
        }
        
        Debug.Log("[ManifestoScreenUI] Manifesto screen dismissed.");
    }
}
