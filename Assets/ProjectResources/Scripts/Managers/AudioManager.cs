using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum SoundEffect
{
    // Player sounds
    PlayerJump,
    PlayerDash,
    PlayerLand,
    PlayerDeath,
    // Game events
    LevelComplete,
    // Environment sounds
    HeartbeatWarning,

}

[System.Serializable]
public class Sound
{
    public SoundEffect effect;
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;
    [Range(0.1f, 3f)] public float pitch = 1f;
    public bool loop = false;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgGameMusic;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource heartbeatSource;

    [Header("Sound Effects")]
    [SerializeField] private List<Sound> soundEffects = new List<Sound>();

    [Header("Settings")]
    [SerializeField] private float musicFadeDuration = 1f;
    [SerializeField] private float heartbeatMaxVolume = 0.7f;
    [SerializeField] private float heartbeatMinPitch = 0.8f;
    [SerializeField] private float heartbeatMaxPitch = 1.5f;

    private Dictionary<SoundEffect, Sound> soundDictionary = new Dictionary<SoundEffect, Sound>();
    private Coroutine heartbeatCoroutine;
    private float currentHeartbeatIntensity = 0f;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeAudioSources();
        BuildSoundDictionary();
    }

    void InitializeAudioSources()
    {
        bgGameMusic.loop = true;
        bgGameMusic.Play();

        // Configure heartbeat source
        heartbeatSource.loop = true;
        heartbeatSource.volume = 0f;
    }


    void BuildSoundDictionary()
    {
        foreach (Sound sound in soundEffects)
        {
            if (!soundDictionary.ContainsKey(sound.effect))
            {
                soundDictionary.Add(sound.effect, sound);
            }
            else
            {
                Debug.LogWarning($"Duplicate sound effect found: {sound.effect}");
            }
        }
    }

    #region Public Methods - Sound Effects

    public void PlayEffect(SoundEffect effect)
    {
        PlaySoundEffect(effect, sfxSource);
    }


    private void PlaySoundEffect(SoundEffect effect, AudioSource source)
    {
        if (soundDictionary.TryGetValue(effect, out Sound sound))
        {
            // source.volume = sound.volume;
            // source.pitch = sound.pitch;
            // source.loop = sound.loop;

            if (sound.loop)
            {
                if (source.clip != sound.clip || !source.isPlaying)
                {
                    source.clip = sound.clip;
                    source.Play();
                }
            }
            else
            {
                source.PlayOneShot(sound.clip, 1);
            }
        }
        else
        {
            Debug.LogWarning($"Sound effect not found: {effect}");
        }
    }

    public void StopSoundEffect(SoundEffect effect)
    {
        // Check all sources for this sound and stop them
        AudioSource[] sources = { sfxSource, heartbeatSource };

        foreach (AudioSource source in sources)
        {
            if (source.isPlaying && soundDictionary.TryGetValue(effect, out Sound sound))
            {
                if (source.clip == sound.clip)
                {
                    source.Stop();
                }
            }
        }
    }

    #endregion

    #region Heartbeat System

    public void SetHeartbeatIntensity(float intensity)
    {
        currentHeartbeatIntensity = Mathf.Clamp01(intensity);

        if (heartbeatCoroutine != null)
        {
            StopCoroutine(heartbeatCoroutine);
        }
        heartbeatCoroutine = StartCoroutine(UpdateHeartbeat());
    }

    private IEnumerator UpdateHeartbeat()
    {
        // Get or load heartbeat sound

        float bgTargetVolume = Mathf.Lerp(1f, 0.3f, currentHeartbeatIntensity);
        if (!soundDictionary.ContainsKey(SoundEffect.HeartbeatWarning))
        {
            Debug.LogWarning("Heartbeat sound not found in dictionary");
            yield break;
        }

        Sound heartbeatSound = soundDictionary[SoundEffect.HeartbeatWarning];

        // Ensure heartbeat source is configured
        heartbeatSource.clip = heartbeatSound.clip;
        heartbeatSource.volume = heartbeatSound.volume * currentHeartbeatIntensity;
        heartbeatSource.pitch = Mathf.Lerp(heartbeatMinPitch, heartbeatMaxPitch, currentHeartbeatIntensity);

        // Start playing if not already
        if (!heartbeatSource.isPlaying)
        {
            heartbeatSource.Play();
        }

        // Smoothly adjust volume and pitch
        float targetVolume = heartbeatSound.volume * currentHeartbeatIntensity * heartbeatMaxVolume;
        float targetPitch = Mathf.Lerp(heartbeatMinPitch, heartbeatMaxPitch, currentHeartbeatIntensity);

        float duration = 0.5f;
        float elapsed = 0f;
        float startVolume = heartbeatSource.volume;
        float startPitch = heartbeatSource.pitch;

        while (elapsed < duration)
        {
            float bgStartVolume = bgGameMusic.volume;
            bgGameMusic.volume = Mathf.Lerp(bgStartVolume, bgTargetVolume, duration);

            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            heartbeatSource.volume = Mathf.Lerp(startVolume, targetVolume, t);
            heartbeatSource.pitch = Mathf.Lerp(startPitch, targetPitch, t);

            yield return null;
        }

        heartbeatSource.volume = targetVolume;
        heartbeatSource.pitch = targetPitch;
        bgGameMusic.volume = bgTargetVolume;

        // Stop if intensity is zero
        if (currentHeartbeatIntensity <= 0.01f && heartbeatSource.isPlaying)
        {
            heartbeatSource.Stop();
        }
    }

    public void StopHeartbeat()
    {
        SetHeartbeatIntensity(0f);
    }

    #endregion

    #region Utility Methods

    public bool IsPlaying(SoundEffect effect)
    {
        AudioSource[] sources = { sfxSource, heartbeatSource };

        foreach (AudioSource source in sources)
        {
            if (source.isPlaying && soundDictionary.TryGetValue(effect, out Sound sound))
            {
                if (source.clip == sound.clip)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void PauseAll()
    {
        bgGameMusic.Pause();
        sfxSource.Pause();
        heartbeatSource.Pause();
    }

    public void ResumeAll()
    {
        bgGameMusic.UnPause();
        sfxSource.UnPause();
        heartbeatSource.UnPause();
    }

    public void StopAll()
    {
        bgGameMusic.Stop();
        sfxSource.Stop();
        heartbeatSource.Stop();
    }

    #endregion
}