using System.Collections;
using UnityEngine;

public class PlayerJump : MonoBehaviour
{
    public float coyoteTimeCounter;
    public float jumpBufferCounter;
    public bool readyToJump;
    private PlayerInputRelay relay;
    private RigidBodyMovement Hub;

    private void Awake()
    {
        if (Hub == null) Hub = GetComponent<RigidBodyMovement>();
        if (relay == null) relay = GetComponent<PlayerInputRelay>();
    }
    
    public void UpdateJumpState()
    {
        if (Hub.sensor.IsGrounded)
        {
            coyoteTimeCounter = Hub.data.coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        if (jumpBufferCounter > 0f)
        {
            jumpBufferCounter -= Time.deltaTime;
        }
    }

    public void SetJumpBuffer()
    {
        jumpBufferCounter = Hub.data.jumpBuffer;
    }

    public void OnJumpExecuted()
    {
        ResetJump();
        // Hub.rb.AddForce(transform.up * Hub.data.jumpForce, ForceMode.Impulse);
        StartCoroutine(JumpCooldownRoutine());
        Hub.TransitionToState(Hub.AirborneState);
    }

    public void HandleJumpCut()
    {
        float verticalVelocity = Vector3.Dot(Hub.rb.linearVelocity, Hub.rb.transform.up);
        if (verticalVelocity > 0f)
        {
            Hub.rb.linearVelocity -= transform.up * (verticalVelocity * 0.5f);
        }
    }
    
    private IEnumerator JumpCooldownRoutine()
    {
        yield return new WaitForSeconds(Hub.data.jumpCooldown);
        readyToJump = true;
    }

    private void ResetJump()
    {
        readyToJump = false;
        coyoteTimeCounter = 0f;
        jumpBufferCounter = 0f;
        // Vector3 localUp = Hub.rb.transform.up;
        // Vector3 verticalVelocity = Vector3.Project(Hub.rb.linearVelocity, localUp);
        // Hub.rb.linearVelocity -= verticalVelocity;
    }
}
