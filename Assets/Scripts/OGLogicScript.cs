// using UnityEngine;
// using UnityEngine.UI;
// using UnityEngine.InputSystem;
// using UnityEngine.SceneManagement;
// using System;
// using Firebase.Auth;
// using Firebase.Firestore;
// using System.Collections.Generic;
// using System.Threading.Tasks;
// using Firebase;
// using Firebase.Extensions;
// using TMPro;

// public class LogicScript : MonoBehaviour
// {
//     private int playerScore;
//     [SerializeField] private Text scoreText; //used to be public
//     [SerializeField] private GameObject gameOverScreen; //used to be public
//     private int highScore;
//     [SerializeField] private Text highScoreText; //used to be public
//     // private string key;
//     private PlayerInputActions inputActions;
//     private bool isGameOver = false;
//     [SerializeField] private AudioClip pressSound;
//     [SerializeField] private float volume = 1f; //used to be public
//     [SerializeField] private AuthManager authManager;
//     private FirebaseFirestore db;
//     private string userId;

//     //new high score stuff
//     [SerializeField] private TMP_Text newHighScore;

//     [SerializeField] private Button openCreateAccount;
//     [SerializeField] private Button skipButton;
//     [SerializeField] private Button openSignIn;

//     //create account pop up
//     [SerializeField] private GameObject createAccountCanvas;
//     [SerializeField] private TMP_InputField emailInputC;
//     [SerializeField] private TMP_InputField passwordInputC;
//     [SerializeField] private TMP_InputField usernameInputC;
//     [SerializeField] private Button createAccountButton;

//     //sign in pop up

//     [SerializeField] private GameObject signInCanvas;
//     [SerializeField] private TMP_InputField emailInputS;
//     [SerializeField] private TMP_InputField passwordInputS;
//     [SerializeField] private Button signInButton;


//     void Awake()
//     {
//         // key = GetHighScoreKey();
//         inputActions = InputManager.inputActions;
//         db = FirebaseFirestore.DefaultInstance;
//         newHighScore.text = "New High Score!";
//         newHighScore.gameObject.SetActive(false);
//         openCreateAccount.gameObject.SetActive(false);
//         openSignIn.gameObject.SetActive(false);
//         skipButton.gameObject.SetActive(false);
//         createAccountCanvas.SetActive(false);
//         signInCanvas.SetActive(false);
//     }

//     void OnEnable()
//     {
//         if (inputActions != null)
//         {
//             inputActions.UI.PlayAgain.performed += OnPlayAgain;
//             inputActions.UI.PlayAgain.Enable();
//         }
//         openCreateAccount.onClick.AddListener(ShowCreateAccount);
//         skipButton.onClick.AddListener(Skip);
//         openSignIn.onClick.AddListener(ShowSignIn);
//         createAccountButton.onClick.AddListener(authManager.CreateAccount);
//         signInButton.onClick.AddListener(authManager.SignIn);

//     }

//     void OnDisable()
//     {
//         if (inputActions != null)
//         {
//             inputActions.UI.PlayAgain.performed -= OnPlayAgain;
//             inputActions.UI.PlayAgain.Disable();
//         }
//         createAccountButton.onClick.RemoveListener(ShowCreateAccount);
//         skipButton.onClick.RemoveListener(Skip);
//         signInButton.onClick.RemoveListener(ShowSignIn);
//         createAccountButton.onClick.RemoveListener(authManager.CreateAccount);
//         signInButton.onClick.RemoveListener(authManager.SignIn);
//     }

//     private void OnPlayAgain(InputAction.CallbackContext ctx)
//     {
//         if (isGameOver && ctx.performed)
//         {
//             RestartGame();
//         }
//     }

//     private string GetHighScoreKey()
//     {
//         if (GameModeManager.Instance.GetCurrentMode() == GameMode.DailySeed)
//         {
//             // Unique key for each day
//             // return "DailyHighScore_" + DateTime.UtcNow.ToString("yyyyMMdd");
//             return "dailyHighScore";
//         }
//         else
//         {
//             // Normal mode uses a fixed key
//             // return "NormalHighScore";
//             return "normalHighScore";
//         }
//     }
//     public void AddScore(int scoreToAdd)
//     {
//         playerScore = playerScore + scoreToAdd;
//         scoreText.text = "Current Score: " + playerScore.ToString();
//     }
    
//     public void RestartGame()
//     {
//         // createAccountCanvas.SetActive(false);
//         // signInCanvas.SetActive(false);
//         // highScoreText.gameObject.SetActive(false);
//         AudioManager.Instance.PlaySoundFX(pressSound, transform, volume);
//         AudioManager.Instance.MusicUnpause();
//         isGameOver = false; 
//         inputActions.UI.PlayAgain.Disable();
//         inputActions.UI.Pause.Enable();
//         inputActions.Player.Enable();
//         Time.timeScale = 1f;
//         //restarts scene again (so Start() is called again)
//         SceneManager.LoadScene(SceneManager.GetActiveScene().name);
//     }

//     public void GameOver()
//     {
//         isGameOver = true;
//         gameOverScreen.SetActive(true);
        
//         AudioManager.Instance.MusicPause();
        


//         Time.timeScale = 0f;
//         inputActions.Player.Disable();
//         inputActions.UI.Pause.Disable();
//         inputActions.UI.PlayAgain.Enable();


//         //no game mode version
//         /*
//         if (playerScore > highScore)
//         {
//             highScore = playerScore;
//             PlayerPrefs.SetInt("HighScore", highScore);
//             PlayerPrefs.Save();
//         }

//         highScoreText.text = "High Score: " + highScore.ToString();
//         */

