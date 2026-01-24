using UnityEngine;
using UnityEngine.InputSystem;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField]private float speed = 12f;
    [SerializeField]private float slideSpeed = 12f;
    [SerializeField] private float terminalVelocity = -30f;
    
    [Header("Jump")]
    [SerializeField]private float jumpHeight = 3f;
    [SerializeField]private float gravity = -20f;
    
    [Header("Ground Check")]
    [SerializeField]private Transform groundCheck;
    [SerializeField]private float groundCastRadius = 0.2f;
    [SerializeField]private float groundDistance = 0.2f;
    public LayerMask groundMask;
    
    Vector3 velocity;
    [SerializeField]private CharacterController controller;
    bool isGrounded;
    private Vector2 moveInput;
    private bool jumpQueued;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            jumpQueued = true;
        }
    }
    private bool OnSlop(bool grounded, RaycastHit hit)
    {
        if (grounded)
        {
            float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
            Debug.Log($"Hit: {hit.collider.name}, angle: {slopeAngle}, limit: {controller.slopeLimit}");
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
        Debug.Log($"flags={controller.collisionFlags}");

    }
}
