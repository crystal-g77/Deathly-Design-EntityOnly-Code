using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// an AIAgent state for an idle agent
public class AIIdleState : AIState
{
    public AIStateId getId()
    {
        return AIStateId.Idle;
    }
 
    public void enter(AIAgent agent)
    {
        agent.navMeshAgent.ResetPath();
    }

    public void update(AIAgent agent)
    {
    }    

    public void exit(AIAgent agent)
    {
    }
    
    public void onAIEvent(AIAgent agent, AIBaseEvent aiEvent)
    {        
    }
}
