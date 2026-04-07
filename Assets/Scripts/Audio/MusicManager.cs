using UnityEngine;
using System.Collections;

/// <summary>
/// MusicManager handles background music with layered audio support.
/// Supports synced playback of music + layer (e.g., tick_tock with nivo 1)
/// All transitions use configurable fade duration.
/// </summary>
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Audio Sources")]
    public AudioSource sourceA;           // Primary music source A
    public AudioSource sourceB;           // Primary music source B (for crossfade)
    public AudioSource layerSource;       // Synced layer (tick_tock, etc.)
    public AudioSource ambientSource;     // Ambient loops (office_ambi, etc.)

    [Header("Fade Settings")]
    public float fadeDuration = 1.0f;     // 1 second fade in/out

    [Header("Music Clips")]
    [SerializeField] private AudioClip nivoIntro;
    [SerializeField] private AudioClip nivo1;
    [SerializeField] private AudioClip nivo2;
    [SerializeField] private AudioClip nivo3;
    [SerializeField] private AudioClip nivo4;

    [Header("Layer Clips")]
    [SerializeField] private AudioClip tickTock;

    [Header("Ambient Clips")]
    [SerializeField] private AudioClip officeAmbi;

    [Header("Volume Settings")]
    [SerializeField] private float musicVolume = 1f;
    [SerializeField] private float layerVolume = 0.7f;
    [SerializeField] private float ambientVolume = 0.5f;

    private AudioSource activeSource;
    private Coroutine musicFadeCoroutine;
    private Coroutine layerFadeCoroutine;
    private Coroutine ambientFadeCoroutine;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        activeSource = sourceA;
        
        // Setup layer source if not assigned
        if (layerSource == null)
        {
            layerSource = gameObject.AddComponent<AudioSource>();
            layerSource.loop = true;
            layerSource.playOnAwake = false;
        }
        
        // Setup ambient source if not assigned
        if (ambientSource == null)
        {
            ambientSource = gameObject.AddComponent<AudioSource>();
            ambientSource.loop = true;
            ambientSource.playOnAwake = false;
        }
    }

    void Start()
    {
        // Start with nivo intro
        PlayIntro();
    }

    /// <summary>
    /// Game start - plays nivo intro
    /// </summary>
    public void PlayIntro()
    {
        if (nivoIntro == null)
        {
            Debug.LogWarning("[MusicManager] nivoIntro not assigned!");
            return;
        }
        
        StopAllLayers();
        PlayMusicImmediate(nivoIntro);
        Debug.Log("[MusicManager] Playing: nivo intro");
    }

    /// <summary>
    /// Called when Mask 1 is equipped - plays nivo1 + tick_tock synced
    /// </summary>
    public void PlayMask1Music()
    {
        if (nivo1 == null)
        {
            Debug.LogWarning("[MusicManager] nivo1 not assigned!");
            return;
        }
        
        Debug.Log("[MusicManager] Playing: nivo 1 + tick_tock (synced)");
        
        // Fade out ambient if playing
        FadeOutAmbient();
        
        // Crossfade to nivo1 and start tick_tock synced
        StartCoroutine(CrossFadeWithSyncedLayer(nivo1, tickTock));
    }

    /// <summary>
    /// Called when entering office (Manifesto_Worker shown) - plays nivo2 + office_ambi
    /// </summary>
    public void PlayOfficeMusic()
    {
        if (nivo2 == null)
        {
            Debug.LogWarning("[MusicManager] nivo2 not assigned!");
            return;
        }
        
        Debug.Log("[MusicManager] Playing: nivo 2 + office_ambi");
        
        // Fade out layer (tick_tock)
        FadeOutLayer();
        
        // Crossfade to nivo2
        PlayMusic(nivo2);
        
        // Start office ambient
        if (officeAmbi != null)
        {
            FadeInAmbient(officeAmbi);
        }
    }

    /// <summary>
    /// Called when passing Point of No Return - plays nivo3
    /// </summary>
    public void PlayPoNRMusic()
    {
        if (nivo3 == null)
        {
            Debug.LogWarning("[MusicManager] nivo3 not assigned!");
            return;
        }
        
        Debug.Log("[MusicManager] Playing: nivo 3");
        
        // Fade out all layers
        FadeOutLayer();
        FadeOutAmbient();
        
        // Crossfade to nivo3
        PlayMusic(nivo3);
    }

    /// <summary>
    /// Called when entering club - plays nivo4
    /// </summary>
    public void PlayClubMusic()
    {
        if (nivo4 == null)
        {
            Debug.LogWarning("[MusicManager] nivo4 not assigned!");
            return;
        }
        
        Debug.Log("[MusicManager] Playing: nivo 4");
        
        FadeOutLayer();
        FadeOutAmbient();
        PlayMusic(nivo4);
    }

    /// <summary>
    /// Generic play music with crossfade
    /// </summary>
    public void PlayMusic(AudioClip newClip)
    {
        if (newClip == null) return;
        if (activeSource.clip == newClip && activeSource.isPlaying) return;

        if (musicFadeCoroutine != null)
            StopCoroutine(musicFadeCoroutine);
        
        musicFadeCoroutine = StartCoroutine(CrossFadeMusic(newClip));
    }

    /// <summary>
    /// Play music immediately without fade (for game start)
    /// </summary>
    private void PlayMusicImmediate(AudioClip clip)
    {
        if (clip == null) return;
        
        activeSource.clip = clip;
        activeSource.loop = true;
        activeSource.volume = musicVolume;
        activeSource.Play();
    }

    /// <summary>
    /// Crossfade between music tracks
    /// </summary>
    private IEnumerator CrossFadeMusic(AudioClip newClip)
    {
        AudioSource fadeOut = activeSource;
        AudioSource fadeIn = (activeSource == sourceA) ? sourceB : sourceA;

        fadeIn.clip = newClip;
        fadeIn.loop = true;
        fadeIn.volume = 0;
        fadeIn.Play();

        float timer = 0f;
        float startVolume = fadeOut.volume;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = timer / fadeDuration;

            fadeIn.volume = Mathf.Lerp(0, musicVolume, t);
            fadeOut.volume = Mathf.Lerp(startVolume, 0, t);

            yield return null;
        }

        fadeOut.Stop();
        fadeOut.volume = 0;
        fadeIn.volume = musicVolume;

        activeSource = fadeIn;
    }

    /// <summary>
    /// Crossfade to new music while starting a synced layer
    /// Both start at the same time for sync
    /// </summary>
    private IEnumerator CrossFadeWithSyncedLayer(AudioClip musicClip, AudioClip layerClip)
    {
        AudioSource fadeOut = activeSource;
        AudioSource fadeIn = (activeSource == sourceA) ? sourceB : sourceA;

        // Prepare both sources
        fadeIn.clip = musicClip;
        fadeIn.loop = true;
        fadeIn.volume = 0;

        if (layerClip != null)
        {
            layerSource.clip = layerClip;
            layerSource.loop = true;
            layerSource.volume = 0;
        }

        // Start both at EXACTLY the same time for sync
        fadeIn.Play();
        if (layerClip != null)
        {
            layerSource.Play();
        }

        float timer = 0f;
        float startVolume = fadeOut.volume;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = timer / fadeDuration;

            fadeIn.volume = Mathf.Lerp(0, musicVolume, t);
            fadeOut.volume = Mathf.Lerp(startVolume, 0, t);
            
            if (layerClip != null)
            {
                layerSource.volume = Mathf.Lerp(0, layerVolume, t);
            }

            yield return null;
        }

        fadeOut.Stop();
        fadeOut.volume = 0;
        fadeIn.volume = musicVolume;
        
        if (layerClip != null)
        {
            layerSource.volume = layerVolume;
        }

        activeSource = fadeIn;
    }

    /// <summary>
    /// Fade out the synced layer
    /// </summary>
    private void FadeOutLayer()
    {
        if (!layerSource.isPlaying) return;
        
        if (layerFadeCoroutine != null)
            StopCoroutine(layerFadeCoroutine);
        
        layerFadeCoroutine = StartCoroutine(FadeOutSource(layerSource));
    }

    /// <summary>
    /// Fade in ambient audio
    /// </summary>
    private void FadeInAmbient(AudioClip clip)
    {
        if (clip == null) return;
        
        if (ambientFadeCoroutine != null)
            StopCoroutine(ambientFadeCoroutine);
        
        ambientSource.clip = clip;
        ambientSource.loop = true;
        ambientSource.volume = 0;
        ambientSource.Play();
        
        ambientFadeCoroutine = StartCoroutine(FadeInSource(ambientSource, ambientVolume));
    }

    /// <summary>
    /// Fade out ambient audio
    /// </summary>
    private void FadeOutAmbient()
    {
        if (!ambientSource.isPlaying) return;
        
        if (ambientFadeCoroutine != null)
            StopCoroutine(ambientFadeCoroutine);
        
        ambientFadeCoroutine = StartCoroutine(FadeOutSource(ambientSource));
    }

    /// <summary>
    /// Generic fade in for an audio source
    /// </summary>
    private IEnumerator FadeInSource(AudioSource source, float targetVolume)
    {
        float timer = 0f;
        float startVolume = source.volume;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = timer / fadeDuration;
            source.volume = Mathf.Lerp(startVolume, targetVolume, t);
            yield return null;
        }

        source.volume = targetVolume;
    }

    /// <summary>
    /// Generic fade out for an audio source
    /// </summary>
    private IEnumerator FadeOutSource(AudioSource source)
    {
        float timer = 0f;
        float startVolume = source.volume;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = timer / fadeDuration;
            source.volume = Mathf.Lerp(startVolume, 0, t);
            yield return null;
        }

        source.Stop();
        source.volume = 0;
    }

    /// <summary>
    /// Stop all layers immediately
    /// </summary>
    private void StopAllLayers()
    {
        if (layerSource.isPlaying)
        {
            layerSource.Stop();
            layerSource.volume = 0;
        }
        
        if (ambientSource.isPlaying)
        {
            ambientSource.Stop();
            ambientSource.volume = 0;
        }
    }
}
