using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class AgentMovement : MonoBehaviour
{
    private NavMeshAgent navMeshAgent;
    private Vector3 originalDestination;

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
        NavMeshHit hit;
        if (NavMesh.SamplePosition(position, out hit, navMeshAgent.height, NavMesh.AllAreas))
        {
            // Position is on the NavMesh
            navMeshAgent.SetDestination(hit.position);
        }
        else
        {
            // Position is outside the NavMesh
            Debug.LogWarning("Destination is outside the NavMesh. Moving to the nearest point.");

            // Move to the nearest valid point
            Vector3 nearestPoint = FindNearestNavMeshPoint(position);
            if (nearestPoint != position)
            {
                navMeshAgent.SetDestination(nearestPoint);
            }
            else
            {
                Debug.LogWarning("No valid NavMesh point found. Staying at the current position.");
            }
        }
    }

    private Vector3 FindNearestNavMeshPoint(Vector3 position)
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(position, out hit, Mathf.Infinity, NavMesh.AllAreas))
        {
            return hit.position;
        }
        return position; // Fallback if no valid NavMesh point is found
    }

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (
            navMeshAgent != null 
            && navMeshAgent.hasPath
            && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance
        )
        {
            if(navMeshAgent.destination != originalDestination) {
                TryMoveToPosition(originalDestination);
            }
        }
    }
}
