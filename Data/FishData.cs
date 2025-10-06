using Godot;
using System;

/// <summary>
/// Data resource for different fish types
/// Create as .tres resources in Godot editor
/// </summary>
[GlobalClass]
public partial class FishData : Resource
{
    #region Properties
    [Export] public string FishId { get; set; } = "";
    [Export] public string FishName { get; set; } = "Unknown Fish";
    [Export] public Texture2D FishIcon { get; set; }
    
    [ExportGroup("Rarity")]
    [Export] public FishRarity Rarity { get; set; } = FishRarity.Common;
    [Export] public int BaseValue { get; set; } = 10; // Gold value
    
    [ExportGroup("Fishing Difficulty")]
    [Export] public float DifficultyLevel { get; set; } = 1.0f; // 1.0 = easy, 5.0 = very hard
    [Export] public float TensionSpeed { get; set; } = 15f; // How fast tension builds
    [Export] public float EscapeChance { get; set; } = 0.1f; // Chance to escape per second
    [Export] public int RequiredSuccesses { get; set; } = 3; // Button prompts to succeed
    
    [ExportGroup("Spawn Settings")]
    [Export] public float SpawnWeight { get; set; } = 1.0f; // Higher = more common
    [Export] public string[] RequiredLocations { get; set; } = Array.Empty<string>(); // Empty = anywhere
    [Export] public string RequiredRod { get; set; } = ""; // Empty = any rod
    [Export] public int MinimumFishingLevel { get; set; } = 0;
    
    [ExportGroup("Time Requirements")]
    [Export] public bool OnlyMorning { get; set; } = false;
    [Export] public bool OnlyAfternoon { get; set; } = false;
    [Export] public bool OnlyEvening { get; set; } = false;
    [Export] public bool OnlyNight { get; set; } = false;
    [Export] public bool OnlyRainy { get; set; } = false;
    
    [ExportGroup("Description")]
    [Export(PropertyHint.MultilineText)] public string Description { get; set; } = "";
    #endregion
    
    /// <summary>
    /// Get the color associated with this fish's rarity
    /// </summary>
    public Color GetRarityColor()
    {
        return Rarity switch
        {
            FishRarity.Common => new Color(0.8f, 0.8f, 0.8f), // Gray
            FishRarity.Uncommon => new Color(0.2f, 0.8f, 0.2f), // Green
            FishRarity.Rare => new Color(0.2f, 0.5f, 1.0f), // Blue
            FishRarity.Epic => new Color(0.7f, 0.2f, 1.0f), // Purple
            FishRarity.Legendary => new Color(1.0f, 0.6f, 0.0f), // Orange/Gold
            _ => Colors.White
        };
    }
    
    /// <summary>
    /// Check if this fish can spawn in current conditions
    /// </summary>
    public bool CanSpawn(string location, string rodType, int fishingLevel, TimeOfDay timeOfDay, bool isRaining)
    {
        // Check location
        if (RequiredLocations.Length > 0)
        {
            bool validLocation = false;
            foreach (var loc in RequiredLocations)
            {
                if (location.Equals(loc, StringComparison.OrdinalIgnoreCase))
                {
                    validLocation = true;
                    break;
                }
            }
            if (!validLocation) return false;
        }
        
        // Check rod
        if (!string.IsNullOrEmpty(RequiredRod) && !rodType.Equals(RequiredRod, StringComparison.OrdinalIgnoreCase))
            return false;
        
        // Check level
        if (fishingLevel < MinimumFishingLevel)
            return false;
        
        // Check time of day
        if (OnlyMorning && timeOfDay != TimeOfDay.Morning) return false;
        if (OnlyAfternoon && timeOfDay != TimeOfDay.Afternoon) return false;
        if (OnlyEvening && timeOfDay != TimeOfDay.Evening) return false;
        if (OnlyNight && timeOfDay != TimeOfDay.Night) return false;
        
        // Check weather
        if (OnlyRainy && !isRaining) return false;
        
        return true;
    }
}

public enum FishRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

public enum TimeOfDay
{
    Morning,
    Afternoon,
    Evening,
    Night
}