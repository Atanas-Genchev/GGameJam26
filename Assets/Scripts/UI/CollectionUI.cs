using UnityEngine;
using TMPro;

/// <summary>
/// Simple UI to display mask collection progress.
/// </summary>
public class CollectionUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MaskInventory inventory;
    [SerializeField] private TextMeshProUGUI collectionText;
    
    [Header("Display Settings")]
    [SerializeField] private string format = "Mask Pieces: {0}/{1}";
    
    private void Start()
    {
        if (inventory != null)
        {
            inventory.OnPieceCollected += UpdateDisplay;
            UpdateDisplay(inventory.CollectedPieces, inventory.TotalPiecesRequired);
        }
        else
        {
            Debug.LogWarning("[CollectionUI] Inventory not assigned!");
        }
    }
    
    private void OnDestroy()
    {
        if (inventory != null)
        {
            inventory.OnPieceCollected -= UpdateDisplay;
        }
    }
    
    private void UpdateDisplay(int collected, int total)
    {
        if (collectionText != null)
        {
            collectionText.text = string.Format(format, collected, total);
        }
    }
}
