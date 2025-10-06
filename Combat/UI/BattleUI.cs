// ============================================================================
// FILE: Combat/UI/BattleUI.cs (WITH ITEMS, BATON PASS & FULL FEATURES)
// ============================================================================
using Godot;
using System.Linq;
using EchoesAcrossTime.Combat;
using EchoesAcrossTime.Items;

namespace EchoesAcrossTime.Combat.UI
{
    public partial class BattleUI : CanvasLayer
    {
        private BattleManager battleManager;
        
        // UI Components
        private PartyStatusDisplay partyDisplay;
        private EnemyStatusDisplay enemyDisplay;
        private TurnOrderDisplay turnOrderDisplay;
        private SkillMenu skillMenu;
        private TargetSelector targetSelector;
        private BatonPassIndicator batonPassIndicator;
        private TechnicalIndicator technicalIndicator;
        private AllOutAttackPrompt allOutAttackPrompt;
        private ShowtimePrompt showtimePrompt;
        
        // Action menu
        private Panel actionMenuPanel;
        private Button attackButton;
        private Button skillsButton;
        private Button itemsButton;
        private Button guardButton;
        private Button batonPassButton;
        
        // Item Menu components
        private Panel itemMenuPanel;
        private ItemList itemList;
        private Label itemNameLabel;
        private Label itemDescriptionLabel;
        private Label itemQuantityLabel;
        private Button useItemButton;
        private Button backFromItemsButton;
        
        // State tracking
        private SkillData selectedSkill;
        private ConsumableData selectedItem;
        private bool waitingForTarget;
        private bool waitingForSkillMenu;
        private bool selectingBatonPassTarget;
        private bool waitingForItemTarget;
        
        public override void _Ready()
        {
            Layer = 100;
            
            battleManager = GetNode<BattleManager>("/root/BattleScene/BattleManager");
            
            CreateAllUI();
            
            if (battleManager != null)
            {
                battleManager.TurnStarted += OnTurnStarted;
                battleManager.BattleEnded += OnBattleEnded;
                battleManager.BatonPassExecuted += OnBatonPassExecuted;
                battleManager.OneMoreTriggered += OnOneMoreTriggered;
            }
        }
        
        private void CreateAllUI()
        {
            partyDisplay = new PartyStatusDisplay();
            partyDisplay.Position = new Vector2(20, 150);
            AddChild(partyDisplay);
            
            enemyDisplay = new EnemyStatusDisplay();
            AddChild(enemyDisplay);
            
            turnOrderDisplay = new TurnOrderDisplay();
            AddChild(turnOrderDisplay);
            
            skillMenu = new SkillMenu();
            AddChild(skillMenu);
            
            targetSelector = new TargetSelector();
            AddChild(targetSelector);
            
            batonPassIndicator = new BatonPassIndicator();
            AddChild(batonPassIndicator);
            
            technicalIndicator = new TechnicalIndicator();
            AddChild(technicalIndicator);
            
            allOutAttackPrompt = new AllOutAttackPrompt();
            AddChild(allOutAttackPrompt);
            
            showtimePrompt = new ShowtimePrompt();
            AddChild(showtimePrompt);
            
            CreateItemMenu();
            CreateActionMenu();
        }
        
        private void CreateActionMenu()
        {
            actionMenuPanel = new Panel();
            actionMenuPanel.Position = new Vector2(900, 500);
            actionMenuPanel.CustomMinimumSize = new Vector2(280, 300);
            AddChild(actionMenuPanel);
            
            var vbox = new VBoxContainer();
            vbox.Position = new Vector2(15, 15);
            vbox.AddThemeConstantOverride("separation", 10);
            actionMenuPanel.AddChild(vbox);
            
            attackButton = new Button();
            attackButton.Text = "Attack";
            attackButton.CustomMinimumSize = new Vector2(250, 40);
            attackButton.Pressed += OnAttackPressed;
            vbox.AddChild(attackButton);
            
            skillsButton = new Button();
            skillsButton.Text = "Skills";
            skillsButton.CustomMinimumSize = new Vector2(250, 40);
            skillsButton.Pressed += OnSkillsPressed;
            vbox.AddChild(skillsButton);
            
            itemsButton = new Button();
            itemsButton.Text = "Items";
            itemsButton.CustomMinimumSize = new Vector2(250, 40);
            itemsButton.Pressed += OnItemsPressed;
            vbox.AddChild(itemsButton);
            
            guardButton = new Button();
            guardButton.Text = "Guard";
            guardButton.CustomMinimumSize = new Vector2(250, 40);
            guardButton.Pressed += OnGuardPressed;
            vbox.AddChild(guardButton);
            
            batonPassButton = new Button();
            batonPassButton.Text = "Baton Pass";
            batonPassButton.CustomMinimumSize = new Vector2(250, 40);
            batonPassButton.Pressed += OnBatonPassPressed;
            batonPassButton.Disabled = true;
            vbox.AddChild(batonPassButton);
            
            actionMenuPanel.Hide();
        }
        
