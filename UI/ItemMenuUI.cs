using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using EchoesAcrossTime.Combat;
using EchoesAcrossTime.Items;

namespace EchoesAcrossTime.UI
{
    /// <summary>
    /// Item menu for browsing and using items outside of battle
    /// </summary>
    public partial class ItemMenuUI : Control
    {
        #region Node References
        [Export] private ItemList itemList;
        [Export] private Label itemNameLabel;
        [Export] private Label itemDescriptionLabel;
        [Export] private Label itemQuantityLabel;
        [Export] private Label goldLabel;
        [Export] private Button useButton;
        [Export] private Button discardButton;
        [Export] private Button sortButton;
        [Export] private Button closeButton;
        [Export] private OptionButton categoryFilter;
        
        // Character selection for consumables
        [Export] private Control characterSelectionPanel;
        [Export] private VBoxContainer characterList;
        #endregion
        
        #region State
        private List<InventorySlot> currentItems;
        private InventorySlot selectedSlot;
        private ItemData selectedItem;
        private CharacterStats selectedCharacter;
        #endregion
        
        public override void _Ready()
        {
            // Connect signals
            itemList.ItemSelected += OnItemSelected;
            useButton.Pressed += OnUsePressed;
            discardButton.Pressed += OnDiscardPressed;
            sortButton.Pressed += OnSortPressed;
            closeButton.Pressed += OnClosePressed;
            categoryFilter.ItemSelected += OnCategoryChanged;
            
            // Connect to inventory changes
            if (InventorySystem.Instance != null)
            {
                InventorySystem.Instance.ItemAdded += (item, qty) => RefreshItemList();
                InventorySystem.Instance.ItemRemoved += (item, qty) => RefreshItemList();
                InventorySystem.Instance.GoldChanged += OnGoldChanged;
            }
            
            // Setup category filter
            SetupCategoryFilter();
            
            Hide();
        }
        
        private void SetupCategoryFilter()
        {
            categoryFilter.Clear();
            categoryFilter.AddItem("All", 0);
            categoryFilter.AddItem("Consumables", 1);
            categoryFilter.AddItem("Key Items", 2);
            categoryFilter.AddItem("Materials", 3);
            categoryFilter.AddItem("Equipment", 4);
        }
        
        public void OpenMenu()
        {
            Show();
            RefreshItemList();
            UpdateGoldDisplay();
            
            // Focus item list
            if (itemList.ItemCount > 0)
            {
                itemList.Select(0);
                OnItemSelected(0);
            }
            
            itemList.GrabFocus();
        }
        
        private void RefreshItemList()
        {
            itemList.Clear();
            
            // Get filtered items
            currentItems = GetFilteredItems();
            
            // Populate list
            foreach (var slot in currentItems)
            {
                var displayText = $"{slot.Item.DisplayName} x{slot.Quantity}";
                itemList.AddItem(displayText);
                
                // Add icon if available
                if (slot.Item.Icon != null)
                {
                    itemList.SetItemIcon(itemList.ItemCount - 1, slot.Item.Icon);
                }
            }
            
            // Update buttons
            UpdateButtonStates();
        }
        
        private List<InventorySlot> GetFilteredItems()
        {
            if (InventorySystem.Instance == null)
                return new List<InventorySlot>();
            
            var allItems = InventorySystem.Instance.GetAllItems();
            
            // Apply category filter
            int selectedCategory = categoryFilter.Selected;
            
            return selectedCategory switch
            {
                1 => allItems.Where(s => s.Item.Type == ItemType.Consumable).ToList(),
                2 => allItems.Where(s => s.Item.Type == ItemType.KeyItem).ToList(),
                3 => allItems.Where(s => s.Item.Type == ItemType.Material).ToList(),
                4 => allItems.Where(s => s.Item.Type == ItemType.Weapon).ToList(),
                5 => allItems.Where(s => s.Item.Type == ItemType.Armor).ToList(),
                6 => allItems.Where(s => s.Item.Type == ItemType.Accessory).ToList(),
                _ => allItems
            };
        }
        
        private void OnItemSelected(long index)
        {
            if (index < 0 || index >= currentItems.Count)
                return;
            
            selectedSlot = currentItems[(int)index];
            selectedItem = selectedSlot.Item;
            
            // Update detail panel
            itemNameLabel.Text = selectedItem.DisplayName;
            itemDescriptionLabel.Text = selectedItem.Description;
            itemQuantityLabel.Text = $"x{selectedSlot.Quantity}";
            
            // Update buttons
            UpdateButtonStates();
            
            Managers.SystemManager.Instance?.PlayCursorSE();
        }
        
        private void UpdateButtonStates()
        {
            bool hasSelection = selectedItem != null;
            
            // Can only use consumables
            useButton.Disabled = !hasSelection || selectedItem.Type != ItemType.Consumable;
            
            // Can't discard key items
            discardButton.Disabled = !hasSelection || selectedItem.Type == ItemType.KeyItem;
        }
        
