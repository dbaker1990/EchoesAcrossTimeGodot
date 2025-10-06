// LoreCodex/LoreEntryData.cs
using Godot;
using System;
using System.Collections.Generic;

namespace EchoesAcrossTime.LoreCodex
{
    /// <summary>
    /// Categories for lore entries
    /// </summary>
    public enum LoreCategory
    {
        Character,      // NPCs, party members, important figures
        Location,       // Cities, dungeons, landmarks
        Event,          // Historical events, story moments
        Item,           // Legendary items, artifacts
        Faction,        // Organizations, groups
        Creature,       // Monsters, species (different from bestiary)
        Concept,        // Magic systems, technology, world rules
        Timeline        // Historical periods, eras
    }

    /// <summary>
    /// Defines a single lore entry
    /// </summary>
    [GlobalClass]
    public partial class LoreEntryData : Resource
    {
        // Basic Info
        [Export] public string EntryId { get; set; } = "";
        [Export] public string EntryName { get; set; } = "";
        [Export] public LoreCategory Category { get; set; } = LoreCategory.Concept;
        
        // Visual
        [Export] public Texture2D Portrait { get; set; }
        [Export] public Texture2D Banner { get; set; }
        
        // Content - Main entry text
        [Export(PropertyHint.MultilineText)] public string ShortDescription { get; set; } = "";
        [Export(PropertyHint.MultilineText)] public string DetailedDescription { get; set; } = "";
        
        // Additional sections (optional)
        [Export] public Godot.Collections.Array<LoreSection> Sections { get; set; } = new();
        
        // Metadata
        [Export] public string Era { get; set; } = ""; // "Age of Legends", "Modern Era", etc.
        [Export] public string Location { get; set; } = ""; // Where this entry relates to
        [Export] public Godot.Collections.Array<string> RelatedEntryIds { get; set; } = new(); // Cross-references
        
        // Discovery conditions
        [Export] public bool StartsUnlocked { get; set; } = false;
        [Export] public string UnlockSwitchId { get; set; } = ""; // Game switch to check
        [Export] public string UnlockQuestId { get; set; } = ""; // Quest that unlocks this
        
        // Flavor
        [Export] public string Author { get; set; } = ""; // Who wrote this entry (in-universe)
        [Export] public string DateWritten { get; set; } = ""; // In-game date
        
        public LoreEntryData()
        {
            Sections = new Godot.Collections.Array<LoreSection>();
            RelatedEntryIds = new Godot.Collections.Array<string>();
        }
    }

    /// <summary>
    /// A section within a lore entry (for multi-page entries)
    /// </summary>
    [GlobalClass]
    public partial class LoreSection : Resource
    {
        [Export] public string SectionTitle { get; set; } = "";
        [Export(PropertyHint.MultilineText)] public string Content { get; set; } = "";
        [Export] public Texture2D Image { get; set; }
        
        // Optional: Section can be locked separately
        [Export] public bool IsLocked { get; set; } = false;
        [Export] public string UnlockCondition { get; set; } = "";
    }

    /// <summary>
    /// Tracks player's discovery of lore entries
    /// </summary>
    [GlobalClass]
    public partial class LoreCodexEntry : Resource
    {
        [Export] public string EntryId { get; set; } = "";
        [Export] public bool IsDiscovered { get; set; } = false;
        [Export] public bool HasBeenRead { get; set; } = false;
        
        // Discovery tracking
        [Export] public string DiscoveredDateString { get; set; } = "";
        [Export] public int TimesViewed { get; set; } = 0;
        
        // Unlocked sections (for progressive discovery)
        [Export] public Godot.Collections.Array<int> UnlockedSectionIndices { get; set; } = new();
        
        public DateTime DiscoveredDate
        {
            get => DateTime.TryParse(DiscoveredDateString, out var date) ? date : DateTime.MinValue;
            set => DiscoveredDateString = value.ToString("O");
        }
        
        public LoreCodexEntry()
        {
            UnlockedSectionIndices = new Godot.Collections.Array<int>();
        }
    }

    /// <summary>
    /// Save data for the lore codex
    /// </summary>
    [GlobalClass]
    public partial class LoreCodexSaveData : Resource
    {
        [Export] public Godot.Collections.Dictionary<string, LoreCodexEntry> DiscoveredEntries { get; set; } = new();
        [Export] public int TotalEntriesDiscovered { get; set; } = 0;
        [Export] public string LastDiscoveredEntryId { get; set; } = "";
        [Export] public string LastDiscoveredDateString { get; set; } = "";
        
        public DateTime LastDiscoveredDate
        {
            get => DateTime.TryParse(LastDiscoveredDateString, out var date) ? date : DateTime.MinValue;
            set => LastDiscoveredDateString = value.ToString("O");
        }
        
        public LoreCodexSaveData()
        {
            DiscoveredEntries = new Godot.Collections.Dictionary<string, LoreCodexEntry>();
        }
    }

    /// <summary>
    /// Filter options for lore codex
    /// </summary>
    public enum LoreFilterType
    {
        All,
        Discovered,
        Unread,
        ByCategory,
        ByEra,
        ByLocation,
        Recent
    }

    /// <summary>
    /// Sort options for lore codex
    /// </summary>
    public enum LoreSortType
    {
        ByName,
        ByCategory,
        ByDiscoveryDate,
        ByEra,
        ByAuthor
    }
}