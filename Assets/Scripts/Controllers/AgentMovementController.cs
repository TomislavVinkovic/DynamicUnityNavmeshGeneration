using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/************************************************************************************
 * This class is used to control the movement of an agent
 * It is used to move the agent to a specific position
************************************************************************************/
public class AgentMovementController : MonoBehaviour
{
    NavMeshAgent navMeshAgent;
    Vector3? originalDestination;
    AgentManagerController agentManager;

    GameStateController gameStateController;

    const float MAX_SEARCH_DISTANCE = 1000f;

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
        
        if (NavMesh.SamplePosition(position, out hit, MAX_SEARCH_DISTANCE, NavMesh.AllAreas))
        {
            return hit.position;
        }
        return position;
    }

    void Awake()
    {
        gameStateController = GameObject.Find(World.GAME_STATE_CONTROLLER_TAG).GetComponent<GameStateController>();
        agentManager = GameObject.Find(World.AGENT_MANAGER_TAG).GetComponent<AgentManagerController>();
    }

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    Vector3 NewRandomPosition() {
        int randomIndex = Random.Range(0, agentManager.AgentWaypoints.Count);
        navMeshAgent.speed = Random.Range(15f, 30f);
        return agentManager.AgentWaypoints[randomIndex];
    }

    void Update()
    {
        if(gameStateController.RandomMovementEnabled) {
            if(
                originalDestination == null 
                && navMeshAgent != null
                && navMeshAgent.enabled
            ) {
                var newRandom = NewRandomPosition();
                MoveToPosition (newRandom);
            }
            else if(navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance) {
                if(
                    navMeshAgent.destination.x != ((Vector3)originalDestination).x
                    || navMeshAgent.destination.z != ((Vector3)originalDestination).z
                ) {

                    TryMoveToPosition((Vector3)originalDestination);
                }
                else {
                    
                    var newRandom = NewRandomPosition();
                    MoveToPosition(newRandom);
                }
            }

        }
            
        else {
            if (
                navMeshAgent != null
                && navMeshAgent.enabled
                && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance
                && originalDestination != null
            ) {
                if(
                    navMeshAgent.destination.x != ((Vector3)originalDestination).x
                    || navMeshAgent.destination.z != ((Vector3)originalDestination).z
                ) {
                    TryMoveToPosition((Vector3)originalDestination);
                }
                else {
                    originalDestination = null;
                }
            }
        }
    }
}
