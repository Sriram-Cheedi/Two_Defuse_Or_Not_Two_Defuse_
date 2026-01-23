using UnityEngine;
using UnityEngine.InputSystem;

public class FPSCameraLook : MonoBehaviour
{
    [SerializeField]private Transform playerBody; 
    [SerializeField]private Transform cameraPivot; 
    [SerializeField]private float mouseSensitivity = 20f; 
    float xRotation = 0f;
    private Vector2 lookInput;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    public  void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
    }
    // Update is called once per frame
    void Update()
    {
        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        cameraPivot.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);
    }
}
