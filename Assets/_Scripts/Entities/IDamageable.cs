using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// interface for entities that can take damage
public interface IDamageable 
{
    public bool takeDamage(int amount, Vector3 direction);
}
