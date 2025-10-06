using Godot;
using System;
using System.Collections.Generic;

namespace EchoesAcrossTime.Shops
{
    /// <summary>
    /// Shop UI - displays shop interface for buying/selling
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
            ShopManager.Instance.ShopOpened += OnShopOpened;
            ShopManager.Instance.ShopClosed += OnShopClosed;
            ShopManager.Instance.ItemBought += OnItemBought;
            ShopManager.Instance.ItemSold += OnItemSold;
            ShopManager.Instance.TransactionFailed += OnTransactionFailed;
            
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
                bool isUnlocked = ShopManager.Instance.IsShopUnlocked(currentShop);
                if (!isUnlocked && !shopItem.ShowIfLocked)
                    continue;
                
                // Get item data (you'll integrate with your ItemData system)
                var itemData = GetItemData(shopItem.ItemId);
                if (itemData == null) continue;
                
                // Build display text
                string displayText = $"{itemData.ItemName}";
                
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
            
            // Get player's inventory (integrate with your InventorySystem)
            var inventory = GetPlayerInventory();
            
            foreach (var item in inventory)
            {
                string itemId = item.Key;
                int quantity = item.Value;
                
                if (quantity <= 0) continue;
                
                var itemData = GetItemData(itemId);
                if (itemData == null) continue;
                
                int sellPrice = ShopManager.Instance.GetSellPrice(itemId);
                
                string displayText = $"{itemData.ItemName} ({quantity}) - {sellPrice}G each";
                
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
                itemNameLabel.Text = itemData.ItemName;
            
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
                ShowNotification($"Purchased {quantity}x item!");
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
                ShowNotification($"Sold {quantity}x item!");
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
            // TODO: Show actual notification popup
        }
        
        #endregion
        
        #region Integration Points - Connect to your systems
        
        private ItemData GetItemData(string itemId)
        {
            // TODO: Get from your GameDatabase
            // Placeholder for now
            return new ItemData
            {
                ItemName = itemId,
                Description = "Item description here",
                Icon = null
            };
        }
        
        private Dictionary<string, int> GetPlayerInventory()
        {
            // TODO: Get from InventorySystem
            return new Dictionary<string, int>();
        }
        
        private int GetItemCount(string itemId)
        {
            // TODO: Get from InventorySystem
            var inventorySystem = GetNodeOrNull<Node>("/root/InventorySystem");
            if (inventorySystem != null && inventorySystem.HasMethod("GetItemCount"))
            {
                return (int)inventorySystem.Call("GetItemCount", itemId);
            }
            return 0;
        }
        
        private int GetPlayerGold()
        {
            var saveManager = GetNodeOrNull<Node>("/root/SaveManager");
            if (saveManager != null && saveManager.HasMethod("GetGold"))
            {
                return (int)saveManager.Call("GetGold");
            }
            return 9999;
        }
        
        private void PlayCursorSound()
        {
            var systemManager = GetNodeOrNull<Node>("/root/SystemManager");
            systemManager?.Call("PlayCursorSE");
        }
        
        private void PlayCancelSound()
        {
            var systemManager = GetNodeOrNull<Node>("/root/SystemManager");
            systemManager?.Call("PlayCancelSE");
        }
        
        private void PlayErrorSound()
        {
            var systemManager = GetNodeOrNull<Node>("/root/SystemManager");
            systemManager?.Call("PlayErrorSE");
        }
        
        #endregion
    }
    
    // Placeholder class - remove when integrating with your actual ItemData
    public class ItemData
    {
        public string ItemName { get; set; }
        public string Description { get; set; }
        public Texture2D Icon { get; set; }
    }
}