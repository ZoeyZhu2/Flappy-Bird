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
        isCountingDown = true;
        countdownText.gameObject.SetActive(true);
    }

    public void StopDailyCountdown()
    {
        isCountingDown = false;
        countdownText.gameObject.SetActive(false);
    }

    private void UpdateCountdown()
    {
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
        countdownText.gameObject.SetActive(false);
    }
}