using UnityEngine;
using UnityEngine.InputSystem;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class PlayerController : MonoBehaviour
{
    [Header("Character Controller")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private float defaultHeight;
    [SerializeField] private float defaultCenterY;

    [Header("Movement")]
    [SerializeField] private float defaultSpeed = 12f;
    [SerializeField] private float speed;
    [SerializeField] private float slideSpeed = 12f;
    [SerializeField] private float terminalVelocity = -30f;

    [Header("Jump")]
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -20f;

    [Header("Crouch")]
    [SerializeField] private bool isCrouching;
    [SerializeField] private float crouchHeight;
    [SerializeField] private float crouchSpeed;

    private Vector2 moveInput;
    private Vector3 horizontalMove;
    private Vector3 surfaceNormal;
    private Vector3 velocity;
    
    private bool jumpQueued;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        defaultHeight = controller.height;
        defaultCenterY = controller.center.y;
        crouchHeight = controller.height / 2f;
        crouchSpeed = defaultSpeed / 2f;
        speed = defaultSpeed;
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            jumpQueued = true;
        }
    }

    private void SetCrouch(bool isCrouch)
    {
        float bottomLocal = defaultCenterY - (defaultHeight * 0.5f);
        float newCenterY;
        if (isCrouch)
        {
            newCenterY = bottomLocal + crouchHeight / 2f;
            controller.height = crouchHeight;
            controller.center = new Vector3(controller.center.x, newCenterY, controller.center.z);
            speed = crouchSpeed;
        }
        else
        {
            newCenterY = bottomLocal + defaultHeight / 2f;
            controller.height = defaultHeight;
            controller.center = new Vector3(controller.center.x, newCenterY, controller.center.z);
            speed = defaultSpeed;
        }
    }

    public void OnCrouch(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            isCrouching = true;
        }
        else if (ctx.canceled)
        {
            isCrouching = false;
        }
        SetCrouch(isCrouching);
    }

    private bool OnSteepSlope(bool grounded, Vector3 normal)
    {
        if (grounded)
        {
            float slopeAngle = Vector3.Angle(normal, Vector3.up);
            return slopeAngle > controller.slopeLimit;
        }
        return false;
    }
    // Update is called once per frame
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // print the impact point's normal
        surfaceNormal = hit.normal;
    }
    
    void Update()
    {
        if (OnSteepSlope(controller.isGrounded, surfaceNormal))
        {
            //only allow for side to side movement on slop
            Vector3 slopDir = Vector3.ProjectOnPlane(Vector3.down, surfaceNormal).normalized;
            Vector3 sidewayDir = Vector3.Cross(surfaceNormal,slopDir).normalized;
            Vector3 sidewayMove = sidewayDir * moveInput.x;
            horizontalMove = sidewayMove * speed + slopDir * slideSpeed;
        }
        else
        {
            //allow for full move on ground
            Vector3 moveDir = transform.right * moveInput.x + transform.forward * moveInput.y;
            horizontalMove = moveDir * speed;
            if (jumpQueued)
            {
                jumpQueued = false;
                if (controller.isGrounded)
                {
                    velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                }
            }
        }
        
        if (controller.isGrounded && velocity.y < 0.0f)
        {
            velocity.y = -2f;
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }
        velocity.y = Mathf.Max(velocity.y, terminalVelocity);
        Vector3 move = horizontalMove + velocity;
        
        controller.Move(move * Time.deltaTime);
    }
}
