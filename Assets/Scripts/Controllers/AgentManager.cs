using System.Collections.Generic;
using UnityEngine;

/************************************************************************************
 * This class is used to manage the agents in the scene
 * It is used to select, move and hover over agents
************************************************************************************/
public class AgentManager : MonoBehaviour
{
    public List<Vector3> AgentWaypoints {get; private set;}
    public Material selectedMaterial;
    public Material defaultMaterial;
    public Material hoverMaterial;

    public GameObject gameStateObject;
    GameStateController gameStateController;

    GameObject selectedAgent;
    GameObject hoveredAgent;
    Camera mainCamera;

    void Awake()
    {
        gameStateController = gameStateObject.GetComponent<GameStateController>();
        AgentWaypoints = new List<Vector3> {
            new Vector3(-34.5f, 1f, 67.3f),
            new Vector3(53.1f, 1f, -12.7f),
            new Vector3(-58.6f, 1f, 19.4f),
            new Vector3(42.9f, 1f, -59.3f),
            new Vector3(-12.2f, 1f, 34.7f),
            new Vector3(27.8f, 1f, 61.5f),
            new Vector3(-47.6f, 1f, -27.1f),
            new Vector3(65.3f, 1f, 3.2f),
            new Vector3(-69.9f, 1f, -44.1f),
            new Vector3(31.7f, 1f, 22.5f),
            new Vector3(-23.4f, 1f, -68.7f),
            new Vector3(12.6f, 1f, 48.9f),
            new Vector3(-67.1f, 1f, 10.3f),
            new Vector3(55.4f, 1f, -31.8f),
            new Vector3(8.3f, 1f, 15.7f)
        };


    }

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        if(!gameStateController.RandomMovementEnabled) {
            HandleAgentSelection();
            HandleMovementInput();
            HandleAgentHover();
        }
    }

    void HandleAgentSelection()
    {
        if (Input.GetMouseButtonDown(0)) // Left-click to select an agent
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.CompareTag(World.AGENT_TAG)) 
                {   
                    Renderer selectedAgentRenderer = hit.collider.GetComponentInParent<Renderer>();
                    GameObject agent = hit.collider.gameObject;

                    if (agent != null) 
                    {
                        if(selectedAgent != null)
                        {
                            selectedAgent.GetComponent<Renderer>().material = defaultMaterial;
                        }
                        selectedAgent = agent;
                        selectedAgentRenderer.material = selectedMaterial;
                    }
                }
            }
        }
    }

    void HandleMovementInput()
    {
        if (selectedAgent != null && Input.GetMouseButtonDown(1))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider != null && hit.collider.CompareTag("Ground"))
                {
                    AgentMovement agentMovement = selectedAgent.GetComponent<AgentMovement>();
                    agentMovement.MoveToPosition(hit.point);
                }
            }
        }
    }

    void HandleAgentHover()
    {
        if (Input.GetMouseButton(0)) return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.CompareTag(World.AGENT_TAG))
            {
                GameObject hoveredAgentNew = hit.collider.gameObject;
                Renderer hoveredAgentNewRenderer = hit.collider.GetComponentInParent<Renderer>();

                if (hoveredAgent != null && hoveredAgent != selectedAgent)
                {
                    hoveredAgent.GetComponent<Renderer>().material = defaultMaterial;
                    hoveredAgent = hoveredAgentNew;
                    hoveredAgentNewRenderer.material = hoverMaterial;
                }
                if(hoveredAgent == selectedAgent)
                {
                    hoveredAgentNewRenderer.material = selectedMaterial;
                    hoveredAgent = hoveredAgentNew;
                }
                if(hoveredAgent == null)
                {
                    hoveredAgentNewRenderer.material = hoverMaterial;
                    hoveredAgent = hoveredAgentNew;
                }
            }
            else {
                if(hoveredAgent != null && hoveredAgent != selectedAgent) {
                    hoveredAgent.GetComponent<Renderer>().material = defaultMaterial;
                    hoveredAgent = null;
                }
            }
        }
    }
}