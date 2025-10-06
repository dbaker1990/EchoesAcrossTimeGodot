// LoreCodex/LoreCodexManager.cs
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using EchoesAcrossTime.Database;

namespace EchoesAcrossTime.LoreCodex
{
    /// <summary>
    /// Manages the lore codex - tracks discovered entries and provides access
    /// ADD TO PROJECT SETTINGS -> AUTOLOAD as "LoreCodexManager"
    /// </summary>
    public partial class LoreCodexManager : Node
    {
        public static LoreCodexManager Instance { get; private set; }

        #region Signals
        [Signal] public delegate void LoreEntryDiscoveredEventHandler(string entryId);
        [Signal] public delegate void LoreSectionUnlockedEventHandler(string entryId, int sectionIndex);
        [Signal] public delegate void CodexUpdatedEventHandler();
        [Signal] public delegate void EntryReadEventHandler(string entryId);
        #endregion

        #region Data
        // All lore entries in the game
        private Dictionary<string, LoreEntryData> allLoreEntries = new();
        
        // Player's discovered entries
        private Dictionary<string, LoreCodexEntry> discoveredEntries = new();
        #endregion

        #region Properties
        public int TotalEntriesInGame => allLoreEntries.Count;
        public int TotalDiscovered => discoveredEntries.Values.Count(e => e.IsDiscovered);
        public int TotalUnread => discoveredEntries.Values.Count(e => e.IsDiscovered && !e.HasBeenRead);
        public float CompletionPercentage => TotalEntriesInGame > 0 
            ? (float)TotalDiscovered / TotalEntriesInGame * 100f 
            : 0f;
        #endregion

        public override void _EnterTree()
        {
            if (Instance != null)
            {
                QueueFree();
                return;
            }
            Instance = this;
        }

        public override void _Ready()
        {
            GD.Print("LoreCodexManager initialized");
        }

        #region Initialization
        /// <summary>
        /// Load lore entries from a folder containing .tres files
        /// </summary>
        public void LoadLoreEntriesFromFolder(string folderPath)
        {
            var dir = DirAccess.Open(folderPath);
            if (dir == null)
            {
                GD.PrintErr($"LoreCodexManager: Could not open folder {folderPath}");
                return;
            }

            allLoreEntries.Clear();
            dir.ListDirBegin();
            
            string fileName = dir.GetNext();
            while (fileName != "")
            {
                if (!dir.CurrentIsDir() && fileName.EndsWith(".tres"))
                {
                    string fullPath = $"{folderPath}/{fileName}";
                    var entry = GD.Load<LoreEntryData>(fullPath);
                    
                    if (entry != null && !string.IsNullOrEmpty(entry.EntryId))
                    {
                        allLoreEntries[entry.EntryId] = entry;
                        
                        // Create codex entry if doesn't exist
                        if (!discoveredEntries.ContainsKey(entry.EntryId))
                        {
                            discoveredEntries[entry.EntryId] = new LoreCodexEntry
                            {
                                EntryId = entry.EntryId,
                                IsDiscovered = entry.StartsUnlocked
                            };
                        }
                    }
                }
                fileName = dir.GetNext();
            }
            
            dir.ListDirEnd();
            GD.Print($"LoreCodexManager: Loaded {allLoreEntries.Count} lore entries from {folderPath}");
        }

        /// <summary>
        /// Initialize from game database (if lore entries are stored there)
        /// </summary>
        public void InitializeFromDatabase(GameDatabase database)
        {
            if (database == null) return;

            allLoreEntries.Clear();
            
            // You can add lore entries to your GameDatabase if you want
            // For now, this is a placeholder for custom initialization
            
            GD.Print($"LoreCodexManager: Initialized from database");
        }

        /// <summary>
        /// Register a single lore entry
        /// </summary>
        public void RegisterLoreEntry(LoreEntryData entry)
        {
            if (entry == null || string.IsNullOrEmpty(entry.EntryId))
            {
                GD.PrintErr("LoreCodexManager: Cannot register null or invalid entry");
                return;
            }

            allLoreEntries[entry.EntryId] = entry;
            
            if (!discoveredEntries.ContainsKey(entry.EntryId))
            {
                discoveredEntries[entry.EntryId] = new LoreCodexEntry
                {
                    EntryId = entry.EntryId,
                    IsDiscovered = entry.StartsUnlocked
                };
            }
        }
        #endregion

        #region Discovery System
        /// <summary>
        /// Unlock a lore entry for the player
        /// </summary>
        public void DiscoverEntry(string entryId, bool silent = false)
        {
            if (string.IsNullOrEmpty(entryId))
                return;

            var codexEntry = GetOrCreateCodexEntry(entryId);
            
            if (codexEntry.IsDiscovered)
            {
                // Already discovered
                return;
            }

            codexEntry.IsDiscovered = true;
            codexEntry.DiscoveredDate = DateTime.Now;

            if (!silent)
            {
                EmitSignal(SignalName.LoreEntryDiscovered, entryId);
            }
            
            EmitSignal(SignalName.CodexUpdated);
            
            var entryData = GetLoreEntry(entryId);
            GD.Print($"Lore Entry Discovered: {entryData?.EntryName ?? entryId}");
        }

