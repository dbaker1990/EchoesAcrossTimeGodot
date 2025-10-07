// ============================================================================
// FILE: Combat/UI/BattleUI.cs (ENHANCED WITH ALL REFINEMENTS)
// ============================================================================
using Godot;
using System.Linq;
using EchoesAcrossTime.Combat;
using EchoesAcrossTime.Items;
using RPG.Items;

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
        private Button limitBreakButton;
        private Button batonPassButton;
        private Button escapeButton;
        
        // Item Menu components
        private Panel itemMenuPanel;
        private ItemList itemList;
        private Label itemNameLabel;
        private Label itemDescriptionLabel;
        private Label itemQuantityLabel;
        private Button useItemButton;
        private Button backFromItemsButton;
        
        // Limit Break Selection
        private Panel limitBreakPanel;
        private VBoxContainer limitBreakList;
        private Label limitBreakInfoLabel;
        private Button useLimitBreakButton;
        private Button backFromLimitBreakButton;
        
        // Showtime Selection
        private Panel showtimePanel;
        private VBoxContainer showtimeList;
        private Label showtimeInfoLabel;
        private Button useShowtimeButton;
        private Button backFromShowtimeButton;
        
        // State tracking
        private SkillData selectedSkill;
        private ConsumableData selectedItem;
        private LimitBreakData selectedLimitBreak;
        private ShowtimeAttackData selectedShowtime;
        private bool waitingForTarget;
        private bool waitingForSkillMenu;
        private bool selectingBatonPassTarget;
        private bool waitingForItemTarget;
        private bool waitingForLimitBreakTarget;
        private bool waitingForShowtimeConfirmation;
        
        // Visual effects
        private CpuParticles2D hitParticles;
        private CpuParticles2D criticalParticles;
        
        public override void _Ready()
        {
            Layer = 100;
            
            battleManager = GetNode<BattleManager>("/root/BattleScene/BattleManager");
            
            CreateAllUI();
            CreateParticleEffects();
            
            if (battleManager != null)
            {
                ConnectSignals();
            }
        }
        
        private void ConnectSignals()
        {
            battleManager.TurnStarted += OnTurnStarted;
            battleManager.BattleEnded += OnBattleEnded;
            battleManager.BatonPassExecuted += OnBatonPassExecuted;
            battleManager.OneMoreTriggered += OnOneMoreTriggered;
            battleManager.AllOutAttackReady += OnAllOutAttackReady;
            battleManager.TechnicalDamage += OnTechnicalDamage;
            battleManager.ShowtimeTriggered += OnShowtimeTriggered;
            battleManager.LimitBreakReady += OnLimitBreakReady;
        }
        
        private void CreateParticleEffects()
        {
            // Hit particles (general damage)
            hitParticles = new CpuParticles2D();
            hitParticles.Amount = 20;
            hitParticles.Lifetime = 0.5f;
            hitParticles.OneShot = true;
            hitParticles.Explosiveness = 0.8f;
            hitParticles.Spread = Mathf.DegToRad(360); // Godot 4 uses radians
            hitParticles.InitialVelocityMin = 100;
            hitParticles.InitialVelocityMax = 200;
            hitParticles.Scale = new Vector2(2, 2);
            hitParticles.Color = Colors.White;
            hitParticles.ZIndex = 150;
            hitParticles.Emitting = false;
            AddChild(hitParticles);
            
            // Critical hit particles (gold sparkles)
            criticalParticles = new CpuParticles2D();
            criticalParticles.Amount = 40;
            criticalParticles.Lifetime = 0.8f;
            criticalParticles.OneShot = true;
            criticalParticles.Explosiveness = 0.9f;
            criticalParticles.Spread = Mathf.DegToRad(360); // Godot 4 uses radians
            criticalParticles.InitialVelocityMin = 150;
            criticalParticles.InitialVelocityMax = 300;
            criticalParticles.Scale = new Vector2(3, 3);
            criticalParticles.Color = Colors.Gold;
            criticalParticles.ZIndex = 150;
            criticalParticles.Emitting = false;
            AddChild(criticalParticles);
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
            CreateLimitBreakMenu();
            CreateShowtimeMenu();
            CreateActionMenu();
        }
        
        private void CreateActionMenu()
        {
            actionMenuPanel = new Panel();
            actionMenuPanel.Position = new Vector2(900, 450);
            actionMenuPanel.CustomMinimumSize = new Vector2(280, 360);
            AddChild(actionMenuPanel);
            
            var vbox = new VBoxContainer();
            vbox.Position = new Vector2(15, 15);
            vbox.AddThemeConstantOverride("separation", 8);
            actionMenuPanel.AddChild(vbox);
            
            attackButton = new Button();
            attackButton.Text = "Attack";
            attackButton.CustomMinimumSize = new Vector2(250, 35);
            attackButton.Pressed += OnAttackPressed;
            vbox.AddChild(attackButton);
            
            skillsButton = new Button();
            skillsButton.Text = "Skills";
            skillsButton.CustomMinimumSize = new Vector2(250, 35);
            skillsButton.Pressed += OnSkillsPressed;
            vbox.AddChild(skillsButton);
            
            itemsButton = new Button();
            itemsButton.Text = "Items";
            itemsButton.CustomMinimumSize = new Vector2(250, 35);
            itemsButton.Pressed += OnItemsPressed;
            vbox.AddChild(itemsButton);
            
            guardButton = new Button();
            guardButton.Text = "Guard";
            guardButton.CustomMinimumSize = new Vector2(250, 35);
            guardButton.Pressed += OnGuardPressed;
            vbox.AddChild(guardButton);
            
            limitBreakButton = new Button();
            limitBreakButton.Text = "Limit Break";
            limitBreakButton.CustomMinimumSize = new Vector2(250, 35);
            limitBreakButton.Pressed += OnLimitBreakPressed;
            vbox.AddChild(limitBreakButton);
            
            batonPassButton = new Button();
            batonPassButton.Text = "Baton Pass";
            batonPassButton.CustomMinimumSize = new Vector2(250, 35);
            batonPassButton.Pressed += OnBatonPassPressed;
            batonPassButton.Disabled = true;
            vbox.AddChild(batonPassButton);
            
            escapeButton = new Button();
            escapeButton.Text = "Escape";
            escapeButton.CustomMinimumSize = new Vector2(250, 35);
            escapeButton.Pressed += OnEscapePressed;
            vbox.AddChild(escapeButton);
            
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
            
            var titleLabel = new Label();
            titleLabel.Text = "Items";
            titleLabel.AddThemeFontSizeOverride("font_size", 28);
            titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
            vbox.AddChild(titleLabel);
            
            itemList = new ItemList();
            itemList.CustomMinimumSize = new Vector2(390, 250);
            itemList.ItemSelected += OnItemListSelected;
            vbox.AddChild(itemList);
            
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
        
        private void CreateLimitBreakMenu()
        {
            limitBreakPanel = new Panel();
            limitBreakPanel.Position = new Vector2(350, 150);
            limitBreakPanel.CustomMinimumSize = new Vector2(600, 450);
            limitBreakPanel.Hide();
            AddChild(limitBreakPanel);
            
            var vbox = new VBoxContainer();
            vbox.Position = new Vector2(15, 15);
            vbox.AddThemeConstantOverride("separation", 10);
            limitBreakPanel.AddChild(vbox);
            
            var titleLabel = new Label();
            titleLabel.Text = "⚡ LIMIT BREAK ⚡";
            titleLabel.AddThemeFontSizeOverride("font_size", 36);
            titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
            titleLabel.Modulate = Colors.Yellow;
            vbox.AddChild(titleLabel);
            
            var scrollContainer = new ScrollContainer();
            scrollContainer.CustomMinimumSize = new Vector2(570, 240);
            vbox.AddChild(scrollContainer);
            
            limitBreakList = new VBoxContainer();
            limitBreakList.AddThemeConstantOverride("separation", 8);
            scrollContainer.AddChild(limitBreakList);
            
            limitBreakInfoLabel = new Label();
            limitBreakInfoLabel.CustomMinimumSize = new Vector2(570, 80);
            limitBreakInfoLabel.AutowrapMode = TextServer.AutowrapMode.Word;
            limitBreakInfoLabel.AddThemeFontSizeOverride("font_size", 16);
            vbox.AddChild(limitBreakInfoLabel);
            
            var buttonHBox = new HBoxContainer();
            buttonHBox.AddThemeConstantOverride("separation", 15);
            vbox.AddChild(buttonHBox);
            
            useLimitBreakButton = new Button();
            useLimitBreakButton.Text = "Use Limit Break!";
            useLimitBreakButton.CustomMinimumSize = new Vector2(270, 50);
            useLimitBreakButton.Pressed += OnUseLimitBreakPressed;
            buttonHBox.AddChild(useLimitBreakButton);
            
            backFromLimitBreakButton = new Button();
            backFromLimitBreakButton.Text = "Back";
            backFromLimitBreakButton.CustomMinimumSize = new Vector2(270, 50);
            backFromLimitBreakButton.Pressed += OnBackFromLimitBreakPressed;
            buttonHBox.AddChild(backFromLimitBreakButton);
        }
        
        private void CreateShowtimeMenu()
        {
            showtimePanel = new Panel();
            showtimePanel.Position = new Vector2(350, 150);
            showtimePanel.CustomMinimumSize = new Vector2(600, 450);
            showtimePanel.Hide();
            AddChild(showtimePanel);
            
            var vbox = new VBoxContainer();
            vbox.Position = new Vector2(15, 15);
            vbox.AddThemeConstantOverride("separation", 10);
            showtimePanel.AddChild(vbox);
            
            var titleLabel = new Label();
            titleLabel.Text = "★ SHOWTIME ATTACK ★";
            titleLabel.AddThemeFontSizeOverride("font_size", 36);
            titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
            titleLabel.Modulate = Colors.Orange;
            vbox.AddChild(titleLabel);
            
            var scrollContainer = new ScrollContainer();
            scrollContainer.CustomMinimumSize = new Vector2(570, 240);
            vbox.AddChild(scrollContainer);
            
            showtimeList = new VBoxContainer();
            showtimeList.AddThemeConstantOverride("separation", 8);
            scrollContainer.AddChild(showtimeList);
            
            showtimeInfoLabel = new Label();
            showtimeInfoLabel.CustomMinimumSize = new Vector2(570, 80);
            showtimeInfoLabel.AutowrapMode = TextServer.AutowrapMode.Word;
            showtimeInfoLabel.AddThemeFontSizeOverride("font_size", 16);
            vbox.AddChild(showtimeInfoLabel);
            
            var buttonHBox = new HBoxContainer();
            buttonHBox.AddThemeConstantOverride("separation", 15);
            vbox.AddChild(buttonHBox);
            
            useShowtimeButton = new Button();
            useShowtimeButton.Text = "Use Showtime!";
            useShowtimeButton.CustomMinimumSize = new Vector2(270, 50);
            useShowtimeButton.Pressed += OnUseShowtimePressed;
            buttonHBox.AddChild(useShowtimeButton);
            
            backFromShowtimeButton = new Button();
            backFromShowtimeButton.Text = "Back";
            backFromShowtimeButton.CustomMinimumSize = new Vector2(270, 50);
            backFromShowtimeButton.Pressed += OnBackFromShowtimePressed;
            buttonHBox.AddChild(backFromShowtimeButton);
        }
        
        #region Action Menu Handlers
        
        private void OnTurnStarted(string characterName)
        {
            var currentActor = battleManager.CurrentActor;
            if (currentActor != null && currentActor.IsPlayerControlled)
            {
                actionMenuPanel.Show();
                UpdateActionMenuButtons();
            }
            else
            {
                actionMenuPanel.Hide();
            }
        }
        
        private void UpdateActionMenuButtons()
        {
            var currentActor = battleManager.CurrentActor;
            
            // Update Baton Pass
            bool canBatonPass = battleManager.CanBatonPass();
            batonPassButton.Disabled = !canBatonPass;
            
            if (canBatonPass)
            {
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
            
            // Update Limit Break
            bool limitReady = battleManager.IsLimitBreakReady(currentActor);
            limitBreakButton.Disabled = !limitReady;
            
            if (limitReady)
            {
                limitBreakButton.Text = "⚡ LIMIT BREAK! ⚡";
                limitBreakButton.Modulate = Colors.Yellow;
            }
            else
            {
                float gaugePercent = battleManager.GetLimitGaugePercent(currentActor);
                limitBreakButton.Text = $"Limit Break ({gaugePercent:F0}%)";
                limitBreakButton.Modulate = Colors.White;
            }
            
            // Update Escape
            bool canEscape = battleManager.CanEscape();
            escapeButton.Disabled = !canEscape;
            
            if (canEscape)
            {
                int escapeChance = battleManager.GetEscapeChance();
                escapeButton.Text = $"Escape ({escapeChance}%)";
            }
            else
            {
                escapeButton.Text = "Escape (Blocked)";
            }
        }
        
        private void OnOneMoreTriggered(string characterName)
        {
            UpdateActionMenuButtons();
            ShowOneMoreBanner();
            ShakeScreen(5.0f, 0.3f);
            TriggerHitParticles(new Vector2(640, 360), Colors.Yellow);
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
            waitingForLimitBreakTarget = false;
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
        }
        
        private void OnGuardPressed()
        {
            var action = new BattleAction(battleManager.CurrentActor, BattleActionType.Guard);
            battleManager.ExecuteAction(action);
        }
        
        private void OnLimitBreakPressed()
        {
            var currentActor = battleManager.CurrentActor;
            if (!battleManager.IsLimitBreakReady(currentActor))
            {
                GD.Print("Limit Break not ready!");
                return;
            }
            
            actionMenuPanel.Hide();
            ShowLimitBreakMenu();
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
            waitingForLimitBreakTarget = false;
            
            GD.Print($"Select Baton Pass target from {validTargets.Count} party members");
        }
        
        private void OnEscapePressed()
        {
            if (!battleManager.CanEscape())
            {
                GD.Print("Cannot escape!");
                return;
            }
            
            var action = new BattleAction(battleManager.CurrentActor, BattleActionType.Escape);
            battleManager.ExecuteAction(action);
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
            
            var items = InventorySystem.Instance.GetItemsByType(ItemType.Consumable);
            
            foreach (var slot in items)
            {
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
            if (item.HPRestorePercent > 0)
                effects.Add($"+{item.HPRestorePercent * 100:F0}% HP");
            if (item.RestoresMP > 0)
                effects.Add($"+{item.RestoresMP} MP");
            if (item.MPRestorePercent > 0)
                effects.Add($"+{item.MPRestorePercent * 100:F0}% MP");
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
                waitingForLimitBreakTarget = false;
            }
        }
        
        private System.Collections.Generic.List<BattleMember> GetValidItemTargets(ConsumableData item)
        {
            var targets = new System.Collections.Generic.List<BattleMember>();
            
            if (item.DamageAmount > 0)
            {
                targets.AddRange(battleManager.GetLivingEnemies());
            }
            else if (item.Revives)
            {
                var allAllies = battleManager.GetPlayerParty();
                targets.AddRange(allAllies.Where(a => !a.Stats.IsAlive));
            }
            else
            {
                targets.AddRange(battleManager.GetLivingAllies());
            }
            
            return targets;
        }
        
        private void ExecuteItemAction(BattleMember[] targets)
        {
            if (selectedItem == null) return;
    
            // FIX 1: Use battleManager.CurrentActor instead of currentActor
            var action = new BattleAction(battleManager.CurrentActor, BattleActionType.Item)
            {
                ItemData = selectedItem
            };
            action = action.WithTargets(targets);
    
            // FIX 2: Don't assign to var - ExecuteAction returns void
            battleManager.ExecuteAction(action);
    
            // FIX 3: Always remove item (or check if BattleItemSystem already does this)
            InventorySystem.Instance.RemoveItem(selectedItem.ItemId, 1);
            int remaining = InventorySystem.Instance.GetItemCount(selectedItem.ItemId);
            GD.Print($"Used {selectedItem.DisplayName}, {remaining} remaining");
    
            // FIX 4: Instead of HideAllMenus(), manually hide the relevant panels
            selectedItem = null;
            waitingForItemTarget = false;
            itemMenuPanel.Hide();
            actionMenuPanel.Hide();
        }
        
        private void OnBackFromItemsPressed()
        {
            itemMenuPanel.Hide();
            selectedItem = null;
            actionMenuPanel.Show();
        }
        
        #endregion
        
        #region Limit Break Menu Methods
        
        private void ShowLimitBreakMenu()
        {
            limitBreakPanel.Show();
            PopulateLimitBreakList();
        }
        
        private void PopulateLimitBreakList()
        {
            // Clear existing buttons
            foreach (var child in limitBreakList.GetChildren())
            {
                child.QueueFree();
            }
            
            var currentActor = battleManager.CurrentActor;
            var limitBreaks = GetAvailableLimitBreaks(currentActor);
            
            if (limitBreaks.Count == 0)
            {
                var noLBLabel = new Label();
                noLBLabel.Text = "No Limit Breaks available!";
                noLBLabel.HorizontalAlignment = HorizontalAlignment.Center;
                limitBreakList.AddChild(noLBLabel);
                
                useLimitBreakButton.Disabled = true;
                limitBreakInfoLabel.Text = "";
                return;
            }
            
            // Auto-select first one
            selectedLimitBreak = limitBreaks[0];
            
            foreach (var lb in limitBreaks)
            {
                var button = new Button();
                button.Text = $"⚡ {lb.DisplayName}";
                button.CustomMinimumSize = new Vector2(540, 50);
                button.AddThemeFontSizeOverride("font_size", 20);
                
                // Highlight selected
                if (lb == selectedLimitBreak)
                {
                    button.Modulate = Colors.Yellow;
                }
                
                button.Pressed += () => OnLimitBreakSelected(lb);
                limitBreakList.AddChild(button);
            }
            
            UpdateLimitBreakInfo();
            useLimitBreakButton.Disabled = false;
        }
        
        private void OnLimitBreakSelected(LimitBreakData lb)
        {
            selectedLimitBreak = lb;
            
            // Update button highlights
            foreach (var child in limitBreakList.GetChildren())
            {
                if (child is Button btn)
                {
                    btn.Modulate = Colors.White;
                }
            }
            
            // Highlight selected
            foreach (var child in limitBreakList.GetChildren())
            {
                if (child is Button btn && btn.Text.Contains(lb.DisplayName))
                {
                    btn.Modulate = Colors.Yellow;
                }
            }
            
            UpdateLimitBreakInfo();
        }
        
        private void UpdateLimitBreakInfo()
        {
            if (selectedLimitBreak == null)
            {
                limitBreakInfoLabel.Text = "";
                return;
            }
            
            var info = new System.Text.StringBuilder();
            info.AppendLine($"Power: {selectedLimitBreak.BasePower} x{selectedLimitBreak.PowerMultiplier}");
            info.AppendLine($"Type: {selectedLimitBreak.Type}");
            
            if (selectedLimitBreak.HitsAllEnemies)
                info.AppendLine("Targets: All Enemies");
            else
                info.AppendLine("Targets: Single Enemy");
            
            if (selectedLimitBreak.IgnoresDefense)
                info.AppendLine("Ignores Defense!");
            
            if (selectedLimitBreak.StopsTime)
                info.AppendLine($"Stops time for {selectedLimitBreak.TimeStopDuration} turn(s)!");
            
            info.AppendLine();
            info.Append(selectedLimitBreak.Description);
            
            limitBreakInfoLabel.Text = info.ToString();
        }
        
        private void OnUseLimitBreakPressed()
        {
            var currentActor = battleManager.CurrentActor;
            if (!currentActor.IsLimitBreakReady)
                return;
            
            if (selectedLimitBreak == null)
            {
                GD.PrintErr("No limit break selected!");
                return;
            }
            
            limitBreakPanel.Hide();
            
            var targets = selectedLimitBreak.HitsAllEnemies 
                ? battleManager.GetLivingEnemies().ToArray()
                : new[] { battleManager.GetLivingEnemies().FirstOrDefault() };
            
            if (targets.Length == 0 || targets[0] == null)
            {
                GD.Print("No valid targets!");
                actionMenuPanel.Show();
                return;
            }
            
            var action = new BattleAction(currentActor, BattleActionType.LimitBreak)
                .WithTargets(targets);
            
            // Trigger visual effects
            ShakeScreen(15.0f, 0.8f);
            TriggerCriticalParticles(new Vector2(640, 360));
            
            battleManager.ExecuteAction(action);
            selectedLimitBreak = null;
        }
        
        private void OnBackFromLimitBreakPressed()
        {
            limitBreakPanel.Hide();
            selectedLimitBreak = null;
            actionMenuPanel.Show();
        }
        
        #endregion
        
        #region Showtime Methods
        
        private void ShowShowtimeMenu()
        {
            showtimePanel.Show();
            PopulateShowtimeList();
        }
        
        private void PopulateShowtimeList()
        {
            // Clear existing buttons
            foreach (var child in showtimeList.GetChildren())
            {
                child.QueueFree();
            }
            
            var showtimes = battleManager.GetAvailableShowtimes();
            
            if (showtimes.Count == 0)
            {
                var noShowtimeLabel = new Label();
                noShowtimeLabel.Text = "No Showtimes available!";
                noShowtimeLabel.HorizontalAlignment = HorizontalAlignment.Center;
                showtimeList.AddChild(noShowtimeLabel);
                
                useShowtimeButton.Disabled = true;
                showtimeInfoLabel.Text = "";
                return;
            }
            
            // Auto-select first one
            selectedShowtime = showtimes[0];
            
            foreach (var showtime in showtimes)
            {
                var button = new Button();
                button.Text = $"★ {showtime.AttackName}";
                button.CustomMinimumSize = new Vector2(540, 50);
                button.AddThemeFontSizeOverride("font_size", 20);
                
                // Highlight selected
                if (showtime == selectedShowtime)
                {
                    button.Modulate = Colors.Orange;
                }
                
                button.Pressed += () => OnShowtimeSelected(showtime);
                showtimeList.AddChild(button);
                
                // Add participant info
                var participants = new Label();
                participants.Text = $"   {showtime.Character1Id} + {showtime.Character2Id}";
                participants.AddThemeFontSizeOverride("font_size", 14);
                participants.Modulate = Colors.Gray;
                showtimeList.AddChild(participants);
            }
            
            UpdateShowtimeInfo();
            useShowtimeButton.Disabled = false;
        }
        
        private void OnShowtimeSelected(ShowtimeAttackData showtime)
        {
            selectedShowtime = showtime;
            
            // Update button highlights
            foreach (var child in showtimeList.GetChildren())
            {
                if (child is Button btn)
                {
                    btn.Modulate = Colors.White;
                }
            }
            
            // Highlight selected
            foreach (var child in showtimeList.GetChildren())
            {
                if (child is Button btn && btn.Text.Contains(showtime.AttackName))
                {
                    btn.Modulate = Colors.Orange;
                }
            }
            
            UpdateShowtimeInfo();
        }
        
        private void UpdateShowtimeInfo()
        {
            if (selectedShowtime == null)
            {
                showtimeInfoLabel.Text = "";
                return;
            }
            
            var info = new System.Text.StringBuilder();
            info.AppendLine($"Power: {selectedShowtime.BasePower} x{selectedShowtime.DamageMultiplier}");
            info.AppendLine($"Element: {selectedShowtime.Element}");
            info.AppendLine($"Crit Chance: +{selectedShowtime.CriticalChance}%");
            
            if (selectedShowtime.HitsAllEnemies)
                info.AppendLine("Targets: All Enemies");
            else
                info.AppendLine("Targets: Single Enemy");
            
            if (selectedShowtime.IgnoresDefense)
                info.AppendLine("Ignores Defense!");
            
            info.AppendLine();
            info.Append($"\"{selectedShowtime.FlavorText}\"");
            
            showtimeInfoLabel.Text = info.ToString();
        }
        
        private void OnUseShowtimePressed()
        {
            if (selectedShowtime == null)
                return;
            
            showtimePanel.Hide();
            
            // Trigger visual effects
            ShakeScreen(12.0f, 0.6f);
            TriggerCriticalParticles(new Vector2(640, 360));
            
            battleManager.ExecuteShowtime(selectedShowtime);
            selectedShowtime = null;
        }
        
        private void OnBackFromShowtimePressed()
        {
            showtimePanel.Hide();
            selectedShowtime = null;
            actionMenuPanel.Show();
        }
        
        #endregion
        
        #region Signal Handlers
        
        private void OnAllOutAttackReady()
        {
            GD.Print("All-Out Attack is ready!");
            ShakeScreen(8.0f, 0.4f);
        }
        
        private void OnTechnicalDamage(string attackerName, string targetName, string comboType)
        {
            var techLabel = new Label();
            techLabel.Text = $"⚡ TECHNICAL!\n{comboType}";
            techLabel.AddThemeFontSizeOverride("font_size", 48);
            techLabel.Modulate = Colors.Purple;
            techLabel.Position = new Vector2(450, 300);
            techLabel.ZIndex = 180;
            techLabel.HorizontalAlignment = HorizontalAlignment.Center;
            
            techLabel.AddThemeColorOverride("font_outline_color", Colors.Black);
            techLabel.AddThemeConstantOverride("outline_size", 4);
            
            AddChild(techLabel);
            
            var tween = CreateTween();
            tween.TweenProperty(techLabel, "scale", Vector2.One * 1.4f, 0.3f);
            tween.TweenProperty(techLabel, "modulate:a", 0.0f, 0.5f).SetDelay(1.0f);
            tween.TweenCallback(Callable.From(() => techLabel.QueueFree()));
            
            ShakeScreen(6.0f, 0.3f);
            TriggerHitParticles(new Vector2(640, 360), Colors.Purple);
        }
        
        private void OnShowtimeTriggered(string showtimeName, string char1, string char2)
        {
            var showtimeLabel = new Label();
            showtimeLabel.Text = $"★ SHOWTIME! ★\n{showtimeName}";
            showtimeLabel.AddThemeFontSizeOverride("font_size", 52);
            showtimeLabel.Modulate = Colors.Orange;
            showtimeLabel.Position = new Vector2(400, 280);
            showtimeLabel.ZIndex = 190;
            showtimeLabel.HorizontalAlignment = HorizontalAlignment.Center;
            
            showtimeLabel.AddThemeColorOverride("font_outline_color", Colors.Black);
            showtimeLabel.AddThemeConstantOverride("outline_size", 5);
            
            AddChild(showtimeLabel);
            
            var tween = CreateTween();
            tween.TweenProperty(showtimeLabel, "scale", Vector2.One * 1.3f, 0.4f);
            tween.TweenProperty(showtimeLabel, "modulate:a", 0.0f, 0.6f).SetDelay(1.5f);
            tween.TweenCallback(Callable.From(() => showtimeLabel.QueueFree()));
        }
        
        private void OnLimitBreakReady(string characterName)
        {
            var readyLabel = new Label();
            readyLabel.Text = $"⚡ {characterName}\nLIMIT BREAK READY!";
            readyLabel.AddThemeFontSizeOverride("font_size", 42);
            readyLabel.Modulate = Colors.Yellow;
            readyLabel.Position = new Vector2(420, 320);
            readyLabel.ZIndex = 185;
            readyLabel.HorizontalAlignment = HorizontalAlignment.Center;
            
            readyLabel.AddThemeColorOverride("font_outline_color", Colors.Black);
            readyLabel.AddThemeConstantOverride("outline_size", 4);
            
            AddChild(readyLabel);
            
            var tween = CreateTween();
            tween.SetParallel(true);
            tween.TweenProperty(readyLabel, "scale", Vector2.One * 1.2f, 0.3f);
            tween.TweenProperty(readyLabel, "modulate:a", 1.0f, 0.3f);
            tween.Chain().TweenProperty(readyLabel, "modulate:a", 0.0f, 0.5f).SetDelay(1.2f);
            tween.TweenCallback(Callable.From(() => readyLabel.QueueFree()));
            
            UpdateActionMenuButtons();
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
            
            ShakeScreen(4.0f * passLevel, 0.2f);
            TriggerHitParticles(new Vector2(640, 360), effectLabel.Modulate);
        }
        
        #endregion
        
        #region Visual Effects (Screen Shake & Particles)
        
        private void ShakeScreen(float intensity, float duration)
        {
            var camera = GetViewport().GetCamera2D();
            if (camera == null) return;
            
            var originalOffset = camera.Offset;
            var tween = CreateTween();
            
            int shakes = Mathf.RoundToInt(duration * 30); // 30 shakes per second
            
            for (int i = 0; i < shakes; i++)
            {
                var randomOffset = new Vector2(
                    (float)GD.RandRange(-intensity, intensity),
                    (float)GD.RandRange(-intensity, intensity)
                );
                
                tween.TweenProperty(camera, "offset", randomOffset, duration / shakes);
            }
            
            tween.TweenProperty(camera, "offset", originalOffset, 0.1f);
        }
        
        private void TriggerHitParticles(Vector2 position, Color color)
        {
            hitParticles.Position = position;
            hitParticles.Color = color;
            hitParticles.Emitting = true;
        }
        
        private void TriggerCriticalParticles(Vector2 position)
        {
            criticalParticles.Position = position;
            criticalParticles.Emitting = true;
        }
        
        #endregion
        
        #region Skill Targeting Logic
        
        private void HandleSkillTargeting(SkillData skill)
        {
            switch (skill.Target)
            {
                case SkillTarget.AllEnemies:
                    ExecuteSkillAction(battleManager.GetLivingEnemies().ToArray());
                    break;
                    
                case SkillTarget.AllAllies:
                    ExecuteSkillAction(battleManager.GetLivingAllies().ToArray());
                    break;
                    
                case SkillTarget.Everyone:
                    var everyone = new System.Collections.Generic.List<BattleMember>();
                    everyone.AddRange(battleManager.GetPlayerParty().Where(p => p.Stats.IsAlive));
                    everyone.AddRange(battleManager.GetEnemyParty().Where(e => e.Stats.IsAlive));
                    ExecuteSkillAction(everyone.ToArray());
                    break;
                    
                case SkillTarget.Self:
                    ExecuteSkillAction(new[] { battleManager.CurrentActor });
                    break;
                    
                case SkillTarget.DeadAlly:
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
                        waitingForLimitBreakTarget = false;
                    }
                    break;
                    
                case SkillTarget.SingleEnemy:
                case SkillTarget.RandomEnemy:
                    var enemies = battleManager.GetLivingEnemies();
                    
                    if (enemies.Count == 0)
                    {
                        GD.Print("No valid enemy targets!");
                        actionMenuPanel.Show();
                        selectedSkill = null;
                    }
                    else
                    {
                        targetSelector.ShowSelection(enemies);
                        waitingForTarget = true;
                        selectingBatonPassTarget = false;
                        waitingForItemTarget = false;
                        waitingForLimitBreakTarget = false;
                    }
                    break;
                    
                case SkillTarget.SingleAlly:
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
                        waitingForLimitBreakTarget = false;
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
                waitingForLimitBreakTarget = false;
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
        
        #region Helper Methods
        
        /// <summary>
        /// Get available limit breaks for a character - delegates to BattleManager
        /// </summary>
        private System.Collections.Generic.List<LimitBreakData> GetAvailableLimitBreaks(BattleMember member)
        {
            // Delegate to BattleManager which has access to internal systems
            return battleManager.GetAvailableLimitBreaks(member);
        }
        
        #endregion
    }
}