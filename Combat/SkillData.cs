// Combat/SkillData.cs
using Godot;
using System;
using System.Collections.Generic;
using EchoesAcrossTime.Database;
namespace EchoesAcrossTime.Combat
{
    public enum SkillType
    {
        ActiveAttack,       // Offensive skills
        ActiveSupport,      // Buffs, heals
        Passive,            // Always active traits
        CosmicAbility,      // Special cosmic powers
        PoliticalSkill      // Court Mage diplomatic skills
    }

    public enum SkillTarget
    {
        SingleEnemy,
        AllEnemies,
        RandomEnemy,
        SingleAlly,
        AllAllies,
        Self,
        Everyone,
        DeadAlly            // For revival skills
    }

    public enum DamageType
    {
        Physical,
        Magical,
        Fixed,              // Ignores defense
        Percentage          // % of target's HP
    }

    /// <summary>
    /// Complete skill data similar to RPG Maker
    /// </summary>
    [GlobalClass]
    public partial class SkillData : Resource
    {
        [ExportGroup("Basic Info")]
        [Export] public string SkillId { get; set; } = "skill_001";
        [Export] public string DisplayName { get; set; } = "Skill";
        [Export(PropertyHint.MultilineText)] public string Description { get; set; } = "";
        [Export] public Texture2D Icon { get; set; }
        [Export] public SkillType Type { get; set; } = SkillType.ActiveAttack;
        
        [ExportGroup("Targeting")]
        [Export] public SkillTarget Target { get; set; } = SkillTarget.SingleEnemy;
        [Export] public int NumberOfHits { get; set; } = 1;
        
        [ExportGroup("Costs")]
        [Export] public int MPCost { get; set; } = 0;
        [Export] public int HPCost { get; set; } = 0;
        [Export] public int TPCost { get; set; } = 0;  // Tension points, if used
        [Export] public int GoldCost { get; set; } = 0;
        
        [ExportGroup("Damage")]
        [Export] public DamageType DamageType { get; set; } = DamageType.Physical;
        [Export] public ElementType Element { get; set; } = ElementType.Physical;
        [Export] public int BasePower { get; set; } = 100;
        [Export] public float PowerMultiplier { get; set; } = 1.0f;
        [Export] public int Accuracy { get; set; } = 100;
        [Export] public int CriticalBonus { get; set; } = 0;
        [Export] public bool IgnoreDefense { get; set; } = false;
        
        [ExportGroup("Healing")]
        [Export] public int HealAmount { get; set; } = 0;
        [Export] public float HealPercent { get; set; } = 0f;  // 0-1
        [Export] public bool HealsMP { get; set; } = false;
        [Export] public bool RevivesTarget { get; set; } = false;
        
        [ExportGroup("Status Effects")]
        [Export] public Godot.Collections.Array<StatusEffect> InflictsStatuses { get; set; }
        [Export] public Godot.Collections.Array<int> StatusChances { get; set; }  // Parallel array
        [Export] public Godot.Collections.Array<StatusEffect> RemovesStatuses { get; set; }
        [Export] public bool RemovesAllDebuffs { get; set; } = false;
        
        [ExportGroup("Stat Buffs/Debuffs")]
        [Export] public int AttackBuff { get; set; } = 0;
        [Export] public int DefenseBuff { get; set; } = 0;
        [Export] public int MagicAttackBuff { get; set; } = 0;
        [Export] public int MagicDefenseBuff { get; set; } = 0;
        [Export] public int SpeedBuff { get; set; } = 0;
        [Export] public int BuffDuration { get; set; } = 3;  // Turns
        [Export] public bool IsDebuff { get; set; } = false;
        
        [ExportGroup("Special Effects")]
        [Export] public bool DrainHP { get; set; } = false;  // User heals for damage dealt
        [Export] public float DrainPercent { get; set; } = 0.5f;
        [Export] public bool SelfDestruct { get; set; } = false;
        [Export] public bool EscapeBattle { get; set; } = false;
        
        [ExportGroup("Requirements")]
        [Export] public int RequiredLevel { get; set; } = 1;
        [Export] public Godot.Collections.Array<CharacterClass> AllowedClasses { get; set; }
        [Export] public int RequiredWeaponType { get; set; } = -1;  // -1 = any
        [Export] public bool RequiresState { get; set; } = false;  // Like "Berserk" state
        [Export] public StatusEffect RequiredState { get; set; }
        
        [ExportGroup("Passive Effects - Only for Passive type")]
        [Export] public float PassiveDamageMultiplier { get; set; } = 1.0f;
        [Export] public int PassiveHPRegen { get; set; } = 0;  // Per turn
        [Export] public int PassiveMPRegen { get; set; } = 0;
        [Export] public int PassiveEvasionBonus { get; set; } = 0;
        [Export] public int PassiveCriticalBonus { get; set; } = 0;
        [Export] public ElementType PassiveElementBoost { get; set; } = ElementType.None;
        [Export] public Godot.Collections.Array<StatusEffect> PassiveStatusImmunity { get; set; }
        
        [ExportGroup("Animation & Effects")]
        [Export] public string AnimationName { get; set; } = "attack";
        [Export] public string CastAnimationName { get; set; } = "cast";
        [Export] public string HitEffectPath { get; set; } = "";
        [Export] public AudioStream SoundEffect { get; set; }
        [Export] public float AnimationSpeed { get; set; } = 1.0f;
        
