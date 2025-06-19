using UnityEngine;

[CreateAssetMenu(fileName = "AIDefaultConfig", menuName = "AI/AIConfig")]
public class AIStateConfig : ScriptableObject
{
    public int biomassValue = 25;
    public float deathForce = 40f;
    public float patrolSpeed = 4f;
    public float alertRadius = 25f;
    public float pathToTargetSpeed = 6f;
    public float maxAngleToPlayer = 20f;
    public float rotationSpeedToPlayer = 90f;
    public float attackPlayerSpeed = 5f;
    public float attackPlayerAlertTime = 1f;
    public float attackPlayerStoppingDistance = 5f;
    public float attackPlayerMaxDistance = 15f;
    public float guardPatrolInterval = 5f;

}
