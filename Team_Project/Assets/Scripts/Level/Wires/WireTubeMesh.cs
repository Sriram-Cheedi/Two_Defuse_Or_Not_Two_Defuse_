using UnityEngine;

[RequireComponent(typeof(VerletWire))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class WireTubeMesh : MonoBehaviour
{
    [Range(3, 16)] public int sides = 8;
    public float radius = 0.04f;
    public bool showEndCaps = true;
    public Material wireMaterial;
    public float uvTileLength = 1f;
    private VerletWire   _wire;
    private Mesh         _mesh;
    private MeshFilter   _mf;
    private MeshRenderer _mr;
    private Vector3[] _verts;
    private Vector3[] _normals;
    private Vector2[] _uvs;
    private int[]     _tris;
    private int _lastPointCount = -1;
    private int _lastSides      = -1;

    void Awake()
    {
        _wire = GetComponent<VerletWire>();
        _mf   = GetComponent<MeshFilter>();
        _mr   = GetComponent<MeshRenderer>();
        _mesh           = new Mesh { name = "WireTube" };
        _mesh.MarkDynamic();
        _mf.mesh        = _mesh;
        if (wireMaterial != null)
        {
            _mr.material = wireMaterial;
        }
        var lr = GetComponent<LineRenderer>();
        if (lr != null)
        {
            lr.enabled = false;
            lr.forceRenderingOff = true;
        }
    }

    void LateUpdate()
    {
        if (_wire == null || _wire.PointCount < 2) return;
        BuildMesh();
    }

    void BuildMesh()
    {
        int ptCount  = _wire.PointCount;
        int rings    = ptCount;
        int vertRing = sides + 1;
        int capVerts = showEndCaps ? 2 * (sides + 2) : 0;
        int capTris  = showEndCaps ? 2 * sides * 3   : 0;
        int totalVerts = rings * vertRing + capVerts;
        int totalTris  = (rings - 1) * sides * 6 + capTris;
        if (_lastPointCount != ptCount || _lastSides != sides)
        {
            _verts = new Vector3[totalVerts];
            _normals = new Vector3[totalVerts];
            _uvs = new Vector2[totalVerts];
            _tris = new int[totalTris];
            _lastPointCount = ptCount;
            _lastSides = sides;
        }
        Vector3[] worldPoints = new Vector3[ptCount];
        Vector3[] points = new Vector3[ptCount];  
        for (int i = 0; i < ptCount; i++)
        {
            worldPoints[i] = _wire.GetPoint(i);
            points[i] = transform.InverseTransformPoint(worldPoints[i]);
        }
        Vector3[] tangents = BuildTangents(worldPoints, ptCount);
        Vector3[] frameNorms = BuildFrames(tangents, ptCount);
        
        for (int i = 0; i < ptCount; i++)
        {
            tangents[i] = transform.InverseTransformDirection(tangents[i]);
            frameNorms[i] = transform.InverseTransformDirection(frameNorms[i]);
        }
        float angleStep = Mathf.PI * 2f / sides;
        float runningV = 0f;
        for (int ring = 0; ring < rings; ring++)
        {
            Vector3 center = points[ring];
            Vector3 tangent = tangents[ring].sqrMagnitude > 0.000001f ? tangents[ring].normalized : Vector3.forward;
            Vector3 n0 = frameNorms[ring].sqrMagnitude > 0.000001f ? frameNorms[ring].normalized : Vector3.up;
            Vector3 b0 = Vector3.Cross(tangent, n0).normalized;

            if (ring > 0)
                runningV += Vector3.Distance(points[ring], points[ring - 1]);

            float v = runningV / Mathf.Max(uvTileLength, 0.001f);

            for (int s = 0; s <= sides; s++)
            {
                float angle = s * angleStep;
                Vector3 localNormal = Mathf.Cos(angle) * n0 + Mathf.Sin(angle) * b0;
                int idx = ring * vertRing + s;
                _verts[idx] = center + localNormal * radius;
                _normals[idx] = localNormal;
                _uvs[idx] = new Vector2((float)s / sides, v);
            }
        }
        int ti = 0;
        for (int ring = 0; ring < rings - 1; ring++)
        {
            for (int s = 0; s < sides; s++)
            {
                int a = ring * vertRing + s;
                int b = ring * vertRing + s + 1;
                int c = (ring + 1) * vertRing + s;
                int d = (ring + 1) * vertRing + s + 1;
                _tris[ti++] = a; _tris[ti++] = c; _tris[ti++] = b;
                _tris[ti++] = b; _tris[ti++] = c; _tris[ti++] = d;
            }
        }

        if (showEndCaps)
        {
            int baseVert = rings * vertRing;
            ti = WriteEndCap(points[0], -tangents[0], frameNorms[0], baseVert, ti, false);
            int endCapBase = baseVert + sides + 2;
            ti = WriteEndCap(points[ptCount - 1], tangents[ptCount - 1], frameNorms[ptCount - 1], endCapBase, ti, true);
        }

        _mesh.Clear();
        _mesh.vertices = _verts;
        _mesh.normals = _normals;
        _mesh.uv = _uvs;
        _mesh.triangles = _tris;
        _mesh.RecalculateBounds();
    }

    int WriteEndCap(Vector3 center, Vector3 normal, Vector3 frameN, int vertBase, int triBase, bool flipWinding)
    {
        normal = normal.sqrMagnitude > 0.000001f ? normal.normalized : Vector3.forward;
        Vector3 b = Vector3.Cross(normal, frameN).normalized;
        Vector3 n = Vector3.Cross(b, normal).normalized;
        float angleStep = Mathf.PI * 2f / sides;
        _verts[vertBase] = center;
        _normals[vertBase] = normal;
        _uvs[vertBase] = new Vector2(0.5f, 0.5f);
        for (int s = 0; s <= sides; s++)
        {
            float   angle = s * angleStep;
            Vector3 dir = Mathf.Cos(angle) * n + Mathf.Sin(angle) * b;
            int     idx = vertBase + 1 + s;
            _verts[idx] = center + dir * radius;
            _normals[idx] = normal;
            _uvs[idx] = new Vector2(Mathf.Cos(angle) * 0.5f + 0.5f, Mathf.Sin(angle) * 0.5f + 0.5f);
        }

        for (int s = 0; s < sides; s++)
        {
            int ctr = vertBase;
            int cur = vertBase + 1 + s;
            int next = vertBase + 1 + s + 1;

            if (flipWinding)
            {
                _tris[triBase++] = ctr;
                _tris[triBase++] = next;
                _tris[triBase++] = cur;
            }
            else
            {
                _tris[triBase++] = ctr;
                _tris[triBase++] = cur;
                _tris[triBase++] = next;
            }
        }
        return triBase;
    }
    
    static Vector3[] BuildTangents(Vector3[] pts, int count)
    {
        var t = new Vector3[count];
        for (int i = 0; i < count; i++)
        {
            Vector3 prev = pts[Mathf.Max(i - 1, 0)];
            Vector3 next = pts[Mathf.Min(i + 1, count - 1)];
            Vector3 d = next - prev;
            t[i] = d.sqrMagnitude > 0.000001f ? d.normalized : Vector3.forward;
        }
        return t;
    }

    static Vector3[] BuildFrames(Vector3[] tangents, int count)
    {
        var normals = new Vector3[count];
        Vector3 seed = Mathf.Abs(Vector3.Dot(tangents[0], Vector3.up)) < 0.99f ? Vector3.up : Vector3.right;
        normals[0] = Vector3.Cross(tangents[0], seed).normalized;
        for (int i = 1; i < count; i++)
        {
            Vector3 t0 = tangents[i - 1];
            Vector3 t1 = tangents[i];
            Vector3 axis = Vector3.Cross(t0, t1);
            if (axis.sqrMagnitude < 0.000001f)
            {
                normals[i] = normals[i - 1];
            }
            else
            {
                float angle = Mathf.Acos(Mathf.Clamp(Vector3.Dot(t0, t1), -1f, 1f));
                normals[i]  = Quaternion.AngleAxis(angle * Mathf.Rad2Deg, axis.normalized) * normals[i - 1];
            }
        }
        return normals;
    }
}