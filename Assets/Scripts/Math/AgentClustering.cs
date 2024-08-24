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
        return combinedClusters;
    }


    static List<AgentCluster> GroupAgents(AgentCluster agents, Vector3 direction) {

        var gameStateController = GameObject.FindGameObjectWithTag(World.GAME_STATE_CONTROLLER_TAG).GetComponent<GameStateController>();

        List<AgentCluster> agentClusters = new List<AgentCluster>();
        AgentCluster currentCluster = new AgentCluster();

        float d = 2*gameStateController.AgentNavmeshSize;
        for (int i = 0; i < agents.Count; i++) {
            if (i == 0) {
                currentCluster.Add(agents[i]);
                continue;
            }

            bool isInCluster = false;

            // Check by the specified axis
            if (direction == LinearAlgebra.XAxis) {
                float distanceX = Mathf.Abs(agents[i].transform.position.x - currentCluster.Last().transform.position.x);
                if (distanceX < d) {
                    isInCluster = true;
                }
            } else if (direction == LinearAlgebra.ZAxis) {
                float distanceZ = Mathf.Abs(agents[i].transform.position.z - currentCluster.Last().transform.position.z);
                if (distanceZ < d) {
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