using Godot;
using System;
using EchoesAcrossTime.Managers;

namespace EchoesAcrossTime.Environment
{
    /// <summary>
    /// Handles day/night visual effects with ambient lighting
    /// Attach to a CanvasModulate node in your scene
    /// </summary>
    public partial class DayNightCycle : CanvasModulate
    {
        // Color presets for different times of day
        [ExportGroup("Time of Day Colors")]
        [Export] public Color DawnColor { get; set; } = new Color(0.9f, 0.7f, 0.6f, 1.0f);
        [Export] public Color MorningColor { get; set; } = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        [Export] public Color AfternoonColor { get; set; } = new Color(1.0f, 0.95f, 0.9f, 1.0f);
        [Export] public Color EveningColor { get; set; } = new Color(0.8f, 0.6f, 0.5f, 1.0f);
        [Export] public Color NightColor { get; set; } = new Color(0.3f, 0.35f, 0.5f, 1.0f);
        
        [ExportGroup("Settings")]
        [Export] public bool SmoothTransitions { get; set; } = true;
        [Export] public float TransitionSpeed { get; set; } = 0.5f;
        
        private Color targetColor;
        private Color currentColor;
        
        public override void _Ready()
        {
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.TimeOfDayChanged += OnTimeOfDayChanged;
                
                // Set initial color based on current time
                UpdateColorForTimeOfDay(TimeManager.Instance.CurrentTimeOfDay);
                Color = currentColor;
            }
            else
            {
                GD.PrintErr("[DayNightCycle] TimeManager not found!");
            }
            
            GD.Print("[DayNightCycle] Initialized");
        }
        
        private void OnTimeOfDayChanged(int timeOfDay)
        {
            var tod = (TimeManager.TimeOfDay)timeOfDay;
            UpdateColorForTimeOfDay(tod);
        }
        
        private void UpdateColorForTimeOfDay(TimeManager.TimeOfDay timeOfDay)
        {
            targetColor = timeOfDay switch
            {
                TimeManager.TimeOfDay.Dawn => DawnColor,
                TimeManager.TimeOfDay.Morning => MorningColor,
                TimeManager.TimeOfDay.Afternoon => AfternoonColor,
                TimeManager.TimeOfDay.Evening => EveningColor,
                TimeManager.TimeOfDay.Night => NightColor,
                _ => MorningColor
            };
            
            if (!SmoothTransitions)
            {
                currentColor = targetColor;
                Color = targetColor;
            }
        }
        
        public override void _Process(double delta)
        {
            if (!SmoothTransitions || TimeManager.Instance == null)
                return;
            
            // Smoothly interpolate to target color
            if (!ColorsAreClose(currentColor, targetColor))
            {
                currentColor = currentColor.Lerp(targetColor, TransitionSpeed * (float)delta);
                Color = currentColor;
            }
        }
        
        /// <summary>
        /// Check if two colors are close enough (within threshold)
        /// </summary>
        private bool ColorsAreClose(Color a, Color b, float threshold = 0.01f)
        {
            return Mathf.Abs(a.R - b.R) < threshold &&
                   Mathf.Abs(a.G - b.G) < threshold &&
                   Mathf.Abs(a.B - b.B) < threshold &&
                   Mathf.Abs(a.A - b.A) < threshold;
        }
        
        /// <summary>
        /// Manually set the ambient color (for cutscenes, etc.)
        /// </summary>
        public void SetAmbientColor(Color color, float duration = 1.0f)
        {
            if (duration <= 0)
            {
                Color = color;
                currentColor = color;
                targetColor = color;
            }
            else
            {
                var tween = CreateTween();
                tween.TweenProperty(this, "color", color, duration);
            }
        }
        
        public override void _ExitTree()
        {
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.TimeOfDayChanged -= OnTimeOfDayChanged;
            }
        }
    }
}