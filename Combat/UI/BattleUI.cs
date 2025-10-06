// ============================================================================
// FILE: Combat/UI/BattleUI.cs (WITH BATON PASS BUTTON)
// ============================================================================
using Godot;
using System.Linq;
using EchoesAcrossTime.Combat;

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
        private Button guardButton;
        private Button batonPassButton;  // NEW!
        
        // State tracking
        private SkillData selectedSkill;
        private bool waitingForTarget;
        private bool waitingForSkillMenu;
        private bool selectingBatonPassTarget;  // NEW!
        
        public override void _Ready()
        {
            Layer = 100;
            
            battleManager = GetNode<BattleManager>("/root/BattleScene/BattleManager");
            
            CreateAllUI();
            
            if (battleManager != null)
            {
                battleManager.TurnStarted += OnTurnStarted;
                battleManager.BattleEnded += OnBattleEnded;
                battleManager.BatonPassExecuted += OnBatonPassExecuted;  // NEW!
                battleManager.OneMoreTriggered += OnOneMoreTriggered;    // NEW!
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
            
            CreateActionMenu();
        }
        
        private void CreateActionMenu()
        {
            actionMenuPanel = new Panel();
            actionMenuPanel.Position = new Vector2(900, 500);
            actionMenuPanel.CustomMinimumSize = new Vector2(280, 250);
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
            
            guardButton = new Button();
            guardButton.Text = "Guard";
            guardButton.CustomMinimumSize = new Vector2(250, 40);
            guardButton.Pressed += OnGuardPressed;
            vbox.AddChild(guardButton);
            
            // NEW: Baton Pass Button
            batonPassButton = new Button();
            batonPassButton.Text = "Baton Pass";
            batonPassButton.CustomMinimumSize = new Vector2(250, 40);
            batonPassButton.Pressed += OnBatonPassPressed;
            batonPassButton.Disabled = true;  // Disabled by default
            vbox.AddChild(batonPassButton);
            
            actionMenuPanel.Hide();
        }
        
        private void OnTurnStarted(string characterName)
        {
            var currentActor = battleManager.CurrentActor;
            if (currentActor != null && currentActor.IsPlayerControlled)
            {
                actionMenuPanel.Show();
                UpdateBatonPassButton();  // NEW!
            }
            else
            {
                actionMenuPanel.Hide();
            }
        }
        
        // NEW: Update Baton Pass button availability
        private void UpdateBatonPassButton()
        {
            bool canBatonPass = battleManager.CanBatonPass();
            batonPassButton.Disabled = !canBatonPass;
            
            if (canBatonPass)
            {
                // Get the pass level for display
                var currentActor = battleManager.CurrentActor;
                int passLevel = currentActor.BatonPassData.PassCount + 1;
                float multiplier = 1.0f + (passLevel * 0.5f);
                
                // Show power level in button text
                batonPassButton.Text = $"Baton Pass (x{multiplier:F1})";
                
                // Make it visually stand out
                batonPassButton.Modulate = Colors.Orange;
            }
            else
            {
                batonPassButton.Text = "Baton Pass";
                batonPassButton.Modulate = Colors.White;
            }
        }
        
        // NEW: Called when One More is triggered
        private void OnOneMoreTriggered(string characterName)
        {
            UpdateBatonPassButton();
            
            // Show a banner notification
            ShowOneMoreBanner();
        }
        
        // NEW: Show "ONE MORE!" banner
        private void ShowOneMoreBanner()
        {
            var banner = new Label();
            banner.Text = "⭐ ONE MORE! ⭐";
            banner.AddThemeFontSizeOverride("font_size", 64);
            banner.Modulate = Colors.Yellow;
            banner.Position = new Vector2(400, 250);
            banner.ZIndex = 200;
            
            // Add outline for visibility
            banner.AddThemeColorOverride("font_outline_color", Colors.Black);
            banner.AddThemeConstantOverride("outline_size", 5);
            
            AddChild(banner);
            
            // Animate
            var tween = CreateTween();
            tween.SetParallel(true);
            
            // Scale punch
            tween.TweenProperty(banner, "scale", Vector2.One * 1.5f, 0.2f);
            tween.TweenProperty(banner, "scale", Vector2.One, 0.2f).SetDelay(0.2f);
            
            // Fade out
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
            var enemies = battleManager.GetLivingEnemies();
            targetSelector.ShowSelection(enemies);
            waitingForTarget = true;
            selectingBatonPassTarget = false;
        }
        
        private void OnSkillsPressed()
        {
            actionMenuPanel.Hide();
            skillMenu.ShowMenu(battleManager.CurrentActor);
            waitingForSkillMenu = true;
        }
        
        private void OnGuardPressed()
        {
            var action = new BattleAction(battleManager.CurrentActor, BattleActionType.Guard);
            battleManager.ExecuteAction(action);
        }
        
        // NEW: Baton Pass button handler
        private void OnBatonPassPressed()
        {
            if (!battleManager.CanBatonPass())
            {
                GD.Print("Cannot Baton Pass right now!");
                return;
            }
            
            actionMenuPanel.Hide();
            
            // Get valid targets (party members who haven't acted yet)
            var validTargets = battleManager.GetBatonPassTargets();
            
            if (validTargets.Count == 0)
            {
                GD.Print("No valid Baton Pass targets!");
                actionMenuPanel.Show();
                return;
            }
            
            // Show target selection with only valid party members
            targetSelector.ShowSelection(validTargets);
            selectingBatonPassTarget = true;
            waitingForTarget = true;
            
            GD.Print($"Select Baton Pass target from {validTargets.Count} party members");
        }
        
        // NEW: Called when Baton Pass is executed
        private void OnBatonPassExecuted(string fromCharacter, string toCharacter, int passLevel)
        {
            // Show Baton Pass visual effect
            ShowBatonPassEffect(fromCharacter, toCharacter, passLevel);
        }
        
        // NEW: Visual effect for Baton Pass
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
            
            // Add outline
            effectLabel.AddThemeColorOverride("font_outline_color", Colors.Black);
            effectLabel.AddThemeConstantOverride("outline_size", 4);
            
            AddChild(effectLabel);
            
            // Animate
            var tween = CreateTween();
            tween.SetParallel(true);
            tween.TweenProperty(effectLabel, "scale", Vector2.One * 1.3f, 0.3f);
            tween.TweenProperty(effectLabel, "scale", Vector2.One, 0.3f).SetDelay(0.3f);
            tween.Chain().TweenProperty(effectLabel, "modulate:a", 0.0f, 0.5f).SetDelay(0.5f);
            tween.TweenCallback(Callable.From(() => effectLabel.QueueFree()));
        }
        
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
                    var validTargets = battleManager.GetLivingEnemies();
                    targetSelector.ShowSelection(validTargets);
                    waitingForTarget = true;
                    selectingBatonPassTarget = false;
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
                        // Execute Baton Pass
                        HandleBatonPassTarget(target);
                    }
                    else
                    {
                        // Execute normal action
                        HandleTargetSelected(target);
                    }
                }
                else
                {
                    actionMenuPanel.Show();
                }
                
                selectingBatonPassTarget = false;
            }
        }
        
        // NEW: Handle Baton Pass target selection
        private void HandleBatonPassTarget(BattleMember target)
        {
            // Execute the baton pass
            bool success = battleManager.ExecuteBatonPass(target);
            
            if (success)
            {
                GD.Print($"Baton Pass executed to {target.Stats.CharacterName}!");
                // Action menu will be shown again when the new actor's turn starts
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
    }
}