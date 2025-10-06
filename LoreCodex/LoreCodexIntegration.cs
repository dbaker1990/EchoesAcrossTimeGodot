// LoreCodex/LoreCodexIntegration.cs
using Godot;
using System;
using EchoesAcrossTime.LoreCodex;

namespace EchoesAcrossTime.LoreCodex
{
    /// <summary>
    /// Integration for saving/loading lore codex data
    /// INSTRUCTIONS: Add a property to your SaveData.cs class:
    /// 
    /// [Export] public string LoreCodexJson { get; set; } = "";
    /// 
    /// Then in CaptureCurrentState(), add:
    /// LoreCodexJson = LoreCodexSaveIntegration.CaptureToJson();
    /// 
    /// And in ApplyToGame(), add:
    /// LoreCodexSaveIntegration.ApplyFromJson(LoreCodexJson);
    /// </summary>
    public static class LoreCodexSaveIntegration
    {
        /// <summary>
        /// Capture lore codex data to JSON string
        /// </summary>
        public static string CaptureToJson()
        {
            if (LoreCodexManager.Instance == null)
                return "";
    
            try
            {
                var saveData = LoreCodexManager.Instance.GetSaveData();
        
                // Convert to dictionary for JSON serialization
                var dict = new Godot.Collections.Dictionary
                {
                    ["TotalEntriesDiscovered"] = saveData.TotalEntriesDiscovered,
                    ["LastDiscoveredEntryId"] = saveData.LastDiscoveredEntryId,
                    ["LastDiscoveredDate"] = saveData.LastDiscoveredDateString
                };
        
                // Convert entries
                var entriesDict = new Godot.Collections.Dictionary();
                foreach (var kvp in saveData.DiscoveredEntries)
                {
                    var entry = kvp.Value;
                    var entryDict = new Godot.Collections.Dictionary
                    {
                        ["EntryId"] = entry.EntryId,
                        ["IsDiscovered"] = entry.IsDiscovered,
                        ["HasBeenRead"] = entry.HasBeenRead,
                        ["DiscoveredDate"] = entry.DiscoveredDateString,
                        ["TimesViewed"] = entry.TimesViewed
                    };
            
                    // PUT IT HERE - RIGHT AFTER creating entryDict
                    var sectionsArray = new Godot.Collections.Array();
                    foreach (var sectionIndex in entry.UnlockedSectionIndices)
                    {
                        sectionsArray.Add(sectionIndex);
                    }
                    entryDict["UnlockedSections"] = sectionsArray;
            
                    entriesDict[kvp.Key] = entryDict;
                }
                dict["Entries"] = entriesDict;
        
                return Json.Stringify(dict);
            }
            catch (Exception e)
            {
                GD.PrintErr($"Failed to capture lore codex: {e.Message}");
                return "";
            }
        }
        
        /// <summary>
        /// Apply lore codex data from JSON string
        /// </summary>
        public static void ApplyFromJson(string json)
        {
            if (LoreCodexManager.Instance == null || string.IsNullOrEmpty(json))
                return;
            
            try
            {
                var parsed = Json.ParseString(json);
                if (parsed.AsGodotDictionary() is not Godot.Collections.Dictionary dict)
                    return;
                
                var saveData = new LoreCodexSaveData();
                
                if (dict.ContainsKey("TotalEntriesDiscovered"))
                    saveData.TotalEntriesDiscovered = dict["TotalEntriesDiscovered"].AsInt32();
                
                if (dict.ContainsKey("LastDiscoveredEntryId"))
                    saveData.LastDiscoveredEntryId = dict["LastDiscoveredEntryId"].AsString();
                
                if (dict.ContainsKey("LastDiscoveredDate"))
                    saveData.LastDiscoveredDateString = dict["LastDiscoveredDate"].AsString();
                
                // Load entries
                if (dict.ContainsKey("Entries") && dict["Entries"].AsGodotDictionary() is Godot.Collections.Dictionary entriesDict)
                {
                    foreach (var kvp in entriesDict)
                    {
                        if (kvp.Value.AsGodotDictionary() is not Godot.Collections.Dictionary entryDict)
                            continue;
                        
                        var entry = new LoreCodexEntry
                        {
                            EntryId = entryDict.ContainsKey("EntryId") ? entryDict["EntryId"].AsString() : "",
                            IsDiscovered = entryDict.ContainsKey("IsDiscovered") && entryDict["IsDiscovered"].AsBool(),
                            HasBeenRead = entryDict.ContainsKey("HasBeenRead") && entryDict["HasBeenRead"].AsBool(),
                            DiscoveredDateString = entryDict.ContainsKey("DiscoveredDate") ? entryDict["DiscoveredDate"].AsString() : "",
                            TimesViewed = entryDict.ContainsKey("TimesViewed") ? entryDict["TimesViewed"].AsInt32() : 0
                        };
                        
                        if (entryDict.ContainsKey("UnlockedSections") && entryDict["UnlockedSections"].AsGodotArray() is Godot.Collections.Array sections)
                        {
                            foreach (var section in sections)
                            {
                                entry.UnlockedSectionIndices.Add(section.AsInt32());
                            }
                        }
                        
                        saveData.DiscoveredEntries[kvp.Key.AsString()] = entry;
                    }
                }
                
                LoreCodexManager.Instance.LoadSaveData(saveData);
            }
            catch (Exception e)
            {
                GD.PrintErr($"Failed to load lore codex: {e.Message}");
            }
        }
    }

