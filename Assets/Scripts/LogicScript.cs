using UnityEngine;
using UnityEngine.UI;
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
    public void addScore(int scoreToAdd)
    {
        playerScore = playerScore + scoreToAdd;
        scoreText.text = "Current Score: " + playerScore.ToString();
    }
    
    public void restartGame()
    {
        Time.timeScale = 1f;
        //restarts scene again (so Start() is called again)
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void gameOver()
    {
        gameOverScreen.SetActive(true);
        Time.timeScale = 0f;

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
        playerScore = 0;
        scoreText.text = "Current Score: " + playerScore.ToString();

        //no game mode version
        /*
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        highScoreText.text = "High Score: " + highScore.ToString();
        */

        //game mode version
        key = GetHighScoreKey();
        highScore = PlayerPrefs.GetInt(key, 0); //0 is the alternative value if no value for key is found
        highScoreText.text = (GameModeManager.Instance.currentMode == GameMode.DailySeed ? "Daily High Score: " : "High Score: ") + highScore;


    }
}
