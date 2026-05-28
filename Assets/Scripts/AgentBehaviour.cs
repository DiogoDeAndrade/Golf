using UnityEngine;

[RequireComponent(typeof(Agent))]
public abstract class AgentBehaviour : MonoBehaviour
{
    public abstract void Enter(Agent agent);
    public abstract void Exit(Agent agent);
    public abstract void Tick(Agent agent);
}
