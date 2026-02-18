using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class PlayerCameraHandler : MonoBehaviour
{
    [SerializeField] private CapsuleCollider controller;
    [SerializeField] private Transform cameraPosition;

    private void Awake()
    {
        if (controller == null) controller = GetComponent<CapsuleCollider>();
    }

    private void LateUpdate()
    {
        if (controller == null || cameraPosition == null) return;

        // Identical to your old CamFollow()
        float bottomLocal = controller.center.y - controller.height / 2.0f;
        float cameraCenterY = bottomLocal + controller.height;

        Vector3 p = cameraPosition.localPosition;
        p.y = cameraCenterY;
        cameraPosition.localPosition = p;
    }
}