using UnityEngine;

public class MovePlat : MonoBehaviour
{
    [Header("Way Points")]
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [Header("Movement")]
    [SerializeField] float speed = 10f;

    public Rigidbody rb {get; private set;}
    private Vector3 targetPosition;
    private Vector3 lastPosition;
    public Vector3 Velocity { get; private set; }
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }
    
    private void Start()
    {
        rb.position = pointA.position;
        targetPosition = pointB.position;
        lastPosition = rb.position;  
    }

    private void FixedUpdate()
    {
        Vector3 newPosition = Vector3.MoveTowards(rb.position, targetPosition, speed * Time.fixedDeltaTime);
        rb.MovePosition(newPosition);
        Velocity = (newPosition - lastPosition) / Time.fixedDeltaTime;
        lastPosition = newPosition;
        if (Vector3.Distance(newPosition, targetPosition) < 0.05f)
        {
            targetPosition = (targetPosition == pointA.position) ? pointB.position : pointA.position;
        }
    }
    
}