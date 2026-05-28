using NaughtyAttributes;
using System;
using System.Collections.Specialized;
using UC;
using UnityEngine;
using UnityEngine.AI;

[SelectionBase]
[RequireComponent(typeof(NavMeshAgent))]
public class Agent : MonoBehaviour
{
    [SerializeField] 
    private AgentBehaviour  startBehaviour;
    [SerializeField] 
    private bool            overrideRotation;
    [SerializeField, ShowIf(nameof(overrideRotation))]
    private float           maxRotationSpeed = 720.0f;
    [SerializeField, ShowIf(nameof(overrideRotation)), MinMaxSlider(0.0f, 360.0f)]
    private Vector2Int      randomInitialRotation;
    [SerializeField, MinMaxSlider(-50, 50)]
    private Vector2Int      randomPriority;


    NavMeshAgent agent;
    Animator        animator;

    AgentBehaviour  currentAgentBehaviour;
    Vector3?        currentTarget;
    Action<bool>    currentTargetCallback;

    public Vector3 spawnPos { get; private set; }

    void Start()
    {
        spawnPos = transform.position;
        animator = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();
        agent.avoidancePriority = randomPriority.Random();

        currentAgentBehaviour = startBehaviour;
        currentAgentBehaviour.Enter(this);

        if (overrideRotation)
        {
            agent.updateRotation = false;

            transform.rotation = Quaternion.Euler(0.0f, randomInitialRotation.Random(), 0.0f);
        }
    }

    void Update()
    {
        currentAgentBehaviour?.Tick(this);

        if (currentTarget != null)
        {
            if (agent.HasReachedDestination())
            {
                var callback = currentTargetCallback;
                currentTarget = null;
                currentTargetCallback = null;
                callback?.Invoke(true);
            }
        }

        if (overrideRotation)
        {
            Vector3 targetDirection = agent.desiredVelocity.x0z();
            if (targetDirection.magnitude > 1e-3)
            {
                targetDirection.Normalize();

                var targetRotation = Quaternion.LookRotation(targetDirection, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.deltaTime * maxRotationSpeed);
            }
        }

        animator.SetFloat("Speed", agent.velocity.magnitude);
    }

    public void MoveTo(Vector3 pos, Action<bool> callback)
    {
        StopMovement();

        currentTarget = pos;
        currentTargetCallback = callback;

        agent.SetDestination(pos);
    }

    public void StopMovement()
    {
        if (currentTarget != null)
        {
            currentTarget = null;
            currentTargetCallback?.Invoke(false);
            currentTargetCallback = null;
        }
    }

    public bool HasPath(Vector3 targetPos)
    {
        return agent.HasPath(transform.position, targetPos);
    }
}
