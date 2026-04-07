using UnityEngine;

/// <summary>
/// ScriptableObject containing dialogue data for an NPC.
/// Supports multiple dialogue lines that cycle through on dismiss.
/// </summary>
[CreateAssetMenu(fileName = "NewNPCDialogue", menuName = "Game/NPC Dialogue Data")]
public class NPCDialogueData : ScriptableObject
{
    [Header("NPC Info")]
    public string npcName = "Stranger";
    
    [Header("Dialogue Lines")]
    [TextArea(2, 5)]
    public string[] dialogueLines = new string[] { "Hello there..." };
    
    [Header("Display Settings")]
    public float bubbleDisplayTime = 0f; // 0 = wait for player input, >0 = auto-dismiss after X seconds
    
    [Header("Actions on Specific Lines")]
    [Tooltip("Which line index triggers the throw action (-1 = none)")]
    public int throwOnLineIndex = -1;
    
    // Legacy support - returns first line
    public string dialogueText => dialogueLines != null && dialogueLines.Length > 0 ? dialogueLines[0] : "";
}
