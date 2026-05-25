using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(Rigidbody))]
public class BallPhysics : MonoBehaviour
{
    [SerializeField]
    private float rollingResistance = 0.35f;
    [SerializeField]
    private float stopSpeed = 0.03f;
    [Header("Slope Gravity")]
    [SerializeField]
    private float slopeGravityMultiplier = 0.25f;
    [Tooltip("If true, removes vertical velocity before projecting onto the ground, avoiding accidental slope rolling.")]
    [Header("Collision")]
    [SerializeField]
    private LayerMask collisionMask;
    [SerializeField]
    private float bounce = 0.75f;
    [SerializeField]
    private int maxCollisionIterations = 4;
    [SerializeField]
    private float skinWidth = 0.01f;
    [Tooltip("Wall hits with normals more upward than this are ignored as ground.")]
    [SerializeField, Range(0.0f, 1.0f)]
    private float maxGroundNormalForWallHit = 0.5f;
    [Header("Ground Hugging")]
    [SerializeField]
    private LayerMask groundMask;
    [SerializeField]
    private float groundProbeHeight = 0.5f;
    [SerializeField]
    private float groundProbeDistance = 2.0f;
    [SerializeField]
    private bool projectVelocityOntoGround = true;
    [Header("Debug")]
    [SerializeField]
    private bool drawDebug;

    private Vector3 _linearVelocity;
    private SphereCollider sphereCollider;
    private Rigidbody rb;

    public Vector3 linearVelocity
    {
        get => _linearVelocity;
        set => _linearVelocity = value;
    }

    public bool isMoving
    {
        get
        {
            Vector3 flat = _linearVelocity;
            flat.y = 0.0f;
            return flat.magnitude > stopSpeed;
        }
    }

    private float radius
    {
        get
        {
            float scale = Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.y), Mathf.Abs(transform.lossyScale.z));

