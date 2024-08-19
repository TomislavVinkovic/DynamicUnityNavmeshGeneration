using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class WorldBuilderController : MonoBehaviour
{   
    public WorldBuilderState State { get; private set; }
    public GameObject levelGenerator;
    public GameObject agentGenerator;
    LevelGenerator levelGeneratorController;
    AgentGenerator agentGeneratorController;

    void Awake()
    {
        levelGeneratorController = levelGenerator.GetComponent<LevelGenerator>();
        agentGeneratorController = agentGenerator.GetComponent<AgentGenerator>();
        State = WorldBuilderState.Standby;
    }

    public void Build(int numberOfAgents, float obstacleDensity)
    {
        if(State == WorldBuilderState.Standby)
        {
            State = WorldBuilderState.Building;
            
            // Build the world using a coroutine
            StartCoroutine(BuildWorld(numberOfAgents, obstacleDensity));
        }
    }

    public void Reset()
    {
        StartCoroutine(ResetWorld());
    }

    IEnumerator BuildWorld(int numberOfAgents, float obstacleDensity)
    {   
        // Spawn the agents randomly in the world
        agentGeneratorController.PlaceAgents(numberOfAgents);

        // Generate the level
        levelGeneratorController.GenerateLevel(obstacleDensity);

        // Set the state to Ready
        State = WorldBuilderState.Ready;

        // Build the world
        yield return null;
    }

    IEnumerator ResetWorld()
    {
        // Destroy the level
        levelGeneratorController.DestroyLevel();

        // Remove the agents
        agentGeneratorController.RemoveAgents();

        // Set the state to Standby
        State = WorldBuilderState.Standby;

        yield return null;
    }
}
