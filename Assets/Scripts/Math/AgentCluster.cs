using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * This class is a wrapper for a list of GameObjects. It provides additional functionality to work with the list.
 * It also provides an implicit conversion from List<GameObject> to AgentCluster.
 */
public class AgentCluster : IEnumerable<GameObject>
{
    // Internal list to hold the GameObjects
    private List<GameObject> gameObjects = new List<GameObject>();
    public float boundingBoxPadding = 5f;

    public AgentCluster() { 
        GameStateController gameStateController = GameObject.FindWithTag(World.GAME_STATE_CONTROLLER_TAG).GetComponent<GameStateController>();
        boundingBoxPadding = gameStateController.AgentNavmeshSize;
    }

    // Implicit conversion from List<GameObject> to GameObjectListWrapper
    public static implicit operator AgentCluster(List<GameObject> objects)
    {
        // Create a new instance of the wrapper and set the internal list
        var wrapper = new AgentCluster();
        wrapper.SetList(objects);
        
        return wrapper;
    }

    // Method to set the internal list
    public void SetList(List<GameObject> objects)
    {
        gameObjects = objects ?? new List<GameObject>(); // Ensure it's not null
    }

    // Method to get the internal list
    public List<GameObject> GetList()
    {
        return gameObjects;
    }

    // Indexer to access elements directly
    public GameObject this[int index]
    {
        get => gameObjects[index];
        set => gameObjects[index] = value;
    }

    // Additional methods to work with the list
    public void Add(GameObject obj)
    {
        gameObjects.Add(obj);
    }

    public void Remove(GameObject obj)
    {
        gameObjects.Remove(obj);
    }

    public void AddRange(IEnumerable<GameObject> objects)
    {
        gameObjects.AddRange(objects);
    }
    public IEnumerator<GameObject> GetEnumerator()
    {
        return gameObjects.GetEnumerator();
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public BoundingBoxXZ GetBoundingBoxXZ()
    {
        BoundingBoxXZ boundingBox = new BoundingBoxXZ();
        foreach(var agent in gameObjects) {
            boundingBox.minX = Mathf.Min(boundingBox.minX, agent.transform.position.x);
            boundingBox.maxX = Mathf.Max(boundingBox.maxX, agent.transform.position.x);
            boundingBox.minZ = Mathf.Min(boundingBox.minZ, agent.transform.position.z);
            boundingBox.maxZ = Mathf.Max(boundingBox.maxZ, agent.transform.position.z);
        }
        boundingBox.minX -= boundingBoxPadding;
        boundingBox.maxX += boundingBoxPadding;
        boundingBox.minZ -= boundingBoxPadding;
        boundingBox.maxZ += boundingBoxPadding;

        return boundingBox;
    }

    public int Count => gameObjects.Count;
}
