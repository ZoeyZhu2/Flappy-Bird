using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static PlayerInputActions inputActions; //shared instance
    
    void Awake()
    {
        if (inputActions == null)
        {
            inputActions = new PlayerInputActions();
            inputActions.Player.Enable(); // enable default Player map
            inputActions.UI.Enable(); // enable default UI map
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject); //destroy duplicate InputManager
        }
    }
}
