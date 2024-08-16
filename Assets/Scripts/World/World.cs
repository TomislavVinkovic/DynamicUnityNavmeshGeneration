using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    * Class used for storing world related constants
*/

public static class World
{   
    public static Vector3 AGENT_NAVMESH_BOUNDS_SIZE = new Vector3(10f, 0f, 10f);
    public static Vector3 SMALLER_BOUNDS_DIFF = new Vector3(10f, 0f, 10f);
    public static float BOUNDING_BOX_PADDING_X = 20f;
    public static float BOUNDING_BOX_PADDING_Z = 20f;
    const string AGENT_TAG = "Agent";
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
