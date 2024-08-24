using System.Collections;
using UnityEngine;

/************************************************************************************
 * This class is used to control the world building process
 * It is used to generate the level and agents in the scene
 ************************************************************************************/
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
            StartCoroutine(BuildWorld(numberOfAgents, obstacleDensity));
        }
    }

    IEnumerator BuildWorld(int numberOfAgents, float obstacleDensity)
    {   
        agentGeneratorController.PlaceAgents(numberOfAgents);
        levelGeneratorController.GenerateLevel(obstacleDensity);

        State = WorldBuilderState.Ready;

        globalNavMeshController.MarkForUpdate();

        yield return null;
    }

    public void Reset()
    {
        State = WorldBuilderState.Resetting;
        StartCoroutine(ResetWorld());
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
