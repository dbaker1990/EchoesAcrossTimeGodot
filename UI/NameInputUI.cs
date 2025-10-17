using Godot;
using System;
using System.Text;
using EchoesAcrossTime.Database;

namespace EchoesAcrossTime.UI
{
    public partial class NameInputUI : Control
    {
        #region Node References
        
        [Export] private TextureRect characterPortrait;
        [Export] private Label instructionLabel;
        [Export] private Label currentNameLabel;
        [Export] private LineEdit nameDisplay;
        [Export] private Button confirmButton;
        [Export] private Button cancelButton;
        [Export] private GridContainer keyboardContainer;
        [Export] private AudioStreamPlayer confirmSfx;
        [Export] private AudioStreamPlayer cancelSfx;
        [Export] private AudioStreamPlayer typeSfx;
        
        #endregion
        
        #region Export Properties
        
        [Export] public int MaxNameLength { get; set; } = 10;
        // ✅ FIXED: Changed from CharacterDatabase to GameDatabase
        [Export] public GameDatabase Database { get; set; }
        
        #endregion
        
        #region Private Fields
        
        private StringBuilder currentName = new StringBuilder();
        private string characterId;
        private string originalName;
        private const string KEYBOARD_LAYOUT = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        
        #endregion
        
        #region Signals
        
        [Signal]
        public delegate void NameConfirmedEventHandler(string newName);
        
        [Signal]
        public delegate void NameCancelledEventHandler();
        
        #endregion
        
        #region Initialization
        
        public override void _Ready()
        {
            // Hide initially
            Visible = false;
            
            // Connect button signals
            if (confirmButton != null)
            {
                confirmButton.Pressed += OnConfirmPressed;
            }
            
            if (cancelButton != null)
            {
                cancelButton.Pressed += OnCancelPressed;
            }
            
            // Build virtual keyboard
            BuildKeyboard();
        }
        
        private void BuildKeyboard()
        {
            if (keyboardContainer == null) return;
            
            // Clear existing buttons
            foreach (Node child in keyboardContainer.GetChildren())
            {
                child.QueueFree();
            }
            
            // Add letter buttons
            foreach (char letter in KEYBOARD_LAYOUT)
            {
                var button = new Button();
                button.Text = letter.ToString();
                button.CustomMinimumSize = new Vector2(50, 50);
                button.Pressed += () => OnLetterPressed(letter);
                keyboardContainer.AddChild(button);
            }
            
            // Add space button
            var spaceButton = new Button();
            spaceButton.Text = "SPACE";
            spaceButton.CustomMinimumSize = new Vector2(150, 50);
            spaceButton.Pressed += () => OnLetterPressed(' ');
            keyboardContainer.AddChild(spaceButton);
            
            // Add backspace button
            var backspaceButton = new Button();
            backspaceButton.Text = "←";
            backspaceButton.CustomMinimumSize = new Vector2(100, 50);
            backspaceButton.Pressed += OnBackspacePressed;
            keyboardContainer.AddChild(backspaceButton);
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Initialize the name input screen for a character
        /// </summary>
        public void Initialize(string charId, int maxLength = 10)
        {
            characterId = charId;
            MaxNameLength = maxLength;
            
            // ✅ FIXED: Get character data from GameDatabase
            var character = Database?.GetCharacter(characterId);
            if (character == null)
            {
                GD.PrintErr($"NameInputUI: Character '{characterId}' not found in database");
                originalName = "Unknown";
            }
            else
            {
                // ✅ FIXED: Use DisplayName property
                originalName = character.DisplayName;
                
                // Set portrait if available
                if (characterPortrait != null && !string.IsNullOrEmpty(character.PortraitPath))
                {
                    var portrait = GD.Load<Texture2D>(character.PortraitPath);
                    if (portrait != null)
                    {
                        characterPortrait.Texture = portrait;
                    }
                }
            }
            
            // Reset name to original
            currentName.Clear();
            currentName.Append(originalName);
            
            UpdateDisplay();
            
            // Show with fade in
            Show();
            FadeIn();
        }
        
        #endregion
        
        #region Input Handling
        
        private void OnLetterPressed(char letter)
        {
            if (currentName.Length >= MaxNameLength) return;
            
            currentName.Append(letter);
            UpdateDisplay();
            PlaySound(typeSfx);
        }
        
        private void OnBackspacePressed()
        {
            if (currentName.Length == 0) return;
            
            currentName.Remove(currentName.Length - 1, 1);
            UpdateDisplay();
            PlaySound(typeSfx);
        }
        
        private void OnConfirmPressed()
        {
            string finalName = currentName.ToString().Trim();
            
            // Validate name
            if (string.IsNullOrWhiteSpace(finalName))
            {
                // Show error or just use original name
                finalName = originalName;
            }
            
            PlaySound(confirmSfx);
            EmitSignal(SignalName.NameConfirmed, finalName);
            FadeOut();
        }
        
        private void OnCancelPressed()
        {
            PlaySound(cancelSfx);
            EmitSignal(SignalName.NameCancelled);
            FadeOut();
        }
        
        public override void _Input(InputEvent @event)
        {
            if (!Visible) return;
            
            // Physical keyboard support
            if (@event is InputEventKey keyEvent && keyEvent.Pressed)
            {
                if (keyEvent.Keycode == Key.Enter || keyEvent.Keycode == Key.KpEnter)
                {
                    OnConfirmPressed();
                    AcceptEvent();
                }
                else if (keyEvent.Keycode == Key.Escape)
                {
                    OnCancelPressed();
                    AcceptEvent();
                }
                else if (keyEvent.Keycode == Key.Backspace)
                {
                    OnBackspacePressed();
                    AcceptEvent();
                }
                else if (keyEvent.Unicode != 0)
                {
                    char inputChar = (char)keyEvent.Unicode;
                    if (char.IsLetterOrDigit(inputChar) || inputChar == ' ')
                    {
                        OnLetterPressed(char.ToUpper(inputChar));
                        AcceptEvent();
                    }
                }
            }
        }
        
        #endregion
        
        #region Display Update
        
        private void UpdateDisplay()
        {
            if (nameDisplay != null)
            {
                nameDisplay.Text = currentName.ToString();
            }
            
            if (currentNameLabel != null)
            {
                currentNameLabel.Text = $"Name: {currentName} ({currentName.Length}/{MaxNameLength})";
            }
        }
        
        #endregion
        
        #region Animations
        
        private void FadeIn()
        {
            Modulate = new Color(1, 1, 1, 0);
            var tween = CreateTween();
            tween.TweenProperty(this, "modulate:a", 1.0f, 0.3f);
        }
        
        private async void FadeOut()
        {
            var tween = CreateTween();
            tween.TweenProperty(this, "modulate:a", 0.0f, 0.3f);
            await ToSignal(tween, Tween.SignalName.Finished);
            QueueFree();
        }
        
        #endregion
        
        #region Audio
        
        private void PlaySound(AudioStreamPlayer player)
        {
            if (player != null && player.Stream != null)
            {
                player.Play();
            }
        }
        
        #endregion
    }
}