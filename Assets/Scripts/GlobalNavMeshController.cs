using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using System;

public class GlobalNavMeshController : MonoBehaviour
{
    public GameObject dynamicNavMeshPrefab;
    List<GameObject> agents = new List<GameObject>();
    List<DynamicNavMeshController> navMeshSurfaces = new List<DynamicNavMeshController>();
    Queue<DynamicNavMeshController> updateQueue = new Queue<DynamicNavMeshController>();

    private float updateDelay = .1f;

    void Start() 
    {
        // Find all agents and save them into the array
        agents = new List<GameObject>(GameObject.FindGameObjectsWithTag("Agent"));

        foreach (var agent in agents) {
            GameObject navMeshSurface = InstantiateDynamicNavMeshSurface(new List<GameObject>() {agent});
            navMeshSurfaces.Add(navMeshSurface.GetComponent<DynamicNavMeshController>());
        }

        StartCoroutine(ProcessUpdateQueue());
    }

    void Update() {
        foreach (var navMeshSurface in navMeshSurfaces) {
            if(navMeshSurface.state == DynamicNavMeshState.Create || navMeshSurface.state == DynamicNavMeshState.Update) {
                navMeshSurface.state = 
                    navMeshSurface.state == DynamicNavMeshState.Create ? DynamicNavMeshState.Creating : DynamicNavMeshState.Updating;
                updateQueue.Enqueue(navMeshSurface);
            }
        }
    }

    // get a list of agents and instantiate a dynamic nav mesh surface
    // in the mean of their positions
    GameObject InstantiateDynamicNavMeshSurface(List<GameObject> agents) {
        Vector3 center = LinearAlgebra.GetMeanInSpace(
            agents.ConvertAll(agent => agent.transform.position)
        );
        // maybe change later
        center.y = 0;

        return Instantiate(
            dynamicNavMeshPrefab,
            center,
            Quaternion.identity
        );
    }

    IEnumerator ProcessUpdateQueue()
    {
        while (true)
        {
            if (updateQueue.Count > 0)
            {
                var surfaceController = updateQueue.Dequeue();
                if(surfaceController.state == DynamicNavMeshState.Creating)
                {
                    surfaceController.BuildNavMesh();
                }
                    
                else if(surfaceController.state == DynamicNavMeshState.Updating)
                {
                    surfaceController.UpdateNavMesh();
                }
            }
            yield return new WaitForSeconds(updateDelay);
        }
    }
}
