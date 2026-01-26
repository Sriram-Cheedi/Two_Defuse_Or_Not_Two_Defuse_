using UnityEngine;
using UnityEngine.InputSystem;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class PlayerController : MonoBehaviour
{
    [Header("Character Controller")] [SerializeField]
    private float defaultHeight;
    [SerializeField] private float defaultCenterY;
    
    [Header("Movement")] [SerializeField] private float speed;
    [SerializeField] private float slideSpeed = 12f;
    [SerializeField] private float terminalVelocity = -30f;
    [SerializeField] private float defaultSpeed = 12f;

    [Header("Jump")] 
    [SerializeField] private float jumpHeight = 3f;
    [SerializeField] private float gravity = -20f;

    [Header("crouch")] [SerializeField] private bool isCrouching;
    [SerializeField] private float crouchHeight;
    [SerializeField] private float crouchSpeed;

    [Header("Ground Check")] [SerializeField]
    private Transform groundCheck;

    [SerializeField] private float groundCastRadius = 0.2f;
    [SerializeField] private float groundDistance = 0.2f;
    public LayerMask groundMask;

    Vector3 velocity;
    [SerializeField] private CharacterController controller;
    bool isGrounded;
    private Vector2 moveInput;

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

    private bool OnSlop(bool grounded, RaycastHit hit)
    {
        if (grounded)
        {
            float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
            return slopeAngle > controller.slopeLimit;
        }
        return false;
    }
    // Update is called once per frame
    void Update()
    {
        float radius = controller.radius * 0.9f;
        Vector3 origin = transform.position + controller.center;
        origin.y = transform.position.y + controller.center.y - (controller.height * 0.5f) + controller.radius + 0.02f;
        isGrounded = Physics.SphereCast(origin, radius, Vector3.down, 
            out RaycastHit hit, groundDistance, groundMask, QueryTriggerInteraction.Ignore);
        if (isGrounded && velocity.y < 0.0f)
        {
            velocity.y = -2f;
        }
        
        if (OnSlop(isGrounded, hit))
        {
            //only allow for side to side movement on slop
            Vector3 slopDir = Vector3.ProjectOnPlane(Vector3.down, hit.normal).normalized;
            Vector3 sidewayDir = Vector3.Cross(hit.normal,slopDir).normalized;
            Vector3 sidewayMove = sidewayDir * moveInput.x;
            Vector3 move = sidewayMove * speed + slopDir * slideSpeed;
            controller.Move(move * Time.deltaTime);
        }
        else
        {
            Vector3 moveDir = transform.right * moveInput.x + transform.forward * moveInput.y;
            controller.Move(moveDir * speed * Time.deltaTime);
            if (jumpQueued)
            {
                jumpQueued = false;
                if (isGrounded)
                {
                    velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                }
            }
        }
        velocity.y += gravity * Time.deltaTime;
        velocity.y = Mathf.Max(velocity.y, terminalVelocity);
        controller.Move(velocity * Time.deltaTime);
    }
}
