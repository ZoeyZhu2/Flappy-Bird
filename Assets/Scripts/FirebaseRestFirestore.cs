using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections;

[System.Serializable]
public class FirestoreDocument
{
    public Dictionary<string, FirestoreValue> fields;
}

[System.Serializable]
public class FirestoreValue
{
    public string stringValue;
    public string integerValue;  // Changed from int to string
    public bool booleanValue;
    public string doubleValue;  // Changed from double to string
}

[System.Serializable]
public class FirestoreListResponse
{
    public FirestoreDocument[] documents;
}

public class FirebaseRestFirestore
{
    private const string PROJECT_ID = "flappy-bird-ce77c";
    private const string DATABASE_ID = "(default)";
    private const string BASE_URL = "https://firestore.googleapis.com/v1/projects";
    private string idToken;
    private static GameObject coroutineRunner;

    public FirebaseRestFirestore(string token)
    {
        idToken = token;
        EnsureCoroutineRunner();
    }
    
    private static void EnsureCoroutineRunner()
    {
        if (coroutineRunner == null)
        {
            coroutineRunner = new GameObject("FirebaseRestFirestoreCoroutineRunner");
            coroutineRunner.AddComponent<CoroutineRunner>();
            GameObject.DontDestroyOnLoad(coroutineRunner);
        }
    }
    
    private class CoroutineRunner : MonoBehaviour
    {
        // Just exists to run coroutines
    }

    public async Task<bool> SetDocument(string collection, string documentId, Dictionary<string, object> data)
    {
        string url = $"{BASE_URL}/{PROJECT_ID}/databases/{DATABASE_ID}/documents/{collection}/{documentId}";
        
        // Manually build JSON since JsonUtility doesn't handle nested structures well
        string json = "{\"fields\":{";
        var keys = new List<string>(data.Keys);
        for (int i = 0; i < keys.Count; i++)
        {
            var key = keys[i];
            var value = data[key];
            
            json += $"\"{key}\":";
            
            if (value is string str)
                json += $"{{\"stringValue\":\"{EscapeJson(str)}\"}}";
            else if (value is int || value is long)
                json += $"{{\"integerValue\":\"{value}\"}}";
            else if (value is float || value is double)
                json += $"{{\"doubleValue\":\"{value}\"}}";
            else if (value is bool b)
                json += $"{{\"booleanValue\":{(b ? "true" : "false")}}}";
            else
                json += $"{{\"stringValue\":\"{EscapeJson(value.ToString())}\"}}";
            
            if (i < keys.Count - 1)
                json += ",";
        }
        json += "}}";
        
        Debug.Log($"[SetDocument] Writing to {collection}/{documentId}");

        var tcs = new TaskCompletionSource<bool>();
        var runner = coroutineRunner.GetComponent<CoroutineRunner>();
        
        runner.StartCoroutine(SetDocumentCoroutine(url, json, tcs));
        
        try
        {
            return await tcs.Task;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[SetDocument] Exception: {ex.Message}");
            return false;
        }
    }
    
    private IEnumerator SetDocumentCoroutine(string url, string json, TaskCompletionSource<bool> tcs)
    {
        int maxRetries = 3;
        int retryCount = 0;
        
        while (retryCount < maxRetries)
        {
            using (UnityWebRequest request = new UnityWebRequest(url, "PATCH"))
            {
                request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Authorization", $"Bearer {idToken}");
                request.timeout = 30; // 30 second timeout

                Debug.Log($"[SetDocumentCoroutine] Sending PATCH request to {url} (attempt {retryCount + 1}/{maxRetries})");
                yield return request.SendWebRequest();
                
                Debug.Log($"[SetDocumentCoroutine] Request completed. Result: {request.result}");

                bool success = false;
                if (request.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log($"[SetDocumentCoroutine] Success!");
                    success = true;
                }
                else
                {
                    // Check if this is a retryable error
                    if (request.responseCode >= 500 || 
                        request.error.Contains("Broken pipe") || 
                        request.error.Contains("timeout") ||
                        request.error.Contains("Cannot connect"))
                    {
                        retryCount++;
                        Debug.LogWarning($"[SetDocumentCoroutine] Retryable error: {request.error}. Retrying in 1 second...");
                        yield return new WaitForSeconds(1f);
                        continue;
                    }
                    
                    Debug.LogError($"[SetDocumentCoroutine] Failed: {request.error}\nResponse: {request.downloadHandler.text}");
                    success = false;
                }
                
                tcs.SetResult(success);
                yield break;
            }
        }
        
        Debug.LogError("[SetDocumentCoroutine] Max retries exceeded");
        tcs.SetResult(false);
    }

