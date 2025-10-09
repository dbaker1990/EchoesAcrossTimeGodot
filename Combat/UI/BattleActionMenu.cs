// ============================================================================
// FILE: Combat/UI/BattleActionMenu.cs
// Complete Action Selection Menu System 
// 
// ============================================================================

using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using EchoesAcrossTime.Combat;
using EchoesAcrossTime.Items;
using EchoesAcrossTime.Managers;
using RPG.Items;

namespace EchoesAcrossTime.Combat.UI
{
    /// <summary>
    /// Complete action selection menu for battle system.
    /// Handles Attack, Skills, Items, Guard, Limit Break, Baton Pass, and Escape.
    /// </summary>
    public partial class BattleActionMenu : CanvasLayer
    {
        #region Node References
        
        private BattleManager battleManager;
        
        // Main Action Menu
        private Panel actionMenuPanel;
        private VBoxContainer actionButtonContainer;
        private Button attackButton;
        private Button skillsButton;
        private Button itemsButton;
        private Button guardButton;
        private Button limitBreakButton;
        private Button batonPassButton;
        private Button escapeButton;
        
        // Skill Menu
        private Panel skillMenuPanel;
        private TabBar skillTabs;
        private ScrollContainer skillScrollContainer;
        private VBoxContainer skillListContainer;
        private Panel skillDetailPanel;
        private Label skillNameLabel;
        private Label skillDescriptionLabel;
        private Label skillCostLabel;
        private Label skillPowerLabel;
        private Label skillElementLabel;
        private Button useSkillButton;
        private Button backFromSkillsButton;
        
        // Item Menu
        private Panel itemMenuPanel;
        private ItemList itemList;
        private Panel itemDetailPanel;
        private Label itemNameLabel;
        private Label itemDescriptionLabel;
        private Label itemQuantityLabel;
        private Button useItemButton;
        private Button backFromItemsButton;
        
        // Target Selector
        private Control targetSelector;
        private Sprite2D targetCursor;
        private Label targetNameLabel;
        private List<BattleMember> currentTargetList = new();
        private int currentTargetIndex = 0;
        
        #endregion
        
        #region State Variables
        
        private BattleMember currentActor;
        private SkillData selectedSkill;
        private ConsumableData selectedItem;
        
        private bool waitingForTarget = false;
        private bool selectingForSkill = false;
        private bool selectingForItem = false;
        private bool selectingForBatonPass = false;
        
        private int currentSkillTab = 0; // 0=All, 1=Physical, 2=Magic, 3=Support
        private AudioStreamPlayer sfxPlayer;
        private AudioStream cursorSound;
        private AudioStream confirmSound;
        private AudioStream cancelSound;

        
        #endregion
        
        #region Initialization
        
        public override void _Ready()
        {
            // Set high layer for UI
            Layer = 100;
            
            // Get BattleManager reference
            battleManager = GetNode<BattleManager>("/root/BattleScene/BattleManager");
            
            if (battleManager == null)
            {
                GD.PrintErr("BattleActionMenu: Could not find BattleManager!");
                return;
            }
            
            // Create all UI elements
            CreateActionMenu();
            CreateSkillMenu();
            CreateItemMenu();
            CreateTargetSelector();
            
            // Connect to battle signals
            ConnectBattleSignals();
            
            // Hide everything initially
            HideAllMenus();
            
            sfxPlayer = new AudioStreamPlayer();
            AddChild(sfxPlayer);
            cursorSound = GD.Load<AudioStream>("res://Audio/SFX/cursor.wav");
            confirmSound = GD.Load<AudioStream>("res://Audio/SFX/confirm.wav");
            cancelSound = GD.Load<AudioStream>("res://Audio/SFX/cancel.wav");
            
            GD.Print("BattleActionMenu: Initialized successfully");
        }
        
        private void ConnectBattleSignals()
        {
            // FIXED: TurnStarted takes string, not BattleMember
            battleManager.TurnStarted += OnTurnStarted;
            battleManager.BattleEnded += OnBattleEnded;
        }
        
        #endregion
        
        #region UI Creation - Action Menu
        
