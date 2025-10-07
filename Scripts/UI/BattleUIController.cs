using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using EchoesAcrossTime;
using EchoesAcrossTime.Combat;
using EchoesAcrossTime.Items;
using EchoesAcrossTime.Managers;
using RPG.Items;

/// <summary>
/// Main controller for the Battle UI - manages all UI interactions and updates
/// </summary>
public partial class BattleUIController : Node
{
    #region Node References
    
    private BattleManager battleManager;
    
    // Character Panels
    private VBoxContainer partyPanel;
    private List<PanelContainer> characterPanels = new();
    
    // Enemy Panel
    private HBoxContainer enemyPanel;
    private List<Control> enemyDisplays = new();
    
    // Action Menu
    private PanelContainer actionMenu;
    private Button attackButton;
    private Button skillsButton;
    private Button itemsButton;
    private Button guardButton;
    private Button escapeButton;
    
    // Skill Menu
    private PanelContainer skillMenu;
    private VBoxContainer skillList;
    private TabBar skillTabs;
    private VBoxContainer skillInfo;
    
    // Target Selector
    private Control targetSelector;
    private Sprite2D targetCursor;
    private int currentTargetIndex = 0;
    
    // Notifications
    private Control notifications;
    private Label oneMoreLabel;
    private Label technicalLabel;
    private Label weaknessLabel;
    private PanelContainer batonPassPanel;
    
    // All-Out Attack Prompt
    private CenterContainer allOutAttackPrompt;
    private Button aoeYesButton;
    private Button aoeNoButton;
    
    // Damage Numbers
    private Control damageNumbersContainer;
    
    private PanelContainer itemMenu;
    private ItemList itemList;
    private Label itemNameLabel;
    private Label itemDescriptionLabel;
    private Label itemQuantityLabel;
    private Button useItemButton;
    private Button backFromItemsButton;
    
    #endregion
    
    #region State Variables
    
    private BattleMember currentActor;
    private List<BattleMember> currentTargets = new();
    private SkillData selectedSkill;
    private bool isSelectingTarget = false;
    private ElementType currentSkillTabFilter = ElementType.Physical;
    
    private ConsumableData selectedItem;
    private bool waitingForItemTarget = false;
    
    [Export] private PackedScene victoryScreenScene;
    [Export] private PackedScene defeatScreenScene;
    
    private Control victoryScreen;
    private Control defeatScreen;
    
    private AnimationPlayer showtimeAnimationPlayer;
    private Control showtimeOverlay;
    
    private BattlefieldVisuals battlefieldVisuals;
    #endregion
    
    #region Initialization
    
    public override void _Ready()
    {
        // Get BattleManager using unique name (requires BattleManager to have unique name enabled)
        battleManager = GetNode<BattleManager>("%BattleManager");
        
        // Cache UI references
        CacheUIReferences();
        
        // Connect signals
        ConnectBattleSignals();
        
        // Setup button connections
        SetupButtons();
        
        // Hide menus initially
        HideAllMenus();
        
        battlefieldVisuals = GetNode<BattlefieldVisuals>("../BattleField");
    }
    
    private void CacheUIReferences()
    {
        // Party panels
        partyPanel = GetNode<VBoxContainer>("../UI/PartyPanel");
        characterPanels.Add(GetNode<PanelContainer>("../UI/PartyPanel/Character1Panel"));
        characterPanels.Add(GetNode<PanelContainer>("../UI/PartyPanel/Character2Panel"));
        characterPanels.Add(GetNode<PanelContainer>("../UI/PartyPanel/Character3Panel"));
        characterPanels.Add(GetNode<PanelContainer>("../UI/PartyPanel/Character4Panel"));
        
        // Enemy panel
        enemyPanel = GetNode<HBoxContainer>("../UI/EnemyPanel");
        
        // Action menu
        actionMenu = GetNode<PanelContainer>("../UI/ActionMenu");
        var menuButtons = actionMenu.GetNode<VBoxContainer>("MenuButtons");
        attackButton = menuButtons.GetNode<Button>("AttackButton");
        skillsButton = menuButtons.GetNode<Button>("SkillsButton");
        itemsButton = menuButtons.GetNode<Button>("ItemsButton");
        guardButton = menuButtons.GetNode<Button>("GuardButton");
        escapeButton = menuButtons.GetNode<Button>("Escape");
        
        // Skill menu
        skillMenu = GetNode<PanelContainer>("../UI/SkillMenu");
        var skillContainer = skillMenu.GetNode<HBoxContainer>("MarginContainer/HBoxContainer");
        var leftPanel = skillContainer.GetNode<VBoxContainer>("LeftPanel");
        skillTabs = leftPanel.GetNode<TabBar>("SkillTabs");
        skillList = leftPanel.GetNode<ScrollContainer>("ScrollContainer").GetNode<VBoxContainer>("SkillList");
        skillInfo = skillContainer.GetNode<VBoxContainer>("SkillInfo");
        
        // Target selector
        targetSelector = GetNode<Control>("../UI/TargetSelector");
        targetCursor = targetSelector.GetNode<Sprite2D>("Cursor");
        
        // Notifications
        notifications = GetNode<Control>("../UI/Notifications");
        oneMoreLabel = notifications.GetNode<Label>("OneMoreLabel");
        technicalLabel = notifications.GetNode<Label>("TechnicalLabel");
        weaknessLabel = notifications.GetNode<Label>("WeaknessLabel");
        batonPassPanel = notifications.GetNode<PanelContainer>("BatonPassPanel");
        
        // All-Out Attack
        allOutAttackPrompt = GetNode<CenterContainer>("../UI/AllOutAttackPrompt");
        var promptPanel = allOutAttackPrompt.GetNode<PanelContainer>("PromptPanel");
        var buttonContainer = promptPanel.GetNode<VBoxContainer>("VBoxContainer").GetNode<HBoxContainer>("HBoxContainer");
        aoeYesButton = buttonContainer.GetNode<Button>("YesButton");
        aoeNoButton = buttonContainer.GetNode<Button>("NoButton");
        
        // Damage numbers
        damageNumbersContainer = GetNode<Control>("../UI/DamageNumbers");
    }
    
    
    private void ConnectBattleSignals()
    {
        battleManager.BattleStarted += OnBattleStarted;
        battleManager.TurnStarted += OnTurnStarted;
        battleManager.ActionExecuted += OnActionExecuted;
        battleManager.WeaknessHit += OnWeaknessHit;
        battleManager.OneMoreTriggered += OnOneMore;
        battleManager.AllOutAttackReady += OnAllOutAttackReady;
        battleManager.BattleEnded += OnBattleEnded;
        battleManager.Knockdown += OnKnockdown;
        battleManager.BatonPassExecuted += OnBatonPass;
        battleManager.TechnicalDamage += OnTechnical;
        battleManager.ShowtimeTriggered += OnShowtime;
    }
    
