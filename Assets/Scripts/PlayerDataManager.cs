//save/load player data

using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

[Serializable]
public class PlayerProfile
{
    public string username;
    public string email;

    public int normalHighScore = 0;
    public int dailyHighScore = 0;
    public string dailyHighScoreDate = "";

    public int totalRuns = 0;
    public int totalPipes = 0;

    // --- Player Preferences ---
    //all linear volumes
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

        await SaveProfile();
    }

    public async Task LoadOrCreateUser()
    {
        uid = AuthManager.Instance.User.UserId;

        DocumentReference doc = db.Collection("users").Document(uid);
        var snap = await doc.GetSnapshotAsync();

        if (!snap.Exists)
        {
            Profile = new PlayerProfile();
            await SaveProfile();
        }
        else
        {
            Profile = snap.ConvertTo<PlayerProfile>();
        }
    }

    public async Task SaveProfile()
    {
        await db.Collection("users").Document(uid).SetAsync(Profile);
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
        if (Profile == null) return;

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

            if (isDaily)
            {
                string today = DateTime.UtcNow.ToString("yyyyMMdd");
                await db.Collection("leaderboards").Document("daily").Collection(today).Document(uid).SetAsync(entry);
            }
            else
            {
                await db.Collection("leaderboards").Document("normal").Collection("scores").Document(uid).SetAsync(entry);
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
    }
    else
    {
        if (score > Profile.normalHighScore)
        {
            Profile.normalHighScore = score;
            _ = SaveProfile();
            _ = UpdateLeaderboardEntry(isDaily: false);
        }
    }
}
}
