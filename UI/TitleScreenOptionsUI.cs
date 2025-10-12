using Godot;
using System;

namespace EchoesAcrossTime.UI
{
    /// <summary>
    /// Options menu specifically for the title screen
    /// Handles window size, audio volumes, FPS display, and visual filters
    /// </summary>
    public partial class TitleScreenOptionsUI : Control
    {
        #region Node References
        // Audio settings
        [Export] private HSlider bgmVolumeSlider;
        [Export] private Label bgmVolumeLabel;
        [Export] private HSlider sfxVolumeSlider;
        [Export] private Label sfxVolumeLabel;
        
        // Display settings
        [Export] private CheckButton fullscreenCheckbox;
        [Export] private OptionButton windowSizeDropdown;
        [Export] private CheckButton vsyncCheckbox;
        [Export] private CheckButton showFPSCheckbox;
        
        // Filter settings
        [Export] private OptionButton filterDropdown;
        [Export] private HSlider filterIntensitySlider;
        [Export] private Label filterIntensityLabel;
        
        // Buttons
        [Export] private Button applyButton;
        [Export] private Button defaultsButton;
        [Export] private Button backButton;
        
        // Background panel
        [Export] private Panel backgroundPanel;
        #endregion
        
        #region State
        private const string SETTINGS_PATH = "user://game_settings.cfg";
        private ConfigFile settingsConfig;
        private RetroEffectsManager effectsManager;
        #endregion
        
        public override void _Ready()
        {
            GD.Print("=== TitleScreenOptionsUI Initializing ===");
            
            // Initialize config
            settingsConfig = new ConfigFile();
            
            // Get RetroEffectsManager - try multiple locations
            // 1. Try unique name (scene-based)
            effectsManager = GetNodeOrNull<RetroEffectsManager>("%RetroEffectsManager");
            
            // 2. Try AutoLoad
            if (effectsManager == null)
            {
                effectsManager = GetNodeOrNull<RetroEffectsManager>("/root/RetroEffectsManager");
            }
            
            // 3. Try finding in current scene
            if (effectsManager == null)
            {
                effectsManager = GetTree().CurrentScene.GetNodeOrNull<RetroEffectsManager>("RetroEffectsManager");
            }
            
            // 4. Try recursive search in scene
            if (effectsManager == null)
            {
                effectsManager = FindRetroEffectsManager(GetTree().CurrentScene);
            }
            
            if (effectsManager == null)
            {
                GD.PrintErr("❌ TitleScreenOptionsUI: Could not find RetroEffectsManager!");
                GD.PrintErr("   Make sure it's either:");
                GD.PrintErr("   1. Added to your scene as a CanvasLayer");
                GD.PrintErr("   2. Set as Unique Name (%)");
                GD.PrintErr("   3. Added to Project Settings → AutoLoad");
            }
            else
            {
                GD.Print($"✓ Found RetroEffectsManager at: {effectsManager.GetPath()}");
            }
            
            // Connect signals
            ConnectSignals();
            
            // Setup dropdowns
            SetupWindowSizeDropdown();
            SetupFilterDropdown();
            
            // Load saved settings
            LoadSettings();
            
            // Report current window state
            var currentMode = DisplayServer.WindowGetMode();
            var currentSize = DisplayServer.WindowGetSize();
            GD.Print($"Current window mode: {currentMode}");
            GD.Print($"Current window size: {currentSize.X}x{currentSize.Y}");
            
            Hide();
            
            GD.Print("=== TitleScreenOptionsUI Ready ===");
        }
        
        // Helper method to recursively find RetroEffectsManager
        private RetroEffectsManager FindRetroEffectsManager(Node node)
        {
            if (node is RetroEffectsManager manager)
            {
                return manager;
            }
            
            foreach (Node child in node.GetChildren())
            {
                var found = FindRetroEffectsManager(child);
                if (found != null)
                {
                    return found;
                }
            }
            
            return null;
        }
        
