// store data about a tower that has ended being under attack
public class AIEndTowerUnderAttackEvent : AIBaseEvent
{
    public TowerBuilding tower {get; private set;}

    public AIEndTowerUnderAttackEvent(TowerBuilding tower) : base(AIEventType.EndTowerUnderAttack)
    {
        this.tower = tower;
    }
}
