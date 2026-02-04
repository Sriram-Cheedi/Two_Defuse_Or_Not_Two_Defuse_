using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCam : MonoBehaviour
{
    [SerializeField] private float sensX = 10f;
    [SerializeField] private float sensY = 10f;
    [SerializeField] private Transform orientation;
    private Vector2 lookInput;
    public float rollZ;
    private float xRotation;
    private float yRotation;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    public void OnLook(InputAction.CallbackContext ctx)
    {
        lookInput = ctx.ReadValue<Vector2>();
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
