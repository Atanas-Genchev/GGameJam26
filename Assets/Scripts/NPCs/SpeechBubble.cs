using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// World-space speech bubble that floats above an NPC.
/// Uses a PNG background image (dialog_box_with_button.png) with text overlay.
/// Press C to dismiss.
/// </summary>
public class SpeechBubble : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Canvas bubbleCanvas;
    [SerializeField] private GameObject bubblePanel;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI npcNameText;
    
    [Header("Background Sprite")]
    [SerializeField] private Sprite dialogBoxSprite; // dialog_box_with_button.png
    
    [Header("Settings")]
    [SerializeField] private Vector3 offset = new Vector3(0, 2.5f, 0);
    [SerializeField] private bool faceCamera = true;
    
    private Transform followTarget;
    private Camera mainCamera;
    private bool isShowing = false;
    private System.Action onDismissCallback;
    
    public bool IsShowing => isShowing;
    
    private void Awake()
    {
        mainCamera = Camera.main;
        
        // Set up background image if assigned
        if (backgroundImage != null && dialogBoxSprite != null)
        {
            backgroundImage.sprite = dialogBoxSprite;
            backgroundImage.preserveAspect = true;
        }
        
        // Hide by default
        if (bubblePanel != null)
        {
            bubblePanel.SetActive(false);
        }
    }
    
    private void LateUpdate()
    {
        if (!isShowing) return;
        
        // Follow target
        if (followTarget != null)
        {
            transform.position = followTarget.position + offset;
        }
        
        // Face camera
        if (faceCamera && mainCamera != null)
        {
            transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                           mainCamera.transform.rotation * Vector3.up);
        }
    }
    
    /// <summary>
    /// Show the speech bubble with dialogue
    /// </summary>
    public void Show(string text, string speakerName = "", Transform target = null, System.Action onDismiss = null)
    {
        followTarget = target;
        onDismissCallback = onDismiss;
        
        if (dialogueText != null)
        {
            dialogueText.text = text;
        }
        
        if (npcNameText != null)
        {
            if (!string.IsNullOrEmpty(speakerName))
            {
                npcNameText.text = speakerName;
                npcNameText.gameObject.SetActive(true);
            }
            else
            {
                npcNameText.gameObject.SetActive(false);
            }
        }
        
        if (bubblePanel != null)
        {
            bubblePanel.SetActive(true);
        }
        
        isShowing = true;
        
        // Position immediately
        if (followTarget != null)
        {
            transform.position = followTarget.position + offset;
        }
    }
    
    /// <summary>
    /// Hide the speech bubble
    /// </summary>
    public void Hide()
    {
        if (!isShowing) return;
        
        isShowing = false;
        
        if (bubblePanel != null)
        {
            bubblePanel.SetActive(false);
        }
        
        // Invoke callback
        onDismissCallback?.Invoke();
        onDismissCallback = null;
        followTarget = null;
    }
    
    /// <summary>
    /// Set the vertical offset
    /// </summary>
    public void SetOffset(Vector3 newOffset)
    {
        offset = newOffset;
    }
}