        private void CreateActionMenu()
        {
            // Main panel
            actionMenuPanel = new Panel();
            actionMenuPanel.CustomMinimumSize = new Vector2(300, 350);
            actionMenuPanel.Position = new Vector2(50, 350);
            actionMenuPanel.ZIndex = 10;
            AddChild(actionMenuPanel);
            
            // Title label
            var titleLabel = new Label();
            titleLabel.Text = "Action";
            titleLabel.Position = new Vector2(15, 10);
            titleLabel.AddThemeColorOverride("font_color", Colors.White);
            actionMenuPanel.AddChild(titleLabel);
            
            // Button container
            actionButtonContainer = new VBoxContainer();
            actionButtonContainer.Position = new Vector2(15, 40);
            actionButtonContainer.AddThemeConstantOverride("separation", 8);
            actionMenuPanel.AddChild(actionButtonContainer);
            
            // Create buttons
            attackButton = CreateActionButton("Attack", OnAttackPressed);
            skillsButton = CreateActionButton("Skills", OnSkillsPressed);
            itemsButton = CreateActionButton("Items", OnItemsPressed);
            guardButton = CreateActionButton("Guard", OnGuardPressed);
            limitBreakButton = CreateActionButton("Limit Break", OnLimitBreakPressed);
            batonPassButton = CreateActionButton("Baton Pass", OnBatonPassPressed);
            escapeButton = CreateActionButton("Escape", OnEscapePressed);
            
            actionMenuPanel.Hide();
        }
        
        private Button CreateActionButton(string text, Action onPressed)
        {
            var button = new Button();
            button.Text = text;
            button.CustomMinimumSize = new Vector2(260, 40);
            button.Pressed += () => onPressed?.Invoke();
    
            // Add hover effects
            button.MouseEntered += () => {
                button.Modulate = Colors.Yellow;
                PlaySFX(cursorSound);
            };
            button.MouseExited += () => {
                button.Modulate = Colors.White;
            };
    
            actionButtonContainer.AddChild(button);
            return button;
        }
        
        #endregion
        
        #region UI Creation - Skill Menu
        
        private void CreateSkillMenu()
        {
            // Main panel
            skillMenuPanel = new Panel();
            skillMenuPanel.CustomMinimumSize = new Vector2(700, 500);
            skillMenuPanel.Position = new Vector2(300, 150);
            skillMenuPanel.ZIndex = 11;
            AddChild(skillMenuPanel);
            
            // Main container
            var mainContainer = new VBoxContainer();
            mainContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            mainContainer.AddThemeConstantOverride("separation", 10);
            skillMenuPanel.AddChild(mainContainer);
            
            // Title
            var titleLabel = new Label();
            titleLabel.Text = "Select Skill";
            titleLabel.AddThemeColorOverride("font_color", Colors.White);
            mainContainer.AddChild(titleLabel);
            
            // Tabs for skill categories
            skillTabs = new TabBar();
            skillTabs.AddTab("All");
            skillTabs.AddTab("Physical");
            skillTabs.AddTab("Magic");
            skillTabs.AddTab("Support");
            skillTabs.TabChanged += OnSkillTabChanged;
            mainContainer.AddChild(skillTabs);
            
            // Skill list (scrollable)
            skillScrollContainer = new ScrollContainer();
            skillScrollContainer.CustomMinimumSize = new Vector2(680, 250);
            skillScrollContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            mainContainer.AddChild(skillScrollContainer);
            
            skillListContainer = new VBoxContainer();
            skillListContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            skillScrollContainer.AddChild(skillListContainer);
            
            // Skill detail panel
            skillDetailPanel = new Panel();
            skillDetailPanel.CustomMinimumSize = new Vector2(680, 120);
            mainContainer.AddChild(skillDetailPanel);
            
            var detailVBox = new VBoxContainer();
            detailVBox.Position = new Vector2(10, 10);
            skillDetailPanel.AddChild(detailVBox);
            
            skillNameLabel = new Label();
            skillNameLabel.AddThemeColorOverride("font_color", Colors.Yellow);
            detailVBox.AddChild(skillNameLabel);
            
            skillDescriptionLabel = new Label();
            skillDescriptionLabel.AutowrapMode = TextServer.AutowrapMode.Word;
            skillDescriptionLabel.CustomMinimumSize = new Vector2(660, 0);
            detailVBox.AddChild(skillDescriptionLabel);
            
            skillCostLabel = new Label();
            detailVBox.AddChild(skillCostLabel);
            
            skillPowerLabel = new Label();
            detailVBox.AddChild(skillPowerLabel);
            
            skillElementLabel = new Label();
            detailVBox.AddChild(skillElementLabel);
            
            // Buttons
            var buttonContainer = new HBoxContainer();
            buttonContainer.AddThemeConstantOverride("separation", 10);
            mainContainer.AddChild(buttonContainer);
            
            useSkillButton = new Button();
            useSkillButton.Text = "Use";
            useSkillButton.CustomMinimumSize = new Vector2(100, 40);
            useSkillButton.Pressed += OnUseSkillPressed;
            useSkillButton.Disabled = true;
            buttonContainer.AddChild(useSkillButton);
            
            backFromSkillsButton = new Button();
            backFromSkillsButton.Text = "Back";
            backFromSkillsButton.CustomMinimumSize = new Vector2(100, 40);
            backFromSkillsButton.Pressed += OnBackFromSkillsPressed;
            buttonContainer.AddChild(backFromSkillsButton);
            
            skillMenuPanel.Hide();
        }
        
