using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// UI controller for the party menu. Displays main and sub party members
/// and allows swapping characters between parties.
/// </summary>
public partial class PartyMenuUI : Control
{
    [Export] private VBoxContainer mainPartyContainer;
    [Export] private VBoxContainer subPartyContainer;
    [Export] private Label mainPartyLabel;
    [Export] private Label subPartyLabel;
    [Export] private Button closeButton;
    [Export] private PackedScene partyMemberPanelScene;
    
    // Selection tracking
    private string selectedMainPartyCharacter = null;
    private string selectedSubPartyCharacter = null;
    
    private List<PartyMemberPanel> mainPartyPanels = new List<PartyMemberPanel>();
    private List<PartyMemberPanel> subPartyPanels = new List<PartyMemberPanel>();
    
    public override void _Ready()
    {
        // Connect signals
        if (closeButton != null)
        {
            closeButton.Pressed += OnClosePressed;
        }
        
        if (PartyMenuManager.Instance != null)
        {
            PartyMenuManager.Instance.MainPartyChanged += RefreshMainParty;
            PartyMenuManager.Instance.SubPartyChanged += RefreshSubParty;
            PartyMenuManager.Instance.PartyMemberLocked += OnPartyMemberLocked;
        }
        
        // Initial refresh
        RefreshPartyDisplay();
    }
    
    /// <summary>
    /// Refresh the entire party display
    /// </summary>
    public void RefreshPartyDisplay()
    {
        RefreshMainParty();
        RefreshSubParty();
    }
    
    /// <summary>
    /// Refresh main party UI
    /// </summary>
    private void RefreshMainParty()
    {
        // Clear existing panels
        foreach (var panel in mainPartyPanels)
        {
            panel.QueueFree();
        }
        mainPartyPanels.Clear();
        
        if (PartyMenuManager.Instance == null) return;
        
        var mainParty = PartyMenuManager.Instance.GetMainParty();
        
        // Update label
        if (mainPartyLabel != null)
        {
            mainPartyLabel.Text = $"Main Party ({mainParty.Count}/{PartyMenuManager.MAX_MAIN_PARTY})";
        }
        
        // Create panels for each member
        foreach (var member in mainParty)
        {
            var panel = CreateMemberPanel(member, true);
            if (panel != null)
            {
                mainPartyContainer.AddChild(panel);
                mainPartyPanels.Add(panel);
            }
        }
        
        // Add empty slots
        int emptySlots = PartyMenuManager.MAX_MAIN_PARTY - mainParty.Count;
        for (int i = 0; i < emptySlots; i++)
        {
            var emptyPanel = CreateEmptySlot();
            mainPartyContainer.AddChild(emptyPanel);
        }
    }
    
    /// <summary>
    /// Refresh sub party UI
    /// </summary>
    private void RefreshSubParty()
    {
        // Clear existing panels
        foreach (var panel in subPartyPanels)
        {
            panel.QueueFree();
        }
        subPartyPanels.Clear();
        
        if (PartyMenuManager.Instance == null) return;
        
        var subParty = PartyMenuManager.Instance.GetSubParty();
        
        // Update label
        if (subPartyLabel != null)
        {
            subPartyLabel.Text = $"Sub Party ({subParty.Count}) - Gains 50% EXP";
        }
        
        // Create panels for each member
        foreach (var member in subParty)
        {
            var panel = CreateMemberPanel(member, false);
            if (panel != null)
            {
                subPartyContainer.AddChild(panel);
                subPartyPanels.Add(panel);
            }
        }
    }
    
    /// <summary>
    /// Create a member panel
    /// </summary>
    private PartyMemberPanel CreateMemberPanel(PartyMemberData member, bool isMainParty)
    {
        PartyMemberPanel panel;
        
        // Use scene if provided, otherwise create programmatically
        if (partyMemberPanelScene != null)
        {
            panel = partyMemberPanelScene.Instantiate<PartyMemberPanel>();
        }
        else
        {
            panel = new PartyMemberPanel();
        }
        
        panel.Initialize(member, isMainParty);
        panel.MemberSelected += OnMemberSelected;
        panel.SwapRequested += OnSwapRequested;
        
        return panel;
    }
    
