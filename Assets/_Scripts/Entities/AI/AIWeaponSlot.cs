using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// a class to manage the weapon slots of an AIAgent

public class AIWeaponSlot : WeaponSlot
{
    [SerializeField]
    private float inaccuracy = 0.4f;
    
    [SerializeField]
    private float updateTargetSlerp = 0.04f;

    private Transform aiTarget; // target the AIAgent is shooting at
    private Vector3 targetPos;  
    private float timer;

    private new void Update()
    {
        if(gameManager.isGamePaused() )
        {
            return;
        }

        base.Update();

        if(aiTarget && currWeapon)
        {
            float temp = getTimer();
            if(temp > 0f)
            {
                timer -= Time.deltaTime;
                if(timer <= 0f)
                {
                    // introduce aim variation so that AI misses target sometimes
                    targetPos = aiTarget.position + Random.insideUnitSphere * inaccuracy;
                    timer += temp;
                }
            }
            
            aimTargetTransform.position = Vector3.Slerp(aimTargetTransform.position, targetPos, updateTargetSlerp);
            currWeapon.updateWeapon(Time.deltaTime, aimTargetTransform.position);
        }
    }

    public void setTarget(Transform target)
    {
        aiTarget = target;
        if(target)
        {
            aimTargetTransform.position = targetPos = aiTarget.transform.position;
        }
    }  

    public override void equipWeapon(WeaponTypeUnion weaponType)
    {
        base.equipWeapon(weaponType);

        timer = getTimer();
    }

    public override void onWeaponFire()
    {
        base.onWeaponFire();

        if(aiTarget)
        {
            // introduce aim variation so that AI misses target sometimes
            targetPos = aiTarget.position + Random.insideUnitSphere * inaccuracy;
        }
    }

    private float getTimer()
    {
        float timer = 0f;
        if(currWeapon)
        {
            RaycastWeapon weapon = currWeapon as RaycastWeapon;
            if(weapon && weapon.FireRate > updateTargetSlerp + .2f)
            {
                timer = updateTargetSlerp;
            }
        }

        return timer;
    }
}
