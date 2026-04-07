using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

/// <summary>
/// A pursuing NPC that chases the player when looked at.
/// Uses smooth teleport-step movement and sequential dialogue.
/// Triggers attack effects when reaching the player.
/// </summary>
public class PursuingNPC : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private float detectionRange = 14f;
    [SerializeField] private float viewAngleThreshold = 30f;
    [SerializeField] private string playerTag = "Player";
    
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float teleportInterval = 0.8f;
    [SerializeField] private float teleportSmoothTime = 0.3f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float rotationSpeed = 8f;
    
    [Header("Dialogue Sequence - Before Attack")]
    [SerializeField] private string[] preAttackDialogues = new string[]
    {
        "Ей… костюмар…",
        "Не е ли малко късно е за бизнес, бе…",
        "Никой с костюм не идва тукa случайно.",
        "Значи или си смел… или си много, много тъп.",
        "Хайде да видим кое от двете."
    };
    [SerializeField] private float dialogueInterval = 2.5f;
    
    [Header("Dialogue - After Attack")]
    [SerializeField] private string postAttackDialogue = "Хех… значи смел. На' ти маската. Считай я за билет за Ада.";
    
    [Header("Speech Bubble")]
    [SerializeField] private SpeechBubble speechBubblePrefab;
    [SerializeField] private Vector3 bubbleOffset = new Vector3(0, 2.5f, 0);
    
    [Header("Attack Effects")]
    [SerializeField] private float screenShakeDuration = 0.5f;
    [SerializeField] private float screenShakeIntensity = 0.3f;
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private AudioClip attackVoice;
    
    [Header("Item Drop")]
    [SerializeField] private GameObject itemToDrop;
    [SerializeField] private float dropForwardOffset = 1f;
    [SerializeField] private float dropHeight = 0.5f;
    [SerializeField] private float dropForce = 0.4f;
    
    [Header("Camera Lock")]
    [SerializeField] private float maxLookAwayAngle = 60f;
    
    [Header("Visual Feedback")]
    [SerializeField] private Color aggroColor = new Color(0.8f, 0.2f, 0.2f);
    
    // State
    private Transform playerTransform;
    private Camera playerCamera;
    private SpeechBubble activeBubble;
    private bool hasBeenLookedAt = false;
    private bool isChasing = false;
    private bool hasAttacked = false;
    private int currentDialoguePhase = 0;
    private float lastTeleportTime;
    private Vector3 teleportTarget;
    private Vector3 velocity;
    private Renderer bodyRenderer;
    private Color originalColor;
    private AudioSource audioSource;
    private bool isCameraLocked = false;
    
    // Dialogue state
    private int currentPreAttackLine = 0;
    private bool waitingForPostAttackDismiss = false;
    private bool dialogueFullyComplete = false;
    private bool hasDroppedItem = false;
    
    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player != null)
        {
            playerTransform = player.transform;
            playerCamera = player.GetComponentInChildren<Camera>();
        }
        
        bodyRenderer = GetComponentInChildren<Renderer>();
        if (bodyRenderer != null)
        {
            originalColor = bodyRenderer.material.color;
        }
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        if (speechBubblePrefab != null)
        {
            activeBubble = Instantiate(speechBubblePrefab, transform.position + bubbleOffset, Quaternion.identity);
            activeBubble.SetOffset(bubbleOffset);
        }
        
        teleportTarget = transform.position;
    }
    
