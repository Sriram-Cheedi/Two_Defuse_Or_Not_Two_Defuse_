// using UnityEngine;
//
// public class WireCutter : MonoBehaviour
// {
//     public Camera cam;
//     public float cutDistance = 5f;
//     public LayerMask wireLayer;
//     public KeyCode cutKey = KeyCode.Mouse0;
//
//     void Update()
//     {
//         if (VerletWire.HasAnyWireBeenCut) return;
//         if (!Input.GetKeyDown(cutKey)) return;
//         if (cam == null) return;
//
//         Ray ray = cam.ScreenPointToRay(Input.mousePosition);
//
//         if (Physics.Raycast(ray, out RaycastHit hit, cutDistance, wireLayer))
//         {
//             VerletWire wire = hit.collider.GetComponentInParent<VerletWire>();
//             if (wire != null)
//             {
//                 wire.CutAtWorldPosition(hit.point);
//             }
//         }
//     }
// }

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
        if (Physics.Raycast(ray, out RaycastHit hit, cutDistance, wireLayer))
        {
            VerletInsulatedWire wire = hit.collider.GetComponentInParent<VerletInsulatedWire>();
            if (wire != null)
            {
                wire.CutPosition(hit.point);
            }
        }
    }
}