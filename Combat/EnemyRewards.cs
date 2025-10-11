using Godot;
using System;
using System.Collections.Generic;

namespace EchoesAcrossTime.Combat
{
    /// <summary>
    /// Enemy reward data for drops and experience
    /// DropItem is now in a separate file: Combat/DropItem.cs
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
        
        public int GetExpReward(RandomNumberGenerator rng = null)
        {
            rng ??= new RandomNumberGenerator();
            int baseExp = IsBoss ? BaseExpReward * BossExpMultiplier : BaseExpReward;
            float variance = 1.0f + ((rng.Randf() * 2 - 1) * ExpVariance);
            return Mathf.RoundToInt(baseExp * variance);
        }
        
        public int GetGoldReward(RandomNumberGenerator rng = null)
        {
            rng ??= new RandomNumberGenerator();
            int baseGold = IsBoss ? BaseGoldReward * BossGoldMultiplier : BaseGoldReward;
            float variance = 1.0f + ((rng.Randf() * 2 - 1) * GoldVariance);
            return Mathf.RoundToInt(baseGold * variance);
        }
        
        public List<(string itemId, int quantity)> RollAllDrops(RandomNumberGenerator rng = null)
        {
            rng ??= new RandomNumberGenerator();
            var droppedItems = new List<(string, int)>();
            
            if (GuaranteedDrop != null && GuaranteedDrop.IsValid())
                droppedItems.Add((GuaranteedDrop.GetItemId(), GuaranteedDrop.GetQuantity(rng)));
            
            foreach (var drop in CommonDrops)
                if (drop != null && drop.IsValid() && drop.RollDrop(rng))
                    droppedItems.Add((drop.GetItemId(), drop.GetQuantity(rng)));
            
            foreach (var drop in UncommonDrops)
                if (drop != null && drop.IsValid() && drop.RollDrop(rng))
                    droppedItems.Add((drop.GetItemId(), drop.GetQuantity(rng)));
            
            foreach (var drop in RareDrops)
                if (drop != null && drop.IsValid() && drop.RollDrop(rng))
                    droppedItems.Add((drop.GetItemId(), drop.GetQuantity(rng)));
            
            return droppedItems;
        }
        
        public (bool success, string itemId, int quantity) AttemptSteal(int thiefLuck = 0, RandomNumberGenerator rng = null)
        {
            rng ??= new RandomNumberGenerator();
            
            if (StealableItems.Count == 0)
                return (false, null, 0);
            
            int finalStealChance = BaseStealChance + thiefLuck;
            finalStealChance = Mathf.Clamp(finalStealChance, 1, 95);
            
            if (rng.Randf() * 100 >= finalStealChance)
                return (false, null, 0);
            
            var stolenItem = StealableItems[rng.RandiRange(0, StealableItems.Count - 1)];
            
            if (stolenItem == null || !stolenItem.IsValid())
                return (false, null, 0);
            
            if (!stolenItem.RollDrop(rng))
                return (false, null, 0);
            
            return (true, stolenItem.GetItemId(), stolenItem.GetQuantity(rng));
        }
    }
}