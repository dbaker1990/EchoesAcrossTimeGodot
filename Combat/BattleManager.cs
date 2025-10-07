using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using EchoesAcrossTime.Bestiary;
using EchoesAcrossTime.Events;

namespace EchoesAcrossTime.Combat
{
    /// <summary>
    /// Main battle system manager - handles all combat logic
    /// Integrates: Weaknesses, One More, Knockdown, All-Out Attack, Baton Pass,
    /// Technical, Showtime, Limit Breaks, Guard, Items, Escape, and Rewards
    /// </summary>
    public partial class BattleManager : Node
    {
        #region Fields
        
        // Battle state
        private List<BattleMember> playerParty;
        private List<BattleMember> enemyParty;
        private List<BattleMember> turnOrder;
        private int currentRound = 0;
        private bool isBossBattle = false;
        private bool isPinnedDown = false;
        // Escape system
        private int escapeAttempts = 0;
        private bool escapeBlocked = false;
        
        // Systems
        private RandomNumberGenerator rng;
        private StatusEffectManager statusManager;
        private BatonPassManager batonPassManager;
        private TechnicalDamageSystem technicalSystem;
        private ShowtimeManager showtimeManager;
        private LimitBreakSystem limitBreakSystem;
        private GuardSystem guardSystem;
        private BattleItemSystem itemSystem;
        private EscapeSystem escapeSystem;
        private BattleRewardsManager rewardsManager;
        
        #endregion
        
        #region Properties
        
        public BattlePhase CurrentPhase { get; private set; } = BattlePhase.NotStarted;
        public BattleMember CurrentActor { get; private set; }
        public bool AllOutAttackAvailable { get; private set; } = false;
        
        #endregion
        
        #region Signals
        
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
        
        #endregion
        
        #region Initialization
        
        public override void _Ready()
        {
            rng = new RandomNumberGenerator();
            rng.Randomize();
            
            statusManager = new StatusEffectManager();
            batonPassManager = new BatonPassManager();
            technicalSystem = new TechnicalDamageSystem();
            showtimeManager = new ShowtimeManager();
            limitBreakSystem = new LimitBreakSystem();
            guardSystem = new GuardSystem();
            itemSystem = new BattleItemSystem(statusManager);
            escapeSystem = new EscapeSystem();
            
            // Initialize rewards manager
            rewardsManager = new BattleRewardsManager();
            AddChild(rewardsManager);
            
            // Connect basic tracking
            ActionExecuted += TrackDamageForRewards;
            WeaknessHit += (a, t) => rewardsManager?.RecordEvent("weakness_hit");
            
            // Connect to signals for reward tracking
            ConnectRewardSignals();
        }
        
        /// <summary>
        /// Initialize battle with all parameters
        /// </summary>
        public void InitializeBattle(
            List<CharacterStats> playerStats,
            List<CharacterStats> enemyStats,
            List<ShowtimeAttackData> availableShowtimes = null,
            List<LimitBreakData> availableLimitBreaks = null,
            bool isBossBattle = false,
            bool isPinnedDown = false)
        {
            this.isBossBattle = isBossBattle;
            this.isPinnedDown = isPinnedDown;
            
            // Create battle members with positions
            playerParty = new List<BattleMember>();
            for (int i = 0; i < playerStats.Count; i++)
            {
                playerParty.Add(new BattleMember(playerStats[i], false, i));
            }
            
            enemyParty = new List<BattleMember>();
            for (int i = 0; i < enemyStats.Count; i++)
            {
                enemyParty.Add(new BattleMember(enemyStats[i], true, i));
            }
            
            // Register showtimes if available
            if (availableShowtimes != null && availableShowtimes.Count > 0)
            {
                foreach (var showtime in availableShowtimes)
                {
                    showtimeManager.RegisterShowtime(showtime);
                }
            }
            
            // Register limit breaks if available
            if (availableLimitBreaks != null && availableLimitBreaks.Count > 0)
            {
                foreach (var limitBreak in availableLimitBreaks)
                {
                    limitBreakSystem.RegisterLimitBreak(limitBreak);
                }
            }
            
            // Calculate turn order
            CalculateTurnOrder();
            
            TrackEnemyEncounters();
            
            // Reset rewards tracking
            rewardsManager.ResetMetrics();
            
            CurrentPhase = BattlePhase.NotStarted;
            currentRound = 0;
            
            GD.Print("═══════════════════════════════════════");
            GD.Print("         BATTLE START!");
            GD.Print("═══════════════════════════════════════\n");
            
            EmitSignal(SignalName.BattleStarted);
            StartNextTurn();
        }
        
