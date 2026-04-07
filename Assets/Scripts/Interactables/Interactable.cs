using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

/// <summary>
/// Base class for any interactable object in the world.
/// Handles proximity detection and E key interaction.
/// </summary>
public class Interactable : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] protected string interactionPrompt = "Press E to interact";
    [SerializeField] protected string playerTag = "Player";
    [SerializeField] protected bool singleUse = false;
    
    [Header("Events")]
    [SerializeField] protected UnityEvent onInteract;
    
    protected bool playerInRange = false;
    protected bool hasBeenUsed = false;
    protected GameObject playerObject;
    
    protected virtual void Update()
    {
        if (playerInRange && !hasBeenUsed)
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.eKey.wasPressedThisFrame)
            {
                Interact();
            }
        }
    }
    
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (hasBeenUsed && singleUse) return;
        
        if (other.CompareTag(playerTag))
        {
            playerInRange = true;
            playerObject = other.gameObject;
            
            InteractionPromptUI.Instance?.ShowPrompt(interactionPrompt);
        }
    }
    
    protected virtual void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = false;
            playerObject = null;
            
            InteractionPromptUI.Instance?.HidePrompt();
        }
    }
    
    /// <summary>
    /// Called when player interacts with this object
    /// </summary>
    protected virtual void Interact()
    {
        Debug.Log($"[Interactable] {gameObject.name} interacted with");
        
        if (singleUse)
        {
            hasBeenUsed = true;
            InteractionPromptUI.Instance?.HidePrompt();
        }
        
        onInteract?.Invoke();
    }
    
    /// <summary>
    /// Reset the interactable (for reuse)
    /// </summary>
    public virtual void ResetInteractable()
    {
        hasBeenUsed = false;
    }
}