    private void SetupButtons()
    {
        // Existing button connections...
        attackButton.Pressed += OnAttackPressed;
        skillsButton.Pressed += OnSkillsPressed;
        itemsButton.Pressed += OnItemsPressed;
        guardButton.Pressed += OnGuardPressed;
        escapeButton.Pressed += OnEscapePressed;
    
        // All-Out Attack buttons
        aoeYesButton.Pressed += OnAllOutAttackYes;
        aoeNoButton.Pressed += OnAllOutAttackNo;
    
        // Skill tabs
        skillTabs.TabChanged += OnSkillTabChanged;
    
        // NEW: Item menu buttons
        itemList.ItemSelected += OnItemListSelected;
        useItemButton.Pressed += OnUseItemPressed;
        backFromItemsButton.Pressed += OnBackFromItemsPressed;
    }
    
    #endregion
    
    #region Battle Event Handlers
    
    private void OnBattleStarted()
    {
        GD.Print("Battle UI: Battle started!");
        UpdatePartyDisplay();
        UpdateEnemyDisplay();
    }
    
    private void OnTurnStarted(string characterName)
    {
        GD.Print($"Battle UI: {characterName}'s turn");
        currentActor = battleManager.CurrentActor;
        
        if (battleManager.IsPlayerTurn())
        {
            ShowActionMenu();
        }
        else
        {
            HideAllMenus();
        }
        
        UpdatePartyDisplay();
        UpdateEnemyDisplay();
    }
    
    private void OnActionExecuted(string actorName, string actionName, int damageDealt, bool hitWeakness, bool wasCritical)
    {
        // Show damage number
        if (damageDealt > 0)
        {
            // FIXED: Call with correct parameters (3 params: damage, isCritical, isWeakness)
            ShowDamageNumber(damageDealt, wasCritical, hitWeakness);
        }
    
        UpdatePartyDisplay();
        UpdateEnemyDisplay();
    }
    
    private void OnWeaknessHit(string attackerName, string targetName)
    {
        ShowNotification(weaknessLabel, "WEAKNESS!", Colors.Red, 1.0f);
        
        // Play screen flash effect
        FlashScreen(new Color(1, 0, 0, 0.3f), 0.2f);
    }
    
    private void OnOneMore(string characterName)
    {
        ShowNotification(oneMoreLabel, "ONE MORE!", Colors.Yellow, 1.5f);
        
        // Play sound effect - would need to load the audio stream
        // AudioManager.Instance?.PlaySE(oneMoreSound, 0f);
    }
    
    private void OnKnockdown(string characterName)
    {
        GD.Print($"Battle UI: {characterName} knocked down!");
        UpdateEnemyDisplay();
    }
    
    private void OnBatonPass(string fromCharacter, string toCharacter, int passLevel)
    {
        ShowBatonPassNotification(fromCharacter, toCharacter, passLevel);
        UpdatePartyDisplay();
    }
    
    private void OnTechnical(string attackerName, string targetName, string comboType)
    {
        ShowNotification(technicalLabel, $"TECHNICAL!\n{comboType}", new Color(0.8f, 0, 1), 1.5f);
        
        // Purple screen flash
        FlashScreen(new Color(0.5f, 0, 0.5f, 0.3f), 0.3f);
    }
    
    private void OnShowtime(string showtimeName, string char1, string char2)
    {
        ShowNotification(technicalLabel, $"SHOWTIME: {showtimeName}", Colors.Gold, 2.0f);
        GD.Print($"[Showtime] {char1} + {char2}: {showtimeName}");
    
        PlayShowtimeCutscene(showtimeName, char1, char2);
    }
    
    private void OnAllOutAttackReady()
    {
        allOutAttackPrompt.Show();
    }
    
    private void OnBattleEnded(bool victory)
    {
        HideAllMenus();
    
        if (victory)
        {
            ShowVictoryScreen();
        }
        else
        {
            ShowDefeatScreen();
        }
    }
    
    #endregion
    
    #region Action Menu
    
    /// <summary>
    /// UPDATED: ShowActionMenu() - now checks for items properly
    /// </summary>
    private void ShowActionMenu()
    {
        actionMenu.Show();
    
        bool canBatonPass = battleManager.CanBatonPass();
    
        attackButton.Disabled = false;
        skillsButton.Disabled = currentActor?.Stats?.Skills?.GetEquippedSkills()?.Count == 0;
        itemsButton.Disabled = !HasUsableItems();
        guardButton.Disabled = false;
        escapeButton.Disabled = false;
    }
    
