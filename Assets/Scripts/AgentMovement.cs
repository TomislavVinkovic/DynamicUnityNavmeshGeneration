using UnityEngine;
using UnityEngine.AI;

/************************************************************************************
 * This class is used to control the movement of an agent
 * It is used to move the agent to a specific position
************************************************************************************/
public class AgentMovement : MonoBehaviour
{
    private NavMeshAgent navMeshAgent;
    private Vector3? originalDestination;

    public void MoveToPosition(Vector3 position)
    {
        if (navMeshAgent != null)
        {
            originalDestination = position;
            TryMoveToPosition(position);
        }
    }

    private void TryMoveToPosition(Vector3 position)
    {
        // Check if the position is on the NavMesh
        NavMeshHit hit;
        if (NavMesh.SamplePosition(position, out hit, navMeshAgent.height, NavMesh.AllAreas))
        {
            // Position is on the NavMesh
            navMeshAgent.SetDestination(hit.position);
        }
        else
        {
            // Move to the nearest valid point
            Vector3 nearestPoint = FindNearestNavMeshPoint(position);
            if (nearestPoint != position)
            {
                navMeshAgent.SetDestination(nearestPoint);
            }
        }
    }

    private Vector3 FindNearestNavMeshPoint(Vector3 position)
    {
        NavMeshHit hit;
        float maxSearchDistance = 1000f; // Set a reasonable maximum search distance
    
        if (NavMesh.SamplePosition(position, out hit, maxSearchDistance, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return position;
    }

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (
            navMeshAgent != null
            && navMeshAgent.enabled
            && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance
            && originalDestination != null
        )
        {
            if(navMeshAgent.destination != originalDestination) {
                TryMoveToPosition((Vector3)originalDestination);
            }
            else {
                originalDestination = null;
            }
        }
    }
}