        #endregion
        
        #region UI Creation - Item Menu
        
        private void CreateItemMenu()
        {
            // Main panel
            itemMenuPanel = new Panel();
            itemMenuPanel.CustomMinimumSize = new Vector2(600, 500);
            itemMenuPanel.Position = new Vector2(350, 150);
            itemMenuPanel.ZIndex = 11;
            AddChild(itemMenuPanel);
            
            // Main container
            var mainContainer = new VBoxContainer();
            mainContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            mainContainer.AddThemeConstantOverride("separation", 10);
            mainContainer.OffsetLeft = 10;
            mainContainer.OffsetTop = 10;
            mainContainer.OffsetRight = -10;
            mainContainer.OffsetBottom = -10;
            itemMenuPanel.AddChild(mainContainer);
            
            // Title
            var titleLabel = new Label();
            titleLabel.Text = "Select Item";
            titleLabel.AddThemeColorOverride("font_color", Colors.White);
            mainContainer.AddChild(titleLabel);
            
            // Item list
            itemList = new ItemList();
            itemList.CustomMinimumSize = new Vector2(580, 250);
            itemList.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            itemList.ItemSelected += OnItemListSelected;
            mainContainer.AddChild(itemList);
            
            // Item detail panel
            itemDetailPanel = new Panel();
            itemDetailPanel.CustomMinimumSize = new Vector2(580, 100);
            mainContainer.AddChild(itemDetailPanel);
            
            var detailVBox = new VBoxContainer();
            detailVBox.Position = new Vector2(10, 10);
            itemDetailPanel.AddChild(detailVBox);
            
            itemNameLabel = new Label();
            itemNameLabel.AddThemeColorOverride("font_color", Colors.Yellow);
            detailVBox.AddChild(itemNameLabel);
            
            itemDescriptionLabel = new Label();
            itemDescriptionLabel.AutowrapMode = TextServer.AutowrapMode.Word;
            itemDescriptionLabel.CustomMinimumSize = new Vector2(560, 0);
            detailVBox.AddChild(itemDescriptionLabel);
            
            itemQuantityLabel = new Label();
            detailVBox.AddChild(itemQuantityLabel);
            
            // Buttons
            var buttonContainer = new HBoxContainer();
            buttonContainer.AddThemeConstantOverride("separation", 10);
            mainContainer.AddChild(buttonContainer);
            
            useItemButton = new Button();
            useItemButton.Text = "Use";
            useItemButton.CustomMinimumSize = new Vector2(100, 40);
            useItemButton.Pressed += OnUseItemPressed;
            useItemButton.Disabled = true;
            buttonContainer.AddChild(useItemButton);
            
            backFromItemsButton = new Button();
            backFromItemsButton.Text = "Back";
            backFromItemsButton.CustomMinimumSize = new Vector2(100, 40);
            backFromItemsButton.Pressed += OnBackFromItemsPressed;
            buttonContainer.AddChild(backFromItemsButton);
            
            itemMenuPanel.Hide();
        }
        
