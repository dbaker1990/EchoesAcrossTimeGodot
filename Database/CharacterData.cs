// Database/CharacterData.cs
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
        
        [ExportGroup("Visual Assets")]
        [ExportSubgroup("Overworld")]
        [Export] public SpriteFrames OverworldSpriteFrames { get; set; }
        [Export] public Texture2D OverworldShadow { get; set; }
        [Export] public Vector2 OverworldSpriteOffset { get; set; } = Vector2.Zero;
        [Export] public float OverworldScale { get; set; } = 1.0f;
        
        [ExportSubgroup("Battle")]
        [Export] public SpriteFrames BattleSpriteFrames { get; set; }
        [Export] public Texture2D BattlePortrait { get; set; }
        [Export] public Vector2 BattleSpriteOffset { get; set; } = Vector2.Zero;
        [Export] public float BattleScale { get; set; } = 1.0f;
        
        [ExportSubgroup("UI")]
        [Export] public Texture2D MenuPortrait { get; set; }
        [Export] public Texture2D IconSmall { get; set; }
        [Export] public Texture2D FacePortrait { get; set; }
        
        [ExportGroup("Legacy Graphics (Deprecated - Use Visual Assets instead)")]
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
        
        [ExportGroup("Character Scenes (Optional)")]
        [Export] public PackedScene CustomOverworldScene { get; set; }
        [Export] public PackedScene CustomFollowerScene { get; set; }
        [Export] public PackedScene CustomBattleScene { get; set; }
        
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
        
        /// <summary>
        /// Create an overworld character instance from this data
        /// </summary>
        public OverworldCharacter CreateOverworldInstance(PackedScene characterScene = null)
        {
            OverworldCharacter character;
            
            // Use custom scene if provided
            if (CustomOverworldScene != null)
            {
                character = CustomOverworldScene.Instantiate<OverworldCharacter>();
            }
            else if (characterScene != null)
            {
                character = characterScene.Instantiate<OverworldCharacter>();
            }
            else
            {
                // Use default character scene
                var defaultScene = GD.Load<PackedScene>("res://Characters/GenericCharacter.tscn");
                character = defaultScene.Instantiate<OverworldCharacter>();
            }
            
            // Apply visual data
            if (character.AnimatedSprite != null && OverworldSpriteFrames != null)
            {
                character.AnimatedSprite.SpriteFrames = OverworldSpriteFrames;
                character.AnimatedSprite.Position = OverworldSpriteOffset;
                character.AnimatedSprite.Scale = new Vector2(OverworldScale, OverworldScale);
            }
            
            if (character.ShadowSprite != null && OverworldShadow != null)
            {
                character.ShadowSprite.Texture = OverworldShadow;
            }
            
            // Apply character data
            character.CharacterName = DisplayName;
            character.CharacterData = this;
            
            // Create stats instance
            if (Type == CharacterType.PlayableCharacter || Type == CharacterType.NPC)
            {
                character.Stats = CreateStatsInstance();
            }
            
            GD.Print($"Created overworld instance for {DisplayName}");
            return character;
        }
        
        /// <summary>
        /// Create follower instance
        /// </summary>
        public FollowerCharacter CreateFollowerInstance(PackedScene followerScene = null)
        {
            FollowerCharacter follower;
            
            // Use custom scene if provided
            if (CustomFollowerScene != null)
            {
                follower = CustomFollowerScene.Instantiate<FollowerCharacter>();
            }
            else if (followerScene != null)
            {
                follower = followerScene.Instantiate<FollowerCharacter>();
            }
            else
            {
                var defaultScene = GD.Load<PackedScene>("res://Characters/GenericFollower.tscn");
                follower = defaultScene.Instantiate<FollowerCharacter>();
            }
            
            // Apply visual data
            if (follower.AnimatedSprite != null && OverworldSpriteFrames != null)
            {
                follower.AnimatedSprite.SpriteFrames = OverworldSpriteFrames;
                follower.AnimatedSprite.Position = OverworldSpriteOffset;
                follower.AnimatedSprite.Scale = new Vector2(OverworldScale, OverworldScale);
            }
            
            if (follower.ShadowSprite != null && OverworldShadow != null)
            {
                follower.ShadowSprite.Texture = OverworldShadow;
            }
            
            follower.CharacterName = DisplayName;
            follower.CharacterData = this;
            follower.Stats = CreateStatsInstance();
            
            GD.Print($"Created follower instance for {DisplayName}");
            return follower;
        }
        
        /// <summary>
        /// Create a battle character instance (for future battle system)
        /// </summary>
        public Node CreateBattleInstance(PackedScene battleScene = null)
        {
            Node battleCharacter;
            
            // Use custom scene if provided
            if (CustomBattleScene != null)
            {
                battleCharacter = CustomBattleScene.Instantiate();
            }
            else if (battleScene != null)
            {
                battleCharacter = battleScene.Instantiate();
            }
            else
            {
                GD.PrintErr($"CharacterData: No battle scene provided for {DisplayName}");
                return null;
            }
            
            // Apply battle visual data through reflection or direct assignment
            // This will be implemented when you create your battle system
            
            GD.Print($"Created battle instance for {DisplayName}");
            return battleCharacter;
        }
        
        /// <summary>
        /// Create a stats instance from this character data
        /// </summary>
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
        
        /// <summary>
        /// Check if this character has overworld sprites assigned
        /// </summary>
        public bool HasOverworldSprites()
        {
            return OverworldSpriteFrames != null;
        }
        
        /// <summary>
        /// Check if this character has battle sprites assigned
        /// </summary>
        public bool HasBattleSprites()
        {
            return BattleSpriteFrames != null;
        }
        
        /// <summary>
        /// Get the appropriate portrait based on context
        /// </summary>
        public Texture2D GetPortrait(PortraitType type = PortraitType.Menu)
        {
            return type switch
            {
                PortraitType.Menu => MenuPortrait ?? FacePortrait ?? BattlePortrait,
                PortraitType.Battle => BattlePortrait ?? MenuPortrait ?? FacePortrait,
                PortraitType.Dialogue => FacePortrait ?? MenuPortrait ?? BattlePortrait,
                PortraitType.Icon => IconSmall ?? MenuPortrait,
                _ => MenuPortrait
            };
        }
    }
    
    public enum PortraitType
    {
        Menu,
        Battle,
        Dialogue,
        Icon
    }
}