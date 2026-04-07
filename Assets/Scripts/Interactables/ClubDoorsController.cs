using UnityEngine;

/// <summary>
/// Controls the gangster club doors - they open when Mask Set 2 is equipped.
/// Listens to MaskInventory.OnMaskEquipped event and checks for Mask Set 2.
/// </summary>
public class ClubDoorsController : MonoBehaviour
{
    [Header("Door References")]
    [SerializeField] private GameObject doorLeft;
    [SerializeField] private GameObject doorRight;
    
    [Header("Settings")]
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float openSpeed = 2f;
    [SerializeField] private AudioClip doorOpenSound;
    
    private bool doorsOpen = false;
    private bool isOpening = false;
    private Quaternion doorLeftClosedRotation;
    private Quaternion doorRightClosedRotation;
    private Quaternion doorLeftOpenRotation;
    private Quaternion doorRightOpenRotation;
    
    private void Start()
    {
        // Store initial rotations
        if (doorLeft != null)
        {
            doorLeftClosedRotation = doorLeft.transform.localRotation;
            doorLeftOpenRotation = doorLeftClosedRotation * Quaternion.Euler(0, openAngle, 0);
        }
        if (doorRight != null)
        {
            doorRightClosedRotation = doorRight.transform.localRotation;
            doorRightOpenRotation = doorRightClosedRotation * Quaternion.Euler(0, -openAngle, 0);
        }
        
        // Subscribe to mask equipped event
        if (MaskInventory.Instance != null)
        {
            MaskInventory.Instance.OnMaskEquipped += OnMaskEquipped;
        }
    }
    
    private void OnDestroy()
    {
        if (MaskInventory.Instance != null)
        {
            MaskInventory.Instance.OnMaskEquipped -= OnMaskEquipped;
        }
    }
    
    private void Update()
    {
        if (isOpening)
        {
            AnimateDoors();
        }
    }
    
    /// <summary>
    /// Called when player equips a mask - check if it's Mask Set 2
    /// </summary>
    private void OnMaskEquipped(MaskData maskData)
    {
        // Check if this is Mask Set 2 (gangster mask)
        int currentMaskSet = 1;
        if (MaskTrackerUI.Instance != null)
        {
            currentMaskSet = MaskTrackerUI.Instance.CurrentMaskSet;
        }
        
        if (currentMaskSet == 2)
        {
            OpenDoors();
        }
    }
    
    /// <summary>
    /// Open the doors
    /// </summary>
    public void OpenDoors()
    {
        if (doorsOpen) return;
        
        doorsOpen = true;
        isOpening = true;
        
        Debug.Log("[ClubDoorsController] Opening club doors!");
        
        // Play sound
        if (doorOpenSound != null)
        {
            AudioSource.PlayClipAtPoint(doorOpenSound, transform.position);
        }
    }
    
    private void AnimateDoors()
    {
        bool leftDone = true;
        bool rightDone = true;
        
        if (doorLeft != null)
        {
            doorLeft.transform.localRotation = Quaternion.Slerp(
                doorLeft.transform.localRotation, 
                doorLeftOpenRotation, 
                openSpeed * Time.deltaTime
            );
            
            if (Quaternion.Angle(doorLeft.transform.localRotation, doorLeftOpenRotation) > 0.5f)
            {
                leftDone = false;
            }
        }
        
        if (doorRight != null)
        {
            doorRight.transform.localRotation = Quaternion.Slerp(
                doorRight.transform.localRotation, 
                doorRightOpenRotation, 
                openSpeed * Time.deltaTime
            );
            
            if (Quaternion.Angle(doorRight.transform.localRotation, doorRightOpenRotation) > 0.5f)
            {
                rightDone = false;
            }
        }
        
        if (leftDone && rightDone)
        {
            isOpening = false;
            Debug.Log("[ClubDoorsController] Doors fully open.");
        }
    }
    
    /// <summary>
    /// Reset doors to closed state
    /// </summary>
    public void ResetDoors()
    {
        doorsOpen = false;
        isOpening = false;
        
        if (doorLeft != null)
        {
            doorLeft.transform.localRotation = doorLeftClosedRotation;
        }
        if (doorRight != null)
        {
            doorRight.transform.localRotation = doorRightClosedRotation;
        }
    }
}