        #endregion
        
        #region UI Creation - Target Selector
        
        private void CreateTargetSelector()
        {
            targetSelector = new Control();
            targetSelector.ZIndex = 20;
            AddChild(targetSelector);
            
            // Cursor sprite
            targetCursor = new Sprite2D();
            targetCursor.Modulate = Colors.Yellow;
            targetCursor.Scale = new Vector2(2, 2);
            targetSelector.AddChild(targetCursor);
            
            // Target name label
            targetNameLabel = new Label();
            targetNameLabel.Position = new Vector2(0, -50);
            targetNameLabel.AddThemeColorOverride("font_color", Colors.White);
            targetCursor.AddChild(targetNameLabel);
            
            targetSelector.Hide();
        }
        
        #endregion
        
        #region Action Menu Handlers
        
        private void OnAttackPressed()
        {
            GD.Print("Attack selected");
            PlaySFX(confirmSound);
            actionMenuPanel.Hide();
            
            var enemies = battleManager.GetLivingEnemies().ToList();
            if (enemies.Count == 0)
            {
                GD.PrintErr("No living enemies to attack!");
                actionMenuPanel.Show();
                return;
            }
            
            ShowTargetSelection(enemies, false, false, false);
        }
        
        private void PlaySFX(AudioStream sound)
        {
            if (sound == null || sfxPlayer == null) return;
            sfxPlayer.Stream = sound;
            sfxPlayer.Play();
        }
        
        private void OnSkillsPressed()
        {
            GD.Print("Skills selected");
            PlaySFX(confirmSound);
            actionMenuPanel.Hide();
            ShowSkillMenu();
        }
        
        private void OnItemsPressed()
        {
            GD.Print("Items selected");
            PlaySFX(confirmSound);
            actionMenuPanel.Hide();
            ShowItemMenu();
        }
        
        private void OnGuardPressed()
        {
            GD.Print("Guard selected");
            PlaySFX(confirmSound);
            var action = new BattleAction(currentActor, BattleActionType.Guard);
            battleManager.ExecuteAction(action);
            
            HideAllMenus();
        }
        
        private void OnLimitBreakPressed()
        {
            GD.Print("Limit Break selected");
            
            // FIXED: LimitGauge is on BattleMember, not CharacterStats
            if (currentActor?.LimitGauge < 100)
            {
                GD.Print("Limit Gauge not full!");
                return;
            }
            
            // Get available limit breaks from BattleManager
            var limitBreaks = battleManager.GetAvailableLimitBreaks(currentActor);
            if (limitBreaks == null || limitBreaks.Count == 0)
            {
                GD.Print("No Limit Breaks equipped!");
                return;
            }
            
            // For now, just use the first one
            var limitBreak = limitBreaks[0];
            
            actionMenuPanel.Hide();
            
            // Determine targets based on limit break
            var targets = GetTargetsForLimitBreak(limitBreak);
            if (targets.Count == 0)
            {
                GD.Print("No valid targets for Limit Break!");
                actionMenuPanel.Show();
                return;
            }
            
            // Execute limit break
            var action = new BattleAction(currentActor, BattleActionType.LimitBreak)
                .WithLimitBreak(limitBreak)
                .WithTargets(targets.ToArray());
            
            battleManager.ExecuteAction(action);
            HideAllMenus();
        }
        
        private void OnBatonPassPressed()
        {
            GD.Print("Baton Pass selected");
            
            // Get eligible baton pass targets (living allies except current actor)
            var allies = battleManager.GetLivingAllies()
                .Where(a => a != currentActor)
                .ToList();
            
            if (allies.Count == 0)
            {
                GD.Print("No valid Baton Pass targets!");
                return;
            }
            
            actionMenuPanel.Hide();
            ShowTargetSelection(allies, false, false, true);
        }
        
        private void OnEscapePressed()
        {
            GD.Print("Escape selected");
            
            if (!battleManager.CanEscape())
            {
                GD.Print("Cannot escape from this battle!");
                return;
            }
            
            var action = new BattleAction(currentActor, BattleActionType.Escape);
            battleManager.ExecuteAction(action);
            
            HideAllMenus();
        }
        
