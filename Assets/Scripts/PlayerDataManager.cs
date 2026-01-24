using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

[System.Serializable]
public class PlayerProfile
{
    public string username;
    public string email;

    public int normalHighScore = 0;
    public int dailyHighScore = 0;
    public string dailyHighScoreDate = "";

    public int totalRuns = 0;
    public int totalPipes = 0;

    public float musicVolume = 0.05f;
    public float soundFXVolume = 1f;
    public bool musicMuted = false;
    public bool soundFXMuted = false;
}

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance;

    private FirebaseRestFirestore db;
    private string uid;

    public PlayerProfile Profile { get; private set; }

    private const string GUEST_HIGH_SCORE_KEY = "guest_normal_high_score";
    private const string GUEST_DAILY_HIGH_SCORE_KEY = "guest_daily_high_score";

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public async Task CreateNewUser(string username, string email)
    {
        uid = AuthManager.Instance.UserId;

        Profile = new PlayerProfile
        {
            username = username,
            email = email,
            dailyHighScoreDate = DateTime.UtcNow.ToString("yyyyMMdd")
        };

        await SaveProfile();
    }

    public async Task LoadOrCreateUser()
    {
        uid = AuthManager.Instance.UserId;

        if (AuthManager.Instance.IsGuest)
        {
            Debug.Log("Guest user detected, loading local scores");
            Profile = new PlayerProfile();
            Profile.normalHighScore = PlayerPrefs.GetInt(GUEST_HIGH_SCORE_KEY, 0);
            Profile.dailyHighScore = PlayerPrefs.GetInt(GUEST_DAILY_HIGH_SCORE_KEY, 0);
            Profile.dailyHighScoreDate = DateTime.UtcNow.ToString("yyyyMMdd");
            return;
        }

        // Initialize Firestore with user's auth token
        if (db == null)
        {
            db = new FirebaseRestFirestore(AuthManager.Instance.IdToken);
        }

        try
        {
            var userData = await db.GetDocument("users", uid);

            if (userData == null)
            {
                Debug.Log("User document doesn't exist, creating new profile");
                Profile = new PlayerProfile();
                await SaveProfile();
            }
            else
            {
                Profile = new PlayerProfile
                {
                    username = userData.ContainsKey("username") ? userData["username"].ToString() : "",
                    email = userData.ContainsKey("email") ? userData["email"].ToString() : "",
                    normalHighScore = userData.ContainsKey("normalHighScore") ? Convert.ToInt32(userData["normalHighScore"]) : 0,
                    dailyHighScore = userData.ContainsKey("dailyHighScore") ? Convert.ToInt32(userData["dailyHighScore"]) : 0,
                    dailyHighScoreDate = userData.ContainsKey("dailyHighScoreDate") ? userData["dailyHighScoreDate"].ToString() : "",
                    totalRuns = userData.ContainsKey("totalRuns") ? Convert.ToInt32(userData["totalRuns"]) : 0,
                    totalPipes = userData.ContainsKey("totalPipes") ? Convert.ToInt32(userData["totalPipes"]) : 0,
                    musicVolume = userData.ContainsKey("musicVolume") ? Convert.ToSingle(userData["musicVolume"]) : 0.05f,
                    soundFXVolume = userData.ContainsKey("soundFXVolume") ? Convert.ToSingle(userData["soundFXVolume"]) : 1f,
                    musicMuted = userData.ContainsKey("musicMuted") ? (bool)userData["musicMuted"] : false,
                    soundFXMuted = userData.ContainsKey("soundFXMuted") ? (bool)userData["soundFXMuted"] : false
                };
                Debug.Log("Profile loaded successfully");
                Debug.Log($"  Normal High Score: {Profile.normalHighScore}");
                Debug.Log($"  Daily High Score: {Profile.dailyHighScore}");
                Debug.Log($"  Daily High Score Date: {Profile.dailyHighScoreDate}");
                
                // Submit existing high scores to leaderboard collections
                if (Profile.normalHighScore > 0)
                {
                    Debug.Log($"[SUBMIT] Submitting normal score: {Profile.normalHighScore}");
                    _ = SubmitScoreToLeaderboard(Profile.normalHighScore, false);
                }
                
                // Only submit daily score if it's from today
                string today = DateTime.UtcNow.ToString("yyyyMMdd");
                Debug.Log($"[SUBMIT] Today's date: {today}, Saved date: {Profile.dailyHighScoreDate}");
                if (Profile.dailyHighScore > 0 && Profile.dailyHighScoreDate == today)
                {
                    Debug.Log($"[SUBMIT] Submitting daily score: {Profile.dailyHighScore}");
                    _ = SubmitScoreToLeaderboard(Profile.dailyHighScore, true);
                }
                else
                {
                    Debug.Log($"[SUBMIT] NOT submitting daily score - dailyHighScore={Profile.dailyHighScore}, isToday={Profile.dailyHighScoreDate == today}");
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to load profile: {ex.Message}");
            Profile = new PlayerProfile();
        }
    }

    public async Task SaveProfile()
    {
        if (Profile == null) return;

        if (AuthManager.Instance.IsGuest)
        {
            // Guests save locally
            return;
        }

        try
        {
            var data = new Dictionary<string, object>
            {
                { "username", Profile.username },
                { "email", Profile.email },
                { "normalHighScore", Profile.normalHighScore },
                { "dailyHighScore", Profile.dailyHighScore },
                { "dailyHighScoreDate", Profile.dailyHighScoreDate },
                { "totalRuns", Profile.totalRuns },
                { "totalPipes", Profile.totalPipes },
                { "musicVolume", Profile.musicVolume },
                { "soundFXVolume", Profile.soundFXVolume },
                { "musicMuted", Profile.musicMuted },
                { "soundFXMuted", Profile.soundFXMuted }
            };

            await db.SetDocument("users", uid, data);
            Debug.Log("Profile saved successfully!");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to save profile: {ex.Message}");
        }
    }

    public void AddRun()
    {
        if (Profile == null) return;
        Profile.totalRuns++;
        _ = SaveProfile();
    }

    public void AddPipe()
    {
        if (Profile == null) return;
        Profile.totalPipes++;
        _ = SaveProfile();
    }

    public void TryUpdateHighScore(int score, bool isDaily)
    {
        if (Profile == null) return;

        if (isDaily)
        {
            string today = DateTime.UtcNow.ToString("yyyyMMdd");

            if (Profile.dailyHighScoreDate != today)
            {
                Profile.dailyHighScore = 0;
                Profile.dailyHighScoreDate = today;
            }

            if (score > Profile.dailyHighScore)
            {
                Profile.dailyHighScore = score;
                _ = SaveProfile();
                _ = SubmitScoreToLeaderboard(Profile.dailyHighScore, true);
            }

            if (AuthManager.Instance.IsGuest)
            {
                PlayerPrefs.SetInt(GUEST_DAILY_HIGH_SCORE_KEY, Profile.dailyHighScore);
            }
        }
        else
        {
            if (score > Profile.normalHighScore)
            {
                Profile.normalHighScore = score;
                _ = SaveProfile();
                _ = SubmitScoreToLeaderboard(Profile.normalHighScore, false);
            }

            if (AuthManager.Instance.IsGuest)
            {
                PlayerPrefs.SetInt(GUEST_HIGH_SCORE_KEY, Profile.normalHighScore);
            }
        }
    }

    private async Task SubmitScoreToLeaderboard(int score, bool isDaily)
    {
        if (AuthManager.Instance.IsGuest || db == null) 
        {
            Debug.Log($"SubmitScoreToLeaderboard skipped: IsGuest={AuthManager.Instance.IsGuest}, dbNull={db == null}");
            return;
        }

        try
        {
            if (isDaily)
            {
                string today = DateTime.UtcNow.ToString("yyyyMMdd");
                var leaderboardData = new Dictionary<string, object>
                {
                    { "userId", uid },
                    { "username", Profile.username },
                    { "score", score },
                    { "timestamp", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }
                };
                Debug.Log($"Submitting daily score to leaderboards/daily/{today}/{uid}");
                await db.SetDocument($"leaderboards/daily/{today}", uid, leaderboardData);
                Debug.Log($"Daily score submitted to leaderboard: {score}");
            }
            else
            {
                var leaderboardData = new Dictionary<string, object>
                {
                    { "userId", uid },
                    { "username", Profile.username },
                    { "score", score },
                    { "timestamp", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }
                };
                Debug.Log($"Submitting normal score to leaderboards/normal/scores/{uid}");
                await db.SetDocument("leaderboards/normal/scores", uid, leaderboardData);
                Debug.Log($"Normal score submitted to leaderboard: {score}");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to submit score to leaderboard: {ex.Message}\n{ex.StackTrace}");
        }
    }

    public void UpdateMusicVolume(float value)
    {
        if (Profile == null) return;
        Profile.musicVolume = value;
        _ = SaveProfile();
    }

    public void UpdateSoundFXVolume(float value)
    {
        if (Profile == null) return;
        Profile.soundFXVolume = value;
        _ = SaveProfile();
    }

    public void UpdateMusicMute(bool muted)
    {
        if (Profile == null) return;
        Profile.musicMuted = muted;
        _ = SaveProfile();
    }

    public void UpdateSoundFXMute(bool muted)
    {
        if (Profile == null) return;
        Profile.soundFXMuted = muted;
        _ = SaveProfile();
    }

    public async Task SyncGuestScoresToAccount()
    {
        // When a guest signs in, transfer their guest scores to the new account if they're higher
        if (Profile == null) return;

        int guestNormalScore = PlayerPrefs.GetInt(GUEST_HIGH_SCORE_KEY, 0);
        int guestDailyScore = PlayerPrefs.GetInt(GUEST_DAILY_HIGH_SCORE_KEY, 0);

        bool updated = false;

        // Sync normal score if guest score is higher
        if (guestNormalScore > Profile.normalHighScore)
        {
            Profile.normalHighScore = guestNormalScore;
            updated = true;
            Debug.Log($"[SYNC] Updated normal score from guest: {guestNormalScore}");
        }

        // Sync daily score if guest score is higher and from today
        string today = DateTime.UtcNow.ToString("yyyyMMdd");
        if (guestDailyScore > Profile.dailyHighScore)
        {
            Profile.dailyHighScore = guestDailyScore;
            Profile.dailyHighScoreDate = today;
            updated = true;
            Debug.Log($"[SYNC] Updated daily score from guest: {guestDailyScore}");
        }

        // Save if anything was updated
        if (updated)
        {
            await SaveProfile();
            Debug.Log("[SYNC] Guest scores synced to account");
        }
    }
}