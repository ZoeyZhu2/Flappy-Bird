using UnityEngine;

public class BirdScript : MonoBehaviour
{
    public Rigidbody2D myRigidBody; //creating slot for a RigidBody2D (like the bird's component!)
    private PlayerInputActions inputActions; //created a class for my PlayerInputActions InputActions asset
    private float flapStrength = 7;
    public bool birdIsAlive = true;
    public LogicScript logic;
    private Camera cam;
    private float camTop;
    private float camBottom;
    private void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        inputActions.Player.Jump.Enable();
    }
    private void OnDisable()
    {
        inputActions.Player.Jump.Disable();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        logic = GameObject.FindGameObjectWithTag("Logic").GetComponent<LogicScript>();
        cam = Camera.main;
        updateCameraBounds();
    }

    // Update is called once per frame
    void Update()
    {
        if (inputActions.Player.Jump.WasPressedThisFrame() && birdIsAlive)
        {
            myRigidBody.linearVelocity = Vector2.up * flapStrength;
        }

        if (birdIsAlive && (transform.position.y > camTop || transform.position.y < camBottom))
        {
            logic.gameOver();
            birdIsAlive = false;
        }
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //all obstacles are layer 6
        if (collision.gameObject.layer == 6)
        {
            logic.gameOver();
            birdIsAlive = false;
        }
    }

    private void updateCameraBounds()
    {
        camTop = cam.transform.position.y + cam.orthographicSize;
        camBottom = cam.transform.position.y - cam.orthographicSize;
    }

    //if bird is below or above screen, also die
}