        /// <summary>
        /// Calculate turn order based on speed
        /// </summary>
        private void CalculateTurnOrder()
        {
            turnOrder = new List<BattleMember>();
            turnOrder.AddRange(playerParty);
            turnOrder.AddRange(enemyParty);
            
            // Sort by speed (descending)
            turnOrder = turnOrder.OrderByDescending(m => m.Stats.Speed).ToList();
            
            GD.Print("Turn Order:");
            foreach (var member in turnOrder)
            {
                GD.Print($"  {member.Stats.CharacterName} (Speed: {member.Stats.Speed})");
            }
        }
        
        #endregion
        
        #region Turn Management
        
        /// <summary>
        /// Start the next character's turn
        /// </summary>
        public void StartNextTurn()
        {
            // Process guard HP/MP regeneration at start of turn
            foreach (var member in playerParty.Concat(enemyParty))
            {
                guardSystem.ProcessGuardEffects(member);
            }
            
            // Get next actor
            CurrentActor = GetNextActor();
            
            if (CurrentActor == null)
            {
                // Round complete
                EndRound();
                return;
            }
            
            // Process status effects at turn start
            statusManager.ProcessStatusEffects(CurrentActor.Stats, true);
            
            // Check if actor can act
            if (!CurrentActor.CanAct())
            {
                GD.Print($"{CurrentActor.Stats.CharacterName} cannot act (stunned/sleeping)");
                CurrentActor.EndTurn();
                StartNextTurn();
                return;
            }
            
            // Determine battle phase
            CurrentPhase = CurrentActor.IsPlayerControlled ? 
                BattlePhase.PlayerAction : BattlePhase.EnemyAction;
            
            GD.Print($"\n--- {CurrentActor.Stats.CharacterName}'s Turn ---");
            
            // Check for limit break ready
            if (CurrentActor.IsLimitBreakReady)
            {
                EmitSignal(SignalName.LimitBreakReady, CurrentActor.Stats.CharacterName);
            }
            
            EmitSignal(SignalName.TurnStarted, CurrentActor.Stats.CharacterName);
        }
        
        /// <summary>
        /// End the current round and start a new one
        /// </summary>
        private void EndRound()
        {
            currentRound++;
            GD.Print($"\n═══ Round {currentRound} Complete ═══\n");
            
            // Reset all turn flags
            foreach (var member in turnOrder)
            {
                member.StartRound();
            }
            
            // Reset baton pass
            batonPassManager.ResetForNewRound();
            
            // Stand up knocked down members
            foreach (var member in turnOrder)
            {
                if (member.IsKnockedDown)
                {
                    member.StandUp();
                }
            }
            
            // Process end-of-round status effects
            foreach (var member in turnOrder.Where(m => m.Stats.IsAlive))
            {
                statusManager.ProcessStatusEffects(member.Stats, false);
            }
            
            // Update showtime cooldowns
            showtimeManager.IncrementTurn();
            
            // Start next round
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
                guardSystem.ClearGuard(action.Actor);
            }
            
            // Track for rewards
            bool isPlayer = playerParty.Contains(action.Actor);
            if (result.DamageDealt > 0)
            {
                rewardsManager.RecordDamage(result.DamageDealt, isPlayer);
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
                    StartNextTurn();
                }
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
                    GD.Print($"  MISS!");
                    continue;
                }
                
                // Calculate damage
                int damage = CalculatePhysicalDamage(action.Actor, targetMember);
                
                // Check critical
                bool isCritical = attacker.BattleStats.RollCritical(attacker.BattleStats.CriticalRate, rng);
                if (isCritical)
                {
                    damage = Mathf.RoundToInt(damage * 2.0f);
                    result.WasCritical = true;
                    rewardsManager.RecordEvent("critical_hit");
                }
                
                // Apply guard reduction
                damage = guardSystem.ApplyGuardReduction(targetMember, damage);
                
