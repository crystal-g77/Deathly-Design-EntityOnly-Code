using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// a state machine for managing the state of AIAgents
public class AIStateMachine
{
    public AIState[] states;
    public AIAgent agent;
    public AIStateId currStateId;
    public AIStateId nextStateId;

    public AIStateMachine(AIAgent agent)
    {
        this.agent = agent;
        states = new AIState[System.Enum.GetValues(typeof(AIStateId)).Length];
    }

    public void registerState(AIState state)
    {
        int index = (int)state.getId();
        states[index] = state;
    }

    public AIState getState(AIStateId stateId)
    {
        return states[(int)stateId];
    }

    public void update() 
    {
        if(nextStateId != currStateId)
        {     
            // change to a new state if it is different from current   
            getState(currStateId)?.exit(agent);
            currStateId = nextStateId;
            getState(currStateId)?.enter(agent);
        }

        getState(currStateId)?.update(agent);
    }

    public void changeState(AIStateId newStateId)
    {
        if(newStateId == currStateId)
        {
            return;
        }

        nextStateId = newStateId;
    }

    // process events
    public void onAIEvent(AIBaseEvent aiEvent)
    {
        // let the state process how the AI will respond
        getState(currStateId)?.onAIEvent(agent, aiEvent);
    }
}
