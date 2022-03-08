using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private PlayerInput playerInput;
    private PlayerInput.OnGroundActions onGround;

    private PlayerMovement movement;
    private PlayerLook look;

    // Start is called before the first frame update
    void Awake()
    {
        playerInput = new PlayerInput();
        onGround = playerInput.OnGround;

        movement = GetComponent<PlayerMovement>();
        look = GetComponent<PlayerLook>();

        onGround.Jump.performed += ctx => movement.Jump();
        onGround.Crouch.performed += ctx => movement.Crouch();
        onGround.Sprint.performed += ctx => movement.Sprint();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //tell the playermotor to move using the value from our movement action.
        movement.ProcessMove(onGround.Movement.ReadValue<Vector2>());
        
    }

    private void LateUpdate()
    {
        //tell the playermotor to look using the value from our look action.
        look.ProcessLook(onGround.Look.ReadValue<Vector2>());
    }

    private void OnEnable()
    {
        onGround.Enable();
    }

    private void OnDisable()
    {
        onGround.Disable();
    }

}