using UnityEngine;

public class SoundFXManager : MonoBehaviour
{
    public static SoundFXManager Instance;
    [SerializeField] private AudioSource soundFXSourcePrefab; //SerializeField makes private variables visible in the inspector
    [SerializeField] private AudioClip defaultSoundFX; // fallback clip if caller didn't assign one

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
    }

    public void PlaySoundFX(AudioClip audioClip, Transform spawnTransform, float volume)
    {
        if (soundFXSourcePrefab == null)
        {
            Debug.LogWarning("SoundFXManager: soundFXSourcePrefab is not assigned.");
            return;
        }

        // fallback to default clip if caller didn't provide one
        // AudioClip clipToPlay = audioClip != null ? audioClip : defaultSoundFX;
        // if (clipToPlay == null)
        // {
        //     Debug.LogWarning("SoundFXManager: no audio clip provided and defaultSoundFX is not assigned.");
        //     return;
        // }

        // choose clip with fallback
        AudioClip clipToPlay = audioClip != null ? audioClip : defaultSoundFX;
        if (clipToPlay == null)
        {
            Debug.LogWarning("SoundFXManager: no audio clip provided and defaultSoundFX is not assigned.");
            return;
        }

        Vector3 spawnPos = spawnTransform != null ? spawnTransform.position : Vector3.zero;
        var clipName = clipToPlay != null ? clipToPlay.name : "(null)";
        var spawnName = spawnTransform != null ? spawnTransform.name : "(no transform)";
        Debug.Log("SoundFXManager: PlaySoundFX called with clip=" + clipName + " spawn=" + spawnName + " prefabAssigned=" + (soundFXSourcePrefab != null));

        //create an audio source instance at the spawn position
        AudioSource audioSource = Instantiate(soundFXSourcePrefab, spawnPos, Quaternion.identity);
        // Assign audio clip and ensure non-spatial playback for UI/general SFX
        audioSource.spatialBlend = 0f;
        audioSource.mute = false;
        audioSource.volume = Mathf.Clamp01(volume);

        // Diagnostic info to help find why UI sounds may be silent
        Debug.Log("SoundFXManager: audioSource activeInHierarchy=" + audioSource.gameObject.activeInHierarchy +
              " enabled=" + audioSource.enabled +
              " mute=" + audioSource.mute +
              " playOnAwake=" + audioSource.playOnAwake +
              " spatialBlend=" + audioSource.spatialBlend +
              " output=" + (audioSource.outputAudioMixerGroup != null ? audioSource.outputAudioMixerGroup.name : "(none)"));

        var listener = FindObjectOfType<AudioListener>();
        Debug.Log("SoundFXManager: AudioListener present=" + (listener != null) + (listener != null ? " name=" + listener.gameObject.name : ""));

        //audioSource.Play();
        //Debug.Log("SoundFXManager: audioSource.Play() called for clip=" + (audioSource.clip != null ? audioSource.clip.name : "(null)"));
          // Use PlayOneShot for short UI sounds (more reliable for tiny clips)
        audioSource.PlayOneShot(clipToPlay, Mathf.Clamp01(volume));
        Debug.Log("SoundFXManager: audiosource.PlayOneShot() called for clip=" + (clipToPlay != null ? clipToPlay.name : "(null)"));


        //put length of audio clip (defensive)
        //float clipLength = audioSource.clip != null ? audioSource.clip.length : 0f;
        float clipLength = clipToPlay != null ? clipToPlay.length : 0f;

        // Ensure a small minimum lifetime so very-short clips still play
        float life = clipLength > 0f ? clipLength : 0.1f;
        Destroy(audioSource.gameObject, life + 0.05f);
        //destroy audio source after clipLength seconds (fallback to 1s if length unknown)
        //Destroy(audioSource.gameObject, clipLength > 0f ? clipLength : 1f);
    }
}
