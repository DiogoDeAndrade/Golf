using NaughtyAttributes;
using System;
using UC;
using UC.RPG;
using UnityEngine;
using UnityEngine.AI;

[SelectionBase]
[RequireComponent(typeof(NavMeshAgent))]
public class Agent : MonoBehaviour, IConditionalObstacle
{
    [Header("Agent")]
    [SerializeField] 
    protected bool          overrideRotation = true;
    [SerializeField, ShowIf(nameof(overrideRotation))]
    protected float         maxRotationSpeed = 720.0f;
    [SerializeField, ShowIf(nameof(overrideRotation)), MinMaxSlider(0, 360)]
    protected Vector2Int    randomInitialRotation = new Vector2Int(0, 360);
    [SerializeField, MinMaxSlider(-50, 50)]
    protected Vector2Int    randomPriority = new Vector2Int(-20, 20);
    [SerializeField]
    protected RectTransform emoteContainer;
    [SerializeField]
    protected PopupText     popupTextPrefab;
    [SerializeField]
    protected ParticleSystem    dirtPS;
    [SerializeField, ShowIf(nameof(hasDirtPS))]
    protected float             minSpeedDirtPS;
    [SerializeField]
    protected bool          damageOnTouch;
    [SerializeField, ShowIf(nameof(damageOnTouch))]
    protected int           damage = 1;
    [SerializeField, ShowIf(nameof(damageOnTouch))]
    protected float         knockbackStrength = 2.0f;
    [SerializeField, ShowIf(nameof(damageOnTouch))]
    protected float         collisionCooldown = 1.0f;

    protected NavMeshAgent    agent;
    protected Animator        animator;
    protected float           collisionDisableTimer;

    Vector3?            currentTarget;
    Action<Agent, bool> currentTargetCallback;

    bool hasDirtPS => dirtPS != null;

    public Vector3 spawnPos { get; protected set; }

    protected virtual void Start()
    {
        spawnPos = transform.position;
        animator = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();
        agent.avoidancePriority = randomPriority.Random();

        if (overrideRotation)
        {
            agent.updateRotation = false;

            transform.rotation = Quaternion.Euler(0.0f, randomInitialRotation.Random(), 0.0f);
        }
    }

    protected virtual void Update()
    {
        if (currentTarget != null)
        {
            if (agent.HasReachedDestination())
            {
                var callback = currentTargetCallback;
                currentTarget = null;
                currentTargetCallback = null;
                callback?.Invoke(this, true);
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

        if (collisionDisableTimer > 0)
        {
            collisionDisableTimer -= Time.deltaTime;
        }

        if (dirtPS)
        {
            dirtPS.SetEmission(agent.velocity.magnitude > minSpeedDirtPS);
        }
    }

    public void MoveTo(Vector3 pos, Action<Agent, bool> callback)
    {
        StopMovement();

        currentTarget = pos;
        currentTargetCallback = callback;

        agent.SetDestination(pos);
        agent.isStopped = false;
    }

    public void StopMovement()
    {
        if (currentTarget != null)
        {
            agent.isStopped = true;
            currentTarget = null;
            currentTargetCallback?.Invoke(this, false);
            currentTargetCallback = null;
        }
    }

    public void SetSpeed(float s)
    {
        agent.speed = s;
    }

    public bool HasPath(Vector3 targetPos)
    {
        return agent.HasPath(transform.position, targetPos);
    }

    public void PopupText(string text)
    {
        var txt = Instantiate(popupTextPrefab, emoteContainer);
        txt.SetText(text);
    }


    private void OnTriggerEnter(Collider collider)
    {
        if (damageOnTouch)
        {
            // Check if touched the player
            var ball = collider.GetComponent<Ball>();
            if (ball)
            {
                var hs = collider.FindResourceHandler(Globals.healthResource);
                if (hs)
                {
                    hs.Change(new ChangeData(-damage)
                    {
                        changeSrcPosition = transform.position,
                        changeSrcDirection = (collider.transform.position - transform.position).x0z().normalized,
                        source = gameObject,
                        knockbackStrength = knockbackStrength,
                    });
                }
                collisionDisableTimer = collisionCooldown;
            }
        }
    }

    public bool ShouldIgnoreCollision(BallPhysics ball)
    {
        return (collisionDisableTimer > 0.0f);
    }
}
