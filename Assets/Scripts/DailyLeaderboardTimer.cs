using UnityEngine;
using TMPro;
using System;

public class DailyLeaderboardTimer : MonoBehaviour
{
    [SerializeField] private TMP_Text countdownText;
    private bool isCountingDown = false;

    void Update()
    {
        if (isCountingDown)
        {
            UpdateCountdown();
        }
    }

    public void StartDailyCountdown()
    {
        if (countdownText == null)
        {
            Debug.LogError("DailyLeaderboardTimer: countdownText is not assigned!");
            return;
        }
        
        isCountingDown = true;
        countdownText.gameObject.SetActive(true);
        UpdateCountdown(); // Update immediately so it displays right away
    }

    public void StopDailyCountdown()
    {
        isCountingDown = false;
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
        }    
    }

    private void UpdateCountdown()
    {
        if (countdownText == null) return;

        // Get current UTC time
        DateTime now = DateTime.UtcNow;
        
        // Get tomorrow at midnight UTC
        DateTime tomorrow = now.Date.AddDays(1);
        
        // Calculate time remaining
        TimeSpan timeRemaining = tomorrow - now;
        
        // Format as HH:MM:SS
        string countdown = string.Format("{0:D2}:{1:D2}:{2:D2}",
            (int)timeRemaining.TotalHours,
            timeRemaining.Minutes,
            timeRemaining.Seconds);
        
        countdownText.text = $"Daily Reset In: {countdown}";
    }

    void Start()
    {
        if (countdownText == null)
        {
            GameObject countdownObj = GameObject.FindWithTag("CountdownText");
            if (countdownObj != null)
            {
                countdownText = countdownObj.GetComponent<TMP_Text>();
            }        
        }
    
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("DailyLeaderboardTimer: Could not find TMP_Text component!");
        }
    }
}