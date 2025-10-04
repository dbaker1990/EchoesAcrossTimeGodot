// Skits/SkitManager.cs
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EchoesAcrossTime.Skits
{
    /// <summary>
    /// Manages skit availability, triggering, and playback
    /// </summary>
    public partial class SkitManager : Node
    {
        // Singleton
        public static SkitManager Instance { get; private set; }

        // Skit database
        private Dictionary<string, SkitData> allSkits = new();
        private List<string> viewedSkits = new();
        private List<string> availableSkits = new();
        
        // References
        private SkitUI skitUI;
        
        // State
        private bool isPlayingSkit = false;
        private SkitData currentSkit;
        
        // Signals
        [Signal]
        public delegate void SkitStartedEventHandler(string skitId);
        
        [Signal]
        public delegate void SkitEndedEventHandler(string skitId);
        
        [Signal]
        public delegate void SkitAvailableEventHandler(string skitId);

        public override void _Ready()
        {
            if (Instance != null)
            {
                QueueFree();
                return;
            }
            Instance = this;
            
            // Find or create SkitUI
            skitUI = GetNodeOrNull<SkitUI>("%SkitUI");
            if (skitUI == null)
            {
                GD.PrintErr("SkitUI not found! Please add SkitUI node to scene.");
            }
        }

        /// <summary>
        /// Register a skit to the database
        /// </summary>
        public void RegisterSkit(SkitData skit)
        {
            if (skit == null || string.IsNullOrEmpty(skit.SkitId))
            {
                GD.PrintErr("Cannot register null or invalid skit");
                return;
            }

            allSkits[skit.SkitId] = skit;
            GD.Print($"Registered skit: {skit.SkitId} - {skit.SkitTitle}");
        }

        /// <summary>
        /// Register multiple skits from a folder
        /// </summary>
        public void LoadSkitsFromFolder(string folderPath)
        {
            var dir = DirAccess.Open(folderPath);
            if (dir == null)
            {
                GD.PrintErr($"Failed to open skit folder: {folderPath}");
                return;
            }

            dir.ListDirBegin();
            string fileName = dir.GetNext();
            
            while (!string.IsNullOrEmpty(fileName))
            {
                if (!dir.CurrentIsDir() && fileName.EndsWith(".tres"))
                {
                    var skit = GD.Load<SkitData>($"{folderPath}/{fileName}");
                    if (skit != null)
                    {
                        RegisterSkit(skit);
                    }
                }
                fileName = dir.GetNext();
            }
            
            dir.ListDirEnd();
        }

        /// <summary>
        /// Check and update available skits based on current game state
        /// </summary>
        public void UpdateAvailableSkits(List<string> currentParty, int currentChapter, List<string> clearedFlags)
        {
            availableSkits.Clear();

            foreach (var kvp in allSkits)
            {
                var skit = kvp.Value;
                
                // Skip if already viewed and once-only
                if (skit.OnceOnly && viewedSkits.Contains(skit.SkitId))
                    continue;

                // Check if skit can trigger
                if (skit.CanTrigger(currentParty, currentChapter, clearedFlags))
                {
                    availableSkits.Add(skit.SkitId);
                    EmitSignal(SignalName.SkitAvailable, skit.SkitId);
                }
            }
        }

        /// <summary>
        /// Get all currently available skits
        /// </summary>
        public List<SkitData> GetAvailableSkits()
        {
            return availableSkits
                .Select(id => allSkits.GetValueOrDefault(id))
                .Where(skit => skit != null)
                .ToList();
        }

        /// <summary>
        /// Play a specific skit
        /// </summary>
        public async Task PlaySkit(string skitId)
        {
            if (isPlayingSkit)
            {
                GD.PrintErr("Cannot play skit - another skit is already playing");
                return;
            }

            if (!allSkits.TryGetValue(skitId, out var skit))
            {
                GD.PrintErr($"Skit not found: {skitId}");
                return;
            }

            await PlaySkit(skit);
        }

        /// <summary>
        /// Play a skit directly
        /// </summary>
        public async Task PlaySkit(SkitData skit)
        {
            if (skit == null || skitUI == null)
                return;

            isPlayingSkit = true;
            currentSkit = skit;
            
            EmitSignal(SignalName.SkitStarted, skit.SkitId);
            GD.Print($"Playing skit: {skit.SkitTitle}");

            // Play the skit through the UI
            await skitUI.PlaySkit(skit);

            // Mark as viewed
            if (!viewedSkits.Contains(skit.SkitId))
            {
                viewedSkits.Add(skit.SkitId);
            }

            isPlayingSkit = false;
            currentSkit = null;
            
            EmitSignal(SignalName.SkitEnded, skit.SkitId);
        }

        /// <summary>
        /// Check if a skit has been viewed
        /// </summary>
        public bool HasViewedSkit(string skitId)
        {
            return viewedSkits.Contains(skitId);
        }

        /// <summary>
        /// Get skit by ID
        /// </summary>
        public SkitData GetSkit(string skitId)
        {
            return allSkits.GetValueOrDefault(skitId);
        }

        /// <summary>
        /// Skip current skit
        /// </summary>
        public void SkipCurrentSkit()
        {
            if (isPlayingSkit && skitUI != null)
            {
                skitUI.SkipSkit();
            }
        }

        /// <summary>
        /// Save/Load functionality
        /// </summary>
        public Dictionary<string, object> SaveData()
        {
            return new Dictionary<string, object>
            {
                { "viewed_skits", viewedSkits }
            };
        }

        public void LoadData(Dictionary<string, object> data)
        {
            if (data.TryGetValue("viewed_skits", out var viewed) && viewed is List<string> skitList)
            {
                viewedSkits = skitList;
            }
        }
    }
}