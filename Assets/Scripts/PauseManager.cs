using UnityEngine;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    public GameObject pausedScreen;
    private bool isPaused = false;
    private PlayerInputActions inputActions;
    public GameObject pauseButton;

    [SerializeField] private AudioClip pressSound;
    public float volume = 1f;

    void Awake()
    {
        inputActions = InputManager.inputActions;
    }

    void OnEnable()
    {
        if (inputActions != null)
        {
            inputActions.UI.Pause.performed += OnPausePerformed;
            inputActions.UI.Pause.Enable();
        }
    }

    void OnDisable()
    {
        if (inputActions != null)
        {
            inputActions.UI.Pause.performed -= OnPausePerformed;
            inputActions.UI.Pause.Disable();
        }
    }

    private void OnPausePerformed(InputAction.CallbackContext ctx)
    {
        TogglePause();
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
        SoundFXManager.Instance.PlaySoundFX(pressSound, transform, volume);
        isPaused = true;
        Time.timeScale = 0f; // Pause the game
        if (pausedScreen != null) pausedScreen.SetActive(true); // Show the paused screen
        if (pauseButton != null) pauseButton.SetActive(false); // Hide the pause button
        if (inputActions != null)
        {
            inputActions.Player.Disable(); // prevent gameplay inputs (e.g., jump) while paused
        }
    }

    public void Resume()
    {

        SoundFXManager.Instance.PlaySoundFX(pressSound, transform, volume);
        isPaused = false;
        Time.timeScale = 1f; // Resume the game
        if (pausedScreen != null) pausedScreen.SetActive(false); // Hide the paused screen
        if (pauseButton != null) pauseButton.SetActive(true); // Show the pause button
        if (inputActions != null)
        {
            inputActions.Player.Enable(); // restore gameplay inputs
        }
    }

    public void goHome()
    {
        SoundFXManager.Instance.PlaySoundFX(pressSound, transform, volume);
        Time.timeScale = 1f; // Ensure time scale is reset
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
    public void QuitGame()
    {
        SoundFXManager.Instance.PlaySoundFX(pressSound, transform, volume);
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
