using UnityEngine;
using System.Collections.Generic;

public class VerletWire : MonoBehaviour
{
    private static readonly List<VerletWire> allwires = new();

    private const int POST_CUT_SETTLE_FRAMES = 30;
    private const int IGNORE_COLLISION_FRAMES = 20;
    private const float EPS = 0.000001f;

    [Header("Cut Piece Feel")]
    [Range(0f, 0.5f)] public float cutPieceBendStiffness = 0.025f;
    [Range(0f, 0.2f)] public float cutPieceDamping = 0.012f;
    [Range(0, 20)] public int cutSettleIterations = 12;
    [Range(0f, 1f)] public float cutImpulseScale = 0.35f;

    [Header("Gravity")]
    public bool useDynamicGravity = true;
    public bool ignoreGravity = false;
    public Vector3 customGravity = new(0, 9.81f, 0);
    public static bool HasAnyWireBeenCut { get; private set; } = false;

    public enum WireColor { Red, Blue, Grey, Brown }

    [Header("Double-Pinned Stabilization")]
    [Range(0f, 1f)] public float doublePinnedFlipGravityScale = 0.08f;
    [Range(0f, 1f)] public float doublePinnedPerpVelocityDamping = 0.85f;
    [Range(0f, 1f)] public float doublePinnedAlongVelocityDamping = 0.15f;
    [Range(0f, 1f)] public float doublePinnedSlackThreshold = 0.03f;
    private bool IsDoublePinnedWire() =>
        pinStart && pinEnd && anchor1 != null && anchor2 != null;

    [Header("Wire Identity")]
    public WireColor wireColor;
    public static WireColor? CutWireColor { get; set; }

    [Header("Anchors")]
    public Transform anchor1;
    public Transform anchor2;
    public WireGravity gravitySource;

    [Header("Pinning")]
    public bool pinStart = true;
    public bool pinEnd = true;

    [Header("Ceiling Collision")]
    public bool ceilingCollision = true;
    public float ropeRadius = 0.25f;
    public BoxCollider ceilingBox;

    [Header("Left Wall Collision")]
    public bool leftWallCollision = true;
    public BoxCollider leftBox;

    [Header("Rope")]
    public int points = 35;
    public float segmentLength = 0.4f;

    [Range(0f, 0.2f)] public float damping = 0.02f;
    [Range(1, 30)] public int iterations = 18;

    [Header("Wire Look")]
    public float insulatedWidth = 0.12f;
    public Material insulatedMat;

    [Header("Rope-Rope Collision")]
    public bool ropeRopeCollision = true;
    [Range(0, 4)] public int ropeRopePasses = 1;
    public int pointSkip = 1;
    [Range(0f, 1f)] public float ropeRopeFriction = 0.3f;
    [Range(0f, 0.5f)] public float ropeRopeSlop = 0.02f;

    [Header("Self Collision")]
    public bool selfCollision = true;
    [Range(2, 20)] public int selfCollisionSkipNear = 6;
    [Range(0, 4)] public int selfCollisionPasses = 1;

    [Header("Collision")]
    public bool enableCollision = true;

    [Header("Player Interaction")]
    public float playerPushForce = 0.05f;
    public LayerMask playerLayer;

    [Header("Cutting")]
    public VerletWire wirePiecePrefab;
    public float cutSnapImpulse = 0.6f;
    public int cutImpulsePoints = 4;
    public int minPointsPerPiece = 4;

    [Header("Stability")]
    [Range(0f, 0.5f)] public float bendStiffness = 0.18f;
    [Range(0, 4)] public int bendPasses = 2;
    [Range(0f, 1f)] public float contactDamping = 0.85f;
    public float sleepThreshold = 0.01f;
    public bool disableSelfCollisionWhenSingleEnded = true;

    [Header("Gravity Flip")]
    [Range(0f, 1f)] public float gravityFlipVelocityKill = 0.92f;
    [Range(1, 60)] public int gravityFlipSettleFrames = 20;
    [Range(0f, 0.3f)] public float gravityFlipExtraDamping = 0.18f;

    // FIX: Minimum stiffness floors so the wire never goes completely limp during a flip,
    // which caused the buckled/folded look mid-fall.
    [Header("Gravity Flip Stiffness Floors")]
    [Range(0f, 0.1f)] public float gravityFlipMinBendStiffness = 0.04f;
    [Range(0f, 0.05f)] public float gravityFlipMinSlackStiffness = 0.015f;

    [System.NonSerialized] public bool initialized = false;
    [System.NonSerialized] public VerletWire ignoreCollisionWith = null;

