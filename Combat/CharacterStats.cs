using Godot;
using System;

namespace EchoesAcrossTime.Combat
{
    /// <summary>
    /// Combat-related stats for characters (runtime instance)
    /// </summary>
    public partial class CharacterStats : GodotObject
    {
        public const int MAX_LEVEL = 100;
        public const int MIN_LEVEL = 1;
        
        public string CharacterName { get; set; } = "Character";
        
        private int level = 1;
        public int Level 
        { 
            get => level;
            set => level = Mathf.Clamp(value, MIN_LEVEL, ExperienceCurve?.MaxLevel ?? MAX_LEVEL);
        }
        
        public int MaxHP { get; set; } = 100;
        public int CurrentHP { get; set; } = 100;
        public int MaxMP { get; set; } = 50;
        public int CurrentMP { get; set; } = 50;
        public int Attack { get; set; } = 10;
        public int Defense { get; set; } = 10;
        public int MagicAttack { get; set; } = 10;
        public int MagicDefense { get; set; } = 10;
        public int Speed { get; set; } = 10;
        
        // Experience system
        public int CurrentExp { get; set; } = 0;
        public int ExpToNextLevel { get; set; } = 100;
        
        // Growth rates
        public float HPGrowthRate { get; set; } = 0.05f;
        public float MPGrowthRate { get; set; } = 0.05f;
        public float AttackGrowthRate { get; set; } = 0.03f;
        public float DefenseGrowthRate { get; set; } = 0.03f;
        public float MagicAttackGrowthRate { get; set; } = 0.03f;
        public float MagicDefenseGrowthRate { get; set; } = 0.03f;
        public float SpeedGrowthRate { get; set; } = 0.02f;
        
        // Experience curve
        public ExperienceCurve ExperienceCurve { get; set; }
        
        public ElementAffinityData ElementAffinities { get; set; }
        
        [Signal]
        public delegate void HPChangedEventHandler(int oldHP, int newHP, int maxHP);
        
        [Signal]
        public delegate void MPChangedEventHandler(int oldMP, int newMP, int maxMP);
        
        [Signal]
        public delegate void DeathEventHandler();
        
        [Signal]
        public delegate void LevelUpEventHandler(int newLevel);
        
        [Signal]
        public delegate void ExpGainedEventHandler(int expGained, int currentExp, int expToNext);
        
        public CharacterStats()
        {
            ElementAffinities = new ElementAffinityData();
            ExperienceCurve = ExperienceCurve.CreateQuadraticCurve(); // Default curve
        }
        
        public bool IsAlive => CurrentHP > 0;
        public bool IsMaxLevel => Level >= (ExperienceCurve?.MaxLevel ?? MAX_LEVEL);
        public float HPPercent => MaxHP > 0 ? (float)CurrentHP / MaxHP : 0f;
        public float MPPercent => MaxMP > 0 ? (float)CurrentMP / MaxMP : 0f;
        public float ExpPercent => ExpToNextLevel > 0 ? (float)CurrentExp / ExpToNextLevel : 0f;
        
        /// <summary>
        /// Apply damage with element consideration
        /// </summary>
        public int TakeDamage(int baseDamage, ElementType element = ElementType.Physical)
        {
            if (!IsAlive) return 0;
            
            float multiplier = ElementAffinities?.GetDamageMultiplier(element) ?? 1f;
            int finalDamage = Mathf.RoundToInt(baseDamage * multiplier);
            
            if (multiplier < 0)
            {
                return Heal(Mathf.Abs(finalDamage));
            }
            
            int oldHP = CurrentHP;
            CurrentHP = Mathf.Max(0, CurrentHP - finalDamage);
            
            EmitSignal(SignalName.HPChanged, oldHP, CurrentHP, MaxHP);
            
            if (CurrentHP <= 0)
            {
                EmitSignal(SignalName.Death);
            }
            
            return finalDamage;
        }
        
        public int Heal(int amount)
        {
            if (!IsAlive) return 0;
            
            int oldHP = CurrentHP;
            CurrentHP = Mathf.Min(MaxHP, CurrentHP + amount);
            int actualHealing = CurrentHP - oldHP;
            
            EmitSignal(SignalName.HPChanged, oldHP, CurrentHP, MaxHP);
            
            return actualHealing;
        }
        
        public int RestoreMP(int amount)
        {
            int oldMP = CurrentMP;
            CurrentMP = Mathf.Min(MaxMP, CurrentMP + amount);
            int actualRestore = CurrentMP - oldMP;
            
            EmitSignal(SignalName.MPChanged, oldMP, CurrentMP, MaxMP);
            
            return actualRestore;
        }
        
        public bool ConsumeMP(int amount)
        {
            if (CurrentMP < amount) return false;
            
            int oldMP = CurrentMP;
            CurrentMP -= amount;
            
            EmitSignal(SignalName.MPChanged, oldMP, CurrentMP, MaxMP);
            
            return true;
        }
        