        /// <summary>
        /// Unlock a specific section of an entry
        /// </summary>
        public void UnlockSection(string entryId, int sectionIndex)
        {
            var codexEntry = GetOrCreateCodexEntry(entryId);
            var entryData = GetLoreEntry(entryId);

            if (entryData == null || sectionIndex < 0 || sectionIndex >= entryData.Sections.Count)
                return;

            if (!codexEntry.UnlockedSectionIndices.Contains(sectionIndex))
            {
                codexEntry.UnlockedSectionIndices.Add(sectionIndex);
                EmitSignal(SignalName.LoreSectionUnlocked, entryId, sectionIndex);
                EmitSignal(SignalName.CodexUpdated);
            }
        }

        /// <summary>
        /// Mark an entry as read
        /// </summary>
        public void MarkAsRead(string entryId)
        {
            var codexEntry = GetOrCreateCodexEntry(entryId);
            
            if (!codexEntry.HasBeenRead)
            {
                codexEntry.HasBeenRead = true;
                codexEntry.TimesViewed++;
                EmitSignal(SignalName.EntryRead, entryId);
            }
            else
            {
                codexEntry.TimesViewed++;
            }
        }

        /// <summary>
        /// Check unlock conditions and auto-discover entries
        /// Call this when switches/quests change
        /// </summary>
        public void CheckUnlockConditions()
        {
            foreach (var kvp in allLoreEntries)
            {
                var entryId = kvp.Key;
                var entryData = kvp.Value;
                var codexEntry = GetOrCreateCodexEntry(entryId);

                if (codexEntry.IsDiscovered)
                    continue;

                // Check switch condition
                if (!string.IsNullOrEmpty(entryData.UnlockSwitchId))
                {
                    if (GameManager.Instance?.CurrentSave?.Switches.ContainsKey(entryData.UnlockSwitchId) == true)
                    {
                        if (GameManager.Instance.CurrentSave.Switches[entryData.UnlockSwitchId])
                        {
                            DiscoverEntry(entryId);
                        }
                    }
                }

                // Check quest condition
                if (!string.IsNullOrEmpty(entryData.UnlockQuestId))
                {
                    // Check if quest is completed
                    // You'll need to implement this based on your QuestManager
                    // Example:
                    // if (QuestManager.Instance?.IsQuestCompleted(entryData.UnlockQuestId) == true)
                    // {
                    //     DiscoverEntry(entryId);
                    // }
                }
            }
        }
        #endregion

        #region Data Access
        /// <summary>
        /// Get lore entry data by ID
        /// </summary>
        public LoreEntryData GetLoreEntry(string entryId)
        {
            return allLoreEntries.TryGetValue(entryId, out var entry) ? entry : null;
        }

        /// <summary>
        /// Get player's codex entry (discovery status)
        /// </summary>
        public LoreCodexEntry GetCodexEntry(string entryId)
        {
            return discoveredEntries.TryGetValue(entryId, out var entry) ? entry : null;
        }

        /// <summary>
        /// Check if entry is discovered
        /// </summary>
        public bool IsDiscovered(string entryId)
        {
            var entry = GetCodexEntry(entryId);
            return entry?.IsDiscovered ?? false;
        }

        /// <summary>
        /// Check if entry has been read
        /// </summary>
        public bool HasBeenRead(string entryId)
        {
            var entry = GetCodexEntry(entryId);
            return entry?.HasBeenRead ?? false;
        }

        /// <summary>
        /// Get all lore entries (with discovery status)
        /// </summary>
        public List<(LoreEntryData data, LoreCodexEntry status)> GetAllEntries()
        {
            var result = new List<(LoreEntryData, LoreCodexEntry)>();
            
            foreach (var kvp in allLoreEntries)
            {
                var codexEntry = GetOrCreateCodexEntry(kvp.Key);
                result.Add((kvp.Value, codexEntry));
            }
            
            return result;
        }

        private LoreCodexEntry GetOrCreateCodexEntry(string entryId)
        {
            if (!discoveredEntries.ContainsKey(entryId))
            {
                discoveredEntries[entryId] = new LoreCodexEntry
                {
                    EntryId = entryId,
                    IsDiscovered = false
                };
            }
            return discoveredEntries[entryId];
        }
        #endregion

