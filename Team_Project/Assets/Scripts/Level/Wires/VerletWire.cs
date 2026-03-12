using UnityEngine;
using System.Collections.Generic;

// [RequireComponent(typeof(LineRenderer))]

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
    public Transform anchor;
    public WireGravity gravitySource;
    [Header("Ceiling Collision")]
    public bool ceilingCollision = true;
    public float ropeRadius = 0.25f;  
    public BoxCollider ceilingBox;
    
    [Header("Ceiling Collision")]
    public bool leftWallCollision = true;
    public BoxCollider leftBox;
    
    [Header("Rope")]
    public int points = 50;
    public float segmentLength = 0.5f;
    [Header("Feel")]
    [Range(0f, 0.2f)] public float damping = 0.02f;
    [Range(1, 30)] public int iterations = 12;
    private LineRenderer lr;
    private Vector3[] RopePoint;     
    private Vector3[] RopePointOld; 
    [Header("Wire Look")]
    public LineRenderer copperLR;
    public float exposedTipLength = 0.08f;   
    public float insulatedWidth = 0.5f;
    public float copperWidth = 0.012f;
    public Material insulatedMat;
    public Material copperMat;
    [Header("Rope-Rope Collision")]
    public bool ropeRopeCollision = true;
    [Range(0, 4)] public int ropeRopePasses = 1; 
    public int pointSkip = 1;
    private int MyIndex => allwires.IndexOf(this);
    
    [Header("Bending wire ")]
    [Range(0f, 1f)] public float bendStiffness = 0.08f; 
    [Range(0, 10)] public int bendPasses = 1;         
    
    [Header("Self Collision (wire vs itself)")]
    public bool selfCollision = true;
    [Range(2, 20)] public int selfCollisionSkipNear = 6;
    [Range(0, 4)] public int selfCollisionPasses = 1;     
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
        if (copperLR != null)
        {
            copperLR.useWorldSpace = true;
            copperLR.alignment = LineAlignment.View;
            copperLR.textureMode = LineTextureMode.Tile;
            copperLR.startWidth = copperWidth;
            copperLR.endWidth = copperWidth;
            copperLR.numCapVertices = 8;
            copperLR.numCornerVertices = 8;
            copperLR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            copperLR.receiveShadows = false;
            if (copperMat != null)
            {
                copperLR.material = copperMat;
            }
            else
            {
                copperLR.material = new Material(Shader.Find("Sprites/Default"));
            }
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
        for (int i = 0; i < points; i++)
        {
            Vector3 pos = anchor.position + Vector3.down * (segmentLength * i);
            RopePoint[i] = pos;
            RopePointOld[i] = pos;
        }
        lr.positionCount = points;
        ApplyThickness();
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
    
    private void SolveleftCeiling()
    {
        if (!ceilingCollision)
        {
            return;
        }

        float maxY;

        if (ceilingBox != null)
        {
            maxY = ceilingBox.bounds.min.x - ropeRadius;
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
    
    private void SolveBend()
    {
        if (bendStiffness <= 0f) return;

        float rest = segmentLength * 2f;

        for (int i = 2; i < points - 1; i++)
        {
            Vector3 p0 = RopePoint[i - 1];
            Vector3 p1 = RopePoint[i];
            Vector3 p2 = RopePoint[i + 1];

            Vector3 d = p2 - p0;
            float dist = d.magnitude;
            if (dist < 1e-6f) continue;

            float diff = (dist - rest) / dist;

            Vector3 corr = d * diff * bendStiffness;

            RopePoint[i - 1] += corr * 0.5f;
            RopePoint[i + 1] -= corr * 0.5f;

            Vector3 midTarget = (RopePoint[i - 1] + RopePoint[i + 1]) * 0.5f;
            RopePoint[i] = Vector3.Lerp(RopePoint[i], midTarget, bendStiffness * 0.25f);
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
    private void ApplyThickness()
    {
        if (lr != null)
        {
            lr.startWidth = insulatedWidth;
            lr.endWidth = insulatedWidth;
        }
        if (copperLR != null)
        {
            copperLR.startWidth = copperWidth;
            copperLR.endWidth = copperWidth;
        }
    }
    private void UpdateCopperTip()
    {
        if (copperLR == null || RopePoint == null || points < 2)
		{
			return;
		}
        Vector3 tipEnd = RopePoint[points - 1];
        Vector3 tipDir = (RopePoint[points - 1] - RopePoint[points - 2]).normalized;

        if (tipDir.sqrMagnitude < 1e-8f)
        {
            tipDir = Vector3.down;
        }

        Vector3 tipStart = tipEnd - tipDir * exposedTipLength;

        copperLR.positionCount = 2;
        copperLR.SetPosition(0, tipStart);
        copperLR.SetPosition(1, tipEnd);
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
        RopePoint[0] = anchor.position;
        RopePoint[1] = anchor.position + anchor.TransformDirection(Vector3.down) * segmentLength;
        for (int it = 0; it < iterations; it++)
        {
            RopePoint[0] = anchor.position;
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

            for (int b = 0; b < bendPasses; b++)
            {
                SolveBend();
            }
            SolveCeiling();
            SolveleftCeiling();
        }
        for (int p = 0; p < ropeRopePasses; p++)
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
        UpdateCopperTip();
    }
}