private void Update()
    {
        if (playerTransform == null || playerCamera == null) return;
        if (dialogueFullyComplete) return;
        
        // Handle post-attack dialogue dismissal
        if (waitingForPostAttackDismiss)
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.cKey.wasPressedThisFrame)
            {
                DismissPostAttackDialogue();
            }
            return;
        }
        
        if (hasAttacked) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        
        // Phase 0: Waiting for player to look at us
        if (!hasBeenLookedAt)
        {
            bool playerLookingAtUs = IsPlayerLookingAtUs();
            if (playerLookingAtUs && distanceToPlayer <= detectionRange)
            {
                StartEncounter();
            }
            return;
        }
        
        // Enforce camera lock if active
        if (isCameraLocked)
        {
            EnforceCameraLock();
        }
        
        // Always rotate to face player once encounter started
        RotateTowardsPlayer();
        
        // Handle chasing movement
        if (isChasing)
        {
            HandleTeleportMovement();
            
            if (distanceToPlayer <= attackRange)
            {
                PerformAttack();
            }
        }
    }
    
    private bool IsPlayerLookingAtUs()
    {
        if (playerCamera == null) return false;
        
        Vector3 directionToNPC = (transform.position - playerCamera.transform.position).normalized;
        float angle = Vector3.Angle(playerCamera.transform.forward, directionToNPC);
        
        return angle <= viewAngleThreshold;
    }
    
private void StartEncounter()
    {
        hasBeenLookedAt = true;
        currentPreAttackLine = 0;
        isCameraLocked = true;
        
        Debug.Log($"[PursuingNPC] Player looked at {gameObject.name} - starting encounter");
        
        // Start the dialogue and chase sequence
        StartCoroutine(DialogueAndChaseSequence());
    }
    
