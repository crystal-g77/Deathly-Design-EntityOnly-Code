public abstract class AIBaseEvent 
{
    public enum AIEventType
    {
        PlayerFound,
        TowerUnderAttack,
        EndTowerUnderAttack,
        GameOver,
    }

    public AIEventType type {get; private set;}

    public AIBaseEvent(AIEventType type)
    {
        this.type = type;
    }
}
