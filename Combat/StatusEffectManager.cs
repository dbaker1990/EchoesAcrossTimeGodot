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
        public int Duration { get; set; } // Turns remaining, -1 = permanent
        public int Power { get; set; } // For poison damage, regen amount, etc.
        public string SourceId { get; set; } // Who/what caused it
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
                    StatusEffect.Doom or
                    StatusEffect.AttackDown or StatusEffect.DefenseDown or
                    StatusEffect.MagicAttackDown or StatusEffect.MagicDefenseDown or
                    StatusEffect.SpeedDown or StatusEffect.LuckDown => StatusCategory.Debuff,

                StatusEffect.Regen or StatusEffect.Haste or StatusEffect.Barrier or
                    StatusEffect.AttackUp or StatusEffect.DefenseUp or
                    StatusEffect.MagicAttackUp or StatusEffect.MagicDefenseUp or
                    StatusEffect.SpeedUp or StatusEffect.LuckUp or
                    StatusEffect.MagicReflect => StatusCategory.Buff,

                StatusEffect.Berserk or StatusEffect.Charm => StatusCategory.Neutral,

                _ => StatusCategory.Neutral
            };
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

            // ========================================
// STEP 4: Modify ApplyStatus method
// UPDATE the existing ApplyStatus method
// ========================================

            public bool ApplyStatus(CharacterStats target, StatusEffect status, int duration, int power = 0,
                string sourceId = "")
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
                    existing.StackCount = Math.Min(existing.StackCount + 1, 5); // Max 5 stacks

                    GD.Print($"{status} on {target.CharacterName} refreshed (stacks: {existing.StackCount})");
                }
                else
                {
                    // Add new status
                    var newStatus = new ActiveStatusEffect(status, duration, power, sourceId);
                    target.ActiveStatuses.Add(newStatus);

                    // NEW: Apply stat modifier for buff/debuff statuses
                    float multiplier = GetStatMultiplier(status, power);
                    if (multiplier != 1.0f)
                    {
                        ApplyStatModifier(target, status, multiplier);
                    }

                    EmitSignal(SignalName.StatusApplied, target.CharacterName, (int)status);
                    GD.Print($"{target.CharacterName} afflicted with {status} for {duration} turns");
                }

                return true;
            }


            /// <summary>
            /// Apply stat modifiers when status is applied
            /// </summary>
            private void ApplyStatModifier(CharacterStats target, StatusEffect status, float multiplier)
            {
                switch (status)
                {
                    case StatusEffect.AttackUp:
                        target.TemporaryAttackMultiplier *= multiplier;
                        GD.Print($"{target.CharacterName}'s Attack increased! (x{multiplier})");
                        break;

                    case StatusEffect.AttackDown:
                        target.TemporaryAttackMultiplier *= multiplier;
                        GD.Print($"{target.CharacterName}'s Attack decreased! (x{multiplier})");
                        break;

                    case StatusEffect.DefenseUp:
                        target.TemporaryDefenseMultiplier *= multiplier;
                        GD.Print($"{target.CharacterName}'s Defense increased! (x{multiplier})");
                        break;

                    case StatusEffect.DefenseDown:
                        target.TemporaryDefenseMultiplier *= multiplier;
                        GD.Print($"{target.CharacterName}'s Defense decreased! (x{multiplier})");
                        break;

                    case StatusEffect.MagicAttackUp:
                        target.TemporaryMagicAttackMultiplier *= multiplier;
                        GD.Print($"{target.CharacterName}'s Magic Attack increased! (x{multiplier})");
                        break;

                    case StatusEffect.MagicAttackDown:
                        target.TemporaryMagicAttackMultiplier *= multiplier;
                        GD.Print($"{target.CharacterName}'s Magic Attack decreased! (x{multiplier})");
                        break;

                    case StatusEffect.MagicDefenseUp:
                        target.TemporaryMagicDefenseMultiplier *= multiplier;
                        GD.Print($"{target.CharacterName}'s Magic Defense increased! (x{multiplier})");
                        break;

                    case StatusEffect.MagicDefenseDown:
                        target.TemporaryMagicDefenseMultiplier *= multiplier;
                        GD.Print($"{target.CharacterName}'s Magic Defense decreased! (x{multiplier})");
                        break;

                    case StatusEffect.SpeedUp:
                        target.TemporarySpeedMultiplier *= multiplier;
                        GD.Print($"{target.CharacterName}'s Speed increased! (x{multiplier})");
                        break;

                    case StatusEffect.SpeedDown:
                        target.TemporarySpeedMultiplier *= multiplier;
                        GD.Print($"{target.CharacterName}'s Speed decreased! (x{multiplier})");
                        break;

                    case StatusEffect.LuckUp:
                        target.TemporaryLuckMultiplier *= multiplier;
                        GD.Print($"{target.CharacterName}'s Luck increased! (x{multiplier})");
                        break;

                    case StatusEffect.LuckDown:
                        target.TemporaryLuckMultiplier *= multiplier;
                        GD.Print($"{target.CharacterName}'s Luck decreased! (x{multiplier})");
                        break;
                }
            }

            /// <summary>
            /// Remove stat modifiers when status expires
            /// </summary>
            private void RemoveStatModifier(CharacterStats target, StatusEffect status, float multiplier)
            {
                // Divide by the multiplier to reverse the effect
                switch (status)
                {
                    case StatusEffect.AttackUp:
                    case StatusEffect.AttackDown:
                        target.TemporaryAttackMultiplier /= multiplier;
                        GD.Print($"{target.CharacterName}'s Attack returned to normal");
                        break;

                    case StatusEffect.DefenseUp:
                    case StatusEffect.DefenseDown:
                        target.TemporaryDefenseMultiplier /= multiplier;
                        GD.Print($"{target.CharacterName}'s Defense returned to normal");
                        break;

                    case StatusEffect.MagicAttackUp:
                    case StatusEffect.MagicAttackDown:
                        target.TemporaryMagicAttackMultiplier /= multiplier;
                        GD.Print($"{target.CharacterName}'s Magic Attack returned to normal");
                        break;

                    case StatusEffect.MagicDefenseUp:
                    case StatusEffect.MagicDefenseDown:
                        target.TemporaryMagicDefenseMultiplier /= multiplier;
                        GD.Print($"{target.CharacterName}'s Magic Defense returned to normal");
                        break;

                    case StatusEffect.SpeedUp:
                    case StatusEffect.SpeedDown:
                        target.TemporarySpeedMultiplier /= multiplier;
                        GD.Print($"{target.CharacterName}'s Speed returned to normal");
                        break;

                    case StatusEffect.LuckUp:
                    case StatusEffect.LuckDown:
                        target.TemporaryLuckMultiplier /= multiplier;
                        GD.Print($"{target.CharacterName}'s Luck returned to normal");
                        break;
                }
            }


            // ========================================
