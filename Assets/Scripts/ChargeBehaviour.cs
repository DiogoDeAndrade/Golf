using NaughtyAttributes;
using System;
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
    [SerializeField]
    private bool          keepCharging = false;
    [SerializeField, ShowIf(nameof(keepCharging))]
    private Agent         parentAgent;
    
    public Transform chargeTarget { get; set; }

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
            agent.MoveTo(chargeTarget.position, CompleteCharge);
        }
        else
        {
            if (timeToCompleteCharge.Update())
            {
                if ((keepCharging) && (parentAgent.IsLOS(chargeTarget)))
                {
                    agent.SetSpeed(movementSpeed);
                    agent.MoveTo(chargeTarget.position, CompleteCharge);
                }
                else
                {
                    onCompleteCharge?.Invoke();
                }
            }
            else if (timeToCompleteCharge.isRunning)
            {
                if ((keepCharging) && (parentAgent.IsLOS(chargeTarget)))
                {
                    timeToCompleteCharge.Stop();
                    agent.SetSpeed(movementSpeed);
                    agent.MoveTo(chargeTarget.position, CompleteCharge);
                }
            }
        }
    }

    private void CompleteCharge(Agent agent, bool success)
    {
        if (success)
        {
            agent.PopupText("?");
            timeToCompleteCharge.Start();
        }
    }
}
