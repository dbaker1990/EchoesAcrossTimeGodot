// Inn/InnKeeperNPC.cs
using Godot;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using EchoesAcrossTime.UI;
using EchoesAcrossTime.Events;
using EchoesAcrossTime.Managers;

namespace EchoesAcrossTime.Inn
{
    /// <summary>
    /// NPC that provides inn services using existing MessageBox/ChoiceBox
    /// </summary>
    public partial class InnKeeperNPC : Area2D
    {
        [Export] public InnKeeperData InnData { get; set; }
        [Export] public NodePath MessageBoxPath { get; set; }
        [Export] public NodePath ChoiceBoxPath { get; set; }
        [Export] public NodePath ScreenEffectsPath { get; set; }
        
        private MessageBox messageBox;
        private ChoiceBox choiceBox;
        private ScreenEffects screenEffects;
        private bool playerInRange = false;
        private bool isInteracting = false;
        
        private Label interactionPrompt;
        
        public override void _Ready()
        {
            // Get MessageBox reference
            if (MessageBoxPath != null && !MessageBoxPath.IsEmpty)
            {
                messageBox = GetNode<MessageBox>(MessageBoxPath);
            }
            else
            {
                messageBox = GetTree().Root.GetNode<MessageBox>("%MessageBox");
            }
            
            // Get ChoiceBox reference
            if (ChoiceBoxPath != null && !ChoiceBoxPath.IsEmpty)
            {
                choiceBox = GetNode<ChoiceBox>(ChoiceBoxPath);
            }
            else
            {
                choiceBox = GetTree().Root.GetNode<ChoiceBox>("%ChoiceBox");
            }
            
            // Get ScreenEffects reference
            if (ScreenEffectsPath != null && !ScreenEffectsPath.IsEmpty)
            {
                screenEffects = GetNode<ScreenEffects>(ScreenEffectsPath);
            }
            else
            {
                screenEffects = GetTree().Root.GetNode<ScreenEffects>("%ScreenEffects");
            }
            
            // Create interaction prompt
            CreateInteractionPrompt();
            
            // Connect signals
            BodyEntered += OnPlayerEntered;
            BodyExited += OnPlayerExited;
            
            if (InnData == null)
            {
                GD.PrintErr($"[InnKeeperNPC] No InnKeeperData assigned!");
            }
        }
        
        public override void _ExitTree()
        {
            BodyEntered -= OnPlayerEntered;
            BodyExited -= OnPlayerExited;
            base._ExitTree();
        }
        
        private void CreateInteractionPrompt()
        {
            interactionPrompt = new Label();
            interactionPrompt.Text = "[E] Talk";
            interactionPrompt.Position = new Vector2(-30, -60);
            interactionPrompt.AddThemeColorOverride("font_color", Colors.White);
            interactionPrompt.AddThemeColorOverride("font_outline_color", Colors.Black);
            interactionPrompt.AddThemeConstantOverride("outline_size", 2);
            interactionPrompt.Visible = false;
            AddChild(interactionPrompt);
        }
        
        private void OnPlayerEntered(Node2D body)
        {
            if (body.IsInGroup("player"))
            {
                playerInRange = true;
                if (interactionPrompt != null)
                {
                    interactionPrompt.Visible = true;
                }
            }
        }
        
        private void OnPlayerExited(Node2D body)
        {
            if (body.IsInGroup("player"))
            {
                playerInRange = false;
                if (interactionPrompt != null)
                {
                    interactionPrompt.Visible = false;
                }
            }
        }
        
        public override void _Input(InputEvent @event)
        {
            if (playerInRange && !isInteracting && @event.IsActionPressed("ui_accept"))
            {
                _ = StartInnInteraction();
                GetViewport().SetInputAsHandled();
            }
        }
        
        /// <summary>
        /// Main inn interaction flow
        /// </summary>
        private async Task StartInnInteraction()
        {
            if (InnData == null || messageBox == null || choiceBox == null)
            {
                GD.PrintErr("[InnKeeperNPC] Missing InnData, MessageBox, or ChoiceBox!");
                return;
            }
            
            isInteracting = true;
            
            // Hide interaction prompt
            if (interactionPrompt != null)
            {
                interactionPrompt.Visible = false;
            }
            
            // Show welcome message
            var welcomeDialogue = new DialogueData
            {
                Text = InnData.GetFormattedWelcome(),
                SpeakerName = InnData.InnName,
                ShowSpeakerName = true,
                UseTypewriterEffect = true,
                TextSpeed = 0.05f,
                Position = InnData.DialoguePosition
            };
            
            await ShowMessageAsync(welcomeDialogue);
            
            // Show choice: Stay or Leave
            var choices = new List<string> { "Stay", "Leave" };
            int choice = await ShowChoicesAsync(choices);
            
            if (choice == 1) // Leave
            {
                // Player chose to leave
                isInteracting = false;
                return;
            }
            
            // Player chose to stay - check if they have enough money
            int currentGold = GetCurrentGold();
            
            if (currentGold < InnData.RestCost)
            {
                // Not enough money
                var noMoneyDialogue = new DialogueData
                {
                    Text = InnData.NotEnoughMoneyMessage,
                    SpeakerName = InnData.InnName,
                    ShowSpeakerName = true,
                    UseTypewriterEffect = true,
                    TextSpeed = 0.05f,
                    Position = InnData.DialoguePosition
                };
                
                await ShowMessageAsync(noMoneyDialogue);
                isInteracting = false;
                return;
            }
            
            // Has enough money - proceed with rest
            await PerformRest();
            
            isInteracting = false;
        }
        