    /// <summary>
    /// Helper class for auto-discovering lore based on game events
    /// ADD TO PROJECT SETTINGS -> AUTOLOAD as "LoreCodexAutoDiscover"
    /// </summary>
    public partial class LoreCodexAutoDiscover : Node
    {
        public override void _Ready()
        {
            // Connect to game event signals
            ConnectToEventSystems();
        }

        private void ConnectToEventSystems()
        {
            // Connect to quest completion
            if (Quests.QuestManager.Instance != null)
            {
                Quests.QuestManager.Instance.QuestCompleted += OnQuestCompleted;
            }

            // Connect to switch changes (for story events)
            if (GameManager.Instance != null)
            {
                // You might need to add a signal for switch changes
                // GameManager.Instance.SwitchChanged += OnSwitchChanged;
            }

            // Check unlock conditions periodically
            var timer = new Timer();
            timer.WaitTime = 1.0f;
            timer.Autostart = true;
            timer.Timeout += () => LoreCodexManager.Instance?.CheckUnlockConditions();
            AddChild(timer);
        }

        private void OnQuestCompleted(string questId)
        {
            // Check if any lore entries unlock from this quest
            LoreCodexManager.Instance?.CheckUnlockConditions();
        }

        private void OnSwitchChanged(string switchId, bool value)
        {
            if (value) // Only check when switches are turned ON
            {
                LoreCodexManager.Instance?.CheckUnlockConditions();
            }
        }
    }

    /// <summary>
    /// Helper functions for triggering lore discovery from gameplay
    /// </summary>
    public static class LoreCodexHelpers
    {
        /// <summary>
        /// Discover lore when reading a document/book in the world
        /// </summary>
        public static void DiscoverFromDocument(string documentId, string loreEntryId)
        {
            LoreCodexManager.Instance?.DiscoverEntry(loreEntryId);
            GD.Print($"Discovered lore from document: {documentId}");
        }

        /// <summary>
        /// Discover lore when talking to an NPC
        /// </summary>
        public static void DiscoverFromDialogue(string npcId, string loreEntryId)
        {
            LoreCodexManager.Instance?.DiscoverEntry(loreEntryId);
            GD.Print($"Discovered lore from {npcId}");
        }

        /// <summary>
        /// Discover lore when entering a location
        /// </summary>
        public static void DiscoverFromLocation(string locationId, string loreEntryId)
        {
            LoreCodexManager.Instance?.DiscoverEntry(loreEntryId);
            GD.Print($"Discovered lore at {locationId}");
        }

        /// <summary>
        /// Discover lore during a cutscene/event
        /// </summary>
        public static void DiscoverFromEvent(string eventId, string loreEntryId)
        {
            LoreCodexManager.Instance?.DiscoverEntry(loreEntryId);
            GD.Print($"Discovered lore from event: {eventId}");
        }

        /// <summary>
        /// Unlock a specific section of an entry
        /// </summary>
        public static void UnlockSection(string loreEntryId, int sectionIndex)
        {
            LoreCodexManager.Instance?.UnlockSection(loreEntryId, sectionIndex);
        }

        /// <summary>
        /// Batch discover multiple entries (e.g., after a major story event)
        /// </summary>
        public static void DiscoverMultiple(params string[] entryIds)
        {
            foreach (var entryId in entryIds)
            {
                LoreCodexManager.Instance?.DiscoverEntry(entryId);
            }
        }
    }
}