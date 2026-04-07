using UnityEngine;

[CreateAssetMenu(fileName = "NewMask", menuName = "Game/Mask Data")]
public class MaskData : ScriptableObject
{
    [Header("Mask Info")]
    public string maskName = "Unknown Mask";
    public string description = "A mysterious mask";

    [Header("Visual")]
    public Sprite maskIcon;
    public Material buildingMaterial;
    public Material groundMaterial;

    [Header("Effects")]
    public Color ambientLightColor = Color.white;
    public Color fogColor = Color.gray;
    public bool enableFog = false;

    // --- НОВО: Добави това тук ---
    [Header("Audio")]
    public AudioClip maskEquipMusic; // Песента, която тръгва, когато сложиш тази маска
    // ----------------------------
}