    private string EscapeJson(string str)
    {
        return str.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");
    }

    public async Task<Dictionary<string, object>> GetDocument(string collection, string documentId)
    {
        Debug.Log($"[GetDocument] Getting document: {collection}/{documentId}");
        
        string url = $"{BASE_URL}/{PROJECT_ID}/databases/{DATABASE_ID}/documents/{collection}/{documentId}";
        
        Debug.Log($"[GetDocument] Requesting: {url}");
        
        var tcs = new TaskCompletionSource<Dictionary<string, object>>();
        var runner = coroutineRunner.GetComponent<CoroutineRunner>();
        
        runner.StartCoroutine(GetDocumentCoroutine(url, tcs));
        
        try
        {
            var result = await tcs.Task;
            Debug.Log($"[GetDocument] Result: {(result == null ? "null" : "object with " + result.Count + " fields")}");
            return result;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[GetDocument] Exception: {ex.Message}");
            return null;
        }
    }
    
    private IEnumerator GetDocumentCoroutine(string url, TaskCompletionSource<Dictionary<string, object>> tcs)
    {
        int maxRetries = 3;
        int retryCount = 0;
        
        while (retryCount < maxRetries)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.SetRequestHeader("Authorization", $"Bearer {idToken}");
                request.timeout = 30; // 30 second timeout
                
                Debug.Log($"[GetDocumentCoroutine] Sending request (attempt {retryCount + 1}/{maxRetries})...");
                yield return request.SendWebRequest();
                
                Debug.Log($"[GetDocumentCoroutine] Request completed. Result: {request.result}");
                
                Dictionary<string, object> result = null;
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    string responseText = request.downloadHandler.text;
                    Debug.Log($"[GetDocumentCoroutine] Success response");
                    
                    // Manually parse the fields from the JSON response
                    result = ParseFirestoreResponse(responseText);
                }
                else
                {
                    // Check if this is a retryable error
                    if (request.responseCode >= 500 || 
                        request.error.Contains("Broken pipe") || 
                        request.error.Contains("timeout") ||
                        request.error.Contains("Cannot connect"))
                    {
                        retryCount++;
                        Debug.LogWarning($"[GetDocumentCoroutine] Retryable error: {request.error}. Retrying in 1 second...");
                        yield return new WaitForSeconds(1f);
                        continue;
                    }
                    
                    Debug.LogWarning($"[GetDocumentCoroutine] Request failed. Error: {request.error}, Code: {request.responseCode}");
                    // Return null on 404 or other errors - caller will handle creating new document
                    result = null;
                }
                