// STEP 5: Add Helper Method for Multipliers
// Add this NEW METHOD to StatusEffectManager
// ========================================

            /// <summary>
            /// Get the stat multiplier based on status effect and power
            /// Power field stores the percentage boost (e.g., 30 = 30% boost)
            /// </summary>
            private float GetStatMultiplier(StatusEffect status, int power)
            {
                // If no power specified, use default values
                if (power == 0)
                {
                    return status switch
                    {
                        StatusEffect.AttackUp => 1.3f, // +30%
                        StatusEffect.AttackDown => 0.7f, // -30%
                        StatusEffect.DefenseUp => 1.3f,
                        StatusEffect.DefenseDown => 0.7f,
                        StatusEffect.MagicAttackUp => 1.3f,
                        StatusEffect.MagicAttackDown => 0.7f,
                        StatusEffect.MagicDefenseUp => 1.3f,
                        StatusEffect.MagicDefenseDown => 0.7f,
                        StatusEffect.SpeedUp => 1.5f, // +50% for speed
                        StatusEffect.SpeedDown => 0.5f, // -50% for speed
                        StatusEffect.LuckUp => 1.2f, // +20%
                        StatusEffect.LuckDown => 0.8f, // -20%
                        _ => 1.0f
                    };
                }

                // Use power value to calculate multiplier
                // For "Up" effects: 1 + (power / 100)
                // For "Down" effects: 1 - (power / 100)
                bool isDebuff = status.ToString().EndsWith("Down");
                float multiplier = isDebuff ? (1.0f - power / 100f) : (1.0f + power / 100f);

                return Mathf.Clamp(multiplier, 0.1f, 3.0f); // Min 10%, Max 300%
            }

            // ========================================
// STEP 6: Modify RemoveStatus method
// UPDATE the existing RemoveStatus method
// ========================================

            public bool RemoveStatus(CharacterStats target, StatusEffect status)
            {
                if (target == null) return false;

                var statusToRemove = target.ActiveStatuses.Find(s => s.Effect == status);

                if (statusToRemove != null)
                {
                    // NEW: Remove stat modifier before removing status
                    float multiplier = GetStatMultiplier(status, statusToRemove.Power);
                    if (multiplier != 1.0f)
                    {
                        RemoveStatModifier(target, status, multiplier);
                    }

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

            // ========================================
// STEP 7: Update ProcessStatusEffects
// Modify this to remove expired stat buffs properly
// ========================================

            public void ProcessStatusEffects(CharacterStats target, bool isTurnStart)
            {
                if (target == null || !target.IsAlive) return;

                var statusesToProcess = new List<ActiveStatusEffect>(target.ActiveStatuses);

                foreach (var status in statusesToProcess)
                {
                    ProcessIndividualStatus(target, status, isTurnStart);
                }

                // NEW: Remove stat modifiers before removing expired statuses
                var expiredStatuses = target.ActiveStatuses.Where(s => s.Duration == 0).ToList();
                foreach (var expired in expiredStatuses)
                {
                    float multiplier = GetStatMultiplier(expired.Effect, expired.Power);
                    if (multiplier != 1.0f)
                    {
                        RemoveStatModifier(target, expired.Effect, multiplier);
                    }
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
                    
                    case StatusEffect.MagicReflect:
                        // Mirror Ward just needs to exist - reflection is handled by MagicReflectionSystem
                        if (isTurnStart)
                        {
                            GD.Print($"🪞 {target.CharacterName}'s Mirror Ward is active! ({status.Duration} turns remaining)");
                        }
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
                    StatusEffect.Poison => new Color(0.5f, 0.2f, 0.8f), // Purple
                    StatusEffect.Burn => new Color(1.0f, 0.3f, 0.0f), // Orange
                    StatusEffect.Freeze => new Color(0.3f, 0.8f, 1.0f), // Cyan
                    StatusEffect.Sleep => new Color(0.3f, 0.3f, 0.8f), // Blue
                    StatusEffect.Paralysis => new Color(0.9f, 0.9f, 0.2f), // Yellow
                    StatusEffect.Silence => new Color(0.6f, 0.6f, 0.6f), // Gray
                    StatusEffect.Regen => new Color(0.2f, 1.0f, 0.3f), // Green
                    StatusEffect.Haste => new Color(1.0f, 0.8f, 0.2f), // Gold
                    StatusEffect.Doom => new Color(0.1f, 0.1f, 0.1f), // Black
                    StatusEffect.MagicReflect => new Color(0.6f, 0.8f, 1.0f), // Light Blue/Silver
                    _ => Colors.White
                };
            }
        }
    }
}
