using Godot;
using System;

namespace EchoesAcrossTime.Combat
{
    /// <summary>
    /// Baton Pass data - tracks passes and stat bonuses
    /// In Persona 5, passing your One More turn to an ally boosts their stats
    /// </summary>
    public class BatonPassData
    {
        public int PassCount { get; set; }
        public BattleMember LastPasser { get; set; }
        public bool IsActive { get; set; }
        
        // Stat multipliers that increase with each pass
        public float DamageMultiplier { get; private set; }
        public float HealingMultiplier { get; private set; }
        public float CriticalBonus { get; private set; }
        
        // Base bonuses per pass level
        private const float BASE_DAMAGE_BONUS = 0.5f;  // +50% per pass
        private const float BASE_HEALING_BONUS = 0.5f; // +50% per pass
        private const float BASE_CRIT_BONUS = 10f;     // +10% crit chance per pass
        
        public BatonPassData()
        {
            Reset();
        }
        
        /// <summary>
        /// Reset baton pass state
        /// </summary>
        public void Reset()
        {
            PassCount = 0;
            LastPasser = null;
            IsActive = false;
            DamageMultiplier = 1.0f;
            HealingMultiplier = 1.0f;
            CriticalBonus = 0f;
        }
        
        /// <summary>
        /// Receive a baton pass and increase bonuses
        /// </summary>
        public void ReceivePass(BattleMember passer)
        {
            PassCount++;
            LastPasser = passer;
            IsActive = true;
            
            // Stack bonuses (gets stronger with each pass)
            // Pass 1: +50% damage, Pass 2: +100% damage, Pass 3: +150% damage
            DamageMultiplier = 1.0f + (BASE_DAMAGE_BONUS * PassCount);
            HealingMultiplier = 1.0f + (BASE_HEALING_BONUS * PassCount);
            CriticalBonus = BASE_CRIT_BONUS * PassCount;
            
            GD.Print($"Sync Pass Level {PassCount}!");
            GD.Print($"  Damage: x{DamageMultiplier:F1}");
            GD.Print($"  Healing: x{HealingMultiplier:F1}");
            GD.Print($"  Critical: +{CriticalBonus}%");
        }
        
        /// <summary>
        /// Get display text for current baton pass level
        /// </summary>
        public string GetBonusText()
        {
            if (!IsActive) return "";
            
            return $"Sync Pass x{PassCount} (DMG +{(DamageMultiplier - 1f) * 100:F0}%, CRIT +{CriticalBonus}%)";
        }
    }
    
    /// <summary>
    /// Manager for handling baton pass logic
    /// </summary>
    public class BatonPassManager
    {
        private BattleMember currentPassHolder;
        
        /// <summary>
        /// Check if baton pass is available
        /// Actor must have One More turn to pass
        /// </summary>
        public bool CanBatonPass(BattleMember actor)
        {
            if (actor == null || !actor.HasExtraTurn) return false;
            if (!actor.Stats.IsAlive) return false;
            
            // Can't pass to self
            return true;
        }
        
        /// <summary>
        /// Check if a target can receive baton pass
        /// </summary>
        public bool CanReceiveBatonPass(BattleMember actor, BattleMember target)
        {
            if (target == null || !target.Stats.IsAlive) return false;
            if (target == actor) return false; // Can't pass to self
            if (target.HasActedThisTurn) return false; // Must not have acted yet
            
            return true;
        }
        
        /// <summary>
        /// Execute baton pass from actor to target
        /// </summary>
        public bool ExecuteBatonPass(BattleMember actor, BattleMember target)
        {
            if (!CanBatonPass(actor) || !CanReceiveBatonPass(actor, target))
            {
                return false;
            }
            
            // Transfer the One More turn
            actor.HasExtraTurn = false;
            actor.EndTurn();
            
            // Target receives pass with bonuses
            target.BatonPassData.ReceivePass(actor);
            target.HasExtraTurn = true;
            
            currentPassHolder = target;
            
            GD.Print($"\n★ SYNC PASS! ★");
            GD.Print($"{actor.Stats.CharacterName} → {target.Stats.CharacterName}");
            
            return true;
        }
        
        /// <summary>
        /// Reset baton pass state for new round
        /// </summary>
        public void ResetForNewRound()
        {
            currentPassHolder = null;
        }
        
        /// <summary>
        /// Apply baton pass damage bonus
        /// </summary>
        public int ApplyDamageBonus(BattleMember actor, int baseDamage)
        {
            if (actor?.BatonPassData == null || !actor.BatonPassData.IsActive)
                return baseDamage;
            
            float multiplier = actor.BatonPassData.DamageMultiplier;
            return Mathf.RoundToInt(baseDamage * multiplier);
        }
        
        /// <summary>
        /// Apply baton pass healing bonus
        /// </summary>
        public int ApplyHealingBonus(BattleMember actor, int baseHealing)
        {
            if (actor?.BatonPassData == null || !actor.BatonPassData.IsActive)
                return baseHealing;
            
            float multiplier = actor.BatonPassData.HealingMultiplier;
            return Mathf.RoundToInt(baseHealing * multiplier);
        }
        
        /// <summary>
        /// Get additional critical chance from baton pass
        /// </summary>
        public int GetCriticalBonus(BattleMember actor)
        {
            if (actor?.BatonPassData == null || !actor.BatonPassData.IsActive)
                return 0;
            
            return (int)actor.BatonPassData.CriticalBonus;
        }
    }
}