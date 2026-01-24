using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.Collections.Generic;

[System.Serializable]
public class AuthResponse
{
    public string idToken;
    public string localId;
    public string email;
    public bool emailVerified;
}

[System.Serializable]
public class SignUpResponse
{
    public string idToken;
    public string email;
    public string localId;
}

public class FirebaseRestAuth
{
    private const string PROJECT_ID = "flappy-bird-ce77c";
    private const string SIGN_UP_URL = "https://identitytoolkit.googleapis.com/v1/accounts:signUp?key=";
    private const string SIGN_IN_URL = "https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key=";
    private const string GET_ACCOUNT_URL = "https://identitytoolkit.googleapis.com/v1/accounts:lookup?key=";

    private string apiKey;
    public string UserId { get; private set; }
    public string IdToken { get; private set; }
    public string Email { get; private set; }
    public bool IsSignedIn => !string.IsNullOrEmpty(IdToken);
    public bool IsAnonymous { get; private set; }

    public FirebaseRestAuth(string webApiKey)
    {
        apiKey = webApiKey;
        LoadTokenFromStorage();
    }

    public async Task<bool> SignUpWithEmailPassword(string email, string password)
    {
        var requestData = new Dictionary<string, object>
        {
            { "email", email },
            { "password", password },
            { "returnSecureToken", true }
        };

        try
        {
            var response = await PostRequest<SignUpResponse>(SIGN_UP_URL + apiKey, requestData);
            
            if (response != null)
            {
                IdToken = response.idToken;
                UserId = response.localId;
                Email = response.email;
                IsAnonymous = false;
                SaveTokenToStorage();
                return true;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Sign up failed: {ex.Message}");
        }
        
        return false;
    }

    public async Task<bool> SignInWithEmailPassword(string email, string password)
    {
        var requestData = new Dictionary<string, object>
        {
            { "email", email },
            { "password", password },
            { "returnSecureToken", true }
        };

        try
        {
            var response = await PostRequest<AuthResponse>(SIGN_IN_URL + apiKey, requestData);
            
            if (response != null)
            {
                IdToken = response.idToken;
                UserId = response.localId;
                Email = response.email;
                IsAnonymous = false;
                SaveTokenToStorage();
                return true;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Sign in failed: {ex.Message}");
        }
        
        return false;
    }

    public async Task<bool> SignInAnonymously()
    {
        // For anonymous auth, generate a fake token
        UserId = System.Guid.NewGuid().ToString();
        IdToken = "anonymous_" + UserId;
        IsAnonymous = true;
        SaveTokenToStorage();
        await Task.CompletedTask;
        return true;
    }

    public void SignOut()
    {
        IdToken = null;
        UserId = null;
        Email = null;
        IsAnonymous = false;
        PlayerPrefs.DeleteKey("firebase_id_token");
        PlayerPrefs.DeleteKey("firebase_user_id");
        PlayerPrefs.DeleteKey("firebase_email");
        PlayerPrefs.Save();
    }

    private async Task<T> PostRequest<T>(string url, Dictionary<string, object> data) where T : class
    {
        // Manually build JSON string to avoid serialization issues
        string json = "{";
        var keys = new List<string>(data.Keys);
        for (int i = 0; i < keys.Count; i++)
        {
            var key = keys[i];
            var value = data[key];
            
            if (value is string)
                json += $"\"{key}\":\"{value}\"";
            else if (value is bool)
                json += $"\"{key}\":{((bool)value ? "true" : "false")}";
            else
                json += $"\"{key}\":{value}";
            
            if (i < keys.Count - 1)
                json += ",";
        }
        json += "}";
        
        Debug.Log($"Sending JSON: {json}");
        
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Referer", "http://localhost");

            var asyncOp = request.SendWebRequest();
            
            while (!asyncOp.isDone)
            {
                await Task.Delay(10);
            }

            if (request.result == UnityWebRequest.Result.Success)
            {
                return JsonUtility.FromJson<T>(request.downloadHandler.text);
            }
            else
            {
                Debug.LogError($"Request failed: {request.error}\n{request.downloadHandler.text}");
                return null;
            }
        }
    }

    private void SaveTokenToStorage()
    {
        PlayerPrefs.SetString("firebase_id_token", IdToken);
        PlayerPrefs.SetString("firebase_user_id", UserId);
        PlayerPrefs.SetString("firebase_email", Email);
        PlayerPrefs.SetInt("firebase_is_anonymous", IsAnonymous ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void LoadTokenFromStorage()
    {
        // Auto sign-in disabled - user must sign in manually each time
        IdToken = "";
        UserId = "";
        Email = "";
        IsAnonymous = false;
    }
}