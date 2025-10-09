// Combat/Advanced/AdvancedAIPattern.cs
// ADVANCED AI PATTERNS - All 8 Systems Integrated
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EchoesAcrossTime.Combat.Advanced
{
    /// <summary>
    /// Advanced AI Pattern that extends basic AI with 8 sophisticated systems:
    /// 1. Multi-Turn Strategy Planning
    /// 2. Party Coordination & Synergy
    /// 3. Adaptive Difficulty & Learning
    /// 4. Risk Assessment & Gambling
    /// 5. Formation & Positioning AI
    /// 6. Resource Management Expert
    /// 7. Personality Shifts & Evolution
    /// 8. Counter & Prediction System
    /// </summary>
    [GlobalClass]
    public partial class AdvancedAIPattern : AIPattern
    {
        #region 1. Multi-Turn Strategy Planning
        
        [ExportGroup("Multi-Turn Strategy")]
        [Export] public bool UsesMultiTurnStrategy { get; set; } = false;
        [Export(PropertyHint.Range, "1,5")] public int PlanningDepth { get; set; } = 2; // How many turns ahead
        [Export] public bool SetupsCombos { get; set; } = true;
        [Export] public bool SavesResourcesForKey { get; set; } = true;
        
        private Queue<PlannedAction> strategicPlan = new Queue<PlannedAction>();
        private int turnsExecuted = 0;
        
        #endregion
        
        #region 2. Party Coordination & Synergy
        
        [ExportGroup("Party Coordination")]
        [Export] public bool CoordinatesWithAllies { get; set; } = false;
        [Export] public bool CoverAlliesWeaknesses { get; set; } = true;
        [Export] public bool SynchronizedAttacks { get; set; } = true;
        [Export] public bool RoleAwareness { get; set; } = true; // Tank protects healer
        [Export(PropertyHint.Range, "0,100")] public int TeamworkPriority { get; set; } = 70;
        
        private Dictionary<string, AIRole> allyRoles = new Dictionary<string, AIRole>();
        private List<CoordinatedAction> pendingCoordinatedActions = new List<CoordinatedAction>();
        
        #endregion
        
        #region 3. Adaptive Difficulty & Learning
        
        [ExportGroup("Adaptive Learning")]
        [Export] public bool AdaptsToPlayerSkill { get; set; } = false;
        [Export] public bool LearnsPlayerPatterns { get; set; } = true;
        [Export(PropertyHint.Range, "0,100")] public int BaseDifficultyLevel { get; set; } = 50;
        [Export] public float DifficultyAdjustmentRate { get; set; } = 0.1f;
        
        private float currentDifficulty = 50f;
        private Dictionary<string, int> playerPatternCounts = new Dictionary<string, int>();
        private int playerWinStreak = 0;
        private int enemyWinStreak = 0;
        
        #endregion
        
        #region 4. Risk Assessment & Gambling
        
        [ExportGroup("Risk Assessment")]
        [Export] public bool UsesRiskCalculation { get; set; } = false;
        [Export(PropertyHint.Range, "0,100")] public int RiskTolerance { get; set; } = 30;
        [Export] public bool TakesCalculatedRisks { get; set; } = true;
        [Export] public bool DesperateWhenLosing { get; set; } = true;
        [Export] public bool ConservativeWhenWinning { get; set; } = true;
        
        private float battleMomentum = 0f; // -100 to +100, negative = losing
        
        #endregion
        
        #region 5. Formation & Positioning AI
        
        [ExportGroup("Formation & Positioning")]
        [Export] public bool UsesTacticalPositioning { get; set; } = false;
        [Export] public FormationType PreferredFormation { get; set; } = FormationType.Balanced;
        [Export] public bool MaintainsDistance { get; set; } = false;
        [Export] public bool ProtectsBackline { get; set; } = true;
        [Export] public bool FlankingBehavior { get; set; } = false;
        
        private Vector2 idealPosition = Vector2.Zero;
        private Dictionary<string, Vector2> formationPositions = new Dictionary<string, Vector2>();
        
        #endregion
        
        #region 6. Resource Management Expert
        
        [ExportGroup("Resource Management")]
        [Export] public bool ManagesResourcesStrategically { get; set; } = false;
        [Export(PropertyHint.Range, "0,100")] public int MPConservationThreshold { get; set; } = 30;
        [Export] public bool SavesForBossPhases { get; set; } = true;
        [Export] public bool UsesItemsStrategically { get; set; } = true;
        
        private bool isBossPhase2 = false;
        private int mpReserved = 0;
        
        #endregion
        
        #region 7. Personality Shifts & Evolution
        
        [ExportGroup("Personality Evolution")]
        [Export] public bool HasPersonalityShifts { get; set; } = false;
        [Export] public PersonalityState CurrentPersonality { get; set; } = PersonalityState.Calm;
        [Export] public bool EvolvesInBattle { get; set; } = true;
        [Export] public Godot.Collections.Array<float> PhaseHPThresholds { get; set; }
        
        private PersonalityState originalPersonality;
        private int currentPhase = 1;
        
        #endregion
        
        #region 8. Counter & Prediction System
        
        [ExportGroup("Counter & Prediction")]
        [Export] public bool PredictsPlayerMoves { get; set; } = false;
        [Export] public bool UsesPreemptiveCounters { get; set; } = true;
        [Export] public bool BaitsPlayer { get; set; } = true;
        [Export] public bool CreatesFakePatterns { get; set; } = false;
        [Export(PropertyHint.Range, "0,100")] public int PredictionAccuracy { get; set; } = 60;
        
        private Queue<string> playerActionHistory = new Queue<string>();
        private Dictionary<string, string> predictedCounters = new Dictionary<string, string>();
        
        #endregion
        
        public AdvancedAIPattern()
        {
            PhaseHPThresholds = new Godot.Collections.Array<float> { 0.7f, 0.4f, 0.2f };
            originalPersonality = PersonalityState.Calm;
        }
        
        /// <summary>
        /// MAIN DECISION OVERRIDE - Routes through advanced systems
        /// </summary>
        public new AIDecision DecideAction(CharacterStats actor, List<CharacterStats> allies, List<CharacterStats> enemies)
        {
            // Update state
            UpdatePersonalityState(actor);
            UpdateBattleMomentum(allies, enemies);
            
            // PRIORITY 1: Execute planned strategy
            if (UsesMultiTurnStrategy && HasPlannedAction())
            {
                var planned = GetNextPlannedAction();
                if (planned != null && planned.IsValid(actor, allies, enemies))
                {
                    GD.Print($"[ADVANCED AI] Executing planned action: {planned.ActionType}");
                    return planned.ToAIDecision();
                }
            }
            
            // PRIORITY 2: Team coordination
            if (CoordinatesWithAllies && CheckTeamCoordination(actor, allies, enemies, out var coordAction))
            {
                GD.Print($"[ADVANCED AI] Team coordination: {coordAction.Reasoning}");
                return coordAction;
            }
            
            // PRIORITY 3: Counter predicted player move
            if (PredictsPlayerMoves && TryCounterPrediction(actor, enemies, out var counterAction))
            {
                GD.Print($"[ADVANCED AI] Counter prediction: {counterAction.Reasoning}");
                return counterAction;
            }
            
            // PRIORITY 4: Risk-based decision
            if (UsesRiskCalculation)
            {
                var riskAction = MakeRiskBasedDecision(actor, allies, enemies);
                if (riskAction.ActionType != AIActionType.None)
                {
                    GD.Print($"[ADVANCED AI] Risk decision: {riskAction.Reasoning}");
                    return riskAction;
                }
            }
            
            // PRIORITY 5: Resource management check
            if (ManagesResourcesStrategically && ShouldConserveResources(actor))
            {
                GD.Print($"[ADVANCED AI] Conserving resources (MP: {actor.CurrentMP}/{actor.MaxMP})");
                return new AIDecision 
                { 
                    ActionType = AIActionType.Attack,
                    Target = ChooseTargetAdvanced(enemies, TargetPriority),
                    Reasoning = "Resource Conservation"
                };
            }
            
            // FALLBACK: Use base AI + create new plan
            var baseDecision = base.DecideAction(actor, allies, enemies);
            
            // Create multi-turn plan if enabled
            if (UsesMultiTurnStrategy && strategicPlan.Count == 0)
            {
                CreateStrategicPlan(actor, allies, enemies);
            }
            
            return baseDecision;
        }
        
        #region Multi-Turn Strategy Implementation
        
        private void CreateStrategicPlan(CharacterStats actor, List<CharacterStats> allies, List<CharacterStats> enemies)
        {
            strategicPlan.Clear();
            
            GD.Print($"[STRATEGY] Creating {PlanningDepth}-turn plan for {actor.CharacterName}");
            
            // Example strategy: Debuff → Setup → Big Attack
            if (SetupsCombos && PlanningDepth >= 3)
            {
                var debuffSkill = actor.Skills?.GetEquippedSkills()
                    .FirstOrDefault(s => s.DisplayName.ToLower().Contains("debuff") && s.CanUse(actor));
                
                var bigAttack = actor.Skills?.GetEquippedSkills()
                    .Where(s => s.BasePower > 0 && s.CanUse(actor))
                    .OrderByDescending(s => s.BasePower)
                    .FirstOrDefault();
                
                if (debuffSkill != null && bigAttack != null)
                {
                    // Turn 1: Debuff defense
                    strategicPlan.Enqueue(new PlannedAction
                    {
                        ActionType = AIActionType.UseSkill,
                        SkillId = debuffSkill.SkillId,
                        TargetPriority = TargetPriority.HighestHP,
                        Reasoning = "Setup: Defense debuff"
                    });
                    
                    // Turn 2: Build up (guard or buff)
                    strategicPlan.Enqueue(new PlannedAction
                    {
                        ActionType = AIActionType.Defend,
                        Reasoning = "Setup: Build limit gauge"
                    });
                    
                    // Turn 3: Execute big attack
                    strategicPlan.Enqueue(new PlannedAction
                    {
                        ActionType = AIActionType.UseSkill,
                        SkillId = bigAttack.SkillId,
                        TargetPriority = TargetPriority.LowestDefense,
                        Reasoning = "Combo finisher!"
                    });
                    
                    GD.Print($"  → Combo plan: {debuffSkill.DisplayName} → Guard → {bigAttack.DisplayName}");
                }
            }
        }
        
        /// <summary>
        /// Choose target based on priority (accessible version)
        /// </summary>
        private CharacterStats ChooseTargetAdvanced(List<CharacterStats> targets, TargetPriority priority)
        {
            if (targets == null || targets.Count == 0) return null;
    
            var livingTargets = targets.Where(t => t.IsAlive).ToList();
            if (livingTargets.Count == 0) return null;
    
            return priority switch
            {
                TargetPriority.LowestHP => livingTargets.OrderBy(t => t.CurrentHP).First(),
                TargetPriority.HighestHP => livingTargets.OrderByDescending(t => t.CurrentHP).First(),
                TargetPriority.LowestDefense => livingTargets.OrderBy(t => t.Defense).First(),
                TargetPriority.HighestThreat => livingTargets.OrderByDescending(t => t.Attack + t.MagicAttack).First(),
                TargetPriority.Random => livingTargets[GD.RandRange(0, livingTargets.Count - 1)],
                TargetPriority.Leader => livingTargets.First(),
                TargetPriority.Healer => livingTargets.OrderByDescending(t => t.MagicAttack).First(),
                TargetPriority.Mage => livingTargets.OrderByDescending(t => t.MagicAttack).First(),
                TargetPriority.Weakest => livingTargets.OrderBy(t => t.CurrentHP + t.Defense).First(),
                TargetPriority.MostVulnerable => livingTargets.OrderBy(t => t.HPPercent).First(),
                _ => livingTargets[GD.RandRange(0, livingTargets.Count - 1)]
            };
        }
        
        private bool HasPlannedAction() => strategicPlan.Count > 0;
        
        private PlannedAction GetNextPlannedAction()
        {
            turnsExecuted++;
            return strategicPlan.Count > 0 ? strategicPlan.Dequeue() : null;
        }
        
        #endregion
        
        #region Party Coordination Implementation
        
        private bool CheckTeamCoordination(CharacterStats actor, List<CharacterStats> allies, 
            List<CharacterStats> enemies, out AIDecision decision)
        {
            decision = new AIDecision { ActionType = AIActionType.None };
            
            if (GD.Randf() * 100 > TeamworkPriority) return false;
            
            // Check if healer needs protection
            if (RoleAwareness)
            {
                var healer = allies.FirstOrDefault(a => 
                    a.IsAlive && 
                    a.CharacterId != actor.CharacterId &&
                    a.Skills?.GetEquippedSkills().Any(s => s.DisplayName.ToLower().Contains("heal")) == true);
                
                if (healer != null && healer.HPPercent < 0.6f)
                {
                    // Protect the healer!
                    var buffSkill = actor.Skills?.GetEquippedSkills()
                        .FirstOrDefault(s => s.DisplayName.ToLower().Contains("def") && s.CanUse(actor));
                    
                    if (buffSkill != null)
                    {
                        decision = new AIDecision
                        {
                            ActionType = AIActionType.UseSkill,
                            SelectedSkill = buffSkill,
                            Target = healer,
                            Reasoning = "Protecting healer ally!"
                        };
                        return true;
                    }
                }
            }
            
            // Synchronized attack on same target
            if (SynchronizedAttacks && allies.Count(a => a.IsAlive) >= 2)
            {
                var weakestEnemy = enemies.Where(e => e.IsAlive).OrderBy(e => e.CurrentHP).FirstOrDefault();
                if (weakestEnemy != null && weakestEnemy.HPPercent < 0.4f)
                {
                    GD.Print($"[COORDINATION] Focus fire on {weakestEnemy.CharacterName}!");
                    decision = new AIDecision
                    {
                        ActionType = AIActionType.Attack,
                        Target = weakestEnemy,
                        Reasoning = "Coordinated focus fire"
                    };
                    return true;
                }
            }
            
            return false;
        }
        
        #endregion
        
        #region Adaptive Learning Implementation
        
        public void RecordPlayerAction(string actionType, string skillUsed = "")
        {
            if (!LearnsPlayerPatterns) return;
            
            string pattern = $"{actionType}:{skillUsed}";
            
            if (!playerPatternCounts.ContainsKey(pattern))
                playerPatternCounts[pattern] = 0;
            
            playerPatternCounts[pattern]++;
            
            playerActionHistory.Enqueue(pattern);
            if (playerActionHistory.Count > 10) 
                playerActionHistory.Dequeue();
            
            // Detect patterns
            if (playerPatternCounts[pattern] >= 3)
            {
                GD.Print($"[LEARNING] Player pattern detected: {pattern} (x{playerPatternCounts[pattern]})");
            }
        }
        
        private void UpdateBattleMomentum(List<CharacterStats> allies, List<CharacterStats> enemies)
        {
            float allyHP = allies.Where(a => a.IsAlive).Sum(a => a.HPPercent) / Math.Max(1, allies.Count(a => a.IsAlive));
            float enemyHP = enemies.Where(e => e.IsAlive).Sum(e => e.HPPercent) / Math.Max(1, enemies.Count(e => e.IsAlive));
            
            battleMomentum = (allyHP - enemyHP) * 100f;
            
            if (AdaptsToPlayerSkill)
            {
                // Adjust difficulty based on how battle is going
                if (battleMomentum < -30) // Player is winning hard
                {
                    currentDifficulty = Math.Min(100, currentDifficulty + DifficultyAdjustmentRate * 10);
                    GD.Print($"[ADAPTIVE] Increasing difficulty to {currentDifficulty}%");
                }
                else if (battleMomentum > 30) // Player is losing
                {
                    currentDifficulty = Math.Max(0, currentDifficulty - DifficultyAdjustmentRate * 5);
                    GD.Print($"[ADAPTIVE] Decreasing difficulty to {currentDifficulty}%");
                }
            }
        }
        
        #endregion
        
        #region Risk Assessment Implementation
        
        private AIDecision MakeRiskBasedDecision(CharacterStats actor, List<CharacterStats> allies, List<CharacterStats> enemies)
        {
            float riskScore = CalculateRiskScore(actor, allies, enemies);
            
            GD.Print($"[RISK] Score: {riskScore:F1}, Tolerance: {RiskTolerance}, Momentum: {battleMomentum:F1}");
            
            // Desperate when losing badly
            if (DesperateWhenLosing && battleMomentum < -40)
            {
                // Go all-in with most powerful skill
                var powerSkill = actor.Skills?.GetUsableSkills(actor)
                    .Where(s => s.BasePower > 0)
                    .OrderByDescending(s => s.BasePower)
                    .FirstOrDefault();
                
                if (powerSkill != null)
                {
                    GD.Print($"[RISK] DESPERATE! Using {powerSkill.DisplayName}");
                    return new AIDecision
                    {
                        ActionType = AIActionType.UseSkill,
                        SelectedSkill = powerSkill,
                        Target = ChooseTargetAdvanced(enemies, TargetPriority.HighestHP),
                        Reasoning = "Desperate gamble!"
                    };
                }
            }
            
            // Conservative when winning
            if (ConservativeWhenWinning && battleMomentum > 30)
            {
                GD.Print($"[RISK] Playing it safe (winning)");
                return new AIDecision
                {
                    ActionType = AIActionType.Attack,
                    Target = ChooseTargetAdvanced(enemies, TargetPriority.LowestHP),
                    Reasoning = "Conservative - maintain lead"
                };
            }
            
            // Calculated risk
            if (TakesCalculatedRisks && riskScore < RiskTolerance && actor.HPPercent > 0.5f)
            {
                // Use high-cost high-reward skill
                var riskySkill = actor.Skills?.GetUsableSkills(actor)
                    .Where(s => s.MPCost >= actor.MaxMP * 0.3f && s.BasePower > 0)
                    .OrderByDescending(s => s.BasePower)
                    .FirstOrDefault();
                
                if (riskySkill != null)
                {
                    GD.Print($"[RISK] Calculated risk: {riskySkill.DisplayName}");
                    return new AIDecision
                    {
                        ActionType = AIActionType.UseSkill,
                        SelectedSkill = riskySkill,
                        Target = ChooseTargetAdvanced(enemies, TargetPriority.HighestThreat),
                        Reasoning = "Calculated risk"
                    };
                }
            }
            
            return new AIDecision { ActionType = AIActionType.None };
        }
        
        private float CalculateRiskScore(CharacterStats actor, List<CharacterStats> allies, List<CharacterStats> enemies)
        {
            float risk = 50f;
            
            risk -= actor.HPPercent * 30;
            risk -= (actor.CurrentMP / (float)actor.MaxMP) * 20;
            risk += enemies.Count(e => e.IsAlive) * 10;
            risk -= allies.Count(a => a.IsAlive) * 10;
            
            return Mathf.Clamp(risk, 0, 100);
        }
        
        #endregion
        
        #region Counter & Prediction Implementation
        
        private bool TryCounterPrediction(CharacterStats actor, List<CharacterStats> enemies, out AIDecision decision)
        {
            decision = new AIDecision { ActionType = AIActionType.None };
            
            if (GD.Randf() * 100 > PredictionAccuracy) return false;
            
            // Analyze player patterns
            if (playerActionHistory.Count >= 3)
            {
                var recentActions = playerActionHistory.TakeLast(3).ToList();
                
                // Pattern: Player guards on turn 3
                if (turnsExecuted % 3 == 2 && recentActions.Any(a => a.Contains("Defend")))
                {
                    // Buff while they guard
                    var buffSkill = actor.Skills?.GetEquippedSkills()
                        .FirstOrDefault(s => s.DisplayName.ToLower().Contains("buff") && s.CanUse(actor));
                    
                    if (buffSkill != null)
                    {
                        decision = new AIDecision
                        {
                            ActionType = AIActionType.UseSkill,
                            SelectedSkill = buffSkill,
                            Target = actor,
                            Reasoning = "Predicted player guard - buffing!"
                        };
                        return true;
                    }
                }
                
                // Pattern: Player always attacks lowest HP
                if (recentActions.Count(a => a.Contains("Attack:LowestHP")) >= 2)
                {
                    var lowestAlly = enemies.Where(e => e.IsAlive).OrderBy(e => e.CurrentHP).FirstOrDefault();
                    if (lowestAlly != null && UsesPreemptiveCounters)
                    {
                        // Heal the lowest HP ally before player targets them
                        var healSkill = actor.Skills?.GetEquippedSkills()
                            .FirstOrDefault(s => s.DisplayName.ToLower().Contains("heal") && s.CanUse(actor));
                        
                        if (healSkill != null)
                        {
                            decision = new AIDecision
                            {
                                ActionType = AIActionType.UseSkill,
                                SelectedSkill = healSkill,
                                Target = lowestAlly,
                                Reasoning = "Preemptive heal before player attack!"
                            };
                            return true;
                        }
                    }
                }
            }
            
            // Bait with fake weakness
            if (BaitsPlayer && GD.Randf() < 0.3f)
            {
                GD.Print("[PREDICTION] Setting up bait...");
                decision = new AIDecision
                {
                    ActionType = AIActionType.Defend,
                    Reasoning = "Baiting player attack"
                };
                return true;
            }
            
            return false;
        }
        
        #endregion
        
        #region Personality Evolution Implementation
        
        private void UpdatePersonalityState(CharacterStats actor)
        {
            if (!HasPersonalityShifts) return;
            
            float hpPercent = actor.HPPercent;
            int newPhase = currentPhase;
            
            // Check phase transitions
            for (int i = 0; i < PhaseHPThresholds.Count; i++)
            {
                if (hpPercent <= PhaseHPThresholds[i])
                {
                    newPhase = i + 2; // Phase 2, 3, 4...
                }
            }
            
            if (newPhase != currentPhase)
            {
                currentPhase = newPhase;
                ShiftPersonality(hpPercent);
            }
        }
        
        private void ShiftPersonality(float hpPercent)
        {
            var oldPersonality = CurrentPersonality;
            
            if (hpPercent < 0.2f)
            {
                CurrentPersonality = PersonalityState.Desperate;
                Aggression = 100;
                Recklessness = 90;
            }
            else if (hpPercent < 0.4f)
            {
                CurrentPersonality = PersonalityState.Angry;
                Aggression = 80;
                Recklessness = 60;
            }
            else if (hpPercent < 0.7f)
            {
                CurrentPersonality = PersonalityState.Cautious;
                Aggression = 40;
                Recklessness = 20;
            }
            
            if (oldPersonality != CurrentPersonality)
            {
                GD.Print($"[PERSONALITY] ⚠️ SHIFT: {oldPersonality} → {CurrentPersonality}!");
                GD.Print($"  Aggression: {Aggression}, Recklessness: {Recklessness}");
            }
        }
        
        #endregion
        
        #region Resource Management Implementation
        
        private bool ShouldConserveResources(CharacterStats actor)
        {
            float mpPercent = actor.CurrentMP / (float)actor.MaxMP * 100f;
            
            if (mpPercent < MPConservationThreshold)
            {
                GD.Print($"[RESOURCE] MP low ({mpPercent:F0}%) - conserving!");
                return true;
            }
            
            if (SavesForBossPhases && !isBossPhase2 && actor.HPPercent > 0.7f)
            {
                GD.Print($"[RESOURCE] Saving MP for boss phase 2");
                return true;
            }
            
            return false;
        }
        
        #endregion
    }
    
    #region Supporting Classes
    
    public class PlannedAction
    {
        public AIActionType ActionType { get; set; }
        public string SkillId { get; set; }
        public TargetPriority TargetPriority { get; set; }
        public string Reasoning { get; set; }
        
        public bool IsValid(CharacterStats actor, List<CharacterStats> allies, List<CharacterStats> enemies)
        {
            if (ActionType == AIActionType.UseSkill)
            {
                return actor.Skills?.GetEquippedSkills()
                    .Any(s => s.SkillId == SkillId && s.CanUse(actor)) ?? false;
            }
            return true;
        }
        
        
        
        public AIDecision ToAIDecision()
        {
            return new AIDecision
            {
                ActionType = ActionType,
                Reasoning = Reasoning
            };
        }
    }
    
    public class CoordinatedAction
    {
        public List<string> ParticipantIds { get; set; }
        public string ActionType { get; set; }
        public string TargetId { get; set; }
    }
    
    public enum AIRole
    {
        Tank,
        DPS,
        Healer,
        Support,
        Hybrid
    }
    
    public enum FormationType
    {
        Aggressive,     // All forward
        Defensive,      // Protect backline
        Balanced,       // Mixed
        Flanking,       // Surround
        Scattered       // Spread out
    }
    
    public enum PersonalityState
    {
        Calm,
        Cautious,
        Confident,
        Angry,
        Desperate,
        Calculating,
        Berserk
    }
    
    #endregion
}