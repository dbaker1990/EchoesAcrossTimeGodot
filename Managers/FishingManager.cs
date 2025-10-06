using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using EchoesAcrossTime;
using EchoesAcrossTime.Items;

/// <summary>
/// Manages all fishing-related functionality
/// Add as Autoload Singleton
/// </summary>
public partial class FishingManager : Node
{
    #region Singleton
    public static FishingManager Instance { get; private set; }
    #endregion
    
    #region Fish Database
    [Export] public FishData[] AllFish { get; set; } = Array.Empty<FishData>();
    
    private Dictionary<string, FishData> fishDatabase = new();
    #endregion
    
    #region Player Data
    [Export] public int PlayerFishingLevel { get; set; } = 1;
    [Export] public string CurrentRodType { get; set; } = "basic_rod";
    
    private Dictionary<string, int> caughtFishCounts = new(); // Track how many of each fish caught
    #endregion
    
    #region Current Fishing Session
    private string currentLocation = "";
    private TimeOfDay currentTimeOfDay = TimeOfDay.Afternoon;
    private bool isRaining = false;
    #endregion
    
    public override void _Ready()
    {
        if (Instance != null && Instance != this)
        {
            QueueFree();
            return;
        }
        
        Instance = this;
        InitializeFishDatabase();
    }
    
    #region Initialization
    private void InitializeFishDatabase()
    {
        fishDatabase.Clear();
        
        foreach (var fish in AllFish)
        {
            if (fish != null && !string.IsNullOrEmpty(fish.FishId))
            {
                fishDatabase[fish.FishId] = fish;
            }
        }
        
        GD.Print($"FishingManager: Loaded {fishDatabase.Count} fish types");
    }
    
    /// <summary>
    /// Load fish from a folder of .tres resources
    /// </summary>
    public void LoadFishFromFolder(string folderPath)
    {
        var dir = DirAccess.Open(folderPath);
        if (dir == null)
        {
            GD.PrintErr($"FishingManager: Cannot open folder {folderPath}");
            return;
        }
        
        var fishList = new List<FishData>();
        dir.ListDirBegin();
        
        string fileName = dir.GetNext();
        while (!string.IsNullOrEmpty(fileName))
        {
            if (!dir.CurrentIsDir() && fileName.EndsWith(".tres"))
            {
                var fishPath = $"{folderPath}/{fileName}";
                var fish = GD.Load<FishData>(fishPath);
                
                if (fish != null)
                {
                    fishList.Add(fish);
                    fishDatabase[fish.FishId] = fish;
                }
            }
            
            fileName = dir.GetNext();
        }
        
        dir.ListDirEnd();
        AllFish = fishList.ToArray();
        
        GD.Print($"FishingManager: Loaded {fishList.Count} fish from {folderPath}");
    }
    #endregion
    
    #region Fishing Context
    /// <summary>
    /// Set the current fishing location and conditions
    /// Call this before starting fishing
    /// </summary>
    public void SetFishingContext(string location, TimeOfDay time, bool raining)
    {
        currentLocation = location;
        currentTimeOfDay = time;
        isRaining = raining;
    }
    #endregion
    
    #region Fish Selection
    /// <summary>
    /// Get a random fish based on current conditions
    /// </summary>
    public FishData GetRandomFish()
    {
        return GetRandomFish(currentLocation, CurrentRodType, PlayerFishingLevel, currentTimeOfDay, isRaining);
    }
    
    /// <summary>
    /// Get a random fish with specific conditions
    /// </summary>
    public FishData GetRandomFish(string location, string rodType, int fishingLevel, TimeOfDay timeOfDay, bool raining)
    {
        // Get all fish that can spawn in current conditions
        var availableFish = fishDatabase.Values
            .Where(f => f.CanSpawn(location, rodType, fishingLevel, timeOfDay, raining))
            .ToList();
        
        if (availableFish.Count == 0)
        {
            GD.PrintErr("FishingManager: No fish available for current conditions!");
            return null;
        }
        
        // Calculate total weight
        float totalWeight = availableFish.Sum(f => f.SpawnWeight);
        
        // Random selection based on weight
        float randomValue = GD.Randf() * totalWeight;
        float currentWeight = 0f;
        
        foreach (var fish in availableFish)
        {
            currentWeight += fish.SpawnWeight;
            if (randomValue <= currentWeight)
            {
                return fish;
            }
        }
        
        // Fallback to last fish
        return availableFish[availableFish.Count - 1];
    }
    
