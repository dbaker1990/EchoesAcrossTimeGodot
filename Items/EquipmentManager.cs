using Godot;
using System;
using System.Collections.Generic;
using EchoesAcrossTime.Combat;
using EchoesAcrossTime.Database;

namespace EchoesAcrossTime.Items
{
    /// <summary>
    /// Manages character equipment
    /// </summary>
    public partial class CharacterEquipment : GodotObject
    {
        public string CharacterId { get; set; }
        private Dictionary<EquipSlot, EquipmentData> equipped = new Dictionary<EquipSlot, EquipmentData>();

        [Signal]
        public delegate void EquipmentChangedEventHandler(EquipSlot slot, string itemId);

        public CharacterEquipment(string characterId)
        {
            CharacterId = characterId;

            // Initialize all slots as empty
            equipped[EquipSlot.Weapon] = null;
            equipped[EquipSlot.Armor] = null;
            equipped[EquipSlot.Accessory1] = null;
            equipped[EquipSlot.Accessory2] = null;
        }

        /// <summary>
        /// Equip an item to a slot
        /// </summary>
        public EquipmentData Equip(EquipmentData equipment, CharacterData character)
        {
            if (equipment == null)
            {
                GD.PrintErr("CharacterEquipment: Attempted to equip null equipment");
                return null;
            }

            if (!equipment.CanEquip(character))
            {
                GD.PrintErr($"CharacterEquipment: {character.DisplayName} cannot equip {equipment.DisplayName}");
                return null;
            }

            EquipSlot slot = equipment.Slot;

            // Unequip current item
            EquipmentData previousEquipment = equipped[slot];

            // Equip new item
            equipped[slot] = equipment;

            EmitSignal(SignalName.EquipmentChanged, (int)slot, equipment.ItemId);
            GD.Print($"{character.DisplayName} equipped {equipment.DisplayName}");

            return previousEquipment;
        }

        /// <summary>
        /// Unequip item from slot
        /// </summary>
        public EquipmentData Unequip(EquipSlot slot)
        {
            EquipmentData equipment = equipped[slot];

            if (equipment != null)
            {
                equipped[slot] = null;
                EmitSignal(SignalName.EquipmentChanged, (int)slot, "");
                GD.Print($"Unequipped {equipment.DisplayName}");
            }

            return equipment;
        }

        /// <summary>
        /// Get equipped item in slot
        /// </summary>
        public EquipmentData GetEquipped(EquipSlot slot)
        {
            return equipped.GetValueOrDefault(slot);
        }

        /// <summary>
        /// Get all equipped items
        /// </summary>
        public Dictionary<EquipSlot, EquipmentData> GetAllEquipped()
        {
            return new Dictionary<EquipSlot, EquipmentData>(equipped);
        }

        /// <summary>
        /// Calculate total stat bonuses from equipment
        /// </summary>
        public EquipmentBonuses GetTotalBonuses()
        {
            var bonuses = new EquipmentBonuses();

            foreach (var equipment in equipped.Values)
            {
                if (equipment != null)
                {
                    bonuses.MaxHPBonus += equipment.MaxHPBonus;
                    bonuses.MaxMPBonus += equipment.MaxMPBonus;
                    bonuses.AttackBonus += equipment.AttackBonus;
                    bonuses.DefenseBonus += equipment.DefenseBonus;
                    bonuses.MagicAttackBonus += equipment.MagicAttackBonus;
                    bonuses.MagicDefenseBonus += equipment.MagicDefenseBonus;
                    bonuses.SpeedBonus += equipment.SpeedBonus;
                    bonuses.CriticalRateBonus += equipment.CriticalRateBonus;
                    bonuses.EvasionRateBonus += equipment.EvasionRateBonus;
                }
            }

            return bonuses;
        }

        /// <summary>
        /// Check if slot is empty
        /// </summary>
        public bool IsSlotEmpty(EquipSlot slot)
        {
            return equipped[slot] == null;
        }

        /// <summary>
        /// Unequip all items
        /// </summary>
        public List<EquipmentData> UnequipAll()
        {
            var allEquipment = new List<EquipmentData>();

            foreach (var slot in equipped.Keys)
            {
                var equipment = Unequip(slot);
                if (equipment != null)
                {
                    allEquipment.Add(equipment);
                }
            }

            return allEquipment;
        }
    }

    /// <summary>
    /// Stores combined equipment bonuses
    /// </summary>
    public struct EquipmentBonuses
    {
        public int MaxHPBonus;
        public int MaxMPBonus;
        public int AttackBonus;
        public int DefenseBonus;
        public int MagicAttackBonus;
        public int MagicDefenseBonus;
        public int SpeedBonus;
        public int LuckBonus;
        public int CriticalRateBonus;
        public int EvasionRateBonus;
    }

