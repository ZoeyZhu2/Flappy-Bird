//save/load player data

using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;
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

    FirebaseFirestore db;
    string uid;

    public PlayerProfile Profile { get; private set; }

    //to track guest high scores
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

        db = FirebaseFirestore.DefaultInstance;
    }

    public async Task CreateNewUser(string username, string email)
    {
        uid = AuthManager.Instance.User.UserId;

        Profile = new PlayerProfile
        {
            username = username,
            email = email,
            dailyHighScoreDate = DateTime.UtcNow.ToString("yyyyMMdd")
        };

        try
        {
            await SaveProfile();
            Debug.Log("Profile saved successfully!");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Failed to save profile: " + ex.Message);
            throw;
        }
    }

    public async Task LoadOrCreateUser()
    {
        uid = AuthManager.Instance.User.UserId;

        if (AuthManager.Instance.IsGuest)
        {
            Debug.Log("Guest user detected, loading local scores");
            Profile = new PlayerProfile();
            Profile.normalHighScore = PlayerPrefs.GetInt(GUEST_HIGH_SCORE_KEY, 0);
            Profile.dailyHighScore = PlayerPrefs.GetInt(GUEST_DAILY_HIGH_SCORE_KEY, 0);
            Profile.dailyHighScoreDate = System.DateTime.UtcNow.ToString("yyyyMMdd");
            return;
        }

        DocumentReference doc = db.Collection("users").Document(uid);
        var snap = await doc.GetSnapshotAsync();

        if (!snap.Exists)
        {
            Debug.Log("User document doesn't exist, creating new profile");
            Profile = new PlayerProfile();
            await SaveProfile();
        }
        else
        {
            try
            {
                Profile = new PlayerProfile
                {
                    username = snap.GetValue<string>("username") ?? "",
                    email = snap.GetValue<string>("email") ?? "",
                    normalHighScore = snap.GetValue<int>("normalHighScore"),
                    dailyHighScore = snap.GetValue<int>("dailyHighScore"),
                    dailyHighScoreDate = snap.GetValue<string>("dailyHighScoreDate") ?? "",
                    totalRuns = snap.GetValue<int>("totalRuns"),
                    totalPipes = snap.GetValue<int>("totalPipes"),
                    musicVolume = snap.GetValue<float>("musicVolume"),
                    soundFXVolume = snap.GetValue<float>("soundFXVolume"),
                    musicMuted = snap.GetValue<bool>("musicMuted"),
                    soundFXMuted = snap.GetValue<bool>("soundFXMuted")
                };
                Debug.Log("Profile loaded successfully");
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Failed to convert document to PlayerProfile: " + ex.Message);
                Debug.Log("Document data: " + snap.GetValue<string>("username")); // Debug what's actually there
                
                // Fallback: create empty profile
                Profile = new PlayerProfile();
            }
        }
    }

    public async Task SaveProfile()
    {
        if (Profile == null) return;

        try
        {
            var data = new System.Collections.Generic.Dictionary<string, object>
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

            await db.Collection("users").Document(uid).SetAsync(data);
            Debug.Log("Profile saved successfully!");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Failed to save profile: " + ex.Message);
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

    // public void TryUpdateHighScore(int score, bool isDaily)
    // {
    //     if (Profile == null) return;

    //     if (isDaily)
    //     {
    //         string today = DateTime.UtcNow.ToString("yyyyMMdd");

    //         if (Profile.dailyHighScoreDate != today)
    //         {
    //             Profile.dailyHighScore = 0;
    //             Profile.dailyHighScoreDate = today;
    //         }

    //         if (score > Profile.dailyHighScore)
    //             Profile.dailyHighScore = score;
    //     }
    //     else
    //     {
    //         if (score > Profile.normalHighScore)
    //             Profile.normalHighScore = score;
    //     }
    //     _ = SaveProfile();
    // }

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

    // Add this to your existing PlayerDataManager class

    private async Task UpdateLeaderboardEntry(bool isDaily)
    {
        if (Profile == null)
        {
            Debug.LogError("UpdateLeaderboardEntry: Profile is null!");
            return;
        }
        
        // Don't save guest scores to leaderboard
        if (AuthManager.Instance.IsGuest)
        {
            Debug.Log("UpdateLeaderboardEntry: Skipping guest user");
            return;
        }

        // Validate uid and username
        if (string.IsNullOrEmpty(uid))
        {
            Debug.LogError("UpdateLeaderboardEntry: uid is not set!");
            return;
        }

        if (string.IsNullOrEmpty(Profile.username))
        {
            Debug.LogError("UpdateLeaderboardEntry: username is empty!");
            return;
        }

        try
        {
            long timestamp = ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds();
            
            var entry = new
            {
                userId = uid,
                username = Profile.username,
                score = isDaily ? Profile.dailyHighScore : Profile.normalHighScore,
                timestamp = timestamp
            };

            Debug.Log($"Saving leaderboard entry: userId={uid}, username={entry.username}, score={entry.score}, isDaily={isDaily}");

            if (isDaily)
            {
                string today = DateTime.UtcNow.ToString("yyyyMMdd");
                await db.Collection("leaderboards").Document("daily").Collection(today).Document(uid).SetAsync(entry);
                Debug.Log($"Daily leaderboard entry saved for {today}");
            }
            else
            {
                await db.Collection("leaderboards").Document("normal").Collection("scores").Document(uid).SetAsync(entry);
                Debug.Log("Normal leaderboard entry saved");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to update leaderboard: {e.Message}");
        }
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
                _ = UpdateLeaderboardEntry(isDaily: true);
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
                _ = UpdateLeaderboardEntry(isDaily: false);
            }
            if (AuthManager.Instance.IsGuest)
            {
                PlayerPrefs.SetInt(GUEST_HIGH_SCORE_KEY, Profile.normalHighScore);
            }
        }
    }
}
