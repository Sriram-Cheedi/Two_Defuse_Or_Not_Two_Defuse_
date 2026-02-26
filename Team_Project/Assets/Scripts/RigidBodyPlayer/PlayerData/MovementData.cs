using UnityEngine;

[CreateAssetMenu(fileName = "MovementData", menuName = "Player/MovementData")]
public class MovementData : ScriptableObject
{
    [Header("Speeds")]
    public float walkSpeed = 10f;
    public float crouchSpeed = 5f;
    public float groundAcceleration = 35f;
    public float airAcceleration = 15f;

    [Header("Jumping & Gravity")]
    public float jumpForceOnMetal = 11.5f;
    public float jumpForceOnPlastic = 11.5f;
    public float jumpCutMultiplier = 3f;
    public float apexGravityMultiplier = 0.8f;
    public float apexThreshold = 0.5f;
    public float fallMultiplier = 1.5f;
    public float jumpCooldown = 0.25f;
    public float gravityMultiplier = 2.0f;
    public float coyoteTime = 0.15f;
    public float jumpBuffer = 0.15f;
    

    [Header("Slope")]
    public float sideWayMoveSpeed = 30f;
    public float slopeSpeed = 10f;

    [Header("Visuals")]
    public float crouchTransitionSpeed = 5f;
    public float crouchHeightMultiplier = 0.5f;
    public float rotationSpeed = 200f;
}