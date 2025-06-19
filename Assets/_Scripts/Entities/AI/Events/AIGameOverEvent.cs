public class AIGameOverEvent : AIBaseEvent
{
    public bool result {get; private set;}

    public AIGameOverEvent(bool result) : base(AIEventType.GameOver)
    {      
        this.result = result;  
    }
}
