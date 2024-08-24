using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEditor;

/***
    * Class used for building a single dynamic navmesh
    * It is also used to check if any agents are so out of bounds so the navmesh needs to be rebuilt
***/
public class DynamicNavMeshController : MonoBehaviour
{
    public DynamicNavMeshState State;
    public NavMeshSurface navMeshSurface;
    public GlobalNavMeshController GlobalNavMeshController { get; set; }
    List<GameObject> agents;
    public List<GameObject> agentsInside;
    Bounds navMeshBounds;
    Bounds smallerBounds;

    // PUBLIC GETTERS
    public Bounds NavMeshBounds { get => navMeshBounds; }
    public List<GameObject> AgentsInside { get => agentsInside; }

    void Awake()
    {
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

    public void SetNavMeshBounds(Bounds bounds)
    {
        navMeshBounds = bounds;
        smallerBounds = new Bounds(
            new Vector3(
                bounds.center.x,
                bounds.center.y,
                bounds.center.z
            ),
            new Vector3(
                bounds.size.x - 1f,
                bounds.size.y,
                bounds.size.z - 1f
            )
        );
    }

    void Update()
    {
        // Only update if the navmeshsurface is not in the update process already
        if (State == DynamicNavMeshState.Ready)
        {
            foreach (var agent in agentsInside)
            {
                if (
                    !smallerBounds.Contains(agent.transform.position)
                )
                {
                    // Mark for update
                    GlobalNavMeshController.MarkForUpdate();
                    foreach(var agentInside in agentsInside)
                    {
                        agentInside.GetComponent<NavMeshAgent>().enabled = false;
                    }
                    break;
                }
            }
        }
    }

    public void BuildNavMesh()
    {
        navMeshSurface.collectObjects = CollectObjects.Volume;
        navMeshSurface.center = Vector3.zero;
        navMeshSurface.size = new Vector3(
            navMeshBounds.size.x,
            1f,
            navMeshBounds.size.z
        );

        navMeshSurface.BuildNavMesh();
        State = DynamicNavMeshState.Ready;

        foreach (var agent in agents)
        {
            if (navMeshBounds.Contains(agent.transform.position - Vector3.up))
            {
                agentsInside.Add(agent);
            }
        }
        agentsInside.ForEach(agent => ReactivateAgentIfOnNavMesh(agent));
    }

    private void ReactivateAgentIfOnNavMesh(GameObject agent)
    {
        if (!agent.GetComponent<NavMeshAgent>()) {
            var navMeshAgent = agent.AddComponent<NavMeshAgent>();
            navMeshAgent.stoppingDistance = 0.1f;
        }
        else {
            var navMeshAgent = agent.GetComponent<NavMeshAgent>();
            navMeshAgent.stoppingDistance = 0.1f;
            navMeshAgent.enabled = true;
        }
        
    }
}