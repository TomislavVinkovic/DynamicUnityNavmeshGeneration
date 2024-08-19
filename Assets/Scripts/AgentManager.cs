using UnityEngine;

/************************************************************************************
 * This class is used to manage the agents in the scene
 * It is used to select, move and hover over agents
************************************************************************************/
public class AgentManager : MonoBehaviour
{
    public static AgentManager Instance { get; private set; }
    public Material selectedMaterial;
    public Material defaultMaterial;
    public Material hoverMaterial;

    GameObject selectedAgent;
    GameObject hoveredAgent;
    Camera mainCamera;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        HandleAgentSelection();
        HandleMovementInput();
        HandleAgentHover();
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