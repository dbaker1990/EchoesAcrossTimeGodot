using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using EchoesAcrossTime.Managers;

namespace EchoesAcrossTime.Shops
{
    /// <summary>
    /// Shop UI - displays shop interface for buying/selling
    /// Complete version with proper game system integration
    /// </summary>
    public partial class ShopUI : Control
    {
        [ExportGroup("UI References")]
        [Export] private Control shopPanel;
        [Export] private Label shopNameLabel;
        [Export] private Label shopDescriptionLabel;
        [Export] private Label goldLabel;
        [Export] private TextureRect shopkeeperPortrait;
        
        [ExportGroup("Tabs")]
        [Export] private Button buyTabButton;
        [Export] private Button sellTabButton;
        
        [ExportGroup("Item Lists")]
        [Export] private ItemList buyItemList;
        [Export] private ItemList sellItemList;
        
        [ExportGroup("Item Details")]
        [Export] private Control detailsPanel;
        [Export] private Label itemNameLabel;
        [Export] private Label itemDescriptionLabel;
        [Export] private Label itemPriceLabel;
        [Export] private Label itemStockLabel;
        [Export] private TextureRect itemIcon;
        [Export] private SpinBox quantitySpinBox;
        [Export] private Label totalCostLabel;
        
        [ExportGroup("Buttons")]
        [Export] private Button buyButton;
        [Export] private Button sellButton;
        [Export] private Button exitButton;
        
        // State
        private ShopData currentShop;
        private bool isBuyMode = true;
        private string selectedItemId = "";
        
        public override void _Ready()
        {
            // Connect signals
            if (ShopManager.Instance != null)
            {
                ShopManager.Instance.ShopOpened += OnShopOpened;
                ShopManager.Instance.ShopClosed += OnShopClosed;
                ShopManager.Instance.ItemBought += OnItemBought;
                ShopManager.Instance.ItemSold += OnItemSold;
                ShopManager.Instance.TransactionFailed += OnTransactionFailed;
            }
            
            // Connect UI signals
            buyTabButton?.Connect("pressed", Callable.From(() => SwitchToTab(true)));
            sellTabButton?.Connect("pressed", Callable.From(() => SwitchToTab(false)));
            
            buyItemList?.Connect("item_selected", Callable.From((long index) => OnItemSelected(index, true)));
            sellItemList?.Connect("item_selected", Callable.From((long index) => OnItemSelected(index, false)));
            
            buyButton?.Connect("pressed", Callable.From(OnBuyButtonPressed));
            sellButton?.Connect("pressed", Callable.From(OnSellButtonPressed));
            exitButton?.Connect("pressed", Callable.From(OnExitButtonPressed));
            
            quantitySpinBox?.Connect("value_changed", Callable.From((double value) => UpdateTotalCost()));
            
            // Hide by default
            Hide();
        }
        
        public override void _ExitTree()
        {
            if (ShopManager.Instance != null)
            {
                ShopManager.Instance.ShopOpened -= OnShopOpened;
                ShopManager.Instance.ShopClosed -= OnShopClosed;
                ShopManager.Instance.ItemBought -= OnItemBought;
                ShopManager.Instance.ItemSold -= OnItemSold;
                ShopManager.Instance.TransactionFailed -= OnTransactionFailed;
            }
        }
        
        #region Shop State
        
        private void OnShopOpened(ShopData shop)
        {
            currentShop = shop;
            Show();
            GetTree().Paused = true;
            
            // Update header
            if (shopNameLabel != null)
                shopNameLabel.Text = shop.ShopName;
            
            if (shopDescriptionLabel != null)
                shopDescriptionLabel.Text = shop.ShopDescription;
            
            if (shopkeeperPortrait != null && shop.ShopkeeperPortrait != null)
                shopkeeperPortrait.Texture = shop.ShopkeeperPortrait;
            
            // Start in buy mode
            SwitchToTab(true);
            UpdateGoldDisplay();
        }
        
