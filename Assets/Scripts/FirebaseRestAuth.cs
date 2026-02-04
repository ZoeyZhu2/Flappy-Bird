using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections;

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

public class FirebaseRestAuth : MonoBehaviour
{
    private const string PROJECT_ID = "flappy-bird-ce77c";
    private const string SIGN_UP_URL = "https://identitytoolkit.googleapis.com/v1/accounts:signUp?key=";
    private const string SIGN_IN_URL = "https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key=";
    private const string GET_ACCOUNT_URL = "https://identitytoolkit.googleapis.com/v1/accounts:lookup?key=";

    private string apiKey;
    public string UserId { get; private set; }
    public string IdToken { get; private set; }
    public string Email { get; private set; }
    public bool EmailVerified { get; private set; }
    public bool IsSignedIn => !string.IsNullOrEmpty(IdToken);
    public bool IsAnonymous { get; private set; }
    
    private static GameObject coroutineRunner;

    public FirebaseRestAuth(string webApiKey)
    {
        apiKey = webApiKey;
        LoadTokenFromStorage();
        EnsureCoroutineRunner();
    }
    
    private static void EnsureCoroutineRunner()
    {
        if (coroutineRunner == null)
        {
            coroutineRunner = new GameObject("FirebaseRestAuthCoroutineRunner");
            coroutineRunner.AddComponent<CoroutineRunner>();
            GameObject.DontDestroyOnLoad(coroutineRunner);
        }
    }
    
    private class CoroutineRunner : MonoBehaviour
    {
        // Just exists to run coroutines
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
                EmailVerified = false; // Newly created accounts are not verified
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
            Debug.Log("[FirebaseRestAuth] Calling PostRequest for sign-in...");
            var response = await PostRequest<AuthResponse>(SIGN_IN_URL + apiKey, requestData);
            
            Debug.Log($"[FirebaseRestAuth] PostRequest returned: {(response == null ? "null" : "object")}");
            
            if (response != null)
            {
                Debug.Log($"[FirebaseRestAuth] Response fields - idToken: {(string.IsNullOrEmpty(response.idToken) ? "NULL/EMPTY" : "HAS VALUE")}");
                Debug.Log($"[FirebaseRestAuth] Response fields - localId: {response.localId}");
                Debug.Log($"[FirebaseRestAuth] Response fields - email: {response.email}");
                Debug.Log($"[FirebaseRestAuth] Response fields - emailVerified: {response.emailVerified}");
                
                IdToken = response.idToken;
                UserId = response.localId;
                Email = response.email;
                EmailVerified = response.emailVerified;
                IsAnonymous = false;
                SaveTokenToStorage();
                
                Debug.Log($"[FirebaseRestAuth] Sign-in successful! Token saved. IsSignedIn: {IsSignedIn}, EmailVerified: {EmailVerified}");
                return true;
            }
            else
            {
                Debug.LogError("[FirebaseRestAuth] PostRequest returned null!");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[FirebaseRestAuth] Sign in failed: {ex.Message}\n{ex.StackTrace}");
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
        Debug.Log("[PostRequest] Starting PostRequest method");
        
        // Manually build JSON string to avoid serialization issues
        string json = "{";
        var keys = new List<string>(data.Keys);
        for (int i = 0; i < keys.Count; i++)
        {
            var key = keys[i];
            var value = data[key];
            
            if (value is string)
            {
                // Escape quotes and special characters in strings
                string escapedValue = ((string)value)
                    .Replace("\\", "\\\\")
                    .Replace("\"", "\\\"")
                    .Replace("\n", "\\n")
                    .Replace("\r", "\\r");
                json += $"\"{key}\":\"{escapedValue}\"";
            }
            else if (value is bool)
                json += $"\"{key}\":{((bool)value ? "true" : "false")}";
            else
                json += $"\"{key}\":{value}";
            
            if (i < keys.Count - 1)
                json += ",";
        }
        json += "}";
        
        Debug.Log($"Sending JSON: {json}");
        Debug.Log($"Sending to URL: {url}");
        
        var result = new TaskCompletionSource<T>();
        
        EnsureCoroutineRunner();
        coroutineRunner.GetComponent<CoroutineRunner>().StartCoroutine(PostRequestCoroutine<T>(url, json, (res) => 
        {
            Debug.Log("[PostRequest] Callback received with result: " + (res == null ? "null" : "object"));
            result.SetResult(res);
        }));
        
        Debug.Log("[PostRequest] Waiting for coroutine result...");
        return await result.Task;
    }
    
    private IEnumerator PostRequestCoroutine<T>(string url, string json, System.Action<T> callback) where T : class
    {
        Debug.Log("[PostRequestCoroutine] Starting coroutine");
        
        var request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        
        Debug.Log("[PostRequestCoroutine] Sending request...");
        yield return request.SendWebRequest();
        
        Debug.Log("[PostRequestCoroutine] Request completed");
        Debug.Log($"[PostRequest] Request result: {request.result}");
        Debug.Log($"[PostRequest] Response Code: {request.responseCode}");
        
        string responseText = request.downloadHandler?.text ?? "";
        Debug.Log($"[PostRequest] Response text length: {responseText.Length}");
        Debug.Log($"[PostRequest] Full response text: {responseText}");
        
        T result = null;
        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"[PostRequest] Request was successful, attempting to parse response...");
            try
            {
                result = JsonUtility.FromJson<T>(responseText);
                if (result == null)
                {
                    Debug.LogError("[PostRequest] JSON parsing returned null!");
                }
                else
                {
                    Debug.Log($"[PostRequest] Successfully parsed response");
                }
            }
            catch (System.Exception parseEx)
            {
                Debug.LogError($"[PostRequest] Failed to parse JSON: {parseEx.Message}");
            }
        }
        else
        {
            Debug.LogError($"[PostRequest] Request failed - Error: {request.error}");
            Debug.LogError($"[PostRequest] Response Code: {request.responseCode}");
            Debug.LogError($"[PostRequest] Response: {responseText}");
        }
        
        request.Dispose();
        Debug.Log("[PostRequestCoroutine] Calling callback...");
        callback(result);
        Debug.Log("[PostRequestCoroutine] Callback called, coroutine ending");
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

    public async Task SendEmailVerification()
    {
        if (string.IsNullOrEmpty(IdToken))
        {
            throw new System.Exception("User not signed in - no ID token available");
        }

        // Firebase Identity Toolkit API for sending verification email
        const string SEND_VERIFICATION_EMAIL_URL = "https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key=";
        
        var requestData = new Dictionary<string, object>
        {
            { "requestType", "VERIFY_EMAIL" },
            { "idToken", IdToken }
        };

        try
        {
            var response = await PostRequest<Dictionary<string, object>>(SEND_VERIFICATION_EMAIL_URL + apiKey, requestData);
            
            if (response != null)
            {
                Debug.Log("[SendEmailVerification] Verification email sent successfully");
                return;
            }
            else
            {
                throw new System.Exception("No response from email verification endpoint");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[SendEmailVerification] Failed: {ex.Message}");
            throw;
        }
    }
}