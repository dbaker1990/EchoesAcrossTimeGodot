using Godot;
using System;
using System.Collections.Generic;
using EchoesAcrossTime.Combat;

namespace EchoesAcrossTime.Bestiary
{
    /// <summary>
    /// Represents a single enemy entry in the bestiary
    /// </summary>
    [GlobalClass]
    public partial class BestiaryEntry : Resource
    {
        [Export] public string EnemyId { get; set; }
        [Export] public bool IsDiscovered { get; set; } = false;
        [Export] public int TimesEncountered { get; set; } = 0;
        [Export] public int TimesDefeated { get; set; } = 0;
        
        // Store as string for Godot compatibility
        [Export] public string FirstEncounteredDateString { get; set; } = "";
        [Export] public string LastEncounteredDateString { get; set; } = "";
        
        // Helper properties for DateTime conversion
        public DateTime FirstEncounteredDate
        {
            get => DateTime.TryParse(FirstEncounteredDateString, out var date) ? date : DateTime.MinValue;
            set => FirstEncounteredDateString = value.ToString("O");
        }
        
        public DateTime LastEncounteredDate
        {
            get => DateTime.TryParse(LastEncounteredDateString, out var date) ? date : DateTime.MinValue;
            set => LastEncounteredDateString = value.ToString("O");
        }
        
        // Track highest/lowest stats seen (for variants)
        [Export] public int HighestLevelSeen { get; set; } = 0;
        [Export] public int LowestLevelSeen { get; set; } = 999;
        
        // Discovery flags
        [Export] public bool WeaknessesDiscovered { get; set; } = false;
        [Export] public bool DropsDiscovered { get; set; } = false;
        [Export] public bool SkillsDiscovered { get; set; } = false;
        
        // Discovered weaknesses/resistances
        [Export] public Godot.Collections.Array<ElementType> DiscoveredWeaknesses { get; set; } = new();
        [Export] public Godot.Collections.Array<ElementType> DiscoveredResistances { get; set; } = new();
        [Export] public Godot.Collections.Array<ElementType> DiscoveredImmunities { get; set; } = new();
        
        // Discovered skills
        [Export] public Godot.Collections.Array<string> DiscoveredSkillIds { get; set; } = new();
        
        public BestiaryEntry()
        {
            DiscoveredWeaknesses = new Godot.Collections.Array<ElementType>();
            DiscoveredResistances = new Godot.Collections.Array<ElementType>();
            DiscoveredImmunities = new Godot.Collections.Array<ElementType>();
            DiscoveredSkillIds = new Godot.Collections.Array<string>();
        }
    }

    /// <summary>
    /// Save data for the bestiary system
    /// </summary>
    [GlobalClass]
    public partial class BestiarySaveData : Resource
    {
        [Export] public Godot.Collections.Dictionary<string, BestiaryEntry> Entries { get; set; } = new();
        [Export] public int TotalEnemiesDiscovered { get; set; } = 0;
        [Export] public int TotalEnemiesDefeated { get; set; } = 0;
        [Export] public float TotalPlayTimeWhenLastUpdated { get; set; } = 0;
        
        public BestiarySaveData()
        {
            Entries = new Godot.Collections.Dictionary<string, BestiaryEntry>();
        }
    }

    /// <summary>
    /// Filter options for bestiary display
    /// </summary>
    public enum BestiaryFilterType
    {
        All,
        Discovered,
        Undiscovered,
        Bosses,
        RegularEnemies,
        ByElement,
        ByLevel
    }

    /// <summary>
    /// Sort options for bestiary
    /// </summary>
    public enum BestiarySortType
    {
        ByName,
        ByLevel,
        ByType,
        ByTimesEncountered,
        ByFirstEncounter,
        ByLastEncounter
    }
}