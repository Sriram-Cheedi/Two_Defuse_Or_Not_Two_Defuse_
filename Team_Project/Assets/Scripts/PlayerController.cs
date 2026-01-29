using UnityEngine;
using UnityEngine.InputSystem;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class PlayerController : MonoBehaviour
{
    [Header("Character Controller")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private float standHeight;
    [SerializeField] private float standCenterY;

    [Header("Movement")]
    [SerializeField] private float defaultSpeed = 12f;
    [SerializeField] private float speed;
    [SerializeField] private float slideSpeed = 12f;
    [SerializeField] private float terminalVelocity = -30f;

    [Header("Jump")]
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -20f;

    [Header("Crouch")]
    [SerializeField] private bool crouchPressed;
    [SerializeField] private float crouchHeight;
    [SerializeField] private float crouchSpeed;
    [SerializeField] private float downSpeed;

    private Vector2 moveInput;
    private Vector3 horizontalMove;
    private Vector3 surfaceNormal;
    private float verticalVelocity;
    private bool onSlope;
    private bool jumpQueued;
    private bool obstacleAbove;
    private Vector3 standOrigin;
    private Vector3 crouchOrigin;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        standHeight = controller.height;
        standCenterY = controller.center.y;
        crouchHeight = controller.height / 2f;
        crouchSpeed = defaultSpeed / 2f;
        speed = defaultSpeed;
        downSpeed = 5.0f;
        SetStandCenter();
        SetCrouchCenter();
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && !IsObstacleAbove())
        {
            jumpQueued = true;
        }
    }

    private void SetCrouchCenter()
    {
        float bottomLocal = standCenterY - (standHeight * 0.5f);
        float newCenterY = bottomLocal + crouchHeight / 2f;
        crouchOrigin = new Vector3(controller.center.x, newCenterY, controller.center.z);
    }

    private void SetStandCenter()
    {
        float bottomLocal = standCenterY - (standHeight * 0.5f);
        float newCenterY = bottomLocal + standHeight / 2f;
        standOrigin = new Vector3(controller.center.x, newCenterY, controller.center.z);
    }

    private void SetCrouch(bool isCrouch)
    {
        obstacleAbove = IsObstacleAbove();
        if (isCrouch)
        {
            if (controller.isGrounded)
            {
                speed = crouchSpeed;
            }
            controller.height = Mathf.Lerp(controller.height, crouchHeight, downSpeed * Time.deltaTime);
            controller.center = Vector3.Lerp(controller.center, crouchOrigin, downSpeed * Time.deltaTime);
        }
        else
        {
            if (obstacleAbove)
            {
                return;
            }
            speed = defaultSpeed;   
            controller.height = Mathf.Lerp(controller.height, standHeight, downSpeed * Time.deltaTime);
            controller.center = Vector3.Lerp(controller.center, standOrigin, downSpeed * Time.deltaTime);
        }
    }

    public void OnCrouch(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            crouchPressed = true;
        }
        else if (ctx.canceled)
        {
            crouchPressed = false;
        }
    }
    
    private bool IsObstacleAbove()
    {
        Vector3 origin = transform.position + crouchOrigin;
        float radius = controller.radius * 0.95f;
        float maxDist = standHeight - crouchHeight;
        return Physics.SphereCast(origin, radius, Vector3.up, out _, maxDist, ~0, QueryTriggerInteraction.Ignore);
    }
    
    // Update is called once per frame
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.moveDirection.y < -0.3f)
        {
            surfaceNormal = hit.normal;
            onSlope = hit.collider.CompareTag("Slide");
        }
    }

    private Vector3 ComputeHorizontalMoveDir()
    {
        Vector3 moveDir = transform.right * moveInput.x + transform.forward * moveInput.y;
        horizontalMove = moveDir.normalized * speed;
        return horizontalMove;
    }
    
    private Vector3 ComputeHorizontalMoveDirOnSlope()
    {
        Vector3 slopDir = Vector3.ProjectOnPlane(Vector3.down, surfaceNormal).normalized;
        Vector3 sidewayDir = Vector3.Cross(surfaceNormal,slopDir).normalized;
        Vector3 sidewayMove = sidewayDir * moveInput.x;
        horizontalMove = sidewayMove * speed + slopDir * slideSpeed;
        return horizontalMove;
    }

    private void ComputeVerticalVelocity()
    {
        if (jumpQueued)
        {
            jumpQueued = false;
            if (controller.isGrounded)
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
        }
    }
    
    void Update()
    {
        if (controller.isGrounded && onSlope)
        {
            //only allow for side to side movement on slop
            horizontalMove = ComputeHorizontalMoveDirOnSlope();
        }
        else
        {
            //allow for full move
            horizontalMove = ComputeHorizontalMoveDir();
            ComputeVerticalVelocity();
        }   
        
        if (controller.isGrounded && verticalVelocity < 0.0f)
        {
            verticalVelocity = -2f;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }
        verticalVelocity = Mathf.Max(verticalVelocity, terminalVelocity);
        Vector3 move = horizontalMove + Vector3.up * verticalVelocity;
        controller.Move(move * Time.deltaTime);
        SetCrouch(crouchPressed);
    }
}
