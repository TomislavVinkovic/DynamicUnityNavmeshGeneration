using UnityEngine;

/************************************************************************************
 * This class is used to generate agents in the scene
 * It is used to place agents randomly in the scene
 ************************************************************************************/
public class AgentGenerator : MonoBehaviour
{
    public GameObject agent;

    GameStateController gameStateController;

    public bool AreAgentsGenerated { get; private set; } = false;

    void Awake()
    {
        gameStateController = GameObject.FindWithTag(World.GAME_STATE_CONTROLLER_TAG).GetComponent<GameStateController>();
    }

    public void PlaceAgents(int numberOfAgents)
	{
		// Loop over the grid
        for (int i = 0; i < numberOfAgents; i++)
        {
            // Spawn an agent
            Vector3 pos = new Vector3(
                Random.Range(-gameStateController.PlaneWidth / 2f, gameStateController.PlaneWidth / 2f), 1.5f, 
                Random.Range(-gameStateController.PlaneHeight / 2f, gameStateController.PlaneHeight / 2f)
            );
            Instantiate(agent, pos, Quaternion.identity, transform);
        }
        AreAgentsGenerated = true;
    }

    public void RemoveAgents()
    {
        var agents = World.GetActiveAgents();
        foreach (var agent in agents)
        {
            if(agent == null) continue;
            Destroy(agent.gameObject);
        }
    }
}