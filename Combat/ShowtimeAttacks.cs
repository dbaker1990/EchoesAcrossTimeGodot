using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EchoesAcrossTime.Combat
{
    /// <summary>
    /// Showtime attack data - special duo attacks between character pairs
    /// Based on Persona 5 Royal's Showtime system
    /// </summary>
    [GlobalClass]
    public partial class ShowtimeAttackData : Resource
    {
        [ExportGroup("Character Pair")]
        [Export] public string Character1Id { get; set; }
        [Export] public string Character2Id { get; set; }
        
        [ExportGroup("Attack Info")]
        [Export] public string AttackName { get; set; }
        [Export] public string Description { get; set; }
        [Export(PropertyHint.MultilineText)] public string FlavorText { get; set; }
        
        [ExportGroup("Damage")]
        [Export] public int BasePower { get; set; } = 500;
        [Export] public ElementType Element { get; set; } = ElementType.Physical;
        [Export] public bool HitsAllEnemies { get; set; } = true;
        [Export] public float DamageMultiplier { get; set; } = 3.0f; // Very powerful!
        [Export] public bool IgnoresDefense { get; set; } = false;
        [Export] public int CriticalChance { get; set; } = 50; // High crit chance
        
        [ExportGroup("Effects")]
        [Export] public Godot.Collections.Array<StatusEffect> InflictsStatuses { get; set; }
        [Export] public int StatusChance { get; set; } = 80;
        [Export] public bool InstantKillChance { get; set; } = false;
        [Export] public int InstantKillRate { get; set; } = 10;
        
        [ExportGroup("Trigger Conditions")]
        [Export(PropertyHint.Range, "0,100")] public int TriggerChance { get; set; } = 15; // % per turn
        [Export] public int CooldownTurns { get; set; } = 3;
        [Export] public bool RequiresBothAlive { get; set; } = true;
        [Export] public float MinHPPercent { get; set; } = 0.25f; // Can trigger at low HP
        
        [ExportGroup("Animation")]
        [Export] public string AnimationPath { get; set; }
        [Export] public AudioStream SoundEffect { get; set; }
        [Export] public float AnimationDuration { get; set; } = 3.0f;
        
        public ShowtimeAttackData()
        {
            InflictsStatuses = new Godot.Collections.Array<StatusEffect>();
        }
        
        /// <summary>
        /// Check if character pair matches this showtime
        /// </summary>
        public bool IsValidPair(string char1, string char2)
        {
            return (Character1Id == char1 && Character2Id == char2) ||
                   (Character1Id == char2 && Character2Id == char1);
        }
    }
    
    /// <summary>
    /// Tracks showtime attack state during battle
    /// </summary>
    public class ShowtimeState
    {
        public ShowtimeAttackData AttackData { get; set; }
        public int TurnsSinceLastUse { get; set; }
        public bool IsAvailable { get; set; }
        public int TimesUsedThisBattle { get; set; }
        
        public ShowtimeState(ShowtimeAttackData data)
        {
            AttackData = data;
            TurnsSinceLastUse = data.CooldownTurns; // Start ready
            IsAvailable = false;
            TimesUsedThisBattle = 0;
        }
        
        public void UseShowtime()
        {
            TurnsSinceLastUse = 0;
            IsAvailable = false;
            TimesUsedThisBattle++;
        }
        
        public void IncrementTurn()
        {
            TurnsSinceLastUse++;
            if (TurnsSinceLastUse >= AttackData.CooldownTurns)
            {
                IsAvailable = true;
            }
        }
    }
    
    /// <summary>
    /// Manages showtime attacks during battle
    /// </summary>
    public class ShowtimeManager
    {
        private List<ShowtimeAttackData> availableShowtimes;
        private Dictionary<string, ShowtimeState> showtimeStates;
        private RandomNumberGenerator rng;
        
        public ShowtimeManager()
        {
            availableShowtimes = new List<ShowtimeAttackData>();
            showtimeStates = new Dictionary<string, ShowtimeState>();
            rng = new RandomNumberGenerator();
        }
        
        /// <summary>
        /// Register a showtime attack for this battle
        /// </summary>
        public void RegisterShowtime(ShowtimeAttackData showtime)
        {
            if (showtime == null) return;
            
            availableShowtimes.Add(showtime);
            string key = GetPairKey(showtime.Character1Id, showtime.Character2Id);
            showtimeStates[key] = new ShowtimeState(showtime);
            
            GD.Print($"Registered Synchrony: {showtime.AttackName} ({showtime.Character1Id} + {showtime.Character2Id})");
        }
        
        /// <summary>
        /// Check if showtime should trigger this turn
        /// </summary>
        public ShowtimeAttackData CheckForShowtimeTrigger(List<BattleMember> party)
        {
            if (party == null || party.Count < 2) return null;
            
            // Get all possible pairs in party
            for (int i = 0; i < party.Count; i++)
            {
                for (int j = i + 1; j < party.Count; j++)
                {
                    var member1 = party[i];
                    var member2 = party[j];
                    
                    // Check if valid pair
                    string key = GetPairKey(member1.Stats.CharacterName, member2.Stats.CharacterName);
                    
                    if (showtimeStates.TryGetValue(key, out var state))
                    {
                        // Check conditions
                        if (!CanTriggerShowtime(state, member1, member2))
                            continue;
                        
                        // Roll for trigger
                        if (rng.Randf() * 100 < state.AttackData.TriggerChance)
                        {
                            GD.Print($"\n★★★ SYNCHRONY READY! ★★★");
                            GD.Print($"{state.AttackData.AttackName}");
                            return state.AttackData;
                        }
                    }
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// Check if showtime can trigger
        /// </summary>
        private bool CanTriggerShowtime(ShowtimeState state, BattleMember member1, BattleMember member2)
        {
            if (!state.IsAvailable) return false;
            
            var data = state.AttackData;
            
            // Both must be alive if required
            if (data.RequiresBothAlive)
            {
                if (!member1.Stats.IsAlive || !member2.Stats.IsAlive)
                    return false;
            }
            
            // Check HP requirements
            float hp1Percent = member1.Stats.HPPercent;
            float hp2Percent = member2.Stats.HPPercent;
            
            if (hp1Percent < data.MinHPPercent || hp2Percent < data.MinHPPercent)
            {
                // If min HP is low, this might be a "desperate" showtime
                // Otherwise, they need enough HP
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Execute a showtime attack
        /// </summary>
        public BattleActionResult ExecuteShowtime(
            ShowtimeAttackData showtime, 
            BattleMember member1, 
            BattleMember member2, 
            List<BattleMember> targets)
        {
            var result = new BattleActionResult
            {
                Success = true,
                Message = $"\n╔══════════════════════════════════════╗\n" +
                         $"║  ★★★ SYNCHRONY: {showtime.AttackName} ★★★  \n" +
                         $"╚══════════════════════════════════════╝\n"
            };
            
            GD.Print(result.Message);
            if (!string.IsNullOrEmpty(showtime.FlavorText))
            {
                GD.Print($"   \"{showtime.FlavorText}\"");
            }
            
            // Calculate damage
            int baseDamage = showtime.BasePower;
            
            // Use both characters' stats
            int combinedAttack = (member1.Stats.Attack + member2.Stats.Attack) / 2;
            baseDamage += combinedAttack;
            
            // Apply multiplier
            baseDamage = Mathf.RoundToInt(baseDamage * showtime.DamageMultiplier);
            
            // Apply to all targets
            foreach (var target in targets)
            {
                if (!target.Stats.IsAlive) continue;
                
                int damage = baseDamage;
                
                // Roll for critical
                bool isCrit = rng.Randf() * 100 < showtime.CriticalChance;
                if (isCrit)
                {
                    damage = Mathf.RoundToInt(damage * 1.5f);
                    GD.Print("   *** CRITICAL! ***");
                }
                
                // Apply defense unless ignored
                if (!showtime.IgnoresDefense)
                {
                    damage -= target.Stats.Defense / 2;
                }
                
                damage = Mathf.Max(1, damage);
                
                // Apply elemental multiplier
                float elementMultiplier = target.Stats.ElementAffinities.GetDamageMultiplier(showtime.Element);
                damage = Mathf.RoundToInt(damage * elementMultiplier);
                
                // Deal damage
                int actualDamage = target.Stats.TakeDamage(damage, showtime.Element);
                result.DamageDealt += actualDamage;
                
                GD.Print($"   → {target.Stats.CharacterName}: {actualDamage} damage!");
                
                // Check for instant kill
                if (showtime.InstantKillChance && target.Stats.IsAlive)
                {
                    if (rng.Randf() * 100 < showtime.InstantKillRate)
                    {
                        target.Stats.TakeDamage(target.Stats.MaxHP, showtime.Element);
                        GD.Print($"   ☠ INSTANT KILL! ☠");
                    }
                }
                
                // Apply status effects
                if (target.Stats.IsAlive && showtime.InflictsStatuses.Count > 0)
                {
                    foreach (var status in showtime.InflictsStatuses)
                    {
                        if (rng.Randf() * 100 < showtime.StatusChance)
                        {
                            result.StatusesApplied.Add(status);
                        }
                    }
                }
            }
            
            // Mark as used
            string key = GetPairKey(member1.Stats.CharacterName, member2.Stats.CharacterName);
            if (showtimeStates.TryGetValue(key, out var state))
            {
                state.UseShowtime();
            }
            
            GD.Print($"\nSynchrony complete! Total damage: {result.DamageDealt}");
            
            return result;
        }
        
        
        /// <summary>
        /// Check if a showtime can be activated for two characters
        /// </summary>
        public bool CanShowtimeActivate(ShowtimeAttackData showtime, BattleMember char1, BattleMember char2)
        {
            if (showtime == null || char1 == null || char2 == null)
                return false;
    
            // Check if characters match the showtime requirement
            bool char1Match = char1.Stats.CharacterName == showtime.Character1Id || 
                              char1.Stats.CharacterName == showtime.Character2Id;
            bool char2Match = char2.Stats.CharacterName == showtime.Character1Id || 
                              char2.Stats.CharacterName == showtime.Character2Id;
    
            if (!char1Match || !char2Match)
                return false;
    
            // Check if both characters are alive
            if (char1.Stats.CurrentHP <= 0 || char2.Stats.CurrentHP <= 0)
                return false;
    
            // Check if showtime is off cooldown
            string key = GetPairKey(char1.Stats.CharacterName, char2.Stats.CharacterName);
            if (showtimeStates.TryGetValue(key, out var state))
            {
                return state.IsAvailable;
            }
    
            return false;
        }

        /// <summary>
        /// Put a showtime on cooldown after use
        /// </summary>
        public void PutOnCooldown(ShowtimeAttackData showtime)
        {
            if (showtime == null)
                return;
    
            string key = GetPairKey(showtime.Character1Id, showtime.Character2Id);
    
            if (showtimeStates.TryGetValue(key, out var state))
            {
                state.UseShowtime();
                GD.Print($"Synchrony '{showtime.AttackName}' put on cooldown for {showtime.CooldownTurns} turns");
            }
        }
        
        
        /// <summary>
        /// Increment turn counters for cooldowns
        /// </summary>
        public void IncrementTurn()
        {
            foreach (var state in showtimeStates.Values)
            {
                state.IncrementTurn();
            }
        }
        
        /// <summary>
        /// Get showtime for specific character pair
        /// </summary>
        public ShowtimeAttackData GetShowtimeForPair(string char1, string char2)
        {
            string key = GetPairKey(char1, char2);
            if (showtimeStates.TryGetValue(key, out var state))
            {
                return state.AttackData;
            }
            return null;
        }
        
        /// <summary>
        /// Check if showtime is available for pair
        /// </summary>
        public bool IsShowtimeAvailable(string char1, string char2)
        {
            string key = GetPairKey(char1, char2);
            if (showtimeStates.TryGetValue(key, out var state))
            {
                return state.IsAvailable;
            }
            return false;
        }
        
        /// <summary>
        /// Get consistent key for character pair (order independent)
        /// </summary>
        private string GetPairKey(string char1, string char2)
        {
            // Sort alphabetically so order doesn't matter
            if (string.Compare(char1, char2, StringComparison.Ordinal) < 0)
                return $"{char1}_{char2}";
            return $"{char2}_{char1}";
        }
        
        /// <summary>
        /// Get all available showtimes for display
        /// </summary>
        public List<ShowtimeAttackData> GetAvailableShowtimes()
        {
            return showtimeStates.Values
                .Where(s => s.IsAvailable)
                .Select(s => s.AttackData)
                .ToList();
        }
    }
}