                tcs.SetResult(result);
                yield break;
            }
        }
        
        Debug.LogError("[GetDocumentCoroutine] Max retries exceeded");
        tcs.SetResult(null);
    }

    private Dictionary<string, object> ParseFirestoreResponse(string json)
    {
        var result = new Dictionary<string, object>();
        
        // Find the "fields" section
        int fieldsIndex = json.IndexOf("\"fields\"");
        if (fieldsIndex == -1)
        {
            Debug.LogWarning("No fields found in Firestore response");
            return result;
        }
        
        // Find opening brace after "fields"
        int fieldsStart = json.IndexOf("{", fieldsIndex);
        int fieldsEnd = FindMatchingBrace(json, fieldsStart);
        
        if (fieldsStart == -1 || fieldsEnd == -1)
            return result;
        
        string fieldsContent = json.Substring(fieldsStart + 1, fieldsEnd - fieldsStart - 1);
        
        // Parse each field
        int pos = 0;
        while (pos < fieldsContent.Length)
        {
            // Find field name
            int nameStart = fieldsContent.IndexOf("\"", pos);
            if (nameStart == -1) break;
            
            int nameEnd = fieldsContent.IndexOf("\"", nameStart + 1);
            if (nameEnd == -1) break;
            
            string fieldName = fieldsContent.Substring(nameStart + 1, nameEnd - nameStart - 1);
            
            // Skip to the value object
            int valueStart = fieldsContent.IndexOf("{", nameEnd);
            if (valueStart == -1) break;
            
            int valueEnd = FindMatchingBrace(fieldsContent, valueStart);
            if (valueEnd == -1) break;
            
            string valueContent = fieldsContent.Substring(valueStart + 1, valueEnd - valueStart - 1);
            object fieldValue = ParseFirestoreValue(valueContent);
            
            if (fieldValue != null)
                result[fieldName] = fieldValue;
            
            pos = valueEnd + 1;
        }
        
        Debug.Log($"Parsed Firestore document with {result.Count} fields");
        foreach (var kvp in result)
        {
            Debug.Log($"  {kvp.Key}: {kvp.Value}");
        }
        
        return result;
    }

    private object ParseFirestoreValue(string valueStr)
    {
        // Check for stringValue
        if (valueStr.Contains("\"stringValue\""))
        {
            int start = valueStr.IndexOf("\"stringValue\"");
            int colonPos = valueStr.IndexOf(":", start);
            int quoteStart = valueStr.IndexOf("\"", colonPos);
            int quoteEnd = valueStr.IndexOf("\"", quoteStart + 1);
            
            if (quoteStart != -1 && quoteEnd != -1)
                return valueStr.Substring(quoteStart + 1, quoteEnd - quoteStart - 1);
        }
        
        // Check for integerValue
        if (valueStr.Contains("\"integerValue\""))
        {
            int start = valueStr.IndexOf("\"integerValue\"");
            int colonPos = valueStr.IndexOf(":", start);
            int quoteStart = valueStr.IndexOf("\"", colonPos);
            int quoteEnd = valueStr.IndexOf("\"", quoteStart + 1);
            
            if (quoteStart != -1 && quoteEnd != -1)
            {
                string intStr = valueStr.Substring(quoteStart + 1, quoteEnd - quoteStart - 1);
                if (int.TryParse(intStr, out int intVal))
                    return intVal;
            }
        }
        
        // Check for doubleValue
        if (valueStr.Contains("\"doubleValue\""))
        {
            int start = valueStr.IndexOf("\"doubleValue\"");
            int colonPos = valueStr.IndexOf(":", start);
            int quoteStart = valueStr.IndexOf("\"", colonPos);
            int quoteEnd = valueStr.IndexOf("\"", quoteStart + 1);
            
            if (quoteStart != -1 && quoteEnd != -1)
            {
                string doubleStr = valueStr.Substring(quoteStart + 1, quoteEnd - quoteStart - 1);
                if (double.TryParse(doubleStr, out double doubleVal))
                    return doubleVal;
            }
        }
        
        // Check for booleanValue
        if (valueStr.Contains("\"booleanValue\""))
        {
            return valueStr.Contains("true");
        }
        
        return null;
    }

    private int FindMatchingBrace(string str, int openBraceIndex)
    {
        if (openBraceIndex >= str.Length || str[openBraceIndex] != '{')
            return -1;
        
        int depth = 1;
        bool inString = false;
        for (int i = openBraceIndex + 1; i < str.Length; i++)
        {
            if (str[i] == '"' && (i == 0 || str[i - 1] != '\\'))
                inString = !inString;
            
            if (!inString)
            {
                if (str[i] == '{')
                    depth++;
                else if (str[i] == '}')
                {
                    depth--;
                    if (depth == 0)
                        return i;
                }
            }
        }
        return -1;
    }

    public async Task<List<Dictionary<string, object>>> QueryDocuments(string collection, string orderByField = null, bool descending = false, int limit = 100)
    {
        string url = $"{BASE_URL}/{PROJECT_ID}/databases/{DATABASE_ID}/documents/{collection}";

        var tcs = new TaskCompletionSource<List<Dictionary<string, object>>>();
        var runner = coroutineRunner.GetComponent<CoroutineRunner>();
        
        runner.StartCoroutine(QueryDocumentsCoroutine(url, tcs));
        
        try
        {
            return await tcs.Task;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[QueryDocuments] Exception: {ex.Message}");
            return new List<Dictionary<string, object>>();
        }
    }
    
    private IEnumerator QueryDocumentsCoroutine(string url, TaskCompletionSource<List<Dictionary<string, object>>> tcs)
    {
        int maxRetries = 3;
        int retryCount = 0;
        
        while (retryCount < maxRetries)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.SetRequestHeader("Authorization", $"Bearer {idToken}");
                request.timeout = 30; // 30 second timeout

                Debug.Log($"[QueryDocumentsCoroutine] Sending request (attempt {retryCount + 1}/{maxRetries})...");
                yield return request.SendWebRequest();

                var results = new List<Dictionary<string, object>>();
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    string responseText = request.downloadHandler.text;
                    
                    Debug.Log($"[QueryDocumentsCoroutine] Response length: {responseText.Length}");
                    Debug.Log($"[QueryDocumentsCoroutine] First 500 chars: {responseText.Substring(0, Mathf.Min(500, responseText.Length))}");
                    
                    // Find all document objects - look for each complete document object
                    // Documents are in the "documents" array, each starting with "name" field
                    int docIndex = 0;
                    int docCount = 0;
                    while (true)
                    {
                        // Find next document start - look for "name": which indicates start of a document
                        int nameIndex = responseText.IndexOf("\"name\"", docIndex);
                        if (nameIndex == -1)
                            break;
                        
                        docCount++;
                        Debug.Log($"[QueryDocumentsCoroutine] Found document {docCount} at nameIndex: {nameIndex}");
                        
                        // Backtrack to find the opening brace of this document object
                        int docStart = nameIndex - 1;
                        while (docStart >= 0 && responseText[docStart] != '{')
                            docStart--;
                        
                        if (docStart < 0)
                        {
                            docIndex = nameIndex + 1;
                            continue;
                        }
                        
                        // Find the matching closing brace for this document
                        int docEnd = FindMatchingBrace(responseText, docStart);
                        if (docEnd == -1)
                        {
                            docIndex = nameIndex + 1;
                            continue;
                        }
                        
                        // Extract this document
                        string docStr = responseText.Substring(docStart, docEnd - docStart + 1);
                        
                        // Parse this document using ParseFirestoreResponse
                        var doc = ParseFirestoreResponse(docStr);
                        if (doc.Count > 0)
                        {
                            results.Add(doc);
                            Debug.Log($"[QueryDocumentsCoroutine] Added document {results.Count} with {doc.Count} fields");
                            foreach (var kvp in doc)
                            {
                                Debug.Log($"  {kvp.Key} = {kvp.Value}");
                            }
                        }
                        else
                        {
                            Debug.Log($"[QueryDocumentsCoroutine] Document {docCount} parsed but had 0 fields");
                        }
                        
                        docIndex = docEnd + 1;
                    }

                    Debug.Log($"[QueryDocumentsCoroutine] Total documents found: {docCount}, parsed: {results.Count}");
                }
                else
                {
                    // Check if this is a retryable error
                    if (request.responseCode >= 500 || 
                        request.error.Contains("Broken pipe") || 
                        request.error.Contains("timeout") ||
                        request.error.Contains("Cannot connect"))
                    {
                        retryCount++;
                        Debug.LogWarning($"[QueryDocumentsCoroutine] Retryable error: {request.error}. Retrying in 1 second...");
                        yield return new WaitForSeconds(1f);
                        continue;
                    }
                    
                    Debug.LogError($"[QueryDocumentsCoroutine] Query failed: {request.error}");
                }
                
                tcs.SetResult(results);
                yield break;
            }
        }
        
        Debug.LogError("[QueryDocumentsCoroutine] Max retries exceeded");
        tcs.SetResult(new List<Dictionary<string, object>>());
    }
    
    private Dictionary<string, object> ParseSingleDocument(string fieldsContent)
    {
        var result = new Dictionary<string, object>();
        
        // fieldsContent is the raw content between the outer { } of a document
        // Find the "fields" section within this document
        int fieldsIndex = fieldsContent.IndexOf("\"fields\"");
        if (fieldsIndex == -1)
            return result;
        
        // Find opening brace after "fields"
        int fieldsStart = fieldsContent.IndexOf("{", fieldsIndex);
        int fieldsEnd = FindMatchingBrace(fieldsContent, fieldsStart);
        
        if (fieldsStart == -1 || fieldsEnd == -1)
            return result;
        
        string fields = fieldsContent.Substring(fieldsStart + 1, fieldsEnd - fieldsStart - 1);
        
        // Parse each field
        int pos = 0;
        while (pos < fields.Length)
        {
            // Find field name
            int nameStart = fields.IndexOf("\"", pos);
            if (nameStart == -1) break;
            
            int nameEnd = fields.IndexOf("\"", nameStart + 1);
            if (nameEnd == -1) break;
            
            string fieldName = fields.Substring(nameStart + 1, nameEnd - nameStart - 1);
            
            // Skip to the value object
            int valueStart = fields.IndexOf("{", nameEnd);
            if (valueStart == -1) break;
            
            int valueEnd = FindMatchingBrace(fields, valueStart);
            if (valueEnd == -1) break;
            
            string valueContent = fields.Substring(valueStart + 1, valueEnd - valueStart - 1);
            object fieldValue = ParseFirestoreValue(valueContent);
            
            if (fieldValue != null)
                result[fieldName] = fieldValue;
            
            pos = valueEnd + 1;
        }
        
        return result;
    }

    private FirestoreDocument ConvertToFirestoreDocument(Dictionary<string, object> data)
    {
        var doc = new FirestoreDocument { fields = new Dictionary<string, FirestoreValue>() };

        foreach (var kvp in data)
        {
            var value = new FirestoreValue();

            if (kvp.Value is string str)
                value.stringValue = str;
            else if (kvp.Value is int i)
                value.integerValue = i.ToString();
            else if (kvp.Value is bool b)
                value.booleanValue = b;
            else if (kvp.Value is double d)
                value.doubleValue = d.ToString();
            else if (kvp.Value is long l)
                value.integerValue = l.ToString();
            else if (kvp.Value is float f)
                value.doubleValue = f.ToString();

            doc.fields[kvp.Key] = value;
        }

        return doc;
    }

    private Dictionary<string, object> ConvertFromFirestoreDocument(FirestoreDocument doc)
    {
        var result = new Dictionary<string, object>();

        if (doc.fields != null)
        {
            foreach (var kvp in doc.fields)
            {
                object value = null;
                
                if (!string.IsNullOrEmpty(kvp.Value.stringValue))
                    value = kvp.Value.stringValue;
                else if (!string.IsNullOrEmpty(kvp.Value.integerValue))
                {
                    if (long.TryParse(kvp.Value.integerValue, out long longVal))
                        value = (int)longVal;
                }
                else if (!string.IsNullOrEmpty(kvp.Value.doubleValue))
                {
                    if (double.TryParse(kvp.Value.doubleValue, out double doubleVal))
                        value = doubleVal;
                }
                else
                    value = kvp.Value.booleanValue;

                if (value != null)
                    result[kvp.Key] = value;
            }
        }

        Debug.Log($"Converted document with {result.Count} fields: {string.Join(", ", result.Keys)}");
        foreach (var kvp in result)
        {
            Debug.Log($"  {kvp.Key}: {kvp.Value}");
        }

        return result;
    }
}