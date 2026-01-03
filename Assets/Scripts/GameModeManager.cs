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
    private GameMode currentMode;
    private int currentSeed;
    
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

    public int GetCurrentSeed()
    {
        return currentSeed;
    }

    public GameMode GetCurrentMode()
    {
        return currentMode;
    }

}
