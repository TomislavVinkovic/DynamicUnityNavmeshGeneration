 using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;

/*
    * Class used for managing navmeshes and agents
*/

public class GlobalNavMeshController : MonoBehaviour
{   
    public GameObject navMeshBuilderObject;
    NavMeshBuilder navMeshBuilder;

    List<GameObject> agents = new List<GameObject>();
    Dictionary<(int, int), DynamicNavMeshController> navMeshSurfaces = new Dictionary<(int, int), DynamicNavMeshController>();
    Queue<DynamicNavMeshController> updateQueue = new Queue<DynamicNavMeshController>();

    float UPDATE_DELAY = .1f;
    
    bool shouldUpdate = false;
    public bool ShouldUpdate { get => shouldUpdate; }
    public void MarkForUpdate() {
        shouldUpdate = true;
    }
    public void FinishUpdate() {
        shouldUpdate = false;
    }

    void Awake() {
        navMeshBuilder = navMeshBuilderObject.GetComponent<NavMeshBuilder>();
    }

    void Start() 
    {
        // Find all agents and save them into the array
        agents = World.GetActiveAgents();
        
        // mark for first update
        MarkForUpdate();

        // start the update queue
        StartCoroutine(ProcessUpdateQueue());
    }

    void Update() {
        if(shouldUpdate) {
            RecalculateNavMeshes();
        }
    }

    IEnumerator ProcessUpdateQueue()
    {
        while (true)
        {
            if (updateQueue.Count > 0)
            {
                var surfaceController = updateQueue.Dequeue();
                if(surfaceController.State == DynamicNavMeshState.Build)
                {
                    surfaceController.State = DynamicNavMeshState.Building;
                    surfaceController.BuildNavMesh();
                }
                
                else if(surfaceController.State == DynamicNavMeshState.Destroy)
                {
                    surfaceController.State = DynamicNavMeshState.Destroying;
                    Destroy(surfaceController.gameObject);
                }
            }
            yield return new WaitForSeconds(UPDATE_DELAY);
        }
    }

    void RecalculateNavMeshes() {

        // TODO: optimize further by only recalculating the affected navmeshes

        // cluster agents
        var agentClusters = AgentClustering.ClusterAgents();

        // deactivate all agents
        foreach(var agent in agents) {
            var navMeshAgent = agent.GetComponent<NavMeshAgent>();
            navMeshAgent.isStopped = true;
        }

        // mark all surfaces for destruction
        foreach( var (_, surfaceController) in navMeshSurfaces ) {
            surfaceController.State = DynamicNavMeshState.Destroy;
            updateQueue.Enqueue(surfaceController);
        }
        
        // from agent clusters, create navmesh surfaces
        navMeshSurfaces = navMeshBuilder.BuildNavMeshesFromAgentClusters(agentClusters);
        foreach( var (_, surfaceController) in navMeshSurfaces ) {
            // assign the global navmesh controller
            surfaceController.GlobalNavMeshController = this;
            updateQueue.Enqueue(surfaceController);
        }

        FinishUpdate();
    }
}