using Godot;
using System;
using System.Collections.Generic;

namespace EchoesAcrossTime.Combat
{
    /// <summary>
    /// Stores element affinity information for a character or enemy
    /// </summary>
    [GlobalClass]
    public partial class ElementAffinityData : Resource
    {
        // Using individual exports instead of Dictionary for better Godot compatibility
        [ExportGroup("Fire")]
        [Export] public ElementAffinity FireAffinity { get; set; } = ElementAffinity.Normal;
        
        [ExportGroup("Water")]
        [Export] public ElementAffinity WaterAffinity { get; set; } = ElementAffinity.Normal;
        
        [ExportGroup("Thunder")]
        [Export] public ElementAffinity ThunderAffinity { get; set; } = ElementAffinity.Normal;
        
        [ExportGroup("Ice")]
        [Export] public ElementAffinity IceAffinity { get; set; } = ElementAffinity.Normal;
        
        [ExportGroup("Earth")]
        [Export] public ElementAffinity EarthAffinity { get; set; } = ElementAffinity.Normal;
        
        [ExportGroup("Light")]
        [Export] public ElementAffinity LightAffinity { get; set; } = ElementAffinity.Normal;
        
        [ExportGroup("Dark")]
        [Export] public ElementAffinity DarkAffinity { get; set; } = ElementAffinity.Normal;
        
        [ExportGroup("Physical")]
        [Export] public ElementAffinity PhysicalAffinity { get; set; } = ElementAffinity.Normal;
        
        [ExportGroup("Damage Multipliers")]
        [Export] public float WeakMultiplier { get; set; } = 1.5f;
        [Export] public float ResistMultiplier { get; set; } = 0.5f;
        [Export] public float ImmuneMultiplier { get; set; } = 0f;
        [Export] public float AbsorbMultiplier { get; set; } = -1f; // Negative = healing
        
        /// <summary>
        /// Set affinity for a specific element
        /// </summary>
        public void SetAffinity(ElementType element, ElementAffinity affinity)
        {
            switch (element)
            {
                case ElementType.Fire:
                    FireAffinity = affinity;
                    break;
                case ElementType.Water:
                    WaterAffinity = affinity;
                    break;
                case ElementType.Thunder:
                    ThunderAffinity = affinity;
                    break;
                case ElementType.Ice:
                    IceAffinity = affinity;
                    break;
                case ElementType.Earth:
                    EarthAffinity = affinity;
                    break;
                case ElementType.Light:
                    LightAffinity = affinity;
                    break;
                case ElementType.Dark:
                    DarkAffinity = affinity;
                    break;
                case ElementType.Physical:
                    PhysicalAffinity = affinity;
                    break;
            }
        }
        
        /// <summary>
        /// Get affinity for a specific element
        /// </summary>
        public ElementAffinity GetAffinity(ElementType element)
        {
            return element switch
            {
                ElementType.Fire => FireAffinity,
                ElementType.Water => WaterAffinity,
                ElementType.Thunder => ThunderAffinity,
                ElementType.Ice => IceAffinity,
                ElementType.Earth => EarthAffinity,
                ElementType.Light => LightAffinity,
                ElementType.Dark => DarkAffinity,
                ElementType.Physical => PhysicalAffinity,
                _ => ElementAffinity.Normal
            };
        }
        
        /// <summary>
        /// Calculate damage multiplier based on element affinity
        /// </summary>
        public float GetDamageMultiplier(ElementType element)
        {
            var affinity = GetAffinity(element);
            
            return affinity switch
            {
                ElementAffinity.Weak => WeakMultiplier,
                ElementAffinity.Resist => ResistMultiplier,
                ElementAffinity.Immune => ImmuneMultiplier,
                ElementAffinity.Absorb => AbsorbMultiplier,
                _ => 1f
            };
        }
        
        /// <summary>
        /// Check if this affinity will result in healing
        /// </summary>
        public bool WillAbsorb(ElementType element)
        {
            return GetAffinity(element) == ElementAffinity.Absorb;
        }
        
        /// <summary>
        /// Check if this affinity will result in immunity
        /// </summary>
        public bool IsImmune(ElementType element)
        {
            return GetAffinity(element) == ElementAffinity.Immune;
        }
        