    /// <summary>
    /// Get fish by ID
    /// </summary>
    public FishData GetFish(string fishId)
    {
        return fishDatabase.TryGetValue(fishId, out var fish) ? fish : null;
    }
    #endregion
    
    #region Catching Fish
    /// <summary>
    /// Record that a fish was caught
    /// </summary>
    public void OnFishCaught(FishData fish)
    {
        if (fish == null) return;
        
        // Update caught count
        if (caughtFishCounts.ContainsKey(fish.FishId))
        {
            caughtFishCounts[fish.FishId]++;
        }
        else
        {
            caughtFishCounts[fish.FishId] = 1;
        }
        
        // Add to inventory
        AddFishToInventory(fish);
        
        // Grant fishing XP (could level up fishing skill)
        GrantFishingXP(fish);
        
        GD.Print($"Caught {fish.FishName}! Total caught: {caughtFishCounts[fish.FishId]}");
    }
    
    private void AddFishToInventory(FishData fish)
    {
        // Fish need to be set up as ItemData resources in your inventory
        // Get the ItemData for this fish from GameDatabase
        var inventorySystem = InventorySystem.Instance;
        var gameDatabase = GameManager.Instance?.Database;
        
        if (inventorySystem != null && gameDatabase != null)
        {
            // Try to find the fish as an item in the database
            var fishItem = gameDatabase.GetItem(fish.FishId);
            
            if (fishItem != null)
            {
                inventorySystem.AddItem(fishItem, 1);
            }
            else
            {
                GD.PrintErr($"FishingManager: Fish '{fish.FishId}' not found in item database! Make sure to create an ItemData resource for this fish.");
            }
        }
        else
        {
            GD.PrintErr("FishingManager: InventorySystem or GameDatabase not found!");
        }
    }
    
    private void GrantFishingXP(FishData fish)
    {
        // Calculate XP based on fish rarity and difficulty
        int xp = fish.Rarity switch
        {
            FishRarity.Common => 10,
            FishRarity.Uncommon => 25,
            FishRarity.Rare => 50,
            FishRarity.Epic => 100,
            FishRarity.Legendary => 250,
            _ => 10
        };
        
        // Multiply by difficulty
        xp = (int)(xp * fish.DifficultyLevel);
        
        // TODO: Add to player's fishing XP
        // PartyManager.Instance?.AddSkillXP("fishing", xp);
        
        GD.Print($"Gained {xp} fishing XP!");
    }
    #endregion
    
    #region Statistics
    /// <summary>
    /// Get how many of a specific fish have been caught
    /// </summary>
    public int GetCaughtCount(string fishId)
    {
        return caughtFishCounts.TryGetValue(fishId, out var count) ? count : 0;
    }
    
    /// <summary>
    /// Check if a fish has been caught at least once
    /// </summary>
    public bool HasCaught(string fishId)
    {
        return caughtFishCounts.ContainsKey(fishId) && caughtFishCounts[fishId] > 0;
    }
    
    /// <summary>
    /// Get total number of unique fish caught
    /// </summary>
    public int GetUniqueSpeciesCaught()
    {
        return caughtFishCounts.Count(kvp => kvp.Value > 0);
    }
    
    /// <summary>
    /// Get total number of all fish caught
    /// </summary>
    public int GetTotalFishCaught()
    {
        return caughtFishCounts.Values.Sum();
    }
    #endregion
    
    #region Save/Load
    /// <summary>
    /// Get save data for fishing
    /// </summary>
    public FishingSaveData GetSaveData()
    {
        return new FishingSaveData
        {
            PlayerFishingLevel = PlayerFishingLevel,
            CurrentRodType = CurrentRodType,
            CaughtFishCounts = new Dictionary<string, int>(caughtFishCounts)
        };
    }
    
    /// <summary>
    /// Load save data for fishing
    /// </summary>
    public void LoadSaveData(FishingSaveData data)
    {
        if (data == null) return;
        
        PlayerFishingLevel = data.PlayerFishingLevel;
        CurrentRodType = data.CurrentRodType;
        caughtFishCounts = new Dictionary<string, int>(data.CaughtFishCounts);
    }
    #endregion
}

/// <summary>
/// Save data for fishing system
/// Add this to your main SaveData class
/// </summary>
[Serializable]
public class FishingSaveData
{
    public int PlayerFishingLevel { get; set; } = 1;
    public string CurrentRodType { get; set; } = "basic_rod";
    public Dictionary<string, int> CaughtFishCounts { get; set; } = new();
}