                // Deal damage
                int actualDamage = target.TakeDamage(damage, ElementType.Physical);
                result.DamageDealt += actualDamage;
                
                GD.Print($"  {attacker.CharacterName} → {target.CharacterName}: {actualDamage} damage" +
                    (isCritical ? " CRITICAL!" : ""));
                
                // Build limit gauge
                limitBreakSystem.AddGaugeFromDamage(action.Actor, actualDamage);
                
                // Track KO
                if (!target.IsAlive)
                {
                    if (playerParty.Any(p => p.Stats == target))
                    {
                        rewardsManager.RecordEvent("character_ko");
                    }
                    else if (enemyParty.Any(e => e.Stats == target))
                    {
                        BestiaryManager.Instance?.RecordDefeat(target.CharacterId);
                    }
                }
            }
            
            result.Success = true;
            return result;
        }
        
        /// <summary>
        /// Execute a skill
        /// </summary>
        private BattleActionResult ExecuteSkill(BattleAction action)
        {
            var skill = action.Skill;
            var attacker = action.Actor.Stats;
            var result = new BattleActionResult();
            
            // Check MP cost
            if (attacker.CurrentMP < skill.MPCost)
            {
                result.Success = false;
                result.Message = "Not enough MP!";
                return result;
            }
            
            // Consume MP
            attacker.ConsumeMP(skill.MPCost);
            
            GD.Print($"{attacker.CharacterName} uses {skill.DisplayName}!");
            
            foreach (var targetMember in action.Targets)
            {
                var target = targetMember.Stats;
                
                // Healing skills (check SkillCategory or similar property instead)
                if (skill.BasePower < 0 || skill.DisplayName.ToLower().Contains("heal"))
                {
                    int healing = CalculateHealing(action.Actor, targetMember, skill);
                    
                    // Apply baton pass bonus
                    healing = batonPassManager.ApplyHealingBonus(action.Actor, healing);
                    
                    int actualHealing = target.Heal(healing);
                    result.HealingDone += actualHealing;
                    GD.Print($"  {target.CharacterName} healed for {actualHealing} HP!");
                    continue;
                }
                
                // Check hit
                bool hits = attacker.BattleStats.RollHit(skill.Accuracy, rng);
                if (!hits)
                {
                    result.Missed = true;
                    GD.Print($"  MISS!");
                    continue;
                }
                
                // Calculate damage
                int damage = CalculateMagicDamage(action.Actor, targetMember, skill);
                
                // Check for technical damage
                var technicalResult = technicalSystem.CheckTechnical(targetMember.Stats, skill.Element);
                if (technicalResult.IsTechnical)
                {
                    damage = Mathf.RoundToInt(damage * 1.5f); // Technical multiplier
                    
                    EmitSignal(SignalName.TechnicalDamage,
                        attacker.CharacterName,
                        target.CharacterName,
                        technicalResult.ComboType.ToString());
                    
                    rewardsManager.RecordEvent("technical_hit");
                    
                    GD.Print($"  ⚡ TECHNICAL! ({technicalResult.ComboType})");
                }
                
                // Apply baton pass bonus
                damage = batonPassManager.ApplyDamageBonus(action.Actor, damage);
                
                // Check critical
                int critBonus = batonPassManager.GetCriticalBonus(action.Actor);
                bool isCritical = attacker.BattleStats.RollCritical(
                    attacker.BattleStats.CriticalRate + critBonus, rng);
                
                if (isCritical)
                {
                    damage = Mathf.RoundToInt(damage * 2.0f);
                    result.WasCritical = true;
                    rewardsManager.RecordEvent("critical_hit");
                }
                
                // Check weakness
                var affinity = target.ElementAffinities.GetAffinity(skill.Element);
                result.HitWeakness = affinity == ElementAffinity.Weak;
                
                if (result.HitWeakness)
                {
                    damage = Mathf.RoundToInt(damage * 1.5f);
                    EmitSignal(SignalName.WeaknessHit, attacker.CharacterName, target.CharacterName);
                    rewardsManager.RecordEvent("weakness_hit");
                    GD.Print($"  ★ WEAKNESS!");
                }
                else if (affinity == ElementAffinity.Resist)
                {
                    damage = Mathf.RoundToInt(damage * 0.5f);
                    GD.Print($"  Resisted...");
                }
                else if (affinity == ElementAffinity.Immune)
                {
                    damage = 0;
                    GD.Print($"  Nullified!");
                }
                else if (affinity == ElementAffinity.Absorb)
                {
                    target.Heal(damage);
                    GD.Print($"  Absorbed!");
                    continue;
                }
                
                // Apply guard reduction
                damage = guardSystem.ApplyGuardReduction(targetMember, damage);
                
                // Deal damage
                int actualDamage = target.TakeDamage(damage, skill.Element);
                result.DamageDealt += actualDamage;
                
                GD.Print($"  {target.CharacterName} took {actualDamage} damage" +
                    (isCritical ? " CRITICAL!" : ""));
                
                // Build limit gauge
                limitBreakSystem.AddGaugeFromDamage(action.Actor, actualDamage);
                
                // Apply status effects
                if (skill.InflictsStatuses != null && skill.InflictsStatuses.Count > 0)
                {
                    for (int i = 0; i < skill.InflictsStatuses.Count; i++)
                    {
                        var status = skill.InflictsStatuses[i];
                        int chance = skill.StatusChances != null && i < skill.StatusChances.Count ?
                            skill.StatusChances[i] : 30;
                        
                        statusManager.ApplyStatus(targetMember.Stats, status, 3, 5, attacker.CharacterName);
                    }
                }
                
                // Track KO
                if (!target.IsAlive)
                {
                    if (playerParty.Any(p => p.Stats == target))
                    {
                        rewardsManager.RecordEvent("character_ko");
                    }
                    else if (enemyParty.Any(e => e.Stats == target))
                    {
                        BestiaryManager.Instance?.RecordDefeat(target.CharacterId);
                    }
                }
            }
            
            result.Success = true;
            return result;
        }
        
        /// <summary>
        /// Execute guard action
        /// </summary>
        private BattleActionResult ExecuteGuard(BattleAction action)
        {
            var result = guardSystem.ExecuteGuard(action.Actor);
            
            // Guarding builds limit gauge
            limitBreakSystem.AddGaugeFromDealingDamage(action.Actor, 0, false);
            
            return result;
        }
        
        /// <summary>
        /// Execute item usage
        /// </summary>
        private BattleActionResult ExecuteItem(BattleAction action)
        {
            var item = action.ItemData as EchoesAcrossTime.Items.ConsumableData;
            
            if (item == null)
            {
                return new BattleActionResult
                {
                    Success = false,
                    Message = "No item selected!"
                };
            }
            
            // Use item via item system
            var result = itemSystem.UseItem(action.Actor, item, action.Targets);
            
            // Track item usage
            rewardsManager.RecordEvent("item_used");
            
            // Note: Inventory deduction should be handled by caller
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
                CurrentPhase = BattlePhase.Escaped;
                EmitSignal(SignalName.BattleEnded, false);
            }
            
            return result;
        }
        
        /// <summary>
        /// Execute All-Out Attack
        /// </summary>
        private BattleActionResult ExecuteAllOutAttack(BattleAction action)
        {
            var result = new BattleActionResult();
            
            GD.Print("\n╔═══════════════════════════════════════╗");
            GD.Print("║       ★ ALL-OUT ATTACK! ★          ║");
            GD.Print("╚═══════════════════════════════════════╝\n");
            
            rewardsManager.RecordEvent("all_out_attack");
            
            // All party members attack knocked down enemies
            foreach (var playerMember in playerParty.Where(p => p.Stats.IsAlive))
            {
                foreach (var enemyMember in enemyParty.Where(e => e.IsKnockedDown))
                {
                    var enemy = enemyMember.Stats;
                    
                    int damage = playerMember.Stats.Attack * 2;
                    int actualDamage = enemy.TakeDamage(damage, ElementType.Physical);
                    result.DamageDealt += actualDamage;
                    
                    GD.Print($"  {playerMember.Stats.CharacterName} → {enemy.CharacterName}: {actualDamage} damage!");
                    
                    enemyMember.StandUp();
                }
            }
            
            AllOutAttackAvailable = false;
            result.Success = true;
            result.Message = "All-Out Attack finished!";
            
            return result;
        }
        
        /// <summary>
        /// Execute Limit Break
        /// </summary>
        private BattleActionResult ExecuteLimitBreak(BattleAction action)
        {
            var limitBreak = action.LimitBreak;
            var targets = action.Targets.ToList();
            bool isDuo = action.DuoPartner != null;
            
            rewardsManager.RecordEvent("limit_break");
            
            EmitSignal(SignalName.LimitBreakUsed,
                action.Actor.Stats.CharacterName,
                limitBreak.DisplayName,
                isDuo);
            
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
        
        #endregion
        
        #region Damage Calculation
        
        /// <summary>
        /// Calculate physical damage
        /// </summary>
        private int CalculatePhysicalDamage(BattleMember attacker, BattleMember target)
        {
            float attackPower = attacker.Stats.Attack;
            float defense = target.Stats.Defense;
            
            float baseDamage = (attackPower * attackPower) / (defense + attackPower);
            baseDamage *= 10;
            
            return Mathf.Max(1, Mathf.RoundToInt(baseDamage));
        }
        
        /// <summary>
        /// Calculate magic damage
        /// </summary>
        private int CalculateMagicDamage(BattleMember attacker, BattleMember target, SkillData skill)
        {
            float magicPower = attacker.Stats.MagicAttack;
            float magicDefense = target.Stats.MagicDefense;
            
            float baseDamage = (magicPower * magicPower) / (magicDefense + magicPower);
            baseDamage *= (skill.BasePower / 100.0f);
            
            return Mathf.Max(1, Mathf.RoundToInt(baseDamage));
        }
        
        /// <summary>
        /// Calculate healing
        /// </summary>
        private int CalculateHealing(BattleMember caster, BattleMember target, SkillData skill)
        {
            float magicPower = caster.Stats.MagicAttack;
            float baseHealing = magicPower * (Mathf.Abs(skill.BasePower) / 100.0f);
            
            return Mathf.Max(1, Mathf.RoundToInt(baseHealing));
        }
        
        #endregion
        
        #region Advanced Mechanics
        
        /// <summary>
        /// Handle One More system
        /// </summary>
        private void HandleOneMore(BattleMember actor, BattleActionResult result)
        {
            if (!actor.HasExtraTurn)
            {
                actor.HasExtraTurn = true;
                EmitSignal(SignalName.OneMoreTriggered, actor.Stats.CharacterName);
                GD.Print($"\n*** {actor.Stats.CharacterName} gets ONE MORE! ***\n");
            }
        }
        
        /// <summary>
        /// Check if baton pass is available
        /// </summary>
        public bool CanBatonPass()
        {
            if (CurrentActor == null) return false;
            return batonPassManager.CanBatonPass(CurrentActor);
        }
        
        /// <summary>
        /// Get valid baton pass targets
        /// </summary>
        public List<BattleMember> GetBatonPassTargets()
        {
            if (CurrentActor == null) return new List<BattleMember>();
            
            var targets = new List<BattleMember>();
            var allies = CurrentActor.IsPlayerControlled ? playerParty : enemyParty;
            
            foreach (var ally in allies)
            {
                if (batonPassManager.CanReceiveBatonPass(CurrentActor, ally))
                {
                    targets.Add(ally);
                }
            }
            
            return targets;
        }
        
        /// <summary>
        /// Execute baton pass
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
                
                // Switch current actor
                CurrentActor = target;
                EmitSignal(SignalName.TurnStarted, CurrentActor.Stats.CharacterName);
            }
            
            return success;
        }
        
        /// <summary>
        /// Get all available showtime attacks
        /// </summary>
        public List<ShowtimeAttackData> GetAvailableShowtimes()
        {
            var showtimes = new List<ShowtimeAttackData>();
    
            if (showtimeManager == null)  // ← Changed from ShowtimeSystem.Instance
                return showtimes;
    
            var livingAllies = GetLivingAllies();
    
            // Get available showtimes from manager
            var availableShowtimes = showtimeManager.GetAvailableShowtimes();  // ← Changed
    
            foreach (var showtime in availableShowtimes)
            {
                var char1 = livingAllies.FirstOrDefault(a => a.Stats.CharacterName == showtime.Character1Id);
                var char2 = livingAllies.FirstOrDefault(a => a.Stats.CharacterName == showtime.Character2Id);
        
                if (char1 != null && char2 != null)
                {
                    if (showtimeManager.CanShowtimeActivate(showtime, char1, char2))  // ← Changed
                    {
                        showtimes.Add(showtime);
                    }
                }
            }
    
            return showtimes;
        }
        
        /// <summary>
        /// Get all available limit breaks for a character
        /// </summary>
        public List<LimitBreakData> GetAvailableLimitBreaks(BattleMember member)
        {
            var limitBreaks = new List<LimitBreakData>();
    
            if (member == null || !member.IsLimitBreakReady)
                return limitBreaks;
    
            // Use internal limitBreakSystem field
            if (limitBreakSystem != null)  // ← Changed from LimitBreakSystem.Instance
            {
                var limitBreak = limitBreakSystem.GetLimitBreak(member.Stats.CharacterName);  // ← Changed
                if (limitBreak != null)
                {
                    limitBreaks.Add(limitBreak);
                }
            }
    
            return limitBreaks;
        }
        
        /// <summary>
        /// Execute a showtime attack
        /// </summary>
        public void ExecuteShowtime(ShowtimeAttackData showtime)
        {
            if (showtime == null || showtimeManager == null)  // ← Changed from ShowtimeSystem.Instance
            {
                GD.PrintErr("Invalid showtime data!");
                return;
            }
    
            var livingAllies = GetLivingAllies();
            var char1 = livingAllies.FirstOrDefault(a => a.Stats.CharacterName == showtime.Character1Id);
            var char2 = livingAllies.FirstOrDefault(a => a.Stats.CharacterName == showtime.Character2Id);
    
            if (char1 == null || char2 == null)
            {
                GD.PrintErr("Showtime characters not found!");
                return;
            }
    
            // Get targets
            var targets = showtime.HitsAllEnemies  // ← Changed from HitsAllTargets
                ? GetLivingEnemies()
                : new List<BattleMember> { GetLivingEnemies().FirstOrDefault() };
    
            if (targets.Count == 0 || targets[0] == null)
            {
                GD.Print("No valid targets for showtime!");
                return;
            }
    
            // Execute using internal manager
            var result = showtimeManager.ExecuteShowtime(showtime, char1, char2, targets);  // ← Changed
    
            // Emit signal
            EmitSignal(SignalName.ShowtimeTriggered, showtime.AttackName, char1.Stats.CharacterName, char2.Stats.CharacterName);
    
            // Put showtime on cooldown
            showtimeManager.PutOnCooldown(showtime);  // ← Changed
    
            GD.Print($"Showtime complete! Total damage: {result.DamageDealt}");
        }
        
        /// <summary>
        /// Check if limit break is ready for a character
        /// </summary>
        public bool IsLimitBreakReady(BattleMember member)
        {
            if (member == null)
                return false;
            
            return member.IsLimitBreakReady;
        }
        
        /// <summary>
        /// Get limit gauge percentage for UI display
        /// </summary>
        public float GetLimitGaugePercent(BattleMember member)
        {
            if (member == null)
                return 0f;
            
            return (member.LimitGauge / 100f) * 100f; // Convert to percentage
        }
        
        /// <summary>
        /// Check if can escape
        /// </summary>
        public bool CanEscape()
        {
            // Cannot escape if blocked (boss battle, story battle, etc.)
            if (escapeBlocked)
                return false;
            
            // Cannot escape if all party members are dead
            if (!GetPlayerParty().Any(p => p.Stats.IsAlive))
                return false;
            
            // Can always attempt escape in normal battles
            return true;
        }
        
        /// <summary>
        /// Get escape chance percentage
        /// </summary>
        public int GetEscapeChance()
        {
            if (!CanEscape())
                return 0;
            
            // Base chance
            int chance = 25;
            
            // +10% per failed attempt
            chance += escapeAttempts * 10;
            
            // Speed difference bonus
            var livingAllies = GetLivingAllies();
            var livingEnemies = GetLivingEnemies();
            
            if (livingAllies.Count > 0 && livingEnemies.Count > 0)
            {
                int avgPartySpeed = (int)livingAllies.Average(a => a.Stats.Speed);
                int avgEnemySpeed = (int)livingEnemies.Average(e => e.Stats.Speed);
                
                int speedDiff = avgPartySpeed - avgEnemySpeed;
                chance += speedDiff / 2; // +1% per 2 speed difference
            }
            
            // Cap at 95%
            return Mathf.Min(95, chance);
        }
        
        /// <summary>
        /// Attempt to escape from battle
        /// </summary>
        public bool TryEscape()
        {
            if (!CanEscape())
            {
                GD.Print("Cannot escape from this battle!");
                return false;
            }
            
            int chance = GetEscapeChance();
            int roll = (int)(GD.Randf() * 100);
            
            if (roll < chance)
            {
                GD.Print($"✓ Escape successful! (rolled {roll} vs {chance}%)");
                EmitSignal(SignalName.BattleEnded, false); // false = no victory
                return true;
            }
            else
            {
                escapeAttempts++;
                GD.Print($"✗ Escape failed! (rolled {roll} vs {chance}%)");
                GD.Print($"  Next attempt: {GetEscapeChance()}% chance");
                return false;
            }
        }
        
        /// <summary>
        /// Block escape attempts (for boss battles, etc.)
        /// </summary>
        public void SetEscapeBlocked(bool blocked)
        {
            escapeBlocked = blocked;
            if (blocked)
            {
                GD.Print("⚠ Escape is BLOCKED for this battle!");
            }
        }
        
        
        
        #endregion
        
        #region Battle State Queries
        
        /// <summary>
        /// Check if all enemies are knocked down
        /// </summary>
        private bool AreAllEnemiesKnockedDown()
        {
            return enemyParty.All(e => !e.Stats.IsAlive || e.IsKnockedDown);
        }
        
        /// <summary>
        /// Check if battle has ended
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
                
                // Calculate and distribute rewards!
                var defeatedEnemies = enemyParty.ToList();
                var partyStats = playerParty.Select(p => p.Stats).ToList();
                
                var rewards = rewardsManager.CalculateRewards(
                    partyStats,
                    defeatedEnemies,
                    isBossBattle
                );
                
                // Log rewards
                GD.Print("\n╔═══════════════════════════════════════╗");
                GD.Print("║          VICTORY!                    ║");
                GD.Print("╚═══════════════════════════════════════╝");
                GD.Print($"\n⭐ Battle Rank: {BattleRewardsManager.GetRankDisplay(rewards.Rank)}");
                GD.Print($"💰 Gold: {rewards.TotalGold}");
                GD.Print($"✨ EXP: {rewards.TotalExp}");
                
                if (rewards.ItemDrops.Count > 0)
                {
                    GD.Print($"🎁 Items:");
                    foreach (var drop in rewards.ItemDrops)
                    {
                        GD.Print($"   • {drop.itemId} x{drop.quantity}");
                    }
                }
                
                EmitSignal(SignalName.BattleEnded, true);
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
        /// Get current battle metrics (for UI display)
        /// </summary>
        public BattleMetrics GetCurrentMetrics()
        {
            return rewardsManager.GetMetrics();
        }
        
        /// <summary>
        /// Get predicted battle rank based on current performance
        /// </summary>
        public BattleRank GetPredictedRank()
        {
            var metrics = rewardsManager.GetMetrics();
            int score = 0;
            
            score += metrics.WeaknessHits * 200;
            score += metrics.CriticalHits * 150;
            score += metrics.TechnicalHits * 300;
            score += metrics.AllOutAttacks * 500;
            score += metrics.ShowtimeAttacks * 800;
            score += metrics.LimitBreaksUsed * 600;
            
            if (metrics.NoDamageTaken) score += 2000;
            if (metrics.NoKOs) score += 1500;
            if (metrics.NoItemsUsed) score += 1000;
            
            score -= metrics.CharactersKO * 500;
            
            if (score >= 10000) return BattleRank.SPlus;
            if (score >= 7000) return BattleRank.S;
            if (score >= 5000) return BattleRank.A;
            if (score >= 3000) return BattleRank.B;
            if (score >= 1500) return BattleRank.C;
            if (score >= 500) return BattleRank.D;
            return BattleRank.F;
        }
        
        private void TrackEnemyEncounters()
        {
            if (BestiaryManager.Instance == null) return;
    
            foreach (var enemy in enemyParty)
            {
                if (enemy?.Stats != null)
                {
                    BestiaryManager.Instance.RecordEncounter(
                        enemy.Stats.CharacterId,
                        enemy.Stats.Level
                    );
                }
            }
        }
        #endregion
        
        #region Rewards Signal Handlers
        
        private void ConnectRewardSignals()
        {
            ActionExecuted += OnActionExecutedForRewards;
            WeaknessHit += OnWeaknessHitForRewards;
            TechnicalDamage += OnTechnicalForRewards;
            AllOutAttackReady += OnAllOutAttackForRewards;
            ShowtimeTriggered += OnShowtimeForRewards;
            LimitBreakUsed += OnLimitBreakForRewards;
        }
        
        private void OnActionExecutedForRewards(string actorName, string actionName, int damageDealt, bool hitWeakness, bool wasCritical)
        {
            if (wasCritical)
            {
                rewardsManager.RecordEvent("critical_hit");
            }
        }
        
        private void OnWeaknessHitForRewards(string attackerName, string targetName)
        {
            rewardsManager.RecordEvent("weakness_hit");
        }
        
        private void OnTechnicalForRewards(string attackerName, string targetName, string comboType)
        {
            rewardsManager.RecordEvent("technical_hit");
        }
        
        private void OnAllOutAttackForRewards()
        {
            rewardsManager.RecordEvent("all_out_attack");
        }
        
        private void OnShowtimeForRewards(string showtimeName, string char1, string char2)
        {
            rewardsManager.RecordEvent("showtime");
        }
        
        private void OnLimitBreakForRewards(string characterName, string limitBreakName, bool isDuo)
        {
            rewardsManager.RecordEvent("limit_break");
        }
        
        private void TrackDamageForRewards(string actorName, string actionName, int damage, bool weakness, bool crit)
        {
            if (rewardsManager == null) return;
            
            // Track damage
            bool isPlayer = playerParty?.Any(p => p.Stats.CharacterName == actorName) ?? false;
            if (damage > 0)
            {
                rewardsManager.RecordDamage(damage, isPlayer);
            }
            
            if (crit)
            {
                rewardsManager.RecordEvent("critical_hit");
            }
        }
        
        /// <summary>
        /// Call this when battle ends with victory
        /// </summary>
        private void OnBattleVictory()
        {
            GD.Print("=== BATTLE VICTORY ===");
        
            // Set result for event system
            if (EventCommandExecutor.Instance != null)
            {
                EventCommandExecutor.Instance.SetBattleResult(
                    EventCommandExecutor.BattleResult.Victory
                );
            }
        
            // Your existing victory code...
            // ShowVictoryScreen();
            // GiveRewards();
            // etc.
        
            EmitSignal(SignalName.BattleEnded, true);
        }
        
        /// <summary>
        /// Call this when battle ends with escape
        /// </summary>
        private void OnBattleEscape()
        {
            GD.Print("=== BATTLE ESCAPED ===");
        
            // Set result for event system
            if (EventCommandExecutor.Instance != null)
            {
                EventCommandExecutor.Instance.SetBattleResult(
                    EventCommandExecutor.BattleResult.Escape
                );
            }
        
            CurrentPhase = BattlePhase.Escaped;
            EmitSignal(SignalName.BattleEnded, false);
        }
        
        /// <summary>
        /// Call this when battle ends with defeat
        /// </summary>
        private void OnBattleDefeat()
        {
            GD.Print("=== BATTLE DEFEAT ===");
        
            // Set result for event system
            if (EventCommandExecutor.Instance != null)
            {
                EventCommandExecutor.Instance.SetBattleResult(
                    EventCommandExecutor.BattleResult.Defeat
                );
            }
            
            // Your existing defeat code...
            // ShowGameOverScreen();
            // etc.
        
            EmitSignal(SignalName.BattleEnded, false);
        }

        
        #endregion
    }
    
    /// <summary>
    /// Battle phase states
    /// </summary>
    public enum BattlePhase
    {
        NotStarted,
        PlayerAction,
        EnemyAction,
        Processing,
        AllOutAttackPrompt,
        Victory,
        Defeat,
        Escaped
    }
}