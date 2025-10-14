using Godot;
using System;
using EchoesAcrossTime.Managers;

namespace EchoesAcrossTime.UI
{
    /// <summary>
    /// Title screen with main menu navigation
    /// </summary>
    public partial class TitleScreen : Control
    {
        #region Node References
        [Export] private Button newGameButton;
        [Export] private Button loadGameButton;
        [Export] private Button optionsButton;
        [Export] private Button quitButton;
        [Export] private TextureRect titleBackground;
        [Export] private Label titleLabel;
        
        // Panels
        [Export] private Control loadGamePanel;
        [Export] private TitleScreenOptionsUI optionsPanel;
        #endregion
        
        private Button[] menuButtons;
        private int selectedButtonIndex = 0;
        
        public override void _Ready()
        {
            // Store all menu buttons for keyboard navigation
            menuButtons = new Button[]
            {
                newGameButton,
                loadGameButton,
                optionsButton,
                quitButton
            };
            
            // Connect button signals
            newGameButton.Pressed += OnNewGamePressed;
            loadGameButton.Pressed += OnLoadGamePressed;
            optionsButton.Pressed += OnOptionsPressed;
            quitButton.Pressed += OnQuitPressed;
            
            // Focus on first button
            newGameButton?.GrabFocus();
            
            // Start title music
            SystemManager.Instance?.PlayTitleMusic();
            
            GD.Print("Title Screen loaded");
        }
        
        private void OnNewGamePressed()
        {
            GD.Print("New Game pressed");
            SystemManager.Instance?.PlayOkSE();
            
            // TODO: Implement new game logic
            // For now, just load the first scene or show character creation
            GetTree().ChangeSceneToFile("res://Scenes/TestMap.tscn");
        }
        
        private void OnLoadGamePressed()
        {
            GD.Print("Load Game pressed");
            SystemManager.Instance?.PlayOkSE();
            
            if (loadGamePanel != null)
            {
                // Cast to LoadGameTitleUI and call OpenMenu
                var loadUI = loadGamePanel as LoadGameTitleUI;
                if (loadUI != null)
                {
                    loadUI.OpenMenu();
                }
                else
                {
                    GD.PrintErr("Load Game Panel is not a LoadGameTitleUI!");
                }
            }
            else
            {
                GD.PrintErr("Load Game Panel not assigned!");
            }
        }
        
        private void OnOptionsPressed()
        {
            GD.Print("Options pressed");
            SystemManager.Instance?.PlayOkSE();
            
            if (optionsPanel != null)
            {
                optionsPanel.OpenMenu();
            }
            else
            {
                GD.PrintErr("Options Panel not assigned!");
            }
        }
        
        private void OnQuitPressed()
        {
            GD.Print("Quit pressed");
            SystemManager.Instance?.PlayOkSE();
            
            // Show confirmation dialog
            var dialog = new ConfirmationDialog();
            dialog.DialogText = "Are you sure you want to quit?";
            dialog.Title = "Quit Game";
            dialog.Confirmed += () =>
            {
                GD.Print("Quitting game...");
                GetTree().Quit();
            };
            
            AddChild(dialog);
            dialog.PopupCentered();
        }
        
        public override void _Input(InputEvent @event)
        {
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
        
        private void NavigateUp()
        {
            selectedButtonIndex--;
            if (selectedButtonIndex < 0)
                selectedButtonIndex = menuButtons.Length - 1;
            
            menuButtons[selectedButtonIndex]?.GrabFocus();
            SystemManager.Instance?.PlayCursorSE();
        }
        
        private void NavigateDown()
        {
            selectedButtonIndex++;
            if (selectedButtonIndex >= menuButtons.Length)
                selectedButtonIndex = 0;
            
            menuButtons[selectedButtonIndex]?.GrabFocus();
            SystemManager.Instance?.PlayCursorSE();
        }
        
        private void ActivateSelectedButton()
        {
            menuButtons[selectedButtonIndex]?.EmitSignal("pressed");
        }
        
        /// <summary>
        /// Call this to close panels and return to main menu
        /// </summary>
        public void ReturnToMainMenu()
        {
            if (loadGamePanel != null) loadGamePanel.Visible = false;
            if (optionsPanel != null) optionsPanel.CloseMenu();
            
            newGameButton?.GrabFocus();
            SystemManager.Instance?.PlayCancelSE();
        }
    }
}