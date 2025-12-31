using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;
    // public GameObject settingsScreen;

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
        SceneManager.LoadScene("Settings Scene", LoadSceneMode.Additive);
    }

    public void CloseSettings()
    {
        // settingsScreen.SetActive(false);
        SceneManager.UnloadSceneAsync("Settings Scene");
    }
    
    public void ToggleSettings()
    {
        // settingsScreen.SetActive(!settingsScreen.activeSelf); //if panel is active, deactivate it. If panel is inactive, activate it.
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
