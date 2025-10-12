using Godot;
using System;

namespace EchoesAcrossTime.Managers
{
    /// <summary>
    /// Manages the in-game time system with day/night cycle
    /// Add as Autoload: Managers/TimeManager.cs
    /// </summary>
    public partial class TimeManager : Node
    {
        public static TimeManager Instance { get; private set; }
        
        [Signal]
        public delegate void TimeChangedEventHandler(int hour, int minute);
        
        [Signal]
        public delegate void DayChangedEventHandler(int day);
        
        [Signal]
        public delegate void TimeOfDayChangedEventHandler(int timeOfDay);
        
        public enum TimeOfDay
        {
            Dawn,      // 5:00 - 7:59
            Morning,   // 8:00 - 11:59
            Afternoon, // 12:00 - 17:59
            Evening,   // 18:00 - 20:59
            Night      // 21:00 - 4:59
        }
        
        // Time settings
        [Export] public bool TimeEnabled { get; set; } = true;
        [Export] public float TimeScale { get; set; } = 1.0f; // 1.0 = real-time, higher = faster
        [Export] public int MinutesPerRealSecond { get; set; } = 1; // How many in-game minutes pass per real second
        
        // Current time
        public int CurrentDay { get; private set; } = 1;
        public int CurrentHour { get; private set; } = 8;
        public int CurrentMinute { get; private set; } = 0;
        public TimeOfDay CurrentTimeOfDay { get; private set; } = TimeOfDay.Morning;
        
        private float timeAccumulator = 0f;
        
        public override void _Ready()
        {
            if (Instance != null)
            {
                QueueFree();
                return;
            }
            Instance = this;
            
            UpdateTimeOfDay();
            GD.Print($"[TimeManager] Initialized - Day {CurrentDay}, {CurrentHour:D2}:{CurrentMinute:D2}");
        }
        
        public override void _Process(double delta)
        {
            if (!TimeEnabled) return;
            
            timeAccumulator += (float)delta * TimeScale;
            
            // Calculate how many in-game minutes should pass
            float realSecondsPerMinute = 1.0f / MinutesPerRealSecond;
            
            while (timeAccumulator >= realSecondsPerMinute)
            {
                timeAccumulator -= realSecondsPerMinute;
                AdvanceTime(1);
            }
        }
        
        /// <summary>
        /// Advance time by specified minutes
        /// </summary>
        public void AdvanceTime(int minutes)
        {
            CurrentMinute += minutes;
            
            // Handle minute overflow
            while (CurrentMinute >= 60)
            {
                CurrentMinute -= 60;
                CurrentHour++;
            }
            
            // Handle hour overflow
            if (CurrentHour >= 24)
            {
                CurrentHour = 0;
                CurrentDay++;
                EmitSignal(SignalName.DayChanged, CurrentDay);
            }
            
            UpdateTimeOfDay();
            EmitSignal(SignalName.TimeChanged, CurrentHour, CurrentMinute);
        }
        
        /// <summary>
        /// Set time directly
        /// </summary>
        public void SetTime(int hour, int minute, int day = -1)
        {
            CurrentHour = Mathf.Clamp(hour, 0, 23);
            CurrentMinute = Mathf.Clamp(minute, 0, 59);
            
            if (day >= 1)
                CurrentDay = day;
            
            UpdateTimeOfDay();
            EmitSignal(SignalName.TimeChanged, CurrentHour, CurrentMinute);
            GD.Print($"[TimeManager] Time set to Day {CurrentDay}, {CurrentHour:D2}:{CurrentMinute:D2}");
        }
        
        private void UpdateTimeOfDay()
        {
            TimeOfDay newTimeOfDay = GetTimeOfDayFromHour(CurrentHour);
            
            if (newTimeOfDay != CurrentTimeOfDay)
            {
                CurrentTimeOfDay = newTimeOfDay;
                EmitSignal(SignalName.TimeOfDayChanged, (int)CurrentTimeOfDay);
                GD.Print($"[TimeManager] Time of day changed to: {CurrentTimeOfDay}");
            }
        }
        
        private TimeOfDay GetTimeOfDayFromHour(int hour)
        {
            return hour switch
            {
                >= 5 and < 8 => TimeOfDay.Dawn,
                >= 8 and < 12 => TimeOfDay.Morning,
                >= 12 and < 18 => TimeOfDay.Afternoon,
                >= 18 and < 21 => TimeOfDay.Evening,
                _ => TimeOfDay.Night
            };
        }
        
        /// <summary>
        /// Get formatted time string (HH:MM)
        /// </summary>
        public string GetTimeString()
        {
            return $"{CurrentHour:D2}:{CurrentMinute:D2}";
        }
        
        /// <summary>
        /// Get formatted time with period (8:30 AM)
        /// </summary>
        public string GetTimeString12Hour()
        {
            int hour12 = CurrentHour % 12;
            if (hour12 == 0) hour12 = 12;
            string period = CurrentHour < 12 ? "AM" : "PM";
            return $"{hour12}:{CurrentMinute:D2} {period}";
        }
        
        /// <summary>
        /// Check if it's currently nighttime
        /// </summary>
        public bool IsNight()
        {
            return CurrentTimeOfDay == TimeOfDay.Night;
        }
        
        /// <summary>
        /// Get ambient light level (0.0 = dark, 1.0 = bright)
        /// </summary>
        public float GetAmbientLightLevel()
        {
            return CurrentTimeOfDay switch
            {
                TimeOfDay.Dawn => 0.6f,
                TimeOfDay.Morning => 1.0f,
                TimeOfDay.Afternoon => 1.0f,
                TimeOfDay.Evening => 0.7f,
                TimeOfDay.Night => 0.3f,
                _ => 1.0f
            };
        }
    }
}