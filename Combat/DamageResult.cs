using Godot;
using System.Collections.Generic;

namespace EchoesAcrossTime.Combat
{
    /// <summary>
    /// Result data from damage calculation
    /// Contains information about damage dealt and effects applied
    /// </summary>
    public class DamageResult
    {
        // Damage values
        public int DamageDealt { get; set; }
        public int HealingDealt { get; set; }
        
        // Element information
        public ElementType Element { get; set; } = ElementType.Physical;
        
        // Combat flags
        public bool WasCritical { get; set; }
        public bool HitWeakness { get; set; }
        public bool WasResisted { get; set; }
        public bool WasImmune { get; set; }
        public bool WasAbsorbed { get; set; }
        public bool Missed { get; set; }
        
        // Status effects
        public List<StatusEffect> StatusesApplied { get; set; } = new();
        public List<StatusEffect> StatusesRemoved { get; set; } = new();
        
        // Additional info
        public bool Success { get; set; } = true;
        public string Message { get; set; } = "";
        
        // Multipliers applied
        public float FinalMultiplier { get; set; } = 1.0f;
        
        public DamageResult()
        {
            StatusesApplied = new List<StatusEffect>();
            StatusesRemoved = new List<StatusEffect>();
        }
        
        /// <summary>
        /// Create a simple damage result
        /// </summary>
        public static DamageResult SimpleDamage(int damage, ElementType element = ElementType.Physical)
        {
            return new DamageResult
            {
                DamageDealt = damage,
                Element = element,
                Success = true
            };
        }
        
        /// <summary>
        /// Create a healing result
        /// </summary>
        public static DamageResult SimpleHealing(int healing)
        {
            return new DamageResult
            {
                HealingDealt = healing,
                Success = true
            };
        }
        
        /// <summary>
        /// Create a miss result
        /// </summary>
        public static DamageResult Miss()
        {
            return new DamageResult
            {
                Missed = true,
                Success = false,
                Message = "Miss!"
            };
        }
    }
}