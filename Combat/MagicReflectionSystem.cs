using Godot;
using System;

namespace EchoesAcrossTime.Combat
{
    /// <summary>
    /// Handles magic reflection mechanics (for Mirror Ward spell)
    /// </summary>
    public partial class MagicReflectionSystem : Node
    {
        public static MagicReflectionSystem Instance { get; private set; }

        [Signal]
        public delegate void MagicReflectedEventHandler(string reflectorName, string attackerName, int damage);

        public override void _Ready()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                QueueFree();
            }
        }

        /// <summary>
        /// Check if a character can reflect magic and process reflection
        /// Returns true if magic was reflected, false otherwise
        /// </summary>
        public bool TryReflectMagic(
            CharacterStats attacker, 
            CharacterStats target, 
            SkillData skill, 
            int originalDamage)
        {
            // Only reflect if target has MagicReflect status and skill is magical
            if (!HasMagicReflect(target) || !IsMagicalSkill(skill))
            {
                return false;
            }

            // Reflect the magic back at the attacker
            GD.Print($"✨ {target.CharacterName}'s Mirror Ward reflects the spell!");
            
            int reflectedDamage = CalculateReflectedDamage(originalDamage);
            attacker.TakeDamage(reflectedDamage, skill.Element);

            EmitSignal(SignalName.MagicReflected, target.CharacterName, attacker.CharacterName, reflectedDamage);
            GD.Print($"💥 {attacker.CharacterName} takes {reflectedDamage} reflected damage!");

            return true;
        }

        /// <summary>
        /// Check if character has active Magic Reflect status
        /// </summary>
        public bool HasMagicReflect(CharacterStats character)
        {
            if (character == null || !character.IsAlive) return false;

            foreach (var status in character.ActiveStatuses)
            {
                if (status.Effect == StatusEffect.MagicReflect)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Check if a skill is magical (can be reflected)
        /// </summary>
        private bool IsMagicalSkill(SkillData skill)
        {
            if (skill == null) return false;

            // Physical and Almighty cannot be reflected
            if (skill.Element == ElementType.Physical || skill.Element == ElementType.Almighty)
            {
                return false;
            }

            // Only damage-dealing magical skills can be reflected
            if (skill.DamageType == DamageType.Magical && skill.BasePower > 0)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Calculate reflected damage (can be full damage or reduced)
        /// </summary>
        private int CalculateReflectedDamage(int originalDamage)
        {
            // Mirror Ward reflects 100% of the damage
            return originalDamage;
        }

        /// <summary>
        /// Apply Magic Reflect status to a character
        /// </summary>
        public void ApplyMagicReflect(CharacterStats target, int duration = 2)
        {
            if (target == null) return;

            var statusManager = ActiveStatusEffect.StatusEffectManager.Instance;
            if (statusManager != null)
            {
                statusManager.ApplyStatus(target, StatusEffect.MagicReflect, duration);
                GD.Print($"🪞 {target.CharacterName} is now protected by Mirror Ward!");
            }
        }
    }
}