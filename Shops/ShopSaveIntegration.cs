using Godot;
using System;
using System.Collections.Generic;
using Godot.Collections;

namespace EchoesAcrossTime.Shops
{
    /// <summary>
    /// Shop save data structure
    /// Add this to your SaveData class
    /// </summary>
    [GlobalClass]
    public partial class ShopSaveData : Resource
    {
        [Export] public Dictionary ShopStates { get; set; } = new();
        [Export] public Array<string> UnlockedShops { get; set; } = new();
        [Export] public Dictionary LastVisitTimes { get; set; } = new();
        
        public ShopSaveData() { }
    }
    
    /// <summary>
    /// Individual shop state for saving
    /// </summary>
    [GlobalClass]
    public partial class ShopState : Resource
    {
        [Export] public string ShopId { get; set; }
        [Export] public bool IsUnlocked { get; set; }
        [Export] public Dictionary ItemStocks { get; set; } = new(); // itemId -> stock
        [Export] public double LastRestockTime { get; set; }
        
        public ShopState() { }
    }
    
    /// <summary>
    /// Integration helper for saving shop data
    /// Call these methods from your SaveManager
    /// </summary>
    public static class ShopSaveIntegration
    {
        /// <summary>
        /// Capture current shop states for saving
        /// Call this in SaveData.CaptureCurrentState()
        /// </summary>
        public static ShopSaveData CaptureShopData()
        {
            if (ShopManager.Instance == null)
                return new ShopSaveData();
            
            var saveData = new ShopSaveData();
            
            // Get all registered shops
            var shops = ShopManager.Instance.GetAllShops();
            
            foreach (var shop in shops)
            {
                var shopState = new ShopState
                {
                    ShopId = shop.ShopId,
                    IsUnlocked = shop.IsUnlocked,
                    LastRestockTime = Time.GetTicksMsec()
                };
                
                // Save item stocks
                foreach (var item in shop.ItemsForSale)
                {
                    shopState.ItemStocks[item.ItemId] = item.CurrentStock;
                }
                
                saveData.ShopStates[shop.ShopId] = shopState;
                
                if (shop.IsUnlocked)
                {
                    saveData.UnlockedShops.Add(shop.ShopId);
                }
            }
            
            return saveData;
        }
        
        /// <summary>
        /// Restore shop states from save data
        /// Call this in SaveData.ApplyToGame()
        /// </summary>
        public static void RestoreShopData(ShopSaveData saveData)
        {
            if (ShopManager.Instance == null || saveData == null)
                return;
            
            var shops = ShopManager.Instance.GetAllShops();
            
            foreach (var shop in shops)
            {
                if (!saveData.ShopStates.ContainsKey(shop.ShopId))
                    continue;
                
                var shopState = saveData.ShopStates[shop.ShopId].As<ShopState>();
                if (shopState == null)
                    continue;
                
                // Restore unlock state
                shop.IsUnlocked = shopState.IsUnlocked;
                
                // Restore item stocks
                foreach (var item in shop.ItemsForSale)
                {
                    if (shopState.ItemStocks.ContainsKey(item.ItemId))
                    {
                        item.CurrentStock = shopState.ItemStocks[item.ItemId].AsInt32();
                    }
                }
            }
        }
        
        /// <summary>
        /// Quick integration example for your SaveData class
        /// </summary>
        public static void PrintIntegrationExample()
        {
            GD.Print(@"
=== SHOP SAVE INTEGRATION EXAMPLE ===

Add to your SaveData.cs:

// 1. Add property to SaveData class
[Export]
public ShopSaveData Shops { get; set; } = new ShopSaveData();

// 2. Update CaptureCurrentState()
public void CaptureCurrentState()
{
    // ... your existing code ...
    
    // Capture shop data
    Shops = ShopSaveIntegration.CaptureShopData();
    
    GD.Print(""Shops saved!"");
}

// 3. Update ApplyToGame()
public void ApplyToGame()
{
    // ... your existing code ...
    
    // Restore shop data
    if (Shops != null)
    {
        ShopSaveIntegration.RestoreShopData(Shops);
        GD.Print(""Shops restored!"");
    }
}

=== END INTEGRATION EXAMPLE ===
");
        }
    }
    
    /// <summary>
    /// Extension methods for ShopManager to support save/load
    /// </summary>
    public static class ShopManagerExtensions
    {
        /// <summary>
        /// Get all registered shops (you'll need to add this to ShopManager.cs)
        /// </summary>
        public static List<ShopData> GetAllShops(this ShopManager manager)
        {
            // This accesses private field - you'll need to make it public or add a method
            // For now, return empty list - implement in ShopManager.cs
            
            // In ShopManager.cs, add:
            // public List<ShopData> GetAllShops()
            // {
            //     return new List<ShopData>(registeredShops.Values);
            // }
            
            return new List<ShopData>();
        }
    }
}