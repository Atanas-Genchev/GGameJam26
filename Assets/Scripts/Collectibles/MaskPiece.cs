using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Attach to any collectible mask piece. Handles pickup when player is nearby and presses E.
/// Requires a Collider set as Trigger.
/// </summary>
public class MaskPiece : MonoBehaviour
{
    [Header("Item Type")]
    [Tooltip("The type of collectible for UI tracking: Papka, Riza, or Badge")]
    [SerializeField] private string itemType = "Papka"; // Papka, Riza, or Badge
    
[Header("Pickup Settings")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float interactionRange = 2f;
    
    [Header("Visual Feedback")]
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.25f;
    
    [Header("UI Prompt")]
    [SerializeField] private GameObject promptUI;
    
    [Header("Audio (Optional)")]
    [SerializeField] private AudioClip pickupSound;
    
    private Vector3 startPosition;
    private bool isCollected = false;
    private bool playerInRange = false;
    private GameObject playerObject;
    
    public string ItemType => itemType;
    
    private void Start()
    {
        startPosition = transform.position;
        
        Collider col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            Debug.LogWarning($"[MaskPiece] {gameObject.name} collider should be set as Trigger!");
        }
        
        if (promptUI != null)
        {
            promptUI.SetActive(false);
        }
    }
    
    private void Update()
    {
        if (isCollected) return;
        
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        
        if (playerInRange && playerObject != null)
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.eKey.wasPressedThisFrame)
            {
                Collect(playerObject);
            }
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (isCollected) return;
        
        if (other.CompareTag(playerTag))
        {
            playerInRange = true;
            playerObject = other.gameObject;
            
            if (promptUI != null)
            {
                promptUI.SetActive(true);
            }
            
            InteractionPromptUI.Instance?.ShowPrompt("Press E to collect");
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (isCollected) return;
        
        if (other.CompareTag(playerTag))
        {
            playerInRange = false;
            playerObject = null;
            
            if (promptUI != null)
            {
                promptUI.SetActive(false);
            }
            
            InteractionPromptUI.Instance?.HidePrompt();
        }
    }
    
    private void Collect(GameObject player)
    {
        isCollected = true;
        
        InteractionPromptUI.Instance?.HidePrompt();
        
        // KEY LINE: Notify the MaskTrackerUI about this specific item collection
        MaskTrackerUI.Instance?.CollectItem(itemType);
        
        MaskInventory inventory = player.GetComponent<MaskInventory>();
        if (inventory == null)
        {
            inventory = player.GetComponentInParent<MaskInventory>();
        }
        
        if (inventory != null)
        {
            inventory.CollectPiece();
        }
        else
        {
            Debug.LogError("[MaskPiece] Could not find MaskInventory on player!");
        }
        
        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }
        
        Destroy(gameObject);
    }
}
