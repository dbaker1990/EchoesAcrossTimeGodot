using Godot;
using System;
using System.Collections.Generic;
using EchoesAcrossTime.Combat;

namespace EchoesAcrossTime.Items
{
    /// <summary>
    /// Base item data - similar to RPG Maker's item database
    /// </summary>
    [GlobalClass]
    public partial class ItemData : Resource
    {
        [ExportGroup("Basic Info")]
        [Export] public string ItemId { get; set; } = "item_001";
        [Export] public string DisplayName { get; set; } = "Item";
        [Export(PropertyHint.MultilineText)] public string Description { get; set; } = "";
        [Export] public Texture2D Icon { get; set; }
        [Export] public ItemType Type { get; set; } = ItemType.Consumable;
        [Export] public ItemRarity Rarity { get; set; } = ItemRarity.Common;

        [ExportGroup("Economy")]
        [Export] public int Price { get; set; } = 10;
        [Export] public bool CanSell { get; set; } = true;
        [Export] public bool CanDiscard { get; set; } = true;

        [ExportGroup("Stack")]
        [Export] public int MaxStack { get; set; } = 99;
        [Export] public bool IsConsumable { get; set; } = false;

        [ExportGroup("Usage")]
        [Export] public bool UsableInField { get; set; } = true;
        [Export] public bool UsableInBattle { get; set; } = true;

        [ExportGroup("Metadata")]
        [Export(PropertyHint.MultilineText)] public string Note { get; set; } = "";

        /// <summary>
        /// Validate item data
        /// </summary>
        public virtual bool IsValid()
        {
            return !string.IsNullOrEmpty(ItemId) && 
                   !string.IsNullOrEmpty(DisplayName) &&
                   Price >= 0 &&
                   MaxStack > 0;
        }

        /// <summary>
        /// Get selling price (typically half of purchase price)
        /// </summary>
        public int GetSellPrice()
        {
            return CanSell ? Price / 2 : 0;
        }
    }
}