private IEnumerator DialogueAndChaseSequence()
    {
        // Show pre-attack dialogues
        for (int i = 0; i < preAttackDialogues.Length; i++)
        {
            // Stop if attack already happened
            if (hasAttacked) yield break;
            
            currentPreAttackLine = i;
            ShowDialogue(preAttackDialogues[i], "???");
            
            // Start chasing after the second line
            if (i == 1 && !isChasing)
            {
                StartChasing();
            }
            
            yield return new WaitForSeconds(dialogueInterval);
        }
        
        // Hide dialogue while waiting for attack (only if attack hasn't happened yet)
        if (!hasAttacked && activeBubble != null)
        {
            activeBubble.Hide();
        }
    }
    
    private void StartChasing()
    {
        isChasing = true;
        teleportTarget = transform.position;
        lastTeleportTime = Time.time;
        
        if (bodyRenderer != null)
        {
            bodyRenderer.material.color = aggroColor;
        }
        
        Debug.Log($"[PursuingNPC] {gameObject.name} started chasing!");
    }
    

    
    private void ShowDialogue(string text, string speakerName)
    {
        if (activeBubble != null)
        {
            activeBubble.Show(text, speakerName, transform, null);
        }
        Debug.Log($"[PursuingNPC] Dialogue: {text}");
    }
    
    private void HandleTeleportMovement()
    {
        transform.position = Vector3.SmoothDamp(transform.position, teleportTarget, ref velocity, teleportSmoothTime);
        
        if (Time.time >= lastTeleportTime + teleportInterval)
        {
            lastTeleportTime = Time.time;
            
            Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
            float stepDistance = moveSpeed * teleportInterval;
            
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            stepDistance = Mathf.Min(stepDistance, distanceToPlayer - attackRange * 0.5f);
            
            if (stepDistance > 0)
            {
                teleportTarget = transform.position + directionToPlayer * stepDistance;
                teleportTarget.y = transform.position.y;
            }
        }
    }
    
    private void RotateTowardsPlayer()
    {
        Vector3 directionToPlayer = playerTransform.position - transform.position;
        directionToPlayer.y = 0;
        
        if (directionToPlayer.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
    
    private void EnforceCameraLock()
    {
        if (playerTransform == null || playerCamera == null) return;
        
        Vector3 directionToNPC = (transform.position - playerCamera.transform.position).normalized;
        
        Vector3 playerLookDir = playerCamera.transform.forward;
        playerLookDir.y = 0;
        playerLookDir.Normalize();
        
        Vector3 npcDirFlat = directionToNPC;
        npcDirFlat.y = 0;
        npcDirFlat.Normalize();
        
        float angleFromNPC = Vector3.SignedAngle(npcDirFlat, playerLookDir, Vector3.up);
        
        if (Mathf.Abs(angleFromNPC) > maxLookAwayAngle)
        {
            float clampedAngle = Mathf.Sign(angleFromNPC) * maxLookAwayAngle;
            Vector3 clampedDir = Quaternion.Euler(0, clampedAngle, 0) * npcDirFlat;
            
            Quaternion targetRotation = Quaternion.LookRotation(clampedDir);
            playerTransform.rotation = Quaternion.Euler(0, targetRotation.eulerAngles.y, 0);
        }
    }
    
private void PerformAttack()
    {
        hasAttacked = true;
        isChasing = false;
        isCameraLocked = false;
        
        // Stop dialogue coroutine immediately
        StopAllCoroutines();
        
        Debug.Log($"[PursuingNPC] {gameObject.name} attacked player!");
        
        // Play attack sound effect
        if (attackSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(attackSound);
        }
        
        // Play attack voice (gangster scream)
        if (attackVoice != null && audioSource != null)
        {
            audioSource.PlayOneShot(attackVoice, 1.2f);
        }
        
        StartCoroutine(ScreenShake());
        
        // Reset body color
        if (bodyRenderer != null)
        {
            bodyRenderer.material.color = originalColor;
        }
        
        // Show post-attack dialogue and drop item simultaneously
        ShowDialogue(postAttackDialogue, "???");
        DropItem();
        
        waitingForPostAttackDismiss = true;
        Debug.Log("[PursuingNPC] Waiting for player to dismiss post-attack dialogue (C key)");
    }
    
    private IEnumerator ScreenShake()
    {
        if (playerCamera == null) yield break;
        
        Vector3 originalPos = playerCamera.transform.localPosition;
        float elapsed = 0f;
        
        while (elapsed < screenShakeDuration)
        {
            float x = Random.Range(-1f, 1f) * screenShakeIntensity;
            float y = Random.Range(-1f, 1f) * screenShakeIntensity;
            
            playerCamera.transform.localPosition = originalPos + new Vector3(x, y, 0);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        playerCamera.transform.localPosition = originalPos;
    }
    
public void ResetNPC()
    {
        StopAllCoroutines();
        hasBeenLookedAt = false;
        isChasing = false;
        hasAttacked = false;
        isCameraLocked = false;
        currentPreAttackLine = 0;
        waitingForPostAttackDismiss = false;
        dialogueFullyComplete = false;
        // Note: Don't reset hasDroppedItem - item only drops once
        
        if (bodyRenderer != null)
        {
            bodyRenderer.material.color = originalColor;
        }
        
        if (activeBubble != null)
        {
            activeBubble.Hide();
        }
    }
    
private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }


private void DropItem()
    {
        if (itemToDrop == null || hasDroppedItem) return;
        
        hasDroppedItem = true;
        
        // Calculate drop position in front of NPC
        Vector3 dropPosition = transform.position + transform.forward * dropForwardOffset + Vector3.up * dropHeight;
        
        // Activate and position the item
        itemToDrop.transform.position = dropPosition;
        itemToDrop.transform.rotation = Quaternion.identity;
        itemToDrop.SetActive(true);
        
        Debug.Log($"[PursuingNPC] Dropped item: {itemToDrop.name} at {dropPosition}");
        
        // Add physics
        Rigidbody rb = itemToDrop.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = itemToDrop.AddComponent<Rigidbody>();
            rb.mass = 0.5f;
        }
        
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        
        Vector3 force = (transform.forward * 2f + Vector3.up * 3f) * dropForce;
        rb.AddForce(force, ForceMode.Impulse);
        
        // Remove rigidbody after settling
        StartCoroutine(RemoveItemRigidbodyAfterDelay(rb, 2f));
    }
    
    private IEnumerator RemoveItemRigidbodyAfterDelay(Rigidbody rb, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (rb != null)
        {
            Destroy(rb);
        }
    }
    
    private void DismissPostAttackDialogue()
    {
        waitingForPostAttackDismiss = false;
        dialogueFullyComplete = true;
        
        if (activeBubble != null)
        {
            activeBubble.Hide();
        }
        
        Debug.Log("[PursuingNPC] Post-attack dialogue dismissed. Encounter complete.");
    }
}
