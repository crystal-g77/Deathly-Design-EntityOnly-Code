using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WorldBounds : MonoBehaviour
{
    public Transform PosXPosZ;
    public Transform NegXNegZ;
    public float minRadius = 10f; // Radius to spawn away from point
    public float checkRadius = 1f; // Radius to check around the spawn point for obstacles
    public int maxAttempts = 10; // Number of times to try for valid position
    public LayerMask obstacleLayers; // Layers for player, AI agents, and obstacles

    // get a random position in the world
    public Vector3 getValidPosition(Camera camera = null)
    {
        // Use the bounding box of the world
        return getValidPosition(NegXNegZ.position, PosXPosZ.position, camera);
    }
    
    // get a random position in the world a set radius away from the given point
    public Vector3 getValidPositionAndMaintainDistance(Vector3 point)
    {
        // Use the bounding box of the world
        return getValidPositionAndMaintainDistance(point, NegXNegZ.position, PosXPosZ.position);
    }

    // get a random position in the world within the given bounding box
    // and outside the cameras field of view if it exists
    public Vector3 getValidPosition(Vector3 otherMinPos, Vector3 otherMaxPos, Camera camera = null)
    {        
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            Vector3 potentialPosition = getRandomPosition(otherMinPos, otherMaxPos);

            // Check if the point is free from collisions
            Collider[] hitColliders = Physics.OverlapSphere(potentialPosition, checkRadius, obstacleLayers);
            if (hitColliders.Length == 0)
            {
            // if (!Physics.CheckSphere(potentialPosition, checkRadius, obstacleLayers))
            // {
                // Verify the position is on the NavMesh
                if (NavMesh.SamplePosition(potentialPosition, out NavMeshHit hit, checkRadius, NavMesh.AllAreas))
                {
                    if(!camera)
                    {
                        return hit.position;
                    }
                    else
                    {
                        // Check if the position is outside the camera's field of view
                        Vector3 viewportPoint = camera.WorldToViewportPoint(hit.position);

                        // If the position is outside the field of view (0-1 in viewport coordinates is in the view)
                        if (viewportPoint.x < 0 || viewportPoint.x > 1 || viewportPoint.y < 0 || viewportPoint.y > 1 || viewportPoint.z < 0)
                        {
                            return hit.position; // Valid position found
                        }
                    }
                }
            }
        }

        Debug.LogWarning("Failed to find a valid spawn position.");
        return Vector3.zero; // Return Vector3.zero if no valid position is found after max attempts
    }

    // get a random position in the world within the given bounding box and a set radius away
    // from the given point
    public Vector3 getValidPositionAndMaintainDistance(Vector3 point, Vector3 otherMinPos, Vector3 otherMaxPos)
    {        
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            Vector3 potentialPosition = getRandomPosition(otherMinPos, otherMaxPos);

            // Check if the point is far enough from the target point
            if (Vector3.Distance(potentialPosition, point) >= minRadius)
            {
                // Check if the point is free from collisions
                Collider[] hitColliders = Physics.OverlapSphere(potentialPosition, checkRadius, obstacleLayers);
                if (hitColliders.Length == 0)
                {
                // if (!Physics.CheckSphere(potentialPosition, checkRadius, obstacleLayers))
                // {
                    // Verify the position is on the NavMesh
                    if (NavMesh.SamplePosition(potentialPosition, out NavMeshHit hit, checkRadius, NavMesh.AllAreas))
                    {
                        return hit.position;
                    }
                }
            }
        }

        Debug.LogWarning("Failed to find a valid spawn position.");
        return Vector3.zero; // Return Vector3.zero if no valid position is found after max attempts
    }

    // Get a position just outside a radius from a point
    public Vector3 getValidPositionOutsideObstacleRadius(Vector3 currPos, Vector3 targetPos, float radius, float variation)
    {
        Vector3 directionToTarget = (currPos - targetPos).normalized;

        // Offset the target position by the obstacle radius (along the direction to the target)
        Vector3 offsetPos = targetPos + directionToTarget * radius;
        offsetPos.y = 0f;

        // Get bounds around position
        Vector3 minPos = new Vector3(
            offsetPos.x - variation, 
            0f, 
            offsetPos.z - variation
        );
        Vector3 maxPos = new Vector3(
            offsetPos.x + variation, 
            0f, 
            offsetPos.z + variation
        );

        return getValidPosition(minPos, maxPos);
    }

    // Get a random position within the intersection of the world and the given bounding vectors
    private Vector3 getRandomPosition(Vector3 otherMinPos, Vector3 otherMaxPos)
    {
        Vector3 minPos = Vector3.Max(NegXNegZ.position, otherMinPos);
        minPos.y = NegXNegZ.position.y;
        Vector3 maxPos = Vector3.Min(PosXPosZ.position, otherMaxPos);
        maxPos.y = NegXNegZ.position.y;

        Vector3 randomPos = new Vector3(
            Random.Range(minPos.x, maxPos.x),
            Random.Range(minPos.y, maxPos.y),
            Random.Range(minPos.z, maxPos.z)
        );

        return randomPos;
    }
}
