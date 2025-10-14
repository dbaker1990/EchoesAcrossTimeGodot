using Godot;
using System;

namespace EchoesAcrossTime.UI
{
    /// <summary>
    /// Load Game UI for the Title Screen - displays save slots and loads selected save
    /// </summary>
    public partial class LoadGameTitleUI : Control
    {
        #region Exports - Connect these in the Godot editor
        [Export] private int maxSaveSlots = 10; // Configure how many slots you want
        [Export] private VBoxContainer slotsContainer; // Container for all slot buttons
        [Export] private Panel detailsPanel; // Panel showing save details
        
        // Save Details Labels
        [Export] private Label slotNumberLabel;
        [Export] private Label locationLabel;
        [Export] private Label playtimeLabel;
        [Export] private Label saveDateLabel;
        [Export] private Label levelLabel;
        [Export] private TextureRect screenshotPreview;
        
        // Action Buttons
        [Export] private Button loadButton;
        [Export] private Button backButton;
        #endregion
        
        #region State
        private int selectedSlot = -1;
        private Button[] slotButtons;
        #endregion
        
        /// <summary>
        /// Called when the node enters the scene tree
        /// </summary>
        public override void _Ready()
        {
            // Connect button signals
            if (loadButton != null)
                loadButton.Pressed += OnLoadPressed;
            
            if (backButton != null)
                backButton.Pressed += OnBackPressed;
            
            // Initially hide this panel
            Hide();
        }
        
        /// <summary>
        /// Opens the Load Game menu and refreshes all save slots
        /// </summary>
        public void OpenMenu()
        {
            Show();
            CreateSaveSlotButtons();
            RefreshAllSlots();
            
            // Hide details panel until a slot is selected
            if (detailsPanel != null)
                detailsPanel.Hide();
            
            // Disable load button until a valid slot is selected
            if (loadButton != null)
                loadButton.Disabled = true;
        }
        
        /// <summary>
        /// Creates buttons for each save slot
        /// </summary>
        private void CreateSaveSlotButtons()
        {
            if (slotsContainer == null)
            {
                GD.PrintErr("Slots Container is not assigned!");
                return;
            }
            
            // Clear existing buttons
            foreach (Node child in slotsContainer.GetChildren())
            {
                child.QueueFree();
            }
            
            // Create new buttons for each slot
            slotButtons = new Button[maxSaveSlots];
            
            for (int i = 0; i < maxSaveSlots; i++)
            {
                Button slotButton = new Button();
                int slotIndex = i; // Capture for lambda
                
                // Style the button
                slotButton.CustomMinimumSize = new Vector2(400, 50);
                slotButton.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
                
                // Connect button press
                slotButton.Pressed += () => OnSlotSelected(slotIndex);
                
                slotsContainer.AddChild(slotButton);
                slotButtons[i] = slotButton;
            }
        }
        
        /// <summary>
        /// Refreshes all save slot displays
        /// </summary>
        private void RefreshAllSlots()
        {
            if (SaveSystem.Instance == null)
            {
                GD.PrintErr("SaveSystem is not available!");
                return;
            }
            
            for (int i = 0; i < maxSaveSlots; i++)
            {
                UpdateSlotButton(i);
            }
        }
        
        /// <summary>
        /// Updates a single slot button's display
        /// </summary>
        private void UpdateSlotButton(int slotIndex)
        {
            if (slotButtons == null || slotIndex >= slotButtons.Length)
                return;
            
            Button button = slotButtons[slotIndex];
            
            // Check if this slot has a save file
            if (SaveSystem.Instance.SaveExists(slotIndex))
            {
                // Get save info
                var saveInfo = SaveSystem.Instance.GetSaveInfo(slotIndex);
                
                if (saveInfo != null)
                {
                    // Display save information
                    string displayText = $"Slot {slotIndex + 1} - {saveInfo.MapName}\n" +
                                       $"Lv.{saveInfo.PartyLevel} | {saveInfo.GetPlayTimeFormatted()}";
                    button.Text = displayText;
                    button.Disabled = false;
                }
                else
                {
                    // Corrupted save
                    button.Text = $"Slot {slotIndex + 1} - (Corrupted)";
                    button.Disabled = true;
                }
            }
            else
            {
                // Empty slot
                button.Text = $"Slot {slotIndex + 1} - (Empty)";
                button.Disabled = true;
            }
        }
        
