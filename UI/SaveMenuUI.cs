using Godot;
using System;
using System.Collections.Generic;

namespace EchoesAcrossTime.UI
{
    /// <summary>
    /// Save menu for saving the game to different slots
    /// </summary>
    public partial class SaveMenuUI : Control
    {
        #region Node References
        [Export] private ItemList saveSlotList;
        [Export] private Panel saveDetailPanel;
        
        // Save details
        [Export] private Label slotLabel;
        [Export] private Label locationLabel;
        [Export] private Label playtimeLabel;
        [Export] private Label saveDateLabel;
        [Export] private Label levelLabel;
        [Export] private Label goldLabel;
        [Export] private TextureRect previewImage;
        
        // Buttons
        [Export] private Button saveButton;
        [Export] private Button deleteButton;
        [Export] private Button closeButton;
        
        // Confirmation dialog
        [Export] private Panel confirmationPanel;
        [Export] private Label confirmationText;
        [Export] private Button confirmYesButton;
        [Export] private Button confirmNoButton;
        #endregion
        
        #region State
        private const int MAX_SAVE_SLOTS = 10;
        private int selectedSlot = -1;
        private SaveFileInfo selectedSaveInfo;
        private bool isConfirmingOverwrite;
        #endregion
        
        public override void _Ready()
        {
            // Connect signals
            saveSlotList.ItemSelected += OnSlotSelected;
            saveButton.Pressed += OnSavePressed;
            deleteButton.Pressed += OnDeletePressed;
            closeButton.Pressed += OnClosePressed;
            confirmYesButton.Pressed += OnConfirmYes;
            confirmNoButton.Pressed += OnConfirmNo;
            
            // Hide confirmation panel
            confirmationPanel?.Hide();
            
            Hide();
        }
        
        public void OpenMenu()
        {
            Show();
            RefreshSaveSlots();
            
            // Select first slot
            if (saveSlotList.ItemCount > 0)
            {
                saveSlotList.Select(0);
                OnSlotSelected(0);
            }
            
            saveSlotList.GrabFocus();
        }
        
        private void RefreshSaveSlots()
        {
            saveSlotList.Clear();
            
            if (SaveSystem.Instance == null)
            {
                GD.PrintErr("SaveSystem.Instance is null!");
                return;
            }
            
            for (int i = 0; i < MAX_SAVE_SLOTS; i++)
            {
                if (SaveSystem.Instance.SaveExists(i))
                {
                    var info = SaveSystem.Instance.GetSaveInfo(i);
                    if (info != null)
                    {
                        string displayText = $"Slot {i + 1}: {info.GetSaveDateFormatted()}";
                        saveSlotList.AddItem(displayText);
                    }
                    else
                    {
                        saveSlotList.AddItem($"Slot {i + 1}: (Corrupted)");
                    }
                }
                else
                {
                    saveSlotList.AddItem($"Slot {i + 1}: (Empty)");
                }
            }
        }
        
        private void OnSlotSelected(long index)
        {
            selectedSlot = (int)index;
            
            // Try to load save info for preview
            if (SaveSystem.Instance?.SaveExists(selectedSlot) == true)
            {
                selectedSaveInfo = SaveSystem.Instance.GetSaveInfo(selectedSlot);
                DisplaySaveDetails(selectedSaveInfo);
                deleteButton.Disabled = false;
            }
            else
            {
                selectedSaveInfo = null;
                DisplayEmptySlot();
                deleteButton.Disabled = true;
            }
            
            saveButton.Disabled = false;
            Managers.SystemManager.Instance?.PlayCursorSE();
        }
        
        private void DisplaySaveDetails(SaveFileInfo info)
        {
            if (info == null)
            {
                DisplayEmptySlot();
                return;
            }
            
            slotLabel.Text = $"Slot {selectedSlot + 1}";
            locationLabel.Text = $"Location: {info.MapName}";
            playtimeLabel.Text = $"Playtime: {info.GetPlayTimeFormatted()}";
            saveDateLabel.Text = $"Saved: {info.GetSaveDateFormatted()}";
            levelLabel.Text = $"Level: {info.PartyLevel}";
            goldLabel.Text = "Gold: ---"; // Not in SaveFileInfo
            
            // Set preview image
            if (info.Screenshot != null)
            {
                previewImage.Texture = info.Screenshot;
            }
        }
        
