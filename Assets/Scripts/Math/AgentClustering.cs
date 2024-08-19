using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/**
 * This class is used to cluster agents based on their positions in the scene.
 * It groups agents based on their x and z positions, and then combines the clusters.
 */
public static class AgentClustering
{
    public static Dictionary<(int, int), AgentCluster> ClusterAgents() {
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
        var mergedClusters = MergeOverlappingClusters(combinedClusters);
        return mergedClusters;
    }

    static Dictionary<(int, int), AgentCluster> MergeOverlappingClusters(Dictionary<(int, int), AgentCluster> clusters) {
        bool clustersMerged;
        do {
            clustersMerged = false;
            var clustersToRemove = new HashSet<(int, int)>();
            var mergedClusters = new Dictionary<(int, int), AgentCluster>(clusters);

            foreach (var cluster1 in clusters) {
                if (clustersToRemove.Contains(cluster1.Key)) continue;

                foreach (var cluster2 in clusters) {
                    if (cluster1.Key == cluster2.Key || clustersToRemove.Contains(cluster2.Key)) continue;

                    if (ShouldMergeClusters(cluster1.Value, cluster2.Value)) {
                        // Merge cluster2 into cluster1
                        mergedClusters[cluster1.Key].AddRange(cluster2.Value);
                        clustersToRemove.Add(cluster2.Key);
                        clustersMerged = true;
                        
                        // Update the bounds for the merged cluster
                        mergedClusters[cluster1.Key] = mergedClusters[cluster1.Key]; // Recalculate bounds here

                        break; // Exit the inner loop to avoid modifying the collection while iterating
                    }
                }
            }

            // Remove merged clusters from the dictionary
            foreach (var key in clustersToRemove) {
                mergedClusters.Remove(key);
            }

            clusters = mergedClusters;

        } while (clustersMerged);

        return clusters;
    }


    static bool ShouldMergeClusters(AgentCluster cluster1, AgentCluster cluster2) {
        // Logic to determine if clusters should be merged, based on proximity or overlap
        // For example, comparing bounding boxes:
        var bounds1 = cluster1.GetBoundingBoxXZ();
        var bounds2 = cluster2.GetBoundingBoxXZ();
        return bounds1.Intersects(bounds2);
    }


    static List<AgentCluster> GroupAgents(AgentCluster agents, Vector3 direction) {
        List<AgentCluster> agentClusters = new List<AgentCluster>();
        AgentCluster currentCluster = new AgentCluster();

        for (int i = 0; i < agents.Count; i++) {
            if (i == 0) {
                currentCluster.Add(agents[i]);
                continue;
            }

            bool isInCluster = false;

            // Check by the specified axis
            if (direction == LinearAlgebra.XAxis) {
                float distanceX = Mathf.Abs(agents[i].transform.position.x - currentCluster[0].transform.position.x);
                if (distanceX < World.AGENT_NAVMESH_BOUNDS_SIZE.x) {
                    isInCluster = true;
                }
            } else if (direction == LinearAlgebra.ZAxis) {
                float distanceZ = Mathf.Abs(agents[i].transform.position.z - currentCluster[0].transform.position.z);
                if (distanceZ < World.AGENT_NAVMESH_BOUNDS_SIZE.z) {
                    isInCluster = true;
                }
            }

            if (isInCluster) {
                currentCluster.Add(agents[i]);
            } else {
                agentClusters.Add(currentCluster);
                currentCluster = new AgentCluster { agents[i] };
            }
        }

        agentClusters.Add(currentCluster);
        return agentClusters;
    }


    static Dictionary<(int, int), AgentCluster> CombineClusters
    (
        List<AgentCluster> agentClustersX, 
        List<AgentCluster> agentClustersZ
    )
    {
        // Create a dictionary to store the combined clusters
        Dictionary<(int, int), AgentCluster> combinedClusters = new Dictionary<(int, int), AgentCluster>();

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
                combinedClusters[combinedKey] = new AgentCluster();
            }
            combinedClusters[combinedKey].Add(agent);
        }
        return combinedClusters;
    }
}