    private void HideAllMenus()
    {
        actionMenu.Hide();
        skillMenu.Hide();
        targetSelector.Hide();
        allOutAttackPrompt.Hide();
        itemMenu.Hide(); 
    }
    
    private void OnAttackPressed()
    {
        // Basic attack - show target selection
        selectedSkill = null;
        ShowTargetSelection(battleManager.GetLivingEnemies().ToList());
    }
    
    private void OnSkillsPressed()
    {
        ShowSkillMenu();
    }
    
    /// <summary>
    /// UPDATED: OnItemsPressed() - now shows item menu
    /// </summary>
    private void OnItemsPressed()
    {
        ShowItemMenu();
    }
    
    private void OnGuardPressed()
    {
        var action = new BattleAction(currentActor, BattleActionType.Guard);
        battleManager.ExecuteAction(action);
        HideAllMenus();
    }
    
    private void OnEscapePressed()
    {
        var action = new BattleAction(currentActor, BattleActionType.Escape);
        battleManager.ExecuteAction(action);
        HideAllMenus();
    }
    
    #endregion
    
    #region Skill Menu
    
    private void ShowSkillMenu()
    {
        actionMenu.Hide();
        skillMenu.Show();
        
        // Setup skill tabs
        skillTabs.ClearTabs();
        skillTabs.AddTab("All");
        skillTabs.AddTab("Physical");
        skillTabs.AddTab("Magic");
        skillTabs.AddTab("Support");
        
        // Populate skill list
        PopulateSkillList();
    }
    
    private void PopulateSkillList()
    {
        // Clear existing skills
        foreach (var child in skillList.GetChildren())
        {
            child.QueueFree();
        }
        
        var skills = currentActor?.Stats?.Skills?.GetEquippedSkills();
        if (skills == null) return;
        
        // Filter based on current tab
        var filteredSkills = FilterSkills(skills);
        
        foreach (var skill in filteredSkills)
        {
            var skillButton = CreateSkillButton(skill);
            skillList.AddChild(skillButton);
        }
    }
    
    private List<SkillData> FilterSkills(List<SkillData> skills)
    {
        int tabIndex = skillTabs.CurrentTab;
        
        return tabIndex switch
        {
            0 => skills, // All
            1 => skills.Where(s => s.DamageType == DamageType.Physical).ToList(),
            2 => skills.Where(s => s.DamageType == DamageType.Magical).ToList(),
            3 => skills.Where(s => s.Target == SkillTarget.SingleAlly || s.Target == SkillTarget.AllAllies).ToList(),
            _ => skills
        };
    }
    
    private Button CreateSkillButton(SkillData skill)
    {
        var button = new Button();
        button.CustomMinimumSize = new Vector2(0, 40);
        button.Text = $"{skill.DisplayName} (MP: {skill.MPCost})";
        
        // Disable if not enough MP
        if (currentActor.Stats.CurrentMP < skill.MPCost)
        {
            button.Disabled = true;
            button.Text += " (Not enough MP)";
        }
        
        button.Pressed += () => OnSkillSelected(skill);
        
        return button;
    }
    
    private void OnSkillTabChanged(long tab)
    {
        PopulateSkillList();
    }
    
    private void OnSkillSelected(SkillData skill)
    {
        selectedSkill = skill;
        skillMenu.Hide();
        
        // Determine valid targets
        List<BattleMember> validTargets;
        
        if (skill.Target == SkillTarget.SingleEnemy || skill.Target == SkillTarget.AllEnemies)
        {
            validTargets = battleManager.GetLivingEnemies().ToList();
        }
        else
        {
            validTargets = battleManager.GetLivingAllies().ToList();
        }
        
        ShowTargetSelection(validTargets);
    }
    
    #endregion
    
    #region Target Selection
    
    private void ShowTargetSelection(List<BattleMember> targets)
    {
        currentTargets = targets;
        currentTargetIndex = 0;
        isSelectingTarget = true;
        
        targetSelector.Show();
        UpdateTargetCursor();
    }
    
    private void UpdateTargetCursor()
    {
        if (currentTargets.Count == 0) return;
        
        var target = currentTargets[currentTargetIndex];
        
        // Position cursor above target (you'll need to get actual positions from battle sprites)
        // This is a placeholder - adjust based on your battle layout
        targetCursor.Position = new Vector2(960, 400 + (currentTargetIndex * 100));
    }
    
    public override void _Input(InputEvent @event)
    {
        if (!isSelectingTarget) return;
        
        if (@event.IsActionPressed("ui_up"))
        {
            currentTargetIndex = Mathf.Max(0, currentTargetIndex - 1);
            UpdateTargetCursor();
        }
        else if (@event.IsActionPressed("ui_down"))
        {
            currentTargetIndex = Mathf.Min(currentTargets.Count - 1, currentTargetIndex + 1);
            UpdateTargetCursor();
        }
        else if (@event.IsActionPressed("ui_accept"))
        {
            ConfirmTarget();
        }
        else if (@event.IsActionPressed("ui_cancel"))
        {
            CancelTargetSelection();
        }
    }
    
    private void ConfirmTarget()
    {
        isSelectingTarget = false;
        targetSelector.Hide();
    
        var target = currentTargets[currentTargetIndex];
    
        BattleAction action;
    
        // Handle item usage
        if (waitingForItemTarget && selectedItem != null)
        {
            ExecuteItemAction(new[] { target });
            return;
        }
    
        // Handle skill usage
        if (selectedSkill != null)
        {
            action = new BattleAction(currentActor, BattleActionType.Skill)
                .WithSkill(selectedSkill)
                .WithTargets(target);
        }
        else
        {
            // Basic attack
            action = new BattleAction(currentActor, BattleActionType.Attack)
                .WithTargets(target);
        }
    
        battleManager.ExecuteAction(action);
        HideAllMenus();
    }
    
