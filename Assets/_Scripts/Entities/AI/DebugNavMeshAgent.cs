using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// draw debug information about NavMeshAgent
public class DebugNavMeshAgent : MonoBehaviour
{
    public bool velocity;
    public bool desiredVelocity;
    public bool path;
    private NavMeshAgent agent;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();        
    }

    // Update is called once per frame
    void OnDrawGizmos() 
    {
        if(!transform || !agent)
        {
            return;
        }

        if(velocity)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + agent.velocity);
        }

        if(desiredVelocity)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + agent.desiredVelocity);
        }

        if(path)
        {
            Gizmos.color = Color.black;
            if(agent.pathStatus == NavMeshPathStatus.PathInvalid)
            {
                Gizmos.color = Color.red;
            }
            else if(agent.pathStatus == NavMeshPathStatus.PathPartial)
            {
                Gizmos.color = Color.yellow;
            }
            var agentPath = agent.path;
            Vector3 prevCorner = transform.position;
            foreach(var corner in agentPath.corners)
            {
                Gizmos.DrawLine(prevCorner, corner);
                Gizmos.DrawSphere(corner, 0.1f);
                prevCorner = corner;
            }
        }
    }
}
