using UnityEngine;
using System.Collections.Generic;


public class VerletWire : MonoBehaviour
{
    private static readonly List<VerletWire> allwires = new List<VerletWire>();

    void OnEnable()
    {
        if (!allwires.Contains(this))
        {
            allwires.Add(this);
        }
    }
    void OnDisable()
    {
        allwires.Remove(this); 
    }
    public Transform anchor1;
    public Transform anchor2;
    public WireGravity gravitySource;
    [Header("Ceiling Collision")]
    public bool ceilingCollision = true;
    public float ropeRadius = 0.25f;  
    public BoxCollider ceilingBox;
    
    [Header("leftWall Collision")]
    public bool leftWallCollision = true;
    public BoxCollider leftBox;
    
    [Header("Rope")]
    public int points = 35;
    public float segmentLength = 0.4f;
    [Header("Feel")]
    [Range(0f, 0.2f)] public float damping = 0.02f;
    [Range(1, 30)] public int iterations = 12;
    private LineRenderer lr;
    private Vector3[] RopePoint;     
    private Vector3[] RopePointOld; 
    
    [Header("Wire Look")]
    public float insulatedWidth = 0.12f;
    public Material insulatedMat;
    
    [Header("Rope-Rope Collision")]
    public bool ropeRopeCollision = true;
    [Range(0, 4)] public int ropeRopePasses = 1; 
    public int pointSkip = 1;
    private int MyIndex => allwires.IndexOf(this);

    [Header("Self Collision")]
    public bool selfCollision = true;
    [Range(2, 20)] public int selfCollisionSkipNear = 6;
    [Range(0, 4)] public int selfCollisionPasses = 1;     
    [Header("Collision")]
    public bool enableCollision = true;
    private CapsuleCollider[] segmentColliders;
    
    [Header("Player Interaction")]
    public float playerPushForce = 0.05f;
    
