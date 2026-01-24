using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.Collections.Generic;

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

    public FirebaseRestFirestore(string token)
    {
        idToken = token;
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
        
        Debug.Log($"SetDocument to {collection}/{documentId}: {json.Substring(0, Mathf.Min(200, json.Length))}...");

        try
        {
            using (UnityWebRequest request = new UnityWebRequest(url, "PATCH"))
            {
                request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Authorization", $"Bearer {idToken}");

                var asyncOp = request.SendWebRequest();
                while (!asyncOp.isDone) await Task.Delay(10);

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log($"SetDocument succeeded: {request.downloadHandler.text.Substring(0, Mathf.Min(200, request.downloadHandler.text.Length))}");
                    return true;
                }
                else
                {
                    Debug.LogError($"Set document failed: {request.error}\n{request.downloadHandler.text}");
                    return false;
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Set document error: {ex.Message}");
            return false;
        }
    }

    private string EscapeJson(string str)
    {
        return str.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");
    }

    public async Task<Dictionary<string, object>> GetDocument(string collection, string documentId)
    {
        string url = $"{BASE_URL}/{PROJECT_ID}/databases/{DATABASE_ID}/documents/{collection}/{documentId}";

        try
        {
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.SetRequestHeader("Authorization", $"Bearer {idToken}");

                var asyncOp = request.SendWebRequest();
                while (!asyncOp.isDone) await Task.Delay(10);

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string responseText = request.downloadHandler.text;
                    Debug.Log($"GetDocument response from {collection}/{documentId}:\n{responseText}");
                    
                    // Manually parse the fields from the JSON response
                    return ParseFirestoreResponse(responseText);
                }
                else
                {
                    Debug.LogError($"Get document failed: {request.error}\nURL: {url}\nResponse: {request.downloadHandler.text}");
                    return null;
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Get document error: {ex.Message}");
            return null;
        }
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

        try
        {
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.SetRequestHeader("Authorization", $"Bearer {idToken}");

                var asyncOp = request.SendWebRequest();
                while (!asyncOp.isDone) await Task.Delay(10);

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string responseText = request.downloadHandler.text;
                    var results = new List<Dictionary<string, object>>();
                    
                    Debug.Log($"QueryDocuments response length: {responseText.Length}");
                    if (responseText.Length < 1000)
                        Debug.Log($"QueryDocuments response: {responseText}");
                    else
                        Debug.Log($"QueryDocuments response (first 1000): {responseText.Substring(0, 1000)}...");
                    
                    // Find all document objects - look for each complete document object
                    // Documents are in the "documents" array, each starting with "name" field
                    int docIndex = 0;
                    while (true)
                    {
                        // Find next document start - look for "name": which indicates start of a document
                        int nameIndex = responseText.IndexOf("\"name\"", docIndex);
                        if (nameIndex == -1)
                            break;
                        
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
                        Debug.Log($"Processing document: {docStr.Substring(0, Mathf.Min(200, docStr.Length))}...");
                        
                        // Parse this document using ParseFirestoreResponse
                        var doc = ParseFirestoreResponse(docStr);
                        if (doc.Count > 0)
                        {
                            results.Add(doc);
                            Debug.Log($"  Added document with {doc.Count} fields");
                        }
                        else
                        {
                            Debug.LogWarning($"  Document returned 0 fields");
                        }
                        
                        docIndex = docEnd + 1;
                    }

                    Debug.Log($"Parsed {results.Count} documents from query");
                    return results;
                }
                else
                {
                    Debug.LogError($"Query failed: {request.error}\n{request.downloadHandler.text}");
                    return new List<Dictionary<string, object>>();
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Query error: {ex.Message}\n{ex.StackTrace}");
            return new List<Dictionary<string, object>>();
        }
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