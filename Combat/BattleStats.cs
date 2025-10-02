using Godot;
using System;

namespace EchoesAcrossTime.Combat
{
    /// <summary>
    /// Extended battle statistics for combat
    /// </summary>
    [GlobalClass]
    public partial class BattleStats : Resource
    {
        // ... all the [Export] properties remain the same ...
        
        [ExportGroup("Hit Rates")]
        [Export(PropertyHint.Range, "0,100")] public int AccuracyRate { get; set; } = 95;
        [Export(PropertyHint.Range, "0,100")] public int EvasionRate { get; set; } = 5;
        [Export(PropertyHint.Range, "0,100")] public int MagicEvasionRate { get; set; } = 0;
        
        [ExportGroup("Critical Hits")]
        [Export(PropertyHint.Range, "0,100")] public int CriticalRate { get; set; } = 5;
        [Export(PropertyHint.Range, "100,300")] public int CriticalDamageMultiplier { get; set; } = 200;
        
        [ExportGroup("Special Mechanics")]
        [Export(PropertyHint.Range, "0,100")] public int CounterAttackRate { get; set; } = 0;
        [Export(PropertyHint.Range, "0,100")] public int MagicReflectionRate { get; set; } = 0;
        [Export(PropertyHint.Range, "0,100")] public int DoubleAttackRate { get; set; } = 0;
        [Export(PropertyHint.Range, "0,100")] public int PreemptiveStrikeRate { get; set; } = 5;
        
        [ExportGroup("Defense Mechanics")]
        [Export(PropertyHint.Range, "0,100")] public int GuardRate { get; set; } = 0;
        [Export(PropertyHint.Range, "0,100")] public int ParryRate { get; set; } = 0;
        [Export(PropertyHint.Range, "0,100")] public int MagicBarrierRate { get; set; } = 0;
        
        [ExportGroup("Status Resistance")]
        [Export(PropertyHint.Range, "0,100")] public int PoisonResistance { get; set; } = 0;
        [Export(PropertyHint.Range, "0,100")] public int SleepResistance { get; set; } = 0;
        [Export(PropertyHint.Range, "0,100")] public int ParalysisResistance { get; set; } = 0;
        [Export(PropertyHint.Range, "0,100")] public int ConfusionResistance { get; set; } = 0;
        [Export(PropertyHint.Range, "0,100")] public int SilenceResistance { get; set; } = 0;
        [Export(PropertyHint.Range, "0,100")] public int BlindResistance { get; set; } = 0;
        [Export(PropertyHint.Range, "0,100")] public int StunResistance { get; set; } = 0;
        [Export(PropertyHint.Range, "0,100")] public int DeathResistance { get; set; } = 0;
        
        [ExportGroup("Aggro/Threat")]
        [Export] public float ThreatMultiplier { get; set; } = 1.0f;
        [Export] public int BaseThreat { get; set; } = 100;
        
        [ExportGroup("Special Flags")]
        [Export] public bool ImmuneToInstantDeath { get; set; } = false;
        [Export] public bool ImmuneToAllStatusEffects { get; set; } = false;
        [Export] public bool CannotBeTargeted { get; set; } = false;
        [Export] public bool AlwaysStrikesFirst { get; set; } = false;
        
        /// <summary>
        /// Calculate if an attack hits
        /// </summary>
        public bool RollHit(int attackerAccuracy, RandomNumberGenerator rng = null)
        {
            rng ??= new RandomNumberGenerator();
            
            float hitChance = attackerAccuracy - EvasionRate;
            hitChance = Mathf.Clamp(hitChance, 5, 95);
            
            return rng.Randf() * 100 < hitChance;
        }
        
        /// <summary>
        /// Calculate if attack is a critical hit
        /// </summary>
        public bool RollCritical(int bonusCritRate = 0, RandomNumberGenerator rng = null)
        {
            rng ??= new RandomNumberGenerator();
            
            int totalCritRate = CriticalRate + bonusCritRate;
            totalCritRate = Mathf.Clamp(totalCritRate, 0, 100);
            
            return rng.Randf() * 100 < totalCritRate;
        }
        
        /// <summary>
        /// Calculate critical damage
        /// </summary>
        public int ApplyCriticalDamage(int baseDamage)
        {
            return Mathf.RoundToInt(baseDamage * (CriticalDamageMultiplier / 100f));
        }
        
        /// <summary>
        /// Check status effect resistance
        /// </summary>
        public bool RollStatusResistance(StatusEffect status, RandomNumberGenerator rng = null)
        {
            rng ??= new RandomNumberGenerator();
            
            if (ImmuneToAllStatusEffects) return true;
            
            int resistance = status switch
            {
                StatusEffect.Poison => PoisonResistance,
                StatusEffect.Sleep => SleepResistance,
                StatusEffect.Paralysis => ParalysisResistance,
                StatusEffect.Confusion => ConfusionResistance,
                StatusEffect.Silence => SilenceResistance,
                StatusEffect.Blind => BlindResistance,
                StatusEffect.Stun => StunResistance,
                StatusEffect.Doom => DeathResistance,
                _ => 0
            };
            
            return rng.Randf() * 100 < resistance;
        }
        
        /// <summary>
        /// Clone this battle stats
        /// </summary>
        public BattleStats Clone()
        {
            return new BattleStats
            {
                AccuracyRate = this.AccuracyRate,
                EvasionRate = this.EvasionRate,
                MagicEvasionRate = this.MagicEvasionRate,
                CriticalRate = this.CriticalRate,
                CriticalDamageMultiplier = this.CriticalDamageMultiplier,
                CounterAttackRate = this.CounterAttackRate,
                MagicReflectionRate = this.MagicReflectionRate,
                DoubleAttackRate = this.DoubleAttackRate,
                PreemptiveStrikeRate = this.PreemptiveStrikeRate,
                GuardRate = this.GuardRate,
                ParryRate = this.ParryRate,
                MagicBarrierRate = this.MagicBarrierRate,
                PoisonResistance = this.PoisonResistance,
                SleepResistance = this.SleepResistance,
                ParalysisResistance = this.ParalysisResistance,
                ConfusionResistance = this.ConfusionResistance,
                SilenceResistance = this.SilenceResistance,
                BlindResistance = this.BlindResistance,
                StunResistance = this.StunResistance,
                DeathResistance = this.DeathResistance,
                ThreatMultiplier = this.ThreatMultiplier,
                BaseThreat = this.BaseThreat,
                ImmuneToInstantDeath = this.ImmuneToInstantDeath,
                ImmuneToAllStatusEffects = this.ImmuneToAllStatusEffects,
                CannotBeTargeted = this.CannotBeTargeted,
                AlwaysStrikesFirst = this.AlwaysStrikesFirst
            };
        }
    }
}