using UnityEngine;

public class Destroy_Wall : MonoBehaviour, Interactible_Interface
{
    void Start() { }
    void Update() { }

    public void Interact(RaycastHit hit)
    {
        Destroy(gameObject);
    }
}
