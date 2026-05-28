using NaughtyAttributes;
using UC;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class LionBrain : AgentFSM
{
    [Header("Lion Brain")]
    [HorizontalLine(color: EColor.Green)]
    [SerializeField] protected PatrolBehaviour  patrolBehaviour;
    [SerializeField] protected ChargeBehaviour  chargeBehaviour;
    [SerializeField] protected float            chargeCooldown = 5.0f;
    [SerializeField] protected Hypertag         playerTag;

    [SerializeField] 
    protected float viewDistance = 5;
    [SerializeField]
    protected float viewCone = 45.0f;

    protected float chargeTimer;

    protected override void Start()
    {
        base.Start();

        chargeBehaviour.onCompleteCharge += ChargeBehaviour_onCompleteCharge;
    }

    private void ChargeBehaviour_onCompleteCharge()
    {
        if (currentBehaviour == chargeBehaviour)
        {
            SetBehaviour(patrolBehaviour);
            chargeTimer = chargeCooldown;
        }
    }

    protected override void Update()
    {
        if (chargeTimer > 0)
        {
            chargeTimer -= Time.deltaTime;
        }

        if (currentBehaviour == patrolBehaviour)
        {
            if (chargeTimer <= 0.0f)
            {
                CheckForPlayer();
            }
        }
        else
        {

        }

        base.Update();
    }

    void CheckForPlayer()
    {
        var objects = playerTag.FindAllInRange<Ball>(transform.position, viewDistance);
        foreach (var obj in objects)
        {
            Vector3 toObj = (obj.transform.position - transform.position);
            toObj.SafeNormalize();
            if (Vector3.Angle(toObj, transform.forward) < viewCone)
            {
                // Check for LOS on the NavMesh
                if (!HasNavMeshLOS(transform.position, obj.transform.position))
                    continue;

                // Found the player, so we need to change state
                chargeBehaviour.chargeTarget = obj.transform.position;
                SetBehaviour(chargeBehaviour);
                break;
            }
        }
    }

    bool HasNavMeshLOS(Vector3 from, Vector3 to)
    {
        NavMeshQueryFilter filter = new NavMeshQueryFilter
        {
            agentTypeID = agent.agentTypeID,
            areaMask = agent.areaMask
        };

        if (!NavMesh.SamplePosition(from, out NavMeshHit fromHit, 1.0f, filter))
            return false;

        if (!NavMesh.SamplePosition(to, out NavMeshHit toHit, 1.0f, filter))
            return false;

        // NavMesh.Raycast returns true if blocked
        bool blocked = NavMesh.Raycast(fromHit.position, toHit.position, out NavMeshHit hit, filter);

        return !blocked;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 origin = transform.position;
        Vector3 forward = transform.forward;

        Gizmos.color = new Color(0.0f, 0.75f, 0.5f, 0.35f);

        // Cone limits
        Vector3 leftDir = Quaternion.AngleAxis(-viewCone, transform.up) * forward;
        Vector3 rightDir = Quaternion.AngleAxis(viewCone, transform.up) * forward;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(origin, origin + leftDir * viewDistance);
        Gizmos.DrawLine(origin, origin + rightDir * viewDistance);
        Gizmos.DrawLine(origin, origin + forward * viewDistance);

        // Draw the arc between the two cone limits
        const int segments = 24;

        Vector3 previousPoint = origin + leftDir * viewDistance;

        for (int i = 1; i <= segments; i++)
        {
            float t = i / (float)segments;
            float angle = Mathf.Lerp(-viewCone, viewCone, t);

            Vector3 dir = Quaternion.AngleAxis(angle, transform.up) * forward;
            Vector3 point = origin + dir * viewDistance;

            Gizmos.DrawLine(previousPoint, point);

            previousPoint = point;
        }
    }
}
