using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    public int width = 200;
	public int height = 200;

	public GameObject wall;

    public bool IsLevelGenerated { get; private set; } = false;

	List<GameObject> agents; // List of agents in the scene

	public GameObject agentGenerator;
	AgentGenerator agentGeneratorController;
	
	void Awake()
	{
		agentGeneratorController = agentGenerator.GetComponent<AgentGenerator>();
	}

	void Start() {
		agents = new List<GameObject>(GameObject.FindGameObjectsWithTag("Agent"));

        // Sort agents by their x and z positions
        agents.Sort((a, b) =>
        {
            // Compare by x first, then by z if x values are the same
            int compareX = a.transform.position.x.CompareTo(b.transform.position.x);
            return compareX != 0 ? compareX : a.transform.position.z.CompareTo(b.transform.position.z);
        });
	}

	// Create a grid based level
	public void GenerateLevel(float obstacleDensity)
	{
		// Loop over the grid
		for (int x = 0; x <= width; x+=2)
		{
			for (int z = 0; z <= height; z+=2)
			{
				// Would the wall intersect with an agent (bounds)?
				// If not, spawn the wall
				BoundingBoxXZ wallBounds = new BoundingBoxXZ(x-1, x+1, z-1, z+1);
				bool intersectsAgent = DoesWallIntersectAgent(wallBounds);


				if (UnityEngine.Random.value > Math.Clamp(1 - obstacleDensity, 0f, 0.9f) && !intersectsAgent)
				{
					// Spawn a wall
					Vector3 pos = new Vector3(x - width / 2f, 1.5f, z - height / 2f);
					Instantiate(wall, pos, Quaternion.identity, transform);
				}
			}
		}
		IsLevelGenerated = true;
	}
	public void DestroyLevel()
	{
		var walls = GameObject.FindGameObjectsWithTag("Wall");
		foreach (var wall in walls)
		{
			Destroy(wall);
		}
		IsLevelGenerated = false;
	}

	// possible point of failure
	private bool DoesWallIntersectAgent(BoundingBoxXZ wallBounds)
    {
        float minX = wallBounds.minX;
        float maxX = wallBounds.maxX;

        // Use binary search to find the first relevant agent based on x position
        int startIndex = FindFirstAgentInRange(minX);
        if (startIndex == -1) return false; // No agents in range

        // Iterate through the relevant agents within the x-range
        for (int i = startIndex; i < agents.Count; i++)
        {
            Vector3 agentPosition = agents[i].transform.position;

            // If the agent is outside the x range, we can stop checking further
            if (agentPosition.x > maxX) break;

            // Check if the agent is within the z range
            if (agentPosition.z >= wallBounds.minZ && agentPosition.z <= wallBounds.maxZ)
            {
                BoundingBoxXZ agentBounds = new BoundingBoxXZ(
					agentPosition.x - 0.5f, 
					agentPosition.x + 0.5f,
					agentPosition.z - 0.5f,
					agentPosition.z + 0.5f
				);

                // Perform precise bounding box check
                if (wallBounds.Intersects(agentBounds))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private int FindFirstAgentInRange(float minX)
    {
        int left = 0;
        int right = agents.Count - 1;

        while (left <= right)
        {
            int mid = (left + right) / 2;
            float agentX = agents[mid].transform.position.x;

            if (agentX < minX)
            {
                left = mid + 1;
            }
            else
            {
                right = mid - 1;
            }
        }

        // The left index now points to the first agent with x >= minX
        return (left < agents.Count && agents[left].transform.position.x >= minX) ? left : -1;
    }
}
