using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEditor;
using System;

public class DynamicNavMeshController : MonoBehaviour
{
    public Guid id;
    public DynamicNavMeshState state;
    public NavMeshSurface navMeshSurface;
    List<GameObject> agents;
    List<GameObject> agentsInside;
    Bounds navMeshBounds;
    Bounds smallerBounds;
    
    // PUBLIC GETTERS
    public Bounds NavMeshBounds { get => navMeshBounds; }
    public Bounds SmallerBounds { get => smallerBounds; }
    public List<GameObject> AgentsInside { get => agentsInside; }
    string AGENT_TAG = "Agent";

    void Awake() 
    {
        id = Guid.NewGuid();
        state = DynamicNavMeshState.Create;
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
        agents = GetAllAgents();
        agentsInside = new List<GameObject>();
    }

    void InitializeBounds() {
        navMeshBounds = new Bounds(transform.position, new Vector3(20f, 5f, 20f));
        smallerBounds = new Bounds(navMeshBounds.center, Vector3.Scale(navMeshBounds.size, new Vector3(0.7f, 1f, 0.7f)));
    }
    void UpdateBounds() {
        navMeshBounds.center = transform.position;
        smallerBounds.center = transform.position;
    }


    public void SetNavMeshBounds(Bounds bounds) {
        navMeshBounds = bounds;
        smallerBounds = new Bounds(bounds.center, Vector3.Scale(bounds.size, new Vector3(0.7f, 1f, 0.7f)));
    }
    public void AddAgent(GameObject agent) {
        agents.Add(agent);
    }

    void Update() 
    {
        // only update if the navmeshsurface is not in the update process already
        if(state == DynamicNavMeshState.Ready) {
            agents = GetAllAgents();
            agentsInside = new List<GameObject>();
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
    public void BuildNavMesh() {
        
        navMeshSurface.collectObjects = CollectObjects.Volume;
        navMeshSurface.center = Vector3.zero;
        navMeshSurface.size = new Vector3(
            navMeshBounds.size.x,
            0,
            navMeshBounds.size.z
        );

        navMeshSurface.BuildNavMesh();
        state = DynamicNavMeshState.Ready;

        agents = GetAllAgents();
        agentsInside = new List<GameObject>();
        foreach (var agent in agents) {
            if(navMeshBounds.Contains(agent.transform.position - Vector3.up)) {
                agentsInside.Add(agent);
            }
        }

        agentsInside.ForEach(agent => ReactivateAgentIfOnNavMesh(agent));
    }

    public void UpdateNavMeshStatic() {
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

    private List<GameObject> GetAllAgents() {
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[];

        // Create a list to store all GameObjects with the specified tag
        List<GameObject> agents = new List<GameObject>();

        // Iterate through allObjects and add those with the specified tag to the agents list
        foreach (GameObject obj in allObjects)
        {
            if (obj.CompareTag(AGENT_TAG))
            {
                agents.Add(obj);
            }
        }

        return agents;
    }
}