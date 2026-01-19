using UnityEngine;
using Firebase.Auth;
using Firebase.Firestore;
using UnityEngine.UI;
using Firebase.Extensions;
using TMPro;
using Firebase;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

public class AuthManager : MonoBehaviour
{
    [SerializeField] private StartScreenScript startScreenScript;
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
    private FirebaseFirestore db;
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
                db = FirebaseFirestore.DefaultInstance;
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
        startScreenScript.CloseStartScreen();
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

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            Debug.LogError("Please fill in all fields");
            return;
        }

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
                startScreenScript.OpenStartScreen();
                isSignedIn = true;

                LoadUsername(auth.CurrentUser.UserId); // Fetch username from Firestore
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

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            Debug.LogError("Please fill in all fields");
            return;
        }

        // Step 1: Check if username is already taken
        db.Collection("users").WhereEqualTo("username", username).GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                if (task.Result.Count > 0)
                {
                    Debug.LogError("Username already exists! Pick another one.");
                    return;
                }
                else
                {
                    // Step 2: Create Firebase Auth user
                    auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(authTask =>
                    {
                        if (authTask.IsCanceled || authTask.IsFaulted)
                        {
                            Debug.LogError("Account Creation Failed: " + authTask.Exception);
                        }
                        else
                        {
                            string uid = auth.CurrentUser.UserId;
                            Debug.Log("Account Created! UID: " + uid);
                            isSignedIn = true;

                            // Step 3: Save username and initial stats to Firestore
                            DocumentReference docRef = db.Collection("users").Document(uid);
                            Dictionary<string, object> userData = new Dictionary<string, object>
                            {
                                {"username", username},
                                {"email", email},
                                {"normalHighScore", 0},
                                {"dailyHighScore", 0},
                                {"dailyHighScoreDate", DateTime.UtcNow.ToString("yyyyMMdd")},
                                {"totalRuns", 0},
                                {"dailyRankOnes", 0},
                                {"normalMinutesRankedOne",0}
                            };

                            docRef.SetAsync(userData).ContinueWithOnMainThread(fireTask =>
                            {
                                if (fireTask.IsCompleted)
                                {
                                    Debug.Log("User Firestore record created!");
                                    signInCanvas.SetActive(false);
                                    createAccountCanvas.SetActive(false);
                                    startScreenScript.OpenStartScreen();
                                }
                            });
                        }
                    });
                }
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
                startScreenScript.OpenStartScreen();
            }
        });
    }

     void LoadUsername(string uid)
    {
        DocumentReference docRef = db.Collection("users").Document(uid);
        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result.Exists)
            {
                string username = task.Result.GetValue<string>("username");
                Debug.Log("Welcome back, " + username);
                // You can now display username in your UI
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

    public string GetCurrentUserId()
    {
        return auth.CurrentUser != null ? auth.CurrentUser.UserId : null;
    }


}
