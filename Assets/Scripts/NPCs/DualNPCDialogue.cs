using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

/// <summary>
/// Handles alternating dialogue between two NPCs.
/// Player proximity triggers, C key advances, dialogue persists regardless of distance.
/// Can drop an item on the final line.
/// </summary>
public class DualNPCDialogue : MonoBehaviour
{
    [Header("NPC References")]
    [SerializeField] private Transform npc1Transform;
    [SerializeField] private Transform npc2Transform;
    [SerializeField] private string npc1Name = "NPC 1";
    [SerializeField] private string npc2Name = "NPC 2";
    
    [Header("Detection")]
    [SerializeField] private float triggerRange = 3f;
    [SerializeField] private string playerTag = "Player";
    
    [Header("Dialogue Lines")]
    [SerializeField] private List<DialogueLine> dialogueLines = new List<DialogueLine>();
    
    [Header("Speech Bubble")]
    [SerializeField] private SpeechBubble speechBubblePrefab;
    [SerializeField] private Vector3 bubbleOffset = new Vector3(0, 2.5f, 0);
    
    [Header("Voice Sounds")]
    [SerializeField] private AudioClip[] voiceSounds;
    [SerializeField] private float voiceVolume = 0.8f;
    
    [Header("Item Drop (on final line)")]
    [SerializeField] private GameObject itemToDrop;
    [SerializeField] private Transform dropFromNPC; // Which NPC drops the item
    [SerializeField] private float dropForwardOffset = 1f;
    [SerializeField] private float dropHeight = 0.5f;
    [SerializeField] private float dropForce = 0.4f;
    
    [System.Serializable]
    public class DialogueLine
    {
        public int speakerNPC; // 1 or 2
        public string text;
    }
    
    // State
    private Transform playerTransform;
    private SpeechBubble activeBubble;
    private bool hasTriggered = false;
    private bool dialogueActive = false;
    private bool dialogueComplete = false;
    private int currentLineIndex = 0;
    private bool hasDroppedItem = false;
    private AudioSource audioSource;
    
private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player != null)
        {
            playerTransform = player.transform;
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
        
        // Create speech bubble instance
        if (speechBubblePrefab != null)
        {
            activeBubble = Instantiate(speechBubblePrefab, transform.position + bubbleOffset, Quaternion.identity);
            activeBubble.SetOffset(bubbleOffset);
            activeBubble.gameObject.SetActive(false);
        }
        
        // Make sure item is inactive at start
        if (itemToDrop != null)
        {
            itemToDrop.SetActive(false);
        }
    }
    
    private void Update()
    {
        if (playerTransform == null) return;
        if (dialogueComplete) return; // Dialogue finished, waiting for future mask-based dialogue
        
        // Check for trigger
        if (!hasTriggered)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer <= triggerRange)
            {
                StartDialogue();
            }
            return;
        }
        
        // Handle dialogue advancement with C key
        if (dialogueActive)
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.cKey.wasPressedThisFrame)
            {
                AdvanceDialogue();
            }
        }
    }
    
    private void StartDialogue()
    {
        if (dialogueLines.Count == 0) return;
        
        hasTriggered = true;
        dialogueActive = true;
        currentLineIndex = 0;
        
        Debug.Log("[DualNPCDialogue] Dialogue started");
        
        ShowCurrentLine();
    }
    
private void ShowCurrentLine()
    {
        if (currentLineIndex >= dialogueLines.Count)
        {
            EndDialogue();
            return;
        }
        
        DialogueLine line = dialogueLines[currentLineIndex];
        Transform speakerTransform = line.speakerNPC == 1 ? npc1Transform : npc2Transform;
        string speakerName = line.speakerNPC == 1 ? npc1Name : npc2Name;
        
        // Play random voice sound
        PlayRandomVoice();
        
        if (activeBubble != null && speakerTransform != null)
        {
            activeBubble.gameObject.SetActive(true);
            activeBubble.Show(line.text, speakerName, speakerTransform, null);
        }
        
        // Check if this is the final line - drop item
        if (currentLineIndex == dialogueLines.Count - 1 && !hasDroppedItem)
        {
            DropItem();
        }
        
        Debug.Log($"[DualNPCDialogue] {speakerName}: {line.text}");
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
        currentLineIndex++;
        
        if (currentLineIndex >= dialogueLines.Count)
        {
            EndDialogue();
        }
        else
        {
            ShowCurrentLine();
        }
    }
    
    private void EndDialogue()
    {
        dialogueActive = false;
        dialogueComplete = true;
        
        if (activeBubble != null)
        {
            activeBubble.Hide();
            activeBubble.gameObject.SetActive(false);
        }
        
        Debug.Log("[DualNPCDialogue] Dialogue complete");
    }
    
    private void DropItem()
    {
        if (itemToDrop == null || hasDroppedItem) return;
        if (dropFromNPC == null) dropFromNPC = npc2Transform; // Default to NPC2
        
        hasDroppedItem = true;
        
        // Calculate drop position in front of NPC
        Vector3 dropPosition = dropFromNPC.position + dropFromNPC.forward * dropForwardOffset + Vector3.up * dropHeight;
        
        // Activate and position the item
        itemToDrop.transform.position = dropPosition;
        itemToDrop.transform.rotation = Quaternion.identity;
        itemToDrop.SetActive(true);
        
        Debug.Log($"[DualNPCDialogue] Dropped item: {itemToDrop.name} at {dropPosition}");
        
        // Add physics
        Rigidbody rb = itemToDrop.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = itemToDrop.AddComponent<Rigidbody>();
            rb.mass = 0.5f;
        }
        
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        
        Vector3 force = (dropFromNPC.forward * 2f + Vector3.up * 3f) * dropForce;
        rb.AddForce(force, ForceMode.Impulse);
        
        // Remove rigidbody after settling
        StartCoroutine(RemoveRigidbodyAfterDelay(rb, 2f));
    }
    
    private IEnumerator RemoveRigidbodyAfterDelay(Rigidbody rb, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (rb != null)
        {
            Destroy(rb);
        }
    }
    
    /// <summary>
    /// Reset dialogue state (for future mask-based dialogue)
    /// </summary>
    public void ResetDialogue()
    {
        hasTriggered = false;
        dialogueActive = false;
        dialogueComplete = false;
        currentLineIndex = 0;
        // Note: Don't reset hasDroppedItem - item only drops once
    }
    
    /// <summary>
    /// Set new dialogue lines (for mask-based dialogue switching)
    /// </summary>
    public void SetDialogueLines(List<DialogueLine> newLines)
    {
        dialogueLines = newLines;
        ResetDialogue();
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, triggerRange);
    }
}
