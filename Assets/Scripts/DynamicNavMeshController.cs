using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEditor;

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

        // Initialize a default bounds if needed
        InitializeBounds();
    }

    void Start() 
    {
        agents = new List<GameObject>(GameObject.FindGameObjectsWithTag("Agent"));
        agentsInside = new List<GameObject>();
    }

    void InitializeBounds() {
        navMeshBounds = new Bounds(transform.position, new Vector3(20f, 5f, 20f));
        smallerBounds = new Bounds(navMeshBounds.center, Vector3.Scale(navMeshBounds.size, new Vector3(0.7f, 1f, 0.7f)));
    }

    public void SetNavMeshBounds(Bounds bounds) {
        navMeshBounds = bounds;
        smallerBounds = new Bounds(bounds.center, Vector3.Scale(bounds.size, new Vector3(0.7f, 1f, 0.7f)));
    }

    void UpdateBounds() {
        navMeshBounds.center = transform.position;
        smallerBounds.center = transform.position;
    }

    void Update() 
    {
        // only update if the navmeshsurface is not in the update process already
        if(State == DynamicNavMeshState.Ready) {
            agents = new List<GameObject>(GameObject.FindGameObjectsWithTag("Agent"));
    
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
            
            agent.SetActive(true);
        } 
    }
}