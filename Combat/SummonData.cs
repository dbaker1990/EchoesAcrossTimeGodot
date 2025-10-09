// Combat/SummonData.cs
using Godot;
using System;
using System.Collections.Generic;
using EchoesAcrossTime.Database;

namespace EchoesAcrossTime.Combat
{
    public enum SummonType
    {
        Offensive,      // Attack-focused summons
        Defensive,      // Tank/protect summons  
        Support,        // Buff/heal summons
        Debuffer,       // Status/debuff summons
        Elemental,      // Pure elemental damage
        Special         // Unique mechanics
    }

    public enum SummonDuration
    {
        Instant,        // One-time effect then disappears
        ShortTerm,      // 2-3 turns
        MediumTerm,     // 4-6 turns
        LongTerm,       // 7-10 turns
        Permanent       // Until defeated or battle ends
    }

    /// <summary>
    /// Defines a summonable creature for the summoner character
    /// </summary>
    [GlobalClass]
    public partial class SummonData : Resource
    {
        [ExportGroup("Basic Info")]
        [Export] public string SummonId { get; set; } = "summon_001";
        [Export] public string DisplayName { get; set; } = "Summon";
        [Export(PropertyHint.MultilineText)] public string Description { get; set; } = "";
        [Export] public Texture2D Icon { get; set; }
        [Export] public Texture2D BattleSprite { get; set; }
        [Export] public SummonType Type { get; set; } = SummonType.Offensive;
        
        [ExportGroup("Summoning Costs")]
        [Export] public int MPCost { get; set; } = 30;
        [Export] public int HPCost { get; set; } = 0;
        [Export] public int RequiredLevel { get; set; } = 1;
        [Export] public int Cooldown { get; set; } = 0; // Turns before can resummon
        
        [ExportGroup("Summon Stats")]
        [Export] public int MaxHP { get; set; } = 100;
        [Export] public int Attack { get; set; } = 10;
        [Export] public int Defense { get; set; } = 10;
        [Export] public int MagicAttack { get; set; } = 10;
        [Export] public int MagicDefense { get; set; } = 10;
        [Export] public int Speed { get; set; } = 10;
        
        [ExportGroup("Duration")]
        [Export] public SummonDuration DurationType { get; set; } = SummonDuration.MediumTerm;
        [Export] public int TurnDuration { get; set; } = 5; // For non-instant summons
        [Export] public bool DiesWhenSummonerDies { get; set; } = true;
        
        [ExportGroup("Element Affinity")]
        [Export] public ElementType PrimaryElement { get; set; } = ElementType.Physical;
        [Export] public ElementAffinityData ElementAffinities { get; set; }
        
        [ExportGroup("Abilities")]
        [Export] public Godot.Collections.Array<string> SkillIds { get; set; } // Skills the summon can use
        [Export] public string SignatureSkillId { get; set; } // Special skill only this summon has
        
        [ExportGroup("Instant Effects")]
        // For instant summons that don't stay on field
        [Export] public int InstantDamage { get; set; } = 0;
        [Export] public int InstantHealing { get; set; } = 0;
        [Export] public SkillTarget InstantTarget { get; set; } = SkillTarget.SingleEnemy;
        [Export] public Godot.Collections.Array<StatusEffect> InstantStatuses { get; set; }
        
        [ExportGroup("Passive Effects")]
        // Effects while summon is active
        [Export] public int PassiveHealPerTurn { get; set; } = 0;
        [Export] public int PassiveDamagePerTurn { get; set; } = 0;
        [Export] public int AttackBuffToParty { get; set; } = 0;
        [Export] public int DefenseBuffToParty { get; set; } = 0;
        [Export] public int MagicBuffToParty { get; set; } = 0;
        [Export] public int SpeedBuffToParty { get; set; } = 0;
        
        [ExportGroup("Special Mechanics")]
        [Export] public bool CanBeTargeted { get; set; } = true; // False = can't be attacked
        [Export] public bool CountsAsPartyMember { get; set; } = true; // Takes up party slot
        [Export] public bool CanBatonPass { get; set; } = false; // Can receive baton pass
        [Export] public bool ExplodesOnDeath { get; set; } = false;
        [Export] public int ExplosionDamage { get; set; } = 0;
        [Export] public bool TauntEnemies { get; set; } = false; // Draws enemy attacks
        [Export] public float TauntChance { get; set; } = 0.5f;
        
        [ExportGroup("AI Behavior")]
        [Export] public AIPattern AIBehavior { get; set; }
        
        [ExportGroup("Visual/Audio")]
        [Export] public string SummonAnimationName { get; set; } = "summon_appear";
        [Export] public string IdleAnimationName { get; set; } = "idle";
        [Export] public AudioStream SummonSound { get; set; }
        [Export] public AudioStream DismissSound { get; set; }
        
        [ExportGroup("Lore")]
        [Export(PropertyHint.MultilineText)] public string Lore { get; set; } = "";
        [Export] public string SummonerBondLevel { get; set; } = "1"; // Required bond with summoner

        public SummonData()
        {
            ElementAffinities = new ElementAffinityData();
            SkillIds = new Godot.Collections.Array<string>();
            InstantStatuses = new Godot.Collections.Array<StatusEffect>();
            AIBehavior = new AIPattern();
        }

        /// <summary>
        /// Check if summoner can summon this creature
        /// </summary>
        public bool CanSummon(CharacterStats summoner)
        {
            if (summoner == null) return false;
            
            // Check MP
            if (summoner.CurrentMP < MPCost) return false;
            
            // Check HP (some summons require blood sacrifice)
            if (summoner.CurrentHP <= HPCost) return false;
            
            // Check level
            if (summoner.Level < RequiredLevel) return false;
            
            return true;
        }

        /// <summary>
        /// Create a CharacterStats instance for this summon
        /// </summary>
        public CharacterStats CreateSummonStats(int summonerLevel)
        {
            var stats = new CharacterStats
            {
                CharacterName = DisplayName,
                Level = summonerLevel, // Scale with summoner
                MaxHP = MaxHP + (summonerLevel * 5), // Scale HP with level
                CurrentHP = MaxHP + (summonerLevel * 5),
                MaxMP = 0, // Summons typically don't have MP
                CurrentMP = 0,
                Attack = Attack + (summonerLevel * 2),
                Defense = Defense + (summonerLevel * 2),
                MagicAttack = MagicAttack + (summonerLevel * 2),
                MagicDefense = MagicDefense + (summonerLevel * 2),
                Speed = Speed + summonerLevel,
                ElementAffinities = ElementAffinities
            };

            // Set up AI if provided
            if (AIBehavior != null)
            {
                stats.AIPattern = AIBehavior;
            }

            return stats;
        }

        /// <summary>
        /// Get the skills this summon can use
        /// </summary>
        public List<SkillData> GetSummonSkills(GameDatabase database)
        {
            var skills = new List<SkillData>();
            
            if (database == null) return skills;

            foreach (var skillId in SkillIds)
            {
                var skill = database.GetSkill(skillId);
                if (skill != null)
                {
                    skills.Add(skill);
                }
            }

            // Add signature skill
            if (!string.IsNullOrEmpty(SignatureSkillId))
            {
                var sigSkill = database.GetSkill(SignatureSkillId);
                if (sigSkill != null && !skills.Contains(sigSkill))
                {
                    skills.Add(sigSkill);
                }
            }

            return skills;
        }

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(SummonId) && 
                   !string.IsNullOrEmpty(DisplayName);
        }
    }
}