//         //game mode version
//         // highScore = PlayerPrefs.GetInt(key, 0);

//         AddOneRun();


//         if (playerScore > highScore)
//         {
//             // PlayerPrefs.SetInt(key, playerScore);
//             // PlayerPrefs.Save();
//             highScoreText.gameObject.SetActive(true);
//             highScore = playerScore;

//             if (!string.IsNullOrEmpty(userId))
//             {
//                 string key = GetHighScoreKey();
//                 DocumentReference docRef = db.Collection("users").Document(userId);
//                 Dictionary<string, object> update = new Dictionary<string, object>
//                 {
//                     { key, highScore }
//                 };
//                 if (GameModeManager.Instance.GetCurrentMode() == GameMode.DailySeed)
//                 {
//                     update["dailyHighScoreDate"] = DateTime.UtcNow.ToString("yyyyMMdd");
//                 }
//                 docRef.SetAsync(update, SetOptions.MergeAll).ContinueWithOnMainThread(task =>
//                 {
//                     if (task.IsCompleted)
//                     {
//                         Debug.Log("High score updated in Firestore: " + highScore);
//                     }
//                     else
//                     {
//                         Debug.LogError("Failed to update high score: " + task.Exception);
//                     }
//                 });
//             }
//             else
//             {
//                 createAccountButton.gameObject.SetActive(false);
//                 signInButton.gameObject.SetActive(false);
//             }
//         }
//         highScoreText.text = (GameModeManager.Instance.GetCurrentMode() == GameMode.DailySeed ? "Daily High Score: " : "High Score: ") + highScore;
//     }

//     void Start()
//     {
//         gameOverScreen.SetActive(false);
//         createAccountCanvas.SetActive(false);
//         signInCanvas.SetActive(false);
//         highScoreText.gameObject.SetActive(false);

//         // if (AudioManager.Instance != null) 
//         // { 
//         //     AudioManager.Instance.PlayGameMusic(); 
//         // } 
//         // else 
//         // { 
//         //     Debug.LogWarning("AudioManager or startScreenMusic is not assigned!"); 
//         // }
        
//         playerScore = 0;
//         scoreText.text = "Current Score: " + playerScore.ToString();

//         //no game mode version
//         /*
//         highScore = PlayerPrefs.GetInt("HighScore", 0);
//         highScoreText.text = "High Score: " + highScore.ToString();
//         */

//         //game mode version
//         // highScore = PlayerPrefs.GetInt(key, 0); //0 is the alternative value if no value for key is found
//         // highScoreText.text = (GameModeManager.Instance.GetCurrentMode() == GameMode.DailySeed ? "Daily High Score: " : "High Score: ") + highScore;

//         if (authManager.GetIsSignedIn())
//         {
//             userId = authManager.GetCurrentUserId(); // we’ll make a getter in AuthManager
//         }
//         else
//         {
//             userId = null; // guest, optional: skip saving high score
//         }

//         // Load high score from Firestore
//         LoadHighScore();

//     }

//     private void LoadHighScore()
//     {
//         //I want daily highscore to reset every 24 hours UTC
//         if (string.IsNullOrEmpty(userId)) return; // guest

//         DocumentReference docRef = db.Collection("users").Document(userId);

//         docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
//         {
//             if (task.IsCompleted && task.Result.Exists)
//             {
//                 if (GameModeManager.Instance.GetCurrentMode() == GameMode.DailySeed)
//                 {
//                     string today = DateTime.UtcNow.ToString("yyyyMMdd");
//                     string lastDate = task.Result.ContainsField("dailyHighScoreDate") 
//                     ? task.Result.GetValue<string>("dailyHighScoreDate") 
//                     : "";
//                     if (lastDate != today)
//                     {
//                         // Reset daily score
//                         docRef.UpdateAsync(new Dictionary<string, object>
//                         {
//                             {"dailyHighScore", 0},
//                             {"dailyHighScoreDate", today}
//                         });
//                         highScore = 0;
//                     }
//                     else
//                     {
//                         highScore = (int)task.Result.GetValue<long>("dailyHighScore");
//                     }
//                 }
//                 else
//                 {
//                     highScore = (int)task.Result.GetValue<long>("normalHighScore");
//                 }

//                 highScoreText.text = (GameModeManager.Instance.GetCurrentMode() == GameMode.DailySeed ? "Daily High Score: " : "High Score: ") + highScore;
//             }
//             else
//             {
//                 Debug.LogWarning("Failed to load high score or document missing");
//                 highScore = 0;
//                 highScoreText.text = (GameModeManager.Instance.GetCurrentMode() == GameMode.DailySeed ? "Daily High Score: " : "High Score: ") + highScore;
//             }
//         });
//     }

//     private void AddOneRun()
//     {
//         if (!string.IsNullOrEmpty(userId))
//         {
//             DocumentReference docRef = db.Collection("users").Document(userId);
//             Dictionary<string, object> update = new Dictionary<string, object>
//             {
//                 { "totalRuns", FieldValue.Increment(1) }
//             };
//             docRef.SetAsync(update, SetOptions.MergeAll).ContinueWithOnMainThread(task =>
//             {
//                 if (task.IsCompleted)
//                     Debug.Log("Total runs incremented.");
//                 else
//                     Debug.LogError("Failed to increment total runs: " + task.Exception);
//             });
//         }
//     }

//     private void ShowCreateAccount()
//     {
//         createAccountCanvas.SetActive(true);
//     }

//     private void Skip()
//     {
//         RestartGame();
//     }
    
//     private void ShowSignIn()
//     {
//         signInCanvas.SetActive(true);
//     }

// }
