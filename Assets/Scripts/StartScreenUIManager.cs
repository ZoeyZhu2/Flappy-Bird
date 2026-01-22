using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;


public class StartScreenUIManager : MonoBehaviour
{
    //Start Screen
    [SerializeField] private GameObject startScreen;
    [SerializeField] private Button normalButton;
    [SerializeField] private Button dailyButton;
    [SerializeField] private Button signOutButton;
    [SerializeField] private Button quitButton;

    //Audio
    [SerializeField] private AudioClip pressSound;
    [SerializeField] private float volume = 1f; //used to be public

    //Sign in UI
    [SerializeField] private GameObject signInCanvas;
    [SerializeField] private TMP_InputField emailInputS;
    [SerializeField] private TMP_InputField passwordInputS;
    [SerializeField] private Button signInButton;
    [SerializeField] private Button openCreateAccount;
    [SerializeField] private Button guestButton;


    //Create Account pop up UI"
    [SerializeField] private GameObject createAccountCanvas;
    [SerializeField] private TMP_InputField emailInputC;
    [SerializeField] private TMP_InputField passwordInputC;
    [SerializeField] private TMP_InputField usernameInputC;
    [SerializeField] private Button createAccountButton;
    [SerializeField] private Button closeCreateAccountButton;


    public void Start()
    {
        OpenSignInScreen();
        CloseStartScreen();
    }

    void OnEnable()
    {
        signInButton.onClick.AddListener(SignIn);
        openCreateAccount.onClick.AddListener(ShowCreateAccount);
        guestButton.onClick.AddListener(PlayAsGuest);
        createAccountButton.onClick.AddListener(CreateAccount);
        closeCreateAccountButton.onClick.AddListener(CloseCreateAccount);

        normalButton.onClick.AddListener(PlayNormal);
        dailyButton.onClick.AddListener(PlayDaily);
        signOutButton.onClick.AddListener(AuthManager.Instance.SignOut);
        quitButton.onClick.AddListener(QuitGame);
    }

    void OnDisable()
    {
        signInButton.onClick.RemoveListener(SignIn);
        openCreateAccount.onClick.RemoveListener(ShowCreateAccount);
        guestButton.onClick.RemoveListener(PlayAsGuest);
        createAccountButton.onClick.RemoveListener(CreateAccount);
        closeCreateAccountButton.onClick.RemoveListener(CloseCreateAccount);

        normalButton.onClick.RemoveListener(PlayNormal);
        dailyButton.onClick.RemoveListener(PlayDaily);
        signOutButton.onClick.RemoveListener(AuthManager.Instance.SignOut);
        quitButton.onClick.RemoveListener(QuitGame);
    }

    private async void SignIn()
    {
        bool success = await AuthManager.Instance.SignIn(
            emailInputS.text,
            passwordInputS.text
        );

        if (success)
        {
            CloseSignInScreen();
            OpenStartScreen();
        }
    }

    private async void CreateAccount()
    {
        bool success = await AuthManager.Instance.CreateAccount(
            emailInputC.text,
            passwordInputC.text,
            usernameInputC.text
        );

        if (success)
        {
            CloseCreateAccount();
            CloseSignInScreen();
            OpenStartScreen();
        }
    }

    private async void PlayAsGuest()
    {
        await AuthManager.Instance.GuestLogin();
    }
    private void ShowCreateAccount()
    {
        createAccountCanvas.SetActive(true);
    }

    private void CloseCreateAccount()
    {
        createAccountCanvas.SetActive(false);
    }

    private void OpenSignInScreen()
    {
        signInCanvas.SetActive(true);
    }

    private void CloseSignInScreen()
    {
        signInCanvas.SetActive(false);
    }

    private void OpenStartScreen()
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



    private void CloseStartScreen()
    {
        startScreen.SetActive(false);
    }
    
    private void PlayNormal()
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

    private void PlayDaily()
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

    private void QuitGame()
    {
        AudioManager.Instance.PlaySoundFX(pressSound, transform, volume);
        Application.Quit();
    }
    
}
