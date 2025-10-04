using Godot;
using System;

namespace EchoesAcrossTime.UI
{
    /// <summary>
    /// Options menu for configuring game settings
    /// </summary>
    public partial class OptionsMenuUI : Control
    {
        #region Node References
        // Audio settings
        [Export] private HSlider bgmVolumeSlider;
        [Export] private Label bgmVolumeLabel;
        [Export] private HSlider sfxVolumeSlider;
        [Export] private Label sfxVolumeLabel;
        [Export] private HSlider voiceVolumeSlider;
        [Export] private Label voiceVolumeLabel;
        
        // Display settings
        [Export] private CheckButton fullscreenCheckbox;
        [Export] private OptionButton windowSizeDropdown;
        [Export] private CheckButton vsyncCheckbox;
        
        // Gameplay settings
        [Export] private HSlider textSpeedSlider;
        [Export] private Label textSpeedLabel;
        [Export] private CheckButton autoSaveCheckbox;
        [Export] private CheckButton showTutorialsCheckbox;
        [Export] private CheckButton battleAnimationsCheckbox;
        
        // Controls display (read-only)
        [Export] private RichTextLabel controlsDisplay;
        
        // Buttons
        [Export] private Button applyButton;
        [Export] private Button defaultsButton;
        [Export] private Button closeButton;
        #endregion
        
        #region State
        private const string SETTINGS_PATH = "user://game_settings.cfg";
        private ConfigFile settingsConfig;
        #endregion
        
        public override void _Ready()
        {
            // Connect signals
            bgmVolumeSlider.ValueChanged += OnBGMVolumeChanged;
            sfxVolumeSlider.ValueChanged += OnSFXVolumeChanged;
            voiceVolumeSlider.ValueChanged += OnVoiceVolumeChanged;
            fullscreenCheckbox.Toggled += OnFullscreenToggled;
            windowSizeDropdown.ItemSelected += OnWindowSizeChanged;
            vsyncCheckbox.Toggled += OnVSyncToggled;
            textSpeedSlider.ValueChanged += OnTextSpeedChanged;
            
            applyButton.Pressed += OnApplyPressed;
            defaultsButton.Pressed += OnDefaultsPressed;
            closeButton.Pressed += OnClosePressed;
            
            // Setup
            SetupWindowSizeDropdown();
            SetupControlsDisplay();
            
            // Load settings
            settingsConfig = new ConfigFile();
            LoadSettings();
            
            Hide();
        }
        
        private void SetupWindowSizeDropdown()
        {
            windowSizeDropdown.Clear();
            windowSizeDropdown.AddItem("1280x720", 0);
            windowSizeDropdown.AddItem("1920x1080", 1);
            windowSizeDropdown.AddItem("2560x1440", 2);
            windowSizeDropdown.AddItem("3840x2160", 3);
        }
        
        private void SetupControlsDisplay()
        {
            // Display current controls
            controlsDisplay.Text = @"[b]Controls:[/b]

[b]Movement:[/b]
• Arrow Keys / WASD - Move character
• Shift - Run

[b]Menu Navigation:[/b]
• Arrow Keys / WASD - Navigate
• Enter / Space - Confirm
• Escape / X - Cancel/Back
• Tab - Open Menu

[b]Battle:[/b]
• Enter - Confirm action
• Escape - Return to action menu
• 1-8 - Quick skill selection

[b]Interaction:[/b]
• Enter / Space - Interact with NPCs/Objects
• Escape - Skip cutscenes/dialogue";
        }
        
        public void OpenMenu()
        {
            Show();
            LoadSettings();
        }
        
        #region Audio Settings
        private void OnBGMVolumeChanged(double value)
        {
            bgmVolumeLabel.Text = $"{(int)value}%";
            SetBusVolume("BGM", (float)value);
        }
        
        private void OnSFXVolumeChanged(double value)
        {
            sfxVolumeLabel.Text = $"{(int)value}%";
            SetBusVolume("SFX", (float)value);
            SetBusVolume("UI", (float)value); // UI uses same as SFX
        }
        
        private void OnVoiceVolumeChanged(double value)
        {
            voiceVolumeLabel.Text = $"{(int)value}%";
            SetBusVolume("Voice", (float)value);
        }
        
        private void SetBusVolume(string busName, float volumePercent)
        {
            int busIndex = AudioServer.GetBusIndex(busName);
            if (busIndex == -1) return;
            
            float volumeDb;
            if (volumePercent <= 0)
            {
                volumeDb = -80f;
            }
            else
            {
                volumeDb = Mathf.LinearToDb(volumePercent / 100f);
            }
            
            AudioServer.SetBusVolumeDb(busIndex, volumeDb);
        }
        #endregion
        
        #region Display Settings
        private void OnFullscreenToggled(bool pressed)
        {
            if (pressed)
            {
                DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
            }
            else
            {
                DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
            }
        }
        
        private void OnWindowSizeChanged(long index)
        {
            if (DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Fullscreen)
                return;
            
            var size = index switch
            {
                0 => new Vector2I(1280, 720),
                1 => new Vector2I(1920, 1080),
                2 => new Vector2I(2560, 1440),
                3 => new Vector2I(3840, 2160),
                _ => new Vector2I(1280, 720)
            };
            
            DisplayServer.WindowSetSize(size);
        }
        
        private void OnVSyncToggled(bool pressed)
        {
            DisplayServer.WindowSetVsyncMode(pressed ? 
                DisplayServer.VSyncMode.Enabled : 
                DisplayServer.VSyncMode.Disabled);
        }
        #endregion
        
        #region Gameplay Settings
        private void OnTextSpeedChanged(double value)
        {
            textSpeedLabel.Text = $"{(int)value}%";
        }
        #endregion
        
