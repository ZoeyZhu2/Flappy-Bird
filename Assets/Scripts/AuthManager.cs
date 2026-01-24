//login / logout / account creation

using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using System.Threading.Tasks;

public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance;

    public FirebaseAuth Auth { get; private set; }
    public FirebaseUser User => Auth?.CurrentUser;

    public bool IsSignedIn => User != null;
    public bool IsGuest => User != null && User.IsAnonymous;
    public bool IsEmailVerified => User != null && User.IsEmailVerified;
    
    private bool firebaseReady = false;
    public bool FirebaseReady => firebaseReady;

    void Awake()
    {
        // Singleton (only one survives across scenes)
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Initialize Firebase
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                Auth = FirebaseAuth.DefaultInstance;
                firebaseReady = true;
                Debug.Log("Firebase Auth ready.");
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + task.Result);
                firebaseReady = false;
            }
        });
    }

    public async Task<bool> SignIn(string email, string password)
    {
        if (!firebaseReady)
        {
            Debug.LogError("Firebase not initialized yet. Try again in a moment.");
            return false;
        }

        try
        {
            await Auth.SignInWithEmailAndPasswordAsync(email, password);
            await PlayerDataManager.Instance.LoadOrCreateUser();
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Sign in failed: " + ex.Message);
            return false;
        }
    }

    public async Task<bool> CreateAccount(string email, string password, string username)
    {
        if (!firebaseReady)
        {
            Debug.LogError("Firebase not initialized yet. Try again in a moment.");
            return false;
        }

        try
        {
            await Auth.CreateUserWithEmailAndPasswordAsync(email, password);
            await User.SendEmailVerificationAsync();
            await PlayerDataManager.Instance.CreateNewUser(username, email);
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Account creation failed: " + ex.Message);
            return false;
        }
    }

    public async Task GuestLogin()
    {
        if (!firebaseReady)
        {
            Debug.LogError("Firebase not initialized yet. Try again in a moment.");
            return;
        }

        try
        {
            await Auth.SignInAnonymouslyAsync();
            await PlayerDataManager.Instance.LoadOrCreateUser();
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Guest login failed: " + ex.Message);
        }
    }

    public void SignOut()
    {
        if (Auth != null)
        {
            Auth.SignOut();
        }
    }

    public async Task ResendVerificationEmail()
    {
        if (User != null && !User.IsEmailVerified)
        {
            await User.SendEmailVerificationAsync();
        }
    }
}