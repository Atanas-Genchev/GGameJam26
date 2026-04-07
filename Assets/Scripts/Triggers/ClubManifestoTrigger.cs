using UnityEngine;

/// <summary>
/// Trigger zone inside the gangster club that starts the game end sequence.
/// Place 2-3 meters inside the club entrance.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class ClubManifestoTrigger : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool triggerOnce = true;
    
    private bool hasTriggered = false;
    private BoxCollider triggerCollider;
    
    private void Awake()
    {
        triggerCollider = GetComponent<BoxCollider>();
        triggerCollider.isTrigger = true;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Only trigger for player
        if (!other.CompareTag("Player")) return;
        
        // Check if already triggered
        if (triggerOnce && hasTriggered) return;
        
        hasTriggered = true;
        
        Debug.Log("[ClubManifestoTrigger] Player entered the club! Starting game end sequence.");
        
        // Update mission to 4
        if (MissionPanelUI.Instance != null)
        {
            MissionPanelUI.Instance.SetMission(4);
        }
        
        // Start the game end sequence
        if (GameEndSequenceUI.Instance != null)
        {
            GameEndSequenceUI.Instance.StartSequence();
        }
        else
        {
            Debug.LogError("[ClubManifestoTrigger] GameEndSequenceUI.Instance is null!");
        }
    }
    
    /// <summary>
    /// Reset the trigger
    /// </summary>
    public void ResetTrigger()
    {
        hasTriggered = false;
    }
}
