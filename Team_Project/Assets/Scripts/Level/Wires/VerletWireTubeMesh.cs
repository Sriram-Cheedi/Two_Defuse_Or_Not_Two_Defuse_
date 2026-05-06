using System;
using UnityEngine;
//https://nothkedev.blogspot.com/2018/08/procedurally-generated-meshes-in-unity.html
// https://discussions.unity.com/t/generating-a-procedural-cylindrical-mesh-following-a-path/251777/2

public class VerletWireTubeMesh : MonoBehaviour
{

    [SerializeField] private int numberOfSides = 8;
    [SerializeField] private float wireRadius = 0.04f;
    [SerializeField] private bool showEndCaps = false;
    [SerializeField] private Material wireMaterial;
    // [SerializeField] private float uv = 1; 
    private VerletInsulatedWire wire;
    private Mesh wireMesh;
    private MeshFilter wireMeshFilter;
    private MeshRenderer wireMeshRenderer;
    private Vector3 [] vertices;
    private Vector3[] normals;
    private Vector2[] uvs;
    private int[] triangles;
    private int lastwirePoint = -1;
    private int lastSide = -1;

    private void Awake()
    {
        wire = GetComponent<VerletInsulatedWire>();
        wireMeshFilter = GetComponent<MeshFilter>();
        wireMeshRenderer = GetComponent<MeshRenderer>();
        wireMesh = new Mesh{name = "WireTube"};
        wireMeshFilter.mesh = wireMesh;
        wireMeshRenderer.material = wireMaterial;
    }

    private void LateUpdate()
    {
        BuildMesh();
    }

    private void BuildMesh()
    {
        int wirePoints = wire.WirePointsCount();
        int pointRings = wirePoints;
        int ringVertices = numberOfSides  + 1;
        int ringClosures = 0;
        int tranlgeClosures = 0;
        if (showEndCaps)
        {
            ringClosures = 2*(numberOfSides + 2);
            tranlgeClosures = 2* numberOfSides * 3;
        }
        else
        {
            ringClosures = 0;
            tranlgeClosures = 0;
        }

        int totalVertices = pointRings * ringVertices + ringClosures;
        if (lastwirePoint != wirePoints || lastSide != numberOfSides)
        {
            vertices = new Vector3[totalVertices];
            normals = new Vector3[totalVertices];
            uvs = new Vector2[totalVertices];
            int totalTriangleIndices = (pointRings - 1) * numberOfSides *6 + tranlgeClosures;
            triangles = new int[totalTriangleIndices];
            lastwirePoint = wirePoints;
            lastSide = numberOfSides;
        }

        Vector3[] worldPoints = new Vector3[wirePoints];
        Vector3[] points = new Vector3[wirePoints];

        for (int i = 0; i < wirePoints; i++)
        {
            worldPoints[i] = wire.GetPointPosition(i);
            points[i] = transform.InverseTransformPoint(worldPoints[i]);
        }
        Vector3[] wireDirections = WireDirections(worldPoints, wirePoints);
        Vector3[] frameNormals = BuildFrames(wireDirections, wirePoints);
        for (int i = 0; i < wirePoints; i++)
        {
            wireDirections[i] = transform.InverseTransformDirection(wireDirections[i]);
            frameNormals[i] = transform.InverseTransformDirection(frameNormals[i]);
        }
        
        float angleStep = Mathf.PI * 2f / numberOfSides;
        for (int i = 0; i < pointRings; i++)
        {
            Vector3 center = points[i];
            Vector3 direction = wireDirections[i].normalized;
            Vector3 normal1 = frameNormals[i].normalized;
            Vector3 normal2 = Vector3.Cross(direction, normal1).normalized;
        
            for (int j = 0; j <= numberOfSides; j++)
            {
                float angle = j * angleStep;
                Vector3 localNormal =  normal1 * Mathf.Cos(angle) + normal2 * Mathf.Sin(angle);
                int index = i * ringVertices + j;
                vertices[index] = center + localNormal * wireRadius;
                normals[index] = localNormal;
            }
        }

        int triangleIndex = 0;
        for (int i = 0; i < pointRings - 1; i++)
        {
            for (int j = 0; j < numberOfSides; j++)
            {
                int a = i * ringVertices + j;
                int b = i * ringVertices + j + 1;
                int c = (i + 1) * ringVertices + j;
                int d = (i + 1) * ringVertices + j + 1;
                triangles[triangleIndex++] = a;
                triangles[triangleIndex++] = c;
                triangles[triangleIndex++] = b;
                triangles[triangleIndex++] = b;
                triangles[triangleIndex++] = c;
                triangles[triangleIndex++] = d;
            }
        }
        wireMesh.Clear();
        wireMesh.vertices = vertices;
        wireMesh.normals = normals;
        wireMesh.triangles = triangles;
        wireMesh.RecalculateBounds();
    }
    static Vector3[] WireDirections(Vector3[] worldPoints, int wirePoints)
    {
        var wireDirections = new Vector3[wirePoints];
        for (int i = 0; i < wirePoints; i++)
        {
            Vector3 previous = worldPoints[Mathf.Max(i - 1, 0)];
            Vector3 next = worldPoints[Mathf.Min(i + 1, wirePoints - 1)];
            Vector3 direction = next - previous;
            wireDirections[i] = direction.normalized ;
        }
        return wireDirections;
    }
    
    static Vector3[] BuildFrames(Vector3[] wireDirections, int wirePoints)
    {
        Vector3[] frameNormals = new Vector3[wirePoints];

        for (int i = 0; i < wirePoints; i++)
        {
            Vector3 direction = wireDirections[i].normalized;

            Vector3 decideDirection;
            if (Mathf.Abs(Vector3.Dot(direction, Vector3.up)) < 0.99f)
            {
                decideDirection = Vector3.up;
            }
            else
            {
                decideDirection = Vector3.right;
            }

            frameNormals[i] = Vector3.Cross(direction, decideDirection).normalized;
        }

        return frameNormals;
    }
}


