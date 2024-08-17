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
    public GameObject navMeshMeshGeneratorObject;

    NavMeshBuilder navMeshBuilder;
    NavMeshMeshGenerator navMeshMeshGenerator;

    Dictionary<(int, int), DynamicNavMeshController> navMeshSurfaces = new Dictionary<(int, int), DynamicNavMeshController>();
    Queue<DynamicNavMeshController> updateQueue = new Queue<DynamicNavMeshController>();

    public LevelGenerator LevelGenerator;

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
    }

    void Start() 
    {        
        // mark for first update
        MarkForUpdate();

        // start the update queue
        StartCoroutine(ProcessUpdateQueue());
    }

    void Update() {
        if(ShouldUpdate && LevelGenerator.IsLevelGenerated) {
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
                    navMeshMeshGenerator.GenerateNavMeshMesh();
                }
                
                else if(surfaceController.State == DynamicNavMeshState.Destroy)
                {
                    surfaceController.State = DynamicNavMeshState.Destroying;
                    Destroy(surfaceController.gameObject);
                }
            }
            yield return null;
        }
    }

    void RecalculateNavMeshes() {
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