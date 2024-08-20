using System.Collections.Generic;
using UnityEngine;

/************************************************************************************
 * This class is used to store the world constants
 * It is used to store the world constants such as agent tag, wall tag, etc.
************************************************************************************/

public class World
{   
    public static Vector3 AGENT_NAVMESH_BOUNDS_SIZE = new Vector3(10f, 0f, 10f);
    public static Vector3 SMALLER_BOUNDS_DIFF = new Vector3(10f, 0f, 10f);
    public static float AGENT_WIDTH = 1f;
    public static float WALL_WIDTH = 2f;
    public const string AGENT_TAG = "Agent";
    public const string WALL_TAG = "Wall";
    public const string GAME_STATE_CONTROLLER_TAG = "GameStateController";

    public static List<GameObject> GetActiveAgents()
    {
        return new List<GameObject>(GameObject.FindGameObjectsWithTag(AGENT_TAG));
    }

    public static List<GameObject> GetAllAgents()
    {
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[];

        // Create a list to store all GameObjects with the specified tag
        List<GameObject> agents = new List<GameObject>();

        // Iterate through allObjects and add those with the specified tag to the agents list
        foreach (GameObject obj in allObjects)
        {
            if (obj.CompareTag(AGENT_TAG))
            {
                agents.Add(obj);
            }
        }

        return agents;
    }
}
