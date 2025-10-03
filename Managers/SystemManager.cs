using Godot;
using System;
using EchoesAcrossTime.Database;
using EchoesAcrossTime.Combat;

namespace EchoesAcrossTime.Managers
{
    /// <summary>
    /// Manages system-wide settings and configuration from SystemDatabase
    /// Singleton that loads and applies global game settings
    /// </summary>
    public partial class SystemManager : Node
    {
        public static SystemManager Instance { get; private set; }

        [ExportGroup("System Configuration")]
        [Export] public SystemDatabase SystemData { get; set; }
        [Export] public string SystemDatabasePath { get; set; } = "res://Database/SystemDatabase.tres";

        // Quick access properties
        public string GameTitle => SystemData?.GameTitle ?? "Game";
        public string GameVersion => SystemData?.GameVersion ?? "1.0.0";

        public override void _Ready()
        {
            // Singleton pattern
            if (Instance != null)
            {
                GD.PrintErr("SystemManager: Multiple instances detected! Removing duplicate.");
                QueueFree();
                return;
            }

            Instance = this;

            // Load system database if not assigned
            if (SystemData == null)
            {
                LoadSystemDatabase();
            }

            if (SystemData != null)
            {
                ApplySystemSettings();
                GD.Print($"SystemManager initialized: {GameTitle} v{GameVersion}");
            }
            else
            {
                GD.PrintErr("SystemManager: Failed to load SystemDatabase!");
            }
        }

        /// <summary>
        /// Load the SystemDatabase resource
        /// </summary>
        public void LoadSystemDatabase()
        {
            if (string.IsNullOrEmpty(SystemDatabasePath))
            {
                GD.PrintErr("SystemManager: SystemDatabasePath is not set!");
                return;
            }

            SystemData = GD.Load<SystemDatabase>(SystemDatabasePath);

            if (SystemData == null)
            {
                GD.PrintErr($"SystemManager: Failed to load SystemDatabase from {SystemDatabasePath}");
            }
            else
            {
                GD.Print($"SystemManager: Loaded SystemDatabase from {SystemDatabasePath}");
            }
        }

        /// <summary>
        /// Apply all system settings (audio volumes, window title, etc.)
        /// </summary>
        public void ApplySystemSettings()
        {
            if (SystemData == null) return;

            // Set window title
            DisplayServer.WindowSetTitle(SystemData.GameTitle);

            // Apply audio volume settings
            ApplyVolumeSettings();

            GD.Print("SystemManager: Applied system settings");
        }

        /// <summary>
        /// Apply volume settings to AudioManager
        /// </summary>
        public void ApplyVolumeSettings()
        {
            if (SystemData == null) return;

            // Convert from 0-100 range to decibels
            // 100 = 0dB (full volume), 0 = -80dB (muted)
            SetBusVolume("BGM", SystemData.DefaultBGMVolume);
            SetBusVolume("SFX", SystemData.DefaultSEVolume);
            SetBusVolume("UI", SystemData.DefaultSEVolume); // UI uses same as SFX
        }

