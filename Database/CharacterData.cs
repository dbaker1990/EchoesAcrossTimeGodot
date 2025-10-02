using Godot;
using System;
using EchoesAcrossTime.Combat;

namespace EchoesAcrossTime.Database
{
    public enum CharacterType
    {
        PlayableCharacter,
        Enemy,
        NPC,
        Boss
    }
    
    /// <summary>
    /// Simple character data class (not a Resource)
    /// </summary>
    public partial class CharacterData
    {
        public string CharacterId { get; set; } = "character_001";
        public string DisplayName { get; set; } = "Character";
        public string PortraitPath { get; set; } = "";
        public string BattleSpritePath { get; set; } = "";
        
        public int Level { get; set; } = 1;
        public int MaxHP { get; set; } = 100;
        public int MaxMP { get; set; } = 50;
        public int Attack { get; set; } = 10;
        public int Defense { get; set; } = 10;
        public int MagicAttack { get; set; } = 10;
        public int MagicDefense { get; set; } = 10;
        public int Speed { get; set; } = 10;
        
        public float HPGrowthRate { get; set; } = 0.05f;
        public float MPGrowthRate { get; set; } = 0.05f;
        public float AttackGrowthRate { get; set; } = 0.03f;
        public float DefenseGrowthRate { get; set; } = 0.03f;
        public float MagicAttackGrowthRate { get; set; } = 0.03f;
        public float MagicDefenseGrowthRate { get; set; } = 0.03f;
        public float SpeedGrowthRate { get; set; } = 0.02f;
        
        public ElementAffinityData ElementAffinities { get; set; }
        public ExperienceCurve ExpCurve { get; set; }
        
        public CharacterType Type { get; set; } = CharacterType.PlayableCharacter;
        public bool IsBoss { get; set; } = false;
        
        public string Description { get; set; } = "";
        
        public CharacterData()
        {
            ElementAffinities = new ElementAffinityData();
            ExpCurve = ExperienceCurve.CreateQuadraticCurve();
        }
        
        public CharacterStats CreateStatsInstance()
        {
            int validLevel = Mathf.Clamp(Level, CharacterStats.MIN_LEVEL, CharacterStats.MAX_LEVEL);
            
            var stats = new CharacterStats
            {
                CharacterName = this.DisplayName,
                Level = validLevel,
                MaxHP = this.MaxHP,
                CurrentHP = this.MaxHP,
                MaxMP = this.MaxMP,
                CurrentMP = this.MaxMP,
                Attack = this.Attack,
                Defense = this.Defense,
                MagicAttack = this.MagicAttack,
                MagicDefense = this.MagicDefense,
                Speed = this.Speed,
                CurrentExp = 0,
                HPGrowthRate = this.HPGrowthRate,
                MPGrowthRate = this.MPGrowthRate,
                AttackGrowthRate = this.AttackGrowthRate,
                DefenseGrowthRate = this.DefenseGrowthRate,
                MagicAttackGrowthRate = this.MagicAttackGrowthRate,
                MagicDefenseGrowthRate = this.MagicDefenseGrowthRate,
                SpeedGrowthRate = this.SpeedGrowthRate
            };
            
            if (ExpCurve != null)
            {
                stats.ExperienceCurve = ExpCurve;
            }
            else
            {
                stats.ExperienceCurve = ExperienceCurve.GetRecommendedCurve(Type);
            }
            
            stats.ExpToNextLevel = stats.ExperienceCurve.GetExpForLevelUp(validLevel);
            
            if (this.ElementAffinities != null)
            {
                stats.ElementAffinities = this.ElementAffinities.DuplicateData();
            }
            else
            {
                stats.ElementAffinities = new ElementAffinityData();
            }
            
            return stats;
        }
        
        public CharacterStats CreateStatsInstanceAtLevel(int targetLevel)
        {
            var stats = CreateStatsInstance();
            stats.SetLevel(targetLevel, scaleStats: true);
            return stats;
        }
        
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(CharacterId) && 
                   !string.IsNullOrEmpty(DisplayName) && 
                   MaxHP > 0;
        }
    }
}