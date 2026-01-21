using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScreenUIManager : MonoBehaviour
{
    [Header("Start Screen")]
    [SerializeField] private GameObject startScreen; //used to be public

    [Header("Audio")]
    [SerializeField] private AudioClip pressSound;
    [SerializeField] private float volume = 1f; //used to be public

    [Header("Sign in UI")]
    //something

    [Header("Create Account pop up UI")]
    //something

    public void Start()
    {
        OpenSignInScreen();
    }

    public void OpenSignInScreen()
    {
        //something
    }

    public void CloseSignInScreen()
    {
        //something
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
        SceneManager.LoadScene(1);
    }

    public void QuitGame()
    {
        AudioManager.Instance.PlaySoundFX(pressSound, transform, volume);
        Application.Quit();
    }
    
}