        /// <summary>
        /// Called when a save slot is selected
        /// </summary>
        private void OnSlotSelected(int slotIndex)
        {
            selectedSlot = slotIndex;
            
            // Play cursor sound
            Managers.SystemManager.Instance?.PlayCursorSE();
            
            // Highlight selected button
            for (int i = 0; i < slotButtons.Length; i++)
            {
                if (slotButtons[i] != null)
                {
                    // You can add custom styling here
                    slotButtons[i].Modulate = i == slotIndex ? new Color(1, 1, 0.7f) : Colors.White;
                }
            }
            
            // Show details panel
            if (detailsPanel != null)
                detailsPanel.Show();
            
            // Load and display save details
            DisplaySaveDetails(slotIndex);
            
            // Enable load button if valid save
            if (loadButton != null)
            {
                loadButton.Disabled = !SaveSystem.Instance.SaveExists(slotIndex);
            }
        }
        
        /// <summary>
        /// Displays detailed information about the selected save
        /// </summary>
        private void DisplaySaveDetails(int slotIndex)
        {
            var saveInfo = SaveSystem.Instance.GetSaveInfo(slotIndex);
            
            if (saveInfo == null)
            {
                // Show empty details
                if (slotNumberLabel != null) slotNumberLabel.Text = $"Slot {slotIndex + 1}";
                if (locationLabel != null) locationLabel.Text = "Location: ---";
                if (playtimeLabel != null) playtimeLabel.Text = "Playtime: ---";
                if (saveDateLabel != null) saveDateLabel.Text = "Saved: ---";
                if (levelLabel != null) levelLabel.Text = "Level: ---";
                if (screenshotPreview != null) screenshotPreview.Texture = null;
                return;
            }
            
            // Display save information
            if (slotNumberLabel != null) 
                slotNumberLabel.Text = $"Slot {slotIndex + 1}";
            
            if (locationLabel != null) 
                locationLabel.Text = $"Location: {saveInfo.MapName}";
            
            if (playtimeLabel != null) 
                playtimeLabel.Text = $"Playtime: {saveInfo.GetPlayTimeFormatted()}";
            
            if (saveDateLabel != null) 
                saveDateLabel.Text = $"Saved: {saveInfo.GetSaveDateFormatted()}";
            
            if (levelLabel != null) 
                levelLabel.Text = $"Level: {saveInfo.PartyLevel}";
            
            // Display screenshot if available
            if (screenshotPreview != null && saveInfo.Screenshot != null)
            {
                screenshotPreview.Texture = saveInfo.Screenshot;
            }
        }
        
        /// <summary>
        /// Load button pressed - loads the selected save
        /// </summary>
        private void OnLoadPressed()
        {
            if (selectedSlot < 0)
                return;
            
            // Play confirmation sound
            Managers.SystemManager.Instance?.PlayOkSE();
            
            GD.Print($"Loading save from slot {selectedSlot}...");
            
            // Load the save file
            if (SaveSystem.Instance.LoadGame(selectedSlot))
            {
                GD.Print("Save loaded successfully!");
                
                // The LoadGame method should handle scene transition
                // But if it doesn't, you can add it here:
                // GetTree().ChangeSceneToFile("res://Scenes/YourGameScene.tscn");
            }
            else
            {
                GD.PrintErr("Failed to load save file!");
                Managers.SystemManager.Instance?.PlayBuzzerSE();
                
                // Show error dialog
                ShowErrorDialog("Failed to load save file. The file may be corrupted.");
            }
        }
        
        /// <summary>
        /// Back button pressed - returns to title screen
        /// </summary>
        private void OnBackPressed()
        {
            // Play cancel sound
            Managers.SystemManager.Instance?.PlayCancelSE();
            
            // Hide this panel
            Hide();
            
            GD.Print("Returning to title screen menu");
        }
        
        /// <summary>
        /// Shows an error dialog
        /// </summary>
        private void ShowErrorDialog(string message)
        {
            AcceptDialog dialog = new AcceptDialog();
            dialog.DialogText = message;
            dialog.Title = "Error";
            AddChild(dialog);
            dialog.PopupCentered();
            
            // Auto-remove when closed
            dialog.Confirmed += () => dialog.QueueFree();
        }
        
        /// <summary>
        /// Handle keyboard/gamepad input
        /// </summary>
        public override void _Input(InputEvent @event)
        {
            // Only handle input if this menu is visible
            if (!Visible)
                return;
            
            // ESC or Cancel button to go back
            if (@event.IsActionPressed("ui_cancel"))
            {
                OnBackPressed();
                GetViewport().SetInputAsHandled();
            }
        }
    }
}