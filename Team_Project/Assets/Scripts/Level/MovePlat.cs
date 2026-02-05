using UnityEngine;

public class MovePlat : MonoBehaviour
{
    [Header("Way Points")]
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [Header("Movement")]
    [SerializeField] float speed = 10f;
    [SerializeField] float delay = 0.5f;
    
    [SerializeField] private Rigidbody rb;
    private Vector3 targetPosition;
    private Vector3 prevPosition;
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
        prevPosition = rb.position;
    }

    private void FixedUpdate()
    {
        Vector3 newPosition = Vector3.MoveTowards(rb.position, targetPosition, speed * Time.fixedDeltaTime);
        rb.MovePosition(newPosition);
        Velocity = (newPosition - prevPosition) / Time.fixedDeltaTime;
        prevPosition = newPosition;
        if (Vector3.Distance(newPosition, targetPosition) < 0.05f)
        {
            targetPosition = (targetPosition == pointA.position) ? pointB.position : pointA.position;
        }
            
    }
    
}