using Godot;
using System;

namespace EchoesAcrossTime.Combat
{
    /// <summary>
    /// Represents a battle participant with Persona 5 mechanics
    /// Wraps CharacterStats with battle-specific state
    /// </summary>
    public class BattleMember
    {
        public CharacterStats Stats { get; set; }
        public bool IsPlayerControlled { get; set; }
        public int BattlePosition { get; set; }
        
        // Persona 5 specific states
        public bool IsKnockedDown { get; private set; }
        public bool HasExtraTurn { get; set; }
        public bool HasActedThisTurn { get; set; }
        public bool CanAllOutAttack { get; set; }
        
        // Turn tracking
        public int TurnsTakenThisBattle { get; set; }
        public int LastActionTurn { get; set; }
        
        // One More tracking
        public int OneMoreCount { get; set; } // How many One Mores this turn
        public bool UsedOncePerTurnSkill { get; set; }
        
        // Baton Pass tracking
        public BatonPassData BatonPassData { get; set; }
        
        // Limit Break tracking
        public int LimitGauge { get; set; }
        public bool IsLimitBreakReady { get; set; }
        
        // Guard state
        public GuardState GuardState { get; set; }
        
        public BattleMember(CharacterStats stats, bool isPlayerControlled, int position)
        {
            Stats = stats ?? throw new ArgumentNullException(nameof(stats));
            IsPlayerControlled = isPlayerControlled;
            BattlePosition = position;
            BatonPassData = new BatonPassData();
            GuardState = new GuardState();
            Reset();
        }
        
        /// <summary>
        /// Reset all battle-specific state
        /// </summary>
        public void Reset()
        {
            IsKnockedDown = false;
            HasExtraTurn = false;
            HasActedThisTurn = false;
            CanAllOutAttack = false;
            TurnsTakenThisBattle = 0;
            LastActionTurn = 0;
            OneMoreCount = 0;
            UsedOncePerTurnSkill = false;
            LimitGauge = 0;
            IsLimitBreakReady = false;
        }
        
        /// <summary>
        /// Knock down this member (hit weakness or critical)
        /// </summary>
        public void KnockDown()
        {
            if (!Stats.IsAlive) return;
            
            IsKnockedDown = true;
            HasExtraTurn = true;
            CanAllOutAttack = true;
            
            GD.Print($"{Stats.CharacterName} was knocked down!");
        }
        
        /// <summary>
        /// Stand up from knocked down state
        /// </summary>
        public void StandUp()
        {
            IsKnockedDown = false;
            GD.Print($"{Stats.CharacterName} stands back up!");
        }
        
        /// <summary>
        /// End this member's turn
        /// </summary>
        public void EndTurn()
        {
            HasActedThisTurn = true;
            HasExtraTurn = false;
            UsedOncePerTurnSkill = false;
            TurnsTakenThisBattle++;
        }
        
        /// <summary>
        /// Start a new round for this member
        /// </summary>
        public void StartRound()
        {
            HasActedThisTurn = false;
            HasExtraTurn = false;
            OneMoreCount = 0;
            UsedOncePerTurnSkill = false;
            BatonPassData.Reset(); // Reset baton pass bonuses each round
            
            // Knocked down enemies stand up at start of their turn
            if (IsKnockedDown && !IsPlayerControlled)
            {
                StandUp();
            }
        }
        
        /// <summary>
        /// Check if this member can act
        /// </summary>
        public bool CanAct()
        {
            if (!Stats.IsAlive) return false;
            if (HasActedThisTurn && !HasExtraTurn) return false;
            
            // Check for status effects that prevent action
            foreach (var status in Stats.ActiveStatuses)
            {
                if (status.Effect == StatusEffect.Sleep ||
                    status.Effect == StatusEffect.Stun ||
                    status.Effect == StatusEffect.Freeze ||
                    status.Effect == StatusEffect.Petrify)
                {
                    return false;
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// Get a display state for UI
        /// </summary>
        public string GetStateDisplay()
        {
            if (!Stats.IsAlive) return "DOWNED";
            if (IsKnockedDown) return "KNOCKED DOWN";
            if (HasExtraTurn) return "ONE MORE!";
            if (HasActedThisTurn) return "ACTED";
            return "READY";
        }
        
        public override string ToString()
        {
            return $"{Stats.CharacterName} [{GetStateDisplay()}] HP:{Stats.CurrentHP}/{Stats.MaxHP}";
        }
    }
}