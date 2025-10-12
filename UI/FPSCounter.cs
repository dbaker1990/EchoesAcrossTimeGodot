using Godot;
using System;

namespace EchoesAcrossTime.UI
{
    /// <summary>
    /// FPS Counter display for monitoring game performance
    /// Shows current FPS, average FPS, and frame time
    /// </summary>
    public partial class FPSCounter : CanvasLayer
    {
        private Label fpsLabel;
        private Panel backgroundPanel;
        
        // Performance tracking
        private double[] frameTimes;
        private int frameIndex = 0;
        private const int SAMPLE_COUNT = 60; // Average over 60 frames
        private double updateTimer = 0;
        private const double UPDATE_INTERVAL = 0.25; // Update 4 times per second
        
        // Color thresholds
        private readonly Color colorGood = new Color(0.2f, 0.8f, 0.2f); // Green (60+ FPS)
        private readonly Color colorOk = new Color(0.8f, 0.8f, 0.2f);   // Yellow (30-60 FPS)
        private readonly Color colorBad = new Color(0.8f, 0.2f, 0.2f);  // Red (<30 FPS)
        
        public override void _Ready()
        {
            // Initialize frame time array
            frameTimes = new double[SAMPLE_COUNT];
            
            // Create UI
            CreateUI();
            
            // Load visibility setting
            LoadSettings();
            
            // Set layer to be on top of everything
            Layer = 100;
        }
        
        private void CreateUI()
        {
            // Create background panel
            backgroundPanel = new Panel();
            backgroundPanel.Position = new Vector2(10, 10);
            backgroundPanel.CustomMinimumSize = new Vector2(200, 80);
            
            // Style the panel
            var styleBox = new StyleBoxFlat();
            styleBox.BgColor = new Color(0, 0, 0, 0.7f);
            styleBox.BorderColor = new Color(1, 1, 1, 0.3f);
            styleBox.BorderWidthRight = 2;
            styleBox.BorderWidthBottom = 2;
            styleBox.BorderWidthLeft = 2;
            styleBox.BorderWidthTop = 2;
            styleBox.CornerRadiusBottomLeft = 4;
            styleBox.CornerRadiusBottomRight = 4;
            styleBox.CornerRadiusTopLeft = 4;
            styleBox.CornerRadiusTopRight = 4;
            backgroundPanel.AddThemeStyleboxOverride("panel", styleBox);
            
            AddChild(backgroundPanel);
            
            // Create label
            fpsLabel = new Label();
            fpsLabel.Position = new Vector2(10, 10);
            fpsLabel.Size = new Vector2(180, 60);
            fpsLabel.AutowrapMode = TextServer.AutowrapMode.Off;
            
            // Style the label
            fpsLabel.AddThemeFontSizeOverride("font_size", 14);
            fpsLabel.AddThemeColorOverride("font_outline_color", Colors.Black);
            fpsLabel.AddThemeConstantOverride("outline_size", 2);
            
            backgroundPanel.AddChild(fpsLabel);
        }
        
        public override void _Process(double delta)
        {
            // Record frame time
            frameTimes[frameIndex] = delta;
            frameIndex = (frameIndex + 1) % SAMPLE_COUNT;
            
            // Update display
            updateTimer += delta;
            if (updateTimer >= UPDATE_INTERVAL)
            {
                updateTimer = 0;
                UpdateDisplay();
            }
        }
        
        private void UpdateDisplay()
        {
            // Calculate current FPS
            double currentFPS = Engine.GetFramesPerSecond();
            
            // Calculate average FPS from frame times
            double totalTime = 0;
            int validSamples = 0;
            
            for (int i = 0; i < SAMPLE_COUNT; i++)
            {
                if (frameTimes[i] > 0)
                {
                    totalTime += frameTimes[i];
                    validSamples++;
                }
            }
            
            double avgFPS = validSamples > 0 ? validSamples / totalTime : 0;
            
            // Calculate frame time in milliseconds
            double frameTimeMs = (totalTime / validSamples) * 1000.0;
            
            // Update label text
            fpsLabel.Text = $"FPS: {currentFPS:F0}\n" +
                          $"Avg: {avgFPS:F0}\n" +
                          $"Frame: {frameTimeMs:F2}ms";
            
            // Update color based on FPS
            Color fpsColor;
            if (currentFPS >= 60)
            {
                fpsColor = colorGood;
            }
            else if (currentFPS >= 30)
            {
                fpsColor = colorOk;
            }
            else
            {
                fpsColor = colorBad;
            }
            
            fpsLabel.AddThemeColorOverride("font_color", fpsColor);
        }
        
        private void LoadSettings()
        {
            var config = new ConfigFile();
            var error = config.Load("user://game_settings.cfg");
            
            if (error == Error.Ok)
            {
                bool showFPS = config.GetValue("display", "show_fps", false).AsBool();
                Visible = showFPS;
            }
            else
            {
                Visible = false;
            }
        }
        
        /// <summary>
        /// Toggle FPS counter visibility
        /// </summary>
        public void Toggle()
        {
            Visible = !Visible;
        }
        
        /// <summary>
        /// Set FPS counter visibility
        /// </summary>
        public void SetCounterVisible(bool visible)
        {
            Visible = visible;
        }
    }
}