        /// <summary>
        /// Get all weaknesses
        /// </summary>
        public List<ElementType> GetWeaknesses()
        {
            var weaknesses = new List<ElementType>();
            
            if (FireAffinity == ElementAffinity.Weak) weaknesses.Add(ElementType.Fire);
            if (WaterAffinity == ElementAffinity.Weak) weaknesses.Add(ElementType.Water);
            if (ThunderAffinity == ElementAffinity.Weak) weaknesses.Add(ElementType.Thunder);
            if (IceAffinity == ElementAffinity.Weak) weaknesses.Add(ElementType.Ice);
            if (EarthAffinity == ElementAffinity.Weak) weaknesses.Add(ElementType.Earth);
            if (LightAffinity == ElementAffinity.Weak) weaknesses.Add(ElementType.Light);
            if (DarkAffinity == ElementAffinity.Weak) weaknesses.Add(ElementType.Dark);
            if (PhysicalAffinity == ElementAffinity.Weak) weaknesses.Add(ElementType.Physical);
            
            return weaknesses;
        }
        
        /// <summary>
        /// Get all resistances
        /// </summary>
        public List<ElementType> GetResistances()
        {
            var resistances = new List<ElementType>();
            
            if (FireAffinity == ElementAffinity.Resist) resistances.Add(ElementType.Fire);
            if (WaterAffinity == ElementAffinity.Resist) resistances.Add(ElementType.Water);
            if (ThunderAffinity == ElementAffinity.Resist) resistances.Add(ElementType.Thunder);
            if (IceAffinity == ElementAffinity.Resist) resistances.Add(ElementType.Ice);
            if (EarthAffinity == ElementAffinity.Resist) resistances.Add(ElementType.Earth);
            if (LightAffinity == ElementAffinity.Resist) resistances.Add(ElementType.Light);
            if (DarkAffinity == ElementAffinity.Resist) resistances.Add(ElementType.Dark);
            if (PhysicalAffinity == ElementAffinity.Resist) resistances.Add(ElementType.Physical);
            
            return resistances;
        }
        
        /// <summary>
        /// Get all immunities
        /// </summary>
        public List<ElementType> GetImmunities()
        {
            var immunities = new List<ElementType>();
            
            if (FireAffinity == ElementAffinity.Immune) immunities.Add(ElementType.Fire);
            if (WaterAffinity == ElementAffinity.Immune) immunities.Add(ElementType.Water);
            if (ThunderAffinity == ElementAffinity.Immune) immunities.Add(ElementType.Thunder);
            if (IceAffinity == ElementAffinity.Immune) immunities.Add(ElementType.Ice);
            if (EarthAffinity == ElementAffinity.Immune) immunities.Add(ElementType.Earth);
            if (LightAffinity == ElementAffinity.Immune) immunities.Add(ElementType.Light);
            if (DarkAffinity == ElementAffinity.Immune) immunities.Add(ElementType.Dark);
            if (PhysicalAffinity == ElementAffinity.Immune) immunities.Add(ElementType.Physical);
            
            return immunities;
        }
        
        /// <summary>
        /// Get all absorptions
        /// </summary>
        public List<ElementType> GetAbsorptions()
        {
            var absorptions = new List<ElementType>();
            
            if (FireAffinity == ElementAffinity.Absorb) absorptions.Add(ElementType.Fire);
            if (WaterAffinity == ElementAffinity.Absorb) absorptions.Add(ElementType.Water);
            if (ThunderAffinity == ElementAffinity.Absorb) absorptions.Add(ElementType.Thunder);
            if (IceAffinity == ElementAffinity.Absorb) absorptions.Add(ElementType.Ice);
            if (EarthAffinity == ElementAffinity.Absorb) absorptions.Add(ElementType.Earth);
            if (LightAffinity == ElementAffinity.Absorb) absorptions.Add(ElementType.Light);
            if (DarkAffinity == ElementAffinity.Absorb) absorptions.Add(ElementType.Dark);
            if (PhysicalAffinity == ElementAffinity.Absorb) absorptions.Add(ElementType.Physical);
            
            return absorptions;
        }
        
        /// <summary>
        /// Create a copy of this affinity data
        /// </summary>
        public ElementAffinityData DuplicateData()
        {
            var copy = new ElementAffinityData
            {
                FireAffinity = this.FireAffinity,
                WaterAffinity = this.WaterAffinity,
                ThunderAffinity = this.ThunderAffinity,
                IceAffinity = this.IceAffinity,
                EarthAffinity = this.EarthAffinity,
                LightAffinity = this.LightAffinity,
                DarkAffinity = this.DarkAffinity,
                PhysicalAffinity = this.PhysicalAffinity,
                WeakMultiplier = this.WeakMultiplier,
                ResistMultiplier = this.ResistMultiplier,
                ImmuneMultiplier = this.ImmuneMultiplier,
                AbsorbMultiplier = this.AbsorbMultiplier
            };
            
            return copy;
        }
    }
}