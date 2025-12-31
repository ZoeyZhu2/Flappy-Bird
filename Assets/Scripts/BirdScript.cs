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

    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip deathSound;
    public float volume = 1f; //want it to be editable in settings so making it public
    private void Awake()
    {
        inputActions = InputManager.inputActions;
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
        UpdateCameraBounds();
    }

    // Update is called once per frame
    void Update()
    {
        if (inputActions.Player.Jump.WasPressedThisFrame() && birdIsAlive)
        {
            myRigidBody.linearVelocity = Vector2.up * flapStrength;
            SoundFXManager.Instance.PlaySoundFX(jumpSound, transform, volume);
        }

        if (birdIsAlive && (transform.position.y > camTop || transform.position.y < camBottom))
        {
            Die();
        }
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //all obstacles are layer 6
        if (collision.gameObject.layer == 6)
        {
            Die();
        }
    }

    private void UpdateCameraBounds()
    {
        camTop = cam.transform.position.y + cam.orthographicSize;
        camBottom = cam.transform.position.y - cam.orthographicSize;
    }

    private void Die()
    {
        SoundFXManager.Instance.PlaySoundFX(deathSound, transform, volume);
        logic.GameOver();
        birdIsAlive = false;
    }
}
