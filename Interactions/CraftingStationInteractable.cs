using Godot;
using EchoesAcrossTime.Crafting;
using EchoesAcrossTime.UI;

namespace EchoesAcrossTime.Interactions
{
    /// <summary>
    /// Example interactable crafting station
    /// Attach to an Area2D or similar interactable object in your world
    /// </summary>
    public partial class CraftingStationInteractable : Area2D
    {
        [ExportCategory("Crafting Station")]
        [Export] public CraftingStationType StationType { get; set; } = CraftingStationType.Blacksmith;
        [Export] public string StationName { get; set; } = "Blacksmith";
        [Export] public bool RequiresUnlock { get; set; } = false;
        [Export] public string UnlockQuestId { get; set; } = "";

        [ExportCategory("UI")]
        [Export] public NodePath CraftingUIPath { get; set; }
        [Export] public Label InteractPromptLabel { get; set; }

        private CraftingUI craftingUI;
        private bool isPlayerNearby = false;
        private bool isUnlocked = true;

        public override void _Ready()
        {
            // Get UI reference
            if (!string.IsNullOrEmpty(CraftingUIPath.ToString()))
            {
                craftingUI = GetNode<CraftingUI>(CraftingUIPath);
            }
            else
            {
                // Try to find UI in scene tree
                craftingUI = GetTree().Root.GetNodeOrNull<CraftingUI>("CraftingUI");
            }

            if (craftingUI == null)
            {
                GD.PrintErr($"CraftingStationInteractable: Could not find CraftingUI!");
            }

            // Setup prompt
            if (InteractPromptLabel != null)
            {
                InteractPromptLabel.Visible = false;
            }

            // Connect area signals
            BodyEntered += OnBodyEntered;
            BodyExited += OnBodyExited;

            // Check if unlocked
            if (RequiresUnlock)
            {
                CheckUnlockStatus();
            }

            GD.Print($"CraftingStation '{StationName}' ready");
        }

        public override void _Input(InputEvent @event)
        {
            // Check for interact key (customize to your input system)
            if (@event.IsActionPressed("ui_accept") && isPlayerNearby && isUnlocked)
            {
                OpenCraftingUI();
            }
        }

        private void OnBodyEntered(Node2D body)
        {
            // Check if it's the player (customize based on your player detection)
            if (body.IsInGroup("player"))
            {
                isPlayerNearby = true;
                ShowInteractPrompt();
            }
        }

        private void OnBodyExited(Node2D body)
        {
            if (body.IsInGroup("player"))
            {
                isPlayerNearby = false;
                HideInteractPrompt();
            }
        }

        /// <summary>
        /// Open the crafting UI at this station
        /// </summary>
        private void OpenCraftingUI()
        {
            if (craftingUI == null)
            {
                GD.PrintErr("Cannot open crafting UI - reference not set!");
                return;
            }

            if (!isUnlocked)
            {
                ShowLockedMessage();
                return;
            }

            GD.Print($"Opening {StationName}...");
            craftingUI.OpenAtStation(StationType);
        }

        /// <summary>
        /// Show interact prompt to player
        /// </summary>
        private void ShowInteractPrompt()
        {
            if (InteractPromptLabel != null)
            {
                if (isUnlocked)
                {
                    InteractPromptLabel.Text = $"[E] Use {StationName}";
                }
                else
                {
                    InteractPromptLabel.Text = $"{StationName} (Locked)";
                }
                InteractPromptLabel.Visible = true;
            }
        }

        /// <summary>
        /// Hide interact prompt
        /// </summary>
        private void HideInteractPrompt()
        {
            if (InteractPromptLabel != null)
            {
                InteractPromptLabel.Visible = false;
            }
        }

        /// <summary>
        /// Check if station is unlocked based on quest completion
        /// </summary>
        private void CheckUnlockStatus()
        {
            if (!RequiresUnlock)
            {
                isUnlocked = true;
                return;
            }

            // Check with your quest system
            // Example:
            // isUnlocked = QuestSystem.Instance.IsQuestCompleted(UnlockQuestId);
            
            // For now, default to locked
            isUnlocked = false;
            
            GD.Print($"{StationName} unlock status: {isUnlocked}");
        }

        /// <summary>
        /// Unlock this station (call from quest completion, etc.)
        /// </summary>
        public void UnlockStation()
        {
            if (!RequiresUnlock) return;

            isUnlocked = true;
            GD.Print($"{StationName} unlocked!");

            // Update prompt if player is nearby
            if (isPlayerNearby)
            {
                ShowInteractPrompt();
            }

            // Optional: Play unlock animation/sound
            PlayUnlockEffect();
        }

        /// <summary>
        /// Show message when station is locked
        /// </summary>
        private void ShowLockedMessage()
        {
            // Implement your message system here
            GD.Print($"This {StationName} is locked!");
            // Example: MessageBox.Show($"Complete the quest '{UnlockQuestId}' to use this {StationName}.");
        }

        /// <summary>
        /// Play unlock effect (override in derived classes for custom effects)
        /// </summary>
        protected virtual void PlayUnlockEffect()
        {
            // Add particles, sound, etc.
            GD.Print($"Playing unlock effect for {StationName}");
        }

        /// <summary>
        /// Get station information
        /// </summary>
        public string GetStationInfo()
        {
            string info = $"{StationName}\n";
            info += $"Type: {StationType}\n";
            info += $"Status: {(isUnlocked ? "Available" : "Locked")}";
            
            if (!isUnlocked && RequiresUnlock)
            {
                info += $"\nRequires: {UnlockQuestId}";
            }

            return info;
        }
    }

    /// <summary>
    /// Example of a specific crafting station with custom behavior
    /// </summary>
    public partial class BlacksmithStation : CraftingStationInteractable
    {
        [Export] public AnimationPlayer AnvilAnimation { get; set; }
        [Export] public AudioStreamPlayer HammerSound { get; set; }

        public override void _Ready()
        {
            StationType = CraftingStationType.Blacksmith;
            StationName = "Blacksmith's Forge";
            base._Ready();
        }

        protected override void PlayUnlockEffect()
        {
            base.PlayUnlockEffect();

            // Play anvil animation
            if (AnvilAnimation != null)
            {
                AnvilAnimation.Play("unlock");
            }

            // Play hammer sound
            if (HammerSound != null)
            {
                HammerSound.Play();
            }
        }
    }

    /// <summary>
    /// Example of an alchemy table station
    /// </summary>
    public partial class AlchemyTableStation : CraftingStationInteractable
    {
        [Export] public Node2D PotionBottles { get; set; }
        [Export] public AnimationPlayer BubbleAnimation { get; set; }

        public override void _Ready()
        {
            StationType = CraftingStationType.AlchemyTable;
            StationName = "Alchemy Table";
            base._Ready();
        }

        protected override void PlayUnlockEffect()
        {
            base.PlayUnlockEffect();

            // Play bubble animation
            if (BubbleAnimation != null)
            {
                BubbleAnimation.Play("bubble");
            }

            // Show bottles appearing
            if (PotionBottles != null)
            {
                PotionBottles.Visible = true;
            }
        }
    }
}