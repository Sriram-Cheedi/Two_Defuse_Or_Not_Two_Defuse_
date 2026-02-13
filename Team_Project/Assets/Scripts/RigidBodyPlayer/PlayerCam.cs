using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCam : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private float interactDistance = 5f;
    [SerializeField] private LayerMask interactableLayer;

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

    public void OnClick(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            // Use 'cam' instead of 'this' to access the camera's method
            Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, interactDistance, interactableLayer))
            {
                Debug.Log($"Hit: {hit.collider.name}");

                if (hit.collider.TryGetComponent<Interactible_Interface>(out var interactable))
                {
                    interactable.Interact();
                }
            }
        }
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

        transform.rotation = Quaternion.Euler(xRotation, yRotation, rollZ);
        orientation.rotation = Quaternion.Euler(0f, yRotation, rollZ);
    }
}