        public void FullRestore()
        {
            Heal(MaxHP);
            RestoreMP(MaxMP);
        }
        
        /// <summary>
        /// Add experience points and handle level up
        /// </summary>
        public bool AddExp(int exp)
        {
            if (IsMaxLevel)
            {
                GD.Print($"{CharacterName} is already at max level ({ExperienceCurve?.MaxLevel ?? MAX_LEVEL})");
                return false;
            }
            
            CurrentExp += exp;
            EmitSignal(SignalName.ExpGained, exp, CurrentExp, ExpToNextLevel);
            
            bool leveledUp = false;
            
            while (CurrentExp >= ExpToNextLevel && !IsMaxLevel)
            {
                PerformLevelUp();
                leveledUp = true;
            }
            
            return leveledUp;
        }
        
        /// <summary>
        /// Perform the level up process
        /// </summary>
        private void PerformLevelUp()
        {
            if (IsMaxLevel) return;
            
            int oldLevel = Level;
            Level++;
            
            // Carry over excess exp
            CurrentExp -= ExpToNextLevel;
            
            // Calculate new exp requirement using curve
            ExpToNextLevel = CalculateExpForNextLevel(Level);
            
            // Increase stats
            IncreaseStatsOnLevelUp();
            
            // Heal to full on level up
            FullRestore();
            
            EmitSignal(SignalName.LevelUp, Level);
            
            GD.Print($"{CharacterName} leveled up! Level {oldLevel} -> {Level}");
        }
        
        /// <summary>
        /// Calculate experience required for next level using the experience curve
        /// </summary>
        private int CalculateExpForNextLevel(int currentLevel)
        {
            if (ExperienceCurve == null)
            {
                // Fallback formula if no curve is set
                return Mathf.RoundToInt(100f * Mathf.Pow(currentLevel, 1.5f));
            }
            
            return ExperienceCurve.GetExpForLevelUp(currentLevel);
        }
        
        /// <summary>
        /// Increase stats on level up
        /// </summary>
        private void IncreaseStatsOnLevelUp()
        {
            int hpGrowth = Mathf.Max(1, Mathf.RoundToInt(MaxHP * HPGrowthRate) + 5);
            int mpGrowth = Mathf.Max(1, Mathf.RoundToInt(MaxMP * MPGrowthRate) + 3);
            int attackGrowth = Mathf.Max(1, Mathf.RoundToInt(Attack * AttackGrowthRate) + 1);
            int defenseGrowth = Mathf.Max(1, Mathf.RoundToInt(Defense * DefenseGrowthRate) + 1);
            int magicAttackGrowth = Mathf.Max(1, Mathf.RoundToInt(MagicAttack * MagicAttackGrowthRate) + 1);
            int magicDefenseGrowth = Mathf.Max(1, Mathf.RoundToInt(MagicDefense * MagicDefenseGrowthRate) + 1);
            int speedGrowth = Mathf.Max(1, Mathf.RoundToInt(Speed * SpeedGrowthRate));
            
            MaxHP += hpGrowth;
            MaxMP += mpGrowth;
            Attack += attackGrowth;
            Defense += defenseGrowth;
            MagicAttack += magicAttackGrowth;
            MagicDefense += magicDefenseGrowth;
            Speed += speedGrowth;
            
            GD.Print($"Stats increased - HP+{hpGrowth}, MP+{mpGrowth}, ATK+{attackGrowth}, DEF+{defenseGrowth}, MATK+{magicAttackGrowth}, MDEF+{magicDefenseGrowth}, SPD+{speedGrowth}");
        }
        
        /// <summary>
        /// Set character to specific level with appropriate stats
        /// </summary>
        public void SetLevel(int targetLevel, bool scaleStats = true)
        {
            targetLevel = Mathf.Clamp(targetLevel, MIN_LEVEL, ExperienceCurve?.MaxLevel ?? MAX_LEVEL);
            
            if (!scaleStats)
            {
                Level = targetLevel;
                CurrentExp = 0;
                ExpToNextLevel = CalculateExpForNextLevel(targetLevel);
                return;
            }
            
            while (Level < targetLevel)
            {
                Level++;
                IncreaseStatsOnLevelUp();
            }
            
            CurrentExp = 0;
            ExpToNextLevel = CalculateExpForNextLevel(Level);
            FullRestore();
        }
        
        /// <summary>
        /// Get total experience needed to reach a specific level
        /// </summary>
        public int GetTotalExpForLevel(int targetLevel)
        {
            if (ExperienceCurve == null)
            {
                return 0;
            }
            
            return ExperienceCurve.GetTotalExpForLevel(targetLevel);
        }
        
        public int CalculatePhysicalDamage(int targetDefense)
        {
            return Mathf.Max(1, Attack - targetDefense / 2);
        }
        
        public int CalculateMagicalDamage(int targetMagicDefense, float powerMultiplier = 1f)
        {
            return Mathf.Max(1, Mathf.RoundToInt((MagicAttack * powerMultiplier) - targetMagicDefense / 2));
        }
    }
}