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
        countdownTimer.StartDailyCountdown();
    }

    public void HideDailyLeaderboard()
    {
        countdownTimer.StopDailyCountdown();
    }

    public async void ShowNormalLeaderboard()
    {
        await leaderboardManager.LoadLeaderboard();
        countdownTimer.StopDailyCountdown();
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