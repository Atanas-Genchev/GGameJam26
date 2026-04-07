using UnityEngine;

/// <summary>
/// Trigger zone that shows the manifesto screen when player enters.
/// Place this at the location where the doors were.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class ManifestoTrigger : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool triggerOnce = true;  // Only trigger once per game
    
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
        
        // Check if already triggered (if triggerOnce is enabled)
        if (triggerOnce && hasTriggered) return;
        
        hasTriggered = true;
        
        // Show manifesto screen
        if (ManifestoScreenUI.Instance != null)
        {
            ManifestoScreenUI.Instance.Show();
            Debug.Log("[ManifestoTrigger] Player entered trigger zone. Showing manifesto.");
        }
        else
        {
            Debug.LogError("[ManifestoTrigger] ManifestoScreenUI.Instance is null!");
        }
    }
    
    /// <summary>
    /// Reset the trigger so it can fire again
    /// </summary>
    public void ResetTrigger()
    {
        hasTriggered = false;
    }
}
