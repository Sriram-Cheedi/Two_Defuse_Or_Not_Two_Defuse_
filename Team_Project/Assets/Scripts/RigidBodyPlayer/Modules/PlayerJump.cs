using System;
using System.Collections;
using RigidBodyPlayer.Modules;
using RigidBodyPlayer.PlayerStates;
using UnityEngine;

public class PlayerJump : MonoBehaviour, IPlayerModule
{
    public float CoyoteTimeCounter { get; private set; }
    public float JumpBufferCounter { get; private set; }
    public bool readyToJump;
    private RigidBodyMovement hub;
    public bool canJump;
    
    public void Initialize(RigidBodyMovement Hub)
    {
        hub = Hub;
        if (hub.relay != null)
        {
            hub.relay.JumpPressed += SetJumpBuffer;
        }
    }

    private void OnDisable()
    {
        if (hub.relay != null)
        {
            hub.relay.JumpPressed -= SetJumpBuffer;
        }
    }

    public void Tick()
    {
        if (hub.sensor.IsGrounded)
        {
            CoyoteTimeCounter = hub.data.coyoteTime;
        }
        else
        {
            CoyoteTimeCounter -= Time.deltaTime;
        }

        if (JumpBufferCounter > 0f)
        {
            JumpBufferCounter -= Time.deltaTime;
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
        JumpBufferCounter = hub.data.jumpBuffer;
    }
    
    public void OnJumpExecuted()
    {
        StartCoroutine(JumpCooldownRoutine());
        ResetJump();
        hub.stateMachine.QueueNextState(PlayerState.Airborne);
    }
    
    private IEnumerator JumpCooldownRoutine()
    {
        yield return new WaitForSeconds(hub.data.jumpCooldown);
        readyToJump = true;
    }

    private void ResetJump()
    {
        readyToJump = false;
        CoyoteTimeCounter = 0f;
        JumpBufferCounter = 0f;
    }
}
