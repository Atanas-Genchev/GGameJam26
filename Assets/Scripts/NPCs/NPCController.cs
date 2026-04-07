using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

/// <summary>
/// NPC Controller that handles proximity-based dialogue triggering.
/// Supports multiple dialogue lines and throwing objects on specific lines.
/// </summary>
public class NPCController : MonoBehaviour
{
    [Header("Dialogue - Default (No Mask)")]
    [SerializeField] private NPCDialogueData dialogueData;
    [SerializeField] private bool useRandomLineFromDefault = false;
    
    [Header("Dialogue - With Mask Equipped")]
    [SerializeField] private NPCDialogueData maskDialogueData;
    [SerializeField] private bool useRandomLineFromMask = false;
    [SerializeField] private bool hasMaskDependentDialogue = false;
    [Tooltip("Which mask set this NPC responds to (1 = Worker Mask, 2 = Gangster Mask)")]
    [SerializeField] private int requiredMaskSet = 1;
    
    [Header("Trigger Settings")]
    [SerializeField] private float triggerRadius = 5f;
    [SerializeField] private float dismissRadius = 25f;
    [SerializeField] private string playerTag = "Player";
    
    [Header("Behavior")]
    [SerializeField] private bool rotateToFacePlayer = false;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private bool onlyRotateWhenTriggered = true;
    
    [Header("Speech Bubble")]
    [SerializeField] private SpeechBubble speechBubblePrefab;
    [SerializeField] private Vector3 bubbleOffset = new Vector3(0, 2.5f, 0);
    
    [Header("Voice Sounds")]
    [SerializeField] private AudioClip[] voiceSounds;
    [SerializeField] private float voiceVolume = 0.8f;
    
    [Header("Throw Settings")]
    [SerializeField] private GameObject throwObjectPrefab;
    [SerializeField] private Transform throwPoint;
    [SerializeField] private float throwForce = 5f;
    [SerializeField] private float throwUpwardForce = 3f;
    [SerializeField] private AudioClip throwSound;
    
    private SpeechBubble activeBubble;
    private Transform playerTransform;
    private bool hasTriggered = false;
    private bool isBubbleActive = false;
    private int currentLineIndex = 0;
    private bool dialogueComplete = false;
    private bool maskEquipped = false;
    private bool maskDialogueComplete = false;
    private AudioSource audioSource;
    
    public bool HasTriggered => hasTriggered;
    public bool IsBubbleActive => isBubbleActive;
    public bool DialogueComplete => dialogueComplete;
    
private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player != null)
        {
            playerTransform = player.transform;
        }
        
        if (speechBubblePrefab != null)
        {
            activeBubble = Instantiate(speechBubblePrefab, transform.position + bubbleOffset, Quaternion.identity);
            activeBubble.SetOffset(bubbleOffset);
        }
        
        // Get or add AudioSource for voice
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f;
            audioSource.minDistance = 2f;
            audioSource.maxDistance = 15f;
        }
        
        // Subscribe to mask equipped event
        if (MaskInventory.Instance != null)
        {
            MaskInventory.Instance.OnMaskEquipped += OnMaskEquipped;
        }
    }

private void OnDestroy()
    {
        // Unsubscribe from mask equipped event
        if (MaskInventory.Instance != null)
        {
            MaskInventory.Instance.OnMaskEquipped -= OnMaskEquipped;
        }
    }
    
private void OnMaskEquipped(MaskData maskData)
    {
        if (!hasMaskDependentDialogue) return;
        if (maskDialogueData == null) return;
        
        // Check if this is the correct mask set for this NPC
        int currentMaskSet = MaskInventory.Instance != null ? GetCurrentMaskSetFromInventory() : 1;
        
        if (currentMaskSet != requiredMaskSet)
        {
            Debug.Log($"[NPCController] {gameObject.name} - Mask {currentMaskSet} equipped, but NPC requires mask set {requiredMaskSet}. Ignoring.");
            return;
        }
        
        maskEquipped = true;
        
        // Reset dialogue state so player can trigger new dialogue
        hasTriggered = false;
        dialogueComplete = false;
        currentLineIndex = 0;
        
        Debug.Log($"[NPCController] {gameObject.name} - Mask {currentMaskSet} equipped, new dialogue available");
    }
    
    private int GetCurrentMaskSetFromInventory()
    {
        // Get the mask set number from MaskTrackerUI since MaskInventory tracks it there
        if (MaskTrackerUI.Instance != null)
        {
            return MaskTrackerUI.Instance.CurrentMaskSet;
        }
        return 1;
    }

    
    private void Update()
    {
        if (playerTransform == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        
        // Check for proximity trigger
        if (!hasTriggered && distanceToPlayer <= triggerRadius)
        {
            TriggerDialogue();
        }
        
        // Check for auto-dismiss due to distance
        if (isBubbleActive && distanceToPlayer > dismissRadius)
        {
            AdvanceDialogue();
        }
        
        // Check for C key to advance dialogue
        if (isBubbleActive)
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.cKey.wasPressedThisFrame)
            {
                AdvanceDialogue();
            }
        }
        
        // Handle rotation to face player
        if (rotateToFacePlayer)
        {
            bool shouldRotate = !onlyRotateWhenTriggered || (onlyRotateWhenTriggered && isBubbleActive);
            
            if (shouldRotate && distanceToPlayer <= dismissRadius)
            {
                RotateTowardsPlayer();
            }
        }
    }
    
