    using System;
    using System.Collections.Generic;
    using System.Runtime.Remoting.Messaging;
    using UnityEngine;

    public class VerletInsulatedWire : MonoBehaviour
    {
        [Header("Wire Data")]
        public int points = 35;
        public float segmentLength = 0.4f;
        private Vector3[] currentWirePointsPosition;
        private Vector3[] previousWirePointsPosition;
        [Header("Anchors")]
        [SerializeField] private Transform anchor1;
        [SerializeField] private Transform anchor2;
        [SerializeField] private bool pinSartWirePoint;
        [SerializeField] private bool pinEndWirePoint;
        private bool initialiseWire = false;
        [SerializeField] private float damping = 0.02f;
        [SerializeField] private int wireSolvingIterations = 2;
        
        public float bendStiffness = 0.18f;
        public float slackStiffness = 0.08f;
        
        public bool useDynamicGravity = true;
        public bool ignoreGravity = false;
        public Vector3 customGravity = new Vector3(0f, -9.81f, 0f);
        public WireGravity gravitySource;
        private Vector3 lastGravityDirection = Vector3.down;
        private int gravityFlipIterationsLeft = 0;
        public int settleGravityFlipIterations = 20;
        
        public float slackThreshold = 0.03f;
        public bool pullWire = true;
        public float RetractionSpeed = 6f;
        public float RestoreSpeed = 3f;
        
        private float dynamicSegmentLength;
        private float targetSegmentLength;
        
        public bool ceilingCollision = true;
        public float ropeRadius = 0.25f;
        [SerializeField] private Collider ceilingCollider;
        
        public bool leftWallCollision = true;
        [SerializeField] private Collider leftWallCollider;
        
        public bool rightWallCollision = true;
        [SerializeField] private Collider rightWallCollider;
        

        public int selfCollisionSkipNear = 6;
        public int selfCollisionPasses = 2;
        
        private CapsuleCollider[] segmentColliders;
        
        public bool isCuttable = true;
        public VerletInsulatedWire wirePiecePrefab;
        public int minPointsPerPiece = 2;
        
        [Header("Cut Visuals")]
        [SerializeField] private Transform copperTipPrefab;
        [SerializeField] private float copperTipOffset = 0.02f;

        private void HandelHasCutWire()
        {
            // isCuttable = false;
        }
        

        private int postCutFrames = 0;

        
        public enum  WireColour
        {
            None,
            Red,
            Blue,
            Grey,
            Brown,
        }
        
        public WireColour wireColour;
        public static bool HasAnyWireBeenCut { get; private set; } = false;
        public static VerletInsulatedWire.WireColour? CutWireColor { get; set; }

        private bool hasBeenCut = false;

        void OnEnable()
        {
            WireCut.HasCutSoftwareWire += HandelHasCutWire;
        }

        void OnDisable()
        {
            WireCut.HasCutSoftwareWire -= HandelHasCutWire;
        }

        public static void ResetStaticState()
        {
            HasAnyWireBeenCut = false;
            CutWireColor = null;
        }
        
        private void Start()
        {
            if (!initialiseWire)
            {
                if (anchor1 == null || anchor2 == null)
                {
                    return;
                }

                InitialiseVerletWire();
            }
            BuildColliders();
        }

        private void FixedUpdate()
        {
            if (!initialiseWire)
            {
                return;
            }

            Vector3 gravity = Gravity();
            CheckGravityFlip(gravity);
            UpdateDynamicSegmentLength(gravity);
            MoveWirePoints();
            SettleWire();
            FixAnchors();
            CorrectWireSegments();
            CeilingCollision();
            LeftWallCollision();
            RightWallCollision();
            CorrectSlackWire();
            BendWire();
            CeilingCollision();
            LeftWallCollision();
            RightWallCollision();
            for (int i = 0; i < selfCollisionPasses; i++)
            {
                SelfCollision();
            }
            FixAnchors();
            UpdateColliders();
        }

        private void InitialiseVerletWire()
        {
            currentWirePointsPosition = new Vector3[points];
            previousWirePointsPosition = new Vector3[points];
            for (int i = 0; i < points; i++)
            {
                float step = (float)i / (points - 1);
                Vector3 position = Vector3.Lerp(anchor1.position, anchor2.position, step);
                currentWirePointsPosition[i] = position;
                previousWirePointsPosition[i] = position;
            }
            initialiseWire = true;
            
        }

        private void MoveWirePoints()
        {
            for (int i = 1; i < points - 1; i++)
            {
                float effectiveDamping = ResolveDamping();
                Vector3 gravity = Gravity();
                Vector3 velocity = (currentWirePointsPosition[i] - previousWirePointsPosition[i]) * (1 - effectiveDamping);
                previousWirePointsPosition[i] = currentWirePointsPosition[i];
                currentWirePointsPosition[i] += velocity + gravity * (Time.fixedDeltaTime * Time.fixedDeltaTime);
            }
        }

        private void FixAnchors()
        {
            if (pinSartWirePoint)
            {
                currentWirePointsPosition[0] = anchor1.position;
                previousWirePointsPosition[0] = anchor1.position;
            }

            if (pinEndWirePoint)
            {
                currentWirePointsPosition[points - 1] = anchor2.position;
                previousWirePointsPosition[points - 1] = anchor2.position;
            }
        }

        private bool CheckAnchors(int point)
        {
            bool isAnchor1  = false;
            bool isAnchor2 = false;
            if (point == 0)
            {
                isAnchor1 = true;
            }
            else if(point == points - 1)
            {
                isAnchor2 = true;
            }
            return (pinSartWirePoint && isAnchor1) || (pinEndWirePoint && isAnchor2);
        }

        private void CorrectWireSegments()
        {
            float targetLength = 0;
            if (dynamicSegmentLength > 0)
            {
                targetLength = dynamicSegmentLength;
            }
            else
            {
                targetLength = segmentLength;
            }
            for (int i = 0; i < points - 1; i++)
            {
                Vector3 direction = currentWirePointsPosition[i + 1] - currentWirePointsPosition[i];
                float distance = direction.magnitude;
                float errorDistance = (distance - targetLength) / distance;
                bool isAnchor1 = CheckAnchors(i);
                bool isAnchor2 = CheckAnchors(i + 1);
                if (isAnchor1 && isAnchor2)
                {
                    continue;;
                }

                if (isAnchor1)
                {
                    currentWirePointsPosition[i + 1] -= direction * errorDistance;
                }
                else if (isAnchor2)
                {
                    currentWirePointsPosition[i] +=  direction * errorDistance;
                }
                else
                {
                    currentWirePointsPosition[i] += direction * (errorDistance * 0.5f);
                    currentWirePointsPosition[i + 1] -= direction * (errorDistance * 0.5f);
                }
                
                
            }
        }
        
        private float ResolveDamping()
        {
            float effectiveDamping = damping;
            if (gravityFlipIterationsLeft > 0)
            {
                effectiveDamping += 0.18f;
                gravityFlipIterationsLeft--;
            }
            return effectiveDamping;
        }

        private void DampenFullVelocity(float killVelocity)
        {
            for (int i = 1; i < points - 1; i++)
            {
                Vector3 velocity = currentWirePointsPosition[i] - previousWirePointsPosition[i];
                velocity *= 1f - Mathf.Clamp01(killVelocity);
                previousWirePointsPosition[i] = currentWirePointsPosition[i] -  velocity;
            }
        }
        
        private void BendWire()
        {
            for (int i = 1; i < points - 1; i++)
            {
                if (CheckAnchors(i))
                {
                    continue;
                }
                Vector3 midpoint = (currentWirePointsPosition[i - 1] + currentWirePointsPosition[i + 1]) / 2;
                currentWirePointsPosition[i] = Vector3.Lerp(currentWirePointsPosition[i], midpoint, bendStiffness);
            }
        }

        private float SlackRatio()
        {
            float targetLength = 0;
            if (dynamicSegmentLength > 0)
            {
                targetLength = dynamicSegmentLength;
            }
            else
            {
                targetLength = segmentLength;
            }
            float wireLength = targetLength * (points - 1);
            float anchorDistance = Vector3.Distance(currentWirePointsPosition[0], currentWirePointsPosition[points - 1]);
            float slack = wireLength -  anchorDistance;
            float slackRatio = Mathf.Clamp01(slack / wireLength);
            return slackRatio;
        }
        
        private void CorrectSlackWire()
        {
            float slackRatio = SlackRatio();
            float stiffness = slackStiffness * Mathf.Lerp(0.15f, 0.45f, slackRatio);
            if (stiffness <= 0f)
            {
                return;
            }
            for (int i = 0; i < points - 2; i++)
            {
                Vector3 direction = currentWirePointsPosition[i + 2] - currentWirePointsPosition[i];
                float distance = direction.magnitude;
                float targetLength = 0;
                if (dynamicSegmentLength > 0)
                {
                    targetLength = dynamicSegmentLength;
                }
                else
                {
                    targetLength = segmentLength;
                }
                float errorDistance = (distance - 2 * targetLength) / distance;
                bool isAnchor1 = CheckAnchors(i);
                bool isAnchor2 = CheckAnchors(i + 2);
                if (isAnchor1 && isAnchor2)
                {
                    continue;
                }

                if (isAnchor1)
                {
                    currentWirePointsPosition[i + 2] -= direction * errorDistance;
                }
                else if (isAnchor2)
                {
                    currentWirePointsPosition[i] +=  direction * errorDistance;
                }
                else
                {
                    currentWirePointsPosition[i] += direction * (errorDistance * 0.5f);
                    currentWirePointsPosition[i + 2] -= direction * (errorDistance * 0.5f);
                }
            }
        }
        private Vector3 Gravity()
        {
            if (ignoreGravity)
            {
                return Vector3.zero;
            }

            if (useDynamicGravity)
            {
                if (gravitySource != null)
                {
                    return gravitySource.Gravity;
                }
                else
                {
                    return Physics.gravity;
                }
            }

            return customGravity;
        }
        
        private void CheckGravityFlip(Vector3 gravity)
        {
            float dampFlippingVelocity = 0.92f;
            Vector3 newDirection = gravity.normalized;
            float dot = Vector3.Dot(lastGravityDirection, newDirection);
            if (dot < 0.85f)
            {
                float killFraction = Mathf.Lerp(0.3f, dampFlippingVelocity, Mathf.Clamp01((0.85f - dot) / 1.85f));
                DampenFullVelocity(killFraction);
                gravityFlipIterationsLeft = settleGravityFlipIterations;
            }

            lastGravityDirection = newDirection;
        }
        
        private bool IsDoublePinnedWire()
        {
            return pinSartWirePoint && pinEndWirePoint && anchor1 != null && anchor2 != null;
        }
        
        private void SettleWire()
        {
            if (!IsDoublePinnedWire())
            {
                return;
            }
            if (gravityFlipIterationsLeft <= 0)
            {
                return;
            }

            if (SlackRatio() <= slackThreshold)
            {
                return;
            }

            Vector3 wire = currentWirePointsPosition[points - 1] - currentWirePointsPosition[0];
            float wireSquareMagnitude = wire.sqrMagnitude;
            Vector3 wireMoveDirection = wire / Mathf.Sqrt(wireSquareMagnitude);
            float dampVelocity = 0.85f;
            for (int i = 1; i < points - 1; i++)
            {
                if (CheckAnchors(i))
                {
                    continue;
                }
                Vector3 velocity = currentWirePointsPosition[i] - previousWirePointsPosition[i];
                Vector3 velocityAlongWire = Vector3.Dot(velocity, wireMoveDirection) * wireMoveDirection;
                Vector3 velocityPerpendicular = velocity - velocityAlongWire;
                velocityAlongWire *= dampVelocity;
                velocityPerpendicular *= 1f - dampVelocity;
                Vector3 newVel = velocityAlongWire + velocityPerpendicular;
                previousWirePointsPosition[i] = currentWirePointsPosition[i] - newVel;
            }
        }

        private void UpdateDynamicSegmentLength(Vector3 gravity)
        {
            if (!pullWire)
            {
                dynamicSegmentLength = segmentLength;
                targetSegmentLength = segmentLength;
                return;
            }
            
            Vector3 gravityDirection = gravity.normalized;
            bool upwardGravity = Vector3.Dot(gravityDirection, Vector3.up) > 0.75f;
            float anchorDistance = Vector3.Distance(currentWirePointsPosition[0], currentWirePointsPosition[points - 1]);        float wireLength = segmentLength * (points - 1);
            float slack = wireLength - anchorDistance;
            float slackRatio = slack / wireLength;
            float SlackThreshold = 0.02f;
            if (upwardGravity && slackRatio > SlackThreshold)
            {
                float desiredTotalLength = Mathf.Lerp(wireLength, anchorDistance, 1);
                desiredTotalLength = Mathf.Max(desiredTotalLength, anchorDistance);
                targetSegmentLength = desiredTotalLength / (points - 1);
            }
            else
            {
                targetSegmentLength = segmentLength;
            }

            float speed = 0;
            if (upwardGravity)
            {
                speed = RetractionSpeed;
            }
            else
            {
                speed = RestoreSpeed;
            }
            dynamicSegmentLength = Mathf.Lerp(dynamicSegmentLength, targetSegmentLength, Time.fixedDeltaTime * speed);
        }
        
        private void CeilingCollision()
        {
            if (!ceilingCollision || ceilingCollider == null)
            {
                return;
            }
            float maxY = ceilingCollider.bounds.min.y - ropeRadius;
            for (int i = 1; i < points; i++)
            {
                if (currentWirePointsPosition[i].y > maxY)
                {
                    currentWirePointsPosition[i].y = maxY;
                }
            }
        }

        private void LeftWallCollision()
        {
            if (!leftWallCollision || leftWallCollider == null)
            {
                return;
            }
            float minX = leftWallCollider.bounds.max.x + ropeRadius;

            for (int i = 1; i < points; i++)
            {
                if (currentWirePointsPosition[i].x < minX)
                {
                    currentWirePointsPosition[i].x = minX;
                }
            }
        }
        
        private void RightWallCollision()
        {
            if (!rightWallCollision || rightWallCollider == null)
            {
                return;
            }

            float maxX = rightWallCollider.bounds.min.x - ropeRadius;

            for (int i = 1; i < points; i++)
            {
                if (currentWirePointsPosition[i].x > maxX)
                {
                    currentWirePointsPosition[i].x = maxX;
                }
            }
        }
        
        
        private void SelfCollision()
        {
            float minDistance = ropeRadius * 2f;
            float minSquareDistance = minDistance * minDistance;
            for (int i = 0; i < points; i++)
            {
                if (CheckAnchors(i))
                {
                    continue;
                }
                Vector3 currentWirePoint = currentWirePointsPosition[i];
                for (int j = i + selfCollisionSkipNear; j < points; j++)
                {
                    if (CheckAnchors(j))
                    {
                        continue;
                    }
                    Vector3 wire = currentWirePoint - currentWirePointsPosition[j];;
                    float wireSquareMagnitude = wire.sqrMagnitude;
        
                    if (wireSquareMagnitude >= minSquareDistance)
                    {
                        continue;
                    }
                    float distance = Mathf.Sqrt(wireSquareMagnitude);
                    Vector3 normal = wire / distance;
                    float errorDistanceHalf = (minDistance - distance) / 2;
                    currentWirePointsPosition[i] += normal * errorDistanceHalf;
                    currentWirePointsPosition[j] -= normal * errorDistanceHalf;
                    Vector3 velocityI = currentWirePointsPosition[i] - previousWirePointsPosition[i];
                    Vector3 velocityJ = currentWirePointsPosition[j] - previousWirePointsPosition[j];
                    float relativeVelocity = Vector3.Dot(velocityI - velocityJ, normal);
                    if (relativeVelocity < 0f)
                    {
                        Vector3 impulse = normal * relativeVelocity * 0.5f;
                        previousWirePointsPosition[i] += impulse;
                        previousWirePointsPosition[j] -= impulse;
                    }
                    currentWirePoint = currentWirePointsPosition[i];
                }
            }
        }
        
        private void BuildColliders()
        {
            if (segmentColliders != null)
            {
                foreach (CapsuleCollider collider in segmentColliders)
                {
                    if (collider != null)
                    {
                        Destroy(collider.gameObject);
                    }
                }
            }
            segmentColliders = new CapsuleCollider[Mathf.Max(0, points - 1)];
            for (int i = 0; i < points - 1; i++)
            {
                GameObject objects = new GameObject("WireCollider_" + i);
                objects.transform.SetParent(transform, false);
                CapsuleCollider capsule = objects.AddComponent<CapsuleCollider>();
                capsule.radius = ropeRadius;
                capsule.direction = 1;
                capsule.height = segmentLength;
                capsule.isTrigger = false;
                segmentColliders[i] = capsule;
            }
        }

        private void UpdateColliders()
        {
            for (int i = 0; i < points - 1; i++)
            {
                if (segmentColliders[i] == null)
                {
                    continue;
                }
                Vector3 direction = currentWirePointsPosition[i + 1] - currentWirePointsPosition[i];
                segmentColliders[i].transform.position = (currentWirePointsPosition[i] + currentWirePointsPosition[i + 1]) / 2;
                segmentColliders[i].transform.rotation = Quaternion.FromToRotation(Vector3.up, direction);
                segmentColliders[i].radius = ropeRadius;
                segmentColliders[i].height = Vector3.Distance(currentWirePointsPosition[i], currentWirePointsPosition[i + 1]) + ropeRadius * 2f;
            }
        }
        
        public int WirePointsCount()
        {
            return currentWirePointsPosition.Length;
        }

        public Vector3 GetPointPosition(int i)
        {
            return currentWirePointsPosition[i];
        }
        
        public void CutPosition(Vector3 worldPosistion)
        {
            if (!isCuttable)
            {
                return;
            }

            if (hasBeenCut || HasAnyWireBeenCut)
            {
                return;
            }
            HasAnyWireBeenCut = true;
            CutWireColor = wireColour;
            int minSegment = Mathf.Max(0, minPointsPerPiece - 2);
            int maxSegment = Mathf.Min(points - 2, points - minPointsPerPiece);
            int segementIndex = Mathf.Clamp(FindClosestSegment(worldPosistion), minSegment, maxSegment);
            Vector3 segmentA = currentWirePointsPosition[segementIndex];
            Vector3 segmentB = currentWirePointsPosition[segementIndex + 1];
            Vector3 direction = segmentB - segmentA;
            float position = Vector3.Dot(worldPosistion - segmentA, direction) / direction.sqrMagnitude;
            CreateCutPieces(segementIndex, segmentA + direction * position);
        }
        
        private int FindClosestSegment(Vector3 worldPos)
        {
            int bestIndex = 0;
            float bestDistance = float.MaxValue;
            for (int i = 0; i < points - 1; i++)
            {
                Vector3 closest = ClosestPointOnSegment(currentWirePointsPosition[i], currentWirePointsPosition[i + 1], worldPos);
                float distance = (worldPos - closest).sqrMagnitude;
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestIndex = i;
                }
            }
            return bestIndex;
        }
        
        private static Vector3 ClosestPointOnSegment(Vector3 segmentStart, Vector3 segmentEnd, Vector3 point)
        {
            Vector3 direction = segmentEnd - segmentStart;
            return segmentStart + direction * Mathf.Clamp01(Vector3.Dot(point - segmentStart, direction) / direction.sqrMagnitude);
        }
        
        private void CreateCutPieces(int segementIndex, Vector3 cutPos)
        {
            hasBeenCut = true;
            Vector3 direction = currentWirePointsPosition[segementIndex + 1] - currentWirePointsPosition[segementIndex];
            direction = direction.normalized;
            float gap = ropeRadius * 10.0f;
            Vector3 leftCutPos = cutPos - direction * gap;
            Vector3 rightCutPos = cutPos + direction * gap;
            int leftCount = segementIndex + 1;
            int rightCount = points - segementIndex - 1;
            Vector3[] leftPts = new Vector3[leftCount];
            Vector3[] leftOld = new Vector3[leftCount];
            Vector3[] rightPts = new Vector3[rightCount];
            Vector3[] rightOld = new Vector3[rightCount];
            for (int i = 0; i < leftCount - 1; i++)
            {
                leftPts[i] = currentWirePointsPosition[i];
                leftOld[i] = currentWirePointsPosition[i];
            }
            leftPts[leftCount - 1] = leftCutPos;
            leftOld[leftCount - 1] = leftCutPos;
            rightPts[0] = rightCutPos;
            rightOld[0] = rightCutPos;

            int rightIndex = 1;
            for (int i = segementIndex + 2; i < points; i++)
            {
                rightPts[rightIndex] = currentWirePointsPosition[i];
                rightOld[rightIndex] = currentWirePointsPosition[i];
                rightIndex++;
            }

            VerletInsulatedWire leftWire = SpawnCutPiece(leftPts, leftOld, true, anchor1, false, null);
            VerletInsulatedWire rightWire = SpawnCutPiece(rightPts, rightOld, false, null, true, anchor2);

            if (leftWire != null)
            {
                leftWire.SpawnCopperTip(leftCutPos, true);
            }

            if (rightWire != null)
            {
                rightWire.SpawnCopperTip(rightCutPos, false);
            }
            Destroy(gameObject);
        }
        
        private VerletInsulatedWire SpawnCutPiece(Vector3[] points, Vector3[] oldPpoints, bool newPinStart, Transform newAnchor1, bool newPinEnd, Transform newAnchor2)
        {
            VerletInsulatedWire wire = Instantiate(wirePiecePrefab, transform.position, transform.rotation);
            wire.InitializeCutPiece(points, oldPpoints, newPinStart, newAnchor1, newPinEnd, newAnchor2);
            return wire;
        }
        
        public void InitializeCutPiece(Vector3[] newPoints, Vector3[] newOldPoints, bool newPinStart, Transform newAnchor1, bool newPinEnd, Transform newAnchor2)
        {
            pinSartWirePoint = newPinStart;
            pinEndWirePoint = newPinEnd;
            anchor1 = newAnchor1;
            anchor2 = newAnchor2;
            points = Mathf.Max(newPoints.Length, 2);
            currentWirePointsPosition = (Vector3[])newPoints.Clone();
            previousWirePointsPosition = (Vector3[])newOldPoints.Clone();
            dynamicSegmentLength = segmentLength;
            targetSegmentLength = segmentLength;
            initialiseWire = true;
            ignoreGravity = true;

            if (gravitySource == null)
            {
                gravitySource = FindFirstObjectByType<WireGravity>();
            }

            FixAnchors();
            BuildColliders();
            UpdateColliders();
            enabled = false;
        }
        
        private void SpawnCopperTip(Vector3 cutPosition, bool isTopEnd)
        {
            if (copperTipPrefab == null)
            {
                return;
            }
            Quaternion spawnRot = isTopEnd ? Quaternion.Euler(180f, 0f, 0f) : Quaternion.Euler(0f, 0f, 0f);
            Instantiate(copperTipPrefab, cutPosition, spawnRot, transform);
        }
    }