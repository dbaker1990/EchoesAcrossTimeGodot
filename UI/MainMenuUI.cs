using Godot;
using System;
using System.Collections.Generic;

namespace EchoesAcrossTime.UI
{
    /// <summary>
    /// Main in-game menu (pause menu) that provides access to all game systems
    /// Press ESC/Start to open during gameplay
    /// </summary>
    public partial class MainMenuUI : Control
    {
        #region Node References
        [Export] private Control menuPanel;
        [Export] private VBoxContainer buttonContainer;
        
        // Menu buttons
        [Export] private Button itemButton;
        [Export] private Button skillButton;
        [Export] private Button equipButton;
        [Export] private Button statusButton;
        [Export] private Button craftingButton;
        [Export] private Button partyButton;
        [Export] private Button bestiaryButton;
        [Export] private Button questButton;
        [Export] private Button optionsButton;
        [Export] private Button saveButton;
        [Export] private Button loadButton;
        [Export] private Button endGameButton;
        
        // Sub-menu references (get from tree)
        private Control itemMenuUI;
        private Control skillMenuUI;
        private Control equipMenuUI;
        private Control statusMenuUI;
        private Control craftingUI;
        private Control partyMenuUI;
        private Control bestiaryUI;
        private Control questUI;
        private Control optionsUI;
        private Control saveUI;
        private Control loadUI;
        
        // Header info
        [Export] private Label locationLabel;
        [Export] private Label playtimeLabel;
        [Export] private Label goldLabel;
        #endregion
        
        #region State
        private bool isOpen;
        private int selectedButtonIndex;
        private Button[] menuButtons;
        #endregion
        
        public override void _Ready()
        {
            // Initialize button array
            menuButtons = new[]
            {
                itemButton, skillButton, equipButton, statusButton,
                craftingButton, partyButton, bestiaryButton, questButton,
                optionsButton, saveButton, loadButton, endGameButton
            };
            
            // Connect button signals
            itemButton.Pressed += OnItemPressed;
            skillButton.Pressed += OnSkillPressed;
            equipButton.Pressed += OnEquipPressed;
            statusButton.Pressed += OnStatusPressed;
            craftingButton.Pressed += OnCraftingPressed;
            partyButton.Pressed += OnPartyPressed;
            bestiaryButton.Pressed += OnBestiaryPressed;
            questButton.Pressed += OnQuestPressed;
            optionsButton.Pressed += OnOptionsPressed;
            saveButton.Pressed += OnSavePressed;
            loadButton.Pressed += OnLoadPressed;
            endGameButton.Pressed += OnEndGamePressed;
            
            // Get sub-menu references
            GetSubMenuReferences();
            
            // Start hidden
            Hide();
            isOpen = false;
        }
        
        private void GetSubMenuReferences()
        {
            // Try to find sub-menus in the scene tree
            itemMenuUI = GetNodeOrNull<Control>("%ItemMenuUI");
            skillMenuUI = GetNodeOrNull<Control>("%SkillMenuUI");
            equipMenuUI = GetNodeOrNull<Control>("%EquipMenuUI");
            statusMenuUI = GetNodeOrNull<Control>("%StatusMenuUI");
            craftingUI = GetNodeOrNull<Control>("%CraftingUI");
            partyMenuUI = GetNodeOrNull<Control>("%PartyMenuUI");
            bestiaryUI = GetNodeOrNull<Control>("%BestiaryUI");
            questUI = GetNodeOrNull<Control>("%QuestUI");
            optionsUI = GetNodeOrNull<Control>("%OptionsUI");
            saveUI = GetNodeOrNull<Control>("%SaveUI");
            loadUI = GetNodeOrNull<Control>("%LoadUI");
        }
        
        public override void _Input(InputEvent @event)
        {
            if (!isOpen) return;
            
            // Close menu with cancel button
            if (@event.IsActionPressed("ui_cancel"))
            {
                CloseMenu();
                GetViewport().SetInputAsHandled();
                return;
            }
            
            // Keyboard navigation
            if (@event.IsActionPressed("ui_up"))
            {
                NavigateUp();
                GetViewport().SetInputAsHandled();
            }
            else if (@event.IsActionPressed("ui_down"))
            {
                NavigateDown();
                GetViewport().SetInputAsHandled();
            }
            else if (@event.IsActionPressed("ui_accept"))
            {
                ActivateSelectedButton();
                GetViewport().SetInputAsHandled();
            }
        }
        
