using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;
    // public GameObject settingsScreen;
    [SerializeField] private AudioClip pressSound;
    public float volume = 1f;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void OpenSettings()
    {
        // settingsScreen.SetActive(true);
        SoundFXManager.Instance.PlaySoundFX(pressSound, transform, volume);
        SceneManager.LoadScene("Settings Scene", LoadSceneMode.Additive);
    }

    public void CloseSettings()
    {
        // settingsScreen.SetActive(false);
        SoundFXManager.Instance.PlaySoundFX(pressSound, transform, volume);
        SceneManager.UnloadSceneAsync("Settings Scene");
    }
    
    public void ToggleSettings()
    {
        // settingsScreen.SetActive(!settingsScreen.activeSelf); //if panel is active, deactivate it. If panel is inactive, activate it.
        SoundFXManager.Instance.PlaySoundFX(pressSound, transform, volume);
        if (SceneManager.GetSceneByName("Settings Scene").isLoaded)
        {
            CloseSettings();
        }
        else
        {
            OpenSettings();
        }
    
    }
}