        private void OnShopClosed()
        {
            currentShop = null;
            Hide();
            GetTree().Paused = false;
        }
        
        private void OnExitButtonPressed()
        {
            PlayCancelSound();
            ShopManager.Instance?.CloseShop();
        }
        
        #endregion
        
        #region Tab Management
        
        private void SwitchToTab(bool buyMode)
        {
            isBuyMode = buyMode;
            
            // Update tab button states
            if (buyTabButton != null)
                buyTabButton.Disabled = buyMode;
            
            if (sellTabButton != null)
                sellTabButton.Disabled = !buyMode;
            
            // Show/hide item lists
            if (buyItemList != null)
                buyItemList.Visible = buyMode;
            
            if (sellItemList != null)
                sellItemList.Visible = !buyMode;
            
            // Show/hide action buttons
            if (buyButton != null)
                buyButton.Visible = buyMode;
            
            if (sellButton != null)
                sellButton.Visible = !buyMode;
            
            // Refresh list
            if (buyMode)
                RefreshBuyList();
            else
                RefreshSellList();
            
            PlayCursorSound();
        }
        
        #endregion
        
        #region Item Lists
        
        private void RefreshBuyList()
        {
            if (buyItemList == null || currentShop == null) return;
            
            buyItemList.Clear();
            
            foreach (var shopItem in currentShop.ItemsForSale)
            {
                // Check if item should be shown
                bool isUnlocked = shopItem.IsUnlocked;
                if (!isUnlocked && !shopItem.ShowIfLocked)
                    continue;
                
                // Get item data from GameDatabase
                var itemData = GetItemData(shopItem.ItemId);
                if (itemData == null) continue;
                
                // Build display text
                string displayText = $"{itemData.DisplayName}";
                
                if (!isUnlocked)
                {
                    displayText += " [LOCKED]";
                }
                else if (shopItem.IsFeatured)
                {
                    displayText = "★ " + displayText;
                }
                else if (shopItem.IsNewItem)
                {
                    displayText = "NEW! " + displayText;
                }
                
                displayText += $" - {shopItem.BuyPrice}G";
                
                // Add stock info
                if (!currentShop.HasUnlimitedStock)
                {
                    displayText += $" ({shopItem.CurrentStock} left)";
                }
                
                // Add to list
                int index = buyItemList.AddItem(displayText, itemData.Icon);
                buyItemList.SetItemMetadata(index, shopItem.ItemId);
                
                // Disable if locked or out of stock
                if (!isUnlocked || (!currentShop.HasUnlimitedStock && shopItem.CurrentStock <= 0))
                {
                    buyItemList.SetItemDisabled(index, true);
                }
            }
        }
        
        private void RefreshSellList()
        {
            if (sellItemList == null) return;
            
            sellItemList.Clear();
            
            if (currentShop == null || !currentShop.CanSellItems)
            {
                sellItemList.AddItem("This shop doesn't buy items.");
                sellItemList.SetItemDisabled(0, true);
                return;
            }
            
            // Get player's inventory from InventorySystem
            var inventory = GetPlayerInventory();
            
            foreach (var item in inventory)
            {
                string itemId = item.Key;
                int quantity = item.Value;
                
                if (quantity <= 0) continue;
                
                var itemData = GetItemData(itemId);
                if (itemData == null) continue;
                
                int sellPrice = ShopManager.Instance.GetSellPrice(itemId);
                
                string displayText = $"{itemData.DisplayName} ({quantity}) - {sellPrice}G each";
                
                int index = sellItemList.AddItem(displayText, itemData.Icon);
                sellItemList.SetItemMetadata(index, itemId);
            }
            
            if (sellItemList.ItemCount == 0)
            {
                sellItemList.AddItem("No items to sell");
                sellItemList.SetItemDisabled(0, true);
            }
        }
        
