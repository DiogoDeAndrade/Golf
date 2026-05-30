using NaughtyAttributes;
using UnityEngine;

public class AgentFSM : Agent
{
    [HorizontalLine(color: EColor.Orange)]
    [Header("FSM")]
    [SerializeField]
    protected AgentBehaviour startBehaviour;

    protected AgentBehaviour currentBehaviour;

    protected override void Start()
    {
        base.Start();

        currentBehaviour = startBehaviour;
        currentBehaviour.Enter(this);

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
            currentBehaviour.Exit(this);
        }
        currentBehaviour = behaviour;
        currentBehaviour.Enter(this);
    }
}