        #endregion
        
        #region Skill Menu Methods
        
        private void ShowSkillMenu()
        {
            skillMenuPanel.Show();
            PopulateSkillList();
            backFromSkillsButton.GrabFocus();
        }
        
        private void PopulateSkillList()
        {
            // Clear existing skills
            foreach (var child in skillListContainer.GetChildren())
            {
                child.QueueFree();
            }
            
            var skills = currentActor?.Stats?.Skills?.GetEquippedSkills();
            if (skills == null || skills.Count == 0)
            {
                var noSkillsLabel = new Label();
                noSkillsLabel.Text = "No skills equipped";
                skillListContainer.AddChild(noSkillsLabel);
                return;
            }
            
            // Filter skills based on current tab
            var filteredSkills = FilterSkillsByTab(skills);
            
            foreach (var skill in filteredSkills)
            {
                var skillButton = CreateSkillButton(skill);
                skillListContainer.AddChild(skillButton);
            }
        }
        
        private List<SkillData> FilterSkillsByTab(List<SkillData> skills)
        {
            return currentSkillTab switch
            {
                0 => skills, // All
                1 => skills.Where(s => s.DamageType == DamageType.Physical).ToList(),
                2 => skills.Where(s => s.DamageType == DamageType.Magical).ToList(),
                3 => skills.Where(s => s.Target == SkillTarget.SingleAlly || 
                                      s.Target == SkillTarget.AllAllies).ToList(),
                _ => skills
            };
        }
        
        private Button CreateSkillButton(SkillData skill)
        {
            var button = new Button();
            button.Text = $"{skill.DisplayName} (MP: {skill.MPCost})";
            button.CustomMinimumSize = new Vector2(660, 35);
            button.ButtonPressed = false;
            
            // Check if skill can be used
            bool canUse = skill.CanUse(currentActor.Stats);
            button.Disabled = !canUse;
            
            if (!canUse)
            {
                button.TooltipText = "Not enough MP";
            }
            
            button.Pressed += () => OnSkillButtonPressed(skill);
            
            return button;
        }
        
        private void OnSkillButtonPressed(SkillData skill)
        {
            selectedSkill = skill;
            DisplaySkillDetails(skill);
            useSkillButton.Disabled = false;
        }
        
        private void DisplaySkillDetails(SkillData skill)
        {
            skillNameLabel.Text = skill.DisplayName;
            skillDescriptionLabel.Text = skill.Description;
            skillCostLabel.Text = $"MP Cost: {skill.MPCost}";
            // FIXED: Use BasePower instead of Power
            skillPowerLabel.Text = $"Power: {skill.BasePower}";
            skillElementLabel.Text = $"Element: {skill.Element}";
        }
        
        private void OnSkillTabChanged(long tab)
        {
            currentSkillTab = (int)tab;
            PopulateSkillList();
        }
        
        private void OnUseSkillPressed()
        {
            if (selectedSkill == null)
            {
                GD.PrintErr("No skill selected!");
                return;
            }
            
            GD.Print($"Using skill: {selectedSkill.DisplayName}");
            skillMenuPanel.Hide();
            
            // Determine targets based on skill
            var targets = GetTargetsForSkill(selectedSkill);
            if (targets.Count == 0)
            {
                GD.Print("No valid targets!");
                skillMenuPanel.Show();
                return;
            }
            
            // If skill targets all, execute immediately
            if (selectedSkill.Target == SkillTarget.AllEnemies || 
                selectedSkill.Target == SkillTarget.AllAllies ||
                selectedSkill.Target == SkillTarget.Self)
            {
                ExecuteSkillAction(targets.ToArray());
            }
            else
            {
                // Show target selection
                ShowTargetSelection(targets, true, false, false);
            }
        }
        
        private void OnBackFromSkillsPressed()
        {
            selectedSkill = null;
            skillMenuPanel.Hide();
            actionMenuPanel.Show();
            attackButton.GrabFocus();
        }
        
        #endregion
        
        #region Item Menu Methods
        
