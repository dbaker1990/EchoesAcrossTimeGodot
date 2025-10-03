using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EchoesAcrossTime.Combat
{
    /// <summary>
    /// Manages limit break gauge filling and execution
    /// </summary>
    public class LimitBreakSystem
    {
        private const int MAX_LIMIT_GAUGE = 100;
        private const float DAMAGE_TAKEN_MULTIPLIER = 0.8f; // % of damage taken -> gauge
        private const float DAMAGE_DEALT_MULTIPLIER = 0.2f; // % of damage dealt -> gauge
        private const int ALLY_DEATH_BONUS = 30; // Gauge gained when ally dies
        private const int CRITICAL_BONUS = 10; // Bonus for landing crit
        
        private Dictionary<string, LimitBreakData> registeredLimitBreaks;
        private RandomNumberGenerator rng;
        
        public LimitBreakSystem()
        {
            registeredLimitBreaks = new Dictionary<string, LimitBreakData>();
            rng = new RandomNumberGenerator();
        }
        
        /// <summary>
        /// Register a limit break for use in battle
        /// </summary>
        public void RegisterLimitBreak(LimitBreakData limitBreak)
        {
            if (limitBreak == null) return;
            
            string key = limitBreak.CharacterId.ToLower();
            registeredLimitBreaks[key] = limitBreak;
            
            GD.Print($"Registered Limit Break: {limitBreak.DisplayName} for {limitBreak.CharacterId}");
        }
        
        /// <summary>
        /// Get limit break for character
        /// </summary>
        public LimitBreakData GetLimitBreak(string characterId)
        {
            string key = characterId.ToLower();
            return registeredLimitBreaks.TryGetValue(key, out var lb) ? lb : null;
        }
        
        /// <summary>
        /// Add gauge from taking damage
        /// </summary>
        public void AddGaugeFromDamage(BattleMember member, int damageTaken)
        {
            if (member == null || !member.Stats.IsAlive) return;
            
            float gaugeGain = damageTaken * DAMAGE_TAKEN_MULTIPLIER;
            AddGauge(member, Mathf.RoundToInt(gaugeGain));
        }
        
        /// <summary>
        /// Add gauge from dealing damage
        /// </summary>
        public void AddGaugeFromDealingDamage(BattleMember member, int damageDealt, bool wasCritical)
        {
            if (member == null || !member.Stats.IsAlive) return;
            
            float gaugeGain = damageDealt * DAMAGE_DEALT_MULTIPLIER;
            
            // Bonus for critical hits
            if (wasCritical)
            {
                gaugeGain += CRITICAL_BONUS;
            }
            
            AddGauge(member, Mathf.RoundToInt(gaugeGain));
        }
        
        /// <summary>
        /// Add gauge when ally dies
        /// </summary>
        public void AddGaugeFromAllyDeath(BattleMember member)
        {
            if (member == null || !member.Stats.IsAlive) return;
            
            AddGauge(member, ALLY_DEATH_BONUS);
            GD.Print($"{member.Stats.CharacterName} gains {ALLY_DEATH_BONUS} Limit gauge from ally death!");
        }
        
        /// <summary>
        /// Add gauge directly
        /// </summary>
        private void AddGauge(BattleMember member, int amount)
        {
            if (amount <= 0) return;
            
            int oldGauge = member.LimitGauge;
            member.LimitGauge = Mathf.Min(MAX_LIMIT_GAUGE, member.LimitGauge + amount);
            
            if (member.LimitGauge >= MAX_LIMIT_GAUGE && oldGauge < MAX_LIMIT_GAUGE)
            {
                member.IsLimitBreakReady = true;
                GD.Print($"\n★★★ {member.Stats.CharacterName}'s LIMIT BREAK is READY! ★★★");
            }
        }
        
        /// <summary>
        /// Execute a limit break
        /// </summary>
        public BattleActionResult ExecuteLimitBreak(
            LimitBreakData limitBreak,
            BattleMember user,
            List<BattleMember> targets,
            BattleMember duoPartner = null)
        {
            var result = new BattleActionResult();
            
            if (!limitBreak.CanUse(user))
            {
                result.Success = false;
                result.Message = "Cannot use Limit Break!";
                return result;
            }
            
            // Consume gauge
            user.LimitGauge = 0;
            user.IsLimitBreakReady = false;
            
            // If duo, consume partner gauge too
            bool isDuo = duoPartner != null && limitBreak.IsDuoLimitBreak;
            if (isDuo)
            {
                duoPartner.LimitGauge = 0;
                duoPartner.IsLimitBreakReady = false;
            }
            
            // Display epic header
            GD.Print("\n╔════════════════════════════════════════════════╗");
            if (isDuo)
            {
                GD.Print($"║   ⚡⚡⚡ DUO LIMIT BREAK! ⚡⚡⚡          ║");
                GD.Print($"║   {user.Stats.CharacterName} & {duoPartner.Stats.CharacterName}    ║");
            }
            else
            {
                GD.Print($"║   ⚡⚡⚡ LIMIT BREAK! ⚡⚡⚡              ║");
            }
            GD.Print($"║   {limitBreak.DisplayName.ToUpper()}                    ║");
            GD.Print("╚════════════════════════════════════════════════╝");
            
            if (!string.IsNullOrEmpty(limitBreak.FlavorText))
            {
                GD.Print($"   \"{limitBreak.FlavorText}\"");
            }
            
            result.Success = true;
            
            // Apply effects based on type
            switch (limitBreak.Type)
            {
                case LimitBreakType.Offensive:
                case LimitBreakType.Hybrid:
                    ExecuteOffensiveEffects(limitBreak, user, targets, isDuo, result);
                    break;
            }
            
            // Support effects
            if (limitBreak.HealAmount > 0 || limitBreak.HealPercent > 0)
            {
                ExecuteHealingEffects(limitBreak, user, result);
            }
            
            // Buff effects
            if (limitBreak.AttackBuff > 0 || limitBreak.DefenseBuff > 0 || limitBreak.SpeedBuff > 0)
            {
                ExecuteBuffEffects(limitBreak, user, result);
            }
            
            // Special mechanics
            if (limitBreak.GrantsExtraTurn)
            {
                user.HasExtraTurn = true;
                GD.Print($"  → {user.Stats.CharacterName} gains an extra turn!");
            }
            
            if (limitBreak.StopsTime)
            {
                result.Message += $" Time stopped for {limitBreak.TimeStopDuration} turn(s)!";
                GD.Print($"  → Time Stopped for {limitBreak.TimeStopDuration} turn(s)!");
            }
            
            GD.Print($"\nLimit Break complete! Total damage: {result.DamageDealt}");
            
            return result;
        }
        
        /// <summary>
        /// Execute offensive limit break effects
        /// </summary>
        private void ExecuteOffensiveEffects(
            LimitBreakData limitBreak, 
            BattleMember user, 
            List<BattleMember> targets,
            bool isDuo,
            BattleActionResult result)
        {
            foreach (var target in targets)
            {
                if (!target.Stats.IsAlive) continue;
                
                // Multi-hit attacks
                for (int hit = 0; hit < limitBreak.HitCount; hit++)
                {
                    // Calculate base damage
                    int baseDamage = limitBreak.BasePower;
                    
                    // Add user's attack stat
                    baseDamage += user.Stats.Attack * 2;
                    
                    // Apply power multiplier
                    float multiplier = limitBreak.PowerMultiplier;
                    
                    // Duo bonus
                    if (isDuo)
                    {
                        multiplier *= limitBreak.DuoPowerBonus;
                    }
                    
                    baseDamage = Mathf.RoundToInt(baseDamage * multiplier);
                    
                    // Check for critical
                    bool isCrit = rng.Randf() * 100 < (user.Stats.BattleStats.CriticalRate + limitBreak.CriticalBonus);
                    if (isCrit)
                    {
                        baseDamage = user.Stats.BattleStats.ApplyCriticalDamage(baseDamage);
                        GD.Print("  *** CRITICAL HIT! ***");
                    }
                    
                    // Apply defense unless ignored
                    if (!limitBreak.IgnoresDefense)
                    {
                        baseDamage -= target.Stats.Defense / 2;
                    }
                    
                    baseDamage = Mathf.Max(1, baseDamage);
                    
                    // Check for instant kill
                    if (limitBreak.InstantKillBelow)
                    {
                        if (target.Stats.HPPercent <= limitBreak.InstantKillThreshold)
                        {
                            baseDamage = target.Stats.MaxHP; // Instant kill!
                            GD.Print($"  ☠️ INSTANT KILL! {target.Stats.CharacterName} obliterated!");
                        }
                    }
                    
                    // Apply elemental multiplier (unless piercing)
                    if (!limitBreak.PiercesImmunity)
                    {
                        float elementMult = target.Stats.ElementAffinities.GetDamageMultiplier(limitBreak.Element);
                        baseDamage = Mathf.RoundToInt(baseDamage * elementMult);
                    }
                    
                    // Deal damage
                    int actualDamage = target.Stats.TakeDamage(baseDamage, limitBreak.Element);
                    result.DamageDealt += actualDamage;
                    
                    string hitText = limitBreak.HitCount > 1 ? $" (Hit {hit + 1}/{limitBreak.HitCount})" : "";
                    GD.Print($"  → {target.Stats.CharacterName}: {actualDamage} damage!{hitText}");
                }
                
                // Apply status effects
                if (limitBreak.InflictsStatuses.Count > 0 && target.Stats.IsAlive)
                {
                    foreach (var status in limitBreak.InflictsStatuses)
                    {
                        if (rng.Randf() * 100 < limitBreak.StatusChance)
                        {
                            // Apply status (assuming you have a status manager)
                            result.StatusesApplied.Add(status);
                            GD.Print($"  → {target.Stats.CharacterName} is now {status}!");
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Execute healing effects
        /// </summary>
        private void ExecuteHealingEffects(LimitBreakData limitBreak, BattleMember user, BattleActionResult result)
        {
            int healing = limitBreak.HealAmount;
            
            if (limitBreak.HealPercent > 0)
            {
                healing += Mathf.RoundToInt(user.Stats.MaxHP * limitBreak.HealPercent);
            }
            
            if (healing > 0)
            {
                int actualHeal = user.Stats.Heal(healing);
                result.HealingDone += actualHeal;
                GD.Print($"  → {user.Stats.CharacterName} healed for {actualHeal} HP!");
            }
        }
        
        /// <summary>
        /// Execute buff effects
        /// </summary>
        private void ExecuteBuffEffects(LimitBreakData limitBreak, BattleMember user, BattleActionResult result)
        {
            if (limitBreak.AttackBuff > 0)
            {
                GD.Print($"  → Attack increased by {limitBreak.AttackBuff}%!");
            }
            if (limitBreak.DefenseBuff > 0)
            {
                GD.Print($"  → Defense increased by {limitBreak.DefenseBuff}%!");
            }
            if (limitBreak.SpeedBuff > 0)
            {
                GD.Print($"  → Speed increased by {limitBreak.SpeedBuff}%!");
            }
        }
        
        /// <summary>
        /// Reset all gauges (for new battle)
        /// </summary>
        public void ResetAllGauges(List<BattleMember> party)
        {
            foreach (var member in party)
            {
                member.LimitGauge = 0;
                member.IsLimitBreakReady = false;
            }
        }
        
        /// <summary>
        /// Get gauge percent (0-1)
        /// </summary>
        public float GetGaugePercent(BattleMember member)
        {
            return (float)member.LimitGauge / MAX_LIMIT_GAUGE;
        }
    }
}