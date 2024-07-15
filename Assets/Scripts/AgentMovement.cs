using UnityEngine;
using UnityEngine.AI;
using System;

public class AgentMovement : MonoBehaviour
{
    bool navMeshAgentSet = false;
    NavMeshAgent navMeshAgent;

    Vector3[] waypoints = {
        new Vector3(7, 1, 7),
        new Vector3(5, 1, 3),
        new Vector3(1, 1, 4),
        new Vector3(10, 1, 7),
        new Vector3(13, 1, 3),
        new Vector3(12, 1, 9),
    };

    Vector3 GetRandomWaypoint() {
        var index = UnityEngine.Random.Range(0, waypoints.Length - 1);
        return waypoints[index];
    }
    Vector3 GetNextPosition() {
        // TODO: Implement some more interesting movement
        return transform.position + new Vector3(-1, 0, 1);
    }

    // Start is called before the first frame update
    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        if(!navMeshAgent) {
            navMeshAgent = GetComponent<NavMeshAgent>();
        }
        if(navMeshAgent) {
            if(!navMeshAgentSet) {
                navMeshAgentSet = true;
                navMeshAgent.SetDestination(GetNextPosition());
            }
            else if (navMeshAgent.remainingDistance < navMeshAgent.stoppingDistance) {
                
                

                navMeshAgent.SetDestination(GetNextPosition());
            }
        }
    }
}
