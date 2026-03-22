using UnityEngine;
using System.Collections.Generic;

public class VerletWire : MonoBehaviour
{
    private static readonly List<VerletWire> allwires = new List<VerletWire>();

    void OnEnable()
    {
        if (!allwires.Contains(this))
            allwires.Add(this);
    }

    void OnDisable()
    {
        allwires.Remove(this);
    }

    private Vector3 _lastGravityDir = Vector3.down;

    private int _postCutFrames = 0;
    private const int POST_CUT_SETTLE_FRAMES = 30;

    [Header("Cut Piece Feel")]
    [Range(0f, 0.5f)] public float cutPieceBendStiffness  = 0.025f;
    [Range(0f, 0.2f)] public float cutPieceDamping        = 0.012f;
    [Range(0, 20)]    public int   cutSettleIterations    = 12;
    [Range(0f, 1f)]   public float cutImpulseScale        = 0.35f;

    [Header("Anchors")]
    public Transform anchor1;
    public Transform anchor2;
    public WireGravity gravitySource;

    [Header("Pinning")]
    public bool pinStart = true;
    public bool pinEnd   = true;

    [Header("Ceiling Collision")]
    public bool       ceilingCollision = true;
    public float      ropeRadius       = 0.25f;
    public BoxCollider ceilingBox;

    [Header("Left Wall Collision")]
    public bool        leftWallCollision = true;
    public BoxCollider leftBox;

    [Header("Rope")]
    public int   points        = 35;
    public float segmentLength = 0.4f;

    [Range(0f, 0.2f)] public float damping    = 0.02f;
    [Range(1, 30)]    public int   iterations = 18;  

    [Header("Wire Look")]
    public float    insulatedWidth = 0.12f;
    public Material insulatedMat;

    [Header("Rope-Rope Collision")]
    public bool ropeRopeCollision = true;
    [Range(0, 4)] public int ropeRopePasses = 1;
    public int pointSkip = 1;

    [Header("Self Collision")]
    public bool selfCollision = true;
    [Range(2, 20)] public int selfCollisionSkipNear = 6;
    [Range(0, 4)]  public int selfCollisionPasses   = 1;

    [Header("Collision")]
    public bool enableCollision = true;

    [Header("Player Interaction")]
    public float     playerPushForce = 0.05f;
    public LayerMask playerLayer;

    [Header("Cutting")]
    public VerletWire wirePiecePrefab;
    public float cutSnapImpulse  = 0.6f;
    public int   cutImpulsePoints = 4;
    public int   minPointsPerPiece = 4;

    [Header("Stability")]
    [Range(0f, 0.5f)] public float bendStiffness   = 0.18f;
    [Range(0, 4)]     public int   bendPasses       = 2;
    [Range(0f, 1f)]   public float contactDamping   = 0.85f;
    public float sleepThreshold = 0.01f;
    public bool  disableSelfCollisionWhenSingleEnded = true;

    [Header("Gravity Flip")]
    [Range(0f, 1f)]   public float gravityFlipVelocityKill = 0.92f;  
    [Range(1, 60)]    public int   gravityFlipSettleFrames = 20;    
    [Range(0f, 0.3f)] public float gravityFlipExtraDamping = 0.18f;   

    private int _gravityFlipFramesLeft = 0;

    private LineRenderer       lr;
    private Vector3[]          RopePoint;
    private Vector3[]          RopePointOld;
    private CapsuleCollider[]  segmentColliders;

    private bool initialized = false;
    private bool hasBeenCut  = false;

    private int MyIndex => allwires.IndexOf(this);
    public  int PointCount => RopePoint != null ? RopePoint.Length : 0;

    public Vector3 GetPoint(int i)
    {
        if (RopePoint == null || RopePoint.Length == 0) return Vector3.zero;
        i = Mathf.Clamp(i, 0, RopePoint.Length - 1);
        return RopePoint[i];
    }

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        if (lr == null) lr = gameObject.AddComponent<LineRenderer>();

