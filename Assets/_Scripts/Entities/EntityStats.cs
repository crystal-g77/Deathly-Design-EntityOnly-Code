using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// a class for managing stats used by enemies and player (health)
public abstract class EntityStats : MonoBehaviour, IDamageable
{
    [SerializeField]
    protected int maxHealth;

    private Rigidbody[] rigidbodies;
    private int currHealth;

    // Start is called before the first frame update
    protected void Awake()
    {  
        rigidbodies = GetComponentsInChildren<Rigidbody>();  
    }

    protected void OnEnable()
    {
        currHealth = maxHealth;  

        foreach(Rigidbody rigidbody in rigidbodies)
        {
            rigidbody.gameObject.layer = getLayer();
        }
    }
    
    public virtual bool takeDamage(int amount, Vector3 direction)
    {        
        currHealth = Mathf.Clamp(currHealth - amount, 0, maxHealth);
        if(currHealth <= 0)
        {
            die(direction);
            return true;
        }
        return false;
    }

    public virtual void heal(int amount)
    {
        currHealth = Mathf.Clamp(currHealth + amount, 0, maxHealth);
    }

    public (int, int) getHealthRatio()
    {
        return (currHealth, maxHealth);
    }

    public abstract string getTag();

    public abstract LayerMask getLayer();
    
    public virtual LayerMask getDeadLayer()
    {
        return getLayer();
    }

    protected virtual void die(Vector3 direction)
    {
        BaseDamage[] damages = GetComponents<BaseDamage>();
        foreach(BaseDamage damage in damages)
        {
            Destroy(damage);
        }

        foreach(Rigidbody rigidbody in rigidbodies)
        {
            rigidbody.gameObject.layer = getDeadLayer();
        }
    }
}