    public LayerMask playerLayer;
    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.alignment = LineAlignment.View;          
        lr.textureMode = LineTextureMode.Tile;
        lr.startWidth = insulatedWidth;
        lr.endWidth = insulatedWidth;
        lr.numCapVertices = 8;                      
        lr.numCornerVertices = 8;                  
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;
        if (insulatedMat != null)
        {
            lr.material = insulatedMat;
        }
        else
        {
            lr.material = new Material(Shader.Find("Sprites/Default"));
        }

    }
    public int PointCount => RopePoint != null ? RopePoint.Length : 0;
    public Vector3 GetPoint(int i)
    {
        if (RopePoint == null || RopePoint.Length == 0) return Vector3.zero;
        i = Mathf.Clamp(i, 0, RopePoint.Length - 1);
        return RopePoint[i];
    }

    void Start()
    {
        if (gravitySource == null)
        {
            gravitySource = FindFirstObjectByType<WireGravity>();
        }
        RopePoint = new Vector3[points];
        RopePointOld = new Vector3[points];
        Vector3 start = anchor1.position;
        Vector3 end = anchor2.position;
       
        for (int i = 0; i < points; i++)
        {
            float t = (float)i / (points - 1);
            Vector3 pos = Vector3.Lerp(start, end, t);
            RopePoint[i] = pos;
            RopePointOld[i] = pos;
        }
        lr.positionCount = points;
        if (enableCollision)
        {
            segmentColliders = new CapsuleCollider[points - 1];

            for (int i = 0; i < points - 1; i++)
            {
                GameObject col = new GameObject("WireCollider_" + i);
                col.transform.parent = transform;

                CapsuleCollider capsule = col.AddComponent<CapsuleCollider>();
                capsule.radius = ropeRadius;
                capsule.direction = 1; 
                capsule.height = segmentLength;
                capsule.isTrigger = false;
                segmentColliders[i] = capsule;
            }
        }
    }
    
    void UpdateColliders()
    {
        if (!enableCollision || segmentColliders == null) return;

        for (int i = 0; i < points - 1; i++)
        {
            Vector3 p1 = RopePoint[i];
            Vector3 p2 = RopePoint[i + 1];

            Vector3 mid = (p1 + p2) * 0.5f;

            segmentColliders[i].transform.position = mid;
            segmentColliders[i].transform.rotation =
                Quaternion.FromToRotation(Vector3.up, p2 - p1);

            segmentColliders[i].height = Vector3.Distance(p1, p2) + ropeRadius * 2f;
        }
    }
    void PushWireFromPlayer()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, 2f, playerLayer);

        foreach (Collider hit in hits)
        {
            for (int i = 2; i < points - 2; i++)
            {
                Vector3 point = RopePoint[i];

                Vector3 closest = hit.ClosestPoint(point);

                float dist = Vector3.Distance(point, closest);

                if (dist < ropeRadius * 2f)
                {
                    Vector3 dir = (point - closest).normalized;

                    RopePoint[i] += dir * playerPushForce;
                }
            }
        }
    }

    private void SolveCeiling()
    {
        if (!ceilingCollision)
        {
            return;
        }

        float maxY;

        if (ceilingBox != null)
        {
            maxY = ceilingBox.bounds.min.y - ropeRadius;
        }
        else
        {
            return;
        }
        for (int i = 1; i < points; i++)
        {
            if (RopePoint[i].y > maxY)
            {
                RopePoint[i].y = maxY;
                RopePointOld[i].y = RopePoint[i].y; 
            }
        }
    }
    
    private void SolveLeftWall()
    {
        if (!leftWallCollision)
        {
            return;
        }

        float maxY;

        if (leftBox != null)
        {
            maxY = leftBox.bounds.min.x - ropeRadius;
        }
        else
        {
            return;
        }
        for (int i = 1; i < points; i++)
        {
            if (RopePoint[i].y > maxY)
            {
                RopePoint[i].y = maxY;
                RopePointOld[i].y = RopePoint[i].y; 
            }
        }
    }
    
    
    
    private void SolveRopeRope()
    {
        if (!ropeRopeCollision)
        {
            return;
        }

        float minDistance = ropeRadius * 2f;
        float minSquareDistance = minDistance * minDistance;

        int startIndex = 2;

        int myIdx = MyIndex;

        foreach (VerletWire rope in allwires)
        {
            if (rope == null || rope == this)
            {
                continue;
            }

            if (rope.RopePoint == null)
            {
                continue;
            }

            if (rope.MyIndex <= myIdx)
            {
                continue;
            }

            for (int i = startIndex; i < points; i += pointSkip)
            {
                Vector3 pointI = RopePoint[i];

                for (int j = startIndex; j < rope.points; j += rope.pointSkip)
                {
                    Vector3 pointJ = rope.RopePoint[j];

                    Vector3 difference = pointI - pointJ;
                    float sqrMagnitude = difference.sqrMagnitude;
                    if (sqrMagnitude >= minSquareDistance)
                    {
                        continue;
                    }
                    float distance = Mathf.Sqrt(sqrMagnitude);
                    Vector3 n = difference / distance;

                    float push = (minDistance - distance) * 0.5f;

                    RopePoint[i] += n * push;
                    rope.RopePoint[j] -= n * push;
                    RopePointOld[i] = RopePoint[i];
                    rope.RopePointOld[j] = rope.RopePoint[j];
                    pointI = RopePoint[i];
                }
            }
        }
    }
    
    private void SolveSelfCollision()
    {
        if (!selfCollision) return;

        float minDistance = ropeRadius * 2f;
        float minSquareDistance = minDistance * minDistance;

        int start = 2;

        for (int i = start; i < points; i += pointSkip)
        {
            Vector3 pointI = RopePoint[i];
            int jStart = i + selfCollisionSkipNear;

            for (int j = jStart; j < points; j += pointSkip)
            {
                Vector3 pointJ = RopePoint[j];

                Vector3 diff = pointI - pointJ;
                float squaredDiff = diff.sqrMagnitude;

                if (squaredDiff >= minSquareDistance) continue;

                float dist = Mathf.Sqrt(squaredDiff);
                Vector3 n = diff / dist;

                float push = (minDistance - dist) * 0.5f;

                RopePoint[i] += n * push;
                RopePoint[j] -= n * push;

                RopePointOld[i] = RopePoint[i];
                RopePointOld[j] = RopePoint[j];

                pointI = RopePoint[i];
            }
        }
    }

    void FixedUpdate()
    {
        Vector3 gravity = (gravitySource != null) ? gravitySource.Gravity : Physics.gravity;
        for (int i = 1; i < points; i++)
        {
            Vector3 velocity = (RopePoint[i] - RopePointOld[i]) * (1f - damping);
            RopePointOld[i] = RopePoint[i];
            RopePoint[i] = RopePoint[i] + velocity + gravity * (Time.fixedDeltaTime * Time.fixedDeltaTime);
        }
        RopePoint[0] = anchor1.position;
        RopePoint[points - 1] = anchor2.position;
        for (int it = 0; it < iterations; it++)
        {
            RopePoint[0] = anchor1.position;
            RopePoint[points - 1] = anchor2.position;
            for (int i = 0; i < points - 1; i++)
            {
                Vector3 AdjacentRopeDistance = RopePoint[i + 1] - RopePoint[i];
                float distance = AdjacentRopeDistance.magnitude;
                if (distance < 0.0001f) continue;
                float difference = (distance - segmentLength) / distance;
                if (i == 0)
                {
                    RopePoint[i + 1] -= AdjacentRopeDistance * difference;
                }
                else
                {
                    RopePoint[i] += AdjacentRopeDistance * difference * 0.5f;
                    RopePoint[i + 1] -= AdjacentRopeDistance * difference * 0.5f;
                }
            }
            SolveCeiling();
            SolveLeftWall();
        }
        for (int point = 0; point < ropeRopePasses; point++)
		{
            SolveRopeRope();
		}

        for (int s = 0; s < selfCollisionPasses; s++)
        {
            SolveSelfCollision();
        }
        
        for (int i = 0; i < points; i++)
        {
            lr.SetPosition(i, RopePoint[i]);
        } 
        UpdateColliders();
        PushWireFromPlayer();
    }
}