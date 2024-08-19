using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameStateController : MonoBehaviour
{
    // Public variables to link to the UI components
    public Slider agentSlider;
    public Slider obstacleDensitySlider;
    public TextMeshProUGUI agentText;
    public TextMeshProUGUI obstacleDensityText;

    public Button BuildButton;
    public Button ResetButton;
    public GameObject worldBuilder;
    WorldBuilderController worldBuilderController;

    // Variables to store the values
    public int NumberOfAgents { get; private set; }
    public float ObstacleDensity { get; private set; }

    void Awake() {
        // Get the WorldBuilderController component from the worldBuilder GameObject
        worldBuilderController = worldBuilder.GetComponent<WorldBuilderController>();
    }
    void Start()
    {
        // Initialize the values based on the slider positions
        UpdateNumberOfAgents();
        UpdateObstacleDensity();

        // Add listeners to the sliders to update values when they change
        agentSlider.onValueChanged.AddListener(delegate { UpdateNumberOfAgents(); });
        obstacleDensitySlider.onValueChanged.AddListener(delegate { UpdateObstacleDensity(); });

        // Add listeners to the buttons for their click events
        BuildButton.onClick.AddListener(HandleBuildButtonPressed);
        ResetButton.onClick.AddListener(HandleResetButtonPressed);
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

    void HandleBuildButtonPressed()
    {
        worldBuilderController.Build(NumberOfAgents, ObstacleDensity);
    }

    void HandleResetButtonPressed()
    {
        worldBuilderController.Reset();
    }
}