    private void CancelTargetSelection()
    {
        isSelectingTarget = false;
        targetSelector.Hide();
        
        if (selectedSkill != null)
        {
            ShowSkillMenu();
        }
        else
        {
            ShowActionMenu();
        }
    }
    
    #endregion
    
    #region Display Updates
    
    private void UpdatePartyDisplay()
    {
        var party = battleManager.GetPlayerParty();
        
        for (int i = 0; i < characterPanels.Count; i++)
        {
            var panel = characterPanels[i];
            
            if (i < party.Count)
            {
                UpdateCharacterPanel(panel, party[i]);
                panel.Show();
            }
            else
            {
                panel.Hide();
            }
        }
    }
    
    private void UpdateCharacterPanel(PanelContainer panel, BattleMember member)
    {
        // Clear existing children
        foreach (var child in panel.GetChildren())
        {
            child.QueueFree();
        }
        
        var vbox = new VBoxContainer();
        panel.AddChild(vbox);
        
        // Name
        var nameLabel = new Label();
        nameLabel.Text = member.Stats.CharacterName;
        nameLabel.AddThemeFontSizeOverride("font_size", 18);
        vbox.AddChild(nameLabel);
        
        // HP Bar
        var hpBar = new ProgressBar();
        hpBar.MaxValue = member.Stats.MaxHP;
        hpBar.Value = member.Stats.CurrentHP;
        hpBar.CustomMinimumSize = new Vector2(0, 20);
        vbox.AddChild(hpBar);
        
        var hpLabel = new Label();
        hpLabel.Text = $"HP: {member.Stats.CurrentHP}/{member.Stats.MaxHP}";
        vbox.AddChild(hpLabel);
        
        // MP Bar
        var mpBar = new ProgressBar();
        mpBar.MaxValue = member.Stats.MaxMP;
        mpBar.Value = member.Stats.CurrentMP;
        mpBar.CustomMinimumSize = new Vector2(0, 20);
        vbox.AddChild(mpBar);
        
        var mpLabel = new Label();
        mpLabel.Text = $"MP: {member.Stats.CurrentMP}/{member.Stats.MaxMP}";
        vbox.AddChild(mpLabel);
        
        // Status effects
        if (member.Stats.ActiveStatuses.Count > 0)
        {
            var statusLabel = new Label();
            var statuses = string.Join(", ", member.Stats.ActiveStatuses.Select(s => s.Effect.ToString()));
            statusLabel.Text = $"Status: {statuses}";
            statusLabel.AddThemeColorOverride("font_color", Colors.Orange);
            vbox.AddChild(statusLabel);
        }
        
        // Baton pass indicator
        if (member.BatonPassData.IsActive)
        {
            var batonLabel = new Label();
            batonLabel.Text = $"Baton Pass x{member.BatonPassData.PassCount}";
            batonLabel.AddThemeColorOverride("font_color", GetBatonPassColor(member.BatonPassData.PassCount));
            vbox.AddChild(batonLabel);
        }
    }
    
    private void UpdateEnemyDisplay()
    {
        // Clear existing enemy displays
        foreach (var display in enemyDisplays)
        {
            display.QueueFree();
        }
        enemyDisplays.Clear();
        
        var enemies = battleManager.GetEnemyParty();
        
        foreach (var enemy in enemies)
        {
            var enemyDisplay = CreateEnemyDisplay(enemy);
            enemyPanel.AddChild(enemyDisplay);
            enemyDisplays.Add(enemyDisplay);
        }
    }
    
    private Control CreateEnemyDisplay(BattleMember enemy)
    {
        var vbox = new VBoxContainer();
        vbox.CustomMinimumSize = new Vector2(150, 100);
        
        // Name
        var nameLabel = new Label();
        nameLabel.Text = enemy.Stats.CharacterName;
        nameLabel.HorizontalAlignment = HorizontalAlignment.Center;
        vbox.AddChild(nameLabel);
        
        // HP Bar
        var hpBar = new ProgressBar();
        hpBar.MaxValue = enemy.Stats.MaxHP;
        hpBar.Value = enemy.Stats.CurrentHP;
        hpBar.CustomMinimumSize = new Vector2(0, 15);
        vbox.AddChild(hpBar);
        
        // Knockdown indicator
        if (enemy.IsKnockedDown)
        {
            var downLabel = new Label();
            downLabel.Text = "DOWN!";
            downLabel.HorizontalAlignment = HorizontalAlignment.Center;
            downLabel.AddThemeColorOverride("font_color", Colors.Red);
            vbox.AddChild(downLabel);
        }
        
        return vbox;
    }
    
    #endregion
    
    #region Notifications
    
    private void ShowNotification(Label label, string text, Color color, float duration)
    {
        label.Text = text;
        label.Modulate = color;
        label.Show();
        
        // Create fade out tween
        var tween = CreateTween();
        tween.TweenProperty(label, "modulate:a", 0.0f, duration);
        tween.TweenCallback(Callable.From(() => label.Hide()));
    }
    
    private void ShowBatonPassNotification(string from, string to, int level)
    {
        batonPassPanel.Show();
        
        // Clear and recreate content
        foreach (var child in batonPassPanel.GetChildren())
        {
            child.QueueFree();
        }
        
        var vbox = new VBoxContainer();
        batonPassPanel.AddChild(vbox);
        
        var label = new Label();
        label.Text = $"{from} → {to}\nBaton Pass x{level}";
        label.HorizontalAlignment = HorizontalAlignment.Center;
        label.AddThemeColorOverride("font_color", GetBatonPassColor(level));
        vbox.AddChild(label);
        
        // Auto-hide after delay
        GetTree().CreateTimer(2.0f).Timeout += () => batonPassPanel.Hide();
    }
    
