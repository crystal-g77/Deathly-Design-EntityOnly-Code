using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// interface for hitboxes that can have damage applied
public interface IDamageableHitbox 
{
    public bool applyDamageScript(DamageSO damageSO, Vector3 direction);
    public IDamageable getDamageable();
}
