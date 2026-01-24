using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

[Serializable]
public class LeaderboardEntry
{
    public string userId;
    public string username;
    public int score;
    public long timestamp;
}

public class LeaderboardManager : MonoBehaviour
{
    [SerializeField] private GameObject rowPrefab;
    [SerializeField] private Transform content;
    [SerializeField] private int maxDisplayRows = 20;
    [SerializeField] private float cacheTimeSeconds = 60f;

    private FirebaseFirestore db;
    private List<LeaderboardEntry> cachedLeaderboard = new();
    private float lastLoadTime = -999f;
    private float lastDailyLoadTime = -999f;
    private bool lastLoadWasDaily = false;

    void Start()
    {
        if (db == null)
        {
            db = FirebaseFirestore.DefaultInstance;
        }
    }

    public async Task LoadLeaderboard(bool isDaily = false)
    {
        if (content == null)
        {
            Debug.LogError("LeaderboardManager: content Transform is not assigned!");
            return;
        }

        // Wait for Firebase to be ready
        int retries = 0;
        while (db == null && retries < 50)
        {
            db = FirebaseFirestore.DefaultInstance;
            if (db == null)
            {
                await System.Threading.Tasks.Task.Delay(100);
                retries++;
            }
        }

        if (db == null)
        {
            Debug.LogError("LeaderboardManager: Firebase Firestore failed to initialize!");
            return;
        }

        if (rowPrefab == null)
        {
            Debug.LogError("LeaderboardManager: rowPrefab is not assigned!");
            return;
        }
        
        // Return cached data if still valid and same type
        if (lastLoadWasDaily == isDaily) // Same type as last load
        {
            if (isDaily && Time.realtimeSinceStartup - lastDailyLoadTime < cacheTimeSeconds)
            {
                Debug.Log("Using cached daily leaderboard");
                DisplayLeaderboard();
                return;
            }
            else if (!isDaily && Time.realtimeSinceStartup - lastLoadTime < cacheTimeSeconds)
            {
                Debug.Log("Using cached normal leaderboard");
                DisplayLeaderboard();
                return;
            }
        }

        try
        {
            cachedLeaderboard.Clear();
            
            if (isDaily)
            {
                // Query today's leaderboard directly
                string today = System.DateTime.UtcNow.ToString("yyyyMMdd");
                var query = db.Collection("leaderboards").Document("daily").Collection(today)
                    .OrderByDescending("score")
                    .Limit(maxDisplayRows);
                
                var snapshot = await query.GetSnapshotAsync();
                
                if (snapshot.Documents.Count() == 0)
                {
                    Debug.LogWarning("No scores found for today's leaderboard");
                    DisplayLeaderboard(); // Clear the display
                    return;
                }
                
                foreach (var doc in snapshot.Documents)
                {
                    var entry = new LeaderboardEntry
                    {
                        userId = doc.GetValue<string>("userId") ?? "",
                        username = doc.GetValue<string>("username") ?? "",
                        score = doc.GetValue<int>("score"),
                        timestamp = doc.GetValue<long>("timestamp")
                    };
                    cachedLeaderboard.Add(entry);
                }
            }
            else
            {
                // Query all-time leaderboard
                var query = db.Collection("leaderboards").Document("normal").Collection("scores")
                    .OrderByDescending("score")
                    .Limit(maxDisplayRows);
                
                var snapshot = await query.GetSnapshotAsync();
                
                foreach (var doc in snapshot.Documents)
                {
                    var entry = new LeaderboardEntry
                    {
                        userId = doc.GetValue<string>("userId") ?? "",
                        username = doc.GetValue<string>("username") ?? "",
                        score = doc.GetValue<int>("score"),
                        timestamp = doc.GetValue<long>("timestamp")
                    };
                    cachedLeaderboard.Add(entry);
                }
            }

            lastLoadTime = Time.realtimeSinceStartup;
            lastLoadWasDaily = isDaily;
            if (isDaily)
                lastDailyLoadTime = Time.realtimeSinceStartup;
            
            DisplayLeaderboard();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load leaderboard: {e.Message}");
        }
    }

    private void DisplayLeaderboard()
    {
        Debug.Log($"DisplayLeaderboard called with {cachedLeaderboard.Count} entries");

        // Clear existing rows (only destroy LeaderboardRow components)
        foreach (Transform child in content)
        {
            if (child.GetComponent<LeaderboardRow>() != null)
            {
                Destroy(child.gameObject);
            }
        }

        // Instantiate and populate rows
        for (int i = 0; i < cachedLeaderboard.Count; i++)
        {
            Debug.Log($"Creating row {i+1}: {cachedLeaderboard[i].username} - {cachedLeaderboard[i].score}");

            GameObject rowObj = Instantiate(rowPrefab, content);
            LeaderboardRow row = rowObj.GetComponent<LeaderboardRow>();
            
            if (row == null)
            {
                Debug.LogError($"LeaderboardRow component not found on prefab!");
                continue;
            }
            row.SetRow(i + 1, cachedLeaderboard[i].username, cachedLeaderboard[i].score);
        }
        Debug.Log("DisplayLeaderboard finished");

    }

    public void RefreshLeaderboard(bool isDaily = false)
    {
        lastLoadTime = -999f; // Force reload
        _ = LoadLeaderboard(isDaily);
    }

    public int GetPlayerRank(string userId)
    {
        for (int i = 0; i < cachedLeaderboard.Count; i++)
        {
            if (cachedLeaderboard[i].userId == userId)
            {
                return i + 1;
            }
        }
        return -1;
    }
    private bool IsToday(long timestamp)
    {
        var date = UnixTimeStampToDateTime(timestamp);
        return date.Date == DateTime.UtcNow.Date;
    }

    private DateTime UnixTimeStampToDateTime(long unixTimeStamp)
    {
        System.DateTime dateTime = new(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds(unixTimeStamp).ToUniversalTime();
        return dateTime;
    }
}
