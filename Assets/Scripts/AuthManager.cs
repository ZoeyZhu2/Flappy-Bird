using UnityEngine;
using Firebase.Auth;
using UnityEngine.UI;
using Firebase.Extensions;
using TMPro;

public class AuthManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField emailInputS;
    [SerializeField] private TMP_InputField passwordInputS;
    [SerializeField] private Button signInButtonS;
    [SerializeField] private Button openCreateAccount;
    [SerializeField] private Button guestButton;
    [SerializeField] private GameObject signInCanvas;

    [SerializeField] private GameObject createAccountCanvas;
    [SerializeField] private Button closeButtonC;
    [SerializeField] private TMP_InputField emailInputC;
    [SerializeField] private TMP_InputField usernameInputC;
    [SerializeField] private TMP_InputField passwordInputC;
    [SerializeField] private Button createAccountButton;

    
    private FirebaseAuth auth;
    private bool isSignedIn = false;

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                Debug.Log("Firebase ready!");
                auth = FirebaseAuth.DefaultInstance;
            }
            else
            {
                Debug.LogError("Could not resolve Firebase dependencies: " + task.Result);
            }
        });
    }

    void OnEnable()
    {
        signInButtonS.onClick.AddListener(SignIn);
        openCreateAccount.onClick.AddListener(OpenCreateAccount);
        createAccountButton.onClick.AddListener(CreateAccount);
        guestButton.onClick.AddListener(SignInAnonymously);
        closeButtonC.onClick.AddListener(CloseCreateAccount);
        signInCanvas.SetActive(true);
        createAccountCanvas.SetActive(false);
    }

    void OnDisable()
    {
        signInButtonS.onClick.RemoveListener(SignIn);
        openCreateAccount.onClick.RemoveListener(OpenCreateAccount);
        createAccountButton.onClick.RemoveListener(CreateAccount);
        guestButton.onClick.RemoveListener(SignInAnonymously);
        closeButtonC.onClick.RemoveListener(CloseCreateAccount);
    }

    void SignIn()
    {
        string email = emailInputS.text;
        string password = passwordInputS.text;

        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("Sign In Failed: " + task.Exception);
            }
            else
            {
                Debug.Log("Signed In! User: " + auth.CurrentUser.UserId);
                signInCanvas.SetActive(false); // hide pop-up
                isSignedIn = true;
            }
        });
    }

    public bool GetIsSignedIn()
    {
        return isSignedIn;
    }

    void CreateAccount()
    {
        string email = emailInputC.text;
        string username = usernameInputC.text;
        string password = passwordInputC.text;

        //make it so that each email and username can only be used once
        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("Account Creation Failed: " + task.Exception);
            }
            else
            {
                Debug.Log("Account Created! User: " + auth.CurrentUser.UserId);
                signInCanvas.SetActive(false);
                createAccountCanvas.SetActive(false);
            }
        });
    }

    void SignInAnonymously()
    {
        auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("Guest Sign In Failed: " + task.Exception);
            }
            else
            {
                Debug.Log("Signed In as Guest! User: " + auth.CurrentUser.UserId);
                signInCanvas.SetActive(false);
            }
        });
    }

    void OpenCreateAccount()
    {
        createAccountCanvas.SetActive(true);
    }
    void CloseCreateAccount()
    {
        createAccountCanvas.SetActive(false);
    }

}