private void TriggerDialogue()
    {
        if (hasTriggered) return;
        
        // Determine which dialogue data to use
        NPCDialogueData activeDialogue = GetActiveDialogueData();
        
        if (activeDialogue == null)
        {
            Debug.LogWarning($"[NPCController] {gameObject.name} has no dialogue data assigned!");
            return;
        }
        
        hasTriggered = true;
        currentLineIndex = 0;
        
        // Handle random line selection
        bool useRandom = (maskEquipped && hasMaskDependentDialogue) ? useRandomLineFromMask : useRandomLineFromDefault;
        if (useRandom && activeDialogue.dialogueLines.Length > 1)
        {
            currentLineIndex = UnityEngine.Random.Range(0, activeDialogue.dialogueLines.Length);
            Debug.Log($"[NPCController] {gameObject.name} randomly selected line {currentLineIndex}");
        }
        
        Debug.Log($"[NPCController] {gameObject.name} triggered dialogue (maskEquipped: {maskEquipped})");
        
        ShowCurrentLine();
    }
    
private void ShowCurrentLine()
    {
        NPCDialogueData activeDialogue = GetActiveDialogueData();
        
        if (activeDialogue == null || activeDialogue.dialogueLines == null) return;
        
        // For random selection mode, we only show one line
        bool useRandom = (maskEquipped && hasMaskDependentDialogue) ? useRandomLineFromMask : useRandomLineFromDefault;
        
        if (useRandom)
        {
            // Show single random line then end
            if (dialogueComplete || (maskEquipped && maskDialogueComplete))
            {
                EndDialogue();
                return;
            }
        }
        else
        {
            // Sequential mode - check bounds
            if (currentLineIndex >= activeDialogue.dialogueLines.Length)
            {
                EndDialogue();
                return;
            }
        }
        
        string currentLine = activeDialogue.dialogueLines[currentLineIndex];
        
        isBubbleActive = true;
        
        // Play random voice sound
        PlayRandomVoice();
        
        if (activeBubble != null)
        {
            activeBubble.Show(
                currentLine,
                activeDialogue.npcName,
                transform,
                null
            );
        }
        
        // Check if we should throw on this line (only for default dialogue)
        if (!maskEquipped && activeDialogue.throwOnLineIndex == currentLineIndex)
        {
            StartCoroutine(ThrowObjectDelayed(0.3f));
        }
        
        Debug.Log($"[NPCController] {gameObject.name} showing line {currentLineIndex + 1}/{activeDialogue.dialogueLines.Length}: {currentLine}");
    }
    
    private void PlayRandomVoice()
    {
        if (voiceSounds == null || voiceSounds.Length == 0) return;
        if (audioSource == null) return;
        
        AudioClip clip = voiceSounds[Random.Range(0, voiceSounds.Length)];
        if (clip != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(clip, voiceVolume);
        }
    }
    
private void AdvanceDialogue()
    {
        if (!isBubbleActive) return;
        
        // Hide current bubble
        if (activeBubble != null)
        {
            activeBubble.Hide();
        }
        
        isBubbleActive = false;
        
        // Check if using random mode (single line only)
        bool useRandom = (maskEquipped && hasMaskDependentDialogue) ? useRandomLineFromMask : useRandomLineFromDefault;
        
        if (useRandom)
        {
            // Random mode - end after single line
            EndDialogue();
            return;
        }
        
        // Sequential mode - advance to next line
        currentLineIndex++;
        
        NPCDialogueData activeDialogue = GetActiveDialogueData();
        
        if (activeDialogue != null && currentLineIndex < activeDialogue.dialogueLines.Length)
        {
            // Small delay before showing next line
            StartCoroutine(ShowNextLineDelayed(0.2f));
        }
        else
        {
            EndDialogue();
        }
    }
    
    private IEnumerator ShowNextLineDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        ShowCurrentLine();
    }
    
