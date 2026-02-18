using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class PlayerCrouch : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private CapsuleCollider controller;

    [Header("Tuning (copied from old script)")]
    [SerializeField] private float camDownSpeed = 5f;

    // moved fields
    private float standCenterY;
    private float standHeight;
    private float crouchHeight;
    private Vector3 crouchOrigin;
    private Vector3 standOrigin;

    private void Awake()
    {
        if (controller == null) controller = GetComponent<CapsuleCollider>();

        // IDENTICAL setup from your old Awake
        standHeight = controller.height;
        standCenterY = controller.center.y;
        crouchHeight = controller.height / 2f;

        SetStandCenter();
        SetCrouchCenter();
    }

    public void Tick(bool isCrouchPressed)
    {
        SetCrouch(isCrouchPressed);
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
            controller.height = Mathf.Lerp(controller.height, crouchHeight, camDownSpeed * Time.deltaTime);
            controller.center = Vector3.Lerp(controller.center, crouchOrigin, camDownSpeed * Time.deltaTime);
        }
        else
        {
            controller.height = Mathf.Lerp(controller.height, standHeight, camDownSpeed * Time.deltaTime);
            controller.center = Vector3.Lerp(controller.center, standOrigin, camDownSpeed * Time.deltaTime);
        }
    }
}