        /// <summary>
        /// Show message using MessageBox
        /// </summary>
        private async Task ShowMessageAsync(DialogueData dialogue)
        {
            var tcs = new TaskCompletionSource<bool>();
            
            void OnMessageAdvanced()
            {
                tcs.TrySetResult(true);
            }
            
            messageBox.MessageAdvanced += OnMessageAdvanced;
            messageBox.ShowMessage(dialogue);
            
            await tcs.Task;
            
            messageBox.MessageAdvanced -= OnMessageAdvanced;
        }
        
        /// <summary>
        /// Show choices using ChoiceBox
        /// </summary>
        private async Task<int> ShowChoicesAsync(List<string> choices)
        {
            var tcs = new TaskCompletionSource<int>();
            
            void OnChoiceSelected(int index)
            {
                tcs.TrySetResult(index);
            }
            
            choiceBox.ChoiceSelected += OnChoiceSelected;
            choiceBox.ShowChoices(choices, 0);
            
            int result = await tcs.Task;
            
            choiceBox.ChoiceSelected -= OnChoiceSelected;
            
            return result;
        }
        
        /// <summary>
        /// Perform the rest sequence
        /// </summary>
        private async Task PerformRest()
        {
            // Deduct gold
            SpendGold(InnData.RestCost);
            
            // Play rest sound if available
            SystemManager.Instance?.PlayOkSE();
            
            // Fade to black
            if (screenEffects != null)
            {
                await screenEffects.Fade(true, InnData.FadeDuration, Colors.Black);
            }
            else
            {
                await ToSignal(GetTree().CreateTimer(InnData.FadeDuration), SceneTreeTimer.SignalName.Timeout);
            }
            
            // Restore HP and MP for all party members
            RestorePartyHPMP();
            
            // Wait while screen is black
            await ToSignal(GetTree().CreateTimer(InnData.RestDuration), SceneTreeTimer.SignalName.Timeout);
            
            // Fade back in
            if (screenEffects != null)
            {
                await screenEffects.Fade(false, InnData.FadeDuration, Colors.Black);
            }
            else
            {
                await ToSignal(GetTree().CreateTimer(InnData.FadeDuration), SceneTreeTimer.SignalName.Timeout);
            }
            
            // Play wake sound
            SystemManager.Instance?.PlayOkSE();
            
            GD.Print($"[InnKeeper] Rest complete! Party fully restored.");
        }
        
        /// <summary>
        /// Restore HP and MP for all party members (main and sub)
        /// </summary>
        private void RestorePartyHPMP()
        {
            var partyManager = PartyMenuManager.Instance;
            if (partyManager == null)
            {
                GD.PrintErr("[InnKeeperNPC] PartyMenuManager not found!");
                return;
            }
            
            // Restore main party
            var mainParty = partyManager.GetMainParty();
            foreach (var member in mainParty)
            {
                member.Stats.CurrentHP = member.Stats.MaxHP;
                member.Stats.CurrentMP = member.Stats.MaxMP;
                GD.Print($"[InnKeeper] Restored {member.CharacterId}: HP {member.Stats.MaxHP}, MP {member.Stats.MaxMP}");
            }
            
            // Restore sub party
            var subParty = partyManager.GetSubParty();
            foreach (var member in subParty)
            {
                member.Stats.CurrentHP = member.Stats.MaxHP;
                member.Stats.CurrentMP = member.Stats.MaxMP;
                GD.Print($"[InnKeeper] Restored {member.CharacterId}: HP {member.Stats.MaxHP}, MP {member.Stats.MaxMP}");
            }
        }
        
        /// <summary>
        /// Get current gold amount from player
        /// </summary>
        private int GetCurrentGold()
        {
            // Try to get from ShopManager if it exists
            var shopManager = GetNodeOrNull("/root/ShopManager");
            if (shopManager != null && shopManager.HasMethod("GetGold"))
            {
                return (int)shopManager.Call("GetGold");
            }
            
            // Try to get from GameManager or InventorySystem
            var gameManager = GetNodeOrNull("/root/GameManager");
            if (gameManager != null && gameManager.HasMethod("GetGold"))
            {
                return (int)gameManager.Call("GetGold");
            }
            
            // Fallback - try InventorySystem
            var inventorySystem = GetNodeOrNull("/root/InventorySystem");
            if (inventorySystem != null && inventorySystem.HasMethod("GetGold"))
            {
                return (int)inventorySystem.Call("GetGold");
            }
            
            GD.PrintErr("[InnKeeperNPC] Could not find gold getter! Returning 0.");
            return 0;
        }
        
        /// <summary>
        /// Spend gold
        /// </summary>
        private void SpendGold(int amount)
        {
            // Try ShopManager first
            var shopManager = GetNodeOrNull("/root/ShopManager");
            if (shopManager != null && shopManager.HasMethod("SpendGold"))
            {
                shopManager.Call("SpendGold", amount);
                GD.Print($"[InnKeeper] Spent {amount} gold");
                return;
            }
            
            // Try GameManager
            var gameManager = GetNodeOrNull("/root/GameManager");
            if (gameManager != null && gameManager.HasMethod("SpendGold"))
            {
                gameManager.Call("SpendGold", amount);
                GD.Print($"[InnKeeper] Spent {amount} gold");
                return;
            }
            
            // Try InventorySystem
            var inventorySystem = GetNodeOrNull("/root/InventorySystem");
            if (inventorySystem != null && inventorySystem.HasMethod("RemoveGold"))
            {
                inventorySystem.Call("RemoveGold", amount);
                GD.Print($"[InnKeeper] Spent {amount} gold");
                return;
            }
            
            GD.PrintErr($"[InnKeeperNPC] Could not find method to spend {amount} gold!");
        }
    }
}