private void EndDialogue()
    {
        isBubbleActive = false;
        
        // Track completion separately for default vs mask dialogue
        if (maskEquipped && hasMaskDependentDialogue)
        {
            maskDialogueComplete = true;
        }
        else
        {
            dialogueComplete = true;
        }
        
        if (activeBubble != null)
        {
            activeBubble.Hide();
        }
        
        Debug.Log($"[NPCController] {gameObject.name} dialogue complete (maskEquipped: {maskEquipped})");
    }

private NPCDialogueData GetActiveDialogueData()
    {
        if (maskEquipped && hasMaskDependentDialogue && maskDialogueData != null)
        {
            return maskDialogueData;
        }
        return dialogueData;
    }

    
    private IEnumerator ThrowObjectDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        ThrowObject();
    }
    
    private void ThrowObject()
    {
        if (throwObjectPrefab == null)
        {
            Debug.LogWarning($"[NPCController] {gameObject.name} has no throw object prefab assigned!");
            return;
        }
        
        // Spawn position - use throw point or default to in front of NPC
        Vector3 spawnPos = throwPoint != null ? throwPoint.position : transform.position + Vector3.up * 1.5f + transform.forward * 0.5f;
        
        // Create the thrown object
        GameObject thrownObj = Instantiate(throwObjectPrefab, spawnPos, Quaternion.identity);
        thrownObj.name = "MaskPiece_Papka";
        
        // Add rigidbody if not present
        Rigidbody rb = thrownObj.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = thrownObj.AddComponent<Rigidbody>();
        }
        
        // Configure rigidbody to stop quickly after landing
        rb.mass = 0.5f;
        rb.linearDamping = 5f;   // High drag to slow down quickly
        rb.angularDamping = 5f;  // High angular drag to stop rolling
        
        // Drop just in front of NPC (uses throw point's forward direction if available)
        // throwForce controls horizontal distance, throwUpwardForce controls arc height
        Vector3 dropDirection = throwPoint != null ? throwPoint.forward : transform.forward;
        
        // Apply reduced force (5% of original for distance, 70% for speed)
        Vector3 force = (dropDirection * (throwForce * 0.05f) + Vector3.up * (throwUpwardForce * 0.7f));
        rb.AddForce(force, ForceMode.Impulse);
        
        // Add minimal rotation for visual effect
        rb.AddTorque(Random.insideUnitSphere * 0.5f, ForceMode.Impulse);
        
        // Freeze the object after it lands
        StartCoroutine(FreezeAfterLanding(rb, thrownObj.transform));
        
        // Play throw sound
        if (throwSound != null)
        {
            AudioSource.PlayClipAtPoint(throwSound, spawnPos);
        }
        
        Debug.Log($"[NPCController] {gameObject.name} dropped object in front!");
        
        // Remove rigidbody after landing
        StartCoroutine(RemoveRigidbodyAfterDelay(rb, 3f));
    }
    
    private IEnumerator RemoveRigidbodyAfterDelay(Rigidbody rb, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (rb != null)
        {
            Destroy(rb);
        }
    }
    
    private IEnumerator FreezeAfterLanding(Rigidbody rb, Transform objTransform)
    {
        // Wait for object to mostly stop moving (check velocity)
        float timeout = 2f;
        float elapsed = 0f;
        
        while (elapsed < timeout && rb != null)
        {
            // Check if velocity is low enough to consider it "landed"
            if (rb.linearVelocity.magnitude < 0.1f)
            {
                // Freeze it in place
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
                Debug.Log("[NPCController] Thrown object landed and frozen.");
                yield break;
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Timeout - force freeze anyway
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }
    }
    
    private void RotateTowardsPlayer()
    {
        if (playerTransform == null) return;
        
        Vector3 directionToPlayer = playerTransform.position - transform.position;
        directionToPlayer.y = 0;
        
        if (directionToPlayer.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
    
public void ResetNPC()
    {
        hasTriggered = false;
        dialogueComplete = false;
        maskDialogueComplete = false;
        currentLineIndex = 0;
        
        if (activeBubble != null)
        {
            activeBubble.Hide();
        }
        isBubbleActive = false;
    }
    
    /// <summary>
    /// Reset only the default dialogue state (used when crossing PoNR)
    /// </summary>
    public void ResetForNewArea()
    {
        hasTriggered = false;
        dialogueComplete = false;
        maskDialogueComplete = false;
        maskEquipped = false;
        currentLineIndex = 0;
        
        if (activeBubble != null)
        {
            activeBubble.Hide();
        }
        isBubbleActive = false;
        
        Debug.Log($"[NPCController] {gameObject.name} reset for new area");
    }
    
    public void SetDialogueData(NPCDialogueData data)
    {
        dialogueData = data;
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, triggerRadius);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, dismissRadius);
        
        // Draw throw point
        if (throwPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(throwPoint.position, 0.2f);
        }
    }
}