        private void ShowItemMenu()
        {
            itemMenuPanel.Show();
            PopulateItemList();
            backFromItemsButton.GrabFocus();
        }
        
        private void PopulateItemList()
        {
            itemList.Clear();
            
            if (InventorySystem.Instance == null)
            {
                GD.PrintErr("InventorySystem not found!");
                itemList.AddItem("No inventory system available");
                itemList.SetItemDisabled(0, true);
                return;
            }
            
            var items = InventorySystem.Instance.GetItemsByType(ItemType.Consumable);
            
            if (items.Count == 0)
            {
                itemList.AddItem("No consumable items");
                itemList.SetItemDisabled(0, true);
                useItemButton.Disabled = true;
                return;
            }
            
            foreach (var slot in items)
            {
                if (slot.Item is ConsumableData consumable)
                {
                    string displayText = $"{consumable.DisplayName} x{slot.Quantity}";
                    itemList.AddItem(displayText);
                    
                    int index = itemList.ItemCount - 1;
                    itemList.SetItemMetadata(index, consumable);
                    
                    if (consumable.Icon != null)
                    {
                        itemList.SetItemIcon(index, consumable.Icon);
                    }
                }
            }
        }
        
        private void OnItemListSelected(long index)
        {
            var item = itemList.GetItemMetadata((int)index).As<ConsumableData>();
            if (item == null) return;
            
            selectedItem = item;
            DisplayItemDetails(item);
            useItemButton.Disabled = false;
        }
        
        private void DisplayItemDetails(ConsumableData item)
        {
            itemNameLabel.Text = item.DisplayName;
            itemDescriptionLabel.Text = item.Description;
            
            // FIXED: Use GetItemCount instead of GetItemQuantity
            var quantity = InventorySystem.Instance.GetItemCount(item.ItemId);
            itemQuantityLabel.Text = $"Quantity: {quantity}";
        }
        
        private void OnUseItemPressed()
        {
            if (selectedItem == null)
            {
                GD.PrintErr("No item selected!");
                return;
            }
            
            GD.Print($"Using item: {selectedItem.DisplayName}");
            itemMenuPanel.Hide();
            
            // Determine targets based on item effect
            var targets = GetTargetsForItem(selectedItem);
            if (targets.Count == 0)
            {
                GD.Print("No valid targets!");
                itemMenuPanel.Show();
                return;
            }
            
            ShowTargetSelection(targets, false, true, false);
        }
        
        private void OnBackFromItemsPressed()
        {
            selectedItem = null;
            itemMenuPanel.Hide();
            actionMenuPanel.Show();
            attackButton.GrabFocus();
        }
        
        #endregion
        
        #region Target Selection
        
        private void ShowTargetSelection(List<BattleMember> targets, bool forSkill, bool forItem, bool forBatonPass)
        {
            currentTargetList = targets;
            currentTargetIndex = 0;
            
            selectingForSkill = forSkill;
            selectingForItem = forItem;
            selectingForBatonPass = forBatonPass;
            waitingForTarget = true;
            
            targetSelector.Show();
            UpdateTargetCursor();
            
            GD.Print($"Target selection active: {targets.Count} targets available");
        }
        
        private void UpdateTargetCursor()
        {
            if (currentTargetList.Count == 0) return;
            
            var target = currentTargetList[currentTargetIndex];
            
            // Position cursor above target
            var targetPosition = GetTargetPosition(target);
            targetCursor.GlobalPosition = targetPosition + new Vector2(0, -80);
            
            // Update name label
            targetNameLabel.Text = target.Stats.CharacterName;
        }
        
        private Vector2 GetTargetPosition(BattleMember target)
        {
            // This is a placeholder - adjust based on your actual battle layout
            
            if (target.IsPlayerControlled)
            {
                // Player positions (left side)
                int index = battleManager.GetPlayerParty().IndexOf(target);
                return new Vector2(200, 300 + (index * 100));
            }
            else
            {
                // Enemy positions (right side)
                int index = battleManager.GetEnemyParty().IndexOf(target);
                return new Vector2(900, 300 + (index * 100));
            }
        }
        
