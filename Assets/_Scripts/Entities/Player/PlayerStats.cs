using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// a class to manage the stats of the player, health and stamina
public class PlayerStats : EntityStats
{
    // Define events for health and stamina changes
    public Action<(int, int), AudioSource> OnHealthChanged;
    public Action<(int, int)> OnStaminaChanged;
    public Action<Vector3, AudioSource> OnDeath;

    public Transform targetTransform;
    
    [SerializeField]
    private float regenRateStamina = 0.2f;

    [SerializeField]
    private bool isInvincible = false;

    private Ragdoll ragdoll;
    private Animator animator;
    private AudioSource audioSource;
    private int maxStamina;
    private int curStamina;
    private float staminaRegenTimer;

    protected new void Awake()
    {
        base.Awake();

        ragdoll = GetComponent<Ragdoll>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        maxHealth = Constants.BASE_VALUES[UpgradeType.Health];
        maxStamina = Constants.BASE_VALUES[UpgradeType.Stamina];
        
        #if UNITY_EDITOR
        if(!PlayerLoadout.isSet())
        {
            PlayerLoadout.reset();
            PlayerLoadout.addInventoryToLoadout(InventoryType.Health);
            PlayerLoadout.addInventoryToLoadout(InventoryType.Health);
            PlayerLoadout.addInventoryToLoadout(InventoryType.Stamina);
            PlayerLoadout.addInventoryToLoadout(InventoryType.Stamina);            
        }
        #endif

        maxHealth += PlayerLoadout.getUpgradeCount(UpgradeType.Health) * Constants.UPGRADE_VALUES[UpgradeType.Health];
        maxStamina += PlayerLoadout.getUpgradeCount(UpgradeType.Stamina) * Constants.UPGRADE_VALUES[UpgradeType.Stamina];
    } 

    protected new void OnEnable()
    {
        base.OnEnable();

        curStamina = maxStamina;
        staminaRegenTimer = regenRateStamina;
    }

    protected void Update()
    {
        staminaRegenTimer -= Time.deltaTime;
        if(staminaRegenTimer < 0)
        {
            regenStamina(1);
            staminaRegenTimer = regenRateStamina;
        }
    }

    public override bool takeDamage(int amount, Vector3 direction)
    {
        if(isInvincible)
        {
            return false;
        }
        
        bool died = base.takeDamage(amount, direction);

        OnHealthChanged?.Invoke(getHealthRatio(), audioSource);

        return died;
    }

    public override void heal(int amount)
    {
        base.heal(amount);
        OnHealthChanged?.Invoke(getHealthRatio(), audioSource);
    }

    public bool checkStamina(int stam)
    {
        return curStamina >= stam;
    }

    public void useStamina(int stam)
    {
        curStamina = Mathf.Clamp(curStamina - stam, 0, maxStamina);
        OnStaminaChanged?.Invoke(getStaminaRatio());
    }

    public void regenStamina(int stam)
    {
        curStamina = Mathf.Clamp(curStamina + stam, 0, maxStamina);
        OnStaminaChanged?.Invoke(getStaminaRatio());
    }

    public float getRegenRateStamina()
    {
        return regenRateStamina;
    }

    public (int, int) getStaminaRatio()
    {
        return (curStamina, maxStamina);
    }

    public void delayedStart()
    {
        OnHealthChanged?.Invoke(getHealthRatio(), audioSource);
        OnStaminaChanged?.Invoke(getStaminaRatio());
    }

    public override string getTag()
    {
        return "Player";
    }
    
    public override LayerMask getLayer()
    {
        return LayerMask.NameToLayer("PlayerHitBox");
    }

    protected override void die(Vector3 direction)
    {        
        base.die(direction);

        if(direction != Vector3.zero)
        {
            direction.y = 1f;
        }
        Vector3 dir = direction.normalized * 5f;
        if(ragdoll)
        {
            ragdoll.activateRagdoll();
            ragdoll.applyForce(dir);
        }
        else if(animator)
        {
            animator.SetTrigger("triggerDefeat");
            animator.SetLayerWeight(1, 1f); 
        }
        OnDeath?.Invoke(dir, audioSource);
    }
}
