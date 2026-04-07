using UnityEngine;
using System.Collections;

/// <summary>
/// Manages ambient sounds that play randomly at intervals.
/// Attach to a GameObject in the scene.
/// </summary>
public class AmbientSoundManager : MonoBehaviour
{
    public static AmbientSoundManager Instance { get; private set; }
    
    [Header("Car Pass By")]
    [SerializeField] private AudioClip carPassBySound;
    [SerializeField] private float carMinInterval = 5f;
    [SerializeField] private float carMaxInterval = 45f;
    [SerializeField] private float carVolume = 0.6f;
    
    [Header("Settings")]
    [SerializeField] private bool playOnStart = true;
    
    private AudioSource audioSource;
    private Coroutine carSoundCoroutine;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 0f; // 2D sound
            audioSource.playOnAwake = false;
        }
    }
    
    private void Start()
    {
        if (playOnStart)
        {
            StartCarSounds();
        }
    }
    
    public void StartCarSounds()
    {
        if (carSoundCoroutine != null)
        {
            StopCoroutine(carSoundCoroutine);
        }
        carSoundCoroutine = StartCoroutine(CarSoundLoop());
    }
    
    public void StopCarSounds()
    {
        if (carSoundCoroutine != null)
        {
            StopCoroutine(carSoundCoroutine);
            carSoundCoroutine = null;
        }
    }
    
    private IEnumerator CarSoundLoop()
    {
        while (true)
        {
            // Wait random interval
            float waitTime = Random.Range(carMinInterval, carMaxInterval);
            yield return new WaitForSeconds(waitTime);
            
            // Play car sound
            if (carPassBySound != null && audioSource != null)
            {
                audioSource.pitch = Random.Range(0.9f, 1.1f);
                audioSource.PlayOneShot(carPassBySound, carVolume);
                Debug.Log($"[AmbientSoundManager] Playing car pass by sound (next in {waitTime:F1}s)");
            }
        }
    }
    
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
