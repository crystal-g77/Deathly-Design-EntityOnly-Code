using System.Collections.Generic;
using UnityEngine;

// a class for managing the player rotation
public class playerAiming : MonoBehaviour
{
    [SerializeField]
    private float rotationSpeed = 15f;
    
    private GameManager gameManager;
    private InputManager inputManager;
    private Transform cameraTransform;

    void Start()
    {                
        gameManager = GameManager.Instance;
        inputManager = InputManager.Instance;
        cameraTransform = Camera.main.transform;
        
        gameManager.OnGameOver += onGameOver;
    }    

    private void OnDestroy() 
    {        
        gameManager.OnGameOver -= onGameOver;
    }    

    private void FixedUpdate()
    {
        // when moving rotate player to compensate for aim direction
        if(inputManager.moveInput != Vector2.zero)
        {
            float targetAngle = cameraTransform.eulerAngles.y;
            Quaternion rotation = Quaternion.Euler(0f, targetAngle, 0f);
            transform.rotation = Quaternion.Lerp(transform.rotation, rotation, rotationSpeed);  
        }
    }

    public void onGameOver(bool result)
    {
        enabled = false;
    }
}