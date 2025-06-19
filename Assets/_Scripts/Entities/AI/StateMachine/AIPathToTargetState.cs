using UnityEngine;
using UnityEngine.AI;

// an AIAgent state for an agent that is pathing to a target
public class AIPathToTargetState : AIState
{
    private bool isRotating = false;
    private float rotationProgress = 0f; // Tracks how much the agent has rotated
    private float pathUpdateDelay = 0.3f; // Time to wait before checking if the path is complete
    private float pathCheckTimer = 0f; // Timer to track delay

    public AIStateId getId()
    {
        return AIStateId.PathToTarget;
    }

    // enter the state
    public void enter(AIAgent agent)
    {
        agent.navMeshAgent.speed = agent.config.pathToTargetSpeed;
        agent.navMeshAgent.SetDestination(agent.targetPosition);

        pathCheckTimer = pathUpdateDelay;

        isRotating = false; // Ensure rotation starts fresh
        rotationProgress = 0f;
    }

    // update the state
    public void update(AIAgent agent)
    {
        if(!agent.navMeshAgent.enabled)
        {
            return;
        }

        // if the player is in sight, move to an attack state
        if(agent.getPlayerInSight())
        {
            agent.stateMachine.changeState(AIStateId.AttackPlayer); 
            return;
        }

        // If currently rotating, handle the rotation
        if (isRotating)
        {
            rotate360(agent);
            return;
        }

        if (agent.navMeshAgent.pathPending)
        {
            return; // Keep waiting if the path is still pending
        }

        // If the path is calculated, we proceed with the checks
        if (pathCheckTimer > 0f)
        {
            pathCheckTimer -= Time.deltaTime; // Countdown to allow path updates
            return; // Wait for the countdown to finish before proceeding
        }
        
        // Check if the path is invalid
        if (!agent.navMeshAgent.hasPath || agent.navMeshAgent.pathStatus == NavMeshPathStatus.PathInvalid)
        {
            // If no path or invalid path, stop and go back to patrol      
            if (agent.isGuarding)
            {
                agent.stateMachine.changeState(AIStateId.GuardPatrol);
            }
            else
            {
                agent.stateMachine.changeState(AIStateId.Patrol);
            }
            return;
        }

        // Use a small buffer to account for precision issues
        float distanceThreshold = 0.2f; // You can adjust this value to fit your needs
        if (agent.navMeshAgent.pathStatus == NavMeshPathStatus.PathComplete &&
            agent.navMeshAgent.remainingDistance <= agent.navMeshAgent.stoppingDistance + distanceThreshold)
        {
            // The agent has reached its destination, so trigger rotation logic
            isRotating = true;
            rotationProgress = 0f;
        }
    }

    // exit the state
    public void exit(AIAgent agent)
    {
        isRotating = false; // Ensure rotation starts fresh
        rotationProgress = 0f;
    }
    
    // process events
    public void onAIEvent(AIAgent agent, AIBaseEvent aiEvent)
    {     
        switch(aiEvent.type)
        {    
            case AIBaseEvent.AIEventType.PlayerFound:
                AIPlayerFoundEvent playerFoundEvent = aiEvent as AIPlayerFoundEvent;
                bool withinRange = Mathf.Abs((playerFoundEvent.position - agent.transform.position).magnitude) <= Constants.AI_PLAYER_FOUND_MOVE_TO_ATTACK_DISTANCE;
                if(withinRange)
                {
                    // the AI is close enough to the player, force them into attack state
                    agent.stateMachine.changeState(AIStateId.AttackPlayer); 
                    break;
                }

                withinRange = Mathf.Abs((playerFoundEvent.position - agent.transform.position).magnitude) <= agent.config.alertRadius;
                if(agent.isGuarding)
                {
                    withinRange = withinRange && (Mathf.Abs((playerFoundEvent.position - agent.guardBuilding.transform.position).magnitude) <= agent.guardRadius + 1f);
                }

                if(withinRange)
                {
                    if(Mathf.Abs((playerFoundEvent.position - agent.targetPosition).magnitude) > Constants.AI_MIN_DESTINATION_RESET_DISTANCE)
                    {
                        // only update destination if it is far enough away from the current one,
                        // don't generate a new path unless necessary
                        Vector3 pos = agent.getPositionOutsideObstacleRadius(playerFoundEvent.position, playerFoundEvent.collisionRadius);
                        agent.navMeshAgent.SetDestination(pos);
                        agent.targetPosition = pos;

                        pathCheckTimer = pathUpdateDelay;
                    }
                }
                break;

            case AIBaseEvent.AIEventType.TowerUnderAttack:
                if(!agent.isGuarding)
                {
                    AITowerUnderAttackEvent towerUnderAttackEvent = aiEvent as AITowerUnderAttackEvent;
                    float minDistance = Mathf.Sqrt(2f * Mathf.Pow(Constants.AI_TOWER_UNDER_ATTACK_POSITION_VARIATION, 2));
                    if(Mathf.Abs((towerUnderAttackEvent.position - agent.targetPosition).magnitude) > minDistance)
                    {
                        // only update destination if it is far away from the current one,
                        // don't generate a new path unless necessary
                        Vector3 pos = agent.getRandomOffsetPosition(towerUnderAttackEvent.position, Constants.AI_TOWER_UNDER_ATTACK_POSITION_VARIATION);
                        agent.navMeshAgent.SetDestination(pos);
                        agent.targetPosition = pos;
                        
                        pathCheckTimer = pathUpdateDelay;
                    }
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

    private void rotate360(AIAgent agent)
    {
        // Rotate the agent incrementally
        float rotationThisFrame = agent.config.rotationSpeedToPlayer * Time.deltaTime;
        rotationProgress += rotationThisFrame;
        agent.transform.Rotate(0, rotationThisFrame, 0);

        // Check if rotation is complete
        if (rotationProgress >= 360f)
        {
            // End rotation and transition to the appropriate patrol state
            isRotating = false;
            if (agent.isGuarding)
            {
                agent.stateMachine.changeState(AIStateId.GuardPatrol);
            }
            else
            {
                agent.stateMachine.changeState(AIStateId.Patrol);
            }
        }
    }
}
