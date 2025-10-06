using Godot;
using System;
using Godot.Collections;

namespace EchoesAcrossTime.Shops
{
    /// <summary>
    /// Defines a shop's inventory and properties
    /// Create as .tres resource and set items in editor!
    /// </summary>
    [GlobalClass]
    public partial class ShopData : Resource
    {
        [ExportGroup("Shop Info")]
        [Export] public string ShopId { get; set; } = "shop_001";
        [Export] public string ShopName { get; set; } = "General Store";
        [Export(PropertyHint.MultilineText)] 
        public string ShopDescription { get; set; } = "Welcome to our shop!";
        [Export] public string ShopkeeperName { get; set; } = "Merchant";
        
        [ExportGroup("Shop Inventory")]
        [Export] public Array<ShopItem> ItemsForSale { get; set; } = new();
        
        [ExportGroup("Shop Settings")]
        [Export] public bool CanSellItems { get; set; } = true;
        [Export(PropertyHint.Range, "0,1,0.01")] 
        public float SellPriceMultiplier { get; set; } = 0.5f; // Sell for 50% of buy price
        
        [Export] public bool HasUnlimitedStock { get; set; } = true;
        [Export] public bool RestocksOnVisit { get; set; } = false;
        
        [ExportGroup("Unlock Conditions")]
        [Export] public bool IsUnlocked { get; set; } = true;
        [Export] public string RequiredQuestId { get; set; } = "";
        [Export] public int RequiredStoryProgress { get; set; } = 0;
        
        [ExportGroup("Visual")]
        [Export] public Texture2D ShopkeeperPortrait { get; set; }
        [Export] public string ShopBackgroundPath { get; set; } = "";
        
        /// <summary>
        /// Get current stock of an item
        /// </summary>
        public int GetStock(string itemId)
        {
            if (HasUnlimitedStock) return 999;
            
            var item = GetShopItem(itemId);
            return item?.CurrentStock ?? 0;
        }
        
        /// <summary>
        /// Find a shop item by ID
        /// </summary>
        public ShopItem GetShopItem(string itemId)
        {
            foreach (var item in ItemsForSale)
            {
                if (item.ItemId == itemId)
                    return item;
            }
            return null;
        }
        
        /// <summary>
        /// Reduce stock after purchase
        /// </summary>
        public void ReduceStock(string itemId, int amount)
        {
            if (HasUnlimitedStock) return;
            
            var item = GetShopItem(itemId);
            if (item != null)
            {
                item.CurrentStock = Math.Max(0, item.CurrentStock - amount);
            }
        }
        
        /// <summary>
        /// Restock all items to max
        /// </summary>
        public void Restock()
        {
            foreach (var item in ItemsForSale)
            {
                item.CurrentStock = item.MaxStock;
            }
        }
    }
    
    /// <summary>
    /// Individual item in shop inventory
    /// </summary>
    [GlobalClass]
    public partial class ShopItem : Resource
    {
        [Export] public string ItemId { get; set; } = "";
        [Export] public int BuyPrice { get; set; } = 100;
        [Export] public int MaxStock { get; set; } = 10;
        [Export] public int CurrentStock { get; set; } = 10;
        
        [ExportGroup("Special Conditions")]
        [Export] public bool IsUnlocked { get; set; } = true;
        [Export] public string RequiredQuestId { get; set; } = "";
        [Export] public int RequiredLevel { get; set; } = 1;
        
        [ExportGroup("Display")]
        [Export] public bool ShowIfLocked { get; set; } = true;
        [Export] public bool IsNewItem { get; set; } = false;
        [Export] public bool IsFeatured { get; set; } = false;
    }
}