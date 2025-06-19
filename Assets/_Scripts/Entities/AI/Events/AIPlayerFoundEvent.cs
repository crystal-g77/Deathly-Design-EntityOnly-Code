using UnityEngine;

// store data about the player being found
public class AIPlayerFoundEvent : AIAgentInitiatedEvent
{
    public Vector3 position {get; private set;}
    public float collisionRadius {get; private set;}

    public AIPlayerFoundEvent(AIAgent agent, Vector3 position, float radius) : base(AIEventType.PlayerFound, agent)
    {      
        this.position = position;  
        this.collisionRadius = radius;
    }
}
