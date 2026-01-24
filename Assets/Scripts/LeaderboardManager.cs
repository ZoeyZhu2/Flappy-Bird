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
    [SerializeField] private int maxDisplayRows = 100;
    [SerializeField] private float cacheTimeSeconds = 60f; //how long to keep data before fetching from Firestore again
    private FirebaseFirestore db;
    private List<LeaderboardEntry> cachedLeaderboard = new(); //stores most recetnly fetched leaderboard in memory
    private float lastLoadTime = -999f; //so first load always happens

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
    }

    //default is that isDaily is false
    public async Task LoadLeaderboard(bool isDaily = false)
    {
        if (Time.realtimeSinceStartup - lastLoadTime < cacheTimeSeconds)
        {
            DisplayLeaderboard();
            return;
        }
        try
        {
            cachedLeaderboard.Clear();

            if (isDaily) //query today's leaderboard directly
            {
                string today = System.DateTime.UtcNow.ToString("yyyyMMdd");
                var query = db.Collection("leaderboards").Document("daily").Collection(today)
                    .OrderByDescending("score")
                    .Limit(maxDisplayRows);
                var snapshot = await query.GetSnapshotAsync();
                if (snapshot.Documents.Count() == 0)
                {
                    Debug.LogWarning("No scores found for today's leaderboard");
                }
                
                foreach (var doc in snapshot.Documents)
                {
                    var entry = doc.ConvertTo<LeaderboardEntry>();
                    cachedLeaderboard.Add(entry);
                }
            }
            else
            {
               var query = db.Collection("leaderboards").Document("normal").Collection("scores")
                .OrderByDescending("score")
                .Limit(maxDisplayRows); 
                var snapshot = await query.GetSnapshotAsync();
                foreach (var doc in snapshot.Documents)
                {
                    var entry = doc.ConvertTo<LeaderboardEntry>();
                    cachedLeaderboard.Add(entry);
                }
            }
            
            lastLoadTime = Time.realtimeSinceStartup;
            DisplayLeaderboard();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load leaderboard: {e.Message}");
        }
    }

    private void DisplayLeaderboard()
    {   
        //clear existing rows
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < cachedLeaderboard.Count; i++)
        {
            GameObject rowObj = Instantiate(rowPrefab, content); //creates a prefab as child of content
            LeaderboardRow row = rowObj.GetComponent<LeaderboardRow>(); //getting LeaderboardRow script
            row.SetRow(i+1, cachedLeaderboard[i].username, cachedLeaderboard[i].score);
        }
    }

    public void RefreshLeaderboard(bool isDaily = false)
    {
        lastLoadTime = -999f; //make cache "expired"
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
