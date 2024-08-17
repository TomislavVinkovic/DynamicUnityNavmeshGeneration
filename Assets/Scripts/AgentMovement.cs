using System.Collections;
using UnityEngine;
using UnityEngine.AI;

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
            Debug.Log(position);
            

            // Move to the nearest valid point
            Vector3 nearestPoint = FindNearestNavMeshPoint(position);
            Debug.Log(nearestPoint);
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