        /// <summary>
        /// Set volume for a specific audio bus
        /// </summary>
        private void SetBusVolume(string busName, float volumePercent)
        {
            int busIndex = AudioServer.GetBusIndex(busName);
            if (busIndex == -1)
            {
                GD.PrintErr($"Audio bus '{busName}' not found!");
                return;
            }

            // Convert 0-100 percentage to decibels
            // 0% = -80dB (effectively muted)
            // 100% = 0dB (full volume)
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

        #region Music Playback Helpers
        /// <summary>
        /// Play title screen music
        /// </summary>
        public void PlayTitleMusic()
        {
            if (SystemData?.TitleMusic != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayBGM(SystemData.TitleMusic);
            }
        }

        /// <summary>
        /// Play battle music
        /// </summary>
        public void PlayBattleMusic()
        {
            if (SystemData?.BattleMusic != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayBGM(SystemData.BattleMusic);
            }
        }

        /// <summary>
        /// Play victory music
        /// </summary>
        public void PlayVictoryMusic()
        {
            if (SystemData?.VictoryMusic != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayBGM(SystemData.VictoryMusic, fadeInDuration: 0.5f);
            }
        }

        /// <summary>
        /// Play game over music
        /// </summary>
        public void PlayGameOverMusic()
        {
            if (SystemData?.GameOverMusic != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayBGM(SystemData.GameOverMusic);
            }
        }

        /// <summary>
        /// Play vehicle music by type
        /// </summary>
        public void PlayVehicleMusic(VehicleType vehicleType)
        {
            if (AudioManager.Instance == null) return;

            AudioStream music = vehicleType switch
            {
                VehicleType.Boat => SystemData?.BoatMusic,
                VehicleType.Ship => SystemData?.ShipMusic,
                VehicleType.Airship => SystemData?.AirshipMusic,
                _ => null
            };

            if (music != null)
            {
                AudioManager.Instance.PlayBGM(music);
            }
        }
        #endregion

        #region Sound Effect Playback Helpers
        /// <summary>
        /// Play a system sound effect
        /// </summary>
        public void PlaySystemSE(SystemSoundEffect seType)
        {
            SystemData?.PlaySystemSE(seType);
        }

        // Convenience methods for common UI sounds
        public void PlayCursorSE() => PlaySystemSE(SystemSoundEffect.Cursor);
        public void PlayOkSE() => PlaySystemSE(SystemSoundEffect.Ok);
        public void PlayCancelSE() => PlaySystemSE(SystemSoundEffect.Cancel);
        public void PlayBuzzerSE() => PlaySystemSE(SystemSoundEffect.Buzzer);
        public void PlayEquipSE() => PlaySystemSE(SystemSoundEffect.Equip);
        public void PlaySaveSE() => PlaySystemSE(SystemSoundEffect.Save);
        #endregion

        #region Damage Calculation
        /// <summary>
        /// Calculate damage using system settings
        /// This respects the global formula override if enabled
        /// </summary>
        public int CalculateDamage(CharacterStats attacker, CharacterStats target, SkillData skill, bool isCritical = false)
        {
            if (SystemData == null)
            {
                GD.PrintErr("SystemManager: SystemData is null, cannot calculate damage");
                return 0;
            }

            return SystemData.CalculateDamage(attacker, target, skill, isCritical);
        }

        /// <summary>
        /// Check if critical hits are enabled
        /// </summary>
        public bool AreCriticalHitsEnabled()
        {
            return SystemData?.UseCriticalHits ?? true;
        }

        /// <summary>
        /// Get the critical hit multiplier
        /// </summary>
        public float GetCriticalMultiplier()
        {
            return SystemData?.CriticalMultiplier ?? 2.0f;
        }

        /// <summary>
        /// Check if elemental system is enabled
        /// </summary>
        public bool IsElementalSystemEnabled()
        {
            return SystemData?.UseElementalSystem ?? true;
        }
        #endregion

        #region New Game Initialization
        /// <summary>
        /// Initialize a new game using system settings
        /// </summary>
        public void InitializeNewGame(SaveData saveData)
        {
            if (SystemData == null)
            {
                GD.PrintErr("SystemManager: Cannot initialize new game, SystemData is null");
                return;
            }

            if (GameManager.Instance?.Database == null)
            {
                GD.PrintErr("SystemManager: Cannot initialize new game, GameDatabase is null");
                return;
            }

            SystemData.InitializeNewGame(saveData, GameManager.Instance.Database);
        }

        /// <summary>
        /// Get starting party IDs
        /// </summary>
        public string[] GetStartingPartyIds()
        {
            return SystemData?.StartingPartyIds ?? new string[0];
        }

        /// <summary>
        /// Get starting gold
        /// </summary>
        public int GetStartingGold()
        {
            return SystemData?.StartingGold ?? 0;
        }
        #endregion

        #region Utility Methods
        /// <summary>
        /// Reload system database (useful for testing/debugging)
        /// </summary>
        public void ReloadSystemDatabase()
        {
            LoadSystemDatabase();
            if (SystemData != null)
            {
                ApplySystemSettings();
                GD.Print("SystemManager: System database reloaded");
            }
        }

        /// <summary>
        /// Get the active damage formula description
        /// </summary>
        public string GetActiveFormulaDescription()
        {
            if (SystemData == null) return "No system data loaded";

            if (SystemData.UseGlobalFormulaOverride)
            {
                return $"Global Override: {DamageFormula.GetFormulaDescription(SystemData.GlobalDamageFormula)}";
            }
            else
            {
                return "Using per-skill formulas";
            }
        }
        #endregion
    }

    /// <summary>
    /// Vehicle types for music selection
    /// </summary>
    public enum VehicleType
    {
        Boat,
        Ship,
        Airship
    }
}