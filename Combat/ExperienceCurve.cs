using Godot;
using System;
using EchoesAcrossTime.Database;

namespace EchoesAcrossTime.Combat
{
    public enum ExperienceCurveType
    {
        Linear,         // Constant increase per level
        Exponential1,   // Moderate exponential growth
        Exponential2,   // Steep exponential growth  
        Quadratic,      // Quadratic growth (balanced)
        Cubic,          // Cubic growth (very steep late game)
        Custom          // Use custom basis, inflation, and extra values
    }
    
    /// <summary>
    /// Defines experience curve similar to RPG Maker MZ
    /// Formula: EXP = basis * (level - 1)^inflation + extra
    /// </summary>
    [GlobalClass]
    public partial class ExperienceCurve : Resource
    {
        [ExportGroup("Curve Type")]
        [Export] public ExperienceCurveType CurveType { get; set; } = ExperienceCurveType.Quadratic;
        
        [ExportGroup("Custom Curve Parameters")]
        [Export] public int Basis { get; set; } = 30;
        [Export] public float Inflation { get; set; } = 2.0f;
        [Export] public int Extra { get; set; } = 20;
        
        [ExportGroup("Level Cap")]
        [Export] public int MaxLevel { get; set; } = 100;
        
        /// <summary>
        /// Calculate experience required to reach a specific level from level 1
        /// </summary>
        public int GetTotalExpForLevel(int level)
        {
            if (level <= 1) return 0;
            if (level > MaxLevel) level = MaxLevel;
            
            int totalExp = 0;
            for (int i = 2; i <= level; i++)
            {
                totalExp += GetExpForLevelUp(i);
            }
            
            return totalExp;
        }
        
        /// <summary>
        /// Calculate experience required to level up FROM a specific level
        /// e.g., GetExpForLevelUp(5) returns exp needed to go from level 5 to level 6
        /// </summary>
        public int GetExpForLevelUp(int currentLevel)
        {
            if (currentLevel >= MaxLevel) return 0;
            
            return CurveType switch
            {
                ExperienceCurveType.Linear => CalculateLinear(currentLevel),
                ExperienceCurveType.Exponential1 => CalculateExponential1(currentLevel),
                ExperienceCurveType.Exponential2 => CalculateExponential2(currentLevel),
                ExperienceCurveType.Quadratic => CalculateQuadratic(currentLevel),
                ExperienceCurveType.Cubic => CalculateCubic(currentLevel),
                ExperienceCurveType.Custom => CalculateCustom(currentLevel),
                _ => CalculateQuadratic(currentLevel)
            };
        }
        
        // Linear: basis * (level - 1) + extra
        // Example: level 2->3 = 50, level 50->51 = 1470
        private int CalculateLinear(int level)
        {
            return 30 * (level - 1) + 20;
        }
        
        // Exponential Type 1: basis * (level - 1)^1.5 + extra
        // Example: level 2->3 = 62, level 50->51 = 10,287
        private int CalculateExponential1(int level)
        {
            float result = 30f * Mathf.Pow(level - 1, 1.5f) + 20f;
            return Mathf.RoundToInt(result);
        }
        
        // Exponential Type 2: basis * (level - 1)^2 + extra  
        // Example: level 2->3 = 80, level 50->51 = 72,020
        private int CalculateExponential2(int level)
        {
            float result = 25f * Mathf.Pow(level - 1, 2f) + 30f;
            return Mathf.RoundToInt(result);
        }
        
        // Quadratic: basis * (level - 1)^2.4 + extra
        // Balanced curve similar to many JRPGs
        // Example: level 2->3 = 75, level 50->51 = 35,850
        private int CalculateQuadratic(int level)
        {
            float result = 20f * Mathf.Pow(level - 1, 2.4f) + 25f;
            return Mathf.RoundToInt(result);
        }
        
        // Cubic: basis * (level - 1)^3 + extra
        // Very steep late game growth
        // Example: level 2->3 = 120, level 50->51 = 2,941,245
        private int CalculateCubic(int level)
        {
            float result = 10f * Mathf.Pow(level - 1, 3f) + 100f;
            return Mathf.RoundToInt(result);
        }
        
        // Custom: Uses exported Basis, Inflation, and Extra values
        // Formula: basis * (level - 1)^inflation + extra
        private int CalculateCustom(int level)
        {
            float result = Basis * Mathf.Pow(level - 1, Inflation) + Extra;
            return Mathf.RoundToInt(Mathf.Max(1, result));
        }
        
        /// <summary>
        /// Get recommended curve for character type
        /// </summary>
        public static ExperienceCurve GetRecommendedCurve(CharacterType characterType)
        {
            return characterType switch
            {
                CharacterType.PlayableCharacter => CreateQuadraticCurve(),
                CharacterType.Enemy => CreateLinearCurve(),
                CharacterType.Boss => CreateExponential2Curve(),
                _ => CreateQuadraticCurve()
            };
        }
        
        // Preset curves
        public static ExperienceCurve CreateLinearCurve()
        {
            return new ExperienceCurve
            {
                CurveType = ExperienceCurveType.Linear,
                MaxLevel = 100
            };
        }
        
        public static ExperienceCurve CreateExponential1Curve()
        {
            return new ExperienceCurve
            {
                CurveType = ExperienceCurveType.Exponential1,
                MaxLevel = 100
            };
        }
        
        public static ExperienceCurve CreateExponential2Curve()
        {
            return new ExperienceCurve
            {
                CurveType = ExperienceCurveType.Exponential2,
                MaxLevel = 100
            };
        }
        
        public static ExperienceCurve CreateQuadraticCurve()
        {
            return new ExperienceCurve
            {
                CurveType = ExperienceCurveType.Quadratic,
                MaxLevel = 100
            };
        }
        
        public static ExperienceCurve CreateCubicCurve()
        {
            return new ExperienceCurve
            {
                CurveType = ExperienceCurveType.Cubic,
                MaxLevel = 100
            };
        }
        
        public static ExperienceCurve CreateCustomCurve(int basis, float inflation, int extra, int maxLevel = 100)
        {
            return new ExperienceCurve
            {
                CurveType = ExperienceCurveType.Custom,
                Basis = basis,
                Inflation = inflation,
                Extra = extra,
                MaxLevel = maxLevel
            };
        }
    }
}