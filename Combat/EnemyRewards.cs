using Godot;
using System;
using System.Collections.Generic;

namespace EchoesAcrossTime.Combat
{
    /// <summary>
    /// Represents a single drop item with chance
    /// </summary>
    [GlobalClass]
    public partial class DropItem : Resource
    {
        [Export] public string ItemId { get; set; }
        [Export(PropertyHint.Range, "1,100")] public int DropChance { get; set; } = 100;
        [Export] public int MinQuantity { get; set; } = 1;
        [Export] public int MaxQuantity { get; set; } = 1;
        
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
    }
    
    /// <summary>
    /// Enemy reward data for drops and experience
    /// </summary>
    [GlobalClass]
    public partial class EnemyRewards : Resource
    {
        [ExportGroup("Experience & Gold")]
        [Export] public int BaseExpReward { get; set; } = 10;
        [Export] public int BaseGoldReward { get; set; } = 5;
        [Export] public float ExpVariance { get; set; } = 0.1f;
        [Export] public float GoldVariance { get; set; } = 0.2f;
        
        [ExportGroup("Drop Items")]
        [Export] public Godot.Collections.Array<DropItem> CommonDrops { get; set; }
        [Export] public Godot.Collections.Array<DropItem> UncommonDrops { get; set; }
        [Export] public Godot.Collections.Array<DropItem> RareDrops { get; set; }
        [Export] public DropItem GuaranteedDrop { get; set; }
        
        [ExportGroup("Steal Items")]
        [Export] public Godot.Collections.Array<DropItem> StealableItems { get; set; }
        [Export] public int BaseStealChance { get; set; } = 30;
        
        [ExportGroup("Special Rewards")]
        [Export] public bool IsBoss { get; set; } = false;
        [Export] public int BossExpMultiplier { get; set; } = 5;
        [Export] public int BossGoldMultiplier { get; set; } = 10;
        
        public EnemyRewards()
        {
            CommonDrops = new Godot.Collections.Array<DropItem>();
            UncommonDrops = new Godot.Collections.Array<DropItem>();
            RareDrops = new Godot.Collections.Array<DropItem>();
            StealableItems = new Godot.Collections.Array<DropItem>();
        }
        
        /// <summary>
        /// Calculate final exp reward with variance
        /// </summary>
        public int GetExpReward(RandomNumberGenerator rng = null)
        {
            rng ??= new RandomNumberGenerator();
            
            int baseExp = IsBoss ? BaseExpReward * BossExpMultiplier : BaseExpReward;
            float variance = 1.0f + ((rng.Randf() * 2 - 1) * ExpVariance);
            
            return Mathf.RoundToInt(baseExp * variance);
        }
        
        /// <summary>
        /// Calculate final gold reward with variance
        /// </summary>
        public int GetGoldReward(RandomNumberGenerator rng = null)
        {
            rng ??= new RandomNumberGenerator();
            
            int baseGold = IsBoss ? BaseGoldReward * BossGoldMultiplier : BaseGoldReward;
            float variance = 1.0f + ((rng.Randf() * 2 - 1) * GoldVariance);
            
            return Mathf.RoundToInt(baseGold * variance);
        }
        
        /// <summary>
        /// Roll all drops and return list of items that dropped
        /// </summary>
        public List<(string itemId, int quantity)> RollAllDrops(RandomNumberGenerator rng = null)
        {
            rng ??= new RandomNumberGenerator();
            var droppedItems = new List<(string, int)>();
            
            // Guaranteed drop
            if (GuaranteedDrop != null)
            {
                droppedItems.Add((GuaranteedDrop.ItemId, GuaranteedDrop.GetQuantity(rng)));
            }
            
            // Common drops
            foreach (var drop in CommonDrops)
            {
                if (drop.RollDrop(rng))
                {
                    droppedItems.Add((drop.ItemId, drop.GetQuantity(rng)));
                }
            }
            
            // Uncommon drops
            foreach (var drop in UncommonDrops)
            {
                if (drop.RollDrop(rng))
                {
                    droppedItems.Add((drop.ItemId, drop.GetQuantity(rng)));
                }
            }
            
            // Rare drops
            foreach (var drop in RareDrops)
            {
                if (drop.RollDrop(rng))
                {
                    droppedItems.Add((drop.ItemId, drop.GetQuantity(rng)));
                }
            }
            
            return droppedItems;
        }
        
        /// <summary>
        /// Attempt to steal an item
        /// </summary>
        public (bool success, string itemId, int quantity) AttemptSteal(int thiefLuck = 0, RandomNumberGenerator rng = null)
        {
            rng ??= new RandomNumberGenerator();
            
            if (StealableItems.Count == 0)
            {
                return (false, null, 0);
            }
            
            // Calculate steal chance
            int finalStealChance = BaseStealChance + thiefLuck;
            finalStealChance = Mathf.Clamp(finalStealChance, 1, 95);
            
            if (rng.Randf() * 100 >= finalStealChance)
            {
                return (false, null, 0);
            }
            
            // Choose random stealable item
            var stolenItem = StealableItems[rng.RandiRange(0, StealableItems.Count - 1)];
            
            // Check item's drop chance
            if (!stolenItem.RollDrop(rng))
            {
                return (false, null, 0);
            }
            
            return (true, stolenItem.ItemId, stolenItem.GetQuantity(rng));
        }
    }
}