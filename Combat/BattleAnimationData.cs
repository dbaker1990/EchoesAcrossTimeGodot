// Combat/BattleAnimationData.cs
using Godot;
using System;

namespace EchoesAcrossTime.Combat
{
    /// <summary>
    /// Animation references for battle
    /// </summary>
    [GlobalClass]
    public partial class BattleAnimationData : Resource
    {
        [ExportGroup("Battle Stance")]
        [Export] public string IdleAnimation { get; set; } = "battle_idle";
        [Export] public string ReadyAnimation { get; set; } = "battle_ready";
        [Export] public string VictoryAnimation { get; set; } = "victory";
        [Export] public string DefeatAnimation { get; set; } = "defeat";
        
        [ExportGroup("Action Animations")]
        [Export] public string AttackAnimation { get; set; } = "attack";
        [Export] public string CastAnimation { get; set; } = "cast";
        [Export] public string ItemAnimation { get; set; } = "item";
        [Export] public string DefendAnimation { get; set; } = "defend";
        [Export] public string EvadeAnimation { get; set; } = "evade";
        
        [ExportGroup("Reaction Animations")]
        [Export] public string HitAnimation { get; set; } = "hit";
        [Export] public string CriticalHitAnimation { get; set; } = "critical_hit";
        [Export] public string MissAnimation { get; set; } = "miss";
        [Export] public string HealAnimation { get; set; } = "heal";
        [Export] public string BuffAnimation { get; set; } = "buff";
        [Export] public string DebuffAnimation { get; set; } = "debuff";
        
        [ExportGroup("Status Animations")]
        [Export] public string PoisonAnimation { get; set; } = "status_poison";
        [Export] public string SleepAnimation { get; set; } = "status_sleep";
        [Export] public string StunAnimation { get; set; } = "status_stun";
        [Export] public string ParalysisAnimation { get; set; } = "status_paralysis";
        [Export] public string DeathAnimation { get; set; } = "death";
        [Export] public string ReviveAnimation { get; set; } = "revive";
        
        [ExportGroup("Special Animations")]
        [Export] public string TransformAnimation { get; set; } = "transform";
        [Export] public string EscapeAnimation { get; set; } = "escape";
        [Export] public string CounterAnimation { get; set; } = "counter";
        [Export] public string ReflectAnimation { get; set; } = "reflect";
        
        [ExportGroup("Timing")]
        [Export] public float AttackDuration { get; set; } = 0.5f;
        [Export] public float CastDuration { get; set; } = 1.0f;
        [Export] public float HitDelay { get; set; } = 0.3f;
        [Export] public float ReturnDelay { get; set; } = 0.2f;
        
        [ExportGroup("Movement")]
        [Export] public float ApproachDistance { get; set; } = 50f;
        [Export] public float ApproachSpeed { get; set; } = 300f;
        [Export] public float ReturnSpeed { get; set; } = 200f;
        [Export] public bool UsesStepForward { get; set; } = true;
        
        [ExportGroup("Effects")]
        [Export] public string DefaultHitEffectPath { get; set; } = "";
        [Export] public string CriticalHitEffectPath { get; set; } = "";
        [Export] public string MissEffectPath { get; set; } = "";
        [Export] public AudioStream DefaultHitSound { get; set; }
        [Export] public AudioStream CriticalHitSound { get; set; }
        [Export] public AudioStream MissSound { get; set; }

        /// <summary>
        /// Get animation name for a skill
        /// </summary>
        public string GetSkillAnimation(SkillData skill)
        {
            if (skill == null) return AttackAnimation;

            // Use skill's custom animation if set
            if (!string.IsNullOrEmpty(skill.AnimationName))
                return skill.AnimationName;

            // Default based on skill type
            return skill.Type switch
            {
                SkillType.ActiveAttack => AttackAnimation,
                SkillType.ActiveSupport => CastAnimation,
                SkillType.CosmicAbility => CastAnimation,
                SkillType.PoliticalSkill => CastAnimation,
                _ => AttackAnimation
            };
        }

        /// <summary>
        /// Get animation for status effect
        /// </summary>
        public string GetStatusAnimation(StatusEffect status)
        {
            return status switch
            {
                StatusEffect.Poison => PoisonAnimation,
                StatusEffect.Sleep => SleepAnimation,
                StatusEffect.Stun => StunAnimation,
                StatusEffect.Paralysis => ParalysisAnimation,
                StatusEffect.Burn => "status_burn",
                StatusEffect.Freeze => "status_freeze",
                _ => ""
            };
        }
    }
}