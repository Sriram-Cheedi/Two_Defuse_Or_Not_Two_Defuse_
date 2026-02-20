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
    public float jumpForce = 11.5f;
    public float jumpCooldown = 0.25f;
    public float gravityMultiplier = 2.0f;
    public float springForce = 45f;
    public float coyoteTime = 0.15f;
    public float jumpBuffer = 0.15f;

    [Header("Environment")]
    public LayerMask groundLayer;
    public float groundCheckDistance = 0.2f;

    [Header("Slope")]
    public float sideWayMoveSpeed = 30f;
    public float slopeSpeed = 10f;

    [Header("Visuals")]
    public float camDownSpeed = 5f;
    public float rotationSpeed = 200f;
}