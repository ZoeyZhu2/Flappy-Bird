using UnityEngine;
using System;

public enum GameMode
{
    Normal,
    DailySeed
}

public class GameModeManager : MonoBehaviour
{
    public static GameModeManager Instance;
    public GameMode currentMode;
    public int currentSeed;
    
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void SetNormalMode()
    {
        currentMode = GameMode.Normal;
        currentSeed = 0;
    }
    public void SetDailyMode()
    {
        currentMode = GameMode.DailySeed;
        currentSeed = DateTime.UtcNow.Date.GetHashCode();
    }
}
