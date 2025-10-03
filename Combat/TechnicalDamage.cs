using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EchoesAcrossTime.Combat
{
    /// <summary>
    /// Technical damage combos - exploiting status effects with specific attack types
    /// Based on Persona 5's technical damage system
    /// </summary>
    public enum TechnicalComboType
    {
        None,
        BurnWithWind,      // Burn + Wind/Nuclear attack
        FreezeWithPhysical, // Freeze + Physical attack
        ShockWithPhysical,  // Shock/Paralysis + Physical attack
        SleepWithAny,       // Sleep + Any attack (wakes them up)
        PoisonWithNuke,     // Poison + Nuclear attack
        ConfuseWithPsychic, // Confusion + Psychic attack
        FearWithDark        // Fear + Dark attack
    }
    
    /// <summary>
    /// Result of technical damage check
    /// </summary>
    public class TechnicalResult
    {
        public bool IsTechnical { get; set; }
        public TechnicalComboType ComboType { get; set; }
        public float DamageMultiplier { get; set; }
        public string Message { get; set; }
        
        public TechnicalResult()
        {
            IsTechnical = false;
            ComboType = TechnicalComboType.None;
            DamageMultiplier = 1.0f;
            Message = "";
        }
    }
    
    /// <summary>
    /// Technical damage system manager
    /// </summary>
    public class TechnicalDamageSystem
    {
        // Technical damage multiplier
        private const float TECHNICAL_MULTIPLIER = 1.5f;
        
        // Define which combos are valid
        private static readonly Dictionary<StatusEffect, List<(ElementType, TechnicalComboType)>> TechnicalCombos = new()
        {
            {
                StatusEffect.Burn, new List<(ElementType, TechnicalComboType)>
                {
                    (ElementType.Thunder, TechnicalComboType.BurnWithWind),
                    (ElementType.Ice, TechnicalComboType.BurnWithWind) // Ice can work too
                }
            },
            {
                StatusEffect.Freeze, new List<(ElementType, TechnicalComboType)>
                {
                    (ElementType.Physical, TechnicalComboType.FreezeWithPhysical)
                }
            },
            {
                StatusEffect.Paralysis, new List<(ElementType, TechnicalComboType)>
                {
                    (ElementType.Physical, TechnicalComboType.ShockWithPhysical)
                }
            },
            {
                StatusEffect.Sleep, new List<(ElementType, TechnicalComboType)>
                {
                    (ElementType.Physical, TechnicalComboType.SleepWithAny),
                    (ElementType.Fire, TechnicalComboType.SleepWithAny),
                    (ElementType.Ice, TechnicalComboType.SleepWithAny),
                    (ElementType.Thunder, TechnicalComboType.SleepWithAny),
                    (ElementType.Water, TechnicalComboType.SleepWithAny),
                    (ElementType.Earth, TechnicalComboType.SleepWithAny),
                    (ElementType.Light, TechnicalComboType.SleepWithAny),
                    (ElementType.Dark, TechnicalComboType.SleepWithAny)
                }
            },
            {
                StatusEffect.Poison, new List<(ElementType, TechnicalComboType)>
                {
                    (ElementType.Fire, TechnicalComboType.PoisonWithNuke),
                    (ElementType.Thunder, TechnicalComboType.PoisonWithNuke)
                }
            },
            {
                StatusEffect.Confusion, new List<(ElementType, TechnicalComboType)>
                {
                    (ElementType.Light, TechnicalComboType.ConfuseWithPsychic),
                    (ElementType.Dark, TechnicalComboType.ConfuseWithPsychic)
                }
            }
        };
        
        /// <summary>
        /// Check if attack triggers technical damage
        /// </summary>
        public TechnicalResult CheckTechnical(CharacterStats target, ElementType attackElement)
        {
            var result = new TechnicalResult();
            
            if (target == null || !target.IsAlive)
                return result;
            
            // Check each active status effect
            foreach (var status in target.ActiveStatuses)
            {
                if (TechnicalCombos.TryGetValue(status.Effect, out var combos))
                {
                    // Check if this element creates a technical
                    foreach (var (element, comboType) in combos)
                    {
                        if (element == attackElement)
                        {
                            result.IsTechnical = true;
                            result.ComboType = comboType;
                            result.DamageMultiplier = TECHNICAL_MULTIPLIER;
                            result.Message = GetTechnicalMessage(comboType, status.Effect);
                            
                            GD.Print($"★★★ TECHNICAL! {result.Message} ★★★");
                            return result;
                        }
                    }
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// Get descriptive message for technical combo
        /// </summary>
        private string GetTechnicalMessage(TechnicalComboType combo, StatusEffect status)
        {
            return combo switch
            {
                TechnicalComboType.BurnWithWind => $"Exploited {status}!",
                TechnicalComboType.FreezeWithPhysical => $"Shattered frozen foe!",
                TechnicalComboType.ShockWithPhysical => $"Struck paralyzed foe!",
                TechnicalComboType.SleepWithAny => $"Woke sleeping foe violently!",
                TechnicalComboType.PoisonWithNuke => $"Detonated poison!",
                TechnicalComboType.ConfuseWithPsychic => $"Exploited confusion!",
                TechnicalComboType.FearWithDark => $"Exploited fear!",
                _ => "Technical Hit!"
            };
        }
        
        /// <summary>
        /// Apply technical damage multiplier
        /// </summary>
        public int ApplyTechnicalDamage(int baseDamage, TechnicalResult technical)
        {
            if (!technical.IsTechnical)
                return baseDamage;
            
            return Mathf.RoundToInt(baseDamage * technical.DamageMultiplier);
        }
        
        /// <summary>
        /// Check if technical should remove status (like waking from sleep)
        /// </summary>
        public bool ShouldRemoveStatus(TechnicalComboType combo, StatusEffect status)
        {
            // Sleep is always removed when hit
            if (status == StatusEffect.Sleep)
                return true;
            
            // Freeze is removed by physical attacks
            if (status == StatusEffect.Freeze && combo == TechnicalComboType.FreezeWithPhysical)
                return true;
            
            return false;
        }
        
        /// <summary>
        /// Get all possible technical combos for a target
        /// (for UI display or AI decision making)
        /// </summary>
        public List<(StatusEffect, ElementType)> GetPossibleTechnicals(CharacterStats target)
        {
            var possibles = new List<(StatusEffect, ElementType)>();
            
            if (target == null) return possibles;
            
            foreach (var status in target.ActiveStatuses)
            {
                if (TechnicalCombos.TryGetValue(status.Effect, out var combos))
                {
                    foreach (var (element, _) in combos)
                    {
                        possibles.Add((status.Effect, element));
                    }
                }
            }
            
            return possibles;
        }
        
        /// <summary>
        /// Check if element can create technical on this target
        /// </summary>
        public bool CanCreateTechnical(CharacterStats target, ElementType element)
        {
            if (target == null) return false;
            
            foreach (var status in target.ActiveStatuses)
            {
                if (TechnicalCombos.TryGetValue(status.Effect, out var combos))
                {
                    if (combos.Any(c => c.Item1 == element))
                        return true;
                }
            }
            
            return false;
        }
    }
}