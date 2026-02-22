using System.Collections;
using RigidBodyPlayer.Modules;
using UnityEngine;

public class PlayerJump : MonoBehaviour, IPlayerModule
{
    public float coyoteTimeCounter;
    public float jumpBufferCounter;
    public bool readyToJump;
    private RigidBodyMovement Hub;
    
    public void Initialize(RigidBodyMovement hub)
    {
        Hub = hub;
    }

    public void Tick()
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

    public void FixedTick()
    {
        
    }

    public void LateTick()
    {
        
    }
    

    public void SetJumpBuffer()
    {
        jumpBufferCounter = Hub.data.jumpBuffer;
    }

    public void OnJumpExecuted()
    {
        ResetJump();
        StartCoroutine(JumpCooldownRoutine());
        Hub.TransitionToState(Hub.AirborneState);
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
    }
}
