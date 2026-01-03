using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System;

public class LogicScript : MonoBehaviour
{
    private int playerScore;
    public Text scoreText;
    public GameObject gameOverScreen;
    private int highScore;
    public Text highScoreText;
    private string key;
    private PlayerInputActions inputActions;
    private bool isGameOver = false;
    [SerializeField] private AudioClip pressSound;
    public float volume = 1f;

    void Awake()
    {
        key = GetHighScoreKey();
        inputActions = InputManager.inputActions;
    }

    void OnEnable()
    {
        if (inputActions != null)
        {
            inputActions.UI.PlayAgain.performed += OnPlayAgain;
            inputActions.UI.PlayAgain.Enable();
        }
    }

    void OnDisable()
    {
        if (inputActions != null)
        {
            inputActions.UI.PlayAgain.performed -= OnPlayAgain;
            inputActions.UI.PlayAgain.Disable();
        }
    }

    private void OnPlayAgain(InputAction.CallbackContext ctx)
    {
        if (isGameOver && ctx.performed)
        {
            RestartGame();
        }
    }

    private string GetHighScoreKey()
    {
        if (GameModeManager.Instance.currentMode == GameMode.DailySeed)
        {
            // Unique key for each day
            return "DailyHighScore_" + DateTime.UtcNow.ToString("yyyyMMdd");
        }
        else
        {
            // Normal mode uses a fixed key
            return "NormalHighScore";
        }
    }
    public void AddScore(int scoreToAdd)
    {
        playerScore = playerScore + scoreToAdd;
        scoreText.text = "Current Score: " + playerScore.ToString();
    }
    
    public void RestartGame()
    {
        AudioManager.Instance.PlaySoundFX(pressSound, transform, volume);
        AudioManager.Instance.MusicUnpause();
        isGameOver = false; 
        inputActions.UI.PlayAgain.Disable();
        inputActions.UI.Pause.Enable();
        inputActions.Player.Enable();
        Time.timeScale = 1f;
        //restarts scene again (so Start() is called again)
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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


        //no game mode version
        /*
        if (playerScore > highScore)
        {
            highScore = playerScore;
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
        }

        highScoreText.text = "High Score: " + highScore.ToString();
        */

        //game mode version
        highScore = PlayerPrefs.GetInt(key, 0);

        if (playerScore > highScore)
        {
            PlayerPrefs.SetInt(key, playerScore);
            PlayerPrefs.Save();
            highScore = playerScore;
        }
        highScoreText.text = (GameModeManager.Instance.currentMode == GameMode.DailySeed ? "Daily High Score: " : "High Score: ") + highScore;
    }

    void Start()
    {
        gameOverScreen.SetActive(false);

        // if (AudioManager.Instance != null) 
        // { 
        //     AudioManager.Instance.PlayGameMusic(); 
        // } 
        // else 
        // { 
        //     Debug.LogWarning("AudioManager or startScreenMusic is not assigned!"); 
        // }
        
        playerScore = 0;
        scoreText.text = "Current Score: " + playerScore.ToString();

        //no game mode version
        /*
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        highScoreText.text = "High Score: " + highScore.ToString();
        */

        //game mode version
        highScore = PlayerPrefs.GetInt(key, 0); //0 is the alternative value if no value for key is found
        highScoreText.text = (GameModeManager.Instance.currentMode == GameMode.DailySeed ? "Daily High Score: " : "High Score: ") + highScore;


    }
}
