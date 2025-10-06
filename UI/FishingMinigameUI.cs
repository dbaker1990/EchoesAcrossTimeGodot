using Godot;
using System;

/// <summary>
/// UI controller for the fishing minigame
/// Displays tension meter, prompts, and fish info
/// </summary>
public partial class FishingMinigameUI : Control
{
    #region Exports
    [ExportGroup("Main Panel")]
    [Export] private Control mainPanel;
    [Export] private Label instructionLabel;
    
    [ExportGroup("Fish Info")]
    [Export] private Control fishInfoPanel;
    [Export] private TextureRect fishIcon;
    [Export] private Label fishNameLabel;
    [Export] private Label rarityLabel;
    
    [ExportGroup("Tension Meter")]
    [Export] private ProgressBar tensionBar;
    [Export] private Label tensionLabel;
    [Export] private ColorRect tensionDangerOverlay;
    
    [ExportGroup("Success Counter")]
    [Export] private Label successLabel;
    [Export] private Control successIconsContainer;
    
    [ExportGroup("Button Prompt")]
    [Export] private Control buttonPromptPanel;
    [Export] private Label buttonPromptLabel;
    [Export] private ProgressBar promptTimerBar;
    
    [ExportGroup("Result Panel")]
    [Export] private Control resultPanel;
    [Export] private Label resultLabel;
    [Export] private TextureRect resultIcon;
    #endregion
    
    #region State
    private FishingState fishingState;
    private bool isActive = false;
    private float promptTimer = 0f;
    private float promptDuration = 0f;
    #endregion
    
    public override void _Ready()
    {
        Hide();
        HideAllPanels();
    }
    
    #region Public Methods
    /// <summary>
    /// Start the fishing minigame UI
    /// </summary>
    public void StartFishing(FishingState state)
    {
        fishingState = state;
        isActive = true;
        
        // Connect signals
        ConnectSignals();
        
        Show();
        ShowWaitingState();
    }
    
    /// <summary>
    /// Stop the fishing minigame UI
    /// </summary>
    public void StopFishing()
    {
        isActive = false;
        DisconnectSignals();
        Hide();
        HideAllPanels();
    }
    #endregion
    
    #region Signal Connection
    private void ConnectSignals()
    {
        if (fishingState == null) return;
        
        fishingState.FishBite += OnFishBite;
        fishingState.FishHooked += OnFishHooked;
        fishingState.TensionChanged += OnTensionChanged;
        fishingState.SuccessCountChanged += OnSuccessCountChanged;
        fishingState.FishCaught += OnFishCaught;
        fishingState.FishEscaped += OnFishEscaped;
        fishingState.LineBroke += OnLineBroke;
    }
    
    private void DisconnectSignals()
    {
        if (fishingState == null) return;
        
        fishingState.FishBite -= OnFishBite;
        fishingState.FishHooked -= OnFishHooked;
        fishingState.TensionChanged -= OnTensionChanged;
        fishingState.SuccessCountChanged -= OnSuccessCountChanged;
        fishingState.FishCaught -= OnFishCaught;
        fishingState.FishEscaped -= OnFishEscaped;
        fishingState.LineBroke -= OnLineBroke;
    }
    #endregion
    
    #region Display States
    private void HideAllPanels()
    {
        mainPanel?.Hide();
        fishInfoPanel?.Hide();
        buttonPromptPanel?.Hide();
        resultPanel?.Hide();
    }
    
    private void ShowWaitingState()
    {
        HideAllPanels();
        mainPanel?.Show();
        
        if (instructionLabel != null)
        {
            instructionLabel.Text = "Waiting for a bite...";
        }
    }
    
    private void ShowBitePrompt()
    {
        buttonPromptPanel?.Show();
        
        if (buttonPromptLabel != null)
        {
            buttonPromptLabel.Text = "PRESS [SPACE]!";
        }
        
        promptTimer = 0f;
        promptDuration = 1.5f; // Match hook window
    }
    