        private void CreateItemMenu()
        {
            itemMenuPanel = new Panel();
            itemMenuPanel.Position = new Vector2(500, 150);
            itemMenuPanel.CustomMinimumSize = new Vector2(420, 520);
            itemMenuPanel.Hide();
            AddChild(itemMenuPanel);
            
            var vbox = new VBoxContainer();
            vbox.Position = new Vector2(15, 15);
            vbox.AddThemeConstantOverride("separation", 10);
            itemMenuPanel.AddChild(vbox);
            
            // Title
            var titleLabel = new Label();
            titleLabel.Text = "Items";
            titleLabel.AddThemeFontSizeOverride("font_size", 28);
            titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
            vbox.AddChild(titleLabel);
            
            // Item list
            itemList = new ItemList();
            itemList.CustomMinimumSize = new Vector2(390, 250);
            itemList.ItemSelected += OnItemListSelected;
            vbox.AddChild(itemList);
            
            // Item info panel
            var infoPanel = new Panel();
            infoPanel.CustomMinimumSize = new Vector2(390, 120);
            vbox.AddChild(infoPanel);
            
            var infoVBox = new VBoxContainer();
            infoVBox.Position = new Vector2(10, 10);
            infoVBox.AddThemeConstantOverride("separation", 5);
            infoPanel.AddChild(infoVBox);
            
            itemNameLabel = new Label();
            itemNameLabel.AddThemeFontSizeOverride("font_size", 20);
            infoVBox.AddChild(itemNameLabel);
            
            itemDescriptionLabel = new Label();
            itemDescriptionLabel.AutowrapMode = TextServer.AutowrapMode.Word;
            itemDescriptionLabel.CustomMinimumSize = new Vector2(370, 60);
            infoVBox.AddChild(itemDescriptionLabel);
            
            itemQuantityLabel = new Label();
            itemQuantityLabel.AddThemeFontSizeOverride("font_size", 16);
            infoVBox.AddChild(itemQuantityLabel);
            
            // Buttons
            var buttonHBox = new HBoxContainer();
            buttonHBox.AddThemeConstantOverride("separation", 15);
            vbox.AddChild(buttonHBox);
            
            useItemButton = new Button();
            useItemButton.Text = "Use";
            useItemButton.CustomMinimumSize = new Vector2(185, 45);
            useItemButton.Pressed += OnUseItemPressed;
            buttonHBox.AddChild(useItemButton);
            
            backFromItemsButton = new Button();
            backFromItemsButton.Text = "Back";
            backFromItemsButton.CustomMinimumSize = new Vector2(185, 45);
            backFromItemsButton.Pressed += OnBackFromItemsPressed;
            buttonHBox.AddChild(backFromItemsButton);
        }
        
        #region Action Menu Handlers
        
        private void OnTurnStarted(string characterName)
        {
            var currentActor = battleManager.CurrentActor;
            if (currentActor != null && currentActor.IsPlayerControlled)
            {
                actionMenuPanel.Show();
                UpdateBatonPassButton();
            }
            else
            {
                actionMenuPanel.Hide();
            }
        }
        
