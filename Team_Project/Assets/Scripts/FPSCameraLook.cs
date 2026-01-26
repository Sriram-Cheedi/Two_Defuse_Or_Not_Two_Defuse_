using UnityEngine;
using UnityEngine.InputSystem;

public class FPSCameraLook : MonoBehaviour
{
    [SerializeField]private Transform playerBody; 
    [SerializeField]private Transform cameraPivot; 
    [SerializeField]private float mouseSensitivity = 20f; 
    [SerializeField]private CharacterController controller;
    float xRotation = 0f;
    private Vector2 lookInput;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    public void OnLook(InputAction.CallbackContext ctx)
    {
        lookInput = ctx.ReadValue<Vector2>();
    }
    // Update is called once per frame
    void Update()
    {
        float bottomLocal = controller.center.y - controller.height / 2.0f;
        float cameraCenterY = bottomLocal + controller.height;
        cameraPivot.transform.localPosition = new Vector3(cameraPivot.transform.localPosition.x, 
            cameraCenterY,cameraPivot.transform.localPosition.z);
        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        cameraPivot.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);
    }
}
