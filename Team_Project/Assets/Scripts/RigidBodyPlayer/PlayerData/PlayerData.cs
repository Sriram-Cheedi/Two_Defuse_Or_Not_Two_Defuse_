using UnityEngine;

[CreateAssetMenu(fileName = "MovementData", menuName = "Player/MovementData")]
public class MovementData : ScriptableObject
{
    [Header("Speeds")]
    public float walkSpeed = 10f;
    public float crouchSpeed = 5f;
    public float gridSpeed = 7f;
    public float groundAcceleration = 35f;
    public float airAcceleration = 15f;
    public float airApexAcceleration = 25f;

    [Header("Jumping & Gravity")]
    public float jumpForceOnMetal = 11.5f;
    public float jumpForceOnPlastic = 11.5f;
    public float jumpForceOnSemiConductor = 7f;
    public float jumpCutMultiplier = 3f;
    public float apexGravityMultiplier = 0.8f;
    public float apexThreshold = 0.5f;
    public float fallMultiplier = 1.5f;
    public float jumpCooldown = 0.25f;
    public float gravityMultiplier = 2.0f;
    public float coyoteTime = 0.15f;
    public float jumpBuffer = 0.15f;
    public float gravityAlpha = 0.1f;
    public float airTimeBuffer = 0.15f;
    

    [Header("Slope")]
    public float sideWayMoveSpeed = 30f;
    public float slopeSpeed = 10f;
    
    [Header("Step Handling")]
    public float stepOffset = 0.3f;      // max climbable height
    public float stepSmooth = 0.1f;      // lift per FixedUpdate tick
    public float groundClearance = 0.05f; // ray lift above floor
    public float forwardProbe = 0.1f;

    [Header("Visuals")]
    public float crouchTransitionSpeed = 5f;
    public float crouchHeightMultiplier = 0.5f;
    public float rotationSpeed = 200f;
    public float deathRotationSpeed = 20f;
    public float vignetteIntensity = 0.4f;
    public float vignetteFadeSpeed = 1.5f;
    public float blackFadeSpeed = 0.8f;
    
    [Header("Sound Effects")]
    public float stepInterval = 0.5f;
    public float volumnBoost = 2.0f;
}