using UnityEngine;
using System.Collections;

/// <summary>
/// Point of No Return - when player touches this trigger, 
/// an invisible wall activates after a delay, giving the player time to pass through.
/// Mission and mask set switch happen quickly, blocker activates later.
/// </summary>
public class PointOfNoReturn : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject exitBlocker;  // Invisible wall to block return
    
    [Header("Settings")]
    [SerializeField] private float uiUpdateDelay = 0.25f;     // Delay before mission/mask update
    [SerializeField] private float blockerActivationDelay = 2.5f;  // Delay before blocker activates
    [SerializeField] private bool hasTriggered = false;
    
    private void Awake()
    {
        // Make sure blocker is disabled at start
        if (exitBlocker != null)
        {
            exitBlocker.SetActive(false);
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Only trigger for player
        if (!other.CompareTag("Player")) return;
        
        // Only trigger once
        if (hasTriggered) return;
        
        hasTriggered = true;
        
        Debug.Log("[PointOfNoReturn] Player entered point of no return!");
        
        // Start both coroutines
        StartCoroutine(UpdateUIAfterDelay());
        StartCoroutine(ActivateBlockerAfterDelay());
    }
    
    private IEnumerator UpdateUIAfterDelay()
    {
        yield return new WaitForSeconds(uiUpdateDelay);
        
        // Update Mission to 3
        if (MissionPanelUI.Instance != null)
        {
            MissionPanelUI.Instance.SetMission(3);
            Debug.Log("[PointOfNoReturn] Mission set to 3");
        }
        
        // Trigger PoNR music (nivo 3)
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayPoNRMusic();
        }
        
        // Reset MaskInventory counter and set to mask set 2
        if (MaskInventory.Instance != null)
        {
            MaskInventory.Instance.ResetInventory();
            MaskInventory.Instance.SetMaskSetNumber(2);
            Debug.Log("[PointOfNoReturn] MaskInventory reset for mask set 2");
        }
        
        // Switch to Mask Set 2 UI
        if (MaskTrackerUI.Instance != null)
        {
            MaskTrackerUI.Instance.SetMaskSet(2);
            Debug.Log("[PointOfNoReturn] Switched to mask set 2");
        }
    }
    
    private IEnumerator ActivateBlockerAfterDelay()
    {
        yield return new WaitForSeconds(blockerActivationDelay);
        
        // Activate the exit blocker
        if (exitBlocker != null)
        {
            exitBlocker.SetActive(true);
            Debug.Log("[PointOfNoReturn] Exit blocked!");
        }
        else
        {
            Debug.LogError("[PointOfNoReturn] Exit blocker not assigned!");
        }
    }
    
    /// <summary>
    /// Reset the point of no return (for testing or game restart)
    /// </summary>
    public void Reset()
    {
        hasTriggered = false;
        StopAllCoroutines();
        if (exitBlocker != null)
        {
            exitBlocker.SetActive(false);
        }
    }
}
