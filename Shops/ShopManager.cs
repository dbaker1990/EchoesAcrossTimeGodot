using Godot;
using System;
using System.Collections.Generic;
using EchoesAcrossTime.Items;

namespace EchoesAcrossTime.Shops
{
    /// <summary>
    /// Central manager for all shop operations
    /// Add as Autoload singleton
    /// </summary>
    public partial class ShopManager : Node
    {
        public static ShopManager Instance { get; private set; }
        
        // Signals
        [Signal] public delegate void ShopOpenedEventHandler(ShopData shop);
        [Signal] public delegate void ShopClosedEventHandler();
        [Signal] public delegate void ItemBoughtEventHandler(string itemId, int quantity, int totalCost);
        [Signal] public delegate void ItemSoldEventHandler(string itemId, int quantity, int totalValue);
        [Signal] public delegate void TransactionFailedEventHandler(string reason);
        
        // Current state
        public ShopData CurrentShop { get; private set; }
        public bool IsShopOpen { get; private set; }
        
        // Shop registry
        private Dictionary<string, ShopData> registeredShops = new();
        
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
            GD.Print("[ShopManager] Ready!");
        }
        
        
        
        #region Shop Registration
        
        /// <summary>
        /// Register a shop for later use
        /// </summary>
        public void RegisterShop(ShopData shop)
        {
            if (shop == null)
            {
                GD.PushError("[ShopManager] Cannot register null shop!");
                return;
            }
            
            if (registeredShops.ContainsKey(shop.ShopId))
            {
                GD.PushWarning($"[ShopManager] Shop {shop.ShopId} already registered. Overwriting.");
            }
            
            registeredShops[shop.ShopId] = shop;
            GD.Print($"[ShopManager] Registered shop: {shop.ShopName} ({shop.ShopId})");
        }
        
        /// <summary>
        /// Load all shops from a folder
        /// </summary>
        public void LoadShopsFromFolder(string folderPath)
        {
            var dir = DirAccess.Open(folderPath);
            if (dir == null)
            {
                GD.PushError($"[ShopManager] Cannot open folder: {folderPath}");
                return;
            }
            
            dir.ListDirBegin();
            string fileName = dir.GetNext();
            int count = 0;
            
            while (!string.IsNullOrEmpty(fileName))
            {
                if (!dir.CurrentIsDir() && fileName.EndsWith(".tres"))
                {
                    var fullPath = $"{folderPath}/{fileName}";
                    var shop = GD.Load<ShopData>(fullPath);
                    if (shop != null)
                    {
                        RegisterShop(shop);
                        count++;
                    }
                }
                fileName = dir.GetNext();
            }
            
            dir.ListDirEnd();
            GD.Print($"[ShopManager] Loaded {count} shops from {folderPath}");
        }
        
        /// <summary>
        /// Get a registered shop by ID
        /// </summary>
        public ShopData GetShop(string shopId)
        {
            return registeredShops.GetValueOrDefault(shopId);
        }
        
        #endregion
        
        #region Shop Operations
        
        /// <summary>
        /// Open a shop
        /// </summary>
        public bool OpenShop(string shopId)
        {
            var shop = GetShop(shopId);
            if (shop == null)
            {
                GD.PushError($"[ShopManager] Shop not found: {shopId}");
                return false;
            }
            
            return OpenShop(shop);
        }
        
        /// <summary>
        /// Open a shop directly
        /// </summary>
        public bool OpenShop(ShopData shop)
        {
            if (shop == null)
            {
                GD.PushError("[ShopManager] Cannot open null shop!");
                return false;
            }
            
            // Check if unlocked
            if (!IsShopUnlocked(shop))
            {
                EmitSignal(SignalName.TransactionFailed, "Shop is locked!");
                return false;
            }
            
            // Restock if needed
            if (shop.RestocksOnVisit)
            {
                shop.Restock();
            }
            
            CurrentShop = shop;
            IsShopOpen = true;
            
            EmitSignal(SignalName.ShopOpened, shop);
            GD.Print($"[ShopManager] Opened shop: {shop.ShopName}");
            
            return true;
        }
        
        /// <summary>
        /// Close current shop
        /// </summary>
        public void CloseShop()
        {
            if (!IsShopOpen) return;
            
            CurrentShop = null;
            IsShopOpen = false;
            
            EmitSignal(SignalName.ShopClosed);
            GD.Print("[ShopManager] Shop closed");
        }
        
