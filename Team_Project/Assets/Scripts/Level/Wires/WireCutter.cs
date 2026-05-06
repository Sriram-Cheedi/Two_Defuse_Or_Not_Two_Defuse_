using UnityEngine;

public class WireCutter : MonoBehaviour
{
    public Camera cam;
    public float cutDistance = 5f;
    public LayerMask wireLayer;
    public KeyCode cutKey = KeyCode.Mouse0;

    void Update()
    {
        if (VerletInsulatedWire.HasAnyWireBeenCut)
        {
            return;
        }

        if (!Input.GetKeyDown(cutKey))
        {
            return;
        }

        if (cam == null)
        {
            return;
        }
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray, cutDistance, wireLayer);
        RaycastHit hit = hits[0];
        VerletInsulatedWire wire = hit.collider.GetComponentInParent<VerletInsulatedWire>();
        if (wire != null) 
        {
            wire.CutPosition(hit.point);
        }
    }
}