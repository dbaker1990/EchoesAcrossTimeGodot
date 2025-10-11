using Godot;
using EchoesAcrossTime.Items;

namespace EchoesAcrossTime.Combat
{
    /// <summary>
    /// Represents a single drop item with chance
    /// STANDALONE FILE - separate from EnemyRewards
    /// </summary>
    [GlobalClass]
    public partial class DropItem : Resource
    {
        [ExportGroup("Item Reference")]
        [Export] public ItemData Item { get; set; }
        [Export] public string ItemId { get; set; } = "";
        
        [ExportGroup("Drop Settings")]
        [Export(PropertyHint.Range, "1,100")] public int DropChance { get; set; } = 100;
        [Export] public int MinQuantity { get; set; } = 1;
        [Export] public int MaxQuantity { get; set; } = 1;
        
        /// <summary>
        /// Get the actual item ID to use (Item takes priority)
        /// </summary>
        public string GetItemId()
        {
            if (Item != null && !string.IsNullOrEmpty(Item.ItemId))
                return Item.ItemId;
            return ItemId;
        }
        
        /// <summary>
        /// Roll to see if this item drops
        /// </summary>
        public bool RollDrop(RandomNumberGenerator rng = null)
        {
            rng ??= new RandomNumberGenerator();
            return rng.Randf() * 100 < DropChance;
        }
        
        /// <summary>
        /// Get random quantity
        /// </summary>
        public int GetQuantity(RandomNumberGenerator rng = null)
        {
            rng ??= new RandomNumberGenerator();
            return rng.RandiRange(MinQuantity, MaxQuantity);
        }
        
        /// <summary>
        /// Check if this drop is valid
        /// </summary>
        public bool IsValid()
        {
            return (Item != null && !string.IsNullOrEmpty(Item.ItemId)) || 
                   !string.IsNullOrEmpty(ItemId);
        }
    }
}