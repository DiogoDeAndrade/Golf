using NaughtyAttributes;
using System.Collections.Generic;
using UC;
using UnityEngine;

public class AgentFSM : Agent
{
    [HorizontalLine(color: EColor.Orange)]
    [Header("FSM")]
    [SerializeField]
    protected AgentBehaviour startBehaviour;

    protected AgentBehaviour        currentBehaviour;
    protected List<AgentBehaviour>  prevBehaviours = new();

    protected override void Start()
    {
        base.Start();

        ResetBehaviour();
    }

    // Update is called once per frame
    protected override void Update()
    {
        currentBehaviour?.Tick(this);

        base.Update();
    }

    public void SetBehaviour(AgentBehaviour behaviour)
    {
        if (currentBehaviour == behaviour) return;
        if (currentBehaviour != null)
        {
            if (prevBehaviours.Count > 10) prevBehaviours.PopFirst();
            prevBehaviours.Add(currentBehaviour);

            currentBehaviour.Exit(this);
        }
        currentBehaviour = behaviour;
        currentBehaviour.Enter(this);
    }

    public void SetPreviousBehaviour()
    {
        if (prevBehaviours.Count > 0)
        {
            var prev = prevBehaviours.PopLast();
            SetBehaviour(prev);
        }
    }

    public void ResetBehaviour()
    {
        SetBehaviour(startBehaviour);
    }
}
