using UnityEngine;
using UnityEngine.AI;

// a class representing an enemy
public class AIAgent : MonoBehaviour
{
    public AIStateConfig config; // contains configurable data about agents
    public AIWeaponType weapon = AIWeaponType.AssaultRifle; // the weapon to equip on the agent
    public Transform aimAt; // the tranform to aim at

    [HideInInspector]
    public Vector3 targetPosition; // the position to path to

    [HideInInspector]
    public bool isGuarding = false;
    [HideInInspector]
    public DefaultBuilding guardBuilding; // the position to guard
    [HideInInspector]
    public float guardRadius; // the leash radius on guard position

    [HideInInspector]
    public AIAgentType type; // the type of agent

    public AIManager aiManager { get; private set;}    
    public NavMeshAgent navMeshAgent { get; private set;}
    public Ragdoll ragdoll { get; private set;}
    public AIWeaponSlot weaponSlot { get; private set;}
    public AISensor sensor { get; private set;}
    public AudioSource audioSource { get; private set;}
    public AIStateMachine stateMachine { get; private set;}

    // Start is called before the first frame update
    void Awake()
    {     
        aiManager = AIManager.Instance;
        navMeshAgent = GetComponent<NavMeshAgent>();
        ragdoll = GetComponent<Ragdoll>();
        weaponSlot = GetComponentInChildren<AIWeaponSlot>();
        sensor = GetComponent<AISensor>();
        audioSource = GetComponent<AudioSource>();
        
        stateMachine = new AIStateMachine(this);
        stateMachine.registerState(new AIIdleState());
        stateMachine.registerState(new AIPatrolState());
        stateMachine.registerState(new AIPathToTargetState());
        stateMachine.registerState(new AIAttackPlayerState());
        stateMachine.registerState(new AIGuardPatrolState());
        stateMachine.registerState(new AIDeathState());
        stateMachine.registerState(new AIGameOverState());

        navMeshAgent.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(gameObject.activeSelf)
        {
            stateMachine.update();  
        }   
    }

    void OnEnable() 
    {
        ragdoll.deactivateRagdoll(); 

        navMeshAgent.enabled = true;  
        navMeshAgent.avoidancePriority = Random.Range(20, 71);

        Invoke(nameof(setupWeapon), .001f);
    }

    void OnDisable() 
    {
        navMeshAgent.enabled = false;

        isGuarding = false;

        stateMachine.changeState(AIStateId.Idle);    

        aiManager.unregister(this);   

        weaponSlot.cleanupDroppedWeapon();
    }

    void OnDestroy()
    {
        weaponSlot.setAimTarget(null);
    }

    // pass an AIBaseEvent to the state machine
    public void onAIEvent(AIBaseEvent aiEvent)
    {
        AIAgentInitiatedEvent agentInitEvent = aiEvent as AIAgentInitiatedEvent;
        if(agentInitEvent != null)
        {
            if(agentInitEvent.initiatingAgent != this)
            {
                stateMachine.onAIEvent(agentInitEvent);
            }
        }
        else
        {
            stateMachine.onAIEvent(aiEvent);
        }
    }

    // check if the player is in our sensor
    public GameObject getPlayerInSight()
    {
        if(sensor.inSightObjects?.Count > 0)
        {
            GameObject[] buffer = new GameObject[1];
            int count = sensor.filter(buffer, "Character");
            if(count > 0)
            {
                return buffer[0];
            }
        }

        return null;
    }

    public void aiHitNotify()
    {
        aiManager.aiHitNotify(this);
    }    

    public Vector3 getRandomOffsetPosition(Vector3 pos, float offset)
    {
        WorldBounds worldBounds = GameObject.FindAnyObjectByType<WorldBounds>();
        if(!worldBounds)
        {
            throw new System.Exception("Need to know the bounds of the world");
        }  
        
        // Get bounds around position
        Vector3 minPos = new Vector3(
            pos.x - offset, 
            0f, 
            pos.z - offset
        );
        Vector3 maxPos = new Vector3(
            pos.x + offset, 
            0f, 
            pos.z + offset
        );

        return worldBounds.getValidPosition(minPos, maxPos);
    }

    public Vector3 getPositionOutsideObstacleRadius(Vector3 targetPos, float radius)
    {
        WorldBounds worldBounds = GameObject.FindAnyObjectByType<WorldBounds>();
        if(!worldBounds)
        {
            throw new System.Exception("Need to know the bounds of the world");
        }  
        
        return worldBounds.getValidPositionOutsideObstacleRadius(transform.position, targetPos, radius, Constants.AI_MIN_DESTINATION_RESET_DISTANCE);
    }

    private void setupWeapon()
    {
        weaponSlot.equipWeapon( new WeaponTypeUnion(weapon));
        weaponSlot.setAimTarget(aimAt);
        Vector3 aimPos = weaponSlot.transform.position;
        aimPos.z += .5f;
        aimAt.transform.position = aimPos;
    }

    private void OnDrawGizmos()
    {
        if(isGuarding)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(guardBuilding.transform.position, .1f);

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(guardBuilding.transform.position, guardRadius);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, config.alertRadius);
    }
}
