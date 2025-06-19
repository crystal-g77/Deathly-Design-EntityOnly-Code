using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// store data about a tower under attack
public class AITowerUnderAttackEvent : AIBaseEvent
{
    public TowerBuilding tower {get; private set;}
    public Vector3 position {get; private set;}
    public bool spawnWave {get; private set;}

    public AITowerUnderAttackEvent(TowerBuilding tower, Vector3 position, bool spawnWave) : base(AIEventType.TowerUnderAttack)
    {
        this.tower = tower;
        this.position = position;
        this.spawnWave = spawnWave;
    }
}
