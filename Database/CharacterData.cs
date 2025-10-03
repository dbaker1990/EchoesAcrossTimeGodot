using Godot;
using System;
using EchoesAcrossTime.Combat;
using EchoesAcrossTime.Items;

namespace EchoesAcrossTime.Database
{
    public enum CharacterType
    {
        PlayableCharacter,
        Enemy,
        NPC,
        Boss
    }
    
    [GlobalClass]
    public partial class CharacterData : Resource
    {
        [ExportGroup("Basic Info")]
        [Export] public string CharacterId { get; set; } = "character_001";
        [Export] public string DisplayName { get; set; } = "Character";
        [Export(PropertyHint.MultilineText)] public string Description { get; set; } = "";
        [Export] public CharacterType Type { get; set; } = CharacterType.PlayableCharacter;
        [Export] public CharacterClass Class { get; set; } = CharacterClass.CourtMage;
        [Export] public bool IsBoss { get; set; } = false;
        
        [ExportGroup("Graphics")]
        [Export] public string PortraitPath { get; set; } = "";
        [Export] public string BattleSpritePath { get; set; } = "";
        [Export] public MenuGraphics Graphics { get; set; }
        
        [ExportGroup("Base Stats")]
        [Export] public int Level { get; set; } = 1;
        [Export] public int MaxHP { get; set; } = 100;
        [Export] public int MaxMP { get; set; } = 50;
        [Export] public int Attack { get; set; } = 10;
        [Export] public int Defense { get; set; } = 10;
        [Export] public int MagicAttack { get; set; } = 10;
        [Export] public int MagicDefense { get; set; } = 10;
        [Export] public int Speed { get; set; } = 10;
        
        [ExportGroup("Growth Rates")]
        [Export(PropertyHint.Range, "0,1,0.01")] public float HPGrowthRate { get; set; } = 0.05f;
        [Export(PropertyHint.Range, "0,1,0.01")] public float MPGrowthRate { get; set; } = 0.05f;
        [Export(PropertyHint.Range, "0,1,0.01")] public float AttackGrowthRate { get; set; } = 0.03f;
        [Export(PropertyHint.Range, "0,1,0.01")] public float DefenseGrowthRate { get; set; } = 0.03f;
        [Export(PropertyHint.Range, "0,1,0.01")] public float MagicAttackGrowthRate { get; set; } = 0.03f;
        [Export(PropertyHint.Range, "0,1,0.01")] public float MagicDefenseGrowthRate { get; set; } = 0.03f;
        [Export(PropertyHint.Range, "0,1,0.01")] public float SpeedGrowthRate { get; set; } = 0.02f;
        
        [ExportGroup("Combat System")]
        [Export] public ElementAffinityData ElementAffinities { get; set; }
        [Export] public ExperienceCurve ExpCurve { get; set; }
        [Export] public BattleStats BattleStats { get; set; }
        
        [ExportGroup("Battle Animations")]
        [Export] public BattleAnimationData BattleAnimations { get; set; }

        [ExportGroup("AI Behavior")]
        [Export] public AIPattern AIBehavior { get; set; }
        
        [ExportGroup("Enemy Rewards")]
        [Export] public EnemyRewards Rewards { get; set; }
        
        [ExportGroup("Skills")]
        [Export] public Godot.Collections.Array<SkillData> StartingSkills { get; set; }
        [Export] public Godot.Collections.Dictionary SkillsLearnedAtLevel { get; set; }
        
        public CharacterData()
        {
            ElementAffinities = new ElementAffinityData();
            ExpCurve = ExperienceCurve.CreateQuadraticCurve();
            Graphics = new MenuGraphics();
            BattleStats = new BattleStats();
            BattleAnimations = new BattleAnimationData();
            AIBehavior = new AIPattern();
            
            // Only create rewards if this is an enemy
            if (Type == CharacterType.Enemy || Type == CharacterType.Boss)
            {
                Rewards = new EnemyRewards { IsBoss = IsBoss };
            }
            
            StartingSkills = new Godot.Collections.Array<SkillData>();
            SkillsLearnedAtLevel = new Godot.Collections.Dictionary();
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
            
            if (this.BattleStats != null)
            {
                stats.BattleStats = this.BattleStats.Clone();
            }
            else
            {
                stats.BattleStats = new BattleStats();
            }
            
            // Initialize skills
            stats.Skills = new CharacterSkills(this.CharacterId);
            
            // Learn starting skills
            if (StartingSkills != null)
            {
                foreach (var skill in StartingSkills)
                {
                    if (skill != null)
                    {
                        stats.Skills.LearnSkill(skill);
                    }
                }
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