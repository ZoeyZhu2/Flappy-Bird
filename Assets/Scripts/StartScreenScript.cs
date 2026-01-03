using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScreenScript : MonoBehaviour
{
    [SerializeField] private GameObject startScreen; //used to be public
    [SerializeField] private AudioClip pressSound;
    [SerializeField] private float volume = 1f; //used to be public

    
    public void Start()
    {
        OpenStartScreen();
    }

    public void OpenStartScreen()
    {
        
        startScreen.SetActive(true);
        // if (AudioManager.Instance != null)
        // {
        //     AudioManager.Instance.ReapplyVolumeSettings();
        //     AudioManager.Instance.PlayStartScreenMusic();
        // }
        // else
        // {
        //     Debug.LogWarning("AudioManager or startScreenMusic is not assigned!");
        // }
    }



    public void CloseStartScreen()
    {
        startScreen.SetActive(false);
    }
    
    public void PlayNormal()
    {
        Debug.Log("PlayNormal called!");
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySoundFX(pressSound, transform, volume);
        }
        else
        {
            Debug.LogWarning("AudioManager or startScreenMusic is not assigned!");
        }
        if (GameModeManager.Instance != null)
        {
            GameModeManager.Instance.SetNormalMode();
        }
        else
        {
            Debug.LogWarning("GameModeManager.Instance is null. Ensure a GameModeManager exists in the scene.");
        }
        Debug.Log("About to load scene 1");
        SceneManager.LoadScene(1);
    }

    public void PlayDaily()
    {
        Debug.Log("PlayDaily called!");
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySoundFX(pressSound, transform, volume);
        }
        else
        {
            Debug.LogWarning("AudioManager or startScreenMusic is not assigned!");
        }
        if (GameModeManager.Instance != null)
        {
            GameModeManager.Instance.SetDailyMode();
        }
        else
        {
            Debug.LogWarning("GameModeManager.Instance is null. Ensure a GameModeManager exists in the scene.");
        }
        Debug.Log("About to load scene 1");
        SceneManager.LoadScene(1);
    }

    public void QuitGame()
    {
        Debug.Log("QuitGame called!");
        AudioManager.Instance.PlaySoundFX(pressSound, transform, volume);
        Application.Quit();
    }
    
}