        private void MoveTargetSelection(int direction)
        {
            currentTargetIndex += direction;
            
            // Wrap around
            if (currentTargetIndex < 0)
                currentTargetIndex = currentTargetList.Count - 1;
            else if (currentTargetIndex >= currentTargetList.Count)
                currentTargetIndex = 0;
            
            UpdateTargetCursor();
        }
        
        private void ConfirmTargetSelection()
        {
            if (currentTargetList.Count == 0) return;
            
            var target = currentTargetList[currentTargetIndex];
            
            targetSelector.Hide();
            waitingForTarget = false;
            
            // Execute appropriate action
            if (selectingForSkill)
            {
                ExecuteSkillAction(new[] { target });
            }
            else if (selectingForItem)
            {
                ExecuteItemAction(new[] { target });
            }
            else if (selectingForBatonPass)
            {
                ExecuteBatonPass(target);
            }
            else
            {
                // Regular attack
                ExecuteAttackAction(target);
            }
            
            // Reset selection flags
            selectingForSkill = false;
            selectingForItem = false;
            selectingForBatonPass = false;
        }
        
        private void CancelTargetSelection()
        {
            targetSelector.Hide();
            waitingForTarget = false;
            
            // Return to appropriate menu
            if (selectingForSkill)
            {
                skillMenuPanel.Show();
            }
            else if (selectingForItem)
            {
                itemMenuPanel.Show();
            }
            else
            {
                actionMenuPanel.Show();
            }
            
            // Reset flags
            selectingForSkill = false;
            selectingForItem = false;
            selectingForBatonPass = false;
        }
        
        #endregion
        
        #region Action Execution
        
        private void ExecuteAttackAction(BattleMember target)
        {
            var action = new BattleAction(currentActor, BattleActionType.Attack)
                .WithTargets(target);
            
            battleManager.ExecuteAction(action);
            HideAllMenus();
        }
        
        private void ExecuteSkillAction(BattleMember[] targets)
        {
            if (selectedSkill == null) return;
            
            var action = new BattleAction(currentActor, BattleActionType.Skill)
                .WithSkill(selectedSkill)
                .WithTargets(targets);
            
            battleManager.ExecuteAction(action);
            
            selectedSkill = null;
            HideAllMenus();
        }
        
        private void ExecuteItemAction(BattleMember[] targets)
        {
            if (selectedItem == null) return;
            
            var action = new BattleAction(currentActor, BattleActionType.Item);
            action.ItemData = selectedItem;
            action = action.WithTargets(targets);
            
            battleManager.ExecuteAction(action);
            
            selectedItem = null;
            HideAllMenus();
        }
        
        private void ExecuteBatonPass(BattleMember target)
        {
            // FIXED: Use Pass instead of BatonPass
            var action = new BattleAction(currentActor, BattleActionType.Pass)
                .WithTargets(target);
            
            battleManager.ExecuteAction(action);
            HideAllMenus();
        }
        
        #endregion
        
        #region Target Determination Helpers
        
        private List<BattleMember> GetTargetsForSkill(SkillData skill)
        {
            return skill.Target switch
            {
                SkillTarget.SingleEnemy => battleManager.GetLivingEnemies().ToList(),
                SkillTarget.AllEnemies => battleManager.GetLivingEnemies().ToList(),
                SkillTarget.RandomEnemy => battleManager.GetLivingEnemies().ToList(),
                SkillTarget.SingleAlly => battleManager.GetLivingAllies().ToList(),
                SkillTarget.AllAllies => battleManager.GetLivingAllies().ToList(),
                SkillTarget.Self => new List<BattleMember> { currentActor },
                SkillTarget.DeadAlly => battleManager.GetPlayerParty()
                    .Where(m => !m.Stats.IsAlive).ToList(),
                _ => new List<BattleMember>()
            };
        }
        
        private List<BattleMember> GetTargetsForItem(ConsumableData item)
        {
            // Determine target based on item effect
            // Most healing items target allies
            if (item.Description.Contains("Revive", StringComparison.OrdinalIgnoreCase))
            {
                return battleManager.GetPlayerParty()
                    .Where(m => !m.Stats.IsAlive).ToList();
            }
            else
            {
                return battleManager.GetLivingAllies().ToList();
            }
        }
        
