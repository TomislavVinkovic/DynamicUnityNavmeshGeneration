using UnityEngine;

public class AgentManager : MonoBehaviour
{
    public static AgentManager Instance { get; private set; } // Singleton pattern for easy access

    private AgentMovement selectedAgent; // The currently selected agent
    private Camera mainCamera;

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
    }

    void HandleAgentSelection()
    {
        if (Input.GetMouseButtonDown(0)) // Left-click to select an agent
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if(hit.collider.CompareTag(World.AGENT_TAG)) {
                    AgentMovement agent = hit.collider.GetComponentInParent<AgentMovement>();

                    if (agent != null) {
                        selectedAgent = agent; // Set the clicked agent as the selected agent
                    }
                }
            }
        }
    }

    void HandleMovementInput()
    {
        if (selectedAgent != null && Input.GetMouseButtonDown(1)) // Right-click to move the selected agent
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider != null && hit.collider.CompareTag("Ground"))
                {
                    selectedAgent.MoveToPosition(hit.point); 
                }
            }
        }
    }

    public AgentMovement GetSelectedAgent()
    {
        return selectedAgent;
    }
}
