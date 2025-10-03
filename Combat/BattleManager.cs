using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EchoesAcrossTime.Combat
{
    /// <summary>
    /// Core battle manager implementing Persona 5 Royal combat mechanics
    /// Handles turn order, weakness system, One More, and All-Out Attack
    /// </summary>
    public partial class BattleManager : Node
    {
        // Battle participants
        private List<BattleMember> playerParty = new List<BattleMember>();
        private List<BattleMember> enemyParty = new List<BattleMember>();
        private List<BattleMember> turnOrder = new List<BattleMember>();
        
        // Battle state
        public enum BattlePhase
        {
            Initializing,
            TurnStart,
            PlayerAction,
            EnemyAction,
            Processing,
            AllOutAttackPrompt,
            Victory,
            Defeat,
            Escaped
        }
        
        public BattlePhase CurrentPhase { get; private set; }
        public BattleMember CurrentActor { get; private set; }
        public int CurrentTurn { get; private set; }
        public bool AllOutAttackAvailable { get; private set; }
        
        private RandomNumberGenerator rng;
        private StatusEffectManager statusManager;
        private BatonPassManager batonPassManager;
        private TechnicalDamageSystem technicalSystem;
        private ShowtimeManager showtimeManager;
        private LimitBreakSystem limitBreakSystem;
        private GuardSystem guardSystem;
        private BattleItemSystem itemSystem;
        private EscapeSystem escapeSystem;
        
        // Battle flags
        private bool isBossBattle = false;
        private bool isPinnedDown = false;
        
        // Signals for UI updates
        [Signal] public delegate void BattleStartedEventHandler();
        [Signal] public delegate void TurnStartedEventHandler(string characterName);
        [Signal] public delegate void ActionExecutedEventHandler(string actorName, string actionName, int damageDealt, bool hitWeakness, bool wasCritical);
        [Signal] public delegate void WeaknessHitEventHandler(string attackerName, string targetName);
        [Signal] public delegate void OneMoreTriggeredEventHandler(string characterName);
        [Signal] public delegate void AllOutAttackReadyEventHandler();
        [Signal] public delegate void BattleEndedEventHandler(bool victory);
        [Signal] public delegate void KnockdownEventHandler(string characterName);
        [Signal] public delegate void BatonPassExecutedEventHandler(string fromCharacter, string toCharacter, int passLevel);
        [Signal] public delegate void TechnicalDamageEventHandler(string attackerName, string targetName, string comboType);
        [Signal] public delegate void ShowtimeTriggeredEventHandler(string showtimeName, string char1, string char2);
        [Signal] public delegate void LimitBreakReadyEventHandler(string characterName);
        [Signal] public delegate void LimitBreakUsedEventHandler(string characterName, string limitBreakName, bool isDuo);
        
        public override void _Ready()
        {
            rng = new RandomNumberGenerator();
            statusManager = new StatusEffectManager();
            batonPassManager = new BatonPassManager();
            technicalSystem = new TechnicalDamageSystem();
            showtimeManager = new ShowtimeManager();
            limitBreakSystem = new LimitBreakSystem();
            guardSystem = new GuardSystem();
            itemSystem = new BattleItemSystem(statusManager);
            escapeSystem = new EscapeSystem();
        }
        
        #region Battle Initialization
        
        /// <summary>
        /// Initialize battle with party members and enemies
        /// </summary>
        public void InitializeBattle(
            List<CharacterStats> playerStats, 
            List<CharacterStats> enemyStats, 
            List<ShowtimeAttackData> showtimes = null,
            List<LimitBreakData> limitBreaks = null,
            bool bossBattle = false,
            bool pinnedDown = false)
        {
            CurrentPhase = BattlePhase.Initializing;
            CurrentTurn = 0;
            isBossBattle = bossBattle;
            isPinnedDown = pinnedDown;
            
            // Clear previous battle data
            playerParty.Clear();
            enemyParty.Clear();
            turnOrder.Clear();
            
            // Reset escape system
            escapeSystem.ResetAttempts();
            
            // Create player party
            for (int i = 0; i < playerStats.Count; i++)
            {
                var member = new BattleMember(playerStats[i], true, i);
                playerParty.Add(member);
            }
            
            // Create enemy party
            for (int i = 0; i < enemyStats.Count; i++)
            {
                var member = new BattleMember(enemyStats[i], false, i);
                enemyParty.Add(member);
            }
            
            // Register showtime attacks
            if (showtimes != null)
            {
                foreach (var showtime in showtimes)
                {
                    showtimeManager.RegisterShowtime(showtime);
                }
            }
            
            // Register limit breaks
            if (limitBreaks != null)
            {
                foreach (var limitBreak in limitBreaks)
                {
                    limitBreakSystem.RegisterLimitBreak(limitBreak);
                }
            }
            
            // Calculate initial turn order
            CalculateTurnOrder();
            
            CurrentPhase = BattlePhase.TurnStart;
            EmitSignal(SignalName.BattleStarted);
            
            string battleType = bossBattle ? "BOSS BATTLE" : "Battle";
            GD.Print($"{battleType} initialized: {playerParty.Count} heroes vs {enemyParty.Count} enemies");
            
            if (bossBattle)
            {
                GD.Print("⚠️ Boss Battle - Cannot Escape!");
            }
            
            StartNextTurn();
        }
        
        /// <summary>
        /// Calculate turn order based on Speed stat
        /// </summary>
        private void CalculateTurnOrder()
        {
            turnOrder.Clear();
            
            // Add all living combatants
            turnOrder.AddRange(playerParty.Where(m => m.Stats.IsAlive));
            turnOrder.AddRange(enemyParty.Where(m => m.Stats.IsAlive));
            
            // Sort by Speed (highest first)
            turnOrder = turnOrder.OrderByDescending(m => m.Stats.Speed).ToList();
            
            GD.Print("Turn order calculated:");
            foreach (var member in turnOrder)
            {
                GD.Print($"  {member.Stats.CharacterName} (Speed: {member.Stats.Speed})");
            }
        }
        
        #endregion
        
        #region Turn Management
        
        /// <summary>
        /// Start the next turn in battle
        /// </summary>
        public void StartNextTurn()
        {
            // Check for battle end conditions
            if (CheckBattleEnd())
                return;
            
            // Check for showtime trigger (random chance at turn start)
            var showtimeAttack = showtimeManager.CheckForShowtimeTrigger(playerParty);
            if (showtimeAttack != null)
            {
                // Find the two characters involved
                var char1 = playerParty.FirstOrDefault(p => p.Stats.CharacterName == showtimeAttack.Character1Id);
                var char2 = playerParty.FirstOrDefault(p => p.Stats.CharacterName == showtimeAttack.Character2Id);
                
                if (char1 != null && char2 != null)
                {
                    EmitSignal(SignalName.ShowtimeTriggered, showtimeAttack.AttackName, char1.Stats.CharacterName, char2.Stats.CharacterName);
                    // Showtime would be executed here or prompted to player
                }
            }
            
            // Find next actor who can act
            CurrentActor = GetNextActor();
            
            if (CurrentActor == null)
            {
                // Everyone has acted this round, start new round
                StartNewRound();
                return;
            }
            
            CurrentActor.StartRound();
            
            // Process guard effects (HP/MP regen, limit gauge)
            guardSystem.ProcessGuardEffects(CurrentActor);
            
            // Process turn start status effects
            statusManager.ProcessStatusEffects(CurrentActor.Stats, true);
            
            // Check if still alive after status effects
            if (!CurrentActor.Stats.IsAlive)
            {
                GD.Print($"{CurrentActor.Stats.CharacterName} died from status effects!");
                StartNextTurn();
                return;
            }
            
            // Check if can act
            if (!CurrentActor.CanAct())
            {
                GD.Print($"{CurrentActor.Stats.CharacterName} cannot act (status effect)");
                CurrentActor.EndTurn();
                StartNextTurn();
                return;
            }
            
            // Set phase based on actor
            CurrentPhase = CurrentActor.IsPlayerControlled 
                ? BattlePhase.PlayerAction 
                : BattlePhase.EnemyAction;
            
            EmitSignal(SignalName.TurnStarted, CurrentActor.Stats.CharacterName);
            GD.Print($"\n=== {CurrentActor.Stats.CharacterName}'s Turn ===");
            
            // Check for All-Out Attack availability at start of player turn
            if (CurrentActor.IsPlayerControlled)
            {
                CheckAllOutAttackAvailable();
            }
        }
        
        /// <summary>
        /// Start a new round (all combatants have acted)
        /// </summary>
        private void StartNewRound()
        {
            CurrentTurn++;
            GD.Print($"\n╔════════════════════════════════════════╗");
            GD.Print($"║         Round {CurrentTurn} Starting         ║");
            GD.Print($"╔════════════════════════════════════════╗\n");
            
            // Reset all members for new round
            foreach (var member in playerParty.Concat(enemyParty))
            {
                member.StartRound();
            }
            
            // Increment showtime cooldowns
            showtimeManager.IncrementTurn();
            
            // Recalculate turn order (in case speeds changed)
            CalculateTurnOrder();
            
            // Start first turn
            StartNextTurn();
        }
        
        /// <summary>
        /// Get the next actor who hasn't acted yet
        /// </summary>
        private BattleMember GetNextActor()
        {
            foreach (var member in turnOrder)
            {
                if (!member.HasActedThisTurn && member.Stats.IsAlive)
                    return member;
            }
            return null;
        }
        
        #endregion
        
        #region Action Execution
        
        /// <summary>
        /// Execute a battle action
        /// </summary>
        public BattleActionResult ExecuteAction(BattleAction action)
        {
            if (!action.IsValid())
            {
                GD.PrintErr($"Invalid action: {action}");
                return new BattleActionResult { Success = false, Message = "Invalid action" };
            }
            
            CurrentPhase = BattlePhase.Processing;
            GD.Print($"Executing: {action}");
            
            BattleActionResult result = action.ActionType switch
            {
                BattleActionType.Attack => ExecuteBasicAttack(action),
                BattleActionType.Skill => ExecuteSkill(action),
                BattleActionType.Guard => ExecuteGuard(action),
                BattleActionType.Item => ExecuteItem(action),
                BattleActionType.Escape => ExecuteEscape(action),
                BattleActionType.AllOutAttack => ExecuteAllOutAttack(action),
                BattleActionType.LimitBreak => ExecuteLimitBreak(action),
                _ => new BattleActionResult { Success = false, Message = "Not implemented" }
            };
            
            // Handle One More system
            if (result.Success && (result.HitWeakness || result.WasCritical))
            {
                HandleOneMore(action.Actor, result);
            }
            
            // Check for knockdown on weakness/crit
            if (result.Success && (result.HitWeakness || result.WasCritical))
            {
                foreach (var target in action.Targets)
                {
                    if (target.Stats.IsAlive && !target.IsKnockedDown)
                    {
                        target.KnockDown();
                        result.CausedKnockdown = true;
                        EmitSignal(SignalName.Knockdown, target.Stats.CharacterName);
                    }
                }
            }
            
            // End turn if no extra turn granted
            if (!action.Actor.HasExtraTurn)
            {
                action.Actor.EndTurn();
                
                // Clear guard state at end of turn
                guardSystem.ClearGuard(action.Actor);
            }
            
            EmitSignal(SignalName.ActionExecuted, 
                action.Actor.Stats.CharacterName, 
                action.Skill?.DisplayName ?? action.ActionType.ToString(),
                result.DamageDealt,
                result.HitWeakness,
                result.WasCritical);
            
            // Check for battle end
            if (!CheckBattleEnd())
            {
                // Check for All-Out Attack opportunity
                if (action.Actor.IsPlayerControlled && AreAllEnemiesKnockedDown())
                {
                    CurrentPhase = BattlePhase.AllOutAttackPrompt;
                    AllOutAttackAvailable = true;
                    EmitSignal(SignalName.AllOutAttackReady);
                    GD.Print("*** ALL-OUT ATTACK AVAILABLE! ***");
                }
                else
                {
                    // Continue to next turn
                    StartNextTurn();
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// Execute Limit Break
        /// </summary>
        private BattleActionResult ExecuteLimitBreak(BattleAction action)
        {
            var limitBreak = action.LimitBreak;
            var targets = action.Targets.ToList();
            
            // Check if duo
            bool isDuo = action.DuoPartner != null;
            
            EmitSignal(SignalName.LimitBreakUsed, 
                action.Actor.Stats.CharacterName, 
                limitBreak.DisplayName, 
                isDuo);
            
            // Execute via limit break system
            var result = limitBreakSystem.ExecuteLimitBreak(
                limitBreak, 
                action.Actor, 
                targets,
                action.DuoPartner
            );
            
            // End turns
            action.Actor.EndTurn();
            if (isDuo && action.DuoPartner != null)
            {
                action.DuoPartner.EndTurn();
            }
            
            return result;
        }
        
        /// <summary>
        /// Execute a basic attack
        /// </summary>
        private BattleActionResult ExecuteBasicAttack(BattleAction action)
        {
            var result = new BattleActionResult();
            var attacker = action.Actor.Stats;
            
            foreach (var targetMember in action.Targets)
            {
                var target = targetMember.Stats;
                
                // Check if attack hits
                bool hits = attacker.BattleStats.RollHit(attacker.BattleStats.AccuracyRate, rng);
                
                if (!hits)
                {
                    result.Missed = true;
                    result.Message = $"{attacker.CharacterName}'s attack missed!";
                    GD.Print(result.Message);
                    continue;
                }
                
                // Check for critical
                bool isCritical = attacker.BattleStats.RollCritical(0, rng);
                result.WasCritical = isCritical;
                
                // Calculate damage
                int baseDamage = attacker.Attack - target.Defense / 2;
                baseDamage = Mathf.Max(1, baseDamage);
                
                if (isCritical)
                {
                    baseDamage = attacker.BattleStats.ApplyCriticalDamage(baseDamage);
                    GD.Print($"*** CRITICAL HIT! ***");
                }
                
                // Apply damage
                int actualDamage = target.TakeDamage(baseDamage, ElementType.Physical);
                result.DamageDealt += actualDamage;
                result.Success = true;
                
                // Add limit gauge
                limitBreakSystem.AddGaugeFromDealingDamage(action.Actor, actualDamage, isCritical);
                limitBreakSystem.AddGaugeFromDamage(targetMember, actualDamage);
                
                result.Message = $"{attacker.CharacterName} dealt {actualDamage} damage to {target.CharacterName}!";
                GD.Print(result.Message);
            }
            
            return result;
        }
        
        /// <summary>
        /// Execute a skill
        /// </summary>
        private BattleActionResult ExecuteSkill(BattleAction action)
        {
            var result = new BattleActionResult();
            var skill = action.Skill;
            var attacker = action.Actor.Stats;
            
            // Consume resources
            skill.ConsumeResources(attacker);
            
            foreach (var targetMember in action.Targets)
            {
                var target = targetMember.Stats;
                
                // Check if skill hits
                bool hits = skill.RollAccuracy(attacker, target, rng);
                
                if (!hits)
                {
                    result.Missed = true;
                    result.Message = $"{skill.DisplayName} missed!";
                    GD.Print(result.Message);
                    continue;
                }
                
                // Check for technical damage FIRST
                var technical = technicalSystem.CheckTechnical(target, skill.Element);
                
                // Check for critical (bonus from baton pass)
                int critBonus = skill.CriticalBonus + batonPassManager.GetCriticalBonus(action.Actor);
                bool isCritical = attacker.BattleStats.RollCritical(critBonus, rng);
                result.WasCritical = isCritical;
                
                // Calculate damage
                int damage = skill.CalculateDamage(attacker, target, isCritical);
                
                // Apply baton pass damage bonus
                damage = batonPassManager.ApplyDamageBonus(action.Actor, damage);
                
                // Apply technical damage bonus
                if (technical.IsTechnical)
                {
                    damage = technicalSystem.ApplyTechnicalDamage(damage, technical);
                    EmitSignal(SignalName.TechnicalDamage, attacker.CharacterName, target.CharacterName, technical.ComboType.ToString());
                }
                
                // Check elemental affinity
                float affinity = target.ElementAffinities.GetDamageMultiplier(skill.Element);
                bool hitWeakness = affinity > 1.0f;
                bool wasResisted = affinity < 1.0f && affinity > 0f;
                bool wasAbsorbed = affinity < 0f;
                
                result.HitWeakness = hitWeakness;
                result.WasResisted = wasResisted;
                result.WasAbsorbed = wasAbsorbed;
                
                // Apply elemental modifier
                damage = Mathf.RoundToInt(damage * affinity);
                
                // Apply guard reduction if target is guarding
                damage = guardSystem.ApplyGuardReduction(targetMember, damage);
                
                // Apply damage
                int actualDamage = target.TakeDamage(damage, skill.Element);
                result.DamageDealt += actualDamage;
                result.Success = true;
                
                // Add limit gauge
                limitBreakSystem.AddGaugeFromDealingDamage(action.Actor, actualDamage, isCritical);
                limitBreakSystem.AddGaugeFromDamage(targetMember, actualDamage);
                
                // Build message
                string message = $"{attacker.CharacterName} used {skill.DisplayName}!";
                if (technical.IsTechnical) message += $" ★★★ TECHNICAL! {technical.Message} ★★★";
                if (hitWeakness)
                {
                    message += " ★ WEAKNESS! ★";
                    EmitSignal(SignalName.WeaknessHit, attacker.CharacterName, target.CharacterName);
                }
                if (isCritical) message += " *** CRITICAL! ***";
                if (wasResisted) message += " (Resisted)";
                if (wasAbsorbed) message += " (Absorbed)";
                if (action.Actor.BatonPassData.IsActive) message += $" [{action.Actor.BatonPassData.GetBonusText()}]";
                message += $" {actualDamage} damage to {target.CharacterName}!";
                
                result.Message = message;
                GD.Print(message);
                
                // Remove status if technical combo requires it
                if (technical.IsTechnical)
                {
                    foreach (var status in target.ActiveStatuses.ToList())
                    {
                        if (technicalSystem.ShouldRemoveStatus(technical.ComboType, status.Effect))
                        {
                            statusManager.RemoveStatus(target, status.Effect);
                            GD.Print($"  → {status.Effect} removed from {target.CharacterName}");
                        }
                    }
                }
                
                // Apply status effects
                var statuses = skill.RollStatusEffects(rng);
                foreach (var status in statuses)
                {
                    if (!target.BattleStats.RollStatusResistance(status, rng))
                    {
                        statusManager.ApplyStatus(target, status, skill.BuffDuration);
                        result.StatusesApplied.Add(status);
                        GD.Print($"  → {target.CharacterName} is now {status}!");
                    }
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// Execute guard action
        /// </summary>
        private BattleActionResult ExecuteGuard(BattleAction action)
        {
            // Clear previous guard state (in case still active)
            guardSystem.ClearGuard(action.Actor);
            
            // Execute guard
            var result = guardSystem.ExecuteGuard(action.Actor);
            
            return result;
        }
        
        /// <summary>
        /// Execute item usage
        /// </summary>
        private BattleActionResult ExecuteItem(BattleAction action)
        {
            var item = action.ItemData as Items.ConsumableData;
            
            if (item == null)
            {
                return new BattleActionResult 
                { 
                    Success = false, 
                    Message = "Invalid item!" 
                };
            }
            
            // Use item via item system
            var result = itemSystem.UseItem(action.Actor, item, action.Targets);
            
            // Note: Inventory deduction should be handled by caller
            // The battle system just executes the effects
            
            return result;
        }
        
        /// <summary>
        /// Attempt to escape from battle
        /// </summary>
        private BattleActionResult ExecuteEscape(BattleAction action)
        {
            var escapeResult = escapeSystem.AttemptEscape(
                playerParty, 
                enemyParty, 
                isBossBattle, 
                isPinnedDown
            );
            
            var result = new BattleActionResult
            {
                Success = escapeResult.Success,
                Message = escapeResult.Message
            };
            
            if (escapeResult.Success)
            {
                // Battle ends successfully via escape
                CurrentPhase = BattlePhase.Escaped;
                EmitSignal(SignalName.BattleEnded, false); // false = not victory, but not defeat either
            }
            
            return result;
        }
        
        /// <summary>
        /// Execute All-Out Attack (hits all knocked down enemies)
        /// </summary>
        private BattleActionResult ExecuteAllOutAttack(BattleAction action)
        {
            var result = new BattleActionResult();
            
            GD.Print("\n╔═══════════════════════════════════════╗");
            GD.Print("║       ★ ALL-OUT ATTACK! ★          ║");
            GD.Print("╚═══════════════════════════════════════╝\n");
            
            // All party members attack knocked down enemies
            foreach (var playerMember in playerParty.Where(p => p.Stats.IsAlive))
            {
                foreach (var enemyMember in enemyParty.Where(e => e.IsKnockedDown))
                {
                    var enemy = enemyMember.Stats;
                    
                    // Calculate massive damage
                    int damage = playerMember.Stats.Attack * 2;
                    int actualDamage = enemy.TakeDamage(damage, ElementType.Physical);
                    result.DamageDealt += actualDamage;
                    
                    GD.Print($"  {playerMember.Stats.CharacterName} → {enemy.CharacterName}: {actualDamage} damage!");
                    
                    // Stand up enemy after hit
                    enemyMember.StandUp();
                }
            }
            
            result.Success = true;
            result.Message = "All-Out Attack finished!";
            AllOutAttackAvailable = false;
            
            // All party members have acted
            foreach (var member in playerParty)
            {
                member.EndTurn();
            }
            
            return result;
        }
        
        #endregion
        
        #region Persona 5 Mechanics
        
        /// <summary>
        /// Handle One More system (extra turn on weakness/crit)
        /// </summary>
        private void HandleOneMore(BattleMember actor, BattleActionResult result)
        {
            actor.HasExtraTurn = true;
            actor.OneMoreCount++;
            
            string reason = result.HitWeakness ? "hit a weakness" : "landed a critical hit";
            EmitSignal(SignalName.OneMoreTriggered, actor.Stats.CharacterName);
            
            GD.Print($"\n★★★ ONE MORE! ★★★");
            GD.Print($"{actor.Stats.CharacterName} gets another turn for {reason}!\n");
        }
        
        /// <summary>
        /// Check if All-Out Attack is available
        /// </summary>
        private void CheckAllOutAttackAvailable()
        {
            AllOutAttackAvailable = AreAllEnemiesKnockedDown() && 
                                   playerParty.Any(p => p.Stats.IsAlive);
            
            if (AllOutAttackAvailable)
            {
                foreach (var member in playerParty.Where(p => p.Stats.IsAlive))
                {
                    member.CanAllOutAttack = true;
                }
            }
        }
        
        /// <summary>
        /// Check if all enemies are knocked down
        /// </summary>
        private bool AreAllEnemiesKnockedDown()
        {
            var livingEnemies = enemyParty.Where(e => e.Stats.IsAlive).ToList();
            if (livingEnemies.Count == 0) return false;
            
            return livingEnemies.All(e => e.IsKnockedDown);
        }
        
        #endregion
        
        #region Battle End Conditions
        
        /// <summary>
        /// Check if battle should end
        /// </summary>
        private bool CheckBattleEnd()
        {
            bool allPlayersDead = playerParty.All(p => !p.Stats.IsAlive);
            bool allEnemiesDead = enemyParty.All(e => !e.Stats.IsAlive);
            
            if (allPlayersDead)
            {
                CurrentPhase = BattlePhase.Defeat;
                EmitSignal(SignalName.BattleEnded, false);
                GD.Print("\n*** DEFEAT ***\n");
                return true;
            }
            
            if (allEnemiesDead)
            {
                CurrentPhase = BattlePhase.Victory;
                EmitSignal(SignalName.BattleEnded, true);
                GD.Print("\n*** VICTORY ***\n");
                return true;
            }
            
            return false;
        }
        
        #endregion
        
        #region Public Query Methods
        
        public List<BattleMember> GetPlayerParty() => playerParty;
        public List<BattleMember> GetEnemyParty() => enemyParty;
        public List<BattleMember> GetLivingEnemies() => enemyParty.Where(e => e.Stats.IsAlive).ToList();
        public List<BattleMember> GetLivingAllies() => playerParty.Where(p => p.Stats.IsAlive).ToList();
        
        public bool IsPlayerTurn() => CurrentPhase == BattlePhase.PlayerAction;
        public bool CanUseAllOutAttack() => AllOutAttackAvailable;
        
        /// <summary>
        /// Execute baton pass from current actor to target
        /// </summary>
        public bool ExecuteBatonPass(BattleMember target)
        {
            if (CurrentActor == null) return false;
            
            bool success = batonPassManager.ExecuteBatonPass(CurrentActor, target);
            
            if (success)
            {
                EmitSignal(SignalName.BatonPassExecuted, 
                    CurrentActor.Stats.CharacterName, 
                    target.Stats.CharacterName, 
                    target.BatonPassData.PassCount);
                
                // Target becomes current actor
                CurrentActor = target;
                CurrentPhase = BattlePhase.PlayerAction;
            }
            
            return success;
        }
        
        /// <summary>
        /// Check if current actor can baton pass
        /// </summary>
        public bool CanBatonPass()
        {
            return CurrentActor != null && batonPassManager.CanBatonPass(CurrentActor);
        }
        
        /// <summary>
        /// Get valid baton pass targets
        /// </summary>
        public List<BattleMember> GetBatonPassTargets()
        {
            if (CurrentActor == null) return new List<BattleMember>();
            
            return playerParty
                .Where(m => batonPassManager.CanReceiveBatonPass(CurrentActor, m))
                .ToList();
        }
        
        /// <summary>
        /// Execute a showtime attack
        /// </summary>
        public BattleActionResult ExecuteShowtime(ShowtimeAttackData showtime)
        {
            // Find the two characters
            var char1 = playerParty.FirstOrDefault(p => p.Stats.CharacterName == showtime.Character1Id);
            var char2 = playerParty.FirstOrDefault(p => p.Stats.CharacterName == showtime.Character2Id);
            
            if (char1 == null || char2 == null)
            {
                return new BattleActionResult { Success = false, Message = "Invalid showtime pair" };
            }
            
            // Get all living enemies as targets
            var targets = showtime.HitsAllEnemies 
                ? GetLivingEnemies() 
                : new List<BattleMember> { GetLivingEnemies().FirstOrDefault() };
            
            var result = showtimeManager.ExecuteShowtime(showtime, char1, char2, targets);
            
            // Both characters have acted
            char1.EndTurn();
            char2.EndTurn();
            
            return result;
        }
        
        /// <summary>
        /// Get available showtime attacks
        /// </summary>
        public List<ShowtimeAttackData> GetAvailableShowtimes()
        {
            return showtimeManager.GetAvailableShowtimes();
        }
        
        /// <summary>
        /// Check if limit break is ready for character
        /// </summary>
        public bool IsLimitBreakReady(BattleMember member)
        {
            return member != null && member.IsLimitBreakReady;
        }
        
        /// <summary>
        /// Get limit break for character
        /// </summary>
        public LimitBreakData GetLimitBreak(BattleMember member)
        {
            if (member == null) return null;
            return limitBreakSystem.GetLimitBreak(member.Stats.CharacterName);
        }
        
        /// <summary>
        /// Get limit gauge percent (0-1) for UI
        /// </summary>
        public float GetLimitGaugePercent(BattleMember member)
        {
            return limitBreakSystem.GetGaugePercent(member);
        }
        
        /// <summary>
        /// Check if can escape from battle
        /// </summary>
        public bool CanEscape()
        {
            return escapeSystem.CanAttemptEscape(playerParty, isBossBattle, isPinnedDown);
        }
        
        /// <summary>
        /// Get escape chance percentage for UI display
        /// </summary>
        public int GetEscapeChance()
        {
            return escapeSystem.GetEscapeChanceDisplay(playerParty, enemyParty);
        }
        
        /// <summary>
        /// Check if item can be used on target
        /// </summary>
        public bool CanUseItemOn(Items.ConsumableData item, BattleMember target)
        {
            return itemSystem.CanUseItemOn(item, target);
        }
        
        /// <summary>
        /// Get valid targets for an item
        /// </summary>
        public List<BattleMember> GetValidItemTargets(Items.ConsumableData item)
        {
            return itemSystem.GetValidItemTargets(item, playerParty, enemyParty);
        }
        
        #endregion
    }
}