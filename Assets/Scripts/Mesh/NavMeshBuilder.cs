using System.Collections.Generic;
using UnityEngine;

public class NavMeshBuilder : MonoBehaviour
{
    float BOUNDING_BOX_PADDING_X = 10f;
    float BOUNDING_BOX_PADDING_Z = 10f;
    public GameObject dynamicNavMeshPrefab;

    public Dictionary<(int, int), DynamicNavMeshController> BuildNavMeshesFromAgentClusters(
        Dictionary<(int, int), List<GameObject>> agentClusters
    )
    {
        var navMeshSurfaces = new Dictionary<(int, int), DynamicNavMeshController>();

        foreach (var (key, cluster) in agentClusters)
        {
            BoundingBoxXZ boundingBox = GetBoundingBox(cluster);
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
        center.y = 0;

        var navMeshSurface = Instantiate (
            dynamicNavMeshPrefab,
            center,
            Quaternion.identity
        );

        // return the navMeshSurface
        return navMeshSurface;
    }

    BoundingBoxXZ GetBoundingBox(List<GameObject> agentCluster) {
        
        BoundingBoxXZ boundingBox = new BoundingBoxXZ();

        foreach(var agent in agentCluster) {
            boundingBox.minX = Mathf.Min(boundingBox.minX, agent.transform.position.x);
            boundingBox.maxX = Mathf.Max(boundingBox.maxX, agent.transform.position.x);
            boundingBox.minZ = Mathf.Min(boundingBox.minZ, agent.transform.position.z);
            boundingBox.maxZ = Mathf.Max(boundingBox.maxZ, agent.transform.position.z);
        }

        // increase the bounding box size by some fixed padding
        boundingBox.minX -= BOUNDING_BOX_PADDING_X;
        boundingBox.maxX += BOUNDING_BOX_PADDING_X;
        boundingBox.minZ -= BOUNDING_BOX_PADDING_Z;
        boundingBox.maxZ += BOUNDING_BOX_PADDING_Z;

        return boundingBox;
    }
}
