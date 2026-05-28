using UC;
using UnityEngine;

public class ChargeBehaviour : AgentBehaviour
{
    public delegate void OnCompleteCharge();
    public event OnCompleteCharge onCompleteCharge;

    [SerializeField]
    private CooldownTimer timeToCharge = 0.5f;
    [SerializeField]
    private CooldownTimer timeToCompleteCharge = 0.5f;

    float   chargeTimer;
    float   completeTimer;
    
    public Vector3 chargeTarget { get; set; }

    public override void Enter(Agent agent)
    {
        agent.PopupText("!");
        timeToCharge.Start();
    }

    public override void Exit(Agent agent)
    {
        
    }

    public override void Tick(Agent agent)
    {
        if (timeToCharge.Update())
        {
            agent.SetSpeed(movementSpeed);
            agent.MoveTo(chargeTarget, (agent, success) =>
            {
                timeToCompleteCharge.Start();
            });
        }

        if (timeToCompleteCharge.Update())
        {
            onCompleteCharge?.Invoke();
        }
    }
}
