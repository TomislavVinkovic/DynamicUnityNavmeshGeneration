using UnityEngine;

/************************************************************************************
 * This class is used to generate agents in the scene
 * It is used to place agents randomly in the scene
 ************************************************************************************/
public class AgentGenerator : MonoBehaviour
{
    public int width = 200;
	public int height = 200;

    public GameObject agent;

    public bool AreAgentsGenerated { get; private set; } = false;

    public void PlaceAgents(int numberOfAgents)
	{
		// Loop over the grid
        for (int i = 0; i < numberOfAgents; i++)
        {
            // Spawn an agent
            Vector3 pos = new Vector3(
                Random.Range(-width / 2f, width / 2f), 1.5f, 
                Random.Range(-height / 2f, height / 2f)
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
