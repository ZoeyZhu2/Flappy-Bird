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
            Auth = FirebaseAuth.DefaultInstance;
            Debug.Log("Firebase Auth ready.");
        });
    }

    public async Task<bool> SignIn(string email, string password)
    {
        try
        {
            await Auth.SignInWithEmailAndPasswordAsync(email, password);
            await PlayerDataManager.Instance.LoadOrCreateUser();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> CreateAccount(string email, string password, string username)
    {
        try
        {
            await Auth.CreateUserWithEmailAndPasswordAsync(email, password);

            await User.SendEmailVerificationAsync();

            await PlayerDataManager.Instance.CreateNewUser(username, email);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task GuestLogin()
    {
        await Auth.SignInAnonymouslyAsync();
    }

    public void SignOut()
    {
        Auth.SignOut();
    }
}
