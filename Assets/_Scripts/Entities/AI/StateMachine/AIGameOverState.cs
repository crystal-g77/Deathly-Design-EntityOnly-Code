using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// an AIAgent state for an agent after the game has ended
public class AIGameOverState : AIState
{
    public bool result;

    public AIStateId getId()
    {
        return AIStateId.GameOver;
    }
 
    public void enter(AIAgent agent)
    {
        agent.navMeshAgent.ResetPath();
        agent.navMeshAgent.enabled = false;

        agent.weaponSlot.dropWeapon(Vector3.zero);
        // if(result)
        // {
        //     agent.ragdoll.activateRagdoll();
        //     agent.ragdoll.applyForce(Vector3.zero);
        // }
        // else
        // {
        //     if(agent.TryGetComponent<Animator>(out Animator animator))
        //     {
        //         animator.SetLayerWeight(1, 1f); 
        //         animator.SetTrigger("triggerVictory");
        //     }
        // }

        if(agent.TryGetComponent<Animator>(out Animator animator))
        {
            animator.SetLayerWeight(1, 1f); 
            if(result)
            {
                animator.SetTrigger("triggerDefeat");
            }
            else
            {
                animator.SetTrigger("triggerVictory");
            }
        }
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
