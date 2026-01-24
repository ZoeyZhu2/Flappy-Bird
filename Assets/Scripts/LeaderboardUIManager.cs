using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LeaderboardUIController : MonoBehaviour
{
    [SerializeField] private LeaderboardManager leaderboardManager;
    [SerializeField] private DailyLeaderboardTimer countdownTimer;
    [SerializeField] private Button normalLeaderboard;
    [SerializeField] private Button dailyLeaderboard;
    [SerializeField] private Button closeButton;


    void OnEnable()
    {
        normalLeaderboard.onClick.AddListener(ShowNormalLeaderboard);
        dailyLeaderboard.onClick.AddListener(ShowDailyLeaderboard);
        closeButton.onClick.AddListener(CloseLeaderboard);
    }

    void OnDisable()
    {
        normalLeaderboard.onClick.RemoveListener(ShowNormalLeaderboard);
        dailyLeaderboard.onClick.RemoveListener(ShowDailyLeaderboard);
        closeButton.onClick.RemoveListener(CloseLeaderboard);
    }

    public async void ShowDailyLeaderboard()
    {
        await leaderboardManager.LoadLeaderboard(isDaily: true);
        if (countdownTimer != null)
        {
            countdownTimer.StartDailyCountdown();
        }
    }

    public void HideDailyLeaderboard()
    {
        if (countdownTimer != null)
        {
            countdownTimer.StopDailyCountdown();
        }
    }

    public async void ShowNormalLeaderboard()
    {
        HideDailyLeaderboard();
        await leaderboardManager.LoadLeaderboard();
        if (countdownTimer != null)
        {
            countdownTimer.StopDailyCountdown();
        }
    }


    void Start()
    {
        ShowNormalLeaderboard();
    }

    private void CloseLeaderboard()
    {
        SceneManager.UnloadSceneAsync(gameObject.scene);
    }

}