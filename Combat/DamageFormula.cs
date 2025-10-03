using Godot;
using System;

namespace EchoesAcrossTime.Combat
{
    public enum DamageFormulaType
    {
        Simple,              // Basic: Attack - Defense
        Persona,             // Persona 4/5 style
        FinalFantasy7,       // FF7 style
        FinalFantasy8,       // FF8 style  
        FinalFantasy9,       // FF9 style
        Pokemon,             // Pokemon style
        Custom               // Use custom multipliers
    }

    /// <summary>
    /// Damage calculation formulas from various JRPGs
    /// </summary>
    public static class DamageFormula
    {
        /// <summary>
        /// Calculate damage based on selected formula
        /// </summary>
        public static int Calculate(
            DamageFormulaType formulaType,
            CharacterStats attacker,
            CharacterStats defender,
            SkillData skill,
            bool isCritical = false)
        {
            int baseDamage = formulaType switch
            {
                DamageFormulaType.Simple => CalculateSimple(attacker, defender, skill),
                DamageFormulaType.Persona => CalculatePersona(attacker, defender, skill),
                DamageFormulaType.FinalFantasy7 => CalculateFF7(attacker, defender, skill),
                DamageFormulaType.FinalFantasy8 => CalculateFF8(attacker, defender, skill),
                DamageFormulaType.FinalFantasy9 => CalculateFF9(attacker, defender, skill),
                DamageFormulaType.Pokemon => CalculatePokemon(attacker, defender, skill),
                DamageFormulaType.Custom => CalculateCustom(attacker, defender, skill),
                _ => CalculateSimple(attacker, defender, skill)
            };

            // Apply critical hit
            if (isCritical)
            {
                baseDamage = attacker.BattleStats.ApplyCriticalDamage(baseDamage);
            }

            // Apply element multiplier
            float elementMultiplier = defender.ElementAffinities?.GetDamageMultiplier(skill.Element) ?? 1f;
            baseDamage = Mathf.RoundToInt(baseDamage * elementMultiplier);

            // Minimum damage
            return Mathf.Max(1, baseDamage);
        }

        /// <summary>
        /// Simple: Attack - Defense/2
        /// Most basic formula, good for testing
        /// </summary>
        private static int CalculateSimple(CharacterStats attacker, CharacterStats defender, SkillData skill)
        {
            int attackStat = skill.DamageType == Combat.DamageType.Magical 
                ? attacker.MagicAttack 
                : attacker.Attack;
            
            int defenseStat = skill.DamageType == Combat.DamageType.Magical
                ? defender.MagicDefense
                : defender.Defense;

            int baseDamage = attackStat - (defenseStat / 2);
            return Mathf.RoundToInt(baseDamage * (skill.BasePower / 100f) * skill.PowerMultiplier);
        }

        /// <summary>
        /// Persona 4/5: (Atk² / Def) × Power × Multiplier
        /// High damage, defense matters less, power scaling is significant
        /// </summary>
        private static int CalculatePersona(CharacterStats attacker, CharacterStats defender, SkillData skill)
        {
            int attackStat = skill.DamageType == Combat.DamageType.Magical 
                ? attacker.MagicAttack 
                : attacker.Attack;
            
            int defenseStat = skill.DamageType == Combat.DamageType.Magical
                ? defender.MagicDefense
                : defender.Defense;

            // Prevent division by zero
            defenseStat = Mathf.Max(1, defenseStat);

            // (Atk² / Def) × Power/100 × Multiplier
            float damage = (attackStat * attackStat / (float)defenseStat) 
                         * (skill.BasePower / 100f) 
                         * skill.PowerMultiplier;

            return Mathf.RoundToInt(damage);
        }

        /// <summary>
        /// Final Fantasy 7: Complex formula with level scaling
        /// [(Atk + ((Atk + Lvl) / 32) × ((Atk × Lvl) / 32)) / 16] × [Power × (512 - Def) / 512]
        /// Defense reduces percentage of damage rather than flat amount
        /// </summary>
        private static int CalculateFF7(CharacterStats attacker, CharacterStats defender, SkillData skill)
        {
            int attackStat = skill.DamageType == Combat.DamageType.Magical 
                ? attacker.MagicAttack 
                : attacker.Attack;
            
            int defenseStat = skill.DamageType == Combat.DamageType.Magical
                ? defender.MagicDefense
                : defender.Defense;

            int level = attacker.Level;

            // Attack calculation with level scaling
            float attackCalc = attackStat + ((attackStat + level) / 32f) * ((attackStat * level) / 32f);
            attackCalc /= 16f;

            // Defense reduction (max 512 defense = 100% reduction)
            defenseStat = Mathf.Clamp(defenseStat, 0, 511);
            float defenseMultiplier = (512f - defenseStat) / 512f;

            // Final damage
            float damage = attackCalc * (skill.BasePower / 100f) * defenseMultiplier * skill.PowerMultiplier;

            return Mathf.RoundToInt(damage);
        }

