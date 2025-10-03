using Godot;
using System;
using System.Collections.Generic;
using EchoesAcrossTime.Items;

namespace EchoesAcrossTime.Combat
{
    /// <summary>
    /// Handles using items during battle
    /// </summary>
    public class BattleItemSystem
    {
        private StatusEffectManager statusManager;
        
        public BattleItemSystem(StatusEffectManager statusEffectManager)
        {
            statusManager = statusEffectManager;
        }
        
        /// <summary>
        /// Use an item in battle
        /// </summary>
        public BattleActionResult UseItem(
            BattleMember user, 
            ConsumableData item, 
            BattleMember[] targets)
        {
            var result = new BattleActionResult
            {
                Success = false,
                Message = $"{user.Stats.CharacterName} used {item.DisplayName}!"
            };
            
            if (item == null)
            {
                result.Message = "No item to use!";
                return result;
            }
            
            GD.Print($"\n{user.Stats.CharacterName} uses {item.DisplayName}!");
            
            // Apply effects to each target
            foreach (var target in targets)
            {
                ApplyItemEffects(item, target, result);
            }
            
            result.Success = true;
            return result;
        }
        
        /// <summary>
        /// Apply item effects to a target
        /// </summary>
        private void ApplyItemEffects(ConsumableData item, BattleMember target, BattleActionResult result)
        {
            var stats = target.Stats;
            
            // HP Healing
            if (item.RestoresHP > 0)
            {
                int healing = item.RestoresHP;
                int actualHealing = stats.Heal(healing);
                result.HealingDone += actualHealing;
                GD.Print($"  → {stats.CharacterName} healed for {actualHealing} HP!");
            }
            
            if (item.RestoresHPPercent > 0)
            {
                int healing = Mathf.RoundToInt(stats.MaxHP * item.RestoresHPPercent);
                int actualHealing = stats.Heal(healing);
                result.HealingDone += actualHealing;
                GD.Print($"  → {stats.CharacterName} healed for {actualHealing} HP!");
            }
            
            // MP Restoration
            if (item.RestoresMP > 0)
            {
                int mpRestore = stats.RestoreMP(item.RestoresMP);
                GD.Print($"  → {stats.CharacterName} restored {mpRestore} MP!");
            }
            
            if (item.RestoresMPPercent > 0)
            {
                int mpRestore = Mathf.RoundToInt(stats.MaxMP * item.RestoresMPPercent);
                int actualRestore = stats.RestoreMP(mpRestore);
                GD.Print($"  → {stats.CharacterName} restored {actualRestore} MP!");
            }
            
            // Revive (only works on dead targets)
            if (item.Revives && !stats.IsAlive)
            {
                int reviveHP = Mathf.RoundToInt(stats.MaxHP * item.ReviveHPPercent);
                stats.CurrentHP = reviveHP;
                result.HealingDone += reviveHP;
                GD.Print($"  → {stats.CharacterName} was revived with {reviveHP} HP!");
            }
            
            // Status Cure
            if (item.CuresStatus.Count > 0)
            {
                foreach (var status in item.CuresStatus)
                {
                    if (statusManager.HasStatus(stats, status))
                    {
                        statusManager.RemoveStatus(stats, status);
                        GD.Print($"  → Cured {status} from {stats.CharacterName}!");
                    }
                }
            }
            
            if (item.CuresAllDebuffs)
            {
                int removed = statusManager.RemoveAllDebuffs(stats);
                if (removed > 0)
                {
                    GD.Print($"  → Removed {removed} debuffs from {stats.CharacterName}!");
                }
            }
            
            // Temporary Buffs
            if (item.TemporaryAttackBoost > 0)
            {
                GD.Print($"  → {stats.CharacterName}'s Attack increased by {item.TemporaryAttackBoost}!");
                // TODO: Apply temporary stat buff
            }
            
            if (item.TemporaryDefenseBoost > 0)
            {
                GD.Print($"  → {stats.CharacterName}'s Defense increased by {item.TemporaryDefenseBoost}!");
            }
            
            // Apply status effects (for offensive items like bombs)
            if (item.InflictsStatus.Count > 0)
            {
                foreach (var status in item.InflictsStatus)
                {
                    statusManager.ApplyStatus(stats, status, item.StatusDuration);
                    result.StatusesApplied.Add(status);
                    GD.Print($"  → {stats.CharacterName} is now {status}!");
                }
            }
            
            // Damage (for offensive items)
            if (item.DamageAmount > 0)
            {
                int damage = stats.TakeDamage(item.DamageAmount, item.DamageElement);
                result.DamageDealt += damage;
                GD.Print($"  → Dealt {damage} damage to {stats.CharacterName}!");
            }
        }
        
        /// <summary>
        /// Check if item can be used on target
        /// </summary>
        public bool CanUseItemOn(ConsumableData item, BattleMember target)
        {
            if (item == null || target == null)
                return false;
            
            // Revival items only work on dead
            if (item.Revives)
            {
                return !target.Stats.IsAlive;
            }
            
            // Offensive items can target anyone alive
            if (item.DamageAmount > 0)
            {
                return target.Stats.IsAlive;
            }
            
            // Healing/support items only on living allies
            if (item.RestoresHP > 0 || item.RestoresMP > 0 || item.CuresStatus.Count > 0)
            {
                return target.Stats.IsAlive;
            }
            
            return true;
        }
        
        /// <summary>
        /// Get valid targets for an item
        /// </summary>
        public List<BattleMember> GetValidItemTargets(
            ConsumableData item, 
            List<BattleMember> allies, 
            List<BattleMember> enemies)
        {
            var validTargets = new List<BattleMember>();
            
            if (item == null)
                return validTargets;
            
            // Offensive items target enemies
            if (item.DamageAmount > 0)
            {
                validTargets.AddRange(enemies.FindAll(e => e.Stats.IsAlive));
            }
            // Revival items target dead allies
            else if (item.Revives)
            {
                validTargets.AddRange(allies.FindAll(a => !a.Stats.IsAlive));
            }
            // Support items target living allies
            else
            {
                validTargets.AddRange(allies.FindAll(a => a.Stats.IsAlive));
            }
            
            return validTargets;
        }
    }
}