        #region Button Handlers
        private void OnApplyPressed()
        {
            SaveSettings();
            Managers.SystemManager.Instance?.PlayOkSE();
            
            // Show confirmation
            var dialog = new AcceptDialog();
            dialog.DialogText = "Settings saved successfully!";
            dialog.Title = "Settings Applied";
            AddChild(dialog);
            dialog.PopupCentered();
        }
        
        private void OnDefaultsPressed()
        {
            Managers.SystemManager.Instance?.PlayOkSE();
            
            // Show confirmation
            var dialog = new ConfirmationDialog();
            dialog.DialogText = "Reset all settings to default values?";
            dialog.Title = "Confirm Reset";
            dialog.Confirmed += () =>
            {
                ResetToDefaults();
                LoadSettings(); // Refresh UI
            };
            
            AddChild(dialog);
            dialog.PopupCentered();
        }
        
        private void OnClosePressed()
        {
            Hide();
            
            // Return to main menu
            var mainMenu = GetParent().GetNodeOrNull<MainMenuUI>("%MainMenuUI");
            mainMenu?.ReturnToMainMenu();
        }
        #endregion
        
        #region Settings Management
        private void LoadSettings()
        {
            var error = settingsConfig.Load(SETTINGS_PATH);
            
            if (error != Error.Ok)
            {
                // Use defaults if file doesn't exist
                ResetToDefaults();
                return;
            }
            
            // Load audio settings
            bgmVolumeSlider.Value = settingsConfig.GetValue("audio", "bgm_volume", 80).As<Double>();
            sfxVolumeSlider.Value = settingsConfig.GetValue("audio", "sfx_volume", 80).As<Double>();
            voiceVolumeSlider.Value = settingsConfig.GetValue("audio", "voice_volume", 80).As<Double>();
            
            // Load display settings
            bool isFullscreen = settingsConfig.GetValue("display", "fullscreen", false).AsBool();
            fullscreenCheckbox.ButtonPressed = isFullscreen;
            OnFullscreenToggled(isFullscreen);
            
            int windowSizeIndex = settingsConfig.GetValue("display", "window_size", 0).AsInt32();
            windowSizeDropdown.Selected = windowSizeIndex;
            
            bool vsync = settingsConfig.GetValue("display", "vsync", true).AsBool();
            vsyncCheckbox.ButtonPressed = vsync;
            OnVSyncToggled(vsync);
            
            // Load gameplay settings
            double textSpeed = settingsConfig.GetValue("gameplay", "text_speed", 50).As<double>();
            autoSaveCheckbox.ButtonPressed = settingsConfig.GetValue("gameplay", "auto_save", true).AsBool();
            showTutorialsCheckbox.ButtonPressed = settingsConfig.GetValue("gameplay", "show_tutorials", true).AsBool();
            battleAnimationsCheckbox.ButtonPressed = settingsConfig.GetValue("gameplay", "battle_animations", true).AsBool();
            
            // Apply audio settings
            OnBGMVolumeChanged(bgmVolumeSlider.Value);
            OnSFXVolumeChanged(sfxVolumeSlider.Value);
            OnVoiceVolumeChanged(voiceVolumeSlider.Value);
        }
        
        private void SaveSettings()
        {
            // Save audio settings
            settingsConfig.SetValue("audio", "bgm_volume", bgmVolumeSlider.Value);
            settingsConfig.SetValue("audio", "sfx_volume", sfxVolumeSlider.Value);
            settingsConfig.SetValue("audio", "voice_volume", voiceVolumeSlider.Value);
            
            // Save display settings
            settingsConfig.SetValue("display", "fullscreen", fullscreenCheckbox.ButtonPressed);
            settingsConfig.SetValue("display", "window_size", windowSizeDropdown.Selected);
            settingsConfig.SetValue("display", "vsync", vsyncCheckbox.ButtonPressed);
            
            // Save gameplay settings
            settingsConfig.SetValue("gameplay", "text_speed", textSpeedSlider.Value);
            settingsConfig.SetValue("gameplay", "auto_save", autoSaveCheckbox.ButtonPressed);
            settingsConfig.SetValue("gameplay", "show_tutorials", showTutorialsCheckbox.ButtonPressed);
            settingsConfig.SetValue("gameplay", "battle_animations", battleAnimationsCheckbox.ButtonPressed);
            
            // Write to file
            settingsConfig.Save(SETTINGS_PATH);
            GD.Print("Settings saved to: ", SETTINGS_PATH);
        }
        
        private void ResetToDefaults()
        {
            // Clear config
            settingsConfig = new ConfigFile();
            
            // Set defaults
            settingsConfig.SetValue("audio", "bgm_volume", 80);
            settingsConfig.SetValue("audio", "sfx_volume", 80);
            settingsConfig.SetValue("audio", "voice_volume", 80);
            settingsConfig.SetValue("display", "fullscreen", false);
            settingsConfig.SetValue("display", "window_size", 0);
            settingsConfig.SetValue("display", "vsync", true);
            settingsConfig.SetValue("gameplay", "text_speed", 50);
            settingsConfig.SetValue("gameplay", "auto_save", true);
            settingsConfig.SetValue("gameplay", "show_tutorials", true);
            settingsConfig.SetValue("gameplay", "battle_animations", true);
            
            // Save
            settingsConfig.Save(SETTINGS_PATH);
        }
        #endregion
        
        public override void _Input(InputEvent @event)
        {
            if (!Visible) return;
            
            if (@event.IsActionPressed("ui_cancel"))
            {
                OnClosePressed();
                GetViewport().SetInputAsHandled();
            }
        }
    }
}