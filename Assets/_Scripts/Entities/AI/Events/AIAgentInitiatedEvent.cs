// store data about an event that has been initiated by an AIAgent
public abstract class AIAgentInitiatedEvent : AIBaseEvent
{
    public AIAgent initiatingAgent {get; private set;}

    public AIAgentInitiatedEvent(AIEventType type, AIAgent initiatingAgent) : base(type)
    {      
        this.initiatingAgent = initiatingAgent;  
    }
}