    private Color GetBatonPassColor(int level)
    {
        return level switch
        {
            1 => new Color(1, 1, 0),      // Yellow
            2 => new Color(1, 0.5f, 0),   // Orange
            >= 3 => new Color(1, 0, 0),   // Red
            _ => Colors.White
        };
    }
    
    private void ShowDamageNumber(int damage, bool isCritical, bool isWeakness)
    {
        var label = new Label();
        label.Text = damage.ToString();
        label.Position = targetCursor.Position + new Vector2(0, -50);
    
        if (isCritical)
        {
            label.AddThemeColorOverride("font_color", Colors.Red);
            label.AddThemeFontSizeOverride("font_size", 48);
        }
        else if (isWeakness)
        {
            label.AddThemeColorOverride("font_color", Colors.Yellow);
            label.AddThemeFontSizeOverride("font_size", 36);
        }
        else
        {
            label.AddThemeColorOverride("font_color", Colors.White);
            label.AddThemeFontSizeOverride("font_size", 24);
        }
    
        damageNumbersContainer.AddChild(label);
    
        // Animate damage number
        var tween = CreateTween();
        tween.SetParallel(true);
        tween.TweenProperty(label, "position:y", label.Position.Y - 100, 1.0f);
        tween.TweenProperty(label, "modulate:a", 0.0f, 1.0f);
        tween.Chain().TweenCallback(Callable.From(() => label.QueueFree()));
    }
    
    private void FlashScreen(Color color, float duration)
    {
        var flash = new ColorRect();
        flash.Color = color;
        flash.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        flash.MouseFilter = Control.MouseFilterEnum.Ignore;
        AddChild(flash);
        
        var tween = CreateTween();
        tween.TweenProperty(flash, "modulate:a", 0.0f, duration);
        tween.TweenCallback(Callable.From(() => flash.QueueFree()));
    }
    
    #endregion
    
    #region All-Out Attack
    
    private void OnAllOutAttackYes()
    {
        allOutAttackPrompt.Hide();
        
        var leadMember = battleManager.GetPlayerParty()[0];
        var enemies = battleManager.GetLivingEnemies().ToArray();
        
        var action = new BattleAction(leadMember, BattleActionType.AllOutAttack)
            .WithTargets(enemies);
        
        battleManager.ExecuteAction(action);
    }
    
    private void OnAllOutAttackNo()
    {
        allOutAttackPrompt.Hide();
        battleManager.StartNextTurn();
    }
    
    /// <summary>
    /// Show the item menu with available consumables
    /// </summary>
    private void ShowItemMenu()
    {
        actionMenu.Hide();
        itemMenu.Show();
    
        PopulateItemList();
    }
    
    /// <summary>
    /// Populate the item list with available consumables
    /// </summary>
    private void PopulateItemList()
    {
        itemList.Clear();
    
        // Check if InventorySystem exists
        if (InventorySystem.Instance == null)
        {
            GD.PrintErr("InventorySystem not found!");
            itemList.AddItem("No inventory system available");
            itemList.SetItemDisabled(0, true);
            useItemButton.Disabled = true;
            return;
        }
    
        // Get all consumable items
        var consumables = InventorySystem.Instance.GetItemsByType(ItemType.Consumable);
    
        foreach (var slot in consumables)
        {
            if (slot.Item is ConsumableData consumable)
            {
                string displayText = $"{consumable.DisplayName} x{slot.Quantity}";
                itemList.AddItem(displayText);
            
                int index = itemList.ItemCount - 1;
                itemList.SetItemMetadata(index, consumable.ItemId);
            
                // Add icon if available
                if (consumable.Icon != null)
                {
                    itemList.SetItemIcon(index, consumable.Icon);
                }
            }
        }
    
        // Handle empty inventory
        if (itemList.ItemCount == 0)
        {
            itemList.AddItem("No usable items");
            itemList.SetItemDisabled(0, true);
            useItemButton.Disabled = true;
        }
        else
        {
            itemList.Select(0);
            OnItemListSelected(0);
        }
    }
    
    /// <summary>
    /// Called when an item is selected from the list
    /// </summary>
    private void OnItemListSelected(long index)
    {
        if (index < 0 || index >= itemList.ItemCount)
            return;
    
        string itemId = itemList.GetItemMetadata((int)index).AsString();
        var item = InventorySystem.Instance.GetItem(itemId);
    
        if (item is ConsumableData consumable)
        {
            selectedItem = consumable;
            UpdateItemInfo(consumable);
            useItemButton.Disabled = false;
        }
    }
    
    /// <summary>
    /// Update the item info panel with details
    /// </summary>
    private void UpdateItemInfo(ConsumableData item)
    {
        itemNameLabel.Text = item.DisplayName;
    
        int quantity = InventorySystem.Instance.GetItemCount(item.ItemId);
        itemQuantityLabel.Text = $"Owned: {quantity}";
    
        // Build effect description
        var effects = new List<string>();
    
        if (item.RestoresHP > 0)
            effects.Add($"+{item.RestoresHP} HP");
        if (item.HPRestorePercent > 0)
            effects.Add($"+{item.HPRestorePercent * 100:F0}% HP");
        if (item.RestoresMP > 0)
            effects.Add($"+{item.RestoresMP} MP");
        if (item.MPRestorePercent > 0)
            effects.Add($"+{item.MPRestorePercent * 100:F0}% MP");
        if (item.Revives)
            effects.Add($"Revive ({item.ReviveHPPercent * 100:F0}% HP)");
        if (item.CuresStatus?.Count > 0)
            effects.Add("Cure status");
        if (item.CuresAllStatuses)
            effects.Add("Cure all status");
        if (item.DamageAmount > 0)
            effects.Add($"{item.DamageAmount} {item.DamageElement} damage");
        if (item.AttackBuff > 0)
            effects.Add($"+{item.AttackBuff} ATK ({item.BuffDuration} turns)");
        if (item.DefenseBuff > 0)
            effects.Add($"+{item.DefenseBuff} DEF ({item.BuffDuration} turns)");
    
        string effectText = effects.Count > 0 ? string.Join(", ", effects) : "No effects";
        itemDescriptionLabel.Text = $"{item.Description}\n\nEffects: {effectText}";
    }
    
