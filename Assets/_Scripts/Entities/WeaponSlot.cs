using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations.Rigging;

// Class for managing which weapon is equipped, where it is attached
// Responsible for calling startAttack, stopAttack and updateWeapon
// Responsible for playing weapon animations for the equiped weapon
public class WeaponSlot : MonoBehaviour
{
    [SerializeField]
    private RigBuilder rigBuilder;
    protected GameManager gameManager;
    protected DestructionPool destructionPool;
    protected Transform aimTargetTransform;
    protected Animator slotAnimator;
    protected BaseWeapon currWeapon = null;  

    private Rig rig;
    private MultiAimConstraint[] aimConstraints;
    private Transform attachPoint;
    private BaseWeapon droppedWeapon = null;

    protected void Awake()
    {
        gameManager = GameManager.Instance;
        destructionPool = DestructionPool.Instance;
        slotAnimator = GetComponent<Animator>();
        rig = GetComponent<Rig>();
        aimConstraints = rigBuilder.GetComponentsInChildren<MultiAimConstraint>();
        attachPoint = transform.Find("Offset");
        rig.weight = 0f;

        #if UNITY_EDITOR
        slotAnimator.speed = 0.5f;
        #endif
    }

    protected void Update()
    {
        #if UNITY_EDITOR
        slotAnimator.Update(Time.deltaTime); // this is because weapon animation are not playing in the editor
        #endif
    }

    // begin an attack
    public void setAttacking(bool enabled)
    {
        if(!currWeapon)
        {
            return;
        }

        if(enabled)
        {
            if(!currWeapon.useOnWeaponFire)
            {
                slotAnimator.SetTrigger("Attack");
            }
            currWeapon.startAttack();
        }
        else
        {
            currWeapon.stopAttack();
        }
    }

    // if the weapon slot has an aim constraint, update the target
    public void setAimTarget(Transform target)
    {
        aimTargetTransform = target;
        foreach(MultiAimConstraint aimConstraint in aimConstraints)
        {
            WeightedTransformArray wta = aimConstraint.data.sourceObjects;
            wta.Clear();
            wta.Add(new WeightedTransform(aimTargetTransform, 1));
            aimConstraint.data.sourceObjects = wta;
        }
        rigBuilder.Build();
    }

    public bool isAttacking()
    {
        return currWeapon && currWeapon.isAttacking();
    }

    public int getWeaponStaminaCost()
    {
        if(currWeapon)
        {
            return currWeapon.weaponDamageSO.staminaCost;
        }
        return 0;
    }

    // create the given weapon type and attach it to the weapon slot
    public virtual void equipWeapon(WeaponTypeUnion weaponType)
    {
        GameObject weaponGO = destructionPool.spawnWeapon(weaponType, attachPoint);

        currWeapon = weaponGO.GetComponentInChildren<BaseWeapon>();
        currWeapon.wielder = rigBuilder.gameObject;
        if(currWeapon)
        {
            currWeapon.weaponType = weaponType;

            RaycastWeapon raycastWeapon = currWeapon as RaycastWeapon;
            if(raycastWeapon)
            {
                raycastWeapon.setParentSlot(this);
            }

            rig.weight = 1f;
            slotAnimator.Play(currWeapon.weaponType.name() + "_Startup_Animation");
        }     
    }

    public bool hasWeapon()
    {
        return currWeapon != null;
    }

    // drop a weapon and apply a force so that it goes flying
    public void dropWeapon(Vector3 direction)
    {
        if(currWeapon) 
        {
            slotAnimator.Play("Unarmed");
            rig.weight = 0f;

            currWeapon.transform.SetParent(null);

            RaycastWeapon raycastWeapon = currWeapon as RaycastWeapon;
            if(raycastWeapon)
            {
                raycastWeapon.setParentSlot(null);
            }

            Collider collider = currWeapon.GetComponentInChildren<Collider>();
            if(collider)
            {
                Rigidbody rb = collider.AddComponent<Rigidbody>();
                rb.AddForce(direction, ForceMode.VelocityChange);
            }
            droppedWeapon = currWeapon;
            currWeapon = null;
        }
    }

    // when an AIAgent despawns, destroy any weapon that is has dropped
    public void cleanupDroppedWeapon()
    {
        if(droppedWeapon)
        {
            Collider[] colliders = droppedWeapon.GetComponentsInChildren<Collider>();
            foreach (Collider col in colliders)
            {
                Rigidbody rb = col.GetComponent<Rigidbody>();
                if (rb)
                {
                    Destroy(rb);
                }
            }
            if(destructionPool)
            {
                destructionPool.returnWeapon(droppedWeapon.weaponType, droppedWeapon.gameObject);
            }
            droppedWeapon = null;
        }
    }

    // triggered by the weapon in the slot
    public virtual void onWeaponFire() 
    {        
        slotAnimator.SetTrigger("Attack");
    }

    // deal with events in an animation
    private void OnAnimationEvent(string eventName)
    {
        if(eventName == "AttackDone")
        {
            setAttacking(false);
        }
        else if(eventName == "SpawnParticles")
        {
            SporeWeapon sporeWeapon = currWeapon as SporeWeapon;
            if(sporeWeapon)
            {
                sporeWeapon.spawnGroundDamage();
            }
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        currWeapon.weaponHit(collider.gameObject);
    }
}
