// UI/TitleScreen.cs
using Godot;
using EchoesAcrossTime.Managers;

namespace EchoesAcrossTime.UI
{
    /// <summary>
    /// Title Screen controller
    /// Handles main menu navigation and game initialization
    /// </summary>
    public partial class TitleScreen : Control
    {
        #region Node References
        [ExportGroup("UI Elements")]
        [Export] private Button newGameButton;
        [Export] private Button loadGameButton;
        [Export] private Button optionsButton;
        [Export] private Button quitButton;
        
        [ExportGroup("Panels")]
        [Export] private Control loadGamePanel;
        [Export] private Control optionsPanel;
        
        [ExportGroup("Background")]
        [Export] private TextureRect titleBackground;
        [Export] private Label titleLabel;
        #endregion
        
        private int selectedButtonIndex = 0;
        private Button[] menuButtons;
        
        public override void _Ready()
        {
            // Initialize button array for keyboard navigation
            menuButtons = new[] { newGameButton, loadGameButton, optionsButton, quitButton };
            
            // Connect button signals
            ConnectButtonSignals();
            
            // Ensure panels are hidden
            if (loadGamePanel != null) loadGamePanel.Visible = false;
            if (optionsPanel != null) optionsPanel.Visible = false;
            
            // Play title music
            PlayTitleMusic();
            
            // Focus first button
            if (newGameButton != null)
            {
                newGameButton.GrabFocus();
            }
            
            GD.Print("Title Screen Ready");
        }
        
        private void ConnectButtonSignals()
        {
            if (newGameButton != null)
            {
                newGameButton.Pressed += OnNewGamePressed;
                newGameButton.FocusEntered += () => OnButtonHovered(0);
            }
            
            if (loadGameButton != null)
            {
                loadGameButton.Pressed += OnLoadGamePressed;
                loadGameButton.FocusEntered += () => OnButtonHovered(1);
            }
            
            if (optionsButton != null)
            {
                optionsButton.Pressed += OnOptionsPressed;
                optionsButton.FocusEntered += () => OnButtonHovered(2);
            }
            
            if (quitButton != null)
            {
                quitButton.Pressed += OnQuitPressed;
                quitButton.FocusEntered += () => OnButtonHovered(3);
            }
        }
        
        private void OnButtonHovered(int index)
        {
            selectedButtonIndex = index;
            // Play cursor sound
            SystemManager.Instance?.PlayCursorSE();
        }
        
        private void OnNewGamePressed()
        {
            GD.Print("New Game selected");
            SystemManager.Instance?.PlayOkSE();
            
            // Start new game
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartNewGame();
                
                // Get starting map from SystemDatabase
                string startMap = SystemManager.Instance?.SystemData?.StartingMapPath 
                    ?? "res://Maps/Veridia/VeridiaCapital.tscn";
                
                // Change to starting map
                GetTree().ChangeSceneToFile(startMap);
            }
            else
            {
                GD.PrintErr("TitleScreen: GameManager instance not found!");
            }
        }
        
        private void OnLoadGamePressed()
        {
            GD.Print("Load Game selected");
            SystemManager.Instance?.PlayOkSE();
            
            // Show load game panel
            if (loadGamePanel != null)
            {
                loadGamePanel.Visible = true;
            }
            else
            {
                // Fallback: Try to load most recent save
                if (SaveSystem.Instance != null)
                {
                    // Try to load slot 0 (auto-save) or first available save
                    if (SaveSystem.Instance.SaveExists(0))
                    {
                        SaveSystem.Instance.LoadGame(0);
                    }
                    else
                    {
                        SystemManager.Instance?.PlayBuzzerSE();
                        GD.Print("No save file found");
                    }
                }
            }
        }
        
        private void OnOptionsPressed()
        {
            GD.Print("Options selected");
            SystemManager.Instance?.PlayOkSE();
            
            // Show options panel
            if (optionsPanel != null)
            {
                optionsPanel.Visible = true;
            }
            else
            {
                GD.Print("Options panel not configured");
            }
        }
        
        private void OnQuitPressed()
        {
            GD.Print("Quit to Desktop selected");
            SystemManager.Instance?.PlayCancelSE();
            
            // Quit the game
            GetTree().Quit();
        }
        
        private void PlayTitleMusic()
        {
            if (SystemManager.Instance != null)
            {
                SystemManager.Instance.PlayTitleMusic();
                GD.Print("Playing title music");
            }
            else
            {
                GD.PrintErr("TitleScreen: SystemManager instance not found!");
            }
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
            if (optionsPanel != null) optionsPanel.Visible = false;
            
            newGameButton?.GrabFocus();
            SystemManager.Instance?.PlayCancelSE();
        }
    }
}