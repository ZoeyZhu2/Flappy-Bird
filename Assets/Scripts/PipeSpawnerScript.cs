using UnityEngine;
using System;

public class PipeSpawnerScript : MonoBehaviour
{
    public GameObject pipe;
    public float spawnRate = 2;
    private float timer = 0;
    // public float heightOffset = 5;
    private float gapSize = 7;

    private Camera cam;
    private float camTop;
    private float camBottom;
    private float onePipeHeight;
    private float pipeHalfHeight;
    /*Get the tallest pipe half-height if the pipes are different heights
        float topHalfHeight = topSR.bounds.size.y / 2f;
        float bottomHalfHeight = bottomSR.bounds.size.y / 2f;
        float pipeHalfHeight = Mathf.Max(topHalfHeight, bottomHalfHeight);
    */

    private float maxCenterY;
    private float minCenterY;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private System.Random rng;

    
    void Start()
    {
        if (GameModeManager.Instance == null)
        {
            Debug.LogError("GameModeManager.Instance is NULL in Start()");
            return;
        }

        cam = Camera.main;
        
        updateCameraBounds();

        if (GameModeManager.Instance.currentMode == GameMode.DailySeed)
        {
            rng = new System.Random(GameModeManager.Instance.currentSeed);
        }
        else
        {
            rng = new System.Random();
        }
        spawnPipe();
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log($"Timer BEFORE: {timer}, deltaTime: {Time.deltaTime}, timeScale: {Time.timeScale}");
    
        timer += Time.deltaTime;
    
        Debug.Log($"Timer AFTER: {timer}, spawnRate: {spawnRate}");
        while (timer >= spawnRate)
        {
            spawnPipe();
            timer -= spawnRate;
            Debug.Log("Pipe spawned!");
            Debug.Log("Is this running?");
        }
        Debug.Log(spawnRate);
    }
    void updateCameraBounds()
    {
        camTop = cam.transform.position.y + cam.orthographicSize;
        camBottom = cam.transform.position.y - cam.orthographicSize;

        maxCenterY = camTop - (gapSize / 2f);
        minCenterY = camBottom + (gapSize / 2f);
    }


    void spawnPipe()
    {        
        if (pipe == null)
        {
            Debug.LogError("Pipe prefab is null! Assign it in the inspector.");
            return;
        }

        if (GameModeManager.Instance == null)
        {
            Debug.LogError("GameModeManager.Instance is null!");
            return;
        }

        
        // Instantiate first at 0 Y
        float minGapSize = 6f;
        float maxGapSize = 10f;

        //Seedless
        //gapSize = Random.Range(minGapSize, maxGapSize);

        //Seeded
        gapSize = (float)(minGapSize + rng.NextDouble() * (maxGapSize - minGapSize));
        
        GameObject pipeInstance = Instantiate(pipe, new Vector3(transform.position.x, 0f, 0f), transform.rotation);

        Transform topPipe = pipeInstance.transform.Find("Top Pipe");
        Transform bottomPipe = pipeInstance.transform.Find("Bottom Pipe");
        Transform middle = pipeInstance.transform.Find("Middle");

        if (topPipe == null || bottomPipe == null || middle == null)
        {
        Debug.LogError("Pipe children not found! Check prefab hierarchy and names.");
        return;
        }

        onePipeHeight = topPipe.GetComponent<SpriteRenderer>().bounds.size.y;
        pipeHalfHeight = onePipeHeight/2;

        updateCameraBounds();

        // Choose random Y for gap
        //Seedless
        //float spawnY = Random.Range(minCenterY, maxCenterY);

        //Seeded
        float spawnY = (float)(minCenterY + rng.NextDouble() * (maxCenterY - minCenterY));
    
        Debug.Log($"Gap Size: {gapSize}");
        Debug.Log($"Spawn Y: {spawnY}");
        Debug.Log($"One Pipe Height: {onePipeHeight}");

        // Position top/bottom pipes relative to gap
        topPipe.localPosition = new Vector3(0f, (gapSize / 2f) + (onePipeHeight / 2f), 0f);
        bottomPipe.localPosition = new Vector3(0f, -(gapSize / 2f) - (onePipeHeight / 2f), 0f);

        middle.localPosition = new Vector3(0f,0f,0f);
        BoxCollider2D middleCollider = middle.GetComponent<BoxCollider2D>();
        middleCollider.size = new Vector2(middleCollider.size.x , gapSize);

        pipeInstance.transform.position = new Vector3(transform.position.x, spawnY, 0f);

    }
}
