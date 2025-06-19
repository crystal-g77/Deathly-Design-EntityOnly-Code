using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIGuardPatrolState : AIState
{
    private float patrolTimer = 0f;

    public AIStateId getId()
    {
        return AIStateId.GuardPatrol;
    }
 
    public void enter(AIAgent agent)
    {
        agent.navMeshAgent.speed = agent.config.patrolSpeed;
        // Set the initial guard point (where the AI starts guarding)
        agent.navMeshAgent.SetDestination(agent.targetPosition); // Move to the guard point
        agent.navMeshAgent.stoppingDistance = 0f;  // No stopping distance, so the AI can patrol freely
        patrolTimer = 0f;
    }

    public void update(AIAgent agent)
    {
        if(!agent.navMeshAgent.enabled)
        {
            return;
        }

        if(agent.getPlayerInSight())
        {
            agent.stateMachine.changeState(AIStateId.AttackPlayer);
            return;
        }

        // Update the patrol timer for random movement within the leash radius
        patrolTimer += Time.deltaTime;
        
        // Check if the agent needs to change its patrol location
        if (patrolTimer >= agent.config.guardPatrolInterval)
        {
            patrolTimer = 0f;
            wander(agent);
        }

        if(agent.navMeshAgent.velocity.magnitude < 0.1f)
        {
            Quaternion targetRotation = agent.transform.rotation * Quaternion.Euler(0, agent.config.rotationSpeedToPlayer * Time.deltaTime, 0);
            agent.transform.rotation = Quaternion.RotateTowards(agent.transform.rotation, targetRotation, agent.config.rotationSpeedToPlayer * Time.deltaTime);
        }
    }    

    public void exit(AIAgent agent)
    {
        // Perform any necessary cleanup when exiting the guard state
        //agent.navMeshAgent.ResetPath(); // Reset path if needed
    }
    
    public void onAIEvent(AIAgent agent, AIBaseEvent aiEvent)
    {        
        switch(aiEvent.type)
        {
            case AIBaseEvent.AIEventType.PlayerFound:
                AIPlayerFoundEvent playerFoundEvent = aiEvent as AIPlayerFoundEvent;
                if(((playerFoundEvent.position - agent.transform.position).magnitude <= agent.config.alertRadius)
                    && ((playerFoundEvent.position - agent.guardBuilding.transform.position).magnitude <= agent.guardRadius + 1f))
                {
                    agent.targetPosition = agent.getPositionOutsideObstacleRadius(playerFoundEvent.position, playerFoundEvent.collisionRadius);
                    agent.stateMachine.changeState(AIStateId.PathToTarget);
                }
                break;

            case AIBaseEvent.AIEventType.GameOver:
                AIGameOverEvent gameOverEvent = aiEvent as AIGameOverEvent;
                AIGameOverState gameOverState = agent.stateMachine.getState(AIStateId.GameOver) as AIGameOverState;
                gameOverState.result = gameOverEvent.result;
                agent.stateMachine.changeState(AIStateId.GameOver); 
                break;
        }
    }    

    // pick a random point within leash and path to it
    private void wander(AIAgent agent)
    {
        WorldBounds worldBounds = GameObject.FindAnyObjectByType<WorldBounds>();
        if(!worldBounds)
        {
            throw new System.Exception("Need to know the bounds of the world");
        }   

        Vector3 minPos = new Vector3(
            agent.guardBuilding.transform.position.x - agent.guardRadius, 
            agent.guardBuilding.transform.position.y, 
            agent.guardBuilding.transform.position.z - agent.guardRadius
        );
        Vector3 maxPos = new Vector3(
            agent.guardBuilding.transform.position.x + agent.guardRadius, 
            agent.guardBuilding.transform.position.y, 
            agent.guardBuilding.transform.position.z + agent.guardRadius
        );     

        Vector3 potentialDestination = worldBounds.getValidPosition(minPos, maxPos);
        if(potentialDestination == Vector3.zero)
        {
            potentialDestination = agent.guardBuilding.transform.position;
        }

        agent.navMeshAgent.SetDestination(potentialDestination);
    }
}