        /// <summary>
        /// Check if shop is unlocked
        /// </summary>
        public bool IsShopUnlocked(ShopData shop)
        {
            if (!shop.IsUnlocked) return false;
            
            // Check quest requirement
            if (!string.IsNullOrEmpty(shop.RequiredQuestId))
            {
                var questManager = GetNodeOrNull<Node>("/root/QuestManager");
                if (questManager != null && questManager.HasMethod("IsQuestCompleted"))
                {
                    var completed = (bool)questManager.Call("IsQuestCompleted", shop.RequiredQuestId);
                    if (!completed) return false;
                }
            }
            
            // Check story progress (you can implement this based on your game)
            // For now, just return true if basic unlock is true
            
            return true;
        }
        
        #endregion
        
        #region Buy/Sell Transactions
        
        /// <summary>
        /// Buy an item from current shop
        /// </summary>
        public bool BuyItem(string itemId, int quantity = 1)
        {
            if (!IsShopOpen || CurrentShop == null)
            {
                EmitSignal(SignalName.TransactionFailed, "No shop is open!");
                return false;
            }
            
            var shopItem = CurrentShop.GetShopItem(itemId);
            if (shopItem == null)
            {
                EmitSignal(SignalName.TransactionFailed, "Item not available in this shop!");
                return false;
            }
            
            // Check if item is unlocked
            if (!IsItemUnlocked(shopItem))
            {
                EmitSignal(SignalName.TransactionFailed, "This item is locked!");
                return false;
            }
            
            // Check stock
            if (!CurrentShop.HasUnlimitedStock && shopItem.CurrentStock < quantity)
            {
                EmitSignal(SignalName.TransactionFailed, "Not enough stock!");
                return false;
            }
            
            // Calculate cost
            int totalCost = shopItem.BuyPrice * quantity;
            
            // Check if player has enough gold
            int currentGold = GetPlayerGold();
            if (currentGold < totalCost)
            {
                EmitSignal(SignalName.TransactionFailed, "Not enough gold!");
                return false;
            }
            
            // Process transaction
            if (!SpendGold(totalCost))
            {
                EmitSignal(SignalName.TransactionFailed, "Failed to process payment!");
                return false;
            }
            
            if (!AddItemToInventory(itemId, quantity))
            {
                // Refund if inventory add fails
                AddGold(totalCost);
                EmitSignal(SignalName.TransactionFailed, "Inventory full!");
                return false;
            }
            
            // Reduce stock
            CurrentShop.ReduceStock(itemId, quantity);
            
            // Success!
            EmitSignal(SignalName.ItemBought, itemId, quantity, totalCost);
            PlayPurchaseSound();
            
            GD.Print($"[ShopManager] Bought {quantity}x {itemId} for {totalCost} gold");
            return true;
        }
        
        /// <summary>
        /// Sell an item to current shop
        /// </summary>
        public bool SellItem(string itemId, int quantity = 1)
        {
            if (!IsShopOpen || CurrentShop == null)
            {
                EmitSignal(SignalName.TransactionFailed, "No shop is open!");
                return false;
            }
            
            if (!CurrentShop.CanSellItems)
            {
                EmitSignal(SignalName.TransactionFailed, "This shop doesn't buy items!");
                return false;
            }
            
            // Check if player has the item
            if (!HasItemInInventory(itemId, quantity))
            {
                EmitSignal(SignalName.TransactionFailed, "You don't have that item!");
                return false;
            }
            
            // Get item's base value
            int itemValue = GetItemValue(itemId);
            if (itemValue <= 0)
            {
                EmitSignal(SignalName.TransactionFailed, "This item cannot be sold!");
                return false;
            }
            
            // Calculate sell price
            int sellPrice = Mathf.RoundToInt(itemValue * CurrentShop.SellPriceMultiplier);
            int totalValue = sellPrice * quantity;
            
            // Remove from inventory
            if (!RemoveItemFromInventory(itemId, quantity))
            {
                EmitSignal(SignalName.TransactionFailed, "Failed to remove item!");
                return false;
            }
            
            // Give gold
            AddGold(totalValue);
            
            // Success!
            EmitSignal(SignalName.ItemSold, itemId, quantity, totalValue);
            PlaySellSound();
            
            GD.Print($"[ShopManager] Sold {quantity}x {itemId} for {totalValue} gold");
            return true;
        }
        
        /// <summary>
        /// Get the sell price for an item
        /// </summary>
        public int GetSellPrice(string itemId)
        {
            if (CurrentShop == null) return 0;
            
            int baseValue = GetItemValue(itemId);
            return Mathf.RoundToInt(baseValue * CurrentShop.SellPriceMultiplier);
        }
        
