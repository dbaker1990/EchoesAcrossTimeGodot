using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using EchoesAcrossTime.Combat;
using RPG.Items;

namespace EchoesAcrossTime.Items
{
    /// <summary>
    /// Represents an item stack in inventory
    /// </summary>
    public partial class InventorySlot : GodotObject
    {
        public ItemData Item { get; set; }
        public int Quantity { get; set; }

        public InventorySlot(ItemData item, int quantity = 1)
        {
            Item = item;
            Quantity = quantity;
        }

        public bool CanStack(ItemData item)
        {
            return Item.ItemId == item.ItemId && Quantity < Item.MaxStack;
        }

        public int GetRemainingSpace()
        {
            return Item.MaxStack - Quantity;
        }
    }

    /// <summary>
    /// Main inventory management system
    /// </summary>
    public partial class InventorySystem : Node
    {
        public static InventorySystem Instance { get; private set; }

        [ExportGroup("Inventory Settings")]
        [Export] public int MaxInventorySlots { get; set; } = 100;
        [Export] public bool AutoStack { get; set; } = true;
        [Export] public bool AutoSort { get; set; } = false;

        private List<InventorySlot> inventory = new List<InventorySlot>();
        private int gold = 0;

        [Signal]
        public delegate void ItemAddedEventHandler(string itemId, int quantity);

        [Signal]
        public delegate void ItemRemovedEventHandler(string itemId, int quantity);

        [Signal]
        public delegate void GoldChangedEventHandler(int newAmount);

        [Signal]
        public delegate void InventoryFullEventHandler();

        public override void _Ready()
        {
            if (Instance != null)
            {
                QueueFree();
                return;
            }

            Instance = this;
            GD.Print("InventorySystem initialized");
        }

        /// <summary>
        /// Add item to inventory
        /// </summary>
        public bool AddItem(ItemData item, int quantity = 1)
        {
            if (item == null || quantity <= 0)
            {
                GD.PrintErr("InventorySystem: Invalid item or quantity");
                return false;
            }

            int remainingQuantity = quantity;

            // Try to stack with existing items
            if (AutoStack)
            {
                foreach (var slot in inventory)
                {
                    if (slot.CanStack(item))
                    {
                        int spaceAvailable = slot.GetRemainingSpace();
                        int amountToAdd = Mathf.Min(remainingQuantity, spaceAvailable);

                        slot.Quantity += amountToAdd;
                        remainingQuantity -= amountToAdd;

                        if (remainingQuantity <= 0)
                        {
                            EmitSignal(SignalName.ItemAdded, item.ItemId, quantity);
                            GD.Print($"Added {quantity}x {item.DisplayName} to inventory");
                            return true;
                        }
                    }
                }
            }

            // Create new slots for remaining items
            while (remainingQuantity > 0)
            {
                if (inventory.Count >= MaxInventorySlots)
                {
                    EmitSignal(SignalName.InventoryFull);
                    GD.PrintErr("InventorySystem: Inventory is full!");
                    return false;
                }

                int amountForThisSlot = Mathf.Min(remainingQuantity, item.MaxStack);
                inventory.Add(new InventorySlot(item, amountForThisSlot));
                remainingQuantity -= amountForThisSlot;
            }

            EmitSignal(SignalName.ItemAdded, item.ItemId, quantity);
            GD.Print($"Added {quantity}x {item.DisplayName} to inventory");

            if (AutoSort)
            {
                SortInventory();
            }

            return true;
        }

        /// <summary>
        /// Remove item from inventory
        /// </summary>
        public bool RemoveItem(string itemId, int quantity = 1)
        {
            if (!HasItem(itemId, quantity))
            {
                GD.PrintErr($"InventorySystem: Not enough {itemId} in inventory");
                return false;
            }

            int remainingToRemove = quantity;

            for (int i = inventory.Count - 1; i >= 0 && remainingToRemove > 0; i--)
            {
                var slot = inventory[i];
                if (slot.Item.ItemId == itemId)
                {
                    int amountToRemove = Mathf.Min(remainingToRemove, slot.Quantity);
                    slot.Quantity -= amountToRemove;
                    remainingToRemove -= amountToRemove;

                    if (slot.Quantity <= 0)
                    {
                        inventory.RemoveAt(i);
                    }
                }
            }

            EmitSignal(SignalName.ItemRemoved, itemId, quantity);
            GD.Print($"Removed {quantity}x {itemId} from inventory");
            return true;
        }

        /// <summary>
        /// Check if inventory has item
        /// </summary>
        public bool HasItem(string itemId, int quantity = 1)
        {
            return GetItemCount(itemId) >= quantity;
        }

        /// <summary>
        /// Get total count of an item
        /// </summary>
        public int GetItemCount(string itemId)
        {
            return inventory
                .Where(slot => slot.Item.ItemId == itemId)
                .Sum(slot => slot.Quantity);
        }

        /// <summary>
        /// Get item data by ID
        /// </summary>
        public ItemData GetItem(string itemId)
        {
            var slot = inventory.FirstOrDefault(s => s.Item.ItemId == itemId);
            return slot?.Item;
        }

        /// <summary>
        /// Use a consumable item
        /// </summary>
        public bool UseItem(string itemId, CharacterStats target)
        {
            var item = GetItem(itemId);

            if (item == null)
            {
                GD.PrintErr($"InventorySystem: Item {itemId} not found");
                return false;
            }

            if (!item.IsConsumable)
            {
                GD.PrintErr($"InventorySystem: Item {itemId} is not consumable");
                return false;
            }

            if (item is ConsumableData consumable)
            {
                consumable.Use(target);
                RemoveItem(itemId, 1);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get all items of a specific type
        /// </summary>
        public List<InventorySlot> GetItemsByType(ItemType type)
        {
            return inventory.Where(slot => slot.Item.Type == type).ToList();
        }

        /// <summary>
        /// Get all inventory slots
        /// </summary>
        public List<InventorySlot> GetAllItems()
        {
            return new List<InventorySlot>(inventory);
        }

        /// <summary>
        /// Sort inventory by type and rarity
        /// </summary>
        public void SortInventory()
        {
            inventory = inventory
                .OrderBy(slot => slot.Item.Type)
                .ThenByDescending(slot => slot.Item.Rarity)
                .ThenBy(slot => slot.Item.DisplayName)
                .ToList();

            GD.Print("Inventory sorted");
        }

        /// <summary>
        /// Clear entire inventory
        /// </summary>
        public void ClearInventory()
        {
            inventory.Clear();
            GD.Print("Inventory cleared");
        }

        /// <summary>
        /// Get number of used slots
        /// </summary>
        public int GetUsedSlots()
        {
            return inventory.Count;
        }

        /// <summary>
        /// Get number of free slots
        /// </summary>
        public int GetFreeSlots()
        {
            return MaxInventorySlots - inventory.Count;
        }

        /// <summary>
        /// Check if inventory is full
        /// </summary>
        public bool IsFull()
        {
            return inventory.Count >= MaxInventorySlots;
        }

        // Gold management
        public int GetGold() => gold;

        public void AddGold(int amount)
        {
            if (amount <= 0) return;

            gold += amount;
            EmitSignal(SignalName.GoldChanged, gold);
            GD.Print($"Added {amount} gold. Total: {gold}");
        }

        public bool RemoveGold(int amount)
        {
            if (amount <= 0) return false;
            if (gold < amount) return false;

            gold -= amount;
            EmitSignal(SignalName.GoldChanged, gold);
            GD.Print($"Removed {amount} gold. Total: {gold}");
            return true;
        }

        public bool HasGold(int amount)
        {
            return gold >= amount;
        }
    }
}