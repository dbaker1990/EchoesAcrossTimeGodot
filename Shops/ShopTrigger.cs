// Shops/ShopTrigger.cs - Updated with MessageBox integration
using Godot;
using System.Threading.Tasks;
using EchoesAcrossTime.Events;
using EchoesAcrossTime.UI;

namespace EchoesAcrossTime.Shops
{
    /// <summary>
    /// Triggers shop opening when player interacts
    /// </summary>
    public partial class ShopTrigger : Area2D
    {
        [Export] public ShopData ShopToOpen { get; set; }
        [Export] public string ShopId { get; set; } = "";
        [Export] public bool ShowDialogueBeforeOpening { get; set; } = true;
        [Export(PropertyHint.MultilineText)] public string ShopkeeperDialogue { get; set; } = "Welcome to my shop!";
        [Export] public string ShopkeeperName { get; set; } = "Shopkeeper";
        [Export] public string ShopkeeperPortraitPath { get; set; } = "";
        [Export] public NodePath MessageBoxPath { get; set; }
        
        private MessageBox messageBox;
        private bool playerInRange = false;
        private bool isInteracting = false;
        private Label interactionPrompt;
        
        public override void _Ready()
        {
            BodyEntered += OnBodyEntered;
            BodyExited += OnBodyExited;
            
            // Get MessageBox reference
            if (MessageBoxPath != null && !MessageBoxPath.IsEmpty)
            {
                messageBox = GetNode<MessageBox>(MessageBoxPath);
            }
            else
            {
                // Try to find it in the scene tree with unique name
                messageBox = GetTree().Root.GetNodeOrNull<MessageBox>("%MessageBox");
            }
            
            if (messageBox == null)
            {
                GD.PrintErr("[ShopTrigger] MessageBox not found! Dialogue will not work.");
            }
            
            CreateInteractionPrompt();
        }
        
        private void CreateInteractionPrompt()
        {
            interactionPrompt = new Label();
            interactionPrompt.Text = "Press E to talk";
            interactionPrompt.Position = new Vector2(-40, -50);
            interactionPrompt.Visible = false;
            AddChild(interactionPrompt);
        }
        
        private void OnBodyEntered(Node2D body)
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
        
        private void OnBodyExited(Node2D body)
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
            if (playerInRange && !isInteracting)
            {
                if (@event.IsActionPressed("interact"))
                {
                    _ = OnInteract(); // Fire and forget
                }
            }
        }
        
        private async Task OnInteract()
        {
            if (isInteracting) return;
            isInteracting = true;
            
            if (ShopToOpen == null && string.IsNullOrEmpty(ShopId))
            {
                GD.PrintErr("[ShopTrigger] No shop assigned!");
                isInteracting = false;
                return;
            }
            
            // Show dialogue first if enabled
            if (ShowDialogueBeforeOpening && !string.IsNullOrEmpty(ShopkeeperDialogue))
            {
                await ShowDialogue(ShopkeeperDialogue);
            }
            
            // Open the shop
            if (ShopToOpen != null)
            {
                ShopManager.Instance?.OpenShop(ShopToOpen);
            }
            else if (!string.IsNullOrEmpty(ShopId))
            {
                ShopManager.Instance?.OpenShop(ShopId);
            }
            else
            {
                GD.PushError("[ShopTrigger] No shop assigned!");
            }
            
            isInteracting = false;
        }
        
        /// <summary>
        /// Show shopkeeper dialogue using MessageBox
        /// </summary>
        private async Task ShowDialogue(string text)
        {
            if (messageBox == null)
            {
                GD.PrintErr("[ShopTrigger] MessageBox not available - showing debug message");
                GD.Print($"[Shopkeeper] {text}");
                return;
            }
            
            // Create DialogueData
            var dialogueData = new DialogueData
            {
                Text = text,
                SpeakerName = ShopkeeperName,
                ShowSpeakerName = !string.IsNullOrEmpty(ShopkeeperName),
                PortraitPath = ShopkeeperPortraitPath,
                ShowPortrait = !string.IsNullOrEmpty(ShopkeeperPortraitPath),
                UseTypewriterEffect = true,
                TextSpeed = 0.05f,
                Position = MessageBoxPosition.Bottom
            };
            
            // Wait for message to be shown and advanced
            var tcs = new TaskCompletionSource<bool>();
            
            void OnMessageAdvanced()
            {
                tcs.TrySetResult(true);
            }
            
            messageBox.MessageAdvanced += OnMessageAdvanced;
            messageBox.ShowMessage(dialogueData);
            
            await tcs.Task;
            
            messageBox.MessageAdvanced -= OnMessageAdvanced;
        }
        
        public override void _ExitTree()
        {
            BodyEntered -= OnBodyEntered;
            BodyExited -= OnBodyExited;
        }
    }
}