    private void ShowReelingState(FishData fish)
    {
        HideAllPanels();
        mainPanel?.Show();
        fishInfoPanel?.Show();
        
        if (instructionLabel != null)
        {
            instructionLabel.Text = "Press [SPACE] to reel! Don't break the line!";
        }
        
        // Display fish info
        if (fish != null)
        {
            if (fishIcon != null) fishIcon.Texture = fish.FishIcon;
            if (fishNameLabel != null) fishNameLabel.Text = fish.FishName;
            if (rarityLabel != null)
            {
                rarityLabel.Text = fish.Rarity.ToString();
                rarityLabel.Modulate = fish.GetRarityColor();
            }
        }
        else
        {
            // Unknown fish
            if (fishNameLabel != null) fishNameLabel.Text = "???";
            if (rarityLabel != null) rarityLabel.Text = "Unknown";
        }
    }
    
    private void ShowResult(string message, bool success)
    {
        HideAllPanels();
        resultPanel?.Show();
        
        if (resultLabel != null)
        {
            resultLabel.Text = message;
            resultLabel.Modulate = success ? Colors.LimeGreen : Colors.Red;
        }
        
        // Auto-hide after 2 seconds
        GetTree().CreateTimer(2.0).Timeout += StopFishing;
    }
    #endregion
    
    #region Signal Handlers
    private void OnFishBite()
    {
        ShowBitePrompt();
        
        // Flash the screen or play animation
        if (buttonPromptPanel != null)
        {
            var tween = CreateTween();
            tween.TweenProperty(buttonPromptPanel, "modulate:a", 0.0f, 0.2f);
            tween.TweenProperty(buttonPromptPanel, "modulate:a", 1.0f, 0.2f);
            tween.SetLoops(3);
        }
    }
    
    private void OnFishHooked(FishData fish)
    {
        buttonPromptPanel?.Hide();
        ShowReelingState(fish);
    }
    
    private void OnTensionChanged(float tension)
    {
        if (tensionBar != null)
        {
            tensionBar.Value = tension;
        }
        
        if (tensionLabel != null)
        {
            tensionLabel.Text = $"Tension: {tension:F0}%";
        }
        
        // Visual warning when tension is high
        if (tensionDangerOverlay != null)
        {
            if (tension >= 75f)
            {
                tensionDangerOverlay.Visible = true;
                tensionDangerOverlay.Modulate = new Color(1f, 0f, 0f, (tension - 75f) / 25f * 0.5f);
            }
            else
            {
                tensionDangerOverlay.Visible = false;
            }
        }
    }
    
    private void OnSuccessCountChanged(int count, int required)
    {
        if (successLabel != null)
        {
            successLabel.Text = $"Progress: {count}/{required}";
        }
        
        // Update success icons
        UpdateSuccessIcons(count, required);
    }
    
    private void UpdateSuccessIcons(int count, int required)
    {
        if (successIconsContainer == null) return;
        
        // Clear existing icons
        foreach (var child in successIconsContainer.GetChildren())
        {
            child.QueueFree();
        }
        
        // Create new icons
        for (int i = 0; i < required; i++)
        {
            var icon = new ColorRect();
            icon.CustomMinimumSize = new Vector2(30, 30);
            icon.Color = i < count ? Colors.LimeGreen : Colors.DarkGray;
            successIconsContainer.AddChild(icon);
        }
    }
    
    private void OnFishCaught(FishData fish)
    {
        string message = fish != null 
            ? $"Caught {fish.FishName}!" 
            : "Caught a fish!";
        ShowResult(message, true);
    }
    
    private void OnFishEscaped()
    {
        ShowResult("The fish got away...", false);
    }
    
    private void OnLineBroke()
    {
        ShowResult("The line broke!", false);
    }
    #endregion
    
    #region Update
    public override void _Process(double delta)
    {
        if (!isActive) return;
        
        // Update button prompt timer
        if (buttonPromptPanel != null && buttonPromptPanel.Visible)
        {
            promptTimer += (float)delta;
            
            if (promptTimerBar != null)
            {
                promptTimerBar.Value = (promptTimer / promptDuration) * 100f;
            }
        }
    }
    #endregion
}