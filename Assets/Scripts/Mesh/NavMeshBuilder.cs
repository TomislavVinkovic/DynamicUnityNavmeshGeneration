using System.Collections.Generic;
using UnityEngine;

/*
    * Class used for building navmeshes from agent clusters
*/
public class NavMeshBuilder : MonoBehaviour
{
    public GameObject dynamicNavMeshPrefab;

    public Dictionary<(int, int), DynamicNavMeshController> BuildNavMeshesFromAgentClusters(
        Dictionary<(int, int), AgentCluster> agentClusters
    )
    {
        var navMeshSurfaces = new Dictionary<(int, int), DynamicNavMeshController>();

        foreach (var (key, cluster) in agentClusters)
        {
            BoundingBoxXZ boundingBox = cluster.GetBoundingBoxXZ();
            var navMeshSurface = InstantiateDynamicNavMeshSurface(boundingBox.center);
            var surfaceController = navMeshSurface.GetComponent<DynamicNavMeshController>();

            surfaceController.SetNavMeshBounds(
                new Bounds(boundingBox.center, boundingBox.size)
            );
            navMeshSurfaces[key] = surfaceController;
        }

        return navMeshSurfaces;
    }

    GameObject InstantiateDynamicNavMeshSurface(Vector3 center) {
        // maybe change later
        center.y = 1.5f;

        var navMeshSurface = Instantiate (
            dynamicNavMeshPrefab,
            center,
            Quaternion.identity
        );

        // return the navMeshSurface
        return navMeshSurface;
    }
}
