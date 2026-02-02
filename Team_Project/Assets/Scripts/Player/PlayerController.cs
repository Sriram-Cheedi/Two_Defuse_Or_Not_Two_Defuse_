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
    [SerializeField] private float terminalDownVelocity = -30f;
    [SerializeField] private float terminalUpVelocity = -30f;

    [Header("Jump")]
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float springHeight = 6f;
    [SerializeField] private float defaultGravity = -20f;
    [SerializeField] private float gravity = -20f;

    [Header("Crouch")]
    [SerializeField] private bool crouchPressed;
    [SerializeField] private float crouchHeight;
    [SerializeField] private float crouchSpeed;
    [SerializeField] private float downSpeed;

    [Header("External")]
    private API_Provider _API_Provider = API_Provider.Instance;
    private API_Controller_Interface _activeHardware;

    private Vector2 moveInput;
    private Vector3 horizontalMove;
    private Vector3 surfaceNormal;
    private float verticalVelocity;
    private bool onSlope;
    private bool onSpring;
    private bool invertGravity;
    private bool jumpQueued;
    private bool obstacleAbove;
    private Vector3 standOrigin;
    private Vector3 crouchOrigin;
    private bool grounded;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        standHeight = controller.height;
        standCenterY = controller.center.y;

        crouchHeight = controller.height / 2f;
        crouchSpeed = defaultSpeed / 2f;

        speed = defaultSpeed;
        downSpeed = 5.0f;
        invertGravity = false;

        SetStandCenter();
        SetCrouchCenter();

        _activeHardware = _API_Provider.ActiveAPIController;
    }

    private void OnEnable()
    {
        if (_activeHardware != null)
            _activeHardware.OnImuChanged += RotateLevel;
    }

    private void OnDisable()
    {
        if (_activeHardware != null)
            _activeHardware.OnImuChanged -= RotateLevel;
    }

    private void RotateLevel(Vector3 gyro, Vector3 rotationValue)
    {

        // Flip gravity direction (so jump/gravity math stays consistent with sign)
        // gravity = -gravity;
        // Rotate the player (or level) around Z, as you had
        Physics.gravity = gyro;
        Debug.Log(gyro.x + " " + gyro.y + " " + gyro.z);
        transform.Rotate(gyro.x, gyro.y, gyro.z);
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && !IsObstacleAbove())
            jumpQueued = true;
    }

    public void OnCrouch(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) crouchPressed = true;
        else if (ctx.canceled) crouchPressed = false;
    }

    public void OnInvertGravity(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            invertGravity = !invertGravity;
            transform.Rotate(0, 0, 180);

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
            if (grounded)
                speed = crouchSpeed;

            controller.height = Mathf.Lerp(controller.height, crouchHeight, downSpeed * Time.deltaTime);
            controller.center = Vector3.Lerp(controller.center, crouchOrigin, downSpeed * Time.deltaTime);
        }
        else
        {
            if (obstacleAbove)
                return;

            speed = defaultSpeed;

            controller.height = Mathf.Lerp(controller.height, standHeight, downSpeed * Time.deltaTime);
            controller.center = Vector3.Lerp(controller.center, standOrigin, downSpeed * Time.deltaTime);
        }
    }

    private bool IsObstacleAbove()
    {
        Vector3 origin = transform.position + crouchOrigin;
        float radius = controller.radius * 0.95f;
        float maxDist = standHeight - crouchHeight;
        Vector3 rayDir = !invertGravity ? Vector3.up : Vector3.down;

        return Physics.SphereCast(
            origin,
            radius,
            rayDir,
            out _,
            maxDist,
            ~0,
            QueryTriggerInteraction.Ignore
        );
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.moveDirection.y < -0.3f)
        {
            surfaceNormal = hit.normal;
            onSlope = hit.collider.CompareTag("Slide");
            onSpring = hit.collider.CompareTag("Spring");
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
        Vector3 sidewayDir = Vector3.Cross(surfaceNormal, slopDir).normalized;
        Vector3 sidewayMove = sidewayDir * moveInput.x;

        horizontalMove = sidewayMove * speed + slopDir * slideSpeed;
        return horizontalMove;
    }

    private void ComputeVerticalVelocity()
    {
        if (!jumpQueued) return;

        jumpQueued = false;

        if (grounded && !invertGravity)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * defaultGravity);
        }
        else if (grounded && invertGravity)
        {
            verticalVelocity = -Mathf.Sqrt(jumpHeight * -2f * defaultGravity);
        }

    }

    private void Spring()
    {
        if (onSpring && grounded)
            verticalVelocity = Mathf.Sqrt(springHeight * -2f * defaultGravity);
    }

    private void GravityInvert(bool isGravityInvert, CollisionFlags flags)
    {
        if (isGravityInvert)
        {
            gravity = -defaultGravity;
            grounded = (flags & CollisionFlags.Above) != 0;
            if (verticalVelocity > 0.0f && grounded) //if going down in world dir
            {
                verticalVelocity = 2f;
            }
        }
        else
        {
            gravity = defaultGravity;
            grounded = (flags & CollisionFlags.Below) != 0;
            if (verticalVelocity < 0.0f && grounded) //if going up in world dir
            {
                verticalVelocity = -2f;
            }
        }
        verticalVelocity += gravity * Time.deltaTime;
    }

    private void Update()
    {
        if (grounded && onSlope)
        {
            // Only allow side-to-side movement on slope + forced slide direction
            horizontalMove = ComputeHorizontalMoveDirOnSlope();
        }
        else
        {
            // Full movement
            horizontalMove = ComputeHorizontalMoveDir();
            ComputeVerticalVelocity();
        }

        // Apply gravity


        verticalVelocity = Mathf.Max(verticalVelocity, terminalDownVelocity);

        Vector3 move = horizontalMove + Vector3.up * verticalVelocity;
        CollisionFlags flags = controller.Move(move * Time.deltaTime);

        SetCrouch(crouchPressed);
        Spring();
        GravityInvert(invertGravity, flags);
    }
}