        lr.useWorldSpace    = true;
        lr.alignment        = LineAlignment.View;
        lr.textureMode      = LineTextureMode.Tile;
        lr.startWidth       = insulatedWidth;
        lr.endWidth         = insulatedWidth;
        lr.numCapVertices   = 0;
        lr.numCornerVertices = 4;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows   = false;
        lr.material = insulatedMat != null
            ? insulatedMat
            : new Material(Shader.Find("Sprites/Default"));
    }

    void Start()
    {
        if (gravitySource == null)
            gravitySource = FindFirstObjectByType<WireGravity>();

        if (!initialized)
            InitializeBetweenAnchors();

        RebuildColliders();
        UpdateLineRendererNow();
    }

    private void InitializeBetweenAnchors()
    {
        points = Mathf.Max(points, 2);
        RopePoint    = new Vector3[points];
        RopePointOld = new Vector3[points];

        Vector3 startPos = anchor1 != null ? anchor1.position : transform.position;
        Vector3 endPos   = anchor2 != null ? anchor2.position
                                           : transform.position + Vector3.down * (segmentLength * (points - 1));

        for (int i = 0; i < points; i++)
        {
            float   t   = (points == 1) ? 0f : (float)i / (points - 1);
            Vector3 pos = Vector3.Lerp(startPos, endPos, t);
            RopePoint[i]    = pos;
            RopePointOld[i] = pos;
        }
        initialized = true;
    }

    public void InitializeCutPiece(
        Vector3[]  newPoints,
        Vector3[]  newOldPoints,
        bool       newPinStart,
        Transform  newAnchor1,
        bool       newPinEnd,
        Transform  newAnchor2)
    {
        pinStart = newPinStart;
        pinEnd   = newPinEnd;
        anchor1  = newAnchor1;
        anchor2  = newAnchor2;

        points = Mathf.Max(newPoints.Length, 2);
        RopePoint    = (Vector3[])newPoints.Clone();
        RopePointOld = (Vector3[])newOldPoints.Clone();

        initialized = true;

        if (gravitySource == null)
            gravitySource = FindFirstObjectByType<WireGravity>();

        _postCutFrames = POST_CUT_SETTLE_FRAMES;

        RebuildColliders();
        UpdateLineRendererNow();
    }


    private void RebuildColliders()
    {
        if (segmentColliders != null)
        {
            foreach (var c in segmentColliders)
                if (c != null) Destroy(c.gameObject);
        }

        if (!enableCollision) { segmentColliders = null; return; }

        segmentColliders = new CapsuleCollider[Mathf.Max(0, points - 1)];
        for (int i = 0; i < points - 1; i++)
        {
            var col     = new GameObject("WireCollider_" + i);
            col.transform.SetParent(transform, false);
            var capsule = col.AddComponent<CapsuleCollider>();
            capsule.radius    = ropeRadius;
            capsule.direction = 1;
            capsule.height    = segmentLength;
            capsule.isTrigger = false;
            segmentColliders[i] = capsule;
        }
    }

    void UpdateColliders()
    {
        if (!enableCollision || segmentColliders == null || RopePoint == null) return;
        for (int i = 0; i < points - 1; i++)
        {
            if (segmentColliders[i] == null) continue;
            Vector3 p1  = RopePoint[i];
            Vector3 p2  = RopePoint[i + 1];
            Vector3 mid = (p1 + p2) * 0.5f;
            segmentColliders[i].transform.position = mid;
            Vector3 dir = p2 - p1;
            if (dir.sqrMagnitude > 0.000001f)
                segmentColliders[i].transform.rotation = Quaternion.FromToRotation(Vector3.up, dir);
            segmentColliders[i].radius = ropeRadius;
            segmentColliders[i].height = Vector3.Distance(p1, p2) + ropeRadius * 2f;
        }
    }

    private void UpdateLineRendererNow()
    {
        if (lr == null || RopePoint == null) return;
        lr.startWidth    = insulatedWidth;
        lr.endWidth      = insulatedWidth;
        lr.positionCount = RopePoint.Length;
        for (int i = 0; i < RopePoint.Length; i++)
            lr.SetPosition(i, RopePoint[i]);
    }

    private void CheckGravityFlip(Vector3 gravity)
    {
        Vector3 newDir = gravity.normalized;
        float   dot    = Vector3.Dot(_lastGravityDir, newDir);

        if (dot < 0.85f) 
        {
            float killFraction = Mathf.Lerp(0.3f, gravityFlipVelocityKill, Mathf.Clamp01((0.85f - dot) / 1.85f));
            DampenAllVelocities(killFraction);
            _gravityFlipFramesLeft = gravityFlipSettleFrames;
        }

        _lastGravityDir = newDir;
    }

    private void DampenAllVelocities(float killFraction)
    {
        int simStart = pinStart ? 1 : 0;
        int simEnd   = pinEnd   ? points - 1 : points;

        for (int i = simStart; i < simEnd; i++)
        {
            Vector3 vel = RopePoint[i] - RopePointOld[i];
            vel *= (1f - Mathf.Clamp01(killFraction));
            RopePointOld[i] = RopePoint[i] - vel;
        }
    }

    void FixedUpdate()
    {
        if (!initialized || RopePoint == null || RopePoint.Length < 2) return;

        Vector3 gravity = (gravitySource != null) ? gravitySource.Gravity : Physics.gravity;

        CheckGravityFlip(gravity);
        float effectiveDamping = damping;
        if (_gravityFlipFramesLeft > 0)
        {
            effectiveDamping += gravityFlipExtraDamping;
            _gravityFlipFramesLeft--;
        }

        if (_postCutFrames > 0)
        {
            effectiveDamping += 0.12f;
            _postCutFrames--;
        }

        int simStart        = pinStart ? 1 : 0;
        int simEndExclusive = pinEnd   ? points - 1 : points;

        for (int i = simStart; i < simEndExclusive; i++)
        {
            Vector3 vel = (RopePoint[i] - RopePointOld[i]) * (1f - effectiveDamping);
            RopePointOld[i]  = RopePoint[i];
            RopePoint[i]    += vel + gravity * (Time.fixedDeltaTime * Time.fixedDeltaTime);
        }

        PushWireFromPlayer();
        PinEndpoints();

        for (int it = 0; it < iterations; it++)
        {
            PinEndpoints();
            SolveDistanceConstraints();
            SolveCeiling();
            SolveLeftWall();
        }

        for (int b = 0; b < bendPasses; b++)
        {
            SolveBend();
            PinEndpoints();
            SolveCeiling();
            SolveLeftWall();
        }

        for (int i = 0; i < ropeRopePasses; i++)
            SolveRopeRope();

        bool allowSelfCollision =
            selfCollision &&
            _postCutFrames <= 0 &&
            _gravityFlipFramesLeft <= 0 &&
            !(disableSelfCollisionWhenSingleEnded && IsSingleEndedWire());

        if (allowSelfCollision)
        {
            for (int i = 0; i < selfCollisionPasses; i++)
                SolveSelfCollision();
        }

        UpdateLineRendererNow();
        UpdateColliders();
    }


    private void SolveDistanceConstraints()
    {
        for (int i = 0; i < points - 1; i++)
        {
            Vector3 delta    = RopePoint[i + 1] - RopePoint[i];
            float   distance = delta.magnitude;
            if (distance < 0.0001f) continue;

            float difference = (distance - segmentLength) / distance;

            bool pinA = pinStart && i == 0;
            bool pinB = pinEnd   && i + 1 == points - 1;

            if      (pinA && pinB) continue;
            else if (pinA)        RopePoint[i + 1] -= delta * difference;
            else if (pinB)        RopePoint[i]     += delta * difference;
            else
            {
                RopePoint[i]     += delta * difference * 0.3f;
                RopePoint[i + 1] -= delta * difference * 0.3f;
            }
        }
    }

    private void SolveBend()
    {
        if (bendStiffness <= 0f || points < 3) return;

        float maxVelSq = 0f;
        for (int i = 1; i < points - 1; i++)
        {
            float vsq = (RopePoint[i] - RopePointOld[i]).sqrMagnitude;
            if (vsq > maxVelSq) maxVelSq = vsq;
        }
        float stiffnessScale     = 1f - Mathf.Clamp01(maxVelSq / 0.01f);
        float effectiveStiffness = bendStiffness * stiffnessScale;
        if (effectiveStiffness <= 0f) return;

        for (int i = 1; i < points - 1; i++)
        {
            if (IsPinnedPoint(i)) continue;
            Vector3 midpoint = (RopePoint[i - 1] + RopePoint[i + 1]) * 0.5f;
            RopePoint[i] = Vector3.Lerp(RopePoint[i], midpoint, effectiveStiffness);
        }
    }

    private void PinEndpoints()
    {
        if (pinStart && anchor1 != null)
        {
            RopePoint[0]    = anchor1.position;
            RopePointOld[0] = anchor1.position;
        }
        if (pinEnd && anchor2 != null)
        {
            RopePoint[points - 1]    = anchor2.position;
            RopePointOld[points - 1] = anchor2.position;
        }
    }

    private bool IsPinnedPoint(int i) =>
        (pinStart && i == 0) || (pinEnd && i == points - 1);

    private bool IsSingleEndedWire() => pinStart ^ pinEnd;

    private void SolveCeiling()
    {
        if (!ceilingCollision || ceilingBox == null) return;
        float maxY = ceilingBox.bounds.min.y - ropeRadius;
        for (int i = 1; i < points; i++)
        {
            if (RopePoint[i].y > maxY)
            {
                RopePoint[i].y = maxY;
                ApplyCeilingContactVelocity(i);
            }
        }
    }

    private void SolveLeftWall()
    {
        if (!leftWallCollision || leftBox == null) return;
        float minX = leftBox.bounds.max.x + ropeRadius;
        for (int i = 1; i < points; i++)
        {
            if (RopePoint[i].x < minX)
            {
                RopePoint[i].x = minX;
                ApplyLeftWallContactVelocity(i);
            }
        }
    }

    private void ApplyCeilingContactVelocity(int i)
    {
        Vector3 velocity = RopePoint[i] - RopePointOld[i];
        if (velocity.y > 0f) velocity.y = 0f;
        float keep = 1f - contactDamping;
        velocity.x *= keep;
        velocity.z *= keep;
        RopePointOld[i] = velocity.sqrMagnitude < sleepThreshold * sleepThreshold
            ? RopePoint[i]
            : RopePoint[i] - velocity;
    }

    private void ApplyLeftWallContactVelocity(int i)
    {
        Vector3 velocity = RopePoint[i] - RopePointOld[i];
        if (velocity.x < 0f) velocity.x = 0f;
        float keep = 1f - contactDamping;
        velocity.y *= keep;
        velocity.z *= keep;
        RopePointOld[i] = velocity.sqrMagnitude < sleepThreshold * sleepThreshold
            ? RopePoint[i]
            : RopePoint[i] - velocity;
    }

    private void SolveRopeRope()
    {
        if (!ropeRopeCollision) return;
        float minDistance       = ropeRadius * 2f;
        float minSquareDistance = minDistance * minDistance;
        int   myIdx             = MyIndex;

        foreach (VerletWire rope in allwires)
        {
            if (rope == null || rope == this || rope.RopePoint == null) continue;
            if (rope.MyIndex <= myIdx) continue;

            for (int i = 2; i < points; i += Mathf.Max(1, pointSkip))
            {
                Vector3 pointI = RopePoint[i];
                for (int j = 2; j < rope.points; j += Mathf.Max(1, rope.pointSkip))
                {
                    Vector3 pointJ = rope.RopePoint[j];
                    Vector3 diff   = pointI - pointJ;
                    float   sqr    = diff.sqrMagnitude;
                    if (sqr >= minSquareDistance) continue;

                    float   dist   = Mathf.Sqrt(sqr);
                    Vector3 normal = dist < 0.0001f ? Vector3.right : diff / dist;
                    if (dist < 0.0001f) dist = 0.0001f;

                    float push = (minDistance - dist) * 0.5f;
                    RopePoint[i]        += normal * push;
                    rope.RopePoint[j]   -= normal * push;
                    RopePointOld[i]      = RopePoint[i];
                    rope.RopePointOld[j] = rope.RopePoint[j];
                    pointI               = RopePoint[i];
                }
            }
        }
    }

    private void SolveSelfCollision()
    {
        if (!selfCollision) return;
        float minDistance       = ropeRadius * 2f;
        float minSquareDistance = minDistance * minDistance;

        for (int i = 2; i < points; i += Mathf.Max(1, pointSkip))
        {
            Vector3 pointI = RopePoint[i];
            for (int j = i + selfCollisionSkipNear; j < points; j += Mathf.Max(1, pointSkip))
            {
                Vector3 pointJ = RopePoint[j];
                Vector3 diff   = pointI - pointJ;
                float   sqr    = diff.sqrMagnitude;
                if (sqr >= minSquareDistance) continue;

                float   dist   = Mathf.Sqrt(sqr);
                Vector3 normal = dist < 0.0001f ? Vector3.right : diff / dist;
                if (dist < 0.0001f) dist = 0.0001f;

                float push = (minDistance - dist) * 0.5f;
                RopePoint[i]     += normal * push;
                RopePoint[j]     -= normal * push;
                RopePointOld[i]   = RopePoint[i];
                RopePointOld[j]   = RopePoint[j];
                pointI            = RopePoint[i];
            }
        }
    }

    void PushWireFromPlayer()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, 3f, playerLayer);
        foreach (Collider hit in hits)
        {
            for (int i = 1; i < points - 1; i++)
            {
                if ((pinStart && i == 0) || (pinEnd && i == points - 1)) continue;

                Vector3 point = RopePoint[i];
                Vector3 closest = hit.ClosestPoint(point);
                Vector3 delta = point - closest;
                float dist = delta.magnitude;

                float influenceRadius = ropeRadius * 3f;
                if (dist > influenceRadius || dist < 0.0001f) continue;

                Vector3 dir = delta / dist;
                float falloff = 1f - (dist / influenceRadius);
                Vector3 offset = dir * playerPushForce * falloff;

                RopePoint[i] += offset;
                RopePointOld[i] += offset * 0.85f;
                if (i - 1 >= 0)
                {
                    RopePoint[i - 1] += offset * 0.5f;
                    RopePointOld[i - 1] += offset * 0.4f;
                }

                if (i + 1 < points)
                {
                    RopePoint[i + 1] += offset * 0.5f;
                    RopePointOld[i + 1] += offset * 0.4f;
                }

                if (i - 2 >= 0)
                {
                    RopePoint[i - 2] += offset * 0.2f;
                    RopePointOld[i - 2] += offset * 0.15f;
                }

                if (i + 2 < points)
                {
                    RopePoint[i + 2] += offset * 0.2f;
                    RopePointOld[i + 2] += offset * 0.15f;
                }
            }
        }
    }

    public void CutAtWorldPosition(Vector3 worldPos)
    {
        if (hasBeenCut) return;
        if (RopePoint == null || RopePoint.Length < 2) return;
        if (wirePiecePrefab == null) { Debug.LogWarning($"{name}: wirePiecePrefab missing."); return; }

        int minSeg = Mathf.Max(0, minPointsPerPiece - 2);
        int maxSeg = Mathf.Min(points - 2, points - minPointsPerPiece);
        if (maxSeg < minSeg) { Debug.LogWarning($"{name}: not enough points to cut."); return; }

        int segIdx = Mathf.Clamp(FindClosestSegment(worldPos), minSeg, maxSeg);

        Vector3 a  = RopePoint[segIdx];
        Vector3 b  = RopePoint[segIdx + 1];
        Vector3 ab = b - a;
        float   t  = ab.sqrMagnitude > 0.000001f
                     ? Vector3.Dot(worldPos - a, ab) / ab.sqrMagnitude
                     : 0.5f;
        t = Mathf.Clamp(t, 0.25f, 0.75f);

        CreateCutPieces(segIdx, a + ab * t);
    }

    private int FindClosestSegment(Vector3 worldPos)
    {
        int   bestIndex = 0;
        float bestSqr   = float.MaxValue;
        for (int i = 0; i < points - 1; i++)
        {
            float sqr = (worldPos - ClosestPointOnSegment(RopePoint[i], RopePoint[i+1], worldPos)).sqrMagnitude;
            if (sqr < bestSqr) { bestSqr = sqr; bestIndex = i; }
        }
        return bestIndex;
    }

    private Vector3 ClosestPointOnSegment(Vector3 a, Vector3 b, Vector3 p)
    {
        Vector3 ab    = b - a;
        float   lenSq = ab.sqrMagnitude;
        if (lenSq < 0.000001f) return a;
        return a + ab * Mathf.Clamp01(Vector3.Dot(p - a, ab) / lenSq);
    }

    private void CreateCutPieces(int segIdx, Vector3 cutPos)
    {
        hasBeenCut = true;

        Vector3 tangent = RopePoint[segIdx + 1] - RopePoint[segIdx];
        if (tangent.sqrMagnitude < 0.000001f) tangent = Vector3.right;
        else tangent.Normalize();

        float   gap          = ropeRadius * 0.35f;
        Vector3 leftCutPos   = cutPos - tangent * gap;
        Vector3 rightCutPos  = cutPos + tangent * gap;

        int      leftCount  = segIdx + 2;
        Vector3[] leftPts   = new Vector3[leftCount];
        Vector3[] leftOld   = new Vector3[leftCount];
        for (int i = 0; i <= segIdx; i++) { leftPts[i] = RopePoint[i]; leftOld[i] = RopePointOld[i]; }
        leftPts[leftCount - 1] = leftOld[leftCount - 1] = leftCutPos;

        int      rightCount = points - segIdx;
        Vector3[] rightPts  = new Vector3[rightCount];
        Vector3[] rightOld  = new Vector3[rightCount];
        rightPts[0] = rightOld[0] = rightCutPos;
        for (int src = segIdx + 1, dst = 1; src < points; src++, dst++)
        { rightPts[dst] = RopePoint[src]; rightOld[dst] = RopePointOld[src]; }

        VerletWire leftWire = Instantiate(wirePiecePrefab, transform.position, transform.rotation);
        CopySettingsTo(leftWire);
        leftWire.InitializeCutPiece(leftPts, leftOld, true, anchor1, false, null);
        leftWire.bendStiffness = cutPieceBendStiffness;
        leftWire.damping       = cutPieceDamping;
        leftWire.ApplyCutImpulse(freeEndIsLast: true);
        leftWire.RunSettlePass();  

        VerletWire rightWire = Instantiate(wirePiecePrefab, transform.position, transform.rotation);
        CopySettingsTo(rightWire);
        rightWire.InitializeCutPiece(rightPts, rightOld, false, null, true, anchor2);
        rightWire.bendStiffness = cutPieceBendStiffness;
        rightWire.damping       = cutPieceDamping;
        rightWire.ApplyCutImpulse(freeEndIsLast: false);
        rightWire.RunSettlePass(); 

        Destroy(gameObject);
    }

    private void CopySettingsTo(VerletWire other)
    {
        other.gravitySource         = gravitySource;
        other.ceilingCollision      = ceilingCollision;
        other.ropeRadius            = ropeRadius;
        other.ceilingBox            = ceilingBox;
        other.leftWallCollision     = leftWallCollision;
        other.leftBox               = leftBox;
        other.segmentLength         = segmentLength;
        other.damping               = damping;
        other.iterations            = iterations;
        other.insulatedWidth        = insulatedWidth;
        other.insulatedMat          = insulatedMat;
        other.ropeRopeCollision     = ropeRopeCollision;
        other.ropeRopePasses        = ropeRopePasses;
        other.pointSkip             = pointSkip;
        other.selfCollision         = selfCollision;
        other.selfCollisionSkipNear = selfCollisionSkipNear;
        other.selfCollisionPasses   = selfCollisionPasses;
        other.enableCollision       = enableCollision;
        other.playerPushForce       = playerPushForce;
        other.playerLayer           = playerLayer;
        other.wirePiecePrefab       = wirePiecePrefab;
        other.cutSnapImpulse        = cutSnapImpulse;
        other.cutImpulsePoints      = cutImpulsePoints;
        other.minPointsPerPiece     = minPointsPerPiece;
        other.gravityFlipVelocityKill  = gravityFlipVelocityKill;
        other.gravityFlipSettleFrames  = gravityFlipSettleFrames;
        other.gravityFlipExtraDamping  = gravityFlipExtraDamping;
        other.cutImpulseScale          = cutImpulseScale;
        other.cutSettleIterations      = cutSettleIterations;

        other.lr.startWidth = insulatedWidth;
        other.lr.endWidth   = insulatedWidth;
        if (insulatedMat != null) other.lr.material = insulatedMat;
    }

    private void ApplyCutImpulse(bool freeEndIsLast)
    {
        int freeIndex = freeEndIsLast ? points - 1 : 0;

        for (int n = 0; n < cutImpulsePoints; n++)
        {
            int i = freeEndIsLast ? freeIndex - n : freeIndex + n;
            if (i < 0 || i >= points) break;

            Vector3 dir;
            if (freeEndIsLast)
            {
                int prev = Mathf.Max(i - 1, 0);
                dir = RopePoint[i] - RopePoint[prev];
            }
            else
            {
                int next = Mathf.Min(i + 1, points - 1);
                dir = RopePoint[i] - RopePoint[next];
            }

            dir = dir.sqrMagnitude < 0.000001f ? Vector3.up : dir.normalized;

            float falloff = 1f - (n / Mathf.Max(1f, (float)cutImpulsePoints));
            float amount  = cutSnapImpulse * cutImpulseScale * falloff;

            RopePoint[i]    += dir * amount;
            RopePointOld[i]  = RopePoint[i] - dir * amount;
        }
    }

    private void RunSettlePass()
    {
        for (int it = 0; it < cutSettleIterations; it++)
        {
            PinEndpoints();
            SolveDistanceConstraints();
        }
        UpdateLineRendererNow();
    }
}