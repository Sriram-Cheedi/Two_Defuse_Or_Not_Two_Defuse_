using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
public class RideMovingPlat : MonoBehaviour
{
    private Rigidbody rb;
    private MovePlat movingPlatform;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (movingPlatform != null)
        {
            Vector3 velocity = rb.linearVelocity;
            velocity.x = movingPlatform.Velocity.x;
            velocity.z = movingPlatform.Velocity.z;
            rb.linearVelocity = velocity;
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.collider.TryGetComponent(out MovePlat platform))
        {
            foreach (var contact in collision.contacts)
            {
                if (contact.normal.y > 0.5f)
                {
                    movingPlatform = platform;
                    return;
                }
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (movingPlatform != null && collision.collider.gameObject == movingPlatform.gameObject)
        {
            movingPlatform = null;
        }
    }
}
