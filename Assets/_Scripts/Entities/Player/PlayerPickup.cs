using CCLemon.GridSystem.Inventory;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CCLemon.GridSystem.Point;

// a class to manage the pickup collider and when pickups enter the collider
public class PlayerPickup : MonoBehaviour
{
    private Collider playerPickupCollider;

    void Awake()
    {
        playerPickupCollider = GetComponent<Collider>();
    }
    
    void OnTriggerEnter(Collider collider)
    {
        if(collider.gameObject.TryGetComponent<PickupHandler>(out PickupHandler handler))
        {
            handler.PickedUpFunction(); 

            if(playerPickupCollider.gameObject.TryGetComponent<AudioSource>(out AudioSource audio))
            {
                AudioManager.Instance.playSFX(audio);
            }

            Destroy(collider.gameObject);

        }
    }
}
