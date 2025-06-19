using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AIStateId 
{
    Idle,
    Patrol,
    PathToTarget,
    AttackPlayer,
    GuardPatrol,
    Death,
    GameOver,
}

public interface AIState 
{
    AIStateId getId();
    void enter(AIAgent agent);
    void update(AIAgent agent);
    void exit(AIAgent agent);
    void onAIEvent(AIAgent agent, AIBaseEvent aiEvent);
}
