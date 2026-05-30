using NaughtyAttributes;
using System.Collections;
using UC;
using UC.RPG;
using UnityEngine;
using UnityEngine.AI;

public class CrabBrain : AgentFSM
{
    [HorizontalLine(color: EColor.Green)]
    [Header("Crab Brain")]
    [SerializeField] protected WanderBehaviour  wanderBehaviour;
    [SerializeField] protected ChargeBehaviour  chargeBehaviour;
    [SerializeField] protected float            chargeCooldown = 5.0f;
    [SerializeField] protected float            blastTime = 1.0f;
    [SerializeField] protected float            maxBlastRange = 2.5f;
    [SerializeField] protected float            blastDamage = 2.0f;
    [SerializeField] protected GameObject       mainRenderObject;
    [SerializeField] protected Transform        shockwaveTransform;
    [SerializeField] protected MeshRenderer     blastRenderer;

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
                Explode();
            }
            chargeTimer = chargeCooldown;
        }
    }

    [Button("Explode")]
    void Explode()
    {
        StartCoroutine(BlastCR());
    }

    IEnumerator BlastCR()
    {
        float elapsedTime = 0.0f;        
        var     players = playerTag.FindAll<Ball>();
        float   prevDist = 0.0f;
        var     mpb = new MaterialPropertyBlock();
        blastRenderer.GetPropertyBlock(mpb);
        var blastColor = blastRenderer.sharedMaterial.GetColor("_EmissionColor");
        blastColor.Normalize();

        while (elapsedTime < blastTime)
        {
            elapsedTime += Time.deltaTime;

            float t = Mathf.Clamp01(elapsedTime / blastTime);
            t = Mathf.Pow(t, 0.5f);

            float currentDist = maxBlastRange * t;

            foreach (var player in players)
            {
                float d = Vector3.Distance(player.transform.position.x0z(), transform.position.x0z());
                if ((d >= prevDist) && (d < currentDist))
                {
                    var res = player.FindResourceHandler(Globals.healthResource);
                    res.Change(new ChangeData(-blastDamage)
                    {
                        changeSrcPosition = player.transform.position,
                        changeSrcDirection = (player.transform.position - transform.position).normalized,
                        knockbackStrength = 2.0f
                    });
                }
            }

            shockwaveTransform.localScale = currentDist * Vector3.one * 2.0f;
            mpb.SetColor("_EmissionColor", blastColor * 2.5f * (1.0f - t));
            blastRenderer.SetPropertyBlock(mpb);

            prevDist = currentDist;

            mainRenderObject.SetActive(t < 0.5f);

            yield return null;
        }

        Destroy(gameObject);
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
