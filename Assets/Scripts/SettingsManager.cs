using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;
    // public GameObject settingsScreen;
    [SerializeField] private AudioClip pressSound;
    [SerializeField] private float volume = 1f; //used to be public

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
        AudioManager.Instance.PlaySoundFX(pressSound, transform, volume);
        SceneManager.LoadScene("Settings Scene", LoadSceneMode.Additive);
    }

    public void CloseSettings()
    {
        // settingsScreen.SetActive(false);
        AudioManager.Instance.PlaySoundFX(pressSound, transform, volume);
        SceneManager.UnloadSceneAsync("Settings Scene");
    }
    
    public void ToggleSettings()
    {
        // settingsScreen.SetActive(!settingsScreen.activeSelf); //if panel is active, deactivate it. If panel is inactive, activate it.
        AudioManager.Instance.PlaySoundFX(pressSound, transform, volume);
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
