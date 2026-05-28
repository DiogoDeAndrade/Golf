using NaughtyAttributes;
using UC;
using UnityEngine;

public class WanderBehaviour : AgentBehaviour
{
    [SerializeField] 
    private float wanderRadius = 2.0f;
    [SerializeField, MinMaxSlider(0.0f, 10.0f)]
    private Vector2 waitTime;

    Vector3?    currentTarget;
    float       waitTimer = 0;

    public override void Enter(Agent agent)
    {
        waitTimer = waitTime.Random();
        agent.SetSpeed(movementSpeed);
    }

    public override void Exit(Agent agent)
    {
    }

    public override void Tick(Agent agent)
    {
        if (waitTimer > 0)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0.0f)
            {
                SelectTarget(agent);
            }
        }
    }

    void SelectTarget(Agent agent)
    {
        // Select a target - a point within a certain radius of the spawn point that is accessible on the navigation mesh from the current point
        Vector3 spawnPos = agent.spawnPos;

        int nTries = 0;
        while (nTries < 20)
        {
            currentTarget = spawnPos + Random.insideUnitCircle.x0y() * wanderRadius;

            if (agent.HasPath(currentTarget.Value))
            {
                break;
            }
            nTries++;
        }

        if (nTries == 20)
        {
            currentTarget = null;
            return;
        }

        agent.MoveTo(currentTarget.Value, (agent, reachedTarget) =>
        {
            if (reachedTarget)
            {
                waitTimer = waitTime.Random();
            }
        });
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1.0f, 1.0f, 0.0f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, wanderRadius);
    }
}
