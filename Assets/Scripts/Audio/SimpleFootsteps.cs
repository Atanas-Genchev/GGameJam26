using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SimpleFootsteps : MonoBehaviour
{
    [Header("Footstep Sounds")]
    [SerializeField] private AudioClip[] footstepSounds;
    [SerializeField] private float stepRate = 0.20f;

    [Header("Movement Settings")]
    [SerializeField] private float minWalkDistance = 0.05f;

    [Header("Audio Settings")]
    [Range(0f, 1f)][SerializeField] private float minVolume = 0.3f;
    [Range(0f, 1f)][SerializeField] private float maxVolume = 0.5f;
    [Range(0.5f, 1.5f)][SerializeField] private float minPitch = 0.8f;
    [Range(0.5f, 1.5f)][SerializeField] private float maxPitch = 1.1f; // How far you must move to count as walking

    private AudioSource audioSource;
    private float nextStepTime;
    private Vector3 lastPosition;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        lastPosition = transform.position;
    }

    void Update()
    {
        // 1. Calculate how far we moved since last frame (ignoring Up/Down movement)
        float distanceMoved = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z),
                                               new Vector3(lastPosition.x, 0, lastPosition.z));

        // 2. If we moved enough, we are walking
        if (distanceMoved > minWalkDistance)
        {
            if (Time.time >= nextStepTime)
            {
                PlayRandomFootstep();
                nextStepTime = Time.time + stepRate;
            }
        }

        // 3. Update lastPosition for the next frame
        lastPosition = transform.position;
    }

void PlayRandomFootstep()
    {
        if (footstepSounds.Length == 0) return;

        audioSource.pitch = Random.Range(minPitch, maxPitch);
        audioSource.volume = Random.Range(minVolume, maxVolume);

        int index = Random.Range(0, footstepSounds.Length);
        audioSource.PlayOneShot(footstepSounds[index]);
    }
}