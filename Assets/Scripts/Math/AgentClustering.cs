using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/*
    * Class used for agent clustering operations
*/
public static class AgentClustering
{
    public static Dictionary<(int, int), List<GameObject>> ClusterAgents () {
        // get all agents
        var agents = World.GetActiveAgents();
        // sort agents by x axis
        var agentsX = agents.OrderBy(agent => agent.transform.position.x).ToList();
        // sort agents by z axis
        var agentsZ = agents.OrderBy(agent => agent.transform.position.z).ToList();

        // group agents by x axis
        List<List<GameObject>> agentClustersX = GroupAgents(agentsX, LinearAlgebra.XAxis);
        // group agents by z axis
        List<List<GameObject>> agentClustersZ = GroupAgents(agentsZ, LinearAlgebra.ZAxis);

        return CombineClusters(agentClustersX, agentClustersZ);
    }

    static List<List<GameObject>> GroupAgents(List<GameObject> agents, Vector3 direction) {

        List<List<GameObject>> agentClusters = new List<List<GameObject>>();

        List<GameObject> currentCluster = new List<GameObject>();
        for (int i = 0; i < agents.Count; i++) {
            if (i == 0) {
                currentCluster.Add(agents[i]);
                continue;
            }

            bool isInCluster = false;

            // check by x axis
            if(direction == LinearAlgebra.XAxis) {
                if(Math.Abs(agents[i].transform.position.x - currentCluster[0].transform.position.x) < World.AGENT_NAVMESH_BOUNDS_SIZE.x) {
                    isInCluster = true;
                }
            }
            else if(direction == LinearAlgebra.ZAxis) {
                if(Math.Abs(agents[i].transform.position.z - currentCluster[0].transform.position.z) < World.AGENT_NAVMESH_BOUNDS_SIZE.x) {
                    isInCluster = true;
                }
            }

            if(isInCluster) {
                currentCluster.Add(agents[i]);
            } 
            else {
                agentClusters.Add(currentCluster);
                currentCluster = new List<GameObject>
                {
                    agents[i]
                };
            }
        }

        // add the last cluster
        agentClusters.Add(currentCluster);
        return agentClusters;
    }

    static Dictionary<(int, int), List<GameObject>> CombineClusters(
        List<List<GameObject>> agentClustersX, 
        List<List<GameObject>> agentClustersZ)
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