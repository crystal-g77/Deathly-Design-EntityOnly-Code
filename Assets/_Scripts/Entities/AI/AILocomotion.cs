using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// class to manage the animator locomotion blend tree
public class AILocomotion : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator animator;
    private Quaternion previousRotation;

    // Start is called before the first frame update
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        previousRotation = transform.rotation; // Store initial rotation
    }

    // Update is called once per frame
    void Update()
    {
        // Check if timescale is 0 (i.e., the game is paused)
        if (Time.timeScale == 0)
        {
            animator.SetFloat("Speed", 0);  // Set speed to 0 when the game is paused
            return;  // Skip further processing
        }

        // Calculate angular velocity by comparing previous and current rotations
        float angularVelocity = Quaternion.Angle(previousRotation, transform.rotation) / Time.deltaTime;

        // Store current rotation for the next frame
        previousRotation = transform.rotation;

        if(agent.hasPath)
        {
            animator.SetFloat("Speed", agent.velocity.magnitude);
        }
        else if(angularVelocity > 0.1f)  // Check if the agent is rotating (angular velocity threshold)
        {
            // Set speed to 1 if the agent is rotating (walking animation triggers)
            animator.SetFloat("Speed", 1.6f);
        }
        else
        {
            animator.SetFloat("Speed", 0);
        }
    }
}
