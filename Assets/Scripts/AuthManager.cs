using UnityEngine;
using System.Threading.Tasks;

public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance;

    private FirebaseRestAuth auth;
    private const string WEB_API_KEY = "AIzaSyA-cTGbJEmFEOnkXqjAyv4chzSm3Btr3ho";

    public string UserId => auth?.UserId;
    public string Email => auth?.Email;
    public string IdToken => auth?.IdToken;
    public bool IsSignedIn => auth?.IsSignedIn ?? false;
    public bool IsGuest => auth?.IsAnonymous ?? false;
    public bool IsEmailVerified => auth?.EmailVerified ?? false;

    private bool firebaseReady = false;
    public bool FirebaseReady => firebaseReady;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Initialize Firebase REST Auth
        auth = new FirebaseRestAuth(WEB_API_KEY);
        firebaseReady = true;
        Debug.Log("Firebase REST Auth initialized.");
    }

    public async Task<bool> SignIn(string email, string password)
    {
        if (!firebaseReady)
        {
            Debug.LogError("Firebase not initialized yet.");
            return false;
        }

        try
        {
            Debug.Log($"[AuthManager] Attempting sign-in for email: {email}");
            bool success = await auth.SignInWithEmailPassword(email, password);
            if (success)
            {
                Debug.Log($"[AuthManager] Sign-in successful for user: {auth.UserId}");
                await PlayerDataManager.Instance.LoadOrCreateUser();
                return true;
            }
            else
            {
                Debug.LogError("[AuthManager] Sign-in returned false");
                return false;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AuthManager] Sign in failed with exception: {ex.Message}\nStackTrace: {ex.StackTrace}");
            return false;
        }
    }

    public async Task<bool> CreateAccount(string email, string password, string username)
    {
        if (!firebaseReady)
        {
            Debug.LogError("Firebase not initialized yet.");
            return false;
        }

        try
        {
            bool success = await auth.SignUpWithEmailPassword(email, password);
            if (success)
            {
                await PlayerDataManager.Instance.CreateNewUser(username, email);
                return true;
            }
            return false;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Account creation failed: {ex.Message}");
            return false;
        }
    }

    public async Task GuestLogin()
    {
        if (!firebaseReady)
        {
            Debug.LogError("Firebase not initialized yet.");
            return;
        }

        try
        {
            await auth.SignInAnonymously();
            await PlayerDataManager.Instance.LoadOrCreateUser();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Guest login failed: {ex.Message}");
        }
    }

    public void SignOut()
    {
        if (auth != null)
        {
            auth.SignOut();
        }
    }

    public async Task ResendVerificationEmail()
    {
        if (!IsSignedIn || IsGuest)
        {
            Debug.LogError("User not signed in or is a guest");
            return;
        }

        try
        {
            Debug.Log("[AuthManager] Sending verification email...");
            await auth.SendEmailVerification();
            Debug.Log("[AuthManager] Verification email sent successfully");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AuthManager] Failed to send verification email: {ex.Message}");
        }
    }
}