        #endregion
        
        #region Item Selection
        
        private void OnItemSelected(long index, bool isBuyList)
        {
            ItemList list = isBuyList ? buyItemList : sellItemList;
            if (list == null) return;
            
            selectedItemId = list.GetItemMetadata((int)index).AsString();
            
            var itemData = GetItemData(selectedItemId);
            if (itemData == null) return;
            
            // Update details panel
            if (itemNameLabel != null)
                itemNameLabel.Text = itemData.DisplayName;
            
            if (itemDescriptionLabel != null)
                itemDescriptionLabel.Text = itemData.Description;
            
            if (itemIcon != null)
                itemIcon.Texture = itemData.Icon;
            
            // Update price
            if (isBuyList)
            {
                var shopItem = currentShop?.GetShopItem(selectedItemId);
                if (shopItem != null)
                {
                    if (itemPriceLabel != null)
                        itemPriceLabel.Text = $"Price: {shopItem.BuyPrice}G";
                    
                    if (itemStockLabel != null)
                    {
                        if (currentShop.HasUnlimitedStock)
                            itemStockLabel.Text = "Stock: Unlimited";
                        else
                            itemStockLabel.Text = $"Stock: {shopItem.CurrentStock}";
                    }
                    
                    // Update max quantity
                    if (quantitySpinBox != null)
                    {
                        int maxBuy = currentShop.HasUnlimitedStock ? 99 : shopItem.CurrentStock;
                        quantitySpinBox.MaxValue = maxBuy;
                        quantitySpinBox.Value = 1;
                    }
                }
            }
            else
            {
                int sellPrice = ShopManager.Instance.GetSellPrice(selectedItemId);
                if (itemPriceLabel != null)
                    itemPriceLabel.Text = $"Sell for: {sellPrice}G";
                
                if (itemStockLabel != null)
                {
                    int ownedCount = GetItemCount(selectedItemId);
                    itemStockLabel.Text = $"You have: {ownedCount}";
                }
                
                // Update max quantity
                if (quantitySpinBox != null)
                {
                    quantitySpinBox.MaxValue = GetItemCount(selectedItemId);
                    quantitySpinBox.Value = 1;
                }
            }
            
            UpdateTotalCost();
            PlayCursorSound();
        }
        
        private void UpdateTotalCost()
        {
            if (totalCostLabel == null || string.IsNullOrEmpty(selectedItemId)) return;
            
            int quantity = (int)(quantitySpinBox?.Value ?? 1);
            
            if (isBuyMode)
            {
                var shopItem = currentShop?.GetShopItem(selectedItemId);
                if (shopItem != null)
                {
                    int total = shopItem.BuyPrice * quantity;
                    totalCostLabel.Text = $"Total: {total}G";
                }
            }
            else
            {
                int sellPrice = ShopManager.Instance.GetSellPrice(selectedItemId);
                int total = sellPrice * quantity;
                totalCostLabel.Text = $"Total: {total}G";
            }
        }
        
        #endregion
        
        #region Buy/Sell Actions
        
        private void OnBuyButtonPressed()
        {
            if (string.IsNullOrEmpty(selectedItemId)) return;
            
            int quantity = (int)(quantitySpinBox?.Value ?? 1);
            
            if (ShopManager.Instance.BuyItem(selectedItemId, quantity))
            {
                RefreshBuyList();
                UpdateGoldDisplay();
                var itemData = GetItemData(selectedItemId);
                ShowNotification($"Purchased {quantity}x {itemData?.DisplayName ?? selectedItemId}!");
            }
        }
        
        private void OnSellButtonPressed()
        {
            if (string.IsNullOrEmpty(selectedItemId)) return;
            
            int quantity = (int)(quantitySpinBox?.Value ?? 1);
            
            if (ShopManager.Instance.SellItem(selectedItemId, quantity))
            {
                RefreshSellList();
                UpdateGoldDisplay();
                var itemData = GetItemData(selectedItemId);
                ShowNotification($"Sold {quantity}x {itemData?.DisplayName ?? selectedItemId}!");
            }
        }
        
