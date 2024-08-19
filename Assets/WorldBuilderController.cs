using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class WorldBuilderController : MonoBehaviour
{   
    public WorldBuilderState State { get; private set; }
    public GameObject levelGenerator;
    public GameObject agentGenerator;
    public GameObject globalNavMesh;
    LevelGenerator levelGeneratorController;
    AgentGenerator agentGeneratorController;
    GlobalNavMeshController globalNavMeshController;

    void Awake()
    {
        levelGeneratorController = levelGenerator.GetComponent<LevelGenerator>();
        agentGeneratorController = agentGenerator.GetComponent<AgentGenerator>();
        globalNavMeshController = globalNavMesh.GetComponent<GlobalNavMeshController>();
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
        State = WorldBuilderState.Resetting;
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

        globalNavMeshController.MarkForUpdate();

        // Build the world
        yield return null;
    }

    IEnumerator ResetWorld()
    {
        globalNavMeshController.Reset();
        // Destroy the level
        levelGeneratorController.DestroyLevel();

        // Remove the agents
        agentGeneratorController.RemoveAgents();

        // Set the state to Standby
        State = WorldBuilderState.Standby;

        yield return null;
    }
}
