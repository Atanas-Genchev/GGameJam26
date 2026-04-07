using UnityEngine;
using System;

/// <summary>
/// Manages the player's collected mask pieces and equipped mask.
/// Attach to the Player GameObject.
/// </summary>
public class MaskInventory : MonoBehaviour
{
    public static MaskInventory Instance { get; private set; }
    
    [Header("Collection Settings")]
    [SerializeField] private int totalPiecesRequired = 3;
    
    [Header("Current Mask Being Collected")]
    [SerializeField] private MaskData currentMaskData;
    
    [Header("Pickup Sounds")]
    [Tooltip("Sounds to play when collecting pieces 1, 2, 3, etc. Array index matches piece number - 1")]
    [SerializeField] private AudioClip[] pickupSequenceSounds;
    
    private int collectedPieces = 0;
    private bool canEquipMask = false;
    private MaskData equippedMask = null;
    
    [Header("Mask Names")]
    [SerializeField] private string mask1Name = "Worker Mask";
    [SerializeField] private string mask2Name = "Gangster Mask";
    private int currentMaskSetNumber = 1;

    
    /// <summary>
    /// Event fired when a piece is collected. Passes (currentCount, totalRequired)
    /// </summary>
    public event Action<int, int> OnPieceCollected;
    
    /// <summary>
    /// Event fired when all pieces are collected (ready to equip)
    /// </summary>
    public event Action<MaskData> OnAllPiecesCollected;
    
    /// <summary>
    /// Event fired when a mask is equipped
    /// </summary>
    public event Action<MaskData> OnMaskEquipped;
    
    public int CollectedPieces => collectedPieces;
    public int TotalPiecesRequired => totalPiecesRequired;
    public bool HasAllPieces => collectedPieces >= totalPiecesRequired;
    public bool CanEquipMask => canEquipMask;
    public MaskData EquippedMask => equippedMask;
    public MaskData CurrentMaskData => currentMaskData;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }
    
    /// <summary>
    /// Call this when player picks up a mask piece
    /// </summary>
public void CollectPiece()
    {
        // Play pickup sound for this piece (if available)
        if (pickupSequenceSounds != null && collectedPieces < pickupSequenceSounds.Length)
        {
            AudioClip clip = pickupSequenceSounds[collectedPieces];
            if (clip != null)
            {
                AudioSource.PlayClipAtPoint(clip, transform.position);
            }
        }
        
        collectedPieces++;
        Debug.Log($"[MaskInventory] Collected piece {collectedPieces}/{totalPiecesRequired}");
        
        OnPieceCollected?.Invoke(collectedPieces, totalPiecesRequired);
        
        if (HasAllPieces && !canEquipMask)
        {
            canEquipMask = true;
            Debug.Log("[MaskInventory] All pieces collected!");
            OnAllPiecesCollected?.Invoke(currentMaskData);
        }
    }

    /// <summary>
    /// Equip the current mask. Called by SuccessScreenUI when dismissed.
    /// </summary>
public void EquipMask()
    {
        if (!canEquipMask) return;

        equippedMask = currentMaskData;
        canEquipMask = false;

        // Get mask name based on current mask set
        string maskName;
        if (equippedMask != null && !string.IsNullOrEmpty(equippedMask.maskName))
        {
            maskName = equippedMask.maskName;
        }
        else
        {
            maskName = currentMaskSetNumber == 1 ? mask1Name : mask2Name;
        }
        
        Debug.Log($"[MaskInventory] Equipped: {maskName}");

        // Play mask music based on mask set
        if (MusicManager.Instance != null)
        {
            if (currentMaskSetNumber == 1)
            {
                MusicManager.Instance.PlayMask1Music();
            }
            // Mask 2 music is handled by PoNR trigger
        }

        // Fire event for GameManager to handle world changes
        OnMaskEquipped?.Invoke(equippedMask);
    }

    /// <summary>
    /// Set which mask the player is currently collecting pieces for
    /// </summary>
    public void SetCurrentMask(MaskData maskData)
    {
        currentMaskData = maskData;
    }
    
    /// <summary>
    /// Reset the inventory (for new mask set)
    /// </summary>
public void ResetInventory()
    {
        collectedPieces = 0;
        canEquipMask = false;
        Debug.Log("[MaskInventory] Inventory reset");
    }
    
    /// <summary>
    /// Set how many pieces are required
    /// </summary>
    public void SetRequiredPieces(int count)
    {
        totalPiecesRequired = count;
    }


/// <summary>
    /// Set which mask set number we're on (1 or 2)
    /// </summary>
    public void SetMaskSetNumber(int maskSetNumber)
    {
        currentMaskSetNumber = maskSetNumber;
        Debug.Log($"[MaskInventory] Now collecting mask set {currentMaskSetNumber}");
    }
}
