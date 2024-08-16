using UnityEngine;
using UnityEngine.AI;

/*
    * Class used for moving agents
*/

public class AgentMovement : MonoBehaviour
{
    bool navMeshAgentSet = false;
    NavMeshAgent navMeshAgent;

    Vector3[] waypoints = {
        new Vector3(32.04f, 0.0f, -49.81f),
        new Vector3(12.33f, 0.0f, -22.01f),
        new Vector3(3.33f, 0.0f, 3.72f),
        new Vector3(20.17f, 0.0f, -48.17f),
        new Vector3(34.28f, 0.0f, 55.02f),
        new Vector3(69.96f, 0.0f, 57.92f),
        new Vector3(-47.92f, 0.0f, -74.73f),
        new Vector3(-65.81f, 0.0f, -38.81f),
        new Vector3(11.01f, 0.0f, 73.63f),
        new Vector3(16.80f, 0.0f, -1.47f),
        new Vector3(-26.10f, 0.0f, 60.69f),
        new Vector3(-42.91f, 0.0f, -45.88f),
        new Vector3(-43.39f, 0.0f, -27.15f),
        new Vector3(-48.67f, 0.0f, 48.87f),
        new Vector3(-22.31f, 0.0f, 45.46f),
        new Vector3(72.68f, 0.0f, -24.35f),
        new Vector3(-50.16f, 0.0f, -46.99f),
        new Vector3(-72.02f, 0.0f, -65.48f),
        new Vector3(30.58f, 0.0f, -26.77f),
        new Vector3(-59.95f, 0.0f, -51.72f),
        new Vector3(54.78f, 0.0f, -23.97f),
        new Vector3(41.58f, 0.0f, -61.39f),
        new Vector3(52.10f, 0.0f, -22.05f),
        new Vector3(10.63f, 0.0f, 42.72f),
        new Vector3(46.59f, 0.0f, 68.07f),
        new Vector3(55.33f, 0.0f, 31.26f),
        new Vector3(-49.81f, 0.0f, 24.06f),
        new Vector3(-64.31f, 0.0f, 0.91f),
        new Vector3(40.95f, 0.0f, -31.31f),
        new Vector3(-9.83f, 0.0f, 19.08f),
        new Vector3(66.11f, 0.0f, 32.35f),
        new Vector3(60.19f, 0.0f, -40.50f),
        new Vector3(4.64f, 0.0f, -64.63f),
        new Vector3(34.85f, 0.0f, -8.57f),
        new Vector3(12.63f, 0.0f, 12.34f),
        new Vector3(-66.15f, 0.0f, 67.55f),
        new Vector3(71.81f, 0.0f, 69.82f),
        new Vector3(55.44f, 0.0f, 72.66f),
        new Vector3(45.33f, 0.0f, -69.21f),
        new Vector3(19.05f, 0.0f, 14.65f),
        new Vector3(-12.79f, 0.0f, -16.82f),
        new Vector3(27.21f, 0.0f, 23.23f),
        new Vector3(40.51f, 0.0f, 56.53f),
        new Vector3(27.92f, 0.0f, 8.09f),
        new Vector3(-37.94f, 0.0f, -74.01f),
        new Vector3(-61.81f, 0.0f, -34.02f),
        new Vector3(-36.47f, 0.0f, -10.00f),
        new Vector3(64.01f, 0.0f, 24.91f),
        new Vector3(55.21f, 0.0f, -14.52f),
        new Vector3(-10.29f, 0.0f, -53.87f)
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
                navMeshAgent.SetDestination(new Vector3(50f, 0f, 120f));
            }
            else if (navMeshAgent.remainingDistance < navMeshAgent.stoppingDistance) {
                navMeshAgent.SetDestination(new Vector3(50f, 0f, 120f));
            }
        }
    }
}
