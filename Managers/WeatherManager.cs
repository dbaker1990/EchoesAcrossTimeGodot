using Godot;
using System;

namespace EchoesAcrossTime.Managers
{
    /// <summary>
    /// Manages weather states and transitions
    /// Add as Autoload: Managers/WeatherManager.cs
    /// </summary>
    public partial class WeatherManager : Node
    {
        public static WeatherManager Instance { get; private set; }
        
        [Signal]
        public delegate void WeatherChangedEventHandler(int weather);
        
        public enum WeatherType
        {
            Clear,
            Rain,
            HeavyRain,
            Snow,
            HeavySnow,
            Fog,
            Storm,
            Blizzard
        }
        
        [Export] public WeatherType CurrentWeather { get; private set; } = WeatherType.Clear;
        [Export] public bool AutoWeatherEnabled { get; set; } = true;
        [Export] public float WeatherChangeCooldown { get; set; } = 300f; // 5 minutes between weather changes
        [Export] public float TransitionDuration { get; set; } = 3.0f;
        
        private float weatherTimer = 0f;
        private WeatherType targetWeather;
        private bool isTransitioning = false;
        
        public override void _Ready()
        {
            if (Instance != null)
            {
                QueueFree();
                return;
            }
            Instance = this;
            
            targetWeather = CurrentWeather;
            GD.Print($"[WeatherManager] Initialized with weather: {CurrentWeather}");
        }
        
        public override void _Process(double delta)
        {
            if (!AutoWeatherEnabled || isTransitioning) return;
            
            weatherTimer += (float)delta;
            
            if (weatherTimer >= WeatherChangeCooldown)
            {
                weatherTimer = 0f;
                ConsiderWeatherChange();
            }
        }
        
        /// <summary>
        /// Consider changing weather based on time of day and random chance
        /// </summary>
        private void ConsiderWeatherChange()
        {
            // 30% chance to change weather
            if (GD.Randf() > 0.3f) return;
            
            var possibleWeathers = GetPossibleWeathers();
            if (possibleWeathers.Length == 0) return;
            
            var newWeather = possibleWeathers[GD.Randi() % possibleWeathers.Length];
            
            if (newWeather != CurrentWeather)
            {
                ChangeWeather(newWeather);
            }
        }
        
        /// <summary>
        /// Get possible weather types based on current conditions
        /// </summary>
        private WeatherType[] GetPossibleWeathers()
        {
            bool isNight = TimeManager.Instance?.IsNight() ?? false;
            
            // Weight weather based on current weather (make similar weather more likely)
            return CurrentWeather switch
            {
                WeatherType.Clear => new[] { WeatherType.Clear, WeatherType.Clear, WeatherType.Fog, WeatherType.Rain },
                WeatherType.Rain => new[] { WeatherType.Rain, WeatherType.HeavyRain, WeatherType.Clear, WeatherType.Storm },
                WeatherType.HeavyRain => new[] { WeatherType.HeavyRain, WeatherType.Storm, WeatherType.Rain },
                WeatherType.Storm => new[] { WeatherType.Storm, WeatherType.HeavyRain, WeatherType.Rain },
                WeatherType.Snow => new[] { WeatherType.Snow, WeatherType.HeavySnow, WeatherType.Clear },
                WeatherType.HeavySnow => new[] { WeatherType.HeavySnow, WeatherType.Blizzard, WeatherType.Snow },
                WeatherType.Blizzard => new[] { WeatherType.Blizzard, WeatherType.HeavySnow, WeatherType.Snow },
                WeatherType.Fog => new[] { WeatherType.Fog, WeatherType.Clear, WeatherType.Rain },
                _ => new[] { WeatherType.Clear }
            };
        }
        
        /// <summary>
        /// Change weather immediately
        /// </summary>
        public void ChangeWeather(WeatherType newWeather, bool instant = false)
        {
            if (newWeather == CurrentWeather && !instant)
                return;
            
            GD.Print($"[WeatherManager] Changing weather from {CurrentWeather} to {newWeather}");
            
            if (instant)
            {
                CurrentWeather = newWeather;
                EmitSignal(SignalName.WeatherChanged, (int)newWeather);
                return;
            }
            
            // Smooth transition
            targetWeather = newWeather;
            isTransitioning = true;
            
            var tween = CreateTween();
            tween.TweenCallback(Callable.From(() =>
            {
                CurrentWeather = targetWeather;
                EmitSignal(SignalName.WeatherChanged, (int)CurrentWeather);
            }));
            tween.TweenInterval(TransitionDuration);
            tween.TweenCallback(Callable.From(() => isTransitioning = false));
        }
        
        /// <summary>
        /// Set weather based on story requirements
        /// </summary>
        public void SetStoryWeather(WeatherType weather)
        {
            AutoWeatherEnabled = false;
            ChangeWeather(weather, false);
        }
        
        /// <summary>
        /// Resume automatic weather changes
        /// </summary>
        public void ResumeAutoWeather()
        {
            AutoWeatherEnabled = true;
            weatherTimer = 0f;
        }
        
        /// <summary>
        /// Check if current weather is a storm type
        /// </summary>
        public bool IsStormy()
        {
            return CurrentWeather == WeatherType.Storm || 
                   CurrentWeather == WeatherType.HeavyRain || 
                   CurrentWeather == WeatherType.Blizzard;
        }
        
        /// <summary>
        /// Check if current weather reduces visibility
        /// </summary>
        public bool HasReducedVisibility()
        {
            return CurrentWeather == WeatherType.Fog || 
                   CurrentWeather == WeatherType.HeavySnow || 
                   CurrentWeather == WeatherType.Blizzard || 
                   CurrentWeather == WeatherType.Storm;
        }
        
        /// <summary>
        /// Get weather intensity (0.0 - 1.0)
        /// </summary>
        public float GetWeatherIntensity()
        {
            return CurrentWeather switch
            {
                WeatherType.Clear => 0.0f,
                WeatherType.Fog => 0.3f,
                WeatherType.Rain => 0.5f,
                WeatherType.Snow => 0.5f,
                WeatherType.HeavyRain => 0.8f,
                WeatherType.HeavySnow => 0.8f,
                WeatherType.Storm => 1.0f,
                WeatherType.Blizzard => 1.0f,
                _ => 0.0f
            };
        }
    }
}