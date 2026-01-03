using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    private AudioListener audioListener;

    //SoundFX management
    [SerializeField] private AudioSource soundFXSourcePrefab; //SerializeField makes private variables visible in the inspector
    [SerializeField] private AudioClip defaultSoundFX; // fallback clip if caller didn't assign one

    //[SerializeField] private float soundFXVolume = 0.5f; //player setting without audiomixer
    private bool soundFXMuted = false;
     //Object pool for playing soundFX for optimization (so I don't have to create/destroy audio sources all the time)
    private Queue<AudioSource> soundFXPool = new Queue<AudioSource>();
    [SerializeField] private int poolSize = 10; //number of audio sources in the pool


    //Background music management
    private AudioSource backgroundMusicSource;

    [SerializeField] private AudioClip startScreenMusic;
    [SerializeField] private AudioClip gameMusic;
    private bool musicMuted = false;

    //[SerializeField] private float musicVolume = 0.5f; //player setting without audio mixer

    //Default volumes
    //prior to AudioMixer implementation
    //[SerializeField] private float defaultStartScreenMusicVolume = 0.25f; 
    //[SerializeField] private float defaultGameMusicVolume = 0.25f;

    [SerializeField] private float defaultMusicVolume = 0.25f;
    [SerializeField] private float defaultSoundFXVolume = 1f;

    //Audio Mixer Groups
    [SerializeField] private AudioMixerGroup musicGroup;
    [SerializeField] private AudioMixerGroup soundFXGroup;
    [SerializeField] private AudioMixer audioMixer;

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
        backgroundMusicSource.spatialBlend = 0f; //2D sound
        backgroundMusicSource.outputAudioMixerGroup = musicGroup;

        // Load player preferences or use defaults
        float musicVol = PlayerPrefs.HasKey("BackgroundMusicVolume") ? PlayerPrefs.GetFloat("BackgroundMusicVolume") : defaultMusicVolume;
        float soundFXVol = PlayerPrefs.HasKey("SoundFXVolume") ? PlayerPrefs.GetFloat("SoundFXVolume") : defaultSoundFXVolume;
        
        Debug.Log("SettingsUIManager: Retrieved volumes - SoundFX=" + soundFXVol + " Music=" + musicVol);
        
        SetMusicVolume(musicVol);
        SetSoundFXVolume(soundFXVol);

        Debug.Log("AudioManager: Using AudioMixer named '" + audioMixer.name + "'");

    }

    //prior to AudioMixer implementation
    //private void PlayMusic(AudioClip musicClip, float volume)
    private void PlayMusic(AudioClip musicClip)
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
        //prior to AudioMixer implementation
        //backgroundMusicSource.volume = Mathf.Clamp01(volume * musicVolume);
        backgroundMusicSource.Play();
    }

    //ensures only one background music is playing at a time
    public void PlayStartScreenMusic()
    {
        //Prior to AudioMixer implementation
        //defaultMusicVolume = defaultStartScreenMusicVolume;
        //PlayMusic(startScreenMusic, defaultStartScreenMusicVolume);

        PlayMusic(startScreenMusic);
    }

    public void PlayGameMusic()
    {
        //prior to AudioMixer implementation
        //defaultMusicVolume = defaultGameMusicVolume;
        //PlayMusic(gameMusic, defaultGameMusicVolume);

        PlayMusic(gameMusic);
    }

    public float GetMusicVolume()
    {
        //Prior to AudioMixer implementation
        //return musicVolume;

        if (!audioMixer.GetFloat("BackgroundMusicVolume", out float db))
        {
            Debug.LogError("AudioManager: GetMusicVolume - GetFloat FAILED, using fallback -80dB"); 
            db = -80f; // fallback if parameter doesn't exist
        }

        Debug.Log("AudioManager: GetMusicVolume - Got dB=" + db + " from mixer");


        //convert dB back to linear
        float linearVolume = Mathf.Pow(10f, db / 20f);

         Debug.Log("AudioManager: GetMusicVolume - Converted to linear=" + linearVolume);


        return linearVolume;
    }

    public void SetMusicVolume(float linearVolume)
    {
        linearVolume = Mathf.Clamp01(linearVolume);
        //conversion to dB
        float dB = linearVolume <= 0.0001f ? -80f : Mathf.Log10(linearVolume) * 20f;
        
        Debug.Log("AudioManager: SetMusicVolume input=" + linearVolume + " converted to dB=" + dB);

        bool success = audioMixer.SetFloat("BackgroundMusicVolume", dB);
            Debug.Log("AudioManager: SetFloat success=" + success);

audioMixer.GetFloat("BackgroundMusicVolume", out float verifyDb);
    Debug.Log("AudioManager: Verified dB in mixer=" + verifyDb);


        PlayerPrefs.SetFloat("BackgroundMusicVolume", linearVolume);
        PlayerPrefs.Save();
        //prior to AudioMixer implementation 
        // musicVolume = Mathf.Clamp01(volume);
        // if (backgroundMusicSource != null)
        // {
        //      backgroundMusicSource.volume = musicVolume * defaultMusicVolume; 
        // }
    }

    public bool IsMusicMuted()
    {
        return musicMuted;
    }

    public void MusicMute(bool muteStatus)
    {
        musicMuted = muteStatus;

        //music continues running but muted
        backgroundMusicSource.mute = musicMuted;

        //Keeps the music “in sync” if unmuted
        // if (musicMuted && backgroundMusicSource.isPlaying)
        // {
        //     backgroundMusicSource.Pause();
        // }
        // else if (!musicMuted && !backgroundMusicSource.isPlaying)
        // {
        //     backgroundMusicSource.UnPause();
        // }
    }

    public void MusicPause()
    {
        if (backgroundMusicSource != null && backgroundMusicSource.isPlaying)
        {
            backgroundMusicSource.Pause();
        }
    }

    public void MusicUnpause()
    {
        if (backgroundMusicSource != null && !backgroundMusicSource.isPlaying && !musicMuted)
        {
            backgroundMusicSource.UnPause();
        }
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
        // var clipName = clipToPlay != null ? clipToPlay.name : "(null)";
        // var spawnName = spawnTransform != null ? spawnTransform.name : "(no transform)";
        // Debug.Log("SoundFXManager: PlaySoundFX called with clip=" + clipName + " spawn=" + spawnName + " prefabAssigned=" + (soundFXSourcePrefab != null));

        //Get an inactive audio source from the pool or create a new one if none are available
        AudioSource audioSource = soundFXPool.Count > 0 ? soundFXPool.Dequeue() : Instantiate(soundFXSourcePrefab, spawnPos, Quaternion.identity, transform);
        audioSource.outputAudioMixerGroup = soundFXGroup; //just to make sure
        audioSource.transform.position = spawnPos;
        audioSource.gameObject.SetActive(true);
        
        //create an audio source instance at the spawn position
        // AudioSource audioSource = Instantiate(soundFXSourcePrefab, spawnPos, Quaternion.identity);
        audioSource.spatialBlend = 0f; // 0 = 2D sound, 1 = 3D sound
        audioSource.mute = false; //make sure my audio player object not muted
        //Prior to AudioMixer implementation
        //audioSource.volume = Mathf.Clamp01(volume * soundFXVolume); //default volume scaled by player choice
        audioSource.volume = Mathf.Clamp01(volume);

        // Diagnostic info to help find why UI sounds may be silent
        // Debug.Log("SoundFXManager: audioSource activeInHierarchy=" + audioSource.gameObject.activeInHierarchy +
        //       " enabled=" + audioSource.enabled +
        //       " mute=" + audioSource.mute +
        //       " playOnAwake=" + audioSource.playOnAwake +
        //       " spatialBlend=" + audioSource.spatialBlend +
        //       " output=" + (audioSource.outputAudioMixerGroup != null ? audioSource.outputAudioMixerGroup.name : "(none)"));

        // Debug.Log("SoundFXManager: AudioListener present=" + (audioListener != null) + (audioListener != null ? " name=" + audioListener.gameObject.name : ""));

        // Use PlayOneShot for short UI sounds (more reliable for tiny clips)
        //Prior to AudioMixer implementation
        //audioSource.PlayOneShot(clipToPlay, Mathf.Clamp01(volume * soundFXVolume));
        audioSource.PlayOneShot(clipToPlay, Mathf.Clamp01(volume));
        Debug.Log("SoundFXManager: audiosource.PlayOneShot() called for clip=" + (clipToPlay != null ? clipToPlay.name : "(null)"));

        // Ensure a small minimum lifetime so very-short clips still play
        float life = clipToPlay.length > 0f ? clipToPlay.length : 0.2f;
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

    public bool IsSoundFXMuted()
    {
        return soundFXMuted;
    }

    public void SetSoundFXVolume(float linearVolume)
    {
        linearVolume = Mathf.Clamp01(linearVolume);
        float dB = linearVolume <= 0.0001f ? -80f : Mathf.Log10(linearVolume) * 20f;
        audioMixer.SetFloat("SoundFXVolume", dB);

        PlayerPrefs.SetFloat("SoundFXVolume", linearVolume);
        PlayerPrefs.Save();
        //prior to AudioMixer implementation
        //soundFXVolume = Mathf.Clamp01(volume);
    }

    public float GetSoundFXVolume()
    {
       //Prior to AudioMixer implementation
       //return soundFXVolume;
        if (!audioMixer.GetFloat("SoundFXVolume", out float db))
        {
            db = -80f; // fallback if parameter doesn't exist
        }
        //convert dB back to linear
        float linearVolume = Mathf.Pow(10f, db / 20f);
        return linearVolume;
    }

}

