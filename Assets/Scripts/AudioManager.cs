using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    [SerializeField] private AudioSource soundFXSourcePrefab; //SerializeField makes private variables visible in the inspector
    [SerializeField] private AudioClip defaultSoundFX; // fallback clip if caller didn't assign one

    [SerializeField] private float soundFXVolume = 0.5f; //player setting
    private bool soundFXMuted = false;

    private AudioListener audioListener;

    //Object pool for playing soundFX for optimization (so I don't have to create/destroy audio sources all the time)
    private Queue<AudioSource> soundFXPool = new Queue<AudioSource>();
    [SerializeField] private int poolSize = 10; //number of audio sources in the pool


    private AudioSource backgroundMusicSource;

    [SerializeField] private AudioClip startScreenMusic;
    [SerializeField] private AudioClip gameMusic;

    [SerializeField] private float musicVolume = 0.5f; //player setting

    [SerializeField] private float defaultStartScreenMusicVolume = 0.5f; 
    [SerializeField] private float defaultGameMusicVolume = 0.5f;

    private float defaultMusicVolume = 1f;
    private bool musicMuted = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // Initialize the soundFX audio source pool
        //keeping pool under the AudioManager object in hierarchy
        for (int i = 0; i < poolSize; i++)
        {
            AudioSource audioSource = Instantiate(soundFXSourcePrefab, Vector3.zero, Quaternion.identity, transform);
            audioSource.gameObject.SetActive(false);
            soundFXPool.Enqueue(audioSource);
        }

        //Find the AudioListener in the scene
        audioListener = FindAnyObjectByType<AudioListener>();
        if (audioListener == null)
        {
            Debug.LogWarning("AudioManager: No AudioListener found in the scene. Please add one to hear sounds.");
        }

        //Initialize background music source
        backgroundMusicSource = gameObject.AddComponent<AudioSource>();
        backgroundMusicSource.loop = true;
        backgroundMusicSource.gameObject.SetActive(true);
    }


    private void PlayMusic(AudioClip musicClip, float volume)
    {
        if (backgroundMusicSource == null)
        {
            Debug.LogWarning("AudioManager: backgroundMusicSource is not initialized.");
            return;
        }

        if (musicMuted)
        {
            Debug.Log("AudioManager: music is muted, not playing music.");
            return;
        }

        if (backgroundMusicSource.clip == musicClip)
        {
            return; // prevents restarting same music
        }

        backgroundMusicSource.clip = musicClip;
        backgroundMusicSource.volume = Mathf.Clamp01(volume * musicVolume);
        backgroundMusicSource.Play();
    }

    //ensures only one background music is playing at a time
    public void PlayStartScreenMusic()
    {
        defaultMusicVolume = defaultStartScreenMusicVolume;
        PlayMusic(startScreenMusic, defaultStartScreenMusicVolume);
    }

    public void PlayGameMusic()
    {
        defaultMusicVolume = defaultGameMusicVolume;
        PlayMusic(gameMusic, defaultGameMusicVolume);
    }

    public float GetMusicVolume()
    {
        return musicVolume;
    }

    //fix here
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (backgroundMusicSource != null)
        {
             backgroundMusicSource.volume = musicVolume * defaultMusicVolume; 
        }
    }

    public bool IsMusicMuted()
    {
        return musicMuted;
    }

    public void MusicMute(bool muteStatus)
    {
        musicMuted = muteStatus;
        //Keeps the music “in sync” if unmuted
        // if (musicMuted && backgroundMusicSource.isPlaying)
        // {
        //     backgroundMusicSource.Pause();
        // }
        // else if (!musicMuted && !backgroundMusicSource.isPlaying)
        // {
        //     backgroundMusicSource.UnPause();
        // }

        //music continues running but muted
        backgroundMusicSource.mute = musicMuted;
    }

    public void PlaySoundFX(AudioClip audioClip, Transform spawnTransform, float volume)
    {
        
        if (soundFXSourcePrefab == null)
        {
            Debug.LogWarning("SoundFXManager: soundFXSourcePrefab is not assigned.");
            return;
        }

        if (soundFXMuted)
        {
            Debug.Log("SoundFXManager: sound is muted, not playing clip.");
            return;
        }

        // choose clip with fallback
        AudioClip clipToPlay = audioClip != null ? audioClip : defaultSoundFX;
        if (clipToPlay == null)
        {
            Debug.LogWarning("SoundFXManager: no audio clip provided and defaultSoundFX is not assigned.");
            return;
        }

        Vector3 spawnPos = spawnTransform != null ? spawnTransform.position : Vector3.zero;

        //Diagnostic info
        var clipName = clipToPlay != null ? clipToPlay.name : "(null)";
        var spawnName = spawnTransform != null ? spawnTransform.name : "(no transform)";
        Debug.Log("SoundFXManager: PlaySoundFX called with clip=" + clipName + " spawn=" + spawnName + " prefabAssigned=" + (soundFXSourcePrefab != null));

        //Get an inactive audio source from the pool or create a new one if none are available
        AudioSource audioSource = soundFXPool.Count > 0 ? soundFXPool.Dequeue() : Instantiate(soundFXSourcePrefab, spawnPos, Quaternion.identity, transform);
        audioSource.transform.position = spawnPos;
        audioSource.gameObject.SetActive(true);
        
        //create an audio source instance at the spawn position
        // AudioSource audioSource = Instantiate(soundFXSourcePrefab, spawnPos, Quaternion.identity);
        audioSource.spatialBlend = 0f; // 0 = 2D sound, 1 = 3D sound
        audioSource.mute = false; //make sure my audio player object not muted
        audioSource.volume = Mathf.Clamp01(volume * soundFXVolume); //default volume scaled by player choice

        // Diagnostic info to help find why UI sounds may be silent
        Debug.Log("SoundFXManager: audioSource activeInHierarchy=" + audioSource.gameObject.activeInHierarchy +
              " enabled=" + audioSource.enabled +
              " mute=" + audioSource.mute +
              " playOnAwake=" + audioSource.playOnAwake +
              " spatialBlend=" + audioSource.spatialBlend +
              " output=" + (audioSource.outputAudioMixerGroup != null ? audioSource.outputAudioMixerGroup.name : "(none)"));

        Debug.Log("SoundFXManager: AudioListener present=" + (audioListener != null) + (audioListener != null ? " name=" + audioListener.gameObject.name : ""));

        // Use PlayOneShot for short UI sounds (more reliable for tiny clips)
        audioSource.PlayOneShot(clipToPlay, Mathf.Clamp01(volume * soundFXVolume));
        Debug.Log("SoundFXManager: audiosource.PlayOneShot() called for clip=" + (clipToPlay != null ? clipToPlay.name : "(null)"));

        //put length of audio clip (defensive)
        float clipLength = clipToPlay != null ? clipToPlay.length : 0f;

        // Ensure a small minimum lifetime so very-short clips still play
        float life = clipLength > 0f ? clipLength : 0.2f;
        StartCoroutine(ReturnToPoolAfter(audioSource, life));

        //for the case if I create a new audioSource every time (not using my pool)
        //Destroy(audioSource.gameObject, life + 0.05f);
        //destroy audio source after clipLength seconds (fallback to 1s if length unknown)

    }

    private System.Collections.IEnumerator ReturnToPoolAfter(AudioSource src, float seconds)
    {
        yield return new WaitForSeconds(seconds + 0.05f);
        src.gameObject.SetActive(false);
        soundFXPool.Enqueue(src);
    }


    public void SoundFXMute(bool muteStatus)
    {
        soundFXMuted = muteStatus;
    }

    public void SetSoundFXVolume(float volume)
    {
        soundFXVolume = Mathf.Clamp01(volume);
    }
    public bool IsSoundFXMuted()
    {
        return soundFXMuted;
    }

    public float GetSoundFXVolume()
    {
       return soundFXVolume;
    }

}

