using NaughtyAttributes;
using UnityEngine;

public class CrabBrain : AgentFSM
{
    [HorizontalLine(color: EColor.Green)]
    [Header("Crab Brain")]
    [SerializeField] protected WanderBehaviour  wanderBehaviour;
    [SerializeField] protected ChargeBehaviour  chargeBehaviour;
    [SerializeField] protected ExplodeBehaviour explodeBehaviour;
    [SerializeField] protected float            chargeCooldown = 5.0f;

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
            if (chargeTimer <= 0.0f)
            {
                SetBehaviour(explodeBehaviour);
                //Explode();
            }
            chargeTimer = chargeCooldown;
        }
    }

    [Button("Explode")]
    void ForceExplosion()
    {
        SetBehaviour(explodeBehaviour);
    }

    protected override void Update()
    {
        if (chargeTimer > 0)
        {
            chargeTimer -= Time.deltaTime;
        }

        if (currentBehaviour == wanderBehaviour)
        {
            if (chargeTimer <= 0.0f)
            {
                var ball = CheckForPlayer();
                if (ball)
                {
                    // Found the player, so we need to change state
                    chargeBehaviour.chargeTarget = ball.transform;
                    SetBehaviour(chargeBehaviour);
                }
            }
        }
        else
        {

        }

        base.Update();
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
