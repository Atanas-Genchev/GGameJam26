using UnityEngine;
using System.Collections;

/// <summary>
/// Plays car pass-by sounds at random intervals.
/// Attach to a GameObject in the scene (like Player or a dedicated AudioManager).
/// </summary>
public class AmbientCarSounds : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioClip[] carSounds;
    [SerializeField] private float volume = 0.6f;
    
    [Header("Timing")]
    [SerializeField] private float minInterval = 5f;
    [SerializeField] private float maxInterval = 45f;
    
    [Header("Spatial Settings")]
    [SerializeField] private bool use3DSound = true;
    [SerializeField] private float minDistance = 5f;
    [SerializeField] private float maxDistance = 50f;
    [SerializeField] private float spawnRadius = 30f;
    
    private AudioSource audioSource;
    private Transform playerTransform;
    
    private void Start()
    {
        // Get or create AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        audioSource.spatialBlend = use3DSound ? 1f : 0f;
        audioSource.minDistance = minDistance;
        audioSource.maxDistance = maxDistance;
        audioSource.playOnAwake = false;
        
        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        
        // Start the random sound loop
        StartCoroutine(RandomCarSoundLoop());
    }
    
    private IEnumerator RandomCarSoundLoop()
    {
        // Initial delay
        yield return new WaitForSeconds(Random.Range(minInterval, maxInterval));
        
        while (true)
        {
            PlayRandomCarSound();
            
            // Wait for random interval
            float waitTime = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(waitTime);
        }
    }
    
    private void PlayRandomCarSound()
    {
        if (carSounds == null || carSounds.Length == 0) return;
        
        AudioClip clip = carSounds[Random.Range(0, carSounds.Length)];
        if (clip == null) return;
        
        if (use3DSound && playerTransform != null)
        {
            // Play at random position around player
            Vector3 randomOffset = Random.insideUnitSphere * spawnRadius;
            randomOffset.y = 0; // Keep on same Y level
            Vector3 soundPosition = playerTransform.position + randomOffset;
            
            AudioSource.PlayClipAtPoint(clip, soundPosition, volume);
            Debug.Log($"[AmbientCarSounds] Playing car sound at {soundPosition}");
        }
        else
        {
            // Play 2D
            audioSource.PlayOneShot(clip, volume);
            Debug.Log("[AmbientCarSounds] Playing car sound (2D)");
        }
    }
    
    /// <summary>
    /// Manually trigger a car sound
    /// </summary>
    public void PlayCarSound()
    {
        PlayRandomCarSound();
    }
    
    /// <summary>
    /// Set the interval range
    /// </summary>
    public void SetInterval(float min, float max)
    {
        minInterval = min;
        maxInterval = max;
    }
}
