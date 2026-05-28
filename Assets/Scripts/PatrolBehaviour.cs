using NaughtyAttributes;
using System;
using System.Collections.Generic;
using UC;
using UnityEngine;

public class PatrolBehaviour : AgentBehaviour
{
    public enum Mode { Waypoints };

    [SerializeField]
    private Mode        mode;
    [SerializeField, ShowIf(nameof(isWaypoints))] 
    private Transform[] waypoints;
    [SerializeField, MinMaxSlider(0.0f, 10.0f)]
    private Vector2     waitTime;

    bool isWaypoints => mode == Mode.Waypoints;

    List<Vector3>   worldWaypoints;
    int             index = 0;
    float           waitTimer;

    void Awake()
    {
        worldWaypoints = new();
        foreach (var waypoint in waypoints)
        {
            worldWaypoints.Add(waypoint.position);
        }
    }

    public override void Enter(Agent agent)
    {        
        index = FindClosestWaypoint();
        if (index >= 0)
        {
            agent.MoveTo(worldWaypoints[index], GotoNextPoint);
        }
        agent.SetSpeed(movementSpeed);
    }

    private void GotoNextPoint(Agent agent, bool arrived)
    {
        if (arrived)
        {
            index = (index + 1) % worldWaypoints.Count;
            float t = waitTime.Random();
            if (t > 0)
            {
                waitTimer = t;
            }
            else
            {
                agent.MoveTo(worldWaypoints[index], GotoNextPoint);
            }
        }
    }

    public override void Exit(Agent agent)
    {
        agent.StopMovement();
    }

    public override void Tick(Agent agent)
    {
        if (waitTimer > 0)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0.0f)
            {
                agent.MoveTo(worldWaypoints[index], GotoNextPoint);
            }
        }
    }

    int FindClosestWaypoint()
    {
        index = -1;
        float minDist = float.MaxValue;
        for (int i = 0; i < waypoints.Length; i++)
        {
            var pt = worldWaypoints[i];
            float d = Vector3.Distance(pt, transform.position);
            if (d < minDist)
            {
                minDist = d;
                index = i;
            }
        }

        return index;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        if (waypoints != null)
        {
            for (int i = 0; i < waypoints.Length; i++)
            {
                var w1 = waypoints[i];
                var w2 = waypoints[(i + 1) % waypoints.Length];
                if ((w1 != null) && (w2 != null))
                {
                    Gizmos.DrawLine(w1.position, w2.position);
                }
            }
        }
    }
}
