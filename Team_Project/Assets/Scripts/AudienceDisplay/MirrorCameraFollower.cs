using UnityEngine;

[RequireComponent(typeof(Camera))]
public class MirrorCameraFollower : MonoBehaviour
{
    [SerializeField] private Camera sourceCamera;
    private Camera mirrorCamera;

    private void Awake()
    {
        mirrorCamera = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        transform.position = sourceCamera.transform.position;
        transform.rotation = sourceCamera.transform.rotation;
        mirrorCamera.fieldOfView = sourceCamera.fieldOfView;
        mirrorCamera.nearClipPlane = sourceCamera.nearClipPlane;
    }
}