    /// <summary>
    /// Create an empty slot panel
    /// </summary>
    private Control CreateEmptySlot()
    {
        var panel = new PanelContainer();
        panel.CustomMinimumSize = new Vector2(0, 80);
        
        var label = new Label();
        label.Text = "[ Empty Slot ]";
        label.HorizontalAlignment = HorizontalAlignment.Center;
        label.VerticalAlignment = VerticalAlignment.Center;
        
        panel.AddChild(label);
        return panel;
    }
    
    /// <summary>
    /// Handle member selection
    /// </summary>
    private void OnMemberSelected(string characterId, bool isMainParty)
    {
        if (isMainParty)
        {
            // Deselect if clicking the same character
            if (selectedMainPartyCharacter == characterId)
            {
                selectedMainPartyCharacter = null;
            }
            else
            {
                selectedMainPartyCharacter = characterId;
            }
            
            // If both parties have selection, swap them
            if (selectedMainPartyCharacter != null && selectedSubPartyCharacter != null)
            {
                TrySwapCharacters();
            }
        }
        else
        {
            if (selectedSubPartyCharacter == characterId)
            {
                selectedSubPartyCharacter = null;
            }
            else
            {
                selectedSubPartyCharacter = characterId;
            }
            
            if (selectedMainPartyCharacter != null && selectedSubPartyCharacter != null)
            {
                TrySwapCharacters();
            }
        }
        
        UpdateSelectionVisuals();
    }
    
    /// <summary>
    /// Handle direct swap button press
    /// </summary>
    private void OnSwapRequested(string characterId, bool isMainParty)
    {
        if (PartyMenuManager.Instance == null) return;
        
        if (isMainParty)
        {
            PartyMenuManager.Instance.SwapToSubParty(characterId);
        }
        else
        {
            PartyMenuManager.Instance.SwapToMainParty(characterId);
        }
        
        ClearSelections();
    }
    
    /// <summary>
    /// Attempt to swap two selected characters
    /// </summary>
    private void TrySwapCharacters()
    {
        if (PartyMenuManager.Instance == null) return;
        
        if (selectedMainPartyCharacter != null && selectedSubPartyCharacter != null)
        {
            bool success = PartyMenuManager.Instance.SwapCharacters(
                selectedMainPartyCharacter, 
                selectedSubPartyCharacter
            );
            
            if (success)
            {
                ClearSelections();
            }
        }
    }
    
    /// <summary>
    /// Clear all selections
    /// </summary>
    private void ClearSelections()
    {
        selectedMainPartyCharacter = null;
        selectedSubPartyCharacter = null;
        UpdateSelectionVisuals();
    }
    
    /// <summary>
    /// Update visual feedback for selections
    /// </summary>
    private void UpdateSelectionVisuals()
    {
        // Update main party panels
        foreach (var panel in mainPartyPanels)
        {
            bool isSelected = panel.CharacterId == selectedMainPartyCharacter;
            panel.SetSelected(isSelected);
        }
        
        // Update sub party panels
        foreach (var panel in subPartyPanels)
        {
            bool isSelected = panel.CharacterId == selectedSubPartyCharacter;
            panel.SetSelected(isSelected);
        }
    }
    
    /// <summary>
    /// Handle party member locked/unlocked
    /// </summary>
    private void OnPartyMemberLocked(string characterId, bool isLocked)
    {
        var panel = mainPartyPanels.FirstOrDefault(p => p.CharacterId == characterId);
        panel?.SetLocked(isLocked);
    }
    
    /// <summary>
    /// Close the party menu
    /// </summary>
    private void OnClosePressed()
    {
        Hide();
    }
    
    /// <summary>
    /// Show the party menu
    /// </summary>
    public void OpenMenu()
    {
        Show();
        RefreshPartyDisplay();
    }
    
    public override void _ExitTree()
    {
        if (PartyMenuManager.Instance != null)
        {
            PartyMenuManager.Instance.MainPartyChanged -= RefreshMainParty;
            PartyMenuManager.Instance.SubPartyChanged -= RefreshSubParty;
            PartyMenuManager.Instance.PartyMemberLocked -= OnPartyMemberLocked;
        }
        
        base._ExitTree();
    }
}

/// <summary>
/// Individual party member panel
/// </summary>
public partial class PartyMemberPanel : PanelContainer
{
    [Signal]
    public delegate void MemberSelectedEventHandler(string characterId, bool isMainParty);
    
