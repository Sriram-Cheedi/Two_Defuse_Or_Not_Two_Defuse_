using RigidBodyPlayer.Modules;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class PlayerCrouch : MonoBehaviour, IPlayerModule
{
    [Header("Refs")]
    [SerializeField] private CapsuleCollider controller;

    private RigidBodyMovement Hub;
    

    // moved fields
    private float standCenterY;
    private float standHeight;
    private float crouchHeight;
    private Vector3 crouchOrigin;
    private Vector3 standOrigin;

    public void Initialize(RigidBodyMovement hub)
    {
        if (controller == null) controller = GetComponent<CapsuleCollider>();
        Hub = hub;
        
        standHeight = controller.height;
        standCenterY = controller.center.y;
        crouchHeight = controller.height * Hub.data.crouchHeightMultiplier;

        SetStandCenter();
        SetCrouchCenter();
    }

    public void Tick()
    {
        SetCrouch(Hub.isCrouchPressed);
    }

    public void FixedTick()
    {
        
    }

    public void LateTick()
    {
        
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
        if (isCrouch)
        {
            controller.height = Mathf.Lerp(controller.height, crouchHeight, Hub.data.crouchTransitionSpeed * Time.deltaTime);
            controller.center = Vector3.Lerp(controller.center, crouchOrigin, Hub.data.crouchTransitionSpeed * Time.deltaTime);
        }
        else
        {
            controller.height = Mathf.Lerp(controller.height, standHeight, Hub.data.crouchTransitionSpeed * Time.deltaTime);
            controller.center = Vector3.Lerp(controller.center, standOrigin, Hub.data.crouchTransitionSpeed * Time.deltaTime);
        }
    }
}
