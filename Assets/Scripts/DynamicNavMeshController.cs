using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEditor;

public class DynamicNavMeshController : MonoBehaviour
{
    public GUID id;
    public DynamicNavMeshState state;
    public NavMeshSurface navMeshSurface;
    List<GameObject> agents;
    List<GameObject> agentsInside;
    public Bounds navMeshBounds;
    public Bounds smallerBounds;

    void Start() 
    {
        id = GUID.Generate();
        state = DynamicNavMeshState.Create;
        navMeshSurface = GetComponent<NavMeshSurface>();

        if (navMeshSurface.navMeshData == null)
        {
            navMeshSurface.navMeshData = new NavMeshData();
            NavMesh.AddNavMeshData(navMeshSurface.navMeshData);
        }
        navMeshBounds = new Bounds(transform.position, new Vector3(20f, 5f, 20f));
        smallerBounds = new Bounds(transform.position, new Vector3(14f, 5f, 14f)); // 70% of the area

        agents = new List<GameObject>(GameObject.FindGameObjectsWithTag("Agent"));
        agentsInside = new List<GameObject>();
    }

    void UpdateBounds() {
        navMeshBounds.center = transform.position;
        smallerBounds.center = transform.position;
    }

    void Update() 
    {
        // only update if the navmeshsurface is not in the update process already
        if(state == DynamicNavMeshState.Ready) {
            agents = new List<GameObject>(GameObject.FindGameObjectsWithTag("Agent"));
    
            foreach (var agent in agents) {
                if(navMeshBounds.Contains(agent.transform.position)) {
                    agentsInside.Add(agent);
                }
            }
            foreach (var agent in agentsInside) {
                if(!smallerBounds.Contains(agent.transform.position)) {
                    // mark for update
                    state = DynamicNavMeshState.Update;
                    return;
                }
            }
        }
    }

    public void UpdateNavMesh() {
        agentsInside.ForEach(agent => agent.SetActive(false));

        var meanPosition = LinearAlgebra.GetMeanInSpace(
            agentsInside.ConvertAll(agent => agent.transform.position)
        );
        transform.position = new Vector3(meanPosition.x, 0, meanPosition.z);
        UpdateBounds();

        state = DynamicNavMeshState.Ready;

        foreach (var agent in agents) {
            var center = new Vector3(
                transform.position.x,
                0,
                transform.position.z
            );
            if(navMeshBounds.Contains(agent.transform.position - Vector3.up)) {
                agentsInside.Add(agent);
            }
        }
        agentsInside.ForEach(agent => agent.SetActive(true));
    }

    public void BuildNavMesh() {
        navMeshSurface.collectObjects = CollectObjects.Volume;
        navMeshSurface.center = Vector3.zero;
        navMeshSurface.size = new Vector3(20f, 0f, 20f);

        navMeshSurface.BuildNavMesh();
        state = DynamicNavMeshState.Ready;

        foreach (var agent in agents) {
            var center = new Vector3(
                transform.position.x,
                0,
                transform.position.z
            );
            if(navMeshBounds.Contains(agent.transform.position - Vector3.up)) {
                agentsInside.Add(agent);
            }
        }

        agentsInside.ForEach(agent => ReactivateAgentIfOnNavMesh(agent));
    }

    private void ReactivateAgentIfOnNavMesh(GameObject agent)
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(agent.transform.position, out hit, 1.0f, NavMesh.AllAreas))
        {
            agent.transform.position = hit.position;
            var navMeshAgent = agent.AddComponent<NavMeshAgent>();
            navMeshAgent.stoppingDistance = 0.1f;
        } 
    }
}
