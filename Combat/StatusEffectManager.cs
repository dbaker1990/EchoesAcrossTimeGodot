// Combat/StatusEffectManager.cs
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EchoesAcrossTime.Combat
{
    /// <summary>
    /// Represents an active status effect on a character
    /// </summary>
    public partial class ActiveStatusEffect : GodotObject
    {
        public StatusEffect Effect { get; set; }
        public int Duration { get; set; }  // Turns remaining, -1 = permanent
        public int Power { get; set; }     // For poison damage, regen amount, etc.
        public string SourceId { get; set; }  // Who/what caused it
        public int StackCount { get; set; } = 1;
        public bool IsDebuff { get; set; }
        
        public ActiveStatusEffect(StatusEffect effect, int duration, int power = 0, string sourceId = "")
        {
            Effect = effect;
            Duration = duration;
            Power = power;
            SourceId = sourceId;
            IsDebuff = GetStatusCategory(effect) == StatusCategory.Debuff;
        }

        public enum StatusCategory
        {
            Debuff,
            Buff,
            Neutral
        }

        public static StatusCategory GetStatusCategory(StatusEffect effect)
        {
            return effect switch
            {
                StatusEffect.Poison or StatusEffect.Sleep or StatusEffect.Paralysis or
                StatusEffect.Confusion or StatusEffect.Silence or StatusEffect.Blind or
                StatusEffect.Stun or StatusEffect.Burn or StatusEffect.Freeze or
                StatusEffect.Shock or StatusEffect.Curse or StatusEffect.Petrify or
                StatusEffect.Doom => StatusCategory.Debuff,
                
                StatusEffect.Regen or StatusEffect.Haste or StatusEffect.Barrier => StatusCategory.Buff,
                
                StatusEffect.Berserk or StatusEffect.Charm => StatusCategory.Neutral,
                
                _ => StatusCategory.Neutral
            };
        }
    }

    /// <summary>
    /// Manages status effects for all characters
    /// </summary>
    public partial class StatusEffectManager : Node
    {
        public static StatusEffectManager Instance { get; private set; }

        [Signal]
        public delegate void StatusAppliedEventHandler(string characterId, StatusEffect effect);

        [Signal]
        public delegate void StatusRemovedEventHandler(string characterId, StatusEffect effect);

        [Signal]
        public delegate void StatusTickEventHandler(string characterId, StatusEffect effect, int damage);

        public override void _Ready()
        {
            if (Instance != null)
            {
                QueueFree();
                return;
            }

            Instance = this;
            GD.Print("StatusEffectManager initialized");
        }

        /// <summary>
        /// Apply a status effect to a character
        /// </summary>
        public bool ApplyStatus(CharacterStats target, StatusEffect status, int duration, int power = 0, string sourceId = "")
        {
            if (target == null || status == StatusEffect.None)
                return false;

            // Check resistance
            if (target.BattleStats.RollStatusResistance(status))
            {
                GD.Print($"{target.CharacterName} resisted {status}!");
                return false;
            }

            // Check for passive immunity
            var passiveSkills = target.Skills?.GetPassiveSkills() ?? new List<SkillData>();
            foreach (var passive in passiveSkills)
            {
                if (passive.PassiveStatusImmunity.Contains(status))
                {
                    GD.Print($"{target.CharacterName} is immune to {status}!");
                    return false;
                }
            }

            // Check if already has this status
            var existing = target.ActiveStatuses.Find(s => s.Effect == status);

            if (existing != null)
            {
                // Refresh duration and increase power
                existing.Duration = Math.Max(existing.Duration, duration);
                existing.Power = Math.Max(existing.Power, power);
                existing.StackCount = Math.Min(existing.StackCount + 1, 5);  // Max 5 stacks
                
                GD.Print($"{status} on {target.CharacterName} refreshed (stacks: {existing.StackCount})");
            }
            else
            {
                // Add new status
                var newStatus = new ActiveStatusEffect(status, duration, power, sourceId);
                target.ActiveStatuses.Add(newStatus);
                
                EmitSignal(SignalName.StatusApplied, target.CharacterName, (int)status);
                GD.Print($"{target.CharacterName} afflicted with {status} for {duration} turns");
            }

            return true;
        }

        /// <summary>
        /// Remove a status effect
        /// </summary>
        public bool RemoveStatus(CharacterStats target, StatusEffect status)
        {
            if (target == null) return false;

            var statusToRemove = target.ActiveStatuses.Find(s => s.Effect == status);

            if (statusToRemove != null)
            {
                target.ActiveStatuses.Remove(statusToRemove);
                EmitSignal(SignalName.StatusRemoved, target.CharacterName, (int)status);
                GD.Print($"{status} removed from {target.CharacterName}");
                return true;
            }

            return false;
        }

        /// <summary>
        /// Remove all debuffs
        /// </summary>
        public int RemoveAllDebuffs(CharacterStats target)
        {
            if (target == null) return 0;

            int removedCount = 0;
            var debuffs = target.ActiveStatuses.Where(s => s.IsDebuff).ToList();

            foreach (var debuff in debuffs)
            {
                target.ActiveStatuses.Remove(debuff);
                EmitSignal(SignalName.StatusRemoved, target.CharacterName, (int)debuff.Effect);
                removedCount++;
            }

            if (removedCount > 0)
            {
                GD.Print($"Removed {removedCount} debuffs from {target.CharacterName}");
            }

            return removedCount;
        }

        /// <summary>
        /// Process status effects at turn start/end
        /// </summary>
        public void ProcessStatusEffects(CharacterStats target, bool isTurnStart)
        {
            if (target == null || !target.IsAlive) return;

            var statusesToProcess = new List<ActiveStatusEffect>(target.ActiveStatuses);

            foreach (var status in statusesToProcess)
            {
                ProcessIndividualStatus(target, status, isTurnStart);
            }

            // Remove expired statuses
            target.ActiveStatuses.RemoveAll(s => s.Duration == 0);
        }

        /// <summary>
        /// Process individual status effect
        /// </summary>
        private void ProcessIndividualStatus(CharacterStats target, ActiveStatusEffect status, bool isTurnStart)
        {
            switch (status.Effect)
            {
                case StatusEffect.Poison:
                    if (isTurnStart)
                    {
                        int damage = Math.Max(1, status.Power * status.StackCount);
                        target.TakeDamage(damage, ElementType.None);
                        EmitSignal(SignalName.StatusTick, target.CharacterName, (int)status.Effect, damage);
                        GD.Print($"{target.CharacterName} takes {damage} poison damage");
                    }
                    break;

                case StatusEffect.Burn:
                    if (isTurnStart)
                    {
                        int damage = Math.Max(1, status.Power * status.StackCount);
                        target.TakeDamage(damage, ElementType.Fire);
                        EmitSignal(SignalName.StatusTick, target.CharacterName, (int)status.Effect, damage);
                    }
                    break;

                case StatusEffect.Regen:
                    if (isTurnStart)
                    {
                        int healing = Math.Max(1, status.Power * status.StackCount);
                        target.Heal(healing);
                        EmitSignal(SignalName.StatusTick, target.CharacterName, (int)status.Effect, healing);
                        GD.Print($"{target.CharacterName} regenerates {healing} HP");
                    }
                    break;

                case StatusEffect.Sleep:
                case StatusEffect.Stun:
                case StatusEffect.Freeze:
                case StatusEffect.Petrify:
                    // These prevent actions - handled by battle system
                    break;

                case StatusEffect.Doom:
                    if (isTurnStart && status.Duration == 1)
                    {
                        // Instant death when doom counter reaches 0
                        target.TakeDamage(target.MaxHP, ElementType.None);
                        GD.Print($"{target.CharacterName} succumbs to Doom!");
                    }
                    break;
            }

            // Decrement duration
            if (status.Duration > 0)
            {
                status.Duration--;
            }
        }

        /// <summary>
        /// Check if character can act (not stunned, sleeping, etc.)
        /// </summary>
        public bool CanAct(CharacterStats character)
        {
            if (character == null || !character.IsAlive) return false;

            foreach (var status in character.ActiveStatuses)
            {
                if (status.Effect == StatusEffect.Sleep ||
                    status.Effect == StatusEffect.Stun ||
                    status.Effect == StatusEffect.Freeze ||
                    status.Effect == StatusEffect.Petrify)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Check if character has specific status
        /// </summary>
        public bool HasStatus(CharacterStats character, StatusEffect status)
        {
            return character?.ActiveStatuses.Any(s => s.Effect == status) ?? false;
        }

        /// <summary>
        /// Get status power/stacks
        /// </summary>
        public int GetStatusPower(CharacterStats character, StatusEffect status)
        {
            var activeStatus = character?.ActiveStatuses.Find(s => s.Effect == status);
            return activeStatus?.Power ?? 0;
        }

        /// <summary>
        /// Get all active statuses
        /// </summary>
        public List<ActiveStatusEffect> GetActiveStatuses(CharacterStats character)
        {
            return character?.ActiveStatuses ?? new List<ActiveStatusEffect>();
        }

        /// <summary>
        /// Clear all statuses (for battle end, death, etc.)
        /// </summary>
        public void ClearAllStatuses(CharacterStats character)
        {
            if (character == null) return;

            int count = character.ActiveStatuses.Count;
            character.ActiveStatuses.Clear();

            if (count > 0)
            {
                GD.Print($"Cleared all {count} statuses from {character.CharacterName}");
            }
        }

        /// <summary>
        /// Get status icon/color for UI
        /// </summary>
        public Color GetStatusColor(StatusEffect status)
        {
            return status switch
            {
                StatusEffect.Poison => new Color(0.5f, 0.2f, 0.8f),  // Purple
                StatusEffect.Burn => new Color(1.0f, 0.3f, 0.0f),    // Orange
                StatusEffect.Freeze => new Color(0.3f, 0.8f, 1.0f),  // Cyan
                StatusEffect.Sleep => new Color(0.3f, 0.3f, 0.8f),   // Blue
                StatusEffect.Paralysis => new Color(0.9f, 0.9f, 0.2f), // Yellow
                StatusEffect.Silence => new Color(0.6f, 0.6f, 0.6f), // Gray
                StatusEffect.Regen => new Color(0.2f, 1.0f, 0.3f),   // Green
                StatusEffect.Haste => new Color(1.0f, 0.8f, 0.2f),   // Gold
                StatusEffect.Doom => new Color(0.1f, 0.1f, 0.1f),    // Black
                _ => Colors.White
            };
        }
    }
}