        private void ConnectSignals()
        {
            bgmVolumeSlider.ValueChanged += OnBGMVolumeChanged;
            sfxVolumeSlider.ValueChanged += OnSFXVolumeChanged;
            
            fullscreenCheckbox.Toggled += OnFullscreenToggled;
            windowSizeDropdown.ItemSelected += OnWindowSizeChanged;
            vsyncCheckbox.Toggled += OnVSyncToggled;
            showFPSCheckbox.Toggled += OnShowFPSToggled;
            
            filterDropdown.ItemSelected += OnFilterChanged;
            filterIntensitySlider.ValueChanged += OnFilterIntensityChanged;
            
            applyButton.Pressed += OnApplyPressed;
            defaultsButton.Pressed += OnDefaultsPressed;
            backButton.Pressed += OnBackPressed;
        }
        
        private void SetupWindowSizeDropdown()
        {
            windowSizeDropdown.Clear();
            windowSizeDropdown.AddItem("1280x720", 0);
            windowSizeDropdown.AddItem("1920x1080", 1);
            windowSizeDropdown.AddItem("2560x1440", 2);
            windowSizeDropdown.AddItem("3840x2160", 3);
        }
        
        private void SetupFilterDropdown()
        {
            filterDropdown.Clear();
            filterDropdown.AddItem("None", 0);
            // CRT removed - not compatible with AutoLoad
            filterDropdown.AddItem("Scanlines", 1);      // NEW: Classic horizontal lines
            filterDropdown.AddItem("Dithering", 2);      // NEW: SNES-style dithering
            filterDropdown.AddItem("VHS", 3);
            filterDropdown.AddItem("Pixelation", 4);
            filterDropdown.AddItem("Gameboy", 5);
            filterDropdown.AddItem("Color Palette", 6);
            filterDropdown.AddItem("RGB Glitch", 7);
        }
        
        public void OpenMenu()
        {
            Show();
            LoadSettings();
            Managers.SystemManager.Instance?.PlayOkSE();
        }
        
        public void CloseMenu()
        {
            Hide();
            Managers.SystemManager.Instance?.PlayCancelSE();
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
            SetBusVolume("UI", (float)value);
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
                GD.Print("Switching to Fullscreen mode");
                DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
            }
            else
            {
                GD.Print("Switching to Windowed mode");
                DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
                
                // Apply the current window size selection
                OnWindowSizeChanged(windowSizeDropdown.Selected);
            }
        }
        
        private void OnWindowSizeChanged(long index)
        {
            var currentMode = DisplayServer.WindowGetMode();
            
            if (currentMode == DisplayServer.WindowMode.Fullscreen)
            {
                GD.Print("⚠ Window size change ignored - currently in fullscreen mode");
                GD.Print("   Uncheck Fullscreen first to change window size");
                return;
            }
            
            var size = index switch
            {
                0 => new Vector2I(1280, 720),
                1 => new Vector2I(1920, 1080),
                2 => new Vector2I(2560, 1440),
                3 => new Vector2I(3840, 2160),
                _ => new Vector2I(1280, 720)
            };
            
            GD.Print($"Setting window size to: {size.X}x{size.Y}");
            
            // Get screen size to center window
            var screenSize = DisplayServer.ScreenGetSize();
            var windowPos = (screenSize - size) / 2;
            
            // Set size and position
            DisplayServer.WindowSetSize(size);
            DisplayServer.WindowSetPosition(windowPos);
            
            GD.Print($"✓ Window size applied: {size.X}x{size.Y}");
            GD.Print($"  Window position: {windowPos.X}, {windowPos.Y}");
        }
        
        private void OnVSyncToggled(bool pressed)
        {
            DisplayServer.WindowSetVsyncMode(pressed ? 
                DisplayServer.VSyncMode.Enabled : 
                DisplayServer.VSyncMode.Disabled);
        }
        
        private void OnShowFPSToggled(bool pressed)
        {
            // Try both possible names (case-sensitive)
            var fpsCounter = GetNodeOrNull<FPSCounter>("/root/FPSCounter");
            if (fpsCounter == null)
            {
                fpsCounter = GetNodeOrNull<FPSCounter>("/root/FpsCounter");
            }
            
            if (fpsCounter != null)
            {
                fpsCounter.Visible = pressed;
                GD.Print($"FPS Counter visibility set to: {pressed}");
            }
            else
            {
                GD.PrintErr("FPSCounter not found in AutoLoad! Check the name matches.");
            }
        }
        #endregion
        
