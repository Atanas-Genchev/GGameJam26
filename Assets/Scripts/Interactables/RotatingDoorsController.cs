using UnityEngine;

/// <summary>
/// Destroys the doors when the player equips the mask.
/// Listens to MaskInventory.OnMaskEquipped event.
/// </summary>
public class RotatingDoorsController : MonoBehaviour
{
    [Header("Optional")]
    [SerializeField] private AudioClip doorOpenSound;
    
    private void Start()
    {
        // Subscribe to mask equipped event
        if (MaskInventory.Instance != null)
        {
            MaskInventory.Instance.OnMaskEquipped += OnMaskEquipped;
        }
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
    /// Called when player equips a mask - destroy the doors
    /// </summary>
    private void OnMaskEquipped(MaskData maskData)
    {
        Debug.Log("[RotatingDoorsController] Mask equipped! Destroying doors...");
        
        // Play sound if assigned
        if (doorOpenSound != null)
        {
            AudioSource.PlayClipAtPoint(doorOpenSound, transform.position);
        }
        
        // Destroy this GameObject
        Destroy(gameObject);
    }
}