        #region Navigation
        private void NavigateUp()
        {
            selectedButtonIndex--;
            if (selectedButtonIndex < 0)
                selectedButtonIndex = menuButtons.Length - 1;
            
            menuButtons[selectedButtonIndex]?.GrabFocus();
            Managers.SystemManager.Instance?.PlayCursorSE();
        }
        
        private void NavigateDown()
        {
            selectedButtonIndex++;
            if (selectedButtonIndex >= menuButtons.Length)
                selectedButtonIndex = 0;
            
            menuButtons[selectedButtonIndex]?.GrabFocus();
            Managers.SystemManager.Instance?.PlayCursorSE();
        }
        
        private void ActivateSelectedButton()
        {
            menuButtons[selectedButtonIndex]?.EmitSignal("pressed");
        }
        #endregion
        
        #region Menu Control
        /// <summary>
        /// Open the main menu
        /// </summary>
        public void OpenMenu()
        {
            Show();
            isOpen = true;
            
            // Update header info
            UpdateHeaderInfo();
            
            // Focus first button
            selectedButtonIndex = 0;
            itemButton?.GrabFocus();
            
            // Pause game
            GetTree().Paused = true;
            
            Managers.SystemManager.Instance?.PlayOkSE();
        }
        
        /// <summary>
        /// Close the main menu
        /// </summary>
        public void CloseMenu()
        {
            Hide();
            isOpen = false;
            
            // Close all sub-menus
            CloseAllSubMenus();
            
            // Unpause game
            GetTree().Paused = false;
            
            Managers.SystemManager.Instance?.PlayCancelSE();
        }
        
        /// <summary>
        /// Close all sub-menus and return to main menu
        /// </summary>
        public void ReturnToMainMenu()
        {
            CloseAllSubMenus();
            menuPanel?.Show();
            itemButton?.GrabFocus();
            Managers.SystemManager.Instance?.PlayCancelSE();
        }
        
        private void CloseAllSubMenus()
        {
            itemMenuUI?.Hide();
            skillMenuUI?.Hide();
            equipMenuUI?.Hide();
            statusMenuUI?.Hide();
            craftingUI?.Hide();
            partyMenuUI?.Hide();
            bestiaryUI?.Hide();
            questUI?.Hide();
            optionsUI?.Hide();
            saveUI?.Hide();
            loadUI?.Hide();
        }
        
        private void UpdateHeaderInfo()
        {
            // Update location
            if (locationLabel != null)
            {
                // Get current map name
                var currentScene = GetTree().CurrentScene;
                locationLabel.Text = currentScene?.Name ?? "Unknown Location";
            }
            
            // Update playtime
            if (playtimeLabel != null)
            {
                var playtime = Managers.GameManager.Instance?.GetPlaytime() ?? 0;
                var hours = (int)(playtime / 3600);
                var minutes = (int)((playtime % 3600) / 60);
                playtimeLabel.Text = $"{hours:D2}:{minutes:D2}";
            }
            
            // Update gold
            if (goldLabel != null)
            {
                var gold = Items.InventorySystem.Instance?.GetGold() ?? 0;
                goldLabel.Text = $"{gold} G";
            }
        }
        #endregion
        
        #region Button Handlers
        private void OnItemPressed()
        {
            Managers.SystemManager.Instance?.PlayOkSE();
            menuPanel?.Hide();
            
            if (itemMenuUI != null)
            {
                itemMenuUI.Show();
                // Call OpenMenu if it exists
                itemMenuUI.Call("OpenMenu");
            }
            else
            {
                GD.PrintErr("ItemMenuUI not found!");
                menuPanel?.Show();
            }
        }
        
        private void OnSkillPressed()
        {
            Managers.SystemManager.Instance?.PlayOkSE();
            menuPanel?.Hide();
            
            if (skillMenuUI != null)
            {
                skillMenuUI.Show();
                skillMenuUI.Call("OpenMenu");
            }
            else
            {
                GD.PrintErr("SkillMenuUI not found!");
                menuPanel?.Show();
            }
        }
        