        #region Filter Settings
        private void OnFilterChanged(long index)
        {
            if (effectsManager == null)
            {
                GD.PrintErr("RetroEffectsManager not found! Filters will not work. Make sure it's added as an AutoLoad.");
                return;
            }
            
            GD.Print($"Filter changed to index: {index}");
            
            switch (index)
            {
                case 0: // None
                    effectsManager.SetEffect(RetroEffectsManager.RetroEffectType.None);
                    filterIntensitySlider.Editable = false;
                    GD.Print("Filter: None");
                    break;
                case 1: // Scanlines
                    effectsManager.SetEffect(RetroEffectsManager.RetroEffectType.Scanlines);
                    filterIntensitySlider.Editable = true;
                    GD.Print("Filter: Scanlines");
                    break;
                case 2: // Dithering
                    effectsManager.SetEffect(RetroEffectsManager.RetroEffectType.Dithering);
                    filterIntensitySlider.Editable = true;
                    GD.Print("Filter: Dithering");
                    break;
                case 3: // VHS
                    effectsManager.SetEffect(RetroEffectsManager.RetroEffectType.VHS);
                    filterIntensitySlider.Editable = true;
                    GD.Print("Filter: VHS");
                    break;
                case 4: // Pixelation
                    effectsManager.SetEffect(RetroEffectsManager.RetroEffectType.Pixelation);
                    filterIntensitySlider.Editable = true;
                    GD.Print("Filter: Pixelation");
                    break;
                case 5: // Gameboy
                    effectsManager.SetEffect(RetroEffectsManager.RetroEffectType.Gameboy);
                    filterIntensitySlider.Editable = false;
                    GD.Print("Filter: Gameboy");
                    break;
                case 6: // Color Palette
                    effectsManager.SetEffect(RetroEffectsManager.RetroEffectType.ColorPalette);
                    filterIntensitySlider.Editable = true;
                    GD.Print("Filter: Color Palette");
                    break;
                case 7: // RGB Glitch
                    effectsManager.SetEffect(RetroEffectsManager.RetroEffectType.RGBGlitch);
                    filterIntensitySlider.Editable = true;
                    GD.Print("Filter: RGB Glitch");
                    break;
            }
        }
        
        private void OnFilterIntensityChanged(double value)
        {
            filterIntensityLabel.Text = $"{(int)value}%";
            
            if (effectsManager == null) return;
            
            int filterIndex = filterDropdown.Selected;
            float intensity = (float)value / 100f;
            
            // Apply intensity based on filter type (CRT removed)
            switch (filterIndex)
            {
                case 1: // Scanlines
                    effectsManager.SetScanlinesParameters(intensity * 0.8f, 480.0f);
                    break;
                case 2: // Dithering
                    effectsManager.SetDitheringParameters(intensity, Mathf.Max(1, (int)(4 - intensity * 3)));
                    break;
                case 3: // VHS
                    effectsManager.SetVHSDistortion(intensity * 0.05f, intensity * 0.3f);
                    break;
                case 4: // Pixelation
                    effectsManager.SetPixelationSize(Mathf.Max(1, (int)(intensity * 8)));
                    break;
                case 6: // Color Palette
                    effectsManager.SetColorDepth((int)(intensity * 256));
                    break;
                case 7: // RGB Glitch
                    effectsManager.SetRGBGlitchIntensity(intensity, intensity * 0.05f);
                    break;
            }
        }
        #endregion
        
