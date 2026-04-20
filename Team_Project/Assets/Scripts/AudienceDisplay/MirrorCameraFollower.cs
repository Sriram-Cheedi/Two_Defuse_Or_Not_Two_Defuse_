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
        if (sourceCamera == null) return;

        transform.position = sourceCamera.transform.position;
        transform.rotation = sourceCamera.transform.rotation;

        mirrorCamera.fieldOfView = sourceCamera.fieldOfView;
        mirrorCamera.nearClipPlane = sourceCamera.nearClipPlane;
        mirrorCamera.farClipPlane = sourceCamera.farClipPlane;
        mirrorCamera.cullingMask = sourceCamera.cullingMask;
        mirrorCamera.clearFlags = sourceCamera.clearFlags;
        mirrorCamera.backgroundColor = sourceCamera.backgroundColor;
        mirrorCamera.orthographic = sourceCamera.orthographic;
        mirrorCamera.orthographicSize = sourceCamera.orthographicSize;
    }
}