    private Vector3 _lastGravityDir = Vector3.down;
    private int _postCutFrames = 0;
    private int _gravityFlipFramesLeft = 0;
    private int _ignoreCollisionFrames = 0;
    private bool hasBeenCut = false;

    private LineRenderer lr;
    private Vector3[] RopePoint;
    private Vector3[] RopePointOld;
    private CapsuleCollider[] segmentColliders;

    private int MyIndex => allwires.IndexOf(this);
    public int PointCount => RopePoint != null ? RopePoint.Length : 0;

    void OnEnable()
    {
        if (!allwires.Contains(this)) allwires.Add(this);
    }

    void OnDisable()
    {
        allwires.Remove(this);
    }

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        if (lr == null) lr = gameObject.AddComponent<LineRenderer>();
        SetupLineRenderer();
    }

    void Start()
    {
        if (gravitySource == null)
            gravitySource = FindFirstObjectByType<WireGravity>();

        if (anchor1 == null || anchor2 == null) return;

        if (!initialized) InitializeBetweenAnchors();

        RebuildColliders();
        UpdateLineRendererNow();
    }

    void FixedUpdate()
    {
        if (!initialized || RopePoint == null || RopePoint.Length < 2) return;

        TickIgnoreCollision();
        Vector3 gravity = ResolveGravity();
        CheckGravityFlip(gravity);

        Vector3 simGravity = gravity;

        if (IsDoublePinnedWire() &&
            _gravityFlipFramesLeft > 0 &&
            GetSlackRatio() > doublePinnedSlackThreshold)
        {
            simGravity *= doublePinnedFlipGravityScale;
        }

        SimulateVerlet(simGravity, ResolveEffectiveDamping());
        StabilizeDoublePinnedAfterFlip();

        PushWireFromPlayer();
        PinEndpoints();

        for (int it = 0; it < iterations; it++)
        {
            PinEndpoints();
            SolveDistanceConstraints();
            SolveCeiling();
            SolveLeftWall();
            SolveRopeRope();
        }
        for (int b = 0; b < bendPasses; b++)
        {
            SolveBend();
            SolveSlackDistribution();
            PinEndpoints();
            SolveCeiling();
            SolveLeftWall();
        }
        for (int i = 0; i < ropeRopePasses; i++)
            SolveRopeRope();

        if (AllowSelfCollision())
        {
            for (int i = 0; i < selfCollisionPasses; i++)
                SolveSelfCollision();
        }

        UpdateLineRendererNow();
        UpdateColliders();
    }

    public Vector3 GetPoint(int i)
    {
        if (RopePoint == null || RopePoint.Length == 0) return Vector3.zero;
        return RopePoint[Mathf.Clamp(i, 0, RopePoint.Length - 1)];
    }

    private void StabilizeDoublePinnedAfterFlip()
    {
        if (!IsDoublePinnedWire()) return;
        if (_gravityFlipFramesLeft <= 0) return;
        if (GetSlackRatio() <= doublePinnedSlackThreshold) return;

        Vector3 chord = RopePoint[points - 1] - RopePoint[0];
        float chordLenSq = chord.sqrMagnitude;
        if (chordLenSq < EPS) return;

        Vector3 chordDir = chord / Mathf.Sqrt(chordLenSq);

        for (int i = 1; i < points - 1; i++)
        {
            if (IsPinnedPoint(i)) continue;

            Vector3 vel = RopePoint[i] - RopePointOld[i];

            Vector3 along = Vector3.Dot(vel, chordDir) * chordDir;
            Vector3 perp = vel - along;

            along *= (1f - doublePinnedAlongVelocityDamping);
            perp *= (1f - doublePinnedPerpVelocityDamping);

            Vector3 newVel = along + perp;
            RopePointOld[i] = RopePoint[i] - newVel;
        }
    }

    private void SetupLineRenderer()
    {
        lr.useWorldSpace = true;
        lr.alignment = LineAlignment.View;
        lr.textureMode = LineTextureMode.Tile;
        lr.startWidth = insulatedWidth;
        lr.endWidth = insulatedWidth;
        lr.numCapVertices = 0;
        lr.numCornerVertices = 4;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;
        lr.material = insulatedMat != null ? insulatedMat : new Material(Shader.Find("Sprites/Default"));
    }

    // FIX #2: Added minimum stiffness floor during gravity flip to prevent the wire
    // going fully limp, which caused slack bunching and post-flip oscillation.
    private void SolveSlackDistribution()
    {
        if (points < 3) return;

        float slackRatio = GetSlackRatio();
        if (slackRatio <= 0.001f) return;

        float stiffness = bendStiffness * Mathf.Lerp(0.15f, 0.45f, slackRatio);

        if (_gravityFlipFramesLeft > 0)
            stiffness = Mathf.Max(stiffness * 0.5f, gravityFlipMinSlackStiffness);

        if (stiffness <= 0f) return;

        for (int i = 0; i < points - 2; i++)
        {
            int a = i;
            int c = i + 2;

            Vector3 delta = RopePoint[c] - RopePoint[a];
            float dist = delta.magnitude;
            if (dist < 0.0001f) continue;

            float target = segmentLength * 2f;
            float diff = (dist - target) / dist;

            bool pinA = IsPinnedPoint(a);
            bool pinC = IsPinnedPoint(c);

            if (pinA && pinC) continue;

            Vector3 correction = delta * diff * stiffness * 0.5f;

            if (!pinA) RopePoint[a] += correction;
            if (!pinC) RopePoint[c] -= correction;
        }
    }

    private void InitializeBetweenAnchors()
    {
        points = Mathf.Max(points, 2);
        RopePoint = new Vector3[points];
        RopePointOld = new Vector3[points];

        Vector3 startPos = anchor1 != null ? anchor1.position : transform.position;
        Vector3 endPos = anchor2 != null
            ? anchor2.position
            : transform.position + Vector3.down * (segmentLength * (points - 1));

        for (int i = 0; i < points; i++)
        {
            float t = points == 1 ? 0f : (float)i / (points - 1);
            Vector3 pos = Vector3.Lerp(startPos, endPos, t);
            RopePoint[i] = pos;
            RopePointOld[i] = pos;
        }

        initialized = true;
    }

    private float GetSlackRatio()
    {
        if (RopePoint == null || RopePoint.Length < 2) return 0f;

        float ropeLength = segmentLength * (points - 1);
        float anchorDistance = Vector3.Distance(RopePoint[0], RopePoint[points - 1]);

        if (ropeLength < EPS) return 0f;

        float slack = ropeLength - anchorDistance;
        return Mathf.Clamp01(slack / ropeLength);
    }

    public void InitializeCutPiece(
        Vector3[] newPoints,
        Vector3[] newOldPoints,
        bool newPinStart,
        Transform newAnchor1,
        bool newPinEnd,
        Transform newAnchor2)
    {
        pinStart = newPinStart;
        pinEnd = newAnchor2 != null ? newPinEnd : false;
        anchor1 = newAnchor1;
        anchor2 = newAnchor2;

        points = Mathf.Max(newPoints.Length, 2);
        RopePoint = (Vector3[])newPoints.Clone();
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

        if (!enableCollision)
        {
            segmentColliders = null;
            return;
        }

        segmentColliders = new CapsuleCollider[Mathf.Max(0, points - 1)];
        for (int i = 0; i < points - 1; i++)
        {
            var go = new GameObject("WireCollider_" + i);
            go.transform.SetParent(transform, false);

            var capsule = go.AddComponent<CapsuleCollider>();
            capsule.radius = ropeRadius;
            capsule.direction = 1;
            capsule.height = segmentLength;
            capsule.isTrigger = false;
            segmentColliders[i] = capsule;
        }
    }

    private void UpdateColliders()
    {
        if (!enableCollision || segmentColliders == null || RopePoint == null) return;

        for (int i = 0; i < points - 1; i++)
        {
            if (segmentColliders[i] == null) continue;

            Vector3 p1 = RopePoint[i];
            Vector3 p2 = RopePoint[i + 1];
            Vector3 dir = p2 - p1;

            segmentColliders[i].transform.position = (p1 + p2) * 0.5f;
            if (dir.sqrMagnitude > EPS)
                segmentColliders[i].transform.rotation = Quaternion.FromToRotation(Vector3.up, dir);

            segmentColliders[i].radius = ropeRadius;
            segmentColliders[i].height = Vector3.Distance(p1, p2) + ropeRadius * 2f;
        }
    }

    private void UpdateLineRendererNow()
    {
        if (lr == null || RopePoint == null) return;

        lr.startWidth = insulatedWidth;
        lr.endWidth = insulatedWidth;
        lr.positionCount = RopePoint.Length;
        for (int i = 0; i < RopePoint.Length; i++)
            lr.SetPosition(i, RopePoint[i]);
    }

    private void TickIgnoreCollision()
    {
        if (_ignoreCollisionFrames > 0) _ignoreCollisionFrames--;
        else ignoreCollisionWith = null;
    }

    private Vector3 ResolveGravity()
    {
        if (ignoreGravity) return Vector3.zero;
        if (useDynamicGravity) return gravitySource != null ? gravitySource.Gravity : Physics.gravity;
        return customGravity;
    }

    // FIX #3: On the very last frame of the flip settle window, do a hard velocity drain
    // to break the oscillation loop that causes post-flip wobbling. The wobble happens
    // because SolveBend keeps pushing points toward midpoints, but those midpoints shift
    // every frame when the wire has more length than the anchor distance allows — this
    // hard kill interrupts that cycle right as normal simulation resumes.
    private float ResolveEffectiveDamping()
    {
        float effective = damping;

        if (_gravityFlipFramesLeft > 0)
        {
            effective += gravityFlipExtraDamping;
            _gravityFlipFramesLeft--;

            // Last frame of the flip settle period: hard-kill residual velocity
            if (_gravityFlipFramesLeft == 0)
                DampenAllVelocities(0.7f);
        }

        if (_postCutFrames > 0)
        {
            effective += 0.12f;
            _postCutFrames--;
        }

        return effective;
    }

    private void CheckGravityFlip(Vector3 gravity)
    {
        Vector3 newDir = gravity.normalized;
        float dot = Vector3.Dot(_lastGravityDir, newDir);

        if (dot < 0.85f)
        {
            float killFraction = Mathf.Lerp(0.3f, gravityFlipVelocityKill,
                Mathf.Clamp01((0.85f - dot) / 1.85f));
            DampenAllVelocities(killFraction);
            _gravityFlipFramesLeft = gravityFlipSettleFrames;
        }

        _lastGravityDir = newDir;
    }

    private void DampenAllVelocities(float killFraction)
    {
        IterateSimPoints(i =>
        {
            Vector3 vel = RopePoint[i] - RopePointOld[i];
            vel *= 1f - Mathf.Clamp01(killFraction);
            RopePointOld[i] = RopePoint[i] - vel;
        });
    }

    private void SimulateVerlet(Vector3 gravity, float effectiveDamping)
    {
        float dt2 = Time.fixedDeltaTime * Time.fixedDeltaTime;
        IterateSimPoints(i =>
        {
            Vector3 vel = (RopePoint[i] - RopePointOld[i]) * (1f - effectiveDamping);
            RopePointOld[i] = RopePoint[i];
            RopePoint[i] += vel + gravity * dt2;
        });
    }

    private void IterateSimPoints(System.Action<int> action)
    {
        int start = pinStart ? 1 : 0;
        int end = pinEnd ? points - 1 : points;
        for (int i = start; i < end; i++) action(i);
    }

    private void SolveDistanceConstraints()
    {
        for (int i = 0; i < points - 1; i++)
            SolveSegmentDistance(i, i + 1, segmentLength);
    }

    private void SolveSegmentDistance(int aIndex, int bIndex, float targetLength)
    {
        Vector3 delta = RopePoint[bIndex] - RopePoint[aIndex];
        float distance = delta.magnitude;
        if (distance < 0.0001f) return;

        float difference = (distance - targetLength) / distance;
        bool pinA = IsPinnedPoint(aIndex);
        bool pinB = IsPinnedPoint(bIndex);

        if (pinA && pinB) return;
        if (pinA) RopePoint[bIndex] -= delta * difference;
        else if (pinB) RopePoint[aIndex] += delta * difference;
        else
        {
            Vector3 correction = delta * difference * 0.5f;
            RopePoint[aIndex] += correction;
            RopePoint[bIndex] -= correction;
        }
    }

    // FIX #1: Added minimum stiffness floor during gravity flip. Previously the stiffness
    // was multiplied by 0.05, which could bring it to near zero, leaving the wire
    // completely unconstrained mid-fall and causing the broken/folded appearance.
    private void SolveBend()
    {
        if (bendStiffness <= 0f || points < 3) return;

        float slackRatio = GetSlackRatio();

        float maxVelSq = 0f;
        for (int i = 1; i < points - 1; i++)
        {
            float vsq = (RopePoint[i] - RopePointOld[i]).sqrMagnitude;
            if (vsq > maxVelSq) maxVelSq = vsq;
        }

        float stiffnessScale = 1f - Mathf.Clamp01(maxVelSq / 0.01f);
        float effectiveStiffness = bendStiffness * stiffnessScale;

        effectiveStiffness *= Mathf.Lerp(1f, 0.2f, slackRatio);

        if (_gravityFlipFramesLeft > 0)
            effectiveStiffness = Mathf.Max(effectiveStiffness * 0.05f, gravityFlipMinBendStiffness);

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
            RopePoint[0] = anchor1.position;
            RopePointOld[0] = anchor1.position;
        }

        if (pinEnd && anchor2 != null)
        {
            RopePoint[points - 1] = anchor2.position;
            RopePointOld[points - 1] = anchor2.position;
        }
    }

    private bool IsPinnedPoint(int i) =>
        (pinStart && i == 0) || (pinEnd && i == points - 1);

    private bool IsSingleEndedWire() => pinStart ^ pinEnd;

    private bool AllowSelfCollision() =>
        selfCollision &&
        _postCutFrames <= 0 &&
        _gravityFlipFramesLeft <= 0 &&
        !(disableSelfCollisionWhenSingleEnded && IsSingleEndedWire());

    private void SolveCeiling()
    {
        if (!ceilingCollision || ceilingBox == null) return;
        float maxY = ceilingBox.bounds.min.y - ropeRadius;

        for (int i = 1; i < points; i++)
        {
            if (RopePoint[i].y > maxY)
            {
                RopePoint[i].y = maxY;
                ApplyContactVelocity(i, Vector3.up, clampPositiveNormal: true);
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
                ApplyContactVelocity(i, Vector3.right, clampPositiveNormal: false);
            }
        }
    }

    private void ApplyContactVelocity(int i, Vector3 normalAxis, bool clampPositiveNormal)
    {
        Vector3 velocity = RopePoint[i] - RopePointOld[i];
        float axis = Vector3.Dot(velocity, normalAxis);

        if (clampPositiveNormal)
        {
            if (axis > 0f) velocity -= normalAxis * axis;
        }
        else
        {
            if (axis < 0f) velocity -= normalAxis * axis;
        }

        float keep = 1f - contactDamping;
        if (normalAxis == Vector3.up)
        {
            velocity.x *= keep;
            velocity.z *= keep;
        }
        else
        {
            velocity.y *= keep;
            velocity.z *= keep;
        }

        RopePointOld[i] = velocity.sqrMagnitude < sleepThreshold * sleepThreshold
            ? RopePoint[i]
            : RopePoint[i] - velocity;
    }

    private void SolveRopeRope()
    {
        if (!ropeRopeCollision || RopePoint == null || points < 2) return;

        float minDist = ropeRadius * 2f;
        float minDistSq = minDist * minDist;
        int myIdx = MyIndex;

        foreach (VerletWire other in allwires)
        {
            if (other == null || other == this || other.RopePoint == null || other.points < 2) continue;
            if (other.MyIndex <= myIdx) continue;
            if (ignoreCollisionWith == other) continue;
            if (other.ignoreCollisionWith == this) continue;

            for (int i = 0; i < points - 1; i++)
            {
                if (IsPinnedPoint(i) && IsPinnedPoint(i + 1)) continue;

                Vector3 a0 = RopePoint[i];
                Vector3 a1 = RopePoint[i + 1];

                for (int j = 0; j < other.points - 1; j++)
                {
                    if (other.IsPinnedPoint(j) && other.IsPinnedPoint(j + 1)) continue;

                    Vector3 b0 = other.RopePoint[j];
                    Vector3 b1 = other.RopePoint[j + 1];

                    float s, t;
                    Vector3 cA, cB;
                    float sqr = ClosestPointSegmentSegment(a0, a1, b0, b1, out s, out t, out cA, out cB);
                    if (sqr >= minDistSq) continue;

                    Vector3 delta = cA - cB;
                    float dist = Mathf.Sqrt(Mathf.Max(sqr, 1e-8f));
                    Vector3 normal = dist > 1e-5f ? delta / dist : FallbackNormal(a1 - a0);

                    float penetration = (minDist - dist) + ropeRopeSlop;
                    if (penetration <= 0f) continue;

                    Vector3 correction = normal * (penetration * 0.5f);
                    ApplyPointCorrection(this, i, i + 1, s, correction);
                    ApplyPointCorrection(other, j, j + 1, t, -correction);

                    Vector3 velA = Vector3.Lerp(GetPointVelocity(this, i), GetPointVelocity(this, i + 1), s);
                    Vector3 velB = Vector3.Lerp(GetPointVelocity(other, j), GetPointVelocity(other, j + 1), t);

                    RemoveInwardNormalVelocity(this, i, i + 1, s, normal, velB, ropeRopeFriction);
                    RemoveInwardNormalVelocity(other, j, j + 1, t, -normal, velA, other.ropeRopeFriction);

                    a0 = RopePoint[i];
                    a1 = RopePoint[i + 1];
                }
            }
        }
    }

    private static Vector3 FallbackNormal(Vector3 segDir)
    {
        if (segDir.sqrMagnitude < 1e-6f) segDir = Vector3.right;
        Vector3 normal = Vector3.Cross(segDir.normalized, Vector3.forward);
        if (normal.sqrMagnitude < 1e-6f)
            normal = Vector3.Cross(segDir.normalized, Vector3.up);
        return normal.normalized;
    }

    private void SolveSelfCollision()
    {
        if (!selfCollision) return;

        float minDist = ropeRadius * 2f;
        float minDistSq = minDist * minDist;
        int step = Mathf.Max(1, pointSkip);

        for (int i = 0; i < points; i += step)
        {
            if (IsPinnedPoint(i)) continue;
            Vector3 pI = RopePoint[i];

            for (int j = i + selfCollisionSkipNear; j < points; j += step)
            {
                if (IsPinnedPoint(j)) continue;

                Vector3 pJ = RopePoint[j];
                Vector3 diff = pI - pJ;
                float sqr = diff.sqrMagnitude;
                if (sqr >= minDistSq || sqr < EPS) continue;

                float dist = Mathf.Sqrt(sqr);
                Vector3 normal = diff / dist;
                float half = (minDist - dist) * 0.5f;

                RopePoint[i] += normal * half;
                RopePoint[j] -= normal * half;

                Vector3 velI = RopePoint[i] - RopePointOld[i];
                Vector3 velJ = RopePoint[j] - RopePointOld[j];
                float relVel = Vector3.Dot(velI - velJ, normal);

                if (relVel < 0f)
                {
                    Vector3 impulse = normal * relVel * 0.5f;
                    RopePointOld[i] += impulse;
                    RopePointOld[j] -= impulse;
                }

                pI = RopePoint[i];
            }
        }
    }

    private void PushWireFromPlayer()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, 3f, playerLayer);
        foreach (Collider hit in hits)
        {
            for (int i = 1; i < points - 1; i++)
            {
                if (IsPinnedPoint(i)) continue;

                Vector3 point = RopePoint[i];
                Vector3 closest = hit.ClosestPoint(point);
                Vector3 delta = point - closest;
                float dist = delta.magnitude;

                float influenceRadius = ropeRadius * 3f;
                if (dist > influenceRadius || dist < 0.0001f) continue;

                Vector3 dir = delta / dist;
                float falloff = 1f - (dist / influenceRadius);
                Vector3 offset = dir * playerPushForce * falloff;

                ApplyOffset(i, offset, 1f, 0.85f);
                ApplyOffset(i - 1, offset, 0.5f, 0.4f);
                ApplyOffset(i + 1, offset, 0.5f, 0.4f);
                ApplyOffset(i - 2, offset, 0.2f, 0.15f);
                ApplyOffset(i + 2, offset, 0.2f, 0.15f);
            }
        }
    }

    private void ApplyOffset(int index, Vector3 offset, float pointMul, float oldMul)
    {
        if (index < 0 || index >= points) return;
        RopePoint[index] += offset * pointMul;
        RopePointOld[index] += offset * oldMul;
    }

    public void CutAtWorldPosition(Vector3 worldPos)
    {
        if (hasBeenCut || HasAnyWireBeenCut) return;

        HasAnyWireBeenCut = true;
        CutWireColor = wireColor;
        Debug.Log("Cut wire color stored: " + wireColor);

        if (RopePoint == null || RopePoint.Length < 2) return;
        if (wirePiecePrefab == null)
        {
            Debug.LogWarning($"{name}: wirePiecePrefab missing.");
            return;
        }

        int minSeg = Mathf.Max(0, minPointsPerPiece - 2);
        int maxSeg = Mathf.Min(points - 2, points - minPointsPerPiece);
        if (maxSeg < minSeg)
        {
            Debug.LogWarning($"{name}: not enough points to cut.");
            return;
        }

        int segIdx = Mathf.Clamp(FindClosestSegment(worldPos), minSeg, maxSeg);

        Vector3 a = RopePoint[segIdx];
        Vector3 b = RopePoint[segIdx + 1];
        Vector3 ab = b - a;
        float t = ab.sqrMagnitude > EPS ? Vector3.Dot(worldPos - a, ab) / ab.sqrMagnitude : 0.5f;
        t = Mathf.Clamp(t, 0.25f, 0.75f);

        CreateCutPieces(segIdx, a + ab * t);
    }

    private int FindClosestSegment(Vector3 worldPos)
    {
        int bestIndex = 0;
        float bestSqr = float.MaxValue;

        for (int i = 0; i < points - 1; i++)
        {
            float sqr = (worldPos - ClosestPointOnSegment(RopePoint[i], RopePoint[i + 1], worldPos)).sqrMagnitude;
            if (sqr < bestSqr)
            {
                bestSqr = sqr;
                bestIndex = i;
            }
        }

        return bestIndex;
    }

    private static Vector3 ClosestPointOnSegment(Vector3 a, Vector3 b, Vector3 p)
    {
        Vector3 ab = b - a;
        float lenSq = ab.sqrMagnitude;
        if (lenSq < EPS) return a;
        return a + ab * Mathf.Clamp01(Vector3.Dot(p - a, ab) / lenSq);
    }

    private void CreateCutPieces(int segIdx, Vector3 cutPos)
    {
        hasBeenCut = true;

        Vector3 tangent = RopePoint[segIdx + 1] - RopePoint[segIdx];
        tangent = tangent.sqrMagnitude < EPS ? Vector3.right : tangent.normalized;

        float gap = ropeRadius * 0.06f;
        Vector3 leftCutPos = cutPos - tangent * gap;
        Vector3 rightCutPos = cutPos + tangent * gap;

        Vector3 cutVel = Vector3.Lerp(
            RopePoint[segIdx] - RopePointOld[segIdx],
            RopePoint[segIdx + 1] - RopePointOld[segIdx + 1],
            0.5f);

        int leftCount = segIdx + 2;
        int rightCount = points - segIdx;

        Vector3[] leftPts = new Vector3[leftCount];
        Vector3[] leftOld = new Vector3[leftCount];
        Vector3[] rightPts = new Vector3[rightCount];
        Vector3[] rightOld = new Vector3[rightCount];

        for (int i = 0; i <= segIdx; i++)
        {
            leftPts[i] = RopePoint[i];
            leftOld[i] = RopePointOld[i];
        }

        leftPts[leftCount - 1] = leftCutPos;
        leftOld[leftCount - 1] = leftCutPos - cutVel;

        rightPts[0] = rightCutPos;
        rightOld[0] = rightCutPos - cutVel;

        for (int src = segIdx + 1, dst = 1; src < points; src++, dst++)
        {
            rightPts[dst] = RopePoint[src];
            rightOld[dst] = RopePointOld[src];
        }

        VerletWire leftWire = SpawnCutPiece(leftPts, leftOld, true, anchor1, false, null, true);
        VerletWire rightWire = SpawnCutPiece(rightPts, rightOld, false, null, true, anchor2, false);

        IgnoreCollisionPair(leftWire, rightWire);

        foreach (VerletWire w in allwires)
        {
            if (w == null || w == this || w == leftWire || w == rightWire) continue;
            w.ignoreCollisionWith = leftWire;
            w._ignoreCollisionFrames = IGNORE_COLLISION_FRAMES;
        }

        Destroy(gameObject);
    }

    private VerletWire SpawnCutPiece(
        Vector3[] pts,
        Vector3[] oldPts,
        bool newPinStart,
        Transform newAnchor1,
        bool newPinEnd,
        Transform newAnchor2,
        bool freeEndIsLast)
    {
        VerletWire wire = Instantiate(wirePiecePrefab, transform.position, transform.rotation);
        CopySettingsTo(wire);
        wire.InitializeCutPiece(pts, oldPts, newPinStart, newAnchor1, newPinEnd, newAnchor2);
        wire.bendStiffness = cutPieceBendStiffness;
        wire.damping = cutPieceDamping;
        wire.ApplyCutImpulse(freeEndIsLast);
        wire.RunSettlePass();
        return wire;
    }

    private static void IgnoreCollisionPair(VerletWire a, VerletWire b)
    {
        a.ignoreCollisionWith = b;
        b.ignoreCollisionWith = a;
        a._ignoreCollisionFrames = IGNORE_COLLISION_FRAMES;
        b._ignoreCollisionFrames = IGNORE_COLLISION_FRAMES;
    }

    private void CopySettingsTo(VerletWire other)
    {
        other.gravitySource = gravitySource;
        other.ceilingCollision = ceilingCollision;
        other.ropeRadius = ropeRadius;
        other.ceilingBox = ceilingBox;
        other.leftWallCollision = leftWallCollision;
        other.leftBox = leftBox;
        other.segmentLength = segmentLength;
        other.damping = damping;
        other.iterations = iterations;
        other.insulatedWidth = insulatedWidth;
        other.insulatedMat = insulatedMat;
        other.ropeRopeCollision = ropeRopeCollision;
        other.ropeRopePasses = ropeRopePasses;
        other.pointSkip = pointSkip;
        other.ropeRopeFriction = ropeRopeFriction;
        other.ropeRopeSlop = ropeRopeSlop;
        other.selfCollision = selfCollision;
        other.selfCollisionSkipNear = selfCollisionSkipNear;
        other.selfCollisionPasses = selfCollisionPasses;
        other.enableCollision = enableCollision;
        other.playerPushForce = playerPushForce;
        other.playerLayer = playerLayer;
        other.wirePiecePrefab = wirePiecePrefab;
        other.cutSnapImpulse = cutSnapImpulse;
        other.cutImpulsePoints = cutImpulsePoints;
        other.minPointsPerPiece = minPointsPerPiece;
        other.gravityFlipVelocityKill = gravityFlipVelocityKill;
        other.gravityFlipSettleFrames = gravityFlipSettleFrames;
        other.gravityFlipExtraDamping = gravityFlipExtraDamping;
        other.cutImpulseScale = cutImpulseScale;
        other.cutSettleIterations = cutSettleIterations;
        other.gravityFlipMinBendStiffness = gravityFlipMinBendStiffness;
        other.gravityFlipMinSlackStiffness = gravityFlipMinSlackStiffness;

        other.lr.startWidth = insulatedWidth;
        other.lr.endWidth = insulatedWidth;
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

            dir = dir.sqrMagnitude < EPS ? Vector3.up : dir.normalized;

            float falloff = 1f - (n / Mathf.Max(1f, (float)cutImpulsePoints));
            float amount = cutSnapImpulse * cutImpulseScale * falloff;

            RopePoint[i] += dir * amount;
            RopePointOld[i] = RopePoint[i] - dir * amount;
        }
    }

    private void RunSettlePass()
    {
        float[] restLengths = new float[points - 1];
        for (int i = 0; i < points - 1; i++)
            restLengths[i] = Mathf.Min(Vector3.Distance(RopePoint[i], RopePoint[i + 1]), segmentLength * 1.5f);

        for (int it = 0; it < cutSettleIterations; it++)
        {
            PinEndpoints();
            for (int i = 0; i < points - 1; i++)
                SolveSegmentDistance(i, i + 1, restLengths[i]);
        }

        UpdateLineRendererNow();
    }

    private static void ApplyPointCorrection(
        VerletWire wire,
        int indexA,
        int indexB,
        float tOnSegment,
        Vector3 correction)
    {
        float wA = 1f - tOnSegment;
        float wB = tOnSegment;

        if (!wire.IsPinnedPoint(indexA))
            wire.RopePoint[indexA] += correction * wA;

        if (!wire.IsPinnedPoint(indexB))
            wire.RopePoint[indexB] += correction * wB;
    }

    private static Vector3 GetPointVelocity(VerletWire wire, int i) =>
        wire.RopePoint[i] - wire.RopePointOld[i];

    private static void RemoveInwardNormalVelocity(
        VerletWire wire,
        int indexA,
        int indexB,
        float tOnSegment,
        Vector3 normal,
        Vector3 segmentVelocity,
        float friction)
    {
        float wA = 1f - tOnSegment;
        float wB = tOnSegment;

        FixPoint(indexA, wA);
        FixPoint(indexB, wB);

        void FixPoint(int idx, float weight)
        {
            if (weight <= 0f || wire.IsPinnedPoint(idx)) return;

            Vector3 vel = wire.RopePoint[idx] - wire.RopePointOld[idx];
            Vector3 rel = vel - segmentVelocity;
            float inward = Vector3.Dot(rel, normal);

            if (inward < 0f)
            {
                Vector3 remove = normal * inward;
                vel -= remove * (1f - friction * 0.5f);
                wire.RopePointOld[idx] = wire.RopePoint[idx] - vel;
            }
        }
    }

    private static float ClosestPointSegmentSegment(
        Vector3 p1, Vector3 q1,
        Vector3 p2, Vector3 q2,
        out float s, out float t,
        out Vector3 c1, out Vector3 c2)
    {
        Vector3 d1 = q1 - p1;
        Vector3 d2 = q2 - p2;
        Vector3 r = p1 - p2;

        float a = Vector3.Dot(d1, d1);
        float e = Vector3.Dot(d2, d2);
        float f = Vector3.Dot(d2, r);

        if (a <= 1e-6f && e <= 1e-6f)
        {
            s = t = 0f;
            c1 = p1;
            c2 = p2;
            return (c1 - c2).sqrMagnitude;
        }

        if (a <= 1e-6f)
        {
            s = 0f;
            t = Mathf.Clamp01(f / e);
        }
        else
        {
            float c = Vector3.Dot(d1, r);

            if (e <= 1e-6f)
            {
                t = 0f;
                s = Mathf.Clamp01(-c / a);
            }
            else
            {
                float b = Vector3.Dot(d1, d2);
                float denom = a * e - b * b;

                s = denom != 0f ? Mathf.Clamp01((b * f - c * e) / denom) : 0f;
                t = (b * s + f) / e;

                if (t < 0f)
                {
                    t = 0f;
                    s = Mathf.Clamp01(-c / a);
                }
                else if (t > 1f)
                {
                    t = 1f;
                    s = Mathf.Clamp01((b - c) / a);
                }
            }
        }
        c1 = p1 + d1 * s;
        c2 = p2 + d2 * t;
        return (c1 - c2).sqrMagnitude;
    }
}