            return sphereCollider.radius * scale;
        }
    }

    private void Awake()
    {
        sphereCollider = GetComponent<SphereCollider>();
        rb = GetComponent<Rigidbody>();

        rb.isKinematic = true;
        rb.useGravity = false;
    }

    private void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        Vector3 position = rb.position;

        position = HugGround(position, dt, true);

        ApplyRollingResistance(dt);
        StopIfVerySlow();

        if (_linearVelocity.sqrMagnitude > 0.0f)
        {
            position = MoveWithCollisions(position, dt);
        }

        position = HugGround(position, dt, false);

        rb.MovePosition(position);
    }

    private void ApplyRollingResistance(float dt)
    {
        float speed = _linearVelocity.magnitude;

        if (speed <= 0.0f)
            return;

        float newSpeed = Mathf.Max(0.0f, speed - rollingResistance * dt);

        _linearVelocity = _linearVelocity.normalized * newSpeed;
    }

    private void StopIfVerySlow()
    {
        Vector3 flat = _linearVelocity;
        flat.y = 0.0f;

        if (flat.magnitude <= stopSpeed)
        {
            _linearVelocity.x = 0.0f;
            _linearVelocity.z = 0.0f;
        }

        if (Mathf.Abs(_linearVelocity.y) <= stopSpeed)
        {
            _linearVelocity.y = 0.0f;
        }
    }

    private Vector3 MoveWithCollisions(Vector3 position, float dt)
    {
        Vector3 remainingDisplacement = _linearVelocity * dt;

        for (int i = 0; i < maxCollisionIterations; i++)
        {
            float distance = remainingDisplacement.magnitude;

            if (distance <= 0.000001f)
                break;

            Vector3 direction = remainingDisplacement / distance;

            if (FindWallHit(position, direction, distance + skinWidth, out RaycastHit hit))
            {
                float moveDistance = Mathf.Max(0.0f, hit.distance - skinWidth);

                position += direction * moveDistance;
                position += hit.normal * skinWidth;

                IBallContactResponder responder = hit.collider.GetComponentInParent<IBallContactResponder>();

                if (responder != null)
                {
                    responder.OnBallContact(this, hit);
                }

                _linearVelocity = Vector3.Reflect(_linearVelocity, hit.normal) * bounce;

                float consumedFraction = distance > 0.0f ? moveDistance / distance : 1.0f;
                float remainingFraction = Mathf.Clamp01(1.0f - consumedFraction);

                remainingDisplacement = _linearVelocity * dt * remainingFraction;
            }
            else
            {
                position += remainingDisplacement;
                break;
            }
        }

        return position;
    }

    private bool ShouldIgnoreConditionalObstacle(Collider collider)
    {
        IConditionalObstacle condObstacle = collider.GetComponentInParent<IConditionalObstacle>();

        return (condObstacle != null) && condObstacle.ShouldIgnoreCollision(this);
    }

    private bool FindWallHit(Vector3 position, Vector3 direction, float distance, out RaycastHit selectedHit)
    {
        selectedHit = default;

        RaycastHit[] hits = Physics.SphereCastAll(position, radius - skinWidth, direction, distance, collisionMask, QueryTriggerInteraction.Ignore);

        if (hits == null || hits.Length == 0)
            return false;

        float bestDistance = float.PositiveInfinity;
        bool found = false;

        foreach (RaycastHit hit in hits)
        {
            // Ignore self.
            if (hit.collider == sphereCollider)
                continue;

            // Ignore conditional obstacles.
            if (ShouldIgnoreConditionalObstacle(hit.collider))
                continue;

            IBallContactResponder responder = hit.collider.GetComponentInParent<IBallContactResponder>();

            bool hasResponder = responder != null;
            bool shouldHandleAsCollision = false;

            if (hasResponder)
            {
                // Important:
                // Do not call responder.OnBallContact here.
                // This function should only decide whether the hit is solid.
                shouldHandleAsCollision = responder.ShouldHandleAsCollision(this, hit);

                if (!shouldHandleAsCollision)
                    continue;
            }

            // If this is not a special responder, ignore mostly-upward normals as ground.
            // For responders, allow them to behave as solid even if the normal is upward.
            if (!hasResponder && hit.normal.y > maxGroundNormalForWallHit)
                continue;

            if (hit.distance < bestDistance)
            {
                bestDistance = hit.distance;
                selectedHit = hit;
                found = true;
            }
        }

        return found;
    }

    private Vector3 HugGround(Vector3 position, float dt, bool applySlopeGravity)
    {
        Vector3 origin = position + Vector3.up * groundProbeHeight;
        float maxDistance = groundProbeHeight + groundProbeDistance;

        RaycastHit[] hits = Physics.RaycastAll(origin, Vector3.down, maxDistance, groundMask, QueryTriggerInteraction.Ignore);

        if (hits == null || hits.Length == 0)
            return position;

        RaycastHit selectedHit = default;
        float bestDistance = float.PositiveInfinity;
        bool found = false;

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider == sphereCollider)
                continue;

            if (ShouldIgnoreConditionalObstacle(hit.collider))
                continue;

            if (hit.distance < bestDistance)
            {
                bestDistance = hit.distance;
                selectedHit = hit;
                found = true;
            }
        }

        if (!found)
            return position;

        float normalY = Mathf.Max(selectedHit.normal.y, 0.2f);
        position.y = selectedHit.point.y + radius / normalY;

        if (applySlopeGravity)
        {
            ApplySlopeGravity(selectedHit.normal, dt);
        }

        if (projectVelocityOntoGround)
        {
            _linearVelocity = Vector3.ProjectOnPlane(_linearVelocity, selectedHit.normal);
        }

        return position;
    }

    private void ApplySlopeGravity(Vector3 groundNormal, float dt)
    {
        if (slopeGravityMultiplier <= 0.0f)
            return;

        Vector3 slopeGravity = Vector3.ProjectOnPlane(Physics.gravity, groundNormal);

        _linearVelocity += slopeGravity * slopeGravityMultiplier * dt;
    }

    public void Bounce(Vector3 normal)
    {
        Bounce(normal, bounce);
    }

    public void Bounce(Vector3 normal, float bounceMultiplier)
    {
        if (normal.sqrMagnitude <= 0.000001f)
            return;

        normal.Normalize();

        // Only bounce if the ball is moving into the surface.
        // If it is already moving away, reflecting would create weird double-bounces.
        float velocityIntoSurface = Vector3.Dot(_linearVelocity, normal);

        if (velocityIntoSurface < 0.0f)
        {
            _linearVelocity = Vector3.Reflect(_linearVelocity, normal) * bounceMultiplier;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!drawDebug)
            return;

        if (sphereCollider == null)
            sphereCollider = GetComponent<SphereCollider>();

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + _linearVelocity);
    }
#endif
}