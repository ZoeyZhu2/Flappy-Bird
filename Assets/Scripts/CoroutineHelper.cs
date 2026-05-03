using UnityEngine;

/// <summary>
/// Shared utility for running coroutines from non-MonoBehaviour classes.
/// Ensures a single GameObject exists to run coroutines for Firebase operations.
/// </summary>
public class CoroutineHelper : MonoBehaviour
{
    private static CoroutineHelper instance;
    private static GameObject coroutineRunnerObject;

    public static CoroutineHelper Instance
    {
        get
        {
            if (instance == null)
            {
                EnsureCoroutineRunner();
            }
            return instance;
        }
    }

    private static void EnsureCoroutineRunner()
    {
        if (coroutineRunnerObject == null)
        {
            coroutineRunnerObject = new GameObject("CoroutineHelper");
            instance = coroutineRunnerObject.AddComponent<CoroutineHelper>();
            GameObject.DontDestroyOnLoad(coroutineRunnerObject);
        }
    }
}
