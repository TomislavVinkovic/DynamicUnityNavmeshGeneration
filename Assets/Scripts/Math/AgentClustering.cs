using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/*
    * Class used for agent clustering operations
*/
public static class AgentClustering
{
    public static Dictionary<(int, int), List<GameObject>> ClusterAgents() {
        // Get all agents
        var agents = World.GetActiveAgents();

        // Sort and group agents by x-axis
        var agentsX = agents.OrderBy(agent => agent.transform.position.x).ToList();
        var agentClustersX = GroupAgents(agentsX, LinearAlgebra.XAxis);

        // Sort and group agents by z-axis
        var agentsZ = agents.OrderBy(agent => agent.transform.position.z).ToList();
        var agentClustersZ = GroupAgents(agentsZ, LinearAlgebra.ZAxis);

        // Combine clusters based on x and z axes
        var combinedClusters = CombineClusters(agentClustersX, agentClustersZ);

        // Merge nearby clusters
        var mergedClusters = MergeNearbyClusters(combinedClusters);

        return mergedClusters;
    }

    static Dictionary<(int, int), List<GameObject>> MergeNearbyClusters(Dictionary<(int, int), List<GameObject>> clusters) {
        var clustersToRemove = new HashSet<(int, int)>();
        var mergedClusters = new Dictionary<(int, int), List<GameObject>>(clusters);

        foreach (var cluster1 in clusters) {
            if (clustersToRemove.Contains(cluster1.Key)) continue;

            foreach (var cluster2 in clusters) {
                if (cluster1.Key == cluster2.Key || clustersToRemove.Contains(cluster2.Key)) continue;

                if (ShouldMergeClusters(cluster1.Value, cluster2.Value)) {
                    // Merge cluster2 into cluster1
                    mergedClusters[cluster1.Key].AddRange(cluster2.Value);
                    clustersToRemove.Add(cluster2.Key);
                }
            }
        }

        // Remove merged clusters from the dictionary
        foreach (var key in clustersToRemove) {
            mergedClusters.Remove(key);
        }

        return mergedClusters;
    }


    static bool ShouldMergeClusters(List<GameObject> cluster1, List<GameObject> cluster2) {
        // Logic to determine if clusters should be merged, based on proximity or overlap
        // For example, comparing bounding boxes:
        var bounds1 = GetBoundingBox(cluster1);
        var bounds2 = GetBoundingBox(cluster2);
        return bounds1.ToBounds().Intersects(bounds2.ToBounds());
    }

    static BoundingBoxXZ GetBoundingBox(List<GameObject> agentCluster) {
        BoundingBoxXZ boundingBox = new BoundingBoxXZ();

        foreach (var agent in agentCluster) {
            boundingBox.minX = Mathf.Min(boundingBox.minX, agent.transform.position.x) - 20f;
            boundingBox.maxX = Mathf.Max(boundingBox.maxX, agent.transform.position.x) + 20f;
            boundingBox.minZ = Mathf.Min(boundingBox.minZ, agent.transform.position.z) - 20f;
            boundingBox.maxZ = Mathf.Max(boundingBox.maxZ, agent.transform.position.z) + 20f;
        }

        return boundingBox;
    }



    static List<List<GameObject>> GroupAgents(List<GameObject> agents, Vector3 direction) {
        List<List<GameObject>> agentClusters = new List<List<GameObject>>();
        List<GameObject> currentCluster = new List<GameObject>();

        // Define a tolerance threshold
        float tolerance = 5.0f; // Adjust this based on your needs

        for (int i = 0; i < agents.Count; i++) {
            if (i == 0) {
                currentCluster.Add(agents[i]);
                continue;
            }

            bool isInCluster = false;

            // Check by the specified axis
            if (direction == LinearAlgebra.XAxis) {
                float distanceX = Mathf.Abs(agents[i].transform.position.x - currentCluster[0].transform.position.x);
                if (distanceX < World.AGENT_NAVMESH_BOUNDS_SIZE.x + tolerance) {
                    isInCluster = true;
                }
            } else if (direction == LinearAlgebra.ZAxis) {
                float distanceZ = Mathf.Abs(agents[i].transform.position.z - currentCluster[0].transform.position.z);
                if (distanceZ < World.AGENT_NAVMESH_BOUNDS_SIZE.z + tolerance) {
                    isInCluster = true;
                }
            }

            if (isInCluster) {
                currentCluster.Add(agents[i]);
            } else {
                agentClusters.Add(currentCluster);
                currentCluster = new List<GameObject> { agents[i] };
            }
        }

        agentClusters.Add(currentCluster);
        return agentClusters;
    }


    static Dictionary<(int, int), List<GameObject>> CombineClusters
    (
        List<List<GameObject>> agentClustersX, 
        List<List<GameObject>> agentClustersZ
    )
    {
        // Create a dictionary to store the combined clusters
        Dictionary<(int, int), List<GameObject>> combinedClusters = new Dictionary<(int, int), List<GameObject>>();

        // Map each GameObject to its cluster index in the x axis
        Dictionary<GameObject, int> xClusterMap = new Dictionary<GameObject, int>();
        for (int i = 0; i < agentClustersX.Count; i++)
        {
            foreach (var agent in agentClustersX[i])
            {
                xClusterMap[agent] = i;
            }
        }

        // Map each GameObject to its cluster index in the z axis
        Dictionary<GameObject, int> zClusterMap = new Dictionary<GameObject, int>();
        for (int i = 0; i < agentClustersZ.Count; i++)
        {
            foreach (var agent in agentClustersZ[i])
            {
                zClusterMap[agent] = i;
            }
        }

        // Combine clusters
        foreach (var agent in xClusterMap.Keys)
        {
            int xCluster = xClusterMap[agent];
            int zCluster = zClusterMap[agent];

            var combinedKey = (xCluster, zCluster);
            if (!combinedClusters.ContainsKey(combinedKey))
            {
                combinedClusters[combinedKey] = new List<GameObject>();
            }
            combinedClusters[combinedKey].Add(agent);
        }
        return combinedClusters;
    }
}