using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class AttackCollider : MonoBehaviour
{
    private Collider boxCollider;
    private List<IDamageable> damageables;

    // Start is called before the first frame update
    void Awake()
    {
        boxCollider = GetComponent<Collider>();
        damageables = new List<IDamageable>();
    }

    void Update()
    {
        if(!boxCollider.enabled && damageables.Count > 0)
        {
            damageables.Clear();
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        if(collider.TryGetComponent<IDamageableHitbox>(out IDamageableHitbox damageableHitBox))
        {
            IDamageable damageable = damageableHitBox.getDamageable();
            if(damageables.Contains(damageable))
            {
                return;
            }

            damageables.Add(damageable);

            if (transform.parent != null)
            {
                // Try to call OnTriggerEnter on the parent object if it has one
                transform.parent.SendMessage("OnTriggerEnter", collider, SendMessageOptions.DontRequireReceiver);
            }
        }
    }
}
