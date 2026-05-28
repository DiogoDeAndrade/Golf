using UnityEngine;

[RequireComponent(typeof(Agent))]
public abstract class AgentBehaviour : MonoBehaviour
{
    [Header("NavMeshAgent parameters")]
    [SerializeField] protected float movementSpeed = 2.0f;

    public abstract void Enter(Agent agent);
    public abstract void Exit(Agent agent);
    public abstract void Tick(Agent agent);
}
