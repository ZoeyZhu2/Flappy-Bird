using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public GameObject pausedScreen;
    private bool isPaused = false;
    private PlayerInputActions inputActions;
    public GameObject pauseButton;
    
    void Awake()
    {
        inputActions = new PlayerInputActions();
        // Subscribe to the Pause action
        inputActions.UI.Pause.performed += ctx => TogglePause();
        /*
        Explanation of the line above:
        inputActions.UI.Pause: Pause action
        .performed: Event triggered when the action is performed
        +=: subscribes to event (adds a listener)
        ctx => Lambda expression defining the action to take when the event is triggered
        TogglePause(): Method to call when the Pause action is performed
        */
    }
    
    void OnEnable()
    {
        inputActions.UI.Pause.Enable();
    }

    void OnDisable()
    {
        inputActions.UI.Pause.Disable();
    }

    void TogglePause()
    {

        if (isPaused)
        {
            Resume();
        }
        else
        {
            Pause();
        }
    }

    public void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f; // Pause the game
        pausedScreen.SetActive(true); // Show the paused screen
        pauseButton.SetActive(false); // Hide the pause button
    }

    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f; // Resume the game
        pausedScreen.SetActive(false); // Hide the paused screen
        pauseButton.SetActive(true); // Show the pause button
    }

    public void goHome()
    {
        Time.timeScale = 1f; // Ensure time scale is reset
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
    public void QuitGame()
    {
        Time.timeScale = 1f; // Ensure time scale is reset
        Application.Quit();
    }

    public void OpenSettings()
    {
        SettingsManager.Instance.OpenSettings();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