        private List<BattleMember> GetTargetsForLimitBreak(LimitBreakData limitBreak)
        {
            // FIXED: Check HitsAllEnemies instead of TargetAllEnemies
            return limitBreak.HitsAllEnemies 
                ? battleManager.GetLivingEnemies().ToList()
                : battleManager.GetLivingEnemies().ToList();
        }
        
        #endregion
        
        #region Input Handling
        
        public override void _Input(InputEvent @event)
        {
            if (!waitingForTarget) return;
            
            if (@event.IsActionPressed("ui_left"))
            {
                MoveTargetSelection(-1);
                GetViewport().SetInputAsHandled();
            }
            else if (@event.IsActionPressed("ui_right"))
            {
                MoveTargetSelection(1);
                GetViewport().SetInputAsHandled();
            }
            else if (@event.IsActionPressed("ui_up"))
            {
                MoveTargetSelection(-1);
                GetViewport().SetInputAsHandled();
            }
            else if (@event.IsActionPressed("ui_down"))
            {
                MoveTargetSelection(1);
                GetViewport().SetInputAsHandled();
            }
            else if (@event.IsActionPressed("ui_accept"))
            {
                ConfirmTargetSelection();
                GetViewport().SetInputAsHandled();
            }
            else if (@event.IsActionPressed("ui_cancel"))
            {
                CancelTargetSelection();
                GetViewport().SetInputAsHandled();
            }
        }
        
        #endregion
        
        #region Battle Event Handlers
        
        private void OnTurnStarted(string characterName)
        {
            GD.Print($"BattleActionMenu: Turn started for {characterName}");
            
            // Get the current actor from BattleManager
            currentActor = battleManager.CurrentActor;
            
            if (currentActor != null && currentActor.IsPlayerControlled)
            {
                ShowActionMenuForPlayer();
            }
            else
            {
                HideAllMenus();
            }
        }
        
        private void ShowActionMenuForPlayer()
        {
            // Update button states FIRST
            UpdateActionButtonStates();
    
            actionMenuPanel.Show();

            var startPos = actionMenuPanel.Position;
            actionMenuPanel.Position = startPos + new Vector2(-100, 0);

            var tween = CreateTween();
            tween.TweenProperty(actionMenuPanel, "position", startPos, 0.2f)
                .SetTrans(Tween.TransitionType.Cubic)
                .SetEase(Tween.EaseType.Out);

            attackButton.GrabFocus();
        }
        
        private void UpdateActionButtonStates()
        {
            // Check MP for skills
            bool hasMP = currentActor.Stats.CurrentMP > 0;
            skillsButton.Disabled = !hasMP;
            
            // Check if can escape
            escapeButton.Disabled = !battleManager.CanEscape();
            
            // FIXED: Check LimitGauge on BattleMember, not CharacterStats
            bool canLimitBreak = currentActor?.LimitGauge >= 100;
            limitBreakButton.Disabled = !canLimitBreak;
            
            // Check Baton Pass (only if has One More)
            bool canBatonPass = currentActor?.HasExtraTurn ?? false;
            batonPassButton.Disabled = !canBatonPass;
            
            // Items always available (unless inventory is empty)
        }
        
        private void OnBattleEnded(bool victory)
        {
            HideAllMenus();
        }
        
        #endregion
        
        #region Utility Methods
        
        private void HideAllMenus()
        {
            actionMenuPanel.Hide();
            skillMenuPanel.Hide();
            itemMenuPanel.Hide();
            targetSelector.Hide();
            
            waitingForTarget = false;
            selectingForSkill = false;
            selectingForItem = false;
            selectingForBatonPass = false;
            
            selectedSkill = null;
            selectedItem = null;
        }
        
        /// <summary>
        /// Call this to manually show the action menu (for testing)
        /// </summary>
        public void ShowMenu()
        {
            if (currentActor == null)
            {
                GD.PrintErr("No current actor set!");
                return;
            }
            
            ShowActionMenuForPlayer();
        }
        
        /// <summary>
        /// Call this to manually hide all menus
        /// </summary>
        public void HideMenu()
        {
            HideAllMenus();
        }
        
        #endregion
    }
}