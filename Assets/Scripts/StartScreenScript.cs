using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScreenScript : MonoBehaviour
{
    public GameObject startScreen;
    [SerializeField] private AudioClip pressSound;
    public float volume = 1f;

    public void OpenStartScreen()
    {
        startScreen.SetActive(true);
    }

    public void CloseStartScreen()
    {
        startScreen.SetActive(false);
    }
    
    public void PlayNormal()
    {
        Debug.Log("PlayNormal called!");
        SoundFXManager.Instance.PlaySoundFX(pressSound, transform, volume);
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
        SoundFXManager.Instance.PlaySoundFX(pressSound, transform, volume);
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
        SoundFXManager.Instance.PlaySoundFX(pressSound, transform, volume);
        Application.Quit();
    }
    
}