    /// <summary>
    /// Global equipment manager for all party members
    /// </summary>
    public partial class EquipmentManager : Node
    {
        public static EquipmentManager Instance { get; private set; }

        private Dictionary<string, CharacterEquipment> characterEquipment = new Dictionary<string, CharacterEquipment>();

        [Signal]
        public delegate void CharacterEquipmentChangedEventHandler(string characterId, EquipSlot slot);

        public override void _Ready()
        {
            if (Instance != null)
            {
                QueueFree();
                return;
            }

            Instance = this;
            GD.Print("EquipmentManager initialized");
        }

        /// <summary>
        /// Get or create character equipment
        /// </summary>
        public CharacterEquipment GetCharacterEquipment(string characterId)
        {
            if (!characterEquipment.ContainsKey(characterId))
            {
                characterEquipment[characterId] = new CharacterEquipment(characterId);
            }

            return characterEquipment[characterId];
        }

        /// <summary>
        /// Equip item from inventory
        /// </summary>
        public bool EquipFromInventory(string characterId, string itemId, CharacterData character)
        {
            var item = InventorySystem.Instance?.GetItem(itemId);

            if (item == null || item is not EquipmentData equipment)
            {
                GD.PrintErr($"EquipmentManager: Item {itemId} is not equipment");
                return false;
            }

            if (!equipment.CanEquip(character))
            {
                GD.PrintErr($"EquipmentManager: {character.DisplayName} cannot equip {equipment.DisplayName}");
                return false;
            }

            var charEquipment = GetCharacterEquipment(characterId);

            // Remove from inventory
            if (!InventorySystem.Instance.RemoveItem(itemId, 1))
            {
                return false;
            }

            // Equip the item
            var previousEquipment = charEquipment.Equip(equipment, character);

            // Return previous equipment to inventory
            if (previousEquipment != null)
            {
                InventorySystem.Instance.AddItem(previousEquipment, 1);
            }

            // Emit signal for stats update
            EmitSignal(SignalName.CharacterEquipmentChanged, characterId, (int)equipment.Slot);

            return true;
        }

        /// <summary>
        /// Unequip item to inventory
        /// </summary>
        public bool UnequipToInventory(string characterId, EquipSlot slot)
        {
            var charEquipment = GetCharacterEquipment(characterId);
            var equipment = charEquipment.GetEquipped(slot);

            if (equipment == null)
            {
                GD.Print($"EquipmentManager: Slot {slot} is already empty");
                return false;
            }

            // Check if inventory has space
            if (InventorySystem.Instance.IsFull())
            {
                GD.PrintErr("EquipmentManager: Inventory is full");
                return false;
            }

            // Unequip
            charEquipment.Unequip(slot);

            // Add to inventory
            InventorySystem.Instance.AddItem(equipment, 1);

            // Emit signal for stats update
            EmitSignal(SignalName.CharacterEquipmentChanged, characterId, (int)slot);

            return true;
        }

        /// <summary>
        /// Apply equipment bonuses to character stats
        /// Call this after equipment changes
        /// </summary>
        public void ApplyEquipmentBonuses(string characterId, CharacterStats stats)
        {
            if (stats == null)
            {
                GD.PrintErr("EquipmentManager: Cannot apply bonuses to null stats");
                return;
            }

            var equipment = GetCharacterEquipment(characterId);
            var bonuses = equipment.GetTotalBonuses();

            // Apply bonuses to stats
            stats.ApplyEquipmentBonuses(bonuses);

            GD.Print($"Applied equipment bonuses to {stats.CharacterName}");
        }

        /// <summary>
        /// Get equipment bonuses for character
        /// </summary>
        public EquipmentBonuses GetCharacterBonuses(string characterId)
        {
            var equipment = GetCharacterEquipment(characterId);
            return equipment.GetTotalBonuses();
        }

        /// <summary>
        /// Check if character has item equipped in any slot
        /// </summary>
        public bool HasItemEquipped(string characterId, string itemId)
        {
            var equipment = GetCharacterEquipment(characterId);
            var allEquipped = equipment.GetAllEquipped();

            foreach (var item in allEquipped.Values)
            {
                if (item != null && item.ItemId == itemId)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Get equipped item in specific slot
        /// </summary>
        public EquipmentData GetEquippedItem(string characterId, EquipSlot slot)
        {
            var equipment = GetCharacterEquipment(characterId);
            return equipment.GetEquipped(slot);
        }

        /// <summary>
        /// Unequip all items for character
        /// </summary>
        public void UnequipAll(string characterId)
        {
            var equipment = GetCharacterEquipment(characterId);
            var items = equipment.UnequipAll();

            // Add all items back to inventory
            foreach (var item in items)
            {
                InventorySystem.Instance.AddItem(item, 1);
            }

            EmitSignal(SignalName.CharacterEquipmentChanged, characterId, -1); // -1 indicates all slots
        }
    }
}