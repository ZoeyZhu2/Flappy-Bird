using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[System.Serializable]
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

    private FirebaseRestFirestore db;
    private List<LeaderboardEntry> cachedLeaderboard = new();
    private float lastLoadTime = -999f;
    private bool cachedIsDaily = false; // Track which type was cached

    void Start()
    {
        // Wait a moment for AuthManager to be available
        if (db == null)
        {
            if (!string.IsNullOrEmpty(AuthManager.Instance?.IdToken))
            {
                db = new FirebaseRestFirestore(AuthManager.Instance.IdToken);
            }
            else
            {
                Debug.LogWarning("LeaderboardManager: AuthManager not ready yet, will retry on LoadLeaderboard");
            }
        }
    }

    void OnEnable()
    {
        if (db == null && !string.IsNullOrEmpty(AuthManager.Instance.IdToken))
        {
            db = new FirebaseRestFirestore(AuthManager.Instance.IdToken);
        }
    }

    public async Task LoadLeaderboard(bool isDaily = false)
    {
        Debug.Log($"Loading leaderboard, isDaily={isDaily}");
        Debug.Log($"Firestore DB initialized: {db != null}");
        Debug.Log($"Auth token: {AuthManager.Instance.IdToken?.Substring(0, 20)}...");

        // Initialize db if needed
        if (db == null)
        {
            if (string.IsNullOrEmpty(AuthManager.Instance.IdToken))
            {
                Debug.LogError("LeaderboardManager: No auth token available!");
                return;
            }
            db = new FirebaseRestFirestore(AuthManager.Instance.IdToken);
        }

        if (content == null)
        {
            Debug.LogError("LeaderboardManager: content Transform is not assigned!");
            return;
        }

        if (rowPrefab == null)
        {
            Debug.LogError("LeaderboardManager: rowPrefab is not assigned!");
            return;
        }

        // Return cached data if still valid AND it's the same leaderboard type
        if (Time.realtimeSinceStartup - lastLoadTime < cacheTimeSeconds && cachedIsDaily == isDaily)
        {
            DisplayLeaderboard();
            return;
        }

        try
        {
            cachedLeaderboard.Clear();

            if (isDaily)
            {
                // Query today's daily leaderboard
                string today = DateTime.UtcNow.ToString("yyyyMMdd");
                Debug.Log($"Querying daily leaderboard path: leaderboards/daily/{today}");
                var results = await db.QueryDocuments($"leaderboards/daily/{today}");
                Debug.Log($"Daily query returned {results.Count} results");
                
                if (results.Count == 0)
                {
                    Debug.LogWarning("No scores found for today's leaderboard");
                }
                
                foreach (var doc in results)
                {
                    Debug.Log($"Daily doc keys: {string.Join(", ", doc.Keys)}");
                    foreach (var kvp in doc)
                    {
                        Debug.Log($"  {kvp.Key} = {kvp.Value}");
                    }
                }

                foreach (var doc in results)
                {
                    var entry = new LeaderboardEntry
                    {
                        userId = doc.ContainsKey("userId") ? doc["userId"].ToString() : "",
                        username = doc.ContainsKey("username") ? doc["username"].ToString() : "",
                        score = doc.ContainsKey("score") ? Convert.ToInt32(doc["score"]) : 0,
                        timestamp = doc.ContainsKey("timestamp") ? Convert.ToInt64(doc["timestamp"]) : 0
                    };
                    cachedLeaderboard.Add(entry);
                }

                // Sort by score descending and limit
                cachedLeaderboard = cachedLeaderboard
                    .OrderByDescending(e => e.score)
                    .Take(maxDisplayRows)
                    .ToList();
            }
            else
            {
                // Query all-time normal leaderboard
                Debug.Log("Querying normal leaderboard path: leaderboards/normal/scores");

                var results = await db.QueryDocuments("leaderboards/normal/scores");
                Debug.Log($"Normal query returned {results.Count} results");

                foreach (var doc in results)
                {
                    var entry = new LeaderboardEntry
                    {
                        userId = doc.ContainsKey("userId") ? doc["userId"].ToString() : "",
                        username = doc.ContainsKey("username") ? doc["username"].ToString() : "",
                        score = doc.ContainsKey("score") ? Convert.ToInt32(doc["score"]) : 0,
                        timestamp = doc.ContainsKey("timestamp") ? Convert.ToInt64(doc["timestamp"]) : 0
                    };
                    cachedLeaderboard.Add(entry);
                }

                // Sort by score descending and limit
                cachedLeaderboard = cachedLeaderboard
                    .OrderByDescending(e => e.score)
                    .Take(maxDisplayRows)
                    .ToList();
            }

            lastLoadTime = Time.realtimeSinceStartup;
            cachedIsDaily = isDaily;  // Track which type is now cached
            DisplayLeaderboard();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load leaderboard: {e.Message}\n{e.StackTrace}");
        }
    }

    private void DisplayLeaderboard()
    {
        Debug.Log($"DisplayLeaderboard called with {cachedLeaderboard.Count} entries");

        // Clear existing rows (skip first child which is the labels row)
        for (int i = content.childCount - 1; i > 0; i--)
        {
            Destroy(content.GetChild(i).gameObject);
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
}