        private void OnEquipPressed()
        {
            Managers.SystemManager.Instance?.PlayOkSE();
            menuPanel?.Hide();
            
            if (equipMenuUI != null)
            {
                equipMenuUI.Show();
                equipMenuUI.Call("OpenMenu");
            }
            else
            {
                GD.PrintErr("EquipMenuUI not found!");
                menuPanel?.Show();
            }
        }
        
        private void OnStatusPressed()
        {
            Managers.SystemManager.Instance?.PlayOkSE();
            menuPanel?.Hide();
            
            if (statusMenuUI != null)
            {
                statusMenuUI.Show();
                statusMenuUI.Call("OpenMenu");
            }
            else
            {
                GD.PrintErr("StatusMenuUI not found!");
                menuPanel?.Show();
            }
        }
        
        private void OnCraftingPressed()
        {
            Managers.SystemManager.Instance?.PlayOkSE();
            menuPanel?.Hide();
            
            if (craftingUI != null)
            {
                craftingUI.Show();
                craftingUI.Call("OpenMenu");
            }
            else
            {
                GD.PrintErr("CraftingUI not found!");
                menuPanel?.Show();
            }
        }
        
        private void OnPartyPressed()
        {
            Managers.SystemManager.Instance?.PlayOkSE();
            menuPanel?.Hide();
            
            if (partyMenuUI != null)
            {
                partyMenuUI.Show();
                partyMenuUI.Call("OpenMenu");
            }
            else
            {
                GD.PrintErr("PartyMenuUI not found!");
                menuPanel?.Show();
            }
        }
        
        private void OnBestiaryPressed()
        {
            Managers.SystemManager.Instance?.PlayOkSE();
            menuPanel?.Hide();
            
            if (bestiaryUI != null)
            {
                bestiaryUI.Show();
                bestiaryUI.Call("ShowBestiary");
            }
            else
            {
                GD.PrintErr("BestiaryUI not found!");
                menuPanel?.Show();
            }
        }
        
        private void OnQuestPressed()
        {
            Managers.SystemManager.Instance?.PlayOkSE();
            menuPanel?.Hide();
            
            if (questUI != null)
            {
                questUI.Show();
                questUI.Call("OpenMenu");
            }
            else
            {
                GD.PrintErr("QuestUI not found!");
                menuPanel?.Show();
            }
        }
        
        private void OnOptionsPressed()
        {
            Managers.SystemManager.Instance?.PlayOkSE();
            menuPanel?.Hide();
            
            if (optionsUI != null)
            {
                optionsUI.Show();
                optionsUI.Call("OpenMenu");
            }
            else
            {
                GD.PrintErr("OptionsUI not found!");
                menuPanel?.Show();
            }
        }
        
        private void OnSavePressed()
        {
            Managers.SystemManager.Instance?.PlayOkSE();
            menuPanel?.Hide();
            
            if (saveUI != null)
            {
                saveUI.Show();
                saveUI.Call("OpenMenu");
            }
            else
            {
                // Fallback - quick save
                GD.Print("Performing quick save...");
                SaveSystem.SaveManager.Instance?.QuickSave();
                menuPanel?.Show();
            }
        }
        
        private void OnLoadPressed()
        {
            Managers.SystemManager.Instance?.PlayOkSE();
            menuPanel?.Hide();
            
            if (loadUI != null)
            {
                loadUI.Show();
                loadUI.Call("OpenMenu");
            }
            else
            {
                GD.PrintErr("LoadUI not found!");
                menuPanel?.Show();
            }
        }
        
        private void OnEndGamePressed()
        {
            Managers.SystemManager.Instance?.PlayOkSE();
            
            // Show confirmation dialog
            ShowEndGameConfirmation();
        }
        
        private void ShowEndGameConfirmation()
        {
            var dialog = new AcceptDialog();
            dialog.DialogText = "Return to title screen?\n(Make sure to save first!)";
            dialog.Title = "End Game";
            
            var confirmButton = new Button();
            confirmButton.Text = "Yes, Return to Title";
            confirmButton.Pressed += () =>
            {
                GetTree().Paused = false;
                GetTree().ChangeSceneToFile("res://Scenes/TitleScreen.tscn");
            };
            
            dialog.AddButton("Cancel", true, "cancel");
            dialog.AddChild(confirmButton);
            AddChild(dialog);
            dialog.PopupCentered();
        }
        #endregion
    }
}