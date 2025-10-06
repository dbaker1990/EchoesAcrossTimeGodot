using Godot;
using System;

namespace EchoesAcrossTime.Shops
{
    /// <summary>
    /// Attach to NPCs or objects to make them open a shop
    /// Can be triggered by interaction or automatically
    /// </summary>
    public partial class ShopTrigger : Node
    {
        [ExportGroup("Shop Configuration")]
        [Export] public ShopData ShopToOpen { get; set; }
        [Export] public string ShopId { get; set; } = "";
        
        [ExportGroup("Trigger Settings")]
        [Export] public bool OpenOnInteract { get; set; } = true;
        [Export] public bool OpenOnEnter { get; set; } = false;
        [Export] public string InteractionKey { get; set; } = "ui_accept";
        
        [ExportGroup("Optional")]
        [Export] public string ShopkeeperDialogue { get; set; } = "Welcome to my shop!";
        [Export] public bool ShowDialogueBeforeOpening { get; set; } = true;
        
        private bool playerInRange = false;
        private Area2D triggerArea;
        
        public override void _Ready()
        {
            // Try to find Area2D for interaction
            triggerArea = GetNodeOrNull<Area2D>("Area2D");
            if (triggerArea != null)
            {
                triggerArea.BodyEntered += OnBodyEntered;
                triggerArea.BodyExited += OnBodyExited;
            }
            
            // If ShopData is directly assigned, register it
            if (ShopToOpen != null && ShopManager.Instance != null)
            {
                ShopManager.Instance.RegisterShop(ShopToOpen);
                ShopId = ShopToOpen.ShopId;
            }
        }
        
        public override void _Process(double delta)
        {
            if (!OpenOnInteract) return;
            if (!playerInRange) return;
            
            // Check for interaction key
            if (Input.IsActionJustPressed(InteractionKey))
            {
                OpenShop();
            }
        }
        
        private void OnBodyEntered(Node2D body)
        {
            if (body.IsInGroup("Player"))
            {
                playerInRange = true;
                
                if (OpenOnEnter)
                {
                    OpenShop();
                }
            }
        }
        
        private void OnBodyExited(Node2D body)
        {
            if (body.IsInGroup("Player"))
            {
                playerInRange = false;
            }
        }
        
        /// <summary>
        /// Open the shop
        /// </summary>
        public void OpenShop()
        {
            if (ShopManager.Instance == null)
            {
                GD.PushError("[ShopTrigger] ShopManager not found!");
                return;
            }
            
            // Show dialogue first if enabled
            if (ShowDialogueBeforeOpening && !string.IsNullOrEmpty(ShopkeeperDialogue))
            {
                ShowDialogue(ShopkeeperDialogue);
            }
            
            // Open the shop
            if (ShopToOpen != null)
            {
                ShopManager.Instance.OpenShop(ShopToOpen);
            }
            else if (!string.IsNullOrEmpty(ShopId))
            {
                ShopManager.Instance.OpenShop(ShopId);
            }
            else
            {
                GD.PushError("[ShopTrigger] No shop assigned!");
            }
        }
        
        /// <summary>
        /// Show shopkeeper dialogue
        /// TODO: Integrate with your dialogue system
        /// </summary>
        private void ShowDialogue(string text)
        {
            GD.Print($"[Shopkeeper] {text}");
            
            // TODO: Connect to your dialogue system
            // Example:
            // var dialogueManager = GetNode<DialogueManager>("/root/DialogueManager");
            // dialogueManager?.ShowDialogue(text);
        }
        
        public override void _ExitTree()
        {
            if (triggerArea != null)
            {
                triggerArea.BodyEntered -= OnBodyEntered;
                triggerArea.BodyExited -= OnBodyExited;
            }
        }
    }
}