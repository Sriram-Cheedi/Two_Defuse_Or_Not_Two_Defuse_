using UnityEngine;

public class OpenDoor : MonoBehaviour
{
    [Header("Way Points")]
    [SerializeField] private Vector3 targetPosition;
    [Header("Movement")]
    [SerializeField] float speed = 5f;

    public bool isLeft;

    public Rigidbody rb {get; private set;}
    private LedManager ledManager;
    private bool isSolved;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        ledManager = FindAnyObjectByType<LedManager>();
        if (isLeft)
        {
            targetPosition = new Vector3(rb.position.x, rb.position.y, rb.position.z - 5);
        }
        else
        {
            targetPosition = new Vector3(rb.position.x, rb.position.y, rb.position.z + 5);
        }
        
    }

    private void Update()
    {
        isSolved = ledManager.GetIsSolved();
    }

    private void DoorOpening()
    {
        if (isSolved)
        {
            Vector3 newPosition = Vector3.MoveTowards(rb.position, targetPosition, speed * Time.fixedDeltaTime);
            rb.MovePosition(newPosition);
        }
    }

    private void FixedUpdate()
    {
        DoorOpening();
    }
    
}