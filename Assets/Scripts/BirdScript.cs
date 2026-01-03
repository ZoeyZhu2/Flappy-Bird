using UnityEngine;

public class BirdScript : MonoBehaviour
{
    [SerializeField] private Rigidbody2D myRigidBody; //Used to be public. creating slot for a RigidBody2D (like the bird's component!)
    private PlayerInputActions inputActions; //created a class for my PlayerInputActions InputActions asset
    private float flapStrength = 10;
    [SerializeField] private bool birdIsAlive = true; //used to be public
    [SerializeField] private LogicScript logic; //used to be public
    private Camera cam;
    private float camTop;
    private float camBottom;

    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private float jumpVolume = 0.5f; //want it to be testable. A bit too loud compared to other sounds atm
    [SerializeField] private float deathVolume = 0.7f; //want it to be testable. A bit too loud compared to other sounds atm

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
            AudioManager.Instance.PlaySoundFX(jumpSound, transform, jumpVolume);
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
        AudioManager.Instance.PlaySoundFX(deathSound, transform, deathVolume);
        logic.GameOver();
        birdIsAlive = false;
    }
}
