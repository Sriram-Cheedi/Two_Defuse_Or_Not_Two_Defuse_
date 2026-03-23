using UnityEngine;

public class WireDiagnostic : MonoBehaviour
{
    void Start()
    {
        var wires = FindObjectsByType<VerletWire>(FindObjectsSortMode.None);
        Debug.Log($"Found {wires.Length} VerletWire(s)");

        foreach (var w in wires)
        {
            Debug.Log($"Wire: {w.gameObject.name} | PointCount: {w.PointCount} | Active: {w.gameObject.activeInHierarchy}");

            var mf = w.GetComponent<MeshFilter>();
            var mr = w.GetComponent<MeshRenderer>();
            var lr = w.GetComponent<LineRenderer>();
            var tube = w.GetComponent<WireTubeMesh>();

            Debug.Log($"  MeshFilter: {(mf != null ? "YES" : "MISSING")} | mesh assigned: {(mf != null && mf.mesh != null ? "YES" : "NO")}");
            Debug.Log($"  MeshRenderer: {(mr != null ? "YES" : "MISSING")} | enabled: {(mr != null && mr.enabled)}  | material: {(mr != null && mr.material != null ? mr.material.name : "NONE")}");
            Debug.Log($"  LineRenderer: {(lr != null ? "YES" : "MISSING")} | enabled: {(lr != null && lr.enabled)}");
            Debug.Log($"  WireTubeMesh: {(tube != null ? "YES" : "MISSING")}");
        }
    }
}