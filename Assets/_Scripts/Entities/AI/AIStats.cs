using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations;
using UnityEngine.Assertions;

public class AIStats : EntityStats
{
    [SerializeField]
    private float knockBackDistance = 2f; // distance to knock back
    [SerializeField]
    private float knockBackSpeed = 5f;    // speed to knock back
    private AIAgent agent;

    // Start is called before the first frame update
    protected new void Awake()
    {
        base.Awake();

        agent = GetComponent<AIAgent>();
    }    

    // override takeDamage in EntityStats to add AI hit notification and knockback
    public override bool takeDamage(int amount, Vector3 direction)
    {
        bool died = base.takeDamage(amount, direction);

        agent.aiHitNotify();

        if(!died && direction != Vector3.zero && TryGetComponent<NavMeshAgent>(out NavMeshAgent navMeshAgent))
        {
            knockBack(navMeshAgent, direction);
        }

        return died;
    }

    public override string getTag()
    {
        return "Enemy";
    }
    
    public override LayerMask getLayer()
    {
        return LayerMask.NameToLayer("EnemyHitBox");
    }
    
    public override LayerMask getDeadLayer()
    {
        return LayerMask.NameToLayer("DeadEnemyHitBox");
    }

    protected override void die(Vector3 direction)
    {
        base.die(direction);
        
        AIDeathState deathState = agent.stateMachine.getState(AIStateId.Death) as AIDeathState;
        deathState.direction = direction;
        agent.stateMachine.changeState(AIStateId.Death);
    }

    private void knockBack(NavMeshAgent navMeshAgent, Vector3 direction)
    {
        // Normalize the direction vector and reverse it to push the agent away
        Vector3 knockBackDirection = direction.normalized;

        // Calculate the knockback vector
        Vector3 knockBackVector = knockBackDirection * knockBackDistance;

        // Apply the knockback using NavMeshAgent.Move
        StartCoroutine(applyKnockBack(navMeshAgent, knockBackVector, knockBackSpeed));
    }

    private IEnumerator applyKnockBack(NavMeshAgent navMeshAgent, Vector3 knockBackVector, float speed)
    {
        float elapsedTime = 0f;
        float knockBackTime = knockBackVector.magnitude / speed;

        Assert.IsNotNull(navMeshAgent, "");

        while (elapsedTime < knockBackTime)
        {
            float deltaTime = Time.deltaTime;
            elapsedTime += deltaTime;

            // Move the agent manually using NavMeshAgent.Move
            navMeshAgent.Move(knockBackVector * (deltaTime / knockBackTime));
            yield return null;
        }
    }
}
