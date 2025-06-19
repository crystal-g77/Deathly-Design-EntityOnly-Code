using UnityEngine;

[RequireComponent(typeof(Collider))]
public class HitBox : MonoBehaviour, IDamageableHitbox
{
    public IDamageable damageable;

    void Awake()
    {
        damageable = GetComponentInParent<IDamageable>();
    }

    void Start()
    {       
        if(((MonoBehaviour)damageable).TryGetComponent<EntityStats>(out EntityStats stats))
        {
            gameObject.tag = stats.getTag();
            gameObject.layer = stats.getLayer();
        } 
    }

    public bool applyDamageScript(DamageSO damageSO, Vector3 direction)
    {
        return damageSO.attachDamage(((MonoBehaviour)damageable).gameObject, direction);
    }

    public IDamageable getDamageable()
    {
        return damageable;
    } 
}