        #region Settings Management
        private void LoadSettings()
        {
            GD.Print("Loading settings from: user://game_settings.cfg");
            
            var error = settingsConfig.Load(SETTINGS_PATH);
            
            if (error != Error.Ok)
            {
                GD.Print("No saved settings found, using defaults");
                ResetToDefaults();
                return;
            }
            
            GD.Print("✓ Settings file loaded");
            
            // Load audio settings
            bgmVolumeSlider.Value = settingsConfig.GetValue("audio", "bgm_volume", 80).AsDouble();
            sfxVolumeSlider.Value = settingsConfig.GetValue("audio", "sfx_volume", 80).AsDouble();
            
            GD.Print($"  BGM Volume: {bgmVolumeSlider.Value}%");
            GD.Print($"  SFX Volume: {sfxVolumeSlider.Value}%");
            
            // Load display settings
            bool isFullscreen = settingsConfig.GetValue("display", "fullscreen", false).AsBool();
            fullscreenCheckbox.ButtonPressed = isFullscreen;
            OnFullscreenToggled(isFullscreen);
            
            GD.Print($"  Fullscreen: {isFullscreen}");
            
            int windowSizeIndex = settingsConfig.GetValue("display", "window_size", 1).AsInt32();
            windowSizeDropdown.Selected = windowSizeIndex;
            if (!isFullscreen)
            {
                OnWindowSizeChanged(windowSizeIndex); // Apply immediately if not fullscreen
            }
            
            GD.Print($"  Window Size Index: {windowSizeIndex}");
            
            bool vsync = settingsConfig.GetValue("display", "vsync", true).AsBool();
            vsyncCheckbox.ButtonPressed = vsync;
            OnVSyncToggled(vsync);
            
            GD.Print($"  VSync: {vsync}");
            
            bool showFPS = settingsConfig.GetValue("display", "show_fps", false).AsBool();
            showFPSCheckbox.ButtonPressed = showFPS;
            OnShowFPSToggled(showFPS);
            
            GD.Print($"  Show FPS: {showFPS}");
            
            // Load filter settings
            int filterIndex = settingsConfig.GetValue("display", "filter", 0).AsInt32();
            filterDropdown.Selected = filterIndex;
            OnFilterChanged(filterIndex); // Apply immediately
            
            GD.Print($"  Filter Index: {filterIndex}");
            
            double filterIntensity = settingsConfig.GetValue("display", "filter_intensity", 50).AsDouble();
            filterIntensitySlider.Value = filterIntensity;
            OnFilterIntensityChanged(filterIntensity); // Apply immediately
            
            GD.Print($"  Filter Intensity: {filterIntensity}%");
            
            // Apply audio settings
            OnBGMVolumeChanged(bgmVolumeSlider.Value);
            OnSFXVolumeChanged(sfxVolumeSlider.Value);
            
            GD.Print("Settings loaded and applied");
        }
        
        private void SaveSettings()
        {
            // Save audio settings
            settingsConfig.SetValue("audio", "bgm_volume", bgmVolumeSlider.Value);
            settingsConfig.SetValue("audio", "sfx_volume", sfxVolumeSlider.Value);
            
            // Save display settings
            settingsConfig.SetValue("display", "fullscreen", fullscreenCheckbox.ButtonPressed);
            settingsConfig.SetValue("display", "window_size", windowSizeDropdown.Selected);
            settingsConfig.SetValue("display", "vsync", vsyncCheckbox.ButtonPressed);
            settingsConfig.SetValue("display", "show_fps", showFPSCheckbox.ButtonPressed);
            
            // Save filter settings
            settingsConfig.SetValue("display", "filter", filterDropdown.Selected);
            settingsConfig.SetValue("display", "filter_intensity", filterIntensitySlider.Value);
            
            // Write to file
            settingsConfig.Save(SETTINGS_PATH);
            GD.Print($"Settings saved to: {SETTINGS_PATH}");
        }
        
        private void ResetToDefaults()
        {
            // Clear config
            settingsConfig = new ConfigFile();
            
            // Set defaults
            settingsConfig.SetValue("audio", "bgm_volume", 80);
            settingsConfig.SetValue("audio", "sfx_volume", 80);
            settingsConfig.SetValue("display", "fullscreen", false);
            settingsConfig.SetValue("display", "window_size", 1);
            settingsConfig.SetValue("display", "vsync", true);
            settingsConfig.SetValue("display", "show_fps", false);
            settingsConfig.SetValue("display", "filter", 0);
            settingsConfig.SetValue("display", "filter_intensity", 50);
            
            // Save
            settingsConfig.Save(SETTINGS_PATH);
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
                LoadSettings();
            };
            
            AddChild(dialog);
            dialog.PopupCentered();
        }
        
        private void OnBackPressed()
        {
            CloseMenu();
            
            // Return to title screen menu
            var titleScreen = GetParent().GetNodeOrNull<TitleScreen>("%TitleScreen");
            if (titleScreen != null)
            {
                titleScreen.ReturnToMainMenu();
            }
        }
        #endregion
        
        public override void _Input(InputEvent @event)
        {
            if (!Visible) return;
            
            if (@event.IsActionPressed("ui_cancel"))
            {
                OnBackPressed();
                GetViewport().SetInputAsHandled();
            }
        }
    }
}