using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;

/***
    * Class used for building a single dynamic navmesh
    * It is also used to check if any agents are so out of bounds so the navmesh needs to be rebuilt
***/
public class GlobalNavMeshController : MonoBehaviour
{   
    public GameObject navMeshBuilderObject;
    public GameObject navMeshMeshGeneratorObject;

    NavMeshBuilder navMeshBuilder;
    NavMeshMeshGenerator navMeshMeshGenerator;

    Dictionary<(int, int), DynamicNavMeshController> navMeshSurfaces = new Dictionary<(int, int), DynamicNavMeshController>();
    Queue<DynamicNavMeshController> updateQueue = new Queue<DynamicNavMeshController>();

    public GameObject worldBuilderObject;
    WorldBuilderController worldBuilderController;

    public bool ShouldUpdate { get; private set; }
    public void MarkForUpdate() {
        ShouldUpdate = true;
    }
    public void FinishUpdate() {
        ShouldUpdate = false;
    }

    void Awake() {
        navMeshBuilder = navMeshBuilderObject.GetComponent<NavMeshBuilder>();
        navMeshMeshGenerator = navMeshMeshGeneratorObject.GetComponent<NavMeshMeshGenerator>();
        worldBuilderController = worldBuilderObject.GetComponent<WorldBuilderController>();
    }

    void Start() 
    {        
        // mark for first update
        MarkForUpdate();

        // start the update queue
        StartCoroutine(ProcessUpdateQueue());
    }

    void Update() {
        if(ShouldUpdate && worldBuilderController.State == WorldBuilderState.Ready) {
            RecalculateNavMeshes();
        }
    }

    public void Reset() {
        foreach( var (_, surfaceController) in navMeshSurfaces ) {
            surfaceController.State = DynamicNavMeshState.Destroy;
            updateQueue.Enqueue(surfaceController);
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
                    navMeshMeshGenerator.GenerateNavMeshMesh();
                }
                
                else if(surfaceController.State == DynamicNavMeshState.Destroy)
                {
                    surfaceController.State = DynamicNavMeshState.Destroying;
                    if(surfaceController != null && surfaceController.gameObject != null)
                    {
                        Destroy(surfaceController.gameObject);
                    }
                    navMeshMeshGenerator.GenerateNavMeshMesh();
                }
            }
            yield return null;
        }
    }

    void RecalculateNavMeshes() {
        var agents = World.GetActiveAgents();
        foreach( var agent in agents ) {
            agent.GetComponent<NavMeshAgent>().enabled = false;
        }
        // cluster agents
        var agentClusters = AgentClustering.ClusterAgents();

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