        #region Filtering & Sorting
        /// <summary>
        /// Get filtered lore entries
        /// </summary>
        public List<(LoreEntryData data, LoreCodexEntry status)> GetFilteredEntries(
            LoreFilterType filterType, 
            LoreCategory? category = null,
            string era = null,
            string location = null)
        {
            var allEntries = GetAllEntries();

            switch (filterType)
            {
                case LoreFilterType.All:
                    return allEntries;

                case LoreFilterType.Discovered:
                    return allEntries.Where(e => e.status.IsDiscovered).ToList();

                case LoreFilterType.Unread:
                    return allEntries.Where(e => e.status.IsDiscovered && !e.status.HasBeenRead).ToList();

                case LoreFilterType.ByCategory:
                    if (category.HasValue)
                        return allEntries.Where(e => e.data.Category == category.Value).ToList();
                    return allEntries;

                case LoreFilterType.ByEra:
                    if (!string.IsNullOrEmpty(era))
                        return allEntries.Where(e => e.data.Era == era).ToList();
                    return allEntries;

                case LoreFilterType.ByLocation:
                    if (!string.IsNullOrEmpty(location))
                        return allEntries.Where(e => e.data.Location == location).ToList();
                    return allEntries;

                case LoreFilterType.Recent:
                    return allEntries
                        .Where(e => e.status.IsDiscovered)
                        .OrderByDescending(e => e.status.DiscoveredDate)
                        .Take(10)
                        .ToList();

                default:
                    return allEntries;
            }
        }

        /// <summary>
        /// Get sorted lore entries
        /// </summary>
        public List<(LoreEntryData data, LoreCodexEntry status)> GetSortedEntries(
            LoreSortType sortType, 
            bool descending = false)
        {
            var entries = GetAllEntries();

            IOrderedEnumerable<(LoreEntryData data, LoreCodexEntry status)> sorted = sortType switch
            {
                LoreSortType.ByName => descending 
                    ? entries.OrderByDescending(e => e.data.EntryName)
                    : entries.OrderBy(e => e.data.EntryName),
                
                LoreSortType.ByCategory => descending
                    ? entries.OrderByDescending(e => e.data.Category)
                    : entries.OrderBy(e => e.data.Category),
                
                LoreSortType.ByDiscoveryDate => descending
                    ? entries.OrderByDescending(e => e.status.DiscoveredDate)
                    : entries.OrderBy(e => e.status.DiscoveredDate),
                
                LoreSortType.ByEra => descending
                    ? entries.OrderByDescending(e => e.data.Era)
                    : entries.OrderBy(e => e.data.Era),
                
                LoreSortType.ByAuthor => descending
                    ? entries.OrderByDescending(e => e.data.Author)
                    : entries.OrderBy(e => e.data.Author),
                
                _ => entries.OrderBy(e => e.data.EntryName)
            };

            return sorted.ToList();
        }

        /// <summary>
        /// Get all unique eras
        /// </summary>
        public List<string> GetAllEras()
        {
            return allLoreEntries.Values
                .Select(e => e.Era)
                .Where(e => !string.IsNullOrEmpty(e))
                .Distinct()
                .OrderBy(e => e)
                .ToList();
        }

        /// <summary>
        /// Get all unique locations
        /// </summary>
        public List<string> GetAllLocations()
        {
            return allLoreEntries.Values
                .Select(e => e.Location)
                .Where(l => !string.IsNullOrEmpty(l))
                .Distinct()
                .OrderBy(l => l)
                .ToList();
        }
        #endregion

        #region Save/Load
        /// <summary>
        /// Get save data for the lore codex
        /// </summary>
        public LoreCodexSaveData GetSaveData()
        {
            var saveData = new LoreCodexSaveData
            {
                TotalEntriesDiscovered = TotalDiscovered
            };

            foreach (var kvp in discoveredEntries)
            {
                saveData.DiscoveredEntries[kvp.Key] = kvp.Value;
            }

            // Find most recently discovered
            var recent = discoveredEntries.Values
                .Where(e => e.IsDiscovered)
                .OrderByDescending(e => e.DiscoveredDate)
                .FirstOrDefault();

            if (recent != null)
            {
                saveData.LastDiscoveredEntryId = recent.EntryId;
                saveData.LastDiscoveredDate = recent.DiscoveredDate;
            }

            return saveData;
        }

        /// <summary>
        /// Load save data
        /// </summary>
        public void LoadSaveData(LoreCodexSaveData saveData)
        {
            if (saveData == null)
                return;

            discoveredEntries.Clear();
            
            foreach (var kvp in saveData.DiscoveredEntries)
            {
                discoveredEntries[kvp.Key] = kvp.Value;
            }

            EmitSignal(SignalName.CodexUpdated);
            GD.Print($"LoreCodexManager: Loaded {discoveredEntries.Count} entries from save");
        }
        #endregion

        #region Debug Functions
        /// <summary>
        /// Unlock all lore entries (for testing)
        /// </summary>
        public void UnlockAll()
        {
            foreach (var entryId in allLoreEntries.Keys)
            {
                DiscoverEntry(entryId, silent: true);
            }
            GD.Print("All lore entries unlocked!");
        }

        /// <summary>
        /// Reset all progress
        /// </summary>
        public void ResetProgress()
        {
            discoveredEntries.Clear();
            
            foreach (var entryId in allLoreEntries.Keys)
            {
                discoveredEntries[entryId] = new LoreCodexEntry
                {
                    EntryId = entryId,
                    IsDiscovered = allLoreEntries[entryId].StartsUnlocked
                };
            }
            
            EmitSignal(SignalName.CodexUpdated);
            GD.Print("Lore codex progress reset");
        }
        #endregion
    }
}