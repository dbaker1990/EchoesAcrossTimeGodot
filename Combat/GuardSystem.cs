using Godot;
using System;

namespace EchoesAcrossTime.Combat
{
    /// <summary>
    /// Manages guard/defend state for characters
    /// </summary>
    public class GuardState
    {
        public bool IsGuarding { get; set; }
        public float DamageReduction { get; set; } = 0.5f; // 50% damage reduction
        public int HPRegenPerTurn { get; set; } = 0;
        public int MPRegenPerTurn { get; set; } = 0;
        public int LimitGaugeGain { get; set; } = 5; // Small gauge gain per turn
        public bool CountersNextAttack { get; set; } = false; // Advanced guard
        
        /// <summary>
        /// Reset guard state
        /// </summary>
        public void Reset()
        {
            IsGuarding = false;
            CountersNextAttack = false;
        }
    }
    
    /// <summary>
    /// System for handling guard/defend mechanics
    /// </summary>
    public class GuardSystem
    {
        // Guard configuration
        private const float STANDARD_GUARD_REDUCTION = 0.5f;  // 50% damage reduction
        private const float ADVANCED_GUARD_REDUCTION = 0.75f; // 75% with skill
        private const int GUARD_LIMIT_GAIN = 5;              // Limit gauge per turn
        
        /// <summary>
        /// Apply guard to character
        /// </summary>
        public BattleActionResult ExecuteGuard(BattleMember member, bool isAdvancedGuard = false)
        {
            var result = new BattleActionResult
            {
                Success = true,
                Message = $"{member.Stats.CharacterName} takes a defensive stance!"
            };
            
            // Set guard state
            member.GuardState.IsGuarding = true;
            member.GuardState.DamageReduction = isAdvancedGuard 
                ? ADVANCED_GUARD_REDUCTION 
                : STANDARD_GUARD_REDUCTION;
            
            // Small HP regen while guarding
            int hpRegen = Mathf.Max(1, member.Stats.MaxHP / 20); // 5% max HP
            member.GuardState.HPRegenPerTurn = hpRegen;
            
            // Small MP regen
            int mpRegen = Mathf.Max(1, member.Stats.MaxMP / 40); // 2.5% max MP
            member.GuardState.MPRegenPerTurn = mpRegen;
            
            // Limit gauge gain
            member.GuardState.LimitGaugeGain = GUARD_LIMIT_GAIN;
            
            GD.Print($"{member.Stats.CharacterName} guards! Damage reduced by {member.GuardState.DamageReduction * 100}%");
            
            return result;
        }
        
        /// <summary>
        /// Apply damage reduction if guarding
        /// </summary>
        public int ApplyGuardReduction(BattleMember defender, int incomingDamage)
        {
            if (!defender.GuardState.IsGuarding)
                return incomingDamage;
            
            float reduction = defender.GuardState.DamageReduction;
            int reducedDamage = Mathf.RoundToInt(incomingDamage * (1f - reduction));
            
            int damageBlocked = incomingDamage - reducedDamage;
            GD.Print($"  → Guard blocked {damageBlocked} damage!");
            
            return reducedDamage;
        }
        
        /// <summary>
        /// Process guard effects at turn start
        /// </summary>
        public void ProcessGuardEffects(BattleMember member)
        {
            if (!member.GuardState.IsGuarding)
                return;
            
            // HP regeneration
            if (member.GuardState.HPRegenPerTurn > 0)
            {
                int healed = member.Stats.Heal(member.GuardState.HPRegenPerTurn);
                if (healed > 0)
                {
                    GD.Print($"  → {member.Stats.CharacterName} regenerates {healed} HP while guarding");
                }
            }
            
            // MP regeneration
            if (member.GuardState.MPRegenPerTurn > 0)
            {
                int restored = member.Stats.RestoreMP(member.GuardState.MPRegenPerTurn);
                if (restored > 0)
                {
                    GD.Print($"  → {member.Stats.CharacterName} restores {restored} MP while guarding");
                }
            }
            
            // Limit gauge gain
            if (member.GuardState.LimitGaugeGain > 0)
            {
                member.LimitGauge = Mathf.Min(100, member.LimitGauge + member.GuardState.LimitGaugeGain);
            }
        }
        
        /// <summary>
        /// Clear guard state at end of turn
        /// </summary>
        public void ClearGuard(BattleMember member)
        {
            member.GuardState.Reset();
        }
        
        /// <summary>
        /// Check if perfect guard (can trigger counter)
        /// </summary>
        public bool RollPerfectGuard(BattleMember member, RandomNumberGenerator rng = null)
        {
            if (!member.GuardState.IsGuarding)
                return false;
            
            rng ??= new RandomNumberGenerator();
            
            // 20% chance for perfect guard
            int perfectGuardRate = 20 + (member.Stats.Speed / 10); // Higher speed = better timing
            
            return rng.Randf() * 100 < perfectGuardRate;
        }
    }
}