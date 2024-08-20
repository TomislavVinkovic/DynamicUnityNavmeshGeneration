using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/************************************************************************************
 * This class is used to control the game state
 * It is used to handle the UI components and the game state
 ************************************************************************************/
public class GameStateController : MonoBehaviour
{
    // Public variables to link to the UI components
    public Slider agentSlider;
    public Slider obstacleDensitySlider;
    public Slider agentNavmeshSizeSlider;

    public TextMeshProUGUI agentText;
    public TextMeshProUGUI obstacleDensityText;
    public TextMeshProUGUI agentNavmeshSizeText;

    public Button BuildButton;
    public Button ResetButton;
    public Button RandomMovementButton;
    public GameObject worldBuilder;
    WorldBuilderController worldBuilderController;

    // Variables to store the values
    public int NumberOfAgents { get; private set; }
    public float ObstacleDensity { get; private set; }
    public float AgentNavmeshSize { get; private set; }

    public bool RandomMovementEnabled { get; private set; }

    void Awake() {
        // Get the WorldBuilderController component from the worldBuilder GameObject
        worldBuilderController = worldBuilder.GetComponent<WorldBuilderController>();
    }
    void Start()
    {
        // Initialize the values based on the slider positions
        UpdateNumberOfAgents();
        UpdateObstacleDensity();
        UpdateAgentNavmeshSize();

        // Add listeners to the sliders to update values when they change
        agentSlider.onValueChanged.AddListener(delegate { UpdateNumberOfAgents(); });
        obstacleDensitySlider.onValueChanged.AddListener(delegate { UpdateObstacleDensity(); });
        agentNavmeshSizeSlider.onValueChanged.AddListener(delegate { UpdateAgentNavmeshSize(); });

        // Add listeners to the buttons for their click events
        BuildButton.onClick.AddListener(HandleBuildButtonPressed);
        ResetButton.onClick.AddListener(HandleResetButtonPressed);
        RandomMovementButton.onClick.AddListener(HandleRandomMovementButtonPressed);
    }

    void Update() {
        UpdateSlidersInteractable();
        UpdateBuildButtonInteractable();
        UpdateRandomMovementButtonInteractable();
        ExitGameIfEscapePressed();        
    }

    void UpdateNumberOfAgents()
    {
        NumberOfAgents = (int)agentSlider.value; // Convert slider value to int
        agentText.text = "Number of Agents: " + NumberOfAgents;
    }

    void UpdateObstacleDensity()
    {
        ObstacleDensity = obstacleDensitySlider.value; // Keep it as a float
        obstacleDensityText.text = "Obstacle Density: " +  (Math.Round(ObstacleDensity, 2) * 100).ToString();
    }

    void UpdateAgentNavmeshSize() {
        AgentNavmeshSize = agentNavmeshSizeSlider.value;
        agentNavmeshSizeText.text = "Agent navmesh Size: " + AgentNavmeshSize;
    }

    void HandleBuildButtonPressed()
    {
        worldBuilderController.Build(NumberOfAgents, ObstacleDensity);
    }

    void HandleResetButtonPressed()
    {
        worldBuilderController.Reset();
    }

    void HandleRandomMovementButtonPressed() {
        if(RandomMovementEnabled) {
            RandomMovementEnabled = false;
            RandomMovementButton.GetComponentInChildren<TextMeshProUGUI>().text = "Start Random Movement";
        } else {
            RandomMovementEnabled = true;
            RandomMovementButton.GetComponentInChildren<TextMeshProUGUI>().text = "End Random Movement";
        }
    }

    void UpdateSlidersInteractable() {
        agentSlider.interactable = worldBuilderController.State.Equals(WorldBuilderState.Standby);
        obstacleDensitySlider.interactable = worldBuilderController.State.Equals(WorldBuilderState.Standby);
        agentNavmeshSizeSlider.interactable = worldBuilderController.State.Equals(WorldBuilderState.Standby);
    }

    void UpdateBuildButtonInteractable() {
        BuildButton.interactable = worldBuilderController.State.Equals(WorldBuilderState.Standby);
    }

    void UpdateRandomMovementButtonInteractable() {
        RandomMovementButton.interactable = worldBuilderController.State.Equals(WorldBuilderState.Ready);
    }

    void ExitGameIfEscapePressed() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            Application.Quit();
        }
    }
}
