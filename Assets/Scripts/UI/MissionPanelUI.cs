using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the mission panel UI, switching between mission backgrounds.
/// Listens to MaskInventory.OnMaskEquipped to progress missions.
/// </summary>
public class MissionPanelUI : MonoBehaviour
{
    public static MissionPanelUI Instance { get; private set; }
    
    [Header("References")]
    [SerializeField] private Image missionImage;
    
    [Header("Mission Sprites")]
    [SerializeField] private Sprite mission01Sprite;
    [SerializeField] private Sprite mission02Sprite;
    [SerializeField] private Sprite mission03Sprite;
    [SerializeField] private Sprite mission04Sprite;
    
    [Header("Mission Music")]
    [SerializeField] private AudioClip mission1Music;
    [SerializeField] private AudioClip mission2Music;
    [SerializeField] private AudioClip mission3Music; // Mission_03.png
    
    private int currentMission = 1;
    
    public int CurrentMission => currentMission;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        // Auto-find Image component if not assigned
        if (missionImage == null)
        {
            missionImage = GetComponent<Image>();
        }
    }
    
    private void Start()
    {
        // Subscribe to mask equipped event
        if (MaskInventory.Instance != null)
        {
            MaskInventory.Instance.OnMaskEquipped += OnMaskEquipped;
        }
        
        // Set initial mission
        SetMission(1);
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        if (MaskInventory.Instance != null)
        {
            MaskInventory.Instance.OnMaskEquipped -= OnMaskEquipped;
        }
    }
    
    /// <summary>
    /// Called when player equips a mask - advance to next mission
    /// </summary>
    private void OnMaskEquipped(MaskData maskData)
    {
        Debug.Log($"[MissionPanelUI] Mask equipped! Advancing to next mission.");
        AdvanceMission();
    }
    
    /// <summary>
    /// Set the current mission and update the UI
    /// </summary>
    public void SetMission(int missionNumber)
    {
        currentMission = missionNumber;
        UpdateMissionDisplay();
        Debug.Log($"[MissionPanelUI] Mission set to {currentMission}");
    }
    
    /// <summary>
    /// Advance to the next mission
    /// </summary>
    public void AdvanceMission()
    {
        currentMission++;
        UpdateMissionDisplay();
        Debug.Log($"[MissionPanelUI] Advanced to mission {currentMission}");
    }
    
    /// <summary>
    /// Update the mission panel image based on current mission
    /// </summary>
private void UpdateMissionDisplay()
    {
        if (missionImage == null) return;
        
        Sprite targetSprite = currentMission switch
        {
            1 => mission01Sprite,
            2 => mission02Sprite,
            3 => mission03Sprite,
            4 => mission04Sprite,
            _ => null
        };
        
        if (targetSprite != null)
        {
            missionImage.sprite = targetSprite;
            missionImage.SetNativeSize();
        }
        else
        {
            Debug.LogWarning($"[MissionPanelUI] No sprite assigned for mission {currentMission}");
        }
        
        // Note: Music is now handled by MusicManager based on game events
        // (mask equip, manifesto shown, PoNR crossed, etc.)
    }
    
    private void PlayMissionMusic()
    {
        if (MusicManager.Instance == null) return;
        
        AudioClip targetMusic = currentMission switch
        {
            1 => mission1Music,
            2 => mission2Music,
            3 => mission3Music,
            _ => null
        };
        
        if (targetMusic != null)
        {
            MusicManager.Instance.PlayMusic(targetMusic);
            Debug.Log($"[MissionPanelUI] Playing music for mission {currentMission}");
        }
    }
    
    /// <summary>
    /// Reset to mission 1 (for new game)
    /// </summary>
    public void ResetMissions()
    {
        SetMission(1);
    }
}