        /// <summary>
        /// Check if an item is unlocked for purchase
        /// </summary>
        private bool IsItemUnlocked(ShopItem item)
        {
            if (!item.IsUnlocked) return false;
            
            // Check quest requirement
            if (!string.IsNullOrEmpty(item.RequiredQuestId))
            {
                var questManager = GetNodeOrNull<Node>("/root/QuestManager");
                if (questManager != null && questManager.HasMethod("IsQuestCompleted"))
                {
                    var completed = (bool)questManager.Call("IsQuestCompleted", item.RequiredQuestId);
                    if (!completed) return false;
                }
            }
            
            // Check level requirement
            var partyManager = GetNodeOrNull<Node>("/root/PartyManager");
            if (partyManager != null && partyManager.HasMethod("GetAverageLevel"))
            {
                int avgLevel = (int)partyManager.Call("GetAverageLevel");
                if (avgLevel < item.RequiredLevel) return false;
            }
            
            return true;
        }
        
        #endregion
        
        #region Helper Methods - Integrate with your existing systems
        
        private int GetPlayerGold()
        {
            // Primary: Use InventorySystem (most common pattern)
            if (InventorySystem.Instance != null)
            {
                return InventorySystem.Instance.GetGold();
            }
            
            // Fallback: Try SaveManager
            if (GameManager.Instance?.CurrentSave != null)
            {
                if (GameManager.Instance.CurrentSave.Inventory != null)
                {
                    return GameManager.Instance.CurrentSave.Inventory.Gold;
                }
            }
            
            GD.PrintErr("[ShopManager] Could not access gold - no InventorySystem or SaveManager!");
            return 0;
        }
        
        private bool SpendGold(int amount)
        {
            if (amount <= 0) return true;
            
            // Primary: Use InventorySystem
            if (InventorySystem.Instance != null)
            {
                return InventorySystem.Instance.RemoveGold(amount);
            }
            
            // Fallback: Try SaveManager
            if (GameManager.Instance?.CurrentSave?.Inventory != null)
            {
                int currentGold = GameManager.Instance.CurrentSave.Inventory.Gold;
                if (currentGold >= amount)
                {
                    GameManager.Instance.CurrentSave.Inventory.Gold -= amount;
                    return true;
                }
                return false;
            }
            
            GD.PrintErr("[ShopManager] Could not spend gold - no system available!");
            return false;
        }
        
        private void AddGold(int amount)
        {
            if (amount <= 0) return;
            
            // Primary: Use InventorySystem
            if (InventorySystem.Instance != null)
            {
                InventorySystem.Instance.AddGold(amount);
                return;
            }
            
            // Fallback: Try SaveManager
            if (GameManager.Instance?.CurrentSave?.Inventory != null)
            {
                GameManager.Instance.CurrentSave.Inventory.Gold += amount;
                return;
            }
            
            GD.PrintErr("[ShopManager] Could not add gold - no system available!");
        }
        
        private bool AddItemToInventory(string itemId, int quantity)
        {
            if (InventorySystem.Instance == null)
            {
                GD.PrintErr("[ShopManager] InventorySystem not found!");
                return false;
            }
    
            // FIXED: Use fully qualified type name
            EchoesAcrossTime.Items.ItemData itemData = null;
    
            if (GameManager.Instance?.Database != null)
            {
                itemData = GameManager.Instance.Database.GetItem(itemId);
            }
    
            if (itemData == null)
            {
                // Try to load directly as fallback
                try
                {
                    // FIXED: Use fully qualified type name
                    itemData = GD.Load<EchoesAcrossTime.Items.ItemData>($"res://Data/Items/{itemId}.tres");
                }
                catch
                {
                    GD.PrintErr($"[ShopManager] Could not find ItemData for '{itemId}'!");
                    return false;
                }
            }
    
            if (itemData == null)
            {
                GD.PrintErr($"[ShopManager] ItemData is null for '{itemId}'!");
                return false;
            }
    
            // Add to inventory
            return InventorySystem.Instance.AddItem(itemData, quantity);
        }

        
        private bool RemoveItemFromInventory(string itemId, int quantity)
        {
            if (InventorySystem.Instance == null)
            {
                GD.PrintErr("[ShopManager] InventorySystem not found!");
                return false;
            }
            
            return InventorySystem.Instance.RemoveItem(itemId, quantity);
        }
        
        private bool HasItemInInventory(string itemId, int quantity)
        {
            if (InventorySystem.Instance != null)
            {
                int count = InventorySystem.Instance.GetItemCount(itemId);
                return count >= quantity;
            }
            
            GD.PrintErr("[ShopManager] Could not check inventory - InventorySystem not found!");
            return false;
        }
        
