using UnityEngine;

public class GravitySection : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out RigidBodyMovement hub))
        {
            hub.sensor.InGravityZone = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out RigidBodyMovement hub))
        {
            hub.sensor.InGravityZone = false;
        }
    }
}