    /// <summary>
    /// Called when Use button is pressed
    /// </summary>
    private void OnUseItemPressed()
    {
        if (selectedItem == null || itemList.GetSelectedItems().Length == 0)
            return;

        // Verify we still have the item
        if (!InventorySystem.Instance.HasItem(selectedItem.ItemId))
        {
            GD.Print($"No {selectedItem.DisplayName} in inventory!");
            PopulateItemList();
            return;
        }

        // Hide item menu
        itemMenu.Hide();

        // Get valid targets
        var validTargets = GetValidItemTargets(selectedItem);

        if (validTargets.Count == 0)
        {
            GD.Print("No valid targets for this item!");
            itemMenu.Show();
            selectedItem = null;
            return;
        }

        // Check target type
        switch (selectedItem.TargetType)
        {
            case ConsumableTargetType.AllAllies:
            case ConsumableTargetType.AllEnemies:
                // Execute immediately on all targets
                ExecuteItemAction(validTargets.ToArray());
                break;

            case ConsumableTargetType.RandomEnemy:
                // Select random enemy and execute
                if (validTargets.Count > 0)
                {
                    var randomIndex = GD.RandRange(0, validTargets.Count - 1);
                    var randomTarget = validTargets[randomIndex];
                    ExecuteItemAction(new[] { randomTarget });
                }
                break;

            case ConsumableTargetType.OneAlly:
            case ConsumableTargetType.OneEnemy:
                // Show target selection for single target
                ShowTargetSelection(validTargets);
                waitingForItemTarget = true;
                break;
        }
    }
    
    // <summary>
    /// Get valid targets for an item based on its TargetType
    /// </summary>
    private List<BattleMember> GetValidItemTargets(ConsumableData item)
    {
        var targets = new List<BattleMember>();

        if (item == null)
            return targets;

        switch (item.TargetType)
        {
            case ConsumableTargetType.OneAlly:
            case ConsumableTargetType.AllAllies:
                // Revival items target dead allies
                if (item.Revives)
                {
                    var allAllies = battleManager.GetPlayerParty();
                    targets.AddRange(allAllies.Where(a => !a.Stats.IsAlive));
                }
                // Healing/support items target living allies
                else
                {
                    targets.AddRange(battleManager.GetLivingAllies());
                }
                break;

            case ConsumableTargetType.OneEnemy:
            case ConsumableTargetType.AllEnemies:
            case ConsumableTargetType.RandomEnemy:
                // Offensive items target living enemies
                targets.AddRange(battleManager.GetLivingEnemies());
                break;
        }

        return targets;
    }

    /// <summary>
    /// Execute the item action on selected target(s)
    /// </summary>
    private void ExecuteItemAction(BattleMember[] targets)
    {
        if (selectedItem == null)
            return;
    
        // Final check for item availability
        if (!InventorySystem.Instance.HasItem(selectedItem.ItemId))
        {
            GD.Print($"No {selectedItem.DisplayName} in inventory!");
            return;
        }
    
        // Create and execute the action
        var action = new BattleAction(currentActor, BattleActionType.Item)
        {
            ItemData = selectedItem
        };
        action = action.WithTargets(targets);
    
        // Remove item from inventory (BattleManager doesn't do this)
        InventorySystem.Instance.RemoveItem(selectedItem.ItemId, 1);
    
        battleManager.ExecuteAction(action);
    
        // Reset state
        selectedItem = null;
        waitingForItemTarget = false;
        HideAllMenus();
    }
    
    /// <summary>
    /// Back button from item menu
    /// </summary>
    private void OnBackFromItemsPressed()
    {
        itemMenu.Hide();
        selectedItem = null;
        ShowActionMenu();
    }
    
    /// <summary>
    /// 
    /// Checks if party has any usable items
    /// </summary>
    private bool HasUsableItems()
    {
        if (InventorySystem.Instance == null)
            return false;
    
        var consumables = InventorySystem.Instance.GetItemsByType(ItemType.Consumable);
        return consumables != null && consumables.Count > 0;
    }
    
