using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;


public class PauseManager : MonoBehaviour
{
    [SerializeField] private GameObject pausedScreen; //used to be public
    private bool isPaused = false;
    private PlayerInputActions inputActions;
    [SerializeField] private GameObject pauseButton; //used to be public

    [SerializeField] private AudioClip pressSound;
    [SerializeField] private float volume = 1f;

    [SerializeField] private Button quitButton;
    [SerializeField] private TMP_Text quitButtonText;

    [SerializeField] private Button signOutButton;

    [SerializeField] private LogicScript logicScript;

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
        #if UNITY_WEBGL
            quitButtonText.text = "Restart Game";
        #endif
        // SceneManager.sceneLoaded += OnSceneLoaded;
        signOutButton.onClick.AddListener(AuthManager.Instance.SignOut);
    }

    void OnDisable()
    {
        if (inputActions != null)
        {
            inputActions.UI.Pause.performed -= OnPausePerformed;
            inputActions.UI.Pause.Disable();
        }
        // SceneManager.sceneLoaded -= OnSceneLoaded;
        signOutButton.onClick.RemoveListener(AuthManager.Instance.SignOut);
    }

    // private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    // {
    //     // Ensure game is unpaused when a new scene is loaded
    //     isPaused = false;
    //     Time.timeScale = 1f; // Resume the game
    //     if (pausedScreen != null) pausedScreen.SetActive(false); // Hide the paused screen
    //     if (pauseButton != null) pauseButton.SetActive(true); // Show the pause button
    //     if (inputActions != null)
    //     {
    //         inputActions.Player.Enable(); // restore gameplay inputs
    //     }
    // }
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
        AudioManager.Instance.PlaySoundFX(pressSound, transform, volume);
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

        AudioManager.Instance.PlaySoundFX(pressSound, transform, volume);
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
        AudioManager.Instance.PlaySoundFX(pressSound, transform, volume);
        Time.timeScale = 1f; // Ensure time scale is reset
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
    public void QuitGame()
    {
        AudioManager.Instance.PlaySoundFX(pressSound, transform, volume);
        #if UNITY_WEBGL
            logicScript.RestartGame();
            //UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        #else
            Application.Quit();
        #endif
    }

    public void OpenSettings()
    {
        SettingsManager.Instance.OpenSettings();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Ensure quitButtonText has a font asset assigned
        if (quitButtonText != null && quitButtonText.font == null)
        {
            quitButtonText.font = TMP_Settings.defaultFontAsset;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