        private void OnUsePressed()
        {
            if (selectedItem == null || selectedItem.Type != ItemType.Consumable)
                return;
            
            Managers.SystemManager.Instance?.PlayOkSE();
            
            // Show character selection
            ShowCharacterSelection();
        }
        
        private void ShowCharacterSelection()
        {
            // Clear existing character buttons
            foreach (var child in characterList.GetChildren())
            {
                child.QueueFree();
            }
            
            // Get party members
            var party = Party.PartyManager.Instance?.GetMainParty();
            if (party == null || party.Count == 0)
            {
                GD.PrintErr("No party members found!");
                return;
            }
            
            // Create button for each character
            foreach (var stats in party)
            {
                var button = new Button();
                button.CustomMinimumSize = new Vector2(300, 60);
                button.Text = $"{stats.CharacterName}\nHP: {stats.CurrentHP}/{stats.MaxHP}  MP: {stats.CurrentMP}/{stats.MaxMP}";
                
                var characterStats = stats; // Capture for lambda
                button.Pressed += () => OnCharacterSelected(characterStats);
                
                characterList.AddChild(button);
            }
            
            characterSelectionPanel.Show();
        }
        
        private void OnCharacterSelected(CharacterStats character)
        {
            selectedCharacter = character;
            
            // Use item on character
            UseItemOnCharacter();
            
            // Hide character selection
            characterSelectionPanel.Hide();
        }
        
        private void UseItemOnCharacter()
        {
            if (selectedItem == null || selectedCharacter == null)
                return;
            
            var consumable = selectedItem as ConsumableData;
            if (consumable == null)
                return;
            
            // Apply effects
            bool used = false;
            
            // HP recovery
            if (consumable.HPRestore > 0)
            {
                int recovered = selectedCharacter.Heal(consumable.RestoresHP);
                GD.Print($"{selectedCharacter.CharacterName} recovered {recovered} HP!");
                used = true;
            }
            
            // MP recovery
            if (consumable.MPRestore > 0)
            {
                selectedCharacter.RestoreMP(consumable.RestoresMP);
                GD.Print($"{selectedCharacter.CharacterName} recovered {consumable.RestoresMP} MP!");
                used = true;
            }
            
            // Status cure
            if (consumable.CuresStatus != null && consumable.CuresStatus.Count > 0)
            {
                // TODO: Implement status cure
                used = true;
            }
            
            if (used)
            {
                // Remove item from inventory
                InventorySystem.Instance?.RemoveItem(selectedItem.ItemId, 1);
                
                // Play sound
                Managers.SystemManager.Instance.PlayOkSE();
                
                // Refresh display
                RefreshItemList();
            }
        }
        
        private void OnDiscardPressed()
        {
            if (selectedItem == null || selectedItem.Type == ItemType.KeyItem)
                return;
            
            Managers.SystemManager.Instance?.PlayOkSE();
            
            // Show confirmation dialog
            var dialog = new AcceptDialog();
            dialog.DialogText = $"Discard {selectedItem.DisplayName}?";
            dialog.Title = "Confirm Discard";
            dialog.Confirmed += () =>
            {
                InventorySystem.Instance?.RemoveItem(selectedItem.ItemId, 1);
                RefreshItemList();
            };
            
            AddChild(dialog);
            dialog.PopupCentered();
        }
        
        private void OnSortPressed()
        {
            InventorySystem.Instance?.SortInventory();
            RefreshItemList();
            Managers.SystemManager.Instance?.PlayOkSE();
        }
        
        private void OnCategoryChanged(long index)
        {
            RefreshItemList();
            Managers.SystemManager.Instance?.PlayCursorSE();
        }
        
        private void OnGoldChanged(int newGold)
        {
            UpdateGoldDisplay();
        }
        
        private void UpdateGoldDisplay()
        {
            if (goldLabel != null && InventorySystem.Instance != null)
            {
                goldLabel.Text = $"Gold: {InventorySystem.Instance.GetGold()} G";
            }
        }
        
        private void OnClosePressed()
        {
            Hide();
            
            // Return to main menu
            var mainMenu = GetParent().GetNodeOrNull<MainMenuUI>("%MainMenuUI");
            mainMenu?.ReturnToMainMenu();
        }
        
        public override void _Input(InputEvent @event)
        {
            if (!Visible) return;
            
            if (@event.IsActionPressed("ui_cancel"))
            {
                if (characterSelectionPanel.Visible)
                {
                    characterSelectionPanel.Hide();
                    Managers.SystemManager.Instance?.PlayCancelSE();
                }
                else
                {
                    OnClosePressed();
                }
                GetViewport().SetInputAsHandled();
            }
        }
    }
}