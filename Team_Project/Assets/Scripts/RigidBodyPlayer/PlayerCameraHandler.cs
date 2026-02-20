using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class PlayerCameraHandler : MonoBehaviour
{
    [SerializeField] private CapsuleCollider controller;
    [SerializeField] public Transform cameraPosition;
    [SerializeField] public PlayerCam playerCam;

    private void Awake()
    {
        if (controller == null) controller = GetComponent<CapsuleCollider>();
        if (playerCam == null) playerCam = FindAnyObjectByType<PlayerCam>();
    }

    private void LateUpdate()
    {
        if (controller == null || cameraPosition == null ||  playerCam == null) return;

        // Identical to your old CamFollow()
        float bottomLocal = controller.center.y - controller.height / 2.0f;
        float cameraCenterY = bottomLocal + controller.height;

        Vector3 p = cameraPosition.localPosition;
        p.y = cameraCenterY;
        cameraPosition.localPosition = p;
    }
}