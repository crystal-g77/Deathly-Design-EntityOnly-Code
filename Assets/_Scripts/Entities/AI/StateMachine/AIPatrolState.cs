using UnityEngine;
using UnityEngine.AI;

// an AIAgent state for patrolling agents
public class AIPatrolState : AIState
{
    public AIStateId getId()
    {
        return AIStateId.Patrol;
    }
 
    public void enter(AIAgent agent)
    {
        agent.navMeshAgent.speed = agent.config.patrolSpeed;
        agent.navMeshAgent.stoppingDistance = 0f;
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

        if(agent.navMeshAgent.pathPending)
        {
            return;
        }

        if(!agent.navMeshAgent.hasPath || agent.navMeshAgent.pathStatus == NavMeshPathStatus.PathInvalid)
        {
            wander(agent);
        }
    }

    public void exit(AIAgent agent)
    {
    }
    
    public void onAIEvent(AIAgent agent, AIBaseEvent aiEvent)
    {
        switch(aiEvent.type)
        {
            case AIBaseEvent.AIEventType.PlayerFound:
                AIPlayerFoundEvent playerFoundEvent = aiEvent as AIPlayerFoundEvent;
                if((playerFoundEvent.position - agent.transform.position).magnitude <= agent.config.alertRadius)
                {
                    agent.targetPosition = agent.getPositionOutsideObstacleRadius(playerFoundEvent.position, playerFoundEvent.collisionRadius);
                    agent.stateMachine.changeState(AIStateId.PathToTarget);
                }
                break;

            case AIBaseEvent.AIEventType.TowerUnderAttack:
                AITowerUnderAttackEvent towerUnderAttackEvent = aiEvent as AITowerUnderAttackEvent;
                agent.targetPosition = agent.getRandomOffsetPosition(towerUnderAttackEvent.position, Constants.AI_TOWER_UNDER_ATTACK_POSITION_VARIATION);
                agent.stateMachine.changeState(AIStateId.PathToTarget);
                break;
            
            case AIBaseEvent.AIEventType.GameOver:
                AIGameOverEvent gameOverEvent = aiEvent as AIGameOverEvent;
                AIGameOverState gameOverState = agent.stateMachine.getState(AIStateId.GameOver) as AIGameOverState;
                gameOverState.result = gameOverEvent.result;
                agent.stateMachine.changeState(AIStateId.GameOver);
                break;
        }
    }

    private void wander(AIAgent agent)
    {
        WorldBounds worldBounds = GameObject.FindAnyObjectByType<WorldBounds>();
        if(!worldBounds)
        {
            throw new System.Exception("Need to know the bounds of the world");
        }        

        agent.navMeshAgent.SetDestination(worldBounds.getValidPosition());
    }
}
