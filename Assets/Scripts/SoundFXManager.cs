using UnityEngine;

public class SoundFXManager : MonoBehaviour
{
    public static SoundFXManager Instance;
    [SerializeField] private AudioSource soundFXSourcePrefab; //SerializeField makes private variables visible in the inspector

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
        //create an empty game object at the spawnTransform position that will be the audioSource
        AudioSource audioSource = Instantiate(soundFXSourcePrefab, spawnTransform.position, Quaternion.identity);
        //Assign audioClip and volume to the audioSource and play it
        audioSource.clip = audioClip;
        audioSource.volume = volume;
        audioSource.Play();

        //put length of audio clip
        float clipLength = audioSource.clip.length;
        
        //destroy audio source after clipLength seconds
        Destroy(audioSource.gameObject, clipLength);
    }
}
