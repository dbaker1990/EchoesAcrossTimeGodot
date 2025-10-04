using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using EchoesAcrossTime.Combat;

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
    
    #endregion
    
    #region State Variables
    
    private BattleMember currentActor;
    private List<BattleMember> currentTargets = new();
    private SkillData selectedSkill;
    private bool isSelectingTarget = false;
    private ElementType currentSkillTabFilter = ElementType.Physical;
    
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
        // Action menu buttons
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
        GD.Print($"Battle UI: Showtime! {showtimeName}");
        // TODO: Play showtime cutscene animation
    }
    
    private void OnAllOutAttackReady()
    {
        allOutAttackPrompt.Show();
    }
    
    private void OnBattleEnded(bool victory)
    {
        HideAllMenus();
        // TODO: Show victory/defeat screen
    }
    
    #endregion
    
    #region Action Menu
    
    private void ShowActionMenu()
    {
        actionMenu.Show();
        
        // Enable/disable options based on availability
        bool canBatonPass = battleManager.CanBatonPass();
        
        // For now, keep all buttons visible but could disable some
        attackButton.Disabled = false;
        skillsButton.Disabled = currentActor?.Stats?.Skills?.GetEquippedSkills()?.Count == 0;
        itemsButton.Disabled = false; // TODO: Check if party has items
        guardButton.Disabled = false;
        escapeButton.Disabled = false;
    }
    
    private void HideAllMenus()
    {
        actionMenu.Hide();
        skillMenu.Hide();
        targetSelector.Hide();
        allOutAttackPrompt.Hide();
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
    
    private void OnItemsPressed()
    {
        GD.Print("Items not yet implemented");
        // TODO: Show items menu
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
        
        if (selectedSkill != null)
        {
            // Skill action
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
    
    #endregion
}