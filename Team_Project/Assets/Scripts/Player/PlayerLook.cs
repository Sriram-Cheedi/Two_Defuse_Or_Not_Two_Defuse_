using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLook : MonoBehaviour
{
    public InputActionReference look;

    public float mouseSensitivity = 100f;

    float xRotation = 0f;
    float YRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        Vector2 rotation = look.action.ReadValue<Vector2>();

        xRotation -= rotation.y * mouseSensitivity * Time.deltaTime;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        YRotation += rotation.x * mouseSensitivity * Time.deltaTime;

        transform.localRotation = Quaternion.Euler(xRotation, YRotation, 0f);
    }
}