    /// <summary>
    /// Show victory screen with rewards
    /// 
    /// </summary>
    private void ShowVictoryScreen()
    {
        GD.Print("=== VICTORY ===");
    
        // Get battle rewards
        var partyStats = battleManager.GetPlayerParty().Select(p => p.Stats).ToList();
        var defeatedEnemies = battleManager.GetEnemyParty();
        bool isBoss = false; // Set based on your battle setup
    
        var rewardsManager = new BattleRewardsManager();
        var rewards = rewardsManager.CalculateRewards(partyStats, defeatedEnemies, isBoss);
    
        // Create victory UI if scene is assigned
        if (victoryScreenScene != null)
        {
            victoryScreen = victoryScreenScene.Instantiate<Control>();
            AddChild(victoryScreen);
    
            // Set reward data (assuming your victory screen has these labels)
            SetVictoryScreenData(rewards);
    
            // Connect continue button
            var continueBtn = victoryScreen.GetNodeOrNull<Button>("ContinueButton");
            if (continueBtn != null)
            {
                continueBtn.Pressed += OnVictoryContinue;
            }
        }
        else
        {
            // Fallback: Print rewards to console
            GD.Print($"\n╔═══════════════════════════════════════╗");
            GD.Print($"║          VICTORY!                    ║");
            GD.Print($"╚═══════════════════════════════════════╝");
            GD.Print($"\n⭐ Battle Rank: {GetRankDisplay(rewards.Rank)}");
            GD.Print($"💰 Gold: {rewards.TotalGold}");
            GD.Print($"✨ EXP: {rewards.TotalExp}");
    
            if (rewards.ItemDrops.Count > 0)
            {
                GD.Print($"🎁 Items:");
                foreach (var drop in rewards.ItemDrops)
                {
                    GD.Print($"   • {drop.Item1} x{drop.Item2}");
                }
            }
    
            // Auto-continue after 3 seconds
            GetTree().CreateTimer(3.0).Timeout += ReturnToOverworld;
        }
    
        // Apply rewards
        ApplyBattleRewards(rewards);
    }
    
    /// <summary>
    /// Show defeat screen (Game Over)
    /// 
    /// </summary>
    private void ShowDefeatScreen()
    {
        GD.Print("=== DEFEAT ===");
    
        // Play game over music
        if (SystemManager.Instance != null)
        {
            SystemManager.Instance.PlayGameOverMusic();
        }
    
        // Create defeat UI if scene is assigned
        if (defeatScreenScene != null)
        {
            defeatScreen = defeatScreenScene.Instantiate<Control>();
            AddChild(defeatScreen);
        
            // Connect buttons
            var continueBtn = defeatScreen.GetNodeOrNull<Button>("ContinueButton");
            var titleBtn = defeatScreen.GetNodeOrNull<Button>("TitleButton");
        
            if (continueBtn != null)
            {
                continueBtn.Pressed += OnDefeatContinue;
            }
        
            if (titleBtn != null)
            {
                titleBtn.Pressed += OnReturnToTitle;
            }
        }
        else
        {
            // Fallback: Return to title after delay
            GD.Print("Game Over! Returning to title screen...");
            GetTree().CreateTimer(3.0).Timeout += OnReturnToTitle;
        }
    }
    
    /// <summary>
    /// Set victory screen labels with reward data
    /// </summary>
    private void SetVictoryScreenData(BattleRewardsResult rewards)
    {
        if (victoryScreen == null) return;
    
        var rankLabel = victoryScreen.GetNodeOrNull<Label>("RankLabel");
        var goldLabel = victoryScreen.GetNodeOrNull<Label>("GoldLabel");
        var expLabel = victoryScreen.GetNodeOrNull<Label>("ExpLabel");
        var itemsContainer = victoryScreen.GetNodeOrNull<VBoxContainer>("ItemsContainer");
    
        if (rankLabel != null)
            rankLabel.Text = $"Rank: {GetRankDisplay(rewards.Rank)}";
    
        if (goldLabel != null)
            goldLabel.Text = $"Gold: {rewards.TotalGold}";
    
        if (expLabel != null)
            expLabel.Text = $"EXP: {rewards.TotalExp}";
    
        if (itemsContainer != null)
        {
            foreach (var drop in rewards.ItemDrops)
            {
                var itemLabel = new Label();
                itemLabel.Text = $"{drop.Item1} x{drop.Item2}";
                itemsContainer.AddChild(itemLabel);
            }
        }
    }
    
    /// <summary>
    /// Apply rewards to party
    /// </summary>
    private void ApplyBattleRewards(BattleRewardsResult rewards)
    {
        // Add gold (FIXED: Use SaveSystem not SaveManager, and InventorySystem for gold)
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.AddGold(rewards.TotalGold);
        }
    
        // Add EXP to all living party members (FIXED: Use AddExp not AddExperience)
        var party = battleManager.GetPlayerParty();
        foreach (var member in party)
        {
            if (member.Stats.IsAlive)
            {
                member.Stats.AddExp(rewards.TotalExp);
            }
        }
    
        // Add items to inventory (FIXED: Need to fetch ItemData first)
        if (InventorySystem.Instance != null && GameManager.Instance?.Database != null)
        {
            foreach (var drop in rewards.ItemDrops)
            {
                var itemData = GameManager.Instance.Database.GetItem(drop.Item1);
                if (itemData != null)
                {
                    InventorySystem.Instance.AddItem(itemData, drop.Item2);
                }
            }
        }
    }
    
    /// <summary>
    /// Victory continue button handler
    /// </summary>
    private void OnVictoryContinue()
    {
        if (victoryScreen != null)
        {
            victoryScreen.QueueFree();
            victoryScreen = null;
        }
    
        ReturnToOverworld();
    }
    
    /// <summary>
    /// Defeat continue button - load last save
    /// </summary>
    private void OnDefeatContinue()
    {
        if (SystemManager.Instance != null)
        {
            SystemManager.Instance.PlayOkSE();
        }
    
        
        var saveSystem = EchoesAcrossTime.SaveSystem.Instance;
        if (saveSystem != null)
        {
            if (saveSystem.SaveExists(0)) // Autosave slot
            {
                saveSystem.LoadGame(0);
            }
            else
            {
                GD.PrintErr("No save file found! Returning to title.");
                OnReturnToTitle();
            }
        }
        else
        {
            OnReturnToTitle();
        }
    }
    
    /// <summary>
    /// Return to title screen
    /// </summary>
    private void OnReturnToTitle()
    {
        if (SystemManager.Instance != null)
        {
            SystemManager.Instance.PlayCancelSE();
        }
    
        GetTree().Paused = false;
        GetTree().ChangeSceneToFile("res://Scenes/TitleScreen.tscn");
    }
    
    /// <summary>
    /// Return to overworld/exploration
    /// </summary>
    private void ReturnToOverworld()
    {
        GetTree().Paused = false;
        // Replace with your actual overworld scene path
        GetTree().ChangeSceneToFile("res://Scenes/Overworld.tscn");
    }
    
    /// <summary>
    /// Helper to display battle rank
    /// </summary>
    private string GetRankDisplay(BattleRank rank)
    {
        return rank switch
        {
            BattleRank.SPlus => "S+",
            BattleRank.S => "S",
            BattleRank.A => "A",
            BattleRank.B => "B",
            BattleRank.C => "C",
            BattleRank.D => "D",
            _ => "F"
        };
    }
    
    /// <summary>
