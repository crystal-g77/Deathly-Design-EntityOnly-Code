using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof (CharacterController))]
public class PlayerLocomotion : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField]
    private float groundSpeed = 3f; 
    [SerializeField]
    private float smoothingTime = 0.1f; // Time to smooth the transition
    [SerializeField]
    private float gravity = 9.81f; 
    
    [Header("Dashing")]
    [SerializeField]
    private DamageSO dashDamageSO; // damage of dash
    [SerializeField]
    private float dashRadius = 0.5f; // damage radius of dash
     [SerializeField]
    private LayerMask dashLayerMask;

    private CharacterController controller;
    private Animator animator;
    private PlayerStats playerStats;   
    private GameManager gameManager;
    private InputManager inputManager;
    private Vector2 currDirection;
    private Vector2 smoothDirectionVelocity;
    private bool isDashing;
    private float dashMultiplier = Constants.BASE_DASH_SPEED_MULTIPLIER;
    private float dashDuration = Constants.BASE_DASH_DURATION;
    private List<IDamageable> damageables;
    private bool isInAir = false;
    private Vector3 velocity;

    // a class for managing player movement
    private void Start()
    {
        controller = GetComponent<CharacterController>();   
        animator = GetComponent<Animator>(); 
        
        gameManager = GameManager.Instance;
        gameManager.OnGameOver += onGameOver;
        
        inputManager = InputManager.Instance;
        inputManager.OnDashTriggered += onDashTriggered;

        playerStats = gameManager.playerStats;

        isDashing = false;
        damageables = new List<IDamageable>();
    }

    private void OnDestroy() 
    {        
        gameManager.OnGameOver -= onGameOver;
        inputManager.OnDashTriggered -= onDashTriggered;
    }

    private void Update()
    {
        if(gameManager.isGamePaused())
        {
            return;
        }

        // update animator locomotion blend tree to reflect movement
        // since we switched away from a humanoid monster this doesn't actually do
        // anything anymore as the entire blend tree is just the idle animation
        // also update current move direction from input 
        Vector2 targetDirection = inputManager.moveInput;  
        currDirection = Vector2.SmoothDamp(currDirection, targetDirection, ref smoothDirectionVelocity, smoothingTime);     

        animator.SetFloat("InputX", currDirection.x);
        animator.SetFloat("InputY", currDirection.y);
    }

    private void FixedUpdate()
    {
        if(isInAir)
        {
            // fall if the monster is in the air, this shouldn't ever be the case
            velocity.y -= gravity * Time.fixedDeltaTime;
            controller.Move(velocity * Time.fixedDeltaTime);
            isInAir = !controller.isGrounded;
        }
        else
        {
            // move the monster according to current move direction
            Vector3 inputDirection = new Vector3(currDirection.x, 0f, currDirection.y).normalized;      
            Vector3 forwardMotion = transform.TransformDirection(inputDirection) * Time.fixedDeltaTime * groundSpeed;
            if(isDashing)
            {
                forwardMotion *= dashMultiplier;
            }
            Vector3 downwardMotion = Vector3.down * controller.stepOffset;
            controller.Move(forwardMotion + downwardMotion);

            if(!controller.isGrounded)
            {
                isInAir = true;
                velocity = animator.velocity;
                velocity.y = 0f;
            }
        }

        // while dashing, do dash damage to any enemies that enter a sphere within dashRadius
        if(isDashing)
        {
            Collider[] hitEnemies = Physics.OverlapSphere(transform.position, dashRadius, dashLayerMask);
            foreach (Collider enemy in hitEnemies)
            {
                if(enemy.TryGetComponent<IDamageableHitbox>(out IDamageableHitbox damageableHitBox))
                {
                    IDamageable damageable = damageableHitBox.getDamageable();
                    if(damageables.Contains(damageable))
                    {
                        return;
                    }

                    damageables.Add(damageable);

                    Vector3 direction = enemy.transform.position - transform.position;
                    damageableHitBox.applyDamageScript(dashDamageSO, direction);
                }
            }
        }
    }  

    private void onGameOver(bool result)
    {
        if(result)
        {
            // play victory animation if player wins
            animator.SetTrigger("triggerVictory");
            animator.SetLayerWeight(1, 1f); 
        }
        enabled = false;
    } 

    // dash button has been pressed, check if we can dash and do so
    private void onDashTriggered()
    {
        if(gameManager.isGamePaused())
        {
            return;
        }
        
        if(canDash(dashDamageSO.staminaCost))
        {
            gameManager.playerStats.useStamina(dashDamageSO.staminaCost);
            dash();
        }  
    }

    // check if player can dash
    private bool canDash(int stam)
    {
        return !isDashing 
            && !isInAir 
            && gameManager.playerStats.checkStamina(stam) 
            && inputManager.moveInput != Vector2.zero
            && inputManager.moveInput.y >= 0f;
    }   

    private void dash()
    {    
        animator.SetBool("isDashing", true); 
        isDashing = true;
        damageables.Clear();

        Invoke(nameof(resetDash), dashDuration);
    }

    private void resetDash()
    {        
        animator.SetBool("isDashing", false); 
        isDashing = false;
    }    

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, dashRadius);
    }
}
