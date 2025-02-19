using System;
using System.Collections.Generic;
using UnityEngine;

/***
* This class is used to generate a level with walls and obstacles
***/
public class LevelGenerator : MonoBehaviour
{   
    public GameObject wall;

    public bool IsLevelGenerated { get; private set; } = false;

	List<GameObject> agents; // List of agents in the scene

	public GameObject agentGenerator;
    GameStateController gameStateController;
    AgentManagerController agentManager;

    private int WorldWidth;
    private int WorldHeight;
	
	void Awake()
	{
        gameStateController = GameObject.FindWithTag(World.GAME_STATE_CONTROLLER_TAG).GetComponent<GameStateController>();
        agentManager = GameObject.FindWithTag(World.AGENT_MANAGER_TAG).GetComponent<AgentManagerController>();

        WorldWidth = gameStateController.PlaneWidth - 10;
        WorldHeight = gameStateController.PlaneHeight - 10;
	}

	// Create a grid based level
	public void GenerateLevel(float obstacleDensity)
	{

        agents = World.GetActiveAgents();

        // Sort agents by their x and z positions
        agents.Sort((a, b) =>
        {
            // Compare by x first, then by z if x values are the same
            int compareX = a.transform.position.x.CompareTo(b.transform.position.x);
            return compareX != 0 ? compareX : a.transform.position.z.CompareTo(b.transform.position.z);
        });

        float wallWidth = World.WALL_WIDTH;
		// Loop over the grid
		for (int x = 0; x <= WorldWidth; x+=(int)wallWidth)
		{
			for (int z = 0; z <= WorldHeight; z+=(int)wallWidth)
			{
				
				if (UnityEngine.Random.value > 1 - obstacleDensity)
				{
                    BoundingBoxXZ wallBounds = new BoundingBoxXZ(
                        x-wallWidth/2, 
                        x+wallWidth/2, 
                        z-wallWidth/2, 
                        z+wallWidth/2
                    );
                    bool intersectsWaypoint = DoesWallIntersectWaypoint(wallBounds);
                    if(intersectsWaypoint) continue;
                    
                    bool intersectsAgent = DoesWallIntersectAgent(wallBounds);
                    if(intersectsAgent) continue;


					// Spawn a wall
					Vector3 pos = new Vector3(
                        x - WorldWidth / 2f, 
                        1.5f, 
                        z - WorldHeight / 2f
                    );
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
            if(wall == null) continue;
			Destroy(wall);
		}
		IsLevelGenerated = false;
	}
    
    bool DoesWallIntersectWaypoint(BoundingBoxXZ wallBounds)
    {
        // Get the waypoints from the agent manager
        List<Vector3> waypoints = agentManager.AgentWaypoints;

        // Check if the wall intersects with any of the waypoints
        foreach (Vector3 waypoint in waypoints)
        {
            if (wallBounds.Intersects(waypoint))
            {
                return true;
            }
        }

        return false;
    }

	// possible point of failure
	private bool DoesWallIntersectAgent(BoundingBoxXZ wallBounds)
    {

        float agentWidth = World.AGENT_WIDTH;

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
					agentPosition.x - agentWidth / 2, 
					agentPosition.x + agentWidth / 2,
					agentPosition.z - agentWidth / 2,
					agentPosition.z + agentWidth / 2
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
        // Binary search to find the first agent with x >= minX
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
