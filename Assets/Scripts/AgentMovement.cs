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

    GameStateController gameStateController;

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

    void Awake()
    {
        gameStateController = GameObject.Find(World.GAME_STATE_CONTROLLER_TAG).GetComponent<GameStateController>();
    }

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    Vector3 NewRandomPosition() {
        return new Vector3(
            Random.Range(
                -gameStateController.PlaneWidth/2 + World.AGENT_WIDTH, 
                gameStateController.PlaneWidth/2 - World.AGENT_WIDTH
            ), 
            1f, 
            Random.Range(
                -gameStateController.PlaneHeight/2 + World.AGENT_WIDTH, 
                gameStateController.PlaneHeight/2 - World.AGENT_WIDTH
            )
        );
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
                Debug.Log("Moving to first position: " + newRandom);
                MoveToPosition (newRandom);
            }
            else if(navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance) {
                if(navMeshAgent.destination != originalDestination) {
                    TryMoveToPosition((Vector3)originalDestination);
                }
                else {
                    var newRandom = NewRandomPosition();
                    Debug.Log("Moving to another position: " + newRandom);
                    MoveToPosition (newRandom);
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
                if(navMeshAgent.destination != originalDestination) {
                    TryMoveToPosition((Vector3)originalDestination);
                }
                else {
                    originalDestination = null;
                }
            }
        }
    }
}
