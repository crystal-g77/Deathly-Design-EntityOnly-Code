using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// updates to the position the player is currently aiming at
public class CrosshairTarget : MonoBehaviour
{
    [SerializeField]
    private Transform playerTransform;
    [SerializeField]
    private GameObject worldCrossHair;
    [SerializeField]
    private GameObject uiCrossHair;
    [SerializeField]
    private float maxAimAngle;

    private Camera mainCamera;
    private Ray ray;
    private RaycastHit hitInfo;
    private LayerMask layermask;

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;

        layermask = UtilsLayers.getCollidingLayerMask(LayerMask.NameToLayer("PlayerDamage"));
    }

    // Update is called once per frame
    void Update()
    {
        ray.origin = mainCamera.transform.position;
        ray.direction = mainCamera.transform.forward;
        Physics.Raycast(ray, out hitInfo, Mathf.Infinity, layermask.value);
        worldCrossHair.transform.position = hitInfo.point;

        // Calculate the angle between the camera's forward and the player's forward
        float angle = Vector3.Angle(mainCamera.transform.forward, playerTransform.forward);

        // if within allowable angle, activate the ui and world crosshair
        // if not, deactivate them
        if(angle <= maxAimAngle)
        {
            worldCrossHair.SetActive(true);
            uiCrossHair.SetActive(true);
        }
        else
        {
            worldCrossHair.SetActive(false);
            uiCrossHair.SetActive(false);
        }
    }
}