        private void OnItemBought(string itemId, int quantity, int totalCost)
        {
            // Already handled in OnBuyButtonPressed
        }
        
        private void OnItemSold(string itemId, int quantity, int totalValue)
        {
            // Already handled in OnSellButtonPressed
        }
        
        private void OnTransactionFailed(string reason)
        {
            ShowNotification(reason);
            PlayErrorSound();
        }
        
        #endregion
        
        #region Helpers
        
        private void UpdateGoldDisplay()
        {
            if (goldLabel != null)
            {
                int gold = GetPlayerGold();
                goldLabel.Text = $"Gold: {gold}G";
            }
        }
        
        private void ShowNotification(string message)
        {
            GD.Print($"[ShopUI] {message}");
            
            // Show notification via ShopNotification if available
            var notification = GetNodeOrNull<ShopNotification>("/root/ShopUI/ShopNotification");
            if (notification == null)
            {
                // Try finding it as a child
                notification = GetNodeOrNull<ShopNotification>("ShopNotification");
            }
            
            if (notification != null)
            {
                notification.ShowNotification(message);
            }
        }
        
        #endregion
        
        #region Integration Points - Connect to your systems
        
        /// <summary>
        /// Get item data from GameDatabase
        /// </summary>
        private Items.ItemData GetItemData(string itemId)
        {
            // First try GameDatabase
            if (GameManager.Instance?.Database != null)
            {
                return GameManager.Instance.Database.GetItem(itemId);
            }
            
            // Fallback: Try to load directly
            try
            {
                var itemData = GD.Load<Items.ItemData>($"res://Data/Items/{itemId}.tres");
                return itemData;
            }
            catch (Exception e)
            {
                GD.PrintErr($"[ShopUI] Could not load item data for '{itemId}': {e.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Get player inventory from InventorySystem
        /// </summary>
        private Dictionary<string, int> GetPlayerInventory()
        {
            var inventory = new Dictionary<string, int>();
            
            if (Items.InventorySystem.Instance != null)
            {
                // Get all items from inventory
                var items = Items.InventorySystem.Instance.GetAllItems();
                
                foreach (var slot in items)
                {
                    if (slot != null && slot.Item != null)
                    {
                        string itemId = slot.Item.ItemId;
                        if (inventory.ContainsKey(itemId))
                            inventory[itemId] += slot.Quantity;
                        else
                            inventory[itemId] = slot.Quantity;
                    }
                }
            }
            
            return inventory;
        }
        
        /// <summary>
        /// Get item count from InventorySystem
        /// </summary>
        private int GetItemCount(string itemId)
        {
            if (Items.InventorySystem.Instance != null)
            {
                return Items.InventorySystem.Instance.GetItemCount(itemId);
            }
            
            return 0;
        }
        
        /// <summary>
        /// Get player gold
        /// </summary>
        private int GetPlayerGold()
        {
            // Use InventorySystem
            if (Items.InventorySystem.Instance != null)
            {
                return Items.InventorySystem.Instance.GetGold();
            }
            
            // Fallback: Try SaveManager
            if (GameManager.Instance?.CurrentSave?.Inventory != null)
            {
                return GameManager.Instance.CurrentSave.Inventory.Gold;
            }
            
            return 0;
        }
        
        /// <summary>
        /// Play UI sounds
        /// </summary>
        private void PlayCursorSound()
        {
            SystemManager.Instance?.PlayCursorSE();
        }
        
        private void PlayCancelSound()
        {
            SystemManager.Instance?.PlayCancelSE();
        }
        
        private void PlayErrorSound()
        {
            SystemManager.Instance?.PlayBuzzerSE();
        }
        
        #endregion
    }
}