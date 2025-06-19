using UnityEngine;

// an AIAgent state for a dead agent
public class AIDeathState : AIState
{
    public Vector3 direction;
    
    public AIStateId getId()
    {
        return AIStateId.Death;
    }
 
    public void enter(AIAgent agent)
    {
        agent.aiManager.aiDeathNotify(agent);
        
        agent.navMeshAgent.ResetPath();
        agent.navMeshAgent.enabled = false;

        agent.ragdoll.activateRagdoll();
        if(direction != Vector3.zero)
        {
            direction.y = 1f;
        }
        Vector3 dir = direction.normalized * agent.config.deathForce;
        agent.ragdoll.applyForce(dir);

        agent.weaponSlot.dropWeapon(dir/5f);
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
