// Combat/StealMugSystem.cs
using Godot;
using System.Collections.Generic;
using System.Linq;
using EchoesAcrossTime.Items;

namespace EchoesAcrossTime.Combat
{
    /// <summary>
    /// Handles Steal and Mug mechanics from Final Fantasy
    /// Steal: Attempt to steal items/gold from enemies
    /// Mug: Attack + Steal in one action
    /// </summary>
    public partial class StealMugSystem : Node
    {
        private RandomNumberGenerator rng;
        
        // Track what has already been stolen from each enemy
        private Dictionary<BattleMember, HashSet<string>> stolenItems = new();
        private Dictionary<BattleMember, bool> stolenGold = new();
        
        public override void _Ready()
        {
            rng = new RandomNumberGenerator();
        }
        
        /// <summary>
        /// Attempt to steal from an enemy
        /// </summary>
        public StealResult AttemptSteal(BattleMember thief, BattleMember target)
        {
            var result = new StealResult
            {
                Success = false,
                Message = "Steal failed!"
            };
            
            // Cannot steal from already stolen enemy (in some FF games)
            // Comment this out if you want unlimited steal attempts
            if (HasBeenFullyStolen(target))
            {
                result.Message = "Nothing left to steal!";
                return result;
            }
            
            // Get enemy's reward data from SourceData
            var enemyRewards = target.SourceData?.Rewards;
            if (enemyRewards == null)
            {
                result.Message = "Nothing to steal!";
                return result;
            }
            
            // Calculate steal success chance
            int thiefLuck = thief.Stats.Luck;
            var stealAttempt = enemyRewards.AttemptSteal(thiefLuck, rng);
            
            if (!stealAttempt.success)
            {
                result.Message = "Steal failed!";
                return result;
            }
            
            // Successfully stole an item
            string itemId = stealAttempt.itemId;
            int quantity = stealAttempt.quantity;
            
            // Check if this item was already stolen
            if (!stolenItems.ContainsKey(target))
            {
                stolenItems[target] = new HashSet<string>();
            }
            
            if (stolenItems[target].Contains(itemId))
            {
                // Already stole this item, try for gold instead
                return AttemptStealGold(thief, target);
            }
            
            // Add item to inventory
            var itemData = GameManager.Instance?.Database?.GetItem(itemId);
            if (itemData != null)
            {
                InventorySystem.Instance?.AddItem(itemData, quantity);
                stolenItems[target].Add(itemId);
                
                result.Success = true;
                result.ItemStolen = itemData;
                result.Quantity = quantity;
                result.Message = $"Stole {quantity}x {itemData.DisplayName}!";
                
                GD.Print($"[STEAL] Successfully stole {quantity}x {itemData.DisplayName}");
            }
            
            return result;
        }
        
        /// <summary>
        /// Attempt to steal gold from enemy
        /// </summary>
        private StealResult AttemptStealGold(BattleMember thief, BattleMember target)
        {
            var result = new StealResult
            {
                Success = false,
                Message = "Nothing left to steal!"
            };
            
            // Check if gold already stolen
            if (stolenGold.ContainsKey(target) && stolenGold[target])
            {
                return result;
            }
            
            var enemyRewards = target.SourceData?.Rewards;
            if (enemyRewards == null)
            {
                return result;
            }
            
            // Calculate steal chance for gold (usually lower than items)
            int stealChance = enemyRewards.BaseStealChance / 2; // Half chance for gold
            stealChance += thief.Stats.Luck / 4;
            stealChance = Mathf.Clamp(stealChance, 5, 50); // Max 50% for gold
            
            if (rng.Randf() * 100 < stealChance)
            {
                // Steal partial gold (20-40% of enemy's gold reward)
                int goldAmount = enemyRewards.GetGoldReward(rng);
                goldAmount = Mathf.RoundToInt(goldAmount * (0.2f + rng.Randf() * 0.2f));
                goldAmount = Mathf.Max(1, goldAmount); // Minimum 1 gold
                
                InventorySystem.Instance?.AddGold(goldAmount);
                stolenGold[target] = true;
                
                result.Success = true;
                result.GoldStolen = goldAmount;
                result.Message = $"Stole {goldAmount} gold!";
                
                GD.Print($"[STEAL] Successfully stole {goldAmount} gold");
            }
            
            return result;
        }
        
        /// <summary>
        /// Execute Mug: Attack + Steal
        /// </summary>
        public async System.Threading.Tasks.Task<MugResult> ExecuteMug(
            BattleMember attacker, 
            BattleMember target,
            SkillData mugSkill,
            System.Func<BattleMember, BattleMember, SkillData, System.Threading.Tasks.Task<BattleActionResult>> damageFunction)
        {
            var mugResult = new MugResult();
            
            GD.Print($"[MUG] {attacker.Stats.CharacterName} mugs {target.Stats.CharacterName}!");
            
            // First, perform the attack
            mugResult.AttackResult = await damageFunction(attacker, target, mugSkill);
            
            // Then attempt to steal (only if target survived)
            if (target.Stats.IsAlive)
            {
                mugResult.StealResult = AttemptSteal(attacker, target);
            }
            else
            {
                // Target died, but we still try to loot them
                mugResult.StealResult = AttemptSteal(attacker, target);
            }
            
            return mugResult;
        }
        
        /// <summary>
        /// Check if enemy has been fully looted
        /// </summary>
        private bool HasBeenFullyStolen(BattleMember target)
        {
            var enemyRewards = target.SourceData?.Rewards;
            if (enemyRewards == null) return true;
            
            // Check if all stealable items taken
            bool allItemsStolen = false;
            if (stolenItems.ContainsKey(target))
            {
                int totalStealableItems = enemyRewards.StealableItems?.Count ?? 0;
                allItemsStolen = totalStealableItems > 0 && 
                                stolenItems[target].Count >= totalStealableItems;
            }
            
            // Check if gold stolen
            bool goldStolen = stolenGold.ContainsKey(target) && stolenGold[target];
            
            return allItemsStolen && goldStolen;
        }
        
        /// <summary>
        /// Reset steal tracking (call when battle ends)
        /// </summary>
        public void ResetStealTracking()
        {
            stolenItems.Clear();
            stolenGold.Clear();
            GD.Print("[STEAL] Steal tracking reset for new battle");
        }
        
        /// <summary>
        /// Get steal success chance display for UI
        /// </summary>
        public int GetStealChanceDisplay(BattleMember thief, BattleMember target)
        {
            var enemyRewards = target.SourceData?.Rewards;
            if (enemyRewards == null) return 0;
            
            int chance = enemyRewards.BaseStealChance + thief.Stats.Luck;
            return Mathf.Clamp(chance, 1, 95);
        }
    }
    
    /// <summary>
    /// Result of a steal attempt
    /// </summary>
    public class StealResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public ItemData ItemStolen { get; set; }
        public int Quantity { get; set; }
        public int GoldStolen { get; set; }
    }
    
    /// <summary>
    /// Result of a mug attempt (attack + steal)
    /// </summary>
    public class MugResult
    {
        public BattleActionResult AttackResult { get; set; }
        public StealResult StealResult { get; set; }
    }
}