        private void DisplayEmptySlot()
        {
            slotLabel.Text = $"Slot {selectedSlot + 1}";
            locationLabel.Text = "Location: ---";
            playtimeLabel.Text = "Playtime: ---";
            saveDateLabel.Text = "Saved: ---";
            levelLabel.Text = "Level: ---";
            goldLabel.Text = "Gold: ---";
            previewImage.Texture = null;
        }
        
        private void OnSavePressed()
        {
            if (selectedSlot < 0) return;
            
            Managers.SystemManager.Instance?.PlayOkSE();
            
            // Check if slot has existing save
            if (SaveSystem.Instance?.SaveExists(selectedSlot) == true)
            {
                // Show overwrite confirmation
                ShowOverwriteConfirmation();
            }
            else
            {
                // Save directly
                PerformSave();
            }
        }
        
        private void ShowOverwriteConfirmation()
        {
            isConfirmingOverwrite = true;
            confirmationText.Text = $"Overwrite save in Slot {selectedSlot + 1}?";
            confirmationPanel?.Show();
            confirmYesButton?.GrabFocus();
        }
        
        private void ShowDeleteConfirmation()
        {
            isConfirmingOverwrite = false;
            confirmationText.Text = $"Delete save in Slot {selectedSlot + 1}?\nThis cannot be undone!";
            confirmationPanel?.Show();
            confirmYesButton?.GrabFocus();
        }
        
        private void OnConfirmYes()
        {
            confirmationPanel?.Hide();
            
            if (isConfirmingOverwrite)
            {
                PerformSave();
            }
            else
            {
                PerformDelete();
            }
        }
        
        private void OnConfirmNo()
        {
            confirmationPanel?.Hide();
            Managers.SystemManager.Instance?.PlayCancelSE();
        }
        
        private void PerformSave()
        {
            if (SaveSystem.Instance?.SaveGame(selectedSlot) == true)
            {
                GD.Print($"Game saved to slot {selectedSlot}");
                Managers.SystemManager.Instance?.PlaySaveSE();
                
                // Show success message
                var dialog = new AcceptDialog();
                dialog.DialogText = $"Game saved to Slot {selectedSlot + 1}!";
                dialog.Title = "Save Successful";
                AddChild(dialog);
                dialog.PopupCentered();
                
                // Refresh display
                RefreshSaveSlots();
                OnSlotSelected(selectedSlot);
            }
            else
            {
                GD.PrintErr("Failed to save game!");
                Managers.SystemManager.Instance?.PlayBuzzerSE();
                
                // Show error message
                var dialog = new AcceptDialog();
                dialog.DialogText = "Failed to save game. Please try again.";
                dialog.Title = "Save Failed";
                AddChild(dialog);
                dialog.PopupCentered();
            }
        }
        
        private void OnDeletePressed()
        {
            if (selectedSlot < 0 || selectedSaveInfo == null) return;
            
            Managers.SystemManager.Instance?.PlayOkSE();
            ShowDeleteConfirmation();
        }
        
        private void PerformDelete()
        {
            if (SaveSystem.Instance?.DeleteSave(selectedSlot) == true)
            {
                GD.Print($"Deleted save from slot {selectedSlot}");
                Managers.SystemManager.Instance?.PlayOkSE();
                
                // Refresh display
                RefreshSaveSlots();
                OnSlotSelected(selectedSlot);
            }
            else
            {
                GD.PrintErr("Failed to delete save!");
                Managers.SystemManager.Instance?.PlayBuzzerSE();
            }
        }
        
        private void OnClosePressed()
        {
            Hide();
            
            // Return to main menu
            var mainMenu = GetParent().GetNodeOrNull<MainMenuUI>("%MainMenuUI");
            mainMenu?.ReturnToMainMenu();
        }
        
        public override void _Input(InputEvent @event)
        {
            if (!Visible) return;
            
            if (@event.IsActionPressed("ui_cancel"))
            {
                if (confirmationPanel?.Visible == true)
                {
                    OnConfirmNo();
                }
                else
                {
                    OnClosePressed();
                }
                GetViewport().SetInputAsHandled();
            }
        }
    }
}