        /// <summary>
        /// Get item value from database (FIXED - was returning hardcoded 100)
        /// </summary>
        private int GetItemValue(string itemId)
        {
            // Try to get from GameManager/Database
            if (GameManager.Instance?.Database != null)
            {
                var itemData = GameManager.Instance.Database.GetItem(itemId);
                if (itemData != null)
                {
                    // FIXED: ItemData has "Price" property
                    return itemData.Price;
                }
                else
                {
                    GD.PrintErr($"[ShopManager] Item '{itemId}' not found in database!");
                    return 0;
                }
            }
    
            // Fallback - try to load directly if GameManager isn't set up
            try
            {
                // FIXED: Use fully qualified type name
                var itemData = GD.Load<EchoesAcrossTime.Items.ItemData>($"res://Data/Items/{itemId}.tres");
                if (itemData != null)
                {
                    return itemData.Price;
                }
            }
            catch (Exception e)
            {
                GD.PrintErr($"[ShopManager] Could not load item data for '{itemId}': {e.Message}");
            }
    
            GD.PrintErr($"[ShopManager] Failed to get value for item '{itemId}' - returning 0");
            return 0;
        }
        
        private void PlayPurchaseSound()
        {
            if (Managers.SystemManager.Instance != null)
            {
                Managers.SystemManager.Instance.PlayOkSE();
            }
        }
        
        private void PlaySellSound()
        {
            if (Managers.SystemManager.Instance != null)
            {
                Managers.SystemManager.Instance.PlayOkSE();
            }
        }
        
        #endregion
        
        /// <summary>
        /// Get all registered shops (needed for save system)
        /// </summary>
        public List<ShopData> GetAllShops()
        {
            return new List<ShopData>(registeredShops.Values);
        }
        
        public void UnlockShop(string shopId)
        {
            var shop = GetShop(shopId);
            if (shop != null && !shop.IsUnlocked)
            {
                shop.IsUnlocked = true;
                GD.Print($"[ShopManager] Unlocked shop: {shop.ShopName}");
                
                // Show notification if available
                var notification = GetNodeOrNull<ShopNotification>("%ShopNotification");
                notification?.ShowShopUnlocked(shop.ShopName);
            }
        }
        
        public void UnlockItem(string shopId, string itemId)
        {
            var shop = GetShop(shopId);
            if (shop == null) return;
            
            var item = shop.GetShopItem(itemId);
            if (item != null && !item.IsUnlocked)
            {
                item.IsUnlocked = true;
                GD.Print($"[ShopManager] Unlocked item {itemId} in {shop.ShopName}");
                
                // Show notification
                var notification = GetNodeOrNull<ShopNotification>("%ShopNotification");
                notification?.ShowNewItemAvailable(itemId, shop.ShopName);
            }
        }
        
        public void RestockShop(string shopId)
        {
            var shop = GetShop(shopId);
            if (shop != null)
            {
                shop.Restock();
                GD.Print($"[ShopManager] Restocked {shop.ShopName}");
                
                // Show notification
                var notification = GetNodeOrNull<ShopNotification>("%ShopNotification");
                notification?.ShowShopRestocked(shop.ShopName);
            }
        }
        
        public void RestockAllShops()
        {
            foreach (var shop in registeredShops.Values)
            {
                shop.Restock();
            }
            GD.Print($"[ShopManager] Restocked all shops");
        }
        
        public bool CanAfford(string itemId, int quantity = 1)
        {
            if (CurrentShop == null) return false;
            
            var shopItem = CurrentShop.GetShopItem(itemId);
            if (shopItem == null) return false;
            
            int totalCost = shopItem.BuyPrice * quantity;
            return GetPlayerGold() >= totalCost;
        }
        
        public int GetBuyPrice(string itemId)
        {
            if (CurrentShop == null) return 0;
            
            var shopItem = CurrentShop.GetShopItem(itemId);
            return shopItem?.BuyPrice ?? 0;
        }
        
        public int GetTotalShops()
        {
            return registeredShops.Count;
        }
        
        public List<ShopData> GetShopsInLocation(string locationId)
        {
            // You can add a LocationId property to ShopData
            // For now, return all shops
            return new List<ShopData>(registeredShops.Values);
        }
        
        public bool HasNewItemsAvailable()
        {
            foreach (var shop in registeredShops.Values)
            {
                if (!IsShopUnlocked(shop)) continue;
                
                foreach (var item in shop.ItemsForSale)
                {
                    if (item.IsNewItem && IsItemUnlocked(item))
                        return true;
                }
            }
            return false;
        }
        
        public void ClearNewItemFlags()
        {
            foreach (var shop in registeredShops.Values)
            {
                foreach (var item in shop.ItemsForSale)
                {
                    item.IsNewItem = false;
                }
            }
        }
        
    }
}