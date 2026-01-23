using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField]private float speed = 12f;
    
    [Header("Jump")]
    [SerializeField]private float jumpHeight = 3f;
    [SerializeField]private float gravity = -20f;
    
    [Header("Ground Check")]
    [SerializeField]private Transform groundCheck;
    [SerializeField]private float groundDistance = 0.4f;
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
        jumpQueued = true;
    }
    // Update is called once per frame
    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        if (isGrounded && velocity.y < 0.0f)
        {
            velocity.y = -2f;
        }
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        controller.Move(move * speed * Time.deltaTime);

        if (jumpQueued)
        {
            jumpQueued = false;
            if (isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
        }
        
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