        private void UpdateBatonPassButton()
        {
            bool canBatonPass = battleManager.CanBatonPass();
            batonPassButton.Disabled = !canBatonPass;
            
            if (canBatonPass)
            {
                var currentActor = battleManager.CurrentActor;
                int passLevel = currentActor.BatonPassData.PassCount + 1;
                float multiplier = 1.0f + (passLevel * 0.5f);
                
                batonPassButton.Text = $"Baton Pass (x{multiplier:F1})";
                batonPassButton.Modulate = Colors.Orange;
            }
            else
            {
                batonPassButton.Text = "Baton Pass";
                batonPassButton.Modulate = Colors.White;
            }
        }
        
        private void OnOneMoreTriggered(string characterName)
        {
            UpdateBatonPassButton();
            ShowOneMoreBanner();
        }
        
        private void ShowOneMoreBanner()
        {
            var banner = new Label();
            banner.Text = "⭐ ONE MORE! ⭐";
            banner.AddThemeFontSizeOverride("font_size", 64);
            banner.Modulate = Colors.Yellow;
            banner.Position = new Vector2(400, 250);
            banner.ZIndex = 200;
            
            banner.AddThemeColorOverride("font_outline_color", Colors.Black);
            banner.AddThemeConstantOverride("outline_size", 5);
            
            AddChild(banner);
            
            var tween = CreateTween();
            tween.SetParallel(true);
            tween.TweenProperty(banner, "scale", Vector2.One * 1.5f, 0.2f);
            tween.TweenProperty(banner, "scale", Vector2.One, 0.2f).SetDelay(0.2f);
            tween.Chain().TweenProperty(banner, "modulate:a", 0.0f, 0.5f).SetDelay(0.5f);
            tween.TweenCallback(Callable.From(() => banner.QueueFree()));
        }
        
        private void OnBattleEnded(bool playerVictory)
        {
            GD.Print(playerVictory ? "Victory!" : "Defeat!");
        }
        
        private void OnAttackPressed()
        {
            actionMenuPanel.Hide();
            selectedSkill = null;
            selectedItem = null;
            var enemies = battleManager.GetLivingEnemies();
            targetSelector.ShowSelection(enemies);
            waitingForTarget = true;
            selectingBatonPassTarget = false;
            waitingForItemTarget = false;
        }
        
        private void OnSkillsPressed()
        {
            actionMenuPanel.Hide();
            skillMenu.ShowMenu(battleManager.CurrentActor);
            waitingForSkillMenu = true;
        }
        
        private void OnItemsPressed()
        {
            actionMenuPanel.Hide();
            ShowItemMenu();
            waitingForItemTarget = false;
        }
        
        private void OnGuardPressed()
        {
            var action = new BattleAction(battleManager.CurrentActor, BattleActionType.Guard);
            battleManager.ExecuteAction(action);
        }
        
        private void OnBatonPassPressed()
        {
            if (!battleManager.CanBatonPass())
            {
                GD.Print("Cannot Baton Pass right now!");
                return;
            }
            
            actionMenuPanel.Hide();
            
            var validTargets = battleManager.GetBatonPassTargets();
            
            if (validTargets.Count == 0)
            {
                GD.Print("No valid Baton Pass targets!");
                actionMenuPanel.Show();
                return;
            }
            
            targetSelector.ShowSelection(validTargets);
            selectingBatonPassTarget = true;
            waitingForTarget = true;
            waitingForItemTarget = false;
            
            GD.Print($"Select Baton Pass target from {validTargets.Count} party members");
        }
        
        #endregion
        
        #region Item Menu Methods
        
        private void ShowItemMenu()
        {
            itemMenuPanel.Show();
            PopulateItemList();
        }
        
