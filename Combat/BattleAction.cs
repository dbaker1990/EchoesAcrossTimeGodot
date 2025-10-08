using Godot;
using System.Collections.Generic;

namespace EchoesAcrossTime.Combat
{
    /// <summary>
    /// Represents an action in battle
    /// </summary>
    public enum BattleActionType
    {
        Attack,
        Skill,
        Item,
        Guard,
        Escape,
        AllOutAttack,
        LimitBreak,
        Pass,
        DUOLimit,
        Summon
    }
    
    /// <summary>
    /// Result of a battle action
    /// </summary>
    public class BattleActionResult
    {
        public bool Success { get; set; }
        public bool HitWeakness { get; set; }
        public bool WasCritical { get; set; }
        public bool Missed { get; set; }
        public bool WasResisted { get; set; }
        public bool WasAbsorbed { get; set; }
        public int DamageDealt { get; set; }
        public int HealingDone { get; set; }
        public List<StatusEffect> StatusesApplied { get; set; }
        public bool CausedKnockdown { get; set; }
        public string Message { get; set; }
        
        public BattleActionResult()
        {
            StatusesApplied = new List<StatusEffect>();
            Message = "";
        }
    }
    
    /// <summary>
    /// Represents a battle action to be executed
    /// </summary>
    public class BattleAction
    {
        public BattleMember Actor { get; set; }
        public BattleMember[] Targets { get; set; }
        public BattleActionType ActionType { get; set; }
        public SkillData Skill { get; set; }
        public LimitBreakData LimitBreak { get; set; }
        public BattleMember DuoPartner { get; set; } // For duo limit breaks
        public object ItemData { get; set; } // For item usage
        
        public BattleAction(BattleMember actor, BattleActionType actionType)
        {
            Actor = actor;
            ActionType = actionType;
            Targets = System.Array.Empty<BattleMember>();
        }
        
        public BattleAction WithTargets(params BattleMember[] targets)
        {
            Targets = targets;
            return this;
        }
        
        public BattleAction WithSkill(SkillData skill)
        {
            Skill = skill;
            ActionType = BattleActionType.Skill;
            return this;
        }
        
        public BattleAction WithLimitBreak(LimitBreakData limitBreak, BattleMember duoPartner = null)
        {
            LimitBreak = limitBreak;
            DuoPartner = duoPartner;
            ActionType = BattleActionType.LimitBreak;
            return this;
        }
        
        public bool IsValid()
        {
            if (Actor == null || !Actor.Stats.IsAlive) return false;
            
            switch (ActionType)
            {
                case BattleActionType.Skill:
                    return Skill != null && Skill.CanUse(Actor.Stats);
                    
                case BattleActionType.LimitBreak:
                    return LimitBreak != null && LimitBreak.CanUse(Actor);
                    
                case BattleActionType.Item:
                    return ItemData != null;
                    
                case BattleActionType.Escape:
                    return Actor.IsPlayerControlled; // Only players can escape
                    
                case BattleActionType.Attack:
                case BattleActionType.Guard:
                case BattleActionType.Pass:
                    return true;
                    
                case BattleActionType.AllOutAttack:
                    return Actor.CanAllOutAttack;
                    
                default:
                    return false;
            }
        }
        
        public override string ToString()
        {
            string action = ActionType switch
            {
                BattleActionType.Skill => Skill?.DisplayName ?? "Unknown Skill",
                BattleActionType.LimitBreak => LimitBreak?.DisplayName ?? "Unknown Limit Break",
                BattleActionType.Attack => "Attack",
                BattleActionType.Guard => "Guard",
                BattleActionType.AllOutAttack => "All-Out Attack",
                _ => ActionType.ToString()
            };
            
            string targetNames = Targets.Length > 0 
                ? string.Join(", ", System.Array.ConvertAll(Targets, t => t.Stats.CharacterName))
                : "None";
            
            return $"{Actor.Stats.CharacterName} -> {action} -> {targetNames}";
        }
        
        
    }
}