    [Signal]
    public delegate void SwapRequestedEventHandler(string characterId, bool isMainParty);
    
    public string CharacterId { get; private set; }
    private bool isMainParty;
    private bool isLocked;
    
    private Label nameLabel;
    private Label levelLabel;
    private Label hpLabel;
    private TextureRect portrait;
    private Button selectButton;
    private Button swapButton;
    private TextureRect lockIcon;
    
    public void Initialize(PartyMemberData member, bool inMainParty)
    {
        CharacterId = member.CharacterId;
        isMainParty = inMainParty;
        isLocked = member.IsLocked;
        
        CustomMinimumSize = new Vector2(0, 100);
        
        // Create layout
        var hbox = new HBoxContainer();
        AddChild(hbox);
        
        // Portrait
        portrait = new TextureRect();
        portrait.CustomMinimumSize = new Vector2(80, 80);
        portrait.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
        portrait.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
        hbox.AddChild(portrait);
        
        // Info section
        var vbox = new VBoxContainer();
        vbox.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        hbox.AddChild(vbox);
        
        // Name with lock icon
        var nameBox = new HBoxContainer();
        vbox.AddChild(nameBox);
        
        nameLabel = new Label();
        nameLabel.Text = member.Stats.CharacterName;
        nameLabel.AddThemeColorOverride("font_color", Colors.White);
        nameBox.AddChild(nameLabel);
        
        lockIcon = new TextureRect();
        lockIcon.CustomMinimumSize = new Vector2(20, 20);
        lockIcon.Visible = isLocked;
        nameBox.AddChild(lockIcon);
        
        levelLabel = new Label();
        levelLabel.Text = $"Lv. {member.Stats.Level}";
        vbox.AddChild(levelLabel);
        
        hpLabel = new Label();
        hpLabel.Text = $"HP: {member.Stats.CurrentHP}/{member.Stats.MaxHP}";
        vbox.AddChild(hpLabel);
        
        // Buttons section
        var buttonBox = new VBoxContainer();
        hbox.AddChild(buttonBox);
        
        selectButton = new Button();
        selectButton.Text = "Select";
        selectButton.Pressed += OnSelectPressed;
        buttonBox.AddChild(selectButton);
        
        swapButton = new Button();
        swapButton.Text = isMainParty ? "→ Sub" : "→ Main";
        swapButton.Disabled = isLocked;
        swapButton.Pressed += OnSwapPressed;
        buttonBox.AddChild(swapButton);
        
        // Try to load portrait from CharacterData
        TryLoadPortrait(member);
    }
    
    private void TryLoadPortrait(PartyMemberData member)
    {
        // Try to load menu portrait
        string portraitPath = $"res://Characters/Portraits/{member.CharacterId}_Menu.png";
        if (ResourceLoader.Exists(portraitPath))
        {
            portrait.Texture = GD.Load<Texture2D>(portraitPath);
        }
    }
    
    private void OnSelectPressed()
    {
        if (!isLocked)
        {
            EmitSignal(SignalName.MemberSelected, CharacterId, isMainParty);
        }
    }
    
    private void OnSwapPressed()
    {
        if (!isLocked)
        {
            EmitSignal(SignalName.SwapRequested, CharacterId, isMainParty);
        }
    }
    
    public void SetSelected(bool selected)
    {
        if (selected)
        {
            AddThemeStyleboxOverride("panel", CreateSelectedStylebox());
        }
        else
        {
            RemoveThemeStyleboxOverride("panel");
        }
    }
    
    public void SetLocked(bool locked)
    {
        isLocked = locked;
        if (lockIcon != null)
        {
            lockIcon.Visible = locked;
        }
        if (swapButton != null)
        {
            swapButton.Disabled = locked;
        }
        if (selectButton != null)
        {
            selectButton.Disabled = locked;
        }
    }
    
    private StyleBoxFlat CreateSelectedStylebox()
    {
        var stylebox = new StyleBoxFlat();
        stylebox.BgColor = new Color(0.3f, 0.6f, 1.0f, 0.3f);
        stylebox.BorderColor = new Color(0.3f, 0.6f, 1.0f);
        stylebox.BorderWidthLeft = 2;
        stylebox.BorderWidthRight = 2;
        stylebox.BorderWidthTop = 2;
        stylebox.BorderWidthBottom = 2;
        return stylebox;
    }
}