        private void PopulateItemList()
        {
            itemList.Clear();
            
            if (InventorySystem.Instance == null)
            {
                GD.PrintErr("InventorySystem not found!");
                itemList.AddItem("No inventory system!");
                itemList.SetItemDisabled(0, true);
                useItemButton.Disabled = true;
                return;
            }
            
            // Get all consumable items (no CanUseInBattle property - just check if consumable)
            var items = InventorySystem.Instance.GetItemsByType(ItemType.Consumable);
            
            foreach (var slot in items)
            {
                // Cast to ConsumableData to access consumable-specific properties
                if (slot.Item is ConsumableData consumable)
                {
                    string displayText = $"{consumable.DisplayName} x{slot.Quantity}";
                    itemList.AddItem(displayText);
                    
                    int index = itemList.ItemCount - 1;
                    itemList.SetItemMetadata(index, consumable.ItemId);
                    
                    if (consumable.Icon != null)
                    {
                        itemList.SetItemIcon(index, consumable.Icon);
                    }
                }
            }
            
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
        
        private void UpdateItemInfo(ConsumableData item)
        {
            itemNameLabel.Text = item.DisplayName;
            
            int quantity = InventorySystem.Instance.GetItemCount(item.ItemId);
            itemQuantityLabel.Text = $"Owned: {quantity}";
            
            var effects = new System.Collections.Generic.List<string>();
            
            if (item.RestoresHP > 0)
                effects.Add($"+{item.RestoresHP} HP");
            if (item.RestoresHPPercent > 0)
                effects.Add($"+{item.RestoresHPPercent * 100:F0}% HP");
            if (item.RestoresMP > 0)
                effects.Add($"+{item.RestoresMP} MP");
            if (item.RestoresMPPercent > 0)
                effects.Add($"+{item.RestoresMPPercent * 100:F0}% MP");
            if (item.Revives)
                effects.Add($"Revive ({item.ReviveHPPercent * 100:F0}% HP)");
            if (item.CuresStatuses.Count > 0)
                effects.Add("Cure status");
            if (item.CuresAllStatuses)
                effects.Add("Cure all status");
            if (item.DamageAmount > 0)
                effects.Add($"{item.DamageAmount} {item.DamageElement} dmg");
            if (item.AttackBuff > 0)
                effects.Add($"+{item.AttackBuff} ATK ({item.BuffDuration} turns)");
            if (item.DefenseBuff > 0)
                effects.Add($"+{item.DefenseBuff} DEF ({item.BuffDuration} turns)");
            
            string effectText = effects.Count > 0 ? string.Join(", ", effects) : "No effects";
            itemDescriptionLabel.Text = $"{item.Description}\n\nEffects: {effectText}";
        }
        
        private void OnUseItemPressed()
        {
            if (selectedItem == null || itemList.GetSelectedItems().Length == 0)
                return;
            
            if (!InventorySystem.Instance.HasItem(selectedItem.ItemId))
            {
                GD.Print($"No {selectedItem.DisplayName} in inventory!");
                PopulateItemList();
                return;
            }
            
            itemMenuPanel.Hide();
            
            var validTargets = GetValidItemTargets(selectedItem);
            
            if (validTargets.Count == 0)
            {
                GD.Print("No valid targets for this item!");
                itemMenuPanel.Show();
                selectedItem = null;
                return;
            }
            
            if (selectedItem.TargetAll)
            {
                ExecuteItemAction(validTargets.ToArray());
            }
            else
            {
                targetSelector.ShowSelection(validTargets);
                waitingForTarget = true;
                waitingForItemTarget = true;
                selectingBatonPassTarget = false;
            }
        }
        
        private System.Collections.Generic.List<BattleMember> GetValidItemTargets(ConsumableData item)
        {
            var targets = new System.Collections.Generic.List<BattleMember>();
            
            // Offensive items target living enemies
            if (item.DamageAmount > 0)
            {
                targets.AddRange(battleManager.GetLivingEnemies());
            }
            // Revival items target dead allies
            else if (item.Revives)
            {
                var allAllies = battleManager.GetPlayerParty();
                targets.AddRange(allAllies.Where(a => !a.Stats.IsAlive));
            }
            // Support items target living allies
            else
            {
                targets.AddRange(battleManager.GetLivingAllies());
            }
            
            return targets;
        }
        
        private void ExecuteItemAction(BattleMember[] targets)
        {
            if (selectedItem == null)
                return;
            
            if (!InventorySystem.Instance.HasItem(selectedItem.ItemId))
            {
                GD.Print($"No {selectedItem.DisplayName} in inventory!");
                selectedItem = null;
                actionMenuPanel.Show();
                return;
            }
            
            // Create action with ItemData property (no WithItem method exists)
            var action = new BattleAction(battleManager.CurrentActor, BattleActionType.Item)
            {
                ItemData = selectedItem
            };
            action = action.WithTargets(targets);
            
            var result = battleManager.ExecuteAction(action);
            
            if (result.Success)
            {
                InventorySystem.Instance.RemoveItem(selectedItem.ItemId, 1);
                int remaining = InventorySystem.Instance.GetItemCount(selectedItem.ItemId);
                GD.Print($"Used {selectedItem.DisplayName}, {remaining} remaining");
            }
            
            selectedItem = null;
            waitingForItemTarget = false;
        }
        
        private void OnBackFromItemsPressed()
        {
            itemMenuPanel.Hide();
            selectedItem = null;
            actionMenuPanel.Show();
        }
        
        #endregion
        
        #region Baton Pass Visual Effects
        
        private void OnBatonPassExecuted(string fromCharacter, string toCharacter, int passLevel)
        {
            ShowBatonPassEffect(fromCharacter, toCharacter, passLevel);
        }
        
        private void ShowBatonPassEffect(string from, string to, int passLevel)
        {
            var effectLabel = new Label();
            effectLabel.Text = $"🎯 {from} → {to}\nCHAIN LEVEL {passLevel}!";
            effectLabel.AddThemeFontSizeOverride("font_size", 36);
            effectLabel.Modulate = passLevel switch
            {
                1 => Colors.Yellow,
                2 => Colors.Orange,
                3 => Colors.Red,
                _ => Colors.White
            };
            effectLabel.Position = new Vector2(400, 300);
            effectLabel.HorizontalAlignment = HorizontalAlignment.Center;
            effectLabel.ZIndex = 150;
            
            effectLabel.AddThemeColorOverride("font_outline_color", Colors.Black);
            effectLabel.AddThemeConstantOverride("outline_size", 4);
            
            AddChild(effectLabel);
            
            var tween = CreateTween();
            tween.SetParallel(true);
            tween.TweenProperty(effectLabel, "scale", Vector2.One * 1.3f, 0.3f);
            tween.TweenProperty(effectLabel, "scale", Vector2.One, 0.3f).SetDelay(0.3f);
            tween.Chain().TweenProperty(effectLabel, "modulate:a", 0.0f, 0.5f).SetDelay(0.5f);
            tween.TweenCallback(Callable.From(() => effectLabel.QueueFree()));
        }
        
        #endregion
        
        #region Skill Targeting Logic
        
        private void HandleSkillTargeting(SkillData skill)
        {
            // Handle different skill target types
            switch (skill.Target)
            {
                case SkillTarget.AllEnemies:
                    // Automatically target all living enemies
                    ExecuteSkillAction(battleManager.GetLivingEnemies().ToArray());
                    break;
                    
                case SkillTarget.AllAllies:
                    // Automatically target all living allies
                    ExecuteSkillAction(battleManager.GetLivingAllies().ToArray());
                    break;
                    
                case SkillTarget.Everyone:
                    // Target everyone on the battlefield
                    var everyone = new System.Collections.Generic.List<BattleMember>();
                    everyone.AddRange(battleManager.GetPlayerParty().Where(p => p.Stats.IsAlive));
                    everyone.AddRange(battleManager.GetEnemyParty().Where(e => e.Stats.IsAlive));
                    ExecuteSkillAction(everyone.ToArray());
                    break;
                    
                case SkillTarget.Self:
                    // Automatically target the caster
                    ExecuteSkillAction(new[] { battleManager.CurrentActor });
                    break;
                    
                case SkillTarget.DeadAlly:
                    // Show target selection for dead allies only
                    var deadAllies = battleManager.GetPlayerParty()
                        .Where(p => !p.Stats.IsAlive)
                        .ToList();
                    
                    if (deadAllies.Count == 0)
                    {
                        GD.Print("No dead allies to revive!");
                        actionMenuPanel.Show();
                        selectedSkill = null;
                    }
                    else
                    {
                        targetSelector.ShowSelection(deadAllies);
                        waitingForTarget = true;
                        selectingBatonPassTarget = false;
                        waitingForItemTarget = false;
                    }
                    break;
                    
                case SkillTarget.SingleEnemy:
                case SkillTarget.RandomEnemy:
                    // Show target selection for enemies
                    var enemies = battleManager.GetLivingEnemies();
                    
                    if (enemies.Count == 0)
                    {
                        GD.Print("No valid enemy targets!");
                        actionMenuPanel.Show();
                        selectedSkill = null;
                    }
                    else
                    {
                        // RandomEnemy still lets player choose in player-controlled mode
                        targetSelector.ShowSelection(enemies);
                        waitingForTarget = true;
                        selectingBatonPassTarget = false;
                        waitingForItemTarget = false;
                    }
                    break;
                    
                case SkillTarget.SingleAlly:
                    // Show target selection for living allies
                    var allies = battleManager.GetLivingAllies();
                    
                    if (allies.Count == 0)
                    {
                        GD.Print("No valid ally targets!");
                        actionMenuPanel.Show();
                        selectedSkill = null;
                    }
                    else
                    {
                        targetSelector.ShowSelection(allies);
                        waitingForTarget = true;
                        selectingBatonPassTarget = false;
                        waitingForItemTarget = false;
                    }
                    break;
                    
                default:
                    GD.PrintErr($"Unhandled skill target type: {skill.Target}");
                    actionMenuPanel.Show();
                    selectedSkill = null;
                    break;
            }
        }
        
        private void ExecuteSkillAction(BattleMember[] targets)
        {
            if (selectedSkill == null || targets.Length == 0)
                return;
            
            var action = new BattleAction(battleManager.CurrentActor, BattleActionType.Skill)
                .WithSkill(selectedSkill)
                .WithTargets(targets);
            
            battleManager.ExecuteAction(action);
            selectedSkill = null;
        }
        
        #endregion
        
        #region Main Update Loop
        
        public override void _Process(double delta)
        {
            // Check if skill menu completed
            if (waitingForSkillMenu && !skillMenu.Visible)
            {
                waitingForSkillMenu = false;
                
                if (skillMenu.WasCancelled)
                {
                    actionMenuPanel.Show();
                }
                else if (skillMenu.SelectedSkill != null)
                {
                    selectedSkill = skillMenu.SelectedSkill;
                    HandleSkillTargeting(selectedSkill);
                }
            }
            
            // Check if target selection completed
            if (waitingForTarget && !targetSelector.Visible && targetSelector.WasSelectionMade())
            {
                waitingForTarget = false;
                var target = targetSelector.GetSelectedTarget();
                targetSelector.ClearSelection();
                
                if (target != null)
                {
                    if (selectingBatonPassTarget)
                    {
                        HandleBatonPassTarget(target);
                    }
                    else if (waitingForItemTarget)
                    {
                        ExecuteItemAction(new[] { target });
                    }
                    else
                    {
                        HandleTargetSelected(target);
                    }
                }
                else
                {
                    // Cancelled - return to appropriate menu
                    if (waitingForItemTarget)
                    {
                        itemMenuPanel.Show();
                    }
                    else
                    {
                        actionMenuPanel.Show();
                    }
                }
                
                selectingBatonPassTarget = false;
                waitingForItemTarget = false;
            }
        }
        
        #endregion
        
        #region Target Handlers
        
        private void HandleBatonPassTarget(BattleMember target)
        {
            bool success = battleManager.ExecuteBatonPass(target);
            
            if (success)
            {
                GD.Print($"Baton Pass executed to {target.Stats.CharacterName}!");
            }
            else
            {
                GD.Print("Baton Pass failed!");
                actionMenuPanel.Show();
            }
        }
        
        private void HandleTargetSelected(BattleMember target)
        {
            if (selectedSkill != null)
            {
                var action = new BattleAction(battleManager.CurrentActor, BattleActionType.Skill)
                    .WithSkill(selectedSkill)
                    .WithTargets(target);
                
                battleManager.ExecuteAction(action);
                selectedSkill = null;
            }
            else
            {
                var action = new BattleAction(battleManager.CurrentActor, BattleActionType.Attack)
                    .WithTargets(target);
                
                battleManager.ExecuteAction(action);
            }
        }
        
        #endregion
    }
}