        /// <summary>
        /// Final Fantasy 8: Atk - [(Def × (265 - Atk)) / 512]
        /// Defense effectiveness scales with attack difference
        /// </summary>
        private static int CalculateFF8(CharacterStats attacker, CharacterStats defender, SkillData skill)
        {
            int attackStat = skill.DamageType == Combat.DamageType.Magical 
                ? attacker.MagicAttack 
                : attacker.Attack;
            
            int defenseStat = skill.DamageType == Combat.DamageType.Magical
                ? defender.MagicDefense
                : defender.Defense;

            // FF8 formula
            float defenseReduction = (defenseStat * (265f - attackStat)) / 512f;
            float baseDamage = attackStat - defenseReduction;

            // Apply power and multiplier
            float damage = baseDamage * (skill.BasePower / 100f) * skill.PowerMultiplier;

            return Mathf.RoundToInt(Mathf.Max(0, damage));
        }

        /// <summary>
        /// Final Fantasy 9: Base × (Atk + Random(0-Atk/8)) - Def
        /// Has built-in variance, defense is flat reduction
        /// </summary>
        private static int CalculateFF9(CharacterStats attacker, CharacterStats defender, SkillData skill)
        {
            int attackStat = skill.DamageType == Combat.DamageType.Magical 
                ? attacker.MagicAttack 
                : attacker.Attack;
            
            int defenseStat = skill.DamageType == Combat.DamageType.Magical
                ? defender.MagicDefense
                : defender.Defense;

            // Random variance in attack (0 to Atk/8)
            int variance = GD.RandRange(0, attackStat / 8);
            int modifiedAttack = attackStat + variance;

            // Base × (Atk + Variance) - Def
            float baseDamage = (skill.BasePower / 100f) * modifiedAttack - defenseStat;
            float damage = baseDamage * skill.PowerMultiplier;

            return Mathf.RoundToInt(Mathf.Max(1, damage));
        }

        /// <summary>
        /// Pokemon: ((2 × Level / 5 + 2) × Power × Atk / Def / 50 + 2)
        /// Classic Pokemon damage formula
        /// </summary>
        private static int CalculatePokemon(CharacterStats attacker, CharacterStats defender, SkillData skill)
        {
            int attackStat = skill.DamageType == Combat.DamageType.Magical 
                ? attacker.MagicAttack 
                : attacker.Attack;
            
            int defenseStat = skill.DamageType == Combat.DamageType.Magical
                ? defender.MagicDefense
                : defender.Defense;

            // Prevent division by zero
            defenseStat = Mathf.Max(1, defenseStat);

            int level = attacker.Level;
            int power = skill.BasePower;

            // ((2 × Level / 5 + 2) × Power × Atk / Def / 50 + 2) × Multiplier
            float damage = ((2f * level / 5f + 2f) * power * attackStat / defenseStat / 50f + 2f);
            damage *= skill.PowerMultiplier;

            // Pokemon has random variance (85-100%)
            float variance = GD.Randf() * 0.15f + 0.85f; // 0.85 to 1.0
            damage *= variance;

            return Mathf.RoundToInt(damage);
        }

        /// <summary>
        /// Custom: Use skill's custom formula settings
        /// Falls back to simple if no custom settings
        /// </summary>
        private static int CalculateCustom(CharacterStats attacker, CharacterStats defender, SkillData skill)
        {
            // For now, same as simple - you can extend this later
            return CalculateSimple(attacker, defender, skill);
        }

        /// <summary>
        /// Get a description of how each formula works
        /// </summary>
        public static string GetFormulaDescription(DamageFormulaType formulaType)
        {
            return formulaType switch
            {
                DamageFormulaType.Simple => 
                    "Simple: Attack - Defense/2\nBasic formula, good for testing",
                
                DamageFormulaType.Persona => 
                    "Persona: (Atk² / Def) × Power\nHigh damage, power scaling is significant",
                
                DamageFormulaType.FinalFantasy7 => 
                    "FF7: Complex level-scaled formula\nDefense reduces damage percentage",
                
                DamageFormulaType.FinalFantasy8 => 
                    "FF8: Atk - [(Def × (265 - Atk)) / 512]\nDefense effectiveness scales with attack difference",
                
                DamageFormulaType.FinalFantasy9 => 
                    "FF9: Base × (Atk + Random) - Def\nHas built-in variance, flat defense reduction",
                
                DamageFormulaType.Pokemon => 
                    "Pokemon: ((2×Lvl/5+2) × Power × Atk/Def/50+2)\nClassic Pokemon formula with variance",
                
                DamageFormulaType.Custom => 
                    "Custom: Use skill-specific multipliers",
                
                _ => "Unknown formula"
            };
        }
    }
}