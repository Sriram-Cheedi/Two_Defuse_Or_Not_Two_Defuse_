using RigidBodyPlayer.Modules;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class PlayerCameraHandler : MonoBehaviour, IPlayerModule
{
    [SerializeField] private CapsuleCollider controller;
    [SerializeField] public Transform cameraPosition;
    [SerializeField] public PlayerCam playerCam;
    public void Initialize(RigidBodyMovement hub)
    {
        if (controller == null) controller = GetComponent<CapsuleCollider>();
        if (playerCam == null) playerCam = FindAnyObjectByType<PlayerCam>();
    }

    public void Tick()
    {
        
    }

    public void FixedTick()
    {
        
    }

    public void LateTick()
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