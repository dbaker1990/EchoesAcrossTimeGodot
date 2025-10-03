using Godot;
using System;

namespace EchoesAcrossTime.Combat
{
    /// <summary>
    /// Types of limit break effects
    /// </summary>
    public enum LimitBreakType
    {
        Offensive,      // Pure damage
        Support,        // Buffs/healing
        Debuff,         // Weaken enemies
        Utility,        // Special effects
        Hybrid          // Multiple effects
    }
    
    /// <summary>
    /// Data for a character's Limit Break ultimate ability
    /// </summary>
    [GlobalClass]
    public partial class LimitBreakData : Resource
    {
        [ExportGroup("Basic Info")]
        [Export] public string LimitBreakId { get; set; }
        [Export] public string DisplayName { get; set; }
        [Export(PropertyHint.MultilineText)] public string Description { get; set; }
        [Export] public string CharacterId { get; set; } // Who owns this limit break
        [Export] public LimitBreakType Type { get; set; } = LimitBreakType.Offensive;
        
        [ExportGroup("Damage")]
        [Export] public int BasePower { get; set; } = 500;
        [Export] public float PowerMultiplier { get; set; } = 4.0f; // Very powerful!
        [Export] public ElementType Element { get; set; } = ElementType.Physical;
        [Export] public bool HitsAllEnemies { get; set; } = false;
        [Export] public bool IgnoresDefense { get; set; } = false;
        [Export] public int CriticalBonus { get; set; } = 50; // High crit chance
        [Export] public int HitCount { get; set; } = 1; // Multi-hit attacks
        
        [ExportGroup("Special Effects")]
        [Export] public bool InstantKillBelow { get; set; } = false; // Kill if HP < threshold
        [Export] public float InstantKillThreshold { get; set; } = 0.2f; // 20% HP
        [Export] public bool DispelsBuffs { get; set; } = false;
        [Export] public bool DispelsDebuffs { get; set; } = false;
        [Export] public bool PiercesImmunity { get; set; } = false;
        
        [ExportGroup("Status Effects")]
        [Export] public Godot.Collections.Array<StatusEffect> InflictsStatuses { get; set; }
        [Export] public int StatusChance { get; set; } = 100;
        [Export] public int StatusDuration { get; set; } = 3;
        
        [ExportGroup("Support Effects")]
        [Export] public int HealAmount { get; set; } = 0;
        [Export] public float HealPercent { get; set; } = 0f; // 0-1
        [Export] public bool RevivesAllies { get; set; } = false;
        [Export] public bool FullHealParty { get; set; } = false;
        
        [ExportGroup("Buffs/Debuffs")]
        [Export] public int AttackBuff { get; set; } = 0; // % buff
        [Export] public int DefenseBuff { get; set; } = 0;
        [Export] public int SpeedBuff { get; set; } = 0;
        [Export] public bool BuffsEntireParty { get; set; } = false;
        [Export] public int BuffDuration { get; set; } = 3;
        
        [ExportGroup("Unique Mechanics")]
        [Export] public bool GrantsExtraTurn { get; set; } = false;
        [Export] public bool StopsTime { get; set; } = false; // Enemies skip turns
        [Export] public int TimeStopDuration { get; set; } = 1; // Turns
        [Export] public bool SummonsCopy { get; set; } = false; // Temporal clone
        [Export] public int CopyDuration { get; set; } = 3;
        
        [ExportGroup("Animation & VFX")]
        [Export] public string AnimationPath { get; set; }
        [Export] public AudioStream SoundEffect { get; set; }
        [Export] public float AnimationDuration { get; set; } = 2.5f;
        [Export] public Color VFXColor { get; set; } = Colors.White;
        [Export(PropertyHint.MultilineText)] public string FlavorText { get; set; }
        
        [ExportGroup("Duo Limit Break")]
        [Export] public bool IsDuoLimitBreak { get; set; } = false;
        [Export] public string RequiredPartnerId { get; set; } = "";
        [Export] public float DuoPowerBonus { get; set; } = 1.5f; // Extra multiplier
        
        public LimitBreakData()
        {
            InflictsStatuses = new Godot.Collections.Array<StatusEffect>();
        }
        
        /// <summary>
        /// Check if this limit break can be used
        /// </summary>
        public bool CanUse(BattleMember user)
        {
            if (user == null || !user.Stats.IsAlive) return false;
            if (!user.IsLimitBreakReady) return false;
            
            return true;
        }
        
        /// <summary>
        /// Check if duo requirement is met
        /// </summary>
        public bool IsDuoAvailable(BattleMember user, System.Collections.Generic.List<BattleMember> party)
        {
            if (!IsDuoLimitBreak) return false;
            
            var partner = party.Find(m => 
                m.Stats.CharacterName == RequiredPartnerId && 
                m.Stats.IsAlive && 
                m.IsLimitBreakReady
            );
            
            return partner != null;
        }
    }
}