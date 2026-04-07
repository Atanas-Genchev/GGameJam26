using UnityEngine;
using System.Collections;

public class MaskDispenser : Interactable
{
    [Header("Dispenser Settings")]
    [SerializeField] private GameObject maskPiecePrefab;
    [Tooltip("Alternative: Use an existing scene object instead of instantiating a prefab")]
    [SerializeField] private GameObject existingMaskPiece;

    [SerializeField] private Transform dropPoint;
    [SerializeField] private float dropHeight = 1.5f;
    [SerializeField] private float dropForwardOffset = 1f;
    [SerializeField] private float forceMultiplier = 0.4f;

    [Header("Timing")]
    [Tooltip("Колко секунди след звука на механизма да падне маската?")]
    [SerializeField] private float dropDelay = 1.5f; // Нагласи го в Unity!

    [Header("Audio Clips")]
    [SerializeField] private AudioClip staticLoopSound;   // Постоянния шум (Statika)
    [SerializeField] private AudioClip coinInsertSound;   // Звукът на монетата (Vending drop coin)
    [SerializeField] private AudioClip dispenseChoiceSound; // Механизмът (Vending choice)

    [Header("Audio Settings")]
    [SerializeField][Range(0f, 1f)] private float volume = 1f;
    [SerializeField] private float soundDistance = 15f; // Колко далеч се чува

    private AudioSource loopSource;   // За статиката
    private AudioSource effectSource; // За монетата и избора
    private bool isDispensing = false; // Защита да не спамиш бутона

    private void Awake()
    {
        if (string.IsNullOrEmpty(interactionPrompt))
            interactionPrompt = "Press E to use vending machine";

        // --- Настройка на Аудиото ---

        // 1. Създаваме източник за ЕФЕКТИ (Монета/Избор)
        effectSource = gameObject.AddComponent<AudioSource>();
        effectSource.spatialBlend = 1.0f; // 3D звук
        effectSource.minDistance = 2f;
        effectSource.maxDistance = soundDistance;
        effectSource.playOnAwake = false;

        // 2. Създаваме източник за СТАТИКА (Loop)
        loopSource = gameObject.AddComponent<AudioSource>();
        loopSource.spatialBlend = 1.0f; // 3D звук
        loopSource.minDistance = 2f;
        loopSource.maxDistance = soundDistance;
        loopSource.loop = true;          // Да се повтаря
        loopSource.clip = staticLoopSound;
        loopSource.volume = volume * 0.5f; // Малко по-тиха статика, за да не дразни

        // Пускаме статиката веднага
        if (staticLoopSound != null) loopSource.Play();
    }

    protected override void Interact()
    {
        // Ако вече пуска нещо или е еднократна и използвана, спираме
        if (isDispensing || (singleUse && hasBeenUsed)) return;

        base.Interact(); // Това вика interaction prompt логиката

        // Започваме поредицата от звуци и действия
        StartCoroutine(DispenseSequence());
    }

    private IEnumerator DispenseSequence()
    {
        isDispensing = true;

        // 1. ПУСКАМЕ SFX: COIN DROP
        // Това е първият звук - пускане на монетата
        if (coinInsertSound != null)
        {
            effectSource.PlayOneShot(coinInsertSound, volume);

            // Чакаме звука на монетата да свърши напълно, преди да тръгне машината
            yield return new WaitForSeconds(coinInsertSound.length);
        }
        else
        {
            yield return new WaitForSeconds(0.5f); // Малка пауза по подразбиране
        }

        // 2. ПУСКАМЕ SFX: VENDING CHOICE / MECHANISM
        // Това е звукът на машината, която работи
        if (dispenseChoiceSound != null)
        {
            effectSource.PlayOneShot(dispenseChoiceSound, volume);
        }

        // 3. ПРЕЦИЗЕН ТАЙМИНГ ЗА ПАДАНЕТО
        // Тук не чакаме целия звук, а точно определено време (dropDelay),
        // за да може маската да падне точно когато се чуе "ТРАК" в аудио файла.
        yield return new WaitForSeconds(dropDelay);

        // 4. ITEM DROP
        SpawnMask();

        // Изчакваме още малко, ако аудиото е дълго, за да не прекъснем рязко логиката
        yield return new WaitForSeconds(1.0f);

        isDispensing = false;
    }

private void SpawnMask()
    {
        // Option 1: Use existing scene object (move and activate it)
        if (existingMaskPiece != null)
        {
            // Calculate spawn position
            Vector3 spawnPosition;
            if (dropPoint != null)
                spawnPosition = dropPoint.position;
            else
                spawnPosition = transform.position + transform.forward * dropForwardOffset + Vector3.up * dropHeight;
            
            // Move and activate the existing piece
            existingMaskPiece.transform.position = spawnPosition;
            existingMaskPiece.transform.rotation = Quaternion.identity;
            existingMaskPiece.SetActive(true);
            
            Debug.Log($"[MaskDispenser] Activated existing mask piece: {existingMaskPiece.name} at {spawnPosition}");
            
            // Add physics for drop effect
            Rigidbody rb = existingMaskPiece.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = existingMaskPiece.AddComponent<Rigidbody>();
                rb.mass = 0.5f;
                StartCoroutine(RemoveRigidbodyAfterDelay(rb, 2f));
            }
            
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                Vector3 force = (transform.forward * 2f + Vector3.up * 3f) * forceMultiplier;
                rb.AddForce(force, ForceMode.Impulse);
            }
            
            // Mark as used so it can't dispense again
            hasBeenUsed = true;
            
            return;
        }
        
        // Option 2: Instantiate from prefab (original behavior)
        if (maskPiecePrefab == null)
        {
            // No prefab and no existing piece - already dispensed or not configured
            Debug.Log("[MaskDispenser] Nothing to dispense (item already collected or not configured)");
            return;
        }

        // Calculate spawn position
        Vector3 prefabSpawnPosition;
        if (dropPoint != null)
            prefabSpawnPosition = dropPoint.position;
        else
            prefabSpawnPosition = transform.position + transform.forward * dropForwardOffset + Vector3.up * dropHeight;

        // Create the mask
        GameObject droppedPiece = Instantiate(maskPiecePrefab, prefabSpawnPosition, Quaternion.identity);
        droppedPiece.name = "DroppedMaskPiece";

        Debug.Log($"[MaskDispenser] Dispensed mask piece at {prefabSpawnPosition}");

        // Physics for drop effect
        Rigidbody prefabRb = droppedPiece.GetComponent<Rigidbody>();
        if (prefabRb == null)
        {
            prefabRb = droppedPiece.AddComponent<Rigidbody>();
            prefabRb.mass = 0.5f;
            StartCoroutine(RemoveRigidbodyAfterDelay(prefabRb, 2f));
        }

        if (prefabRb != null)
        {
            Vector3 force = (transform.forward * 2f + Vector3.up * 3f) * forceMultiplier;
            prefabRb.AddForce(force, ForceMode.Impulse);
        }
        
        // Mark as used for single-use dispensers
        hasBeenUsed = true;
    }

    private IEnumerator RemoveRigidbodyAfterDelay(Rigidbody rb, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (rb != null)
        {
            Vector3 finalPos = rb.transform.position;
            Destroy(rb);
            // Фиксираме позицията (finalDropY можеш да го върнеш като променлива ако ти трябва)
            rb.transform.position = new Vector3(finalPos.x, rb.transform.position.y, finalPos.z);
        }
    }
}