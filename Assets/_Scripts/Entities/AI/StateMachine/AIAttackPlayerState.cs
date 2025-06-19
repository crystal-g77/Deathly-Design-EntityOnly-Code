using UnityEngine;
using UnityEngine.AI;

// an AIAgent state for when the agent is attacking the player
public class AIAttackPlayerState : AIState
{
    private Transform playerTransform;
    private float timer = 0.0f;

    public AIStateId getId()
    {
        return AIStateId.AttackPlayer;
    }

    public void enter(AIAgent agent)
    {
        playerTransform = GameManager.Instance.playerStats.targetTransform;
        Vector3 groundedPlayerPosition = playerTransform.position;
        groundedPlayerPosition.y = agent.transform.position.y;

        agent.navMeshAgent.stoppingDistance = agent.config.attackPlayerStoppingDistance;
        agent.navMeshAgent.speed = agent.config.attackPlayerSpeed;

        agent.weaponSlot.setTarget(playerTransform);

        float collisionRadius = 0f;
        NavMeshObstacle obstacle = playerTransform.GetComponentInParent<NavMeshObstacle>();
        if(obstacle)
        {
            collisionRadius = obstacle.radius + .1f;
        }

        updateDestination(agent, groundedPlayerPosition, collisionRadius);        
    }

    public void update(AIAgent agent)
    {        
        if(!agent.navMeshAgent.enabled)
        {
            return;
        }
        
        Vector3 groundedPlayerPosition = playerTransform.position;
        groundedPlayerPosition.y = agent.transform.position.y;

        float distance = (agent.transform.position - groundedPlayerPosition).magnitude;
        if (Mathf.Abs(distance) > agent.config.attackPlayerMaxDistance)
        {
            // if the agent has reached its destination and the player hasn't been spotted,
            // move to a patrolling state
            if(agent.isGuarding)
            {
                agent.stateMachine.changeState(AIStateId.GuardPatrol);     
            }
            else
            {
                agent.stateMachine.changeState(AIStateId.Patrol);  
            }    
            return;
        }

        float collisionRadius = 0f;
        NavMeshObstacle obstacle = playerTransform.GetComponentInParent<NavMeshObstacle>();
        if(obstacle)
        {
            collisionRadius = obstacle.radius + .1f;
        }

        updateDestination(agent, groundedPlayerPosition, collisionRadius);

        timer -= Time.deltaTime;
        if (timer < 0.0f)
        {
            agent.aiManager.aiEventNotify(new AIPlayerFoundEvent(agent, groundedPlayerPosition, collisionRadius));
            timer = agent.config.attackPlayerAlertTime;
        }

        updateRotation(agent, groundedPlayerPosition);

        updateAttacking(agent, collisionRadius);
    }

    public void exit(AIAgent agent)
    {
        agent.weaponSlot.setTarget(null);
        agent.weaponSlot.setAttacking(false);
        agent.navMeshAgent.stoppingDistance = 0f;

        playerTransform = null;
    }

    public void onAIEvent(AIAgent agent, AIBaseEvent aiEvent)
    {
        switch (aiEvent.type)
        {
            case AIBaseEvent.AIEventType.GameOver:
                AIGameOverEvent gameOverEvent = aiEvent as AIGameOverEvent;
                AIGameOverState gameOverState = agent.stateMachine.getState(AIStateId.GameOver) as AIGameOverState;
                gameOverState.result = gameOverEvent.result;
                agent.stateMachine.changeState(AIStateId.GameOver); 
                break;
        }
    }

    // update the destination
    private void updateDestination(AIAgent agent, Vector3 groundedPlayerPosition, float radius)
    {
        Vector3 directionToTarget = (agent.transform.position - groundedPlayerPosition).normalized;
        agent.targetPosition = groundedPlayerPosition + directionToTarget * radius;
        if(agent.isGuarding)
        {
            agent.targetPosition = clampDestination(agent, agent.targetPosition);
        }

        if(Mathf.Abs((agent.targetPosition - agent.navMeshAgent.destination).magnitude) > 1f
            && Mathf.Abs((agent.transform.position - agent.targetPosition).magnitude) > agent.navMeshAgent.stoppingDistance)
        {
            // if the target destination is different enough from the currently set destination
            // and the agent is greater thatn the stopping distance away from the target destination
            // update the current destination
            agent.navMeshAgent.SetDestination(agent.targetPosition);
        }
    }

    // make the AIAgent face the player
    private void updateRotation(AIAgent agent, Vector3 groundedPlayerPosition)
    {
        Vector3 directionToTarget = (groundedPlayerPosition - agent.transform.position).normalized;

        float angle = Vector3.Angle(agent.transform.forward, directionToTarget);

        if (Mathf.Abs(angle) > agent.config.maxAngleToPlayer)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            agent.transform.rotation = Quaternion.Slerp(agent.transform.rotation,
                targetRotation,
                Time.deltaTime * agent.config.rotationSpeedToPlayer);
        }
    }

    // Restrict the destination to within the leash radius
    private Vector3 clampDestination(AIAgent agent, Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - agent.guardBuilding.transform.position;
        if (direction.magnitude > agent.guardRadius)
        {
            direction = direction.normalized * agent.guardRadius;
        }
        return agent.guardBuilding.transform.position + direction;
    }

    // if the player is in sight, beginning attacking
    // if not, try to move closer to the player
    private void updateAttacking(AIAgent agent, float radius)
    {
        float distanceToPlayer = Mathf.Abs((agent.transform.position - playerTransform.position).magnitude);
        if (agent.sensor.isInSight(playerTransform.gameObject) && distanceToPlayer <= agent.config.attackPlayerStoppingDistance + radius)
        {
            agent.navMeshAgent.stoppingDistance = agent.config.attackPlayerStoppingDistance;
            if(agent.isGuarding)
            {
                // Adjust stopping distance if the destination was clamped
                float distanceToGuardPosition = (playerTransform.position - agent.guardBuilding.transform.position).magnitude;
                float distanceToClampedDestination = (agent.guardBuilding.transform.position - agent.navMeshAgent.destination).magnitude;
                agent.navMeshAgent.stoppingDistance = Mathf.Max(0, agent.config.attackPlayerStoppingDistance - Mathf.Abs(distanceToGuardPosition - distanceToClampedDestination));
            }
            
            if (!agent.weaponSlot.isAttacking())
            {
                agent.weaponSlot.setAttacking(true);
            }
        }
        else
        {
            agent.navMeshAgent.stoppingDistance -= 1f;
            if(agent.navMeshAgent.stoppingDistance < 0)
            {
                agent.navMeshAgent.stoppingDistance = 0f;
            }

            if (agent.weaponSlot.isAttacking())
            {
                agent.weaponSlot.setAttacking(false);
            }
        }
    }
}