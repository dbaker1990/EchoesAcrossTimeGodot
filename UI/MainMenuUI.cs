using Godot;
using System.Collections.Generic;
using EchoesAcrossTime.Managers;
using EchoesAcrossTime.Items;  // ← ADD THIS LINE

namespace EchoesAcrossTime.UI
{
    public partial class MainMenuUI : Control
    {
        [Export] private GridContainer partyGrid;
        [Export] private Label locationLabel;
        [Export] private Label playtimeLabel;
        [Export] private Label goldLabel;
        [Export] private Button closeButton;
        [Export] private PackedScene partyCharacterPanelScene;
        
        private List<PartyCharacterPanel> characterPanels = new List<PartyCharacterPanel>();
        private bool isOpen = false;
        
        public override void _Ready()
        {
            // Connect close button
            if (closeButton != null)
            {
                closeButton.Pressed += CloseMenu;
            }
            
            // Load party panel scene if not set
            if (partyCharacterPanelScene == null)
            {
                partyCharacterPanelScene = GD.Load<PackedScene>("res://UI/PartyCharacterPanel.tscn");
            }
            
            // Hide by default
            Hide();
            
            // Subscribe to party changes
            if (PartyMenuManager.Instance != null)
            {
                PartyMenuManager.Instance.MainPartyChanged += RefreshPartyDisplay;
            }
        }
        
        public override void _Input(InputEvent @event)
        {
            if (!isOpen) return;
            
            // Close menu with ESC
            if (@event.IsActionPressed("ui_cancel"))
            {
                CloseMenu();
                GetViewport().SetInputAsHandled();
            }
        }
        
        /// <summary>
        /// Open the main menu and display party
        /// </summary>
        public void OpenMenu()
        {
            if (isOpen) return;
            
            isOpen = true;
            Show();
            GetTree().Paused = true;
            
            RefreshPartyDisplay();
            RefreshHeaderInfo();
            RefreshGold();
            
            // Play sound
            Managers.SystemManager.Instance?.PlayOkSE();
        }
        
        /// <summary>
        /// Close the main menu
        /// </summary>
        public void CloseMenu()
        {
            if (!isOpen) return;
            
            isOpen = false;
            Hide();
            GetTree().Paused = false;
            
            // Play sound
            Managers.SystemManager.Instance?.PlayCancelSE();
        }
        
        /// <summary>
        /// Return to main menu from sub-menus (for compatibility with other menu UIs)
        /// </summary>
        public void ReturnToMainMenu()
        {
            // Show this menu again
            Show();
            isOpen = true;
            
            // Refresh display
            RefreshPartyDisplay();
            RefreshHeaderInfo();
            RefreshGold();
        }
        
        /// <summary>
        /// Refresh party member display
        /// </summary>
        private void RefreshPartyDisplay()
        {
            if (partyGrid == null || partyCharacterPanelScene == null) return;
            
            // Clear existing panels
            foreach (var panel in characterPanels)
            {
                panel.QueueFree();
            }
            characterPanels.Clear();
            
            // Get main party
            var mainParty = PartyMenuManager.Instance?.GetMainParty();
            if (mainParty == null || mainParty.Count == 0)
            {
                GD.Print("No party members to display");
                return;
            }
            
            // Create panels for each member (max 4)
            int displayCount = Mathf.Min(mainParty.Count, 4);
            for (int i = 0; i < displayCount; i++)
            {
                var member = mainParty[i];
                var panel = partyCharacterPanelScene.Instantiate<PartyCharacterPanel>();
                
                partyGrid.AddChild(panel);
                panel.Initialize(member.CharacterId, member.Stats);
                characterPanels.Add(panel);
            }
        }
        
        /// <summary>
        /// Refresh header information (location, playtime)
        /// </summary>
        private void RefreshHeaderInfo()
        {
            // Update location
            if (locationLabel != null)
            {
                // Get current location from game manager or scene name
                string location = "Port Capua Torim"; // Replace with actual location system
                locationLabel.Text = location;
            }
            
            // Update playtime
            if (playtimeLabel != null)
            {
                // Get playtime from save system
                float playtime = 0f; // Replace with actual playtime from SaveManager
                int hours = (int)(playtime / 3600);
                int minutes = (int)((playtime % 3600) / 60);
                playtimeLabel.Text = $"{hours:D2}:{minutes:D2}";
            }
        }
        
        /// <summary>
        /// Refresh gold display
        /// </summary>
        private void RefreshGold()
        {
            if (goldLabel != null)
            {
                // Get gold from inventory
                int gold = 0;
                if (InventorySystem.Instance != null)
                {
                    gold = InventorySystem.Instance.GetGold();
                }
                goldLabel.Text = $"Gold: {gold}G";
            }
        }
        
        public override void _ExitTree()
        {
            if (PartyMenuManager.Instance != null)
            {
                PartyMenuManager.Instance.MainPartyChanged -= RefreshPartyDisplay;
            }
            base._ExitTree();
        }
    }
}