using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCam : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float sensX = 10f;
    [SerializeField] private float sensY = 10f;
    [SerializeField] private Transform orientation;
    private Vector2 lookInput;
    public float rollZ;
    private float xRotation;
    private float yRotation;
    private PlayerInputRelay relay;
    private const string fovKEY = "FOV";
    private const float fovDefault = 90f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        relay = FindFirstObjectByType<PlayerInputRelay>();
        if (orientation == null)
        {
            Debug.LogError($"No orientation found on {gameObject.name}" + "please assign them in the Inspector.", this);
        }
    }

    private void OnEnable()
    {
        relay.Look += SetLookInput ;
        SpawnManager.RespawnRequested += ResetCamera;
    }

    private void ResetCamera(RespawnData data)
    {
        yRotation = data.Rotation.eulerAngles.y;
        xRotation = data.Rotation.eulerAngles.x;
        if (data.IsUpsideDown)
        {
            rollZ = 180;
        }
        else
        {
            rollZ = 0;
        }
    }
    
    private void OnDisable()
    {
        relay.Look -= SetLookInput ;
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (playerCamera == null)
        {
            playerCamera = GetComponent<Camera>() ?? GetComponentInChildren<Camera>();
        }

        if (playerCamera != null)
        {
            playerCamera.fieldOfView = PlayerPrefs.GetFloat(fovKEY, fovDefault);
        }
    }

    private void SetLookInput(Vector2 v)
    {
        lookInput = v;
    }
    
    // Update is called once per frame
    private void LateUpdate()
    {
        Look();
    }

    private void Look()
    {
        float mouseX = lookInput.x * sensX * Time.deltaTime;
        float mouseY = lookInput.y * sensY * Time.deltaTime;
        float sign = Vector3.Dot(transform.up, Vector3.up) < 0f ? -1f : 1f;
        mouseX *= sign;
        mouseY *= sign;
        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        transform.rotation = Quaternion.Euler(xRotation, yRotation , rollZ);
        orientation.rotation = Quaternion.Euler(0f, yRotation, rollZ);
    }
}
