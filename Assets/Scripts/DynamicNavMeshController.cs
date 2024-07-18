using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEditor;

/*
    * Class used for building a single dynamic navmesh
    * It is also used to check if any agents are so out of bounds so the navmesh needs to be rebuilt
*/
public class DynamicNavMeshController : MonoBehaviour
{
    public GUID id;
    public DynamicNavMeshState State;
    public NavMeshSurface navMeshSurface;
    public GlobalNavMeshController GlobalNavMeshController {get; set;}
    List<GameObject> agents;
    List<GameObject> agentsInside;
    Bounds navMeshBounds;
    Bounds smallerBounds;
    
    // PUBLIC GETTERS
    public Bounds NavMeshBounds { get => navMeshBounds; }
    public Bounds SmallerBounds { get => smallerBounds; }
    public List<GameObject> AgentsInside { get => agentsInside; }

    Vector3 SMALLER_BOUNDS_DIFF = new Vector3(10f, 0f, 10f);

    void Awake() 
    {
        id = GUID.Generate();
        State = DynamicNavMeshState.Build;
        navMeshSurface = GetComponent<NavMeshSurface>();

        if (navMeshSurface.navMeshData == null)
        {
            navMeshSurface.navMeshData = new NavMeshData();
            NavMesh.AddNavMeshData(navMeshSurface.navMeshData);
        }
    }

    void Start() 
    {
        agents = World.GetActiveAgents();
        agentsInside = new List<GameObject>();
    }

    public void SetNavMeshBounds(Bounds bounds) {
        navMeshBounds = bounds;
        smallerBounds = new Bounds(bounds.center, bounds.size - SMALLER_BOUNDS_DIFF);
    }

    void Update() 
    {
        // only update if the navmeshsurface is not in the update process already
        if(State == DynamicNavMeshState.Ready) {
            agents = World.GetActiveAgents();
    
            foreach (var agent in agents) {
                if(navMeshBounds.Contains(agent.transform.position)) {
                    agentsInside.Add(agent);
                }
            }
            foreach (var agent in agentsInside) {
                if(!smallerBounds.Contains(agent.transform.position)) {
                    // mark for update
                    GlobalNavMeshController.MarkForUpdate();
                }
            }
        }
    }

    public void BuildNavMesh() {
        
        navMeshSurface.collectObjects = CollectObjects.Volume;
        navMeshSurface.center = Vector3.zero;
        navMeshSurface.size = new Vector3(
            navMeshBounds.size.x,
            0,
            navMeshBounds.size.z
        );

        navMeshSurface.BuildNavMesh();
        State = DynamicNavMeshState.Ready;

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
            if(!agent.GetComponent<NavMeshAgent>()) {
                agent.AddComponent<NavMeshAgent>();
            }
            agent.transform.position = hit.position;
            var navMeshAgent = agent.GetComponent<NavMeshAgent>();
            navMeshAgent.stoppingDistance = 0.1f;
            
            navMeshAgent.isStopped = false;
        } 
    }
}