/// Play showtime cutscene animation
/// 
/// </summary>
private async void PlayShowtimeCutscene(string showtimeName, string char1, string char2)
{
    GD.Print($"[Showtime] {char1} + {char2}: {showtimeName}");
    
    // Create overlay if not exists
    if (showtimeOverlay == null)
    {
        showtimeOverlay = new Control();
        showtimeOverlay.Name = "ShowtimeOverlay";
        showtimeOverlay.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        showtimeOverlay.MouseFilter = Control.MouseFilterEnum.Stop;
        AddChild(showtimeOverlay);
        
        // Add black background
        var bg = new ColorRect();
        bg.Color = new Color(0, 0, 0, 0.8f);
        bg.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        showtimeOverlay.AddChild(bg);
        
        // Add title label (FIXED: Changed variable name to avoid conflict)
        var showTitle = new Label();
        showTitle.Name = "TitleLabel";
        showTitle.Text = showtimeName;
        showTitle.AddThemeFontSizeOverride("font_size", 64);
        showTitle.AddThemeColorOverride("font_color", Colors.Gold);
        showTitle.HorizontalAlignment = HorizontalAlignment.Center;
        showTitle.VerticalAlignment = VerticalAlignment.Center;
        showTitle.SetAnchorsPreset(Control.LayoutPreset.Center);
        showtimeOverlay.AddChild(showTitle);
        
        // Add character names (FIXED: Changed variable name to avoid conflict)
        var showNames = new Label();
        showNames.Name = "NamesLabel";
        showNames.Text = $"{char1} & {char2}";
        showNames.AddThemeFontSizeOverride("font_size", 32);
        showNames.AddThemeColorOverride("font_color", Colors.White);
        showNames.HorizontalAlignment = HorizontalAlignment.Center;
        showNames.Position = new Vector2(0, 100);
        showNames.SetAnchorsPreset(Control.LayoutPreset.Center);
        showtimeOverlay.AddChild(showNames);
    }
    
    showtimeOverlay.Show();
    
    // Animate overlay appearance
    var tween = CreateTween();
    tween.SetParallel(true);
    
    // Get the labels we created (using GetNode is safe here since we just created them)
    var titleLabel = showtimeOverlay.GetNode<Label>("TitleLabel");
    var namesLabel = showtimeOverlay.GetNode<Label>("NamesLabel");
    
    // Fade in and scale title
    titleLabel.Modulate = new Color(1, 1, 1, 0);
    titleLabel.Scale = Vector2.One * 0.5f;
    
    tween.TweenProperty(titleLabel, "modulate:a", 1.0f, 0.5);
    tween.TweenProperty(titleLabel, "scale", Vector2.One, 0.5);
    
    // Slide in names
    namesLabel.Position = new Vector2(0, 200);
    namesLabel.Modulate = new Color(1, 1, 1, 0);
    
    tween.TweenProperty(namesLabel, "position:y", 100.0f, 0.5).SetDelay(0.3);
    tween.TweenProperty(namesLabel, "modulate:a", 1.0f, 0.5).SetDelay(0.3);
    
    await ToSignal(tween, Tween.SignalName.Finished);
    
    // Hold for dramatic effect
    await ToSignal(GetTree().CreateTimer(1.5), SceneTreeTimer.SignalName.Timeout);
    
    // Fade out
    var fadeOut = CreateTween();
    fadeOut.TweenProperty(showtimeOverlay, "modulate:a", 0.0f, 0.5);
    
    await ToSignal(fadeOut, Tween.SignalName.Finished);
    
    showtimeOverlay.Hide();
    showtimeOverlay.Modulate = Colors.White; // Reset for next time
    
    GD.Print($"[Showtime] Cutscene complete!");
}
    
    /// <summary>
    /// Alternative: Load and play pre-made AnimationPlayer showtime
    /// Use this if you create custom animations in Godot editor
    /// </summary>
    private async void PlayShowtimeAnimation(string showtimeName)
    {
        if (showtimeAnimationPlayer == null)
        {
            GD.PrintErr("ShowtimeAnimationPlayer not assigned!");
            return;
        }
    
        // Play the animation by name
        string animName = showtimeName.ToLower().Replace(" ", "_");
    
        if (showtimeAnimationPlayer.HasAnimation(animName))
        {
            showtimeAnimationPlayer.Play(animName);
            await ToSignal(showtimeAnimationPlayer, AnimationPlayer.SignalName.AnimationFinished);
        }
        else
        {
            GD.PrintErr($"Showtime animation '{animName}' not found!");
            // Fallback to simple version
            PlayShowtimeCutscene(showtimeName, "", ""); // Don't await this since it's async void
        }
    }
    
    





    
    #endregion
}