//gameplay and UI only

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class LogicScript : MonoBehaviour
{
    private int playerScore;
    private int highScore;
    private bool isGameOver = false;

    private PlayerInputActions inputActions;

    [Header("UI")]
    [SerializeField] private Text scoreText;
    [SerializeField] private Text highScoreText;
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private Button playAgainButton;
    [SerializeField] private TMP_Text newHighScore;
    [SerializeField] private TMP_Text verifyEmailWarning;
    [SerializeField] private Button resendEmailButton;

    [Header("Guest UI")]
    [SerializeField] private Button openCreateAccount;
    [SerializeField] private Button openSignIn;
    [SerializeField] private Button skipButton;

    [Header("Create Account")]
    [SerializeField] private GameObject createAccountCanvas;
    [SerializeField] private TMP_InputField emailInputC;
    [SerializeField] private TMP_InputField passwordInputC;
    [SerializeField] private TMP_InputField usernameInputC;
    [SerializeField] private Button createAccountButton;
    [SerializeField] private Button closeCreateAccountButton;

    [Header("Sign In")]
    [SerializeField] private GameObject signInCanvas;
    [SerializeField] private TMP_InputField emailInputS;
    [SerializeField] private TMP_InputField passwordInputS;
    [SerializeField] private Button signInButton;
    [SerializeField] private Button closeSignInButton;

    [Header("Audio")]
    [SerializeField] private AudioClip pressSound;
    [SerializeField] private float volume = 1f;

    void Awake()
    {
        inputActions = InputManager.inputActions;

        gameOverScreen.SetActive(false);
        createAccountCanvas.SetActive(false);
        signInCanvas.SetActive(false);

        openCreateAccount.gameObject.SetActive(false);
        openSignIn.gameObject.SetActive(false);
        skipButton.gameObject.SetActive(false);

        newHighScore.gameObject.SetActive(false);
        verifyEmailWarning.gameObject.SetActive(false);
        resendEmailButton.gameObject.SetActive(false);

        verifyEmailWarning.text = "Verify Email to save score to leaderboard!";
    }

    void OnEnable()
    {
        inputActions.UI.PlayAgain.performed += OnPlayAgain;
        inputActions.UI.PlayAgain.Enable();

        playAgainButton.onClick.AddListener(RestartGame);

        openCreateAccount.onClick.AddListener(ShowCreateAccount);
        openSignIn.onClick.AddListener(ShowSignIn);
        skipButton.onClick.AddListener(RestartGame);

        createAccountButton.onClick.AddListener(CreateAccount);
        signInButton.onClick.AddListener(SignIn);

        closeCreateAccountButton.onClick.AddListener(() => createAccountCanvas.SetActive(false));
        closeSignInButton.onClick.AddListener(() => signInCanvas.SetActive(false));

        resendEmailButton.onClick.AddListener(ResendEmail);
    }

    void OnDisable()
    {
        inputActions.UI.PlayAgain.performed -= OnPlayAgain;
        playAgainButton.onClick.RemoveListener(RestartGame);
        openCreateAccount.onClick.RemoveAllListeners();
        openSignIn.onClick.RemoveAllListeners();
        skipButton.onClick.RemoveAllListeners();
        createAccountButton.onClick.RemoveAllListeners();
        signInButton.onClick.RemoveAllListeners();
    }

    void Start()
    {
        playerScore = 0;
        scoreText.text = $"Current Score: {playerScore}";

        if (AuthManager.Instance.IsSignedIn)
        {
            // Make sure profile is loaded before trying to access it
            if (PlayerDataManager.Instance.Profile == null)
            {
                Debug.LogWarning("LogicScript: Profile not loaded yet, loading now");
                StartCoroutine(LoadProfileAndDisplayScore());
            }
            else
            {
                LoadHighScoreFromProfile();
            }
        }
        else
        {
            highScoreText.text= "High Score: 0";
        }
    }

    private IEnumerator LoadProfileAndDisplayScore()
    {
        yield return StartCoroutine(LoadProfileCoroutine());
        LoadHighScoreFromProfile();
    }

    private IEnumerator LoadProfileCoroutine()
    {
        var task = PlayerDataManager.Instance.LoadOrCreateUser();
        while (!task.IsCompleted)
        {
            yield return null;
        }
        if (task.IsFaulted)
        {
            Debug.LogError($"Failed to load profile: {task.Exception}");
        }
        else
        {
            var profile = PlayerDataManager.Instance.Profile;
            Debug.Log($"Profile loaded successfully");
            Debug.Log($"  Username: {profile?.username}");
            Debug.Log($"  Normal High Score: {profile?.normalHighScore}");
            Debug.Log($"  Daily High Score: {profile?.dailyHighScore}");
            Debug.Log($"  Daily High Score Date: {profile?.dailyHighScoreDate}");
        }
    }

    public void AddScore(int amount)
    {
        playerScore += amount;
        scoreText.text = $"Current Score: {playerScore}";
    }

    public void GameOver()
    {
        isGameOver = true;
        gameOverScreen.SetActive(true);

        AudioManager.Instance.MusicPause();
        Time.timeScale = 0f;

        inputActions.Player.Disable();
        inputActions.UI.Pause.Disable();
        inputActions.UI.PlayAgain.Enable();

        PlayerDataManager.Instance.AddRun();

        bool isDaily = GameModeManager.Instance.GetCurrentMode() == GameMode.DailySeed;

        // Update high scores for both signed-in users and guests
        if (PlayerDataManager.Instance != null)
            PlayerDataManager.Instance.TryUpdateHighScore(playerScore, isDaily);

        if (AuthManager.Instance != null && AuthManager.Instance.IsSignedIn && !AuthManager.Instance.IsGuest)
        {
            int newHigh = GetCurrentHighScore(isDaily);
            if (playerScore == newHigh && playerScore > 0)
            {
                newHighScore.gameObject.SetActive(true);
                newHighScore.text = "NEW HIGH SCORE: " + playerScore;
            }
            
            // Show verify email warning if email not verified
            if (!AuthManager.Instance.IsEmailVerified)
            {
                verifyEmailWarning.gameObject.SetActive(true);
                resendEmailButton.gameObject.SetActive(true);
            }
            
            // Hide guest buttons
            openCreateAccount.gameObject.SetActive(false);
            openSignIn.gameObject.SetActive(false);
            skipButton.gameObject.SetActive(false);
        }
        else
        {
            int newHigh = GetCurrentHighScore(isDaily);
            if (playerScore == newHigh && playerScore > 0)
            {
                newHighScore.gameObject.SetActive(true);
                newHighScore.text = "NEW HIGH SCORE: " + playerScore;

                // Player is a guest - show sign in options
                openCreateAccount.gameObject.SetActive(true);
                openSignIn.gameObject.SetActive(true);
                skipButton.gameObject.SetActive(true);

                // Hide verify email stuff
                verifyEmailWarning.gameObject.SetActive(false);
                resendEmailButton.gameObject.SetActive(false);
            }
            
        }

        highScore = GetCurrentHighScore(isDaily);
        highScoreText.text = (isDaily ? "Daily High Score: " : "High Score: ") + highScore;
    }
    private int GetCurrentHighScore(bool isDaily)
    {
        var profile = PlayerDataManager.Instance.Profile;
        if (profile == null) return 0;

        return isDaily ? profile.dailyHighScore : profile.normalHighScore;
    }

    private void LoadHighScoreFromProfile()
    {
        if (AuthManager.Instance == null || GameModeManager.Instance == null)
        {
            highScoreText.text = "High Score: 0";
            return;
        }

        bool isDaily = GameModeManager.Instance.GetCurrentMode() == GameMode.DailySeed;
        highScore = GetCurrentHighScore(isDaily);
        highScoreText.text = (isDaily ? "Daily High Score: " : "High Score: ") + highScore;
    }

    public void RestartGame()
    {
        AudioManager.Instance.PlaySoundFX(pressSound, transform, volume);
        AudioManager.Instance.MusicUnpause();

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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
            createAccountCanvas.SetActive(false);
            openCreateAccount.gameObject.SetActive(false);
            openSignIn.gameObject.SetActive(false);
            skipButton.gameObject.SetActive(false);

            // Sync any guest scores to the new account
            await PlayerDataManager.Instance.SyncGuestScoresToAccount();
            LoadHighScoreFromProfile();
        }
    }

    private async void SignIn()
    {
        bool success = await AuthManager.Instance.SignIn(
            emailInputS.text,
            passwordInputS.text
        );

        if (success)
        {
            signInCanvas.SetActive(false);
            openCreateAccount.gameObject.SetActive(false);
            openSignIn.gameObject.SetActive(false);
            skipButton.gameObject.SetActive(false);

            // Sync any guest scores to the account
            await PlayerDataManager.Instance.SyncGuestScoresToAccount();
            LoadHighScoreFromProfile();
        }
    }

    private void ShowCreateAccount() => createAccountCanvas.SetActive(true);
    private void ShowSignIn() => signInCanvas.SetActive(true);

    private void OnPlayAgain(InputAction.CallbackContext ctx)
    {
        if (isGameOver && ctx.performed)
            RestartGame();
    }

    private async void ResendEmail()
    {
        await AuthManager.Instance.ResendVerificationEmail();

        verifyEmailWarning.text = "Verification Email Sent!";
        StartCoroutine(WaitThreeSeconds());
        verifyEmailWarning.text = "Verify Email to save score to leaderboard!";
    }

    IEnumerator WaitThreeSeconds()
    {
        yield return new WaitForSecondsRealtime(3);
    }
}