        [ExportGroup("Battle Messages")]
        [Export] public string UseMessage { get; set; } = "{USER} uses {SKILL}!";
        [Export] public string SuccessMessage { get; set; } = "";
        [Export] public string FailureMessage { get; set; } = "But it failed!";
        
        [ExportGroup("Metadata")]
        [Export(PropertyHint.MultilineText)] public string Note { get; set; } = "";

        public SkillData()
        {
            InflictsStatuses = new Godot.Collections.Array<StatusEffect>();
            StatusChances = new Godot.Collections.Array<int>();
            RemovesStatuses = new Godot.Collections.Array<StatusEffect>();
            AllowedClasses = new Godot.Collections.Array<CharacterClass>();
            PassiveStatusImmunity = new Godot.Collections.Array<StatusEffect>();
        }

        /// <summary>
        /// Check if character can use this skill
        /// </summary>
        public bool CanUse(CharacterStats user)
        {
            if (user == null) return false;
            
            // Check MP
            if (user.CurrentMP < MPCost) return false;
            
            // Check HP
            if (user.CurrentHP <= HPCost) return false;  // Can't kill self with HP cost
            
            // Check level
            if (user.Level < RequiredLevel) return false;
            
            // Check required state
            if (RequiresState)
            {
                bool hasState = user.ActiveStatuses.Exists(s => s.Effect == RequiredState);
                if (!hasState) return false;
            }
            
            return true;
        }

        /// <summary>
        /// Consume resources when skill is used
        /// </summary>
        public void ConsumeResources(CharacterStats user)
        {
            user.ConsumeMP(MPCost);
            
            if (HPCost > 0)
            {
                user.TakeDamage(HPCost, ElementType.Physical);
            }
        }

        /// <summary>
        /// Calculate skill damage
        /// </summary>
        public int CalculateDamage(CharacterStats user, CharacterStats target)
        {
            if (user == null || target == null) return 0;

            int baseDamage = 0;

            switch (DamageType)
            {
                case Combat.DamageType.Physical:
                    baseDamage = IgnoreDefense 
                        ? user.Attack 
                        : user.Attack - target.Defense / 2;
                    break;

                case Combat.DamageType.Magical:
                    baseDamage = IgnoreDefense
                        ? user.MagicAttack
                        : user.MagicAttack - target.MagicDefense / 2;
                    break;

                case Combat.DamageType.Fixed:
                    return BasePower;

                case Combat.DamageType.Percentage:
                    return Mathf.RoundToInt(target.MaxHP * (BasePower / 100f));
            }

            // Apply power and multiplier
            float finalDamage = (baseDamage * BasePower / 100f) * PowerMultiplier;
            
            // Element multiplier
            float elementMultiplier = target.ElementAffinities?.GetDamageMultiplier(Element) ?? 1f;
            finalDamage *= elementMultiplier;

            return Mathf.Max(1, Mathf.RoundToInt(finalDamage));
        }

        /// <summary>
        /// Calculate healing amount
        /// </summary>
        public int CalculateHealing(CharacterStats user, CharacterStats target)
        {
            if (target == null) return 0;

            int healing = HealAmount;

            if (HealPercent > 0)
            {
                int maxStat = HealsMP ? target.MaxMP : target.MaxHP;
                healing += Mathf.RoundToInt(maxStat * HealPercent);
            }

            // Could scale with user's magic attack
            if (user != null && healing > 0)
            {
                healing = Mathf.RoundToInt(healing * (1f + user.MagicAttack / 200f));
            }

            return healing;
        }

        /// <summary>
        /// Check if skill hits
        /// </summary>
        public bool RollAccuracy(CharacterStats user, CharacterStats target, RandomNumberGenerator rng = null)
        {
            rng ??= new RandomNumberGenerator();

            // Perfect accuracy skills always hit
            if (Accuracy >= 100) return true;

            // Calculate hit chance
            int userAccuracy = user?.BattleStats?.AccuracyRate ?? 95;
            int targetEvasion = target?.BattleStats?.EvasionRate ?? 5;

            // For magical skills, use magic evasion
            if (DamageType == Combat.DamageType.Magical)
            {
                targetEvasion = target?.BattleStats?.MagicEvasionRate ?? 0;
            }

            float hitChance = (Accuracy * userAccuracy / 100f) - targetEvasion;
            hitChance = Mathf.Clamp(hitChance, 5, 95);

            return rng.Randf() * 100 < hitChance;
        }

        /// <summary>
        /// Roll for status effect application
        /// </summary>
        public List<StatusEffect> RollStatusEffects(RandomNumberGenerator rng = null)
        {
            rng ??= new RandomNumberGenerator();
            var appliedStatuses = new List<StatusEffect>();

            for (int i = 0; i < InflictsStatuses.Count; i++)
            {
                int chance = i < StatusChances.Count ? StatusChances[i] : 100;
                
                if (rng.Randf() * 100 < chance)
                {
                    appliedStatuses.Add(InflictsStatuses[i]);
                }
            }

            return appliedStatuses;
        }

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(SkillId) && 
                   !string.IsNullOrEmpty(DisplayName);
        }
    }
}