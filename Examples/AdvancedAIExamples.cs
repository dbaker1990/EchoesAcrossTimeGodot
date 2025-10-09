// Examples/AdvancedAIExamples.cs
// Pre-configured advanced AI enemies showcasing all 8 systems
using Godot;
using EchoesAcrossTime.Combat;
using EchoesAcrossTime.Combat.Advanced;

namespace EchoesAcrossTime.Examples
{
    public static class AdvancedAIExamples
    {
        /// <summary>
        /// Example 1: MASTER STRATEGIST
        /// Uses: Multi-Turn Strategy + Resource Management + Counter System
        /// Perfect for chess-like boss battles
        /// </summary>
        public static AdvancedAIPattern CreateMasterStrategist()
        {
            var ai = new AdvancedAIPattern
            {
                // Base behavior
                BehaviorType = AIBehaviorType.Tactical,
                TargetPriority = TargetPriority.MostVulnerable,
                Aggression = 60,
                SkillUsageRate = 80,
                
                // ═══ MULTI-TURN STRATEGY ═══
                UsesMultiTurnStrategy = true,
                PlanningDepth = 3,
                SetupsCombos = true,
                SavesResourcesForKey = true,
                
                // ═══ RESOURCE MANAGEMENT ═══
                ManagesResourcesStrategically = true,
                MPConservationThreshold = 40,
                SavesForBossPhases = true,
                UsesItemsStrategically = true,
                
                // ═══ COUNTER & PREDICTION ═══
                PredictsPlayerMoves = true,
                UsesPreemptiveCounters = true,
                BaitsPlayer = true,
                PredictionAccuracy = 70,
                
                // Smart AI features
                LearnWeaknesses = true,
                ExploitTechnicals = true,
                TechnicalPriority = 85,
                WeaknessPriority = 80
            };
            
            GD.Print("═══════════════════════════════════════");
            GD.Print("    MASTER STRATEGIST LOADED");
            GD.Print("  • Plans 3 turns ahead");
            GD.Print("  • Predicts player moves (70% accuracy)");
            GD.Print("  • Manages resources strategically");
            GD.Print("  • Sets up devastating combos");
            GD.Print("═══════════════════════════════════════");
            
            return ai;
        }
        
        /// <summary>
        /// Example 2: ADAPTIVE CHAMPION
        /// Uses: Adaptive Learning + Risk Assessment + Personality Shifts
        /// Gets smarter and more aggressive as battle goes on
        /// </summary>
        public static AdvancedAIPattern CreateAdaptiveChampion()
        {
            var ai = new AdvancedAIPattern
            {
                // Base behavior
                BehaviorType = AIBehaviorType.Balanced,
                TargetPriority = TargetPriority.HighestThreat,
                Aggression = 50,
                SkillUsageRate = 70,
                
                // ═══ ADAPTIVE LEARNING ═══
                AdaptsToPlayerSkill = true,
                LearnsPlayerPatterns = true,
                BaseDifficultyLevel = 50,
                DifficultyAdjustmentRate = 0.15f,
                
                // ═══ RISK ASSESSMENT ═══
                UsesRiskCalculation = true,
                RiskTolerance = 40,
                TakesCalculatedRisks = true,
                DesperateWhenLosing = true,
                ConservativeWhenWinning = true,
                
                // ═══ PERSONALITY SHIFTS ═══
                HasPersonalityShifts = true,
                CurrentPersonality = PersonalityState.Calm,
                EvolvesInBattle = true,
                PhaseHPThresholds = new Godot.Collections.Array<float> { 0.75f, 0.5f, 0.25f },
                
                // Thresholds
                LowHPThreshold = 30,
                DefensiveThreshold = 60
            };
            
            GD.Print("═══════════════════════════════════════");
            GD.Print("    ADAPTIVE CHAMPION LOADED");
            GD.Print("  • Learns your patterns mid-battle");
            GD.Print("  • Adjusts difficulty dynamically");
            GD.Print("  • Personality evolves (4 phases)");
            GD.Print("  • Risk-taking based on situation");
            GD.Print("═══════════════════════════════════════");
            
            return ai;
        }
        
        /// <summary>
        /// Example 3: TEAM COMMANDER
        /// Uses: Party Coordination + Formation + Role Awareness
        /// Perfect for enemy group battles
        /// </summary>
        public static AdvancedAIPattern CreateTeamCommander()
        {
            var ai = new AdvancedAIPattern
            {
                // Base behavior
                BehaviorType = AIBehaviorType.Supportive,
                TargetPriority = TargetPriority.Healer,
                Aggression = 40,
                SkillUsageRate = 90,
                
                // ═══ PARTY COORDINATION ═══
                CoordinatesWithAllies = true,
                CoverAlliesWeaknesses = true,
                SynchronizedAttacks = true,
                RoleAwareness = true,
                TeamworkPriority = 85,
                
                // ═══ FORMATION & POSITIONING ═══
                UsesTacticalPositioning = true,
                PreferredFormation = FormationType.Defensive,
                ProtectsBackline = true,
                
                // Special behaviors
                GuardsAllies = true,
                UseDefensiveTactics = true
            };
            
            GD.Print("═══════════════════════════════════════");
            GD.Print("    TEAM COMMANDER LOADED");
            GD.Print("  • Coordinates with allies");
            GD.Print("  • Protects healers & backline");
            GD.Print("  • Synchronized focus fire");
            GD.Print("  • Maintains defensive formation");
            GD.Print("═══════════════════════════════════════");
            
            return ai;
        }
        
        /// <summary>
        /// Example 4: RECKLESS GAMBLER
        /// Uses: Risk Assessment + Personality Shifts
        /// High-risk high-reward playstyle
        /// </summary>
        public static AdvancedAIPattern CreateRecklessGambler()
        {
            var ai = new AdvancedAIPattern
            {
                // Base behavior
                BehaviorType = AIBehaviorType.Berserk,
                TargetPriority = TargetPriority.Random,
                Aggression = 90,
                Recklessness = 80,
                SkillUsageRate = 85,
                
                // ═══ RISK ASSESSMENT ═══
                UsesRiskCalculation = true,
                RiskTolerance = 70, // HIGH tolerance
                TakesCalculatedRisks = true,
                DesperateWhenLosing = true,
                ConservativeWhenWinning = false, // NEVER conservative!
                
                // ═══ PERSONALITY ═══
                HasPersonalityShifts = true,
                CurrentPersonality = PersonalityState.Confident,
                EvolvesInBattle = true,
                
                // Special behaviors
                EnragesAtLowHP = true,
                
                // Thresholds
                DefensiveThreshold = 0, // Never defends
                LowHPThreshold = 15
            };
            
            GD.Print("═══════════════════════════════════════");
            GD.Print("    RECKLESS GAMBLER LOADED");
            GD.Print("  • Takes huge risks");
            GD.Print("  • Unpredictable behavior");
            GD.Print("  • Goes all-in when desperate");
            GD.Print("  • Enrages at low HP");
            GD.Print("═══════════════════════════════════════");
            
            return ai;
        }
        
        /// <summary>
        /// Example 5: ULTIMATE BOSS - ALL SYSTEMS
        /// Uses: EVERYTHING!
        /// The ultimate challenge
        /// </summary>
        public static AdvancedAIPattern CreateUltimateBoss()
        {
            var ai = new AdvancedAIPattern
            {
                // Base behavior
                BehaviorType = AIBehaviorType.Tactical,
                TargetPriority = TargetPriority.MostVulnerable,
                Aggression = 70,
                SkillUsageRate = 85,
                
                // ═══ ALL SYSTEMS ENABLED ═══
                
                // Multi-Turn Strategy
                UsesMultiTurnStrategy = true,
                PlanningDepth = 3,
                SetupsCombos = true,
                SavesResourcesForKey = true,
                
                // Party Coordination
                CoordinatesWithAllies = true,
                RoleAwareness = true,
                TeamworkPriority = 80,
                
                // Adaptive Learning
                AdaptsToPlayerSkill = true,
                LearnsPlayerPatterns = true,
                BaseDifficultyLevel = 70,
                DifficultyAdjustmentRate = 0.2f,
                
                // Risk Assessment
                UsesRiskCalculation = true,
                RiskTolerance = 50,
                TakesCalculatedRisks = true,
                DesperateWhenLosing = true,
                ConservativeWhenWinning = true,
                
                // Formation
                UsesTacticalPositioning = true,
                PreferredFormation = FormationType.Balanced,
                
                // Resource Management
                ManagesResourcesStrategically = true,
                MPConservationThreshold = 35,
                SavesForBossPhases = true,
                
                // Personality Evolution
                HasPersonalityShifts = true,
                CurrentPersonality = PersonalityState.Calm,
                EvolvesInBattle = true,
                PhaseHPThresholds = new Godot.Collections.Array<float> { 0.7f, 0.4f, 0.15f },
                
                // Counter System
                PredictsPlayerMoves = true,
                UsesPreemptiveCounters = true,
                BaitsPlayer = true,
                PredictionAccuracy = 75,
                CreatesFakePatterns = true,
                
                // Smart AI
                LearnWeaknesses = true,
                ExploitTechnicals = true,
                UseDefensiveTactics = true,
                TechnicalPriority = 90,
                WeaknessPriority = 85,
                
                // Turn patterns
                HasTurnPattern = true,
                TurnPattern = new Godot.Collections.Array<string>
                {
                    "setup_debuff",
                    "heavy_attack",
                    "buff_self",
                    "combo_finisher"
                }
            };
            
            GD.Print("╔═══════════════════════════════════════╗");
            GD.Print("║     ULTIMATE BOSS INITIALIZED!       ║");
            GD.Print("╠═══════════════════════════════════════╣");
            GD.Print("║  ✓ Multi-Turn Strategy (3 turns)     ║");
            GD.Print("║  ✓ Party Coordination                ║");
            GD.Print("║  ✓ Adaptive Learning (High)          ║");
            GD.Print("║  ✓ Risk Assessment                   ║");
            GD.Print("║  ✓ Tactical Positioning              ║");
            GD.Print("║  ✓ Resource Management               ║");
            GD.Print("║  ✓ 4-Phase Personality Evolution     ║");
            GD.Print("║  ✓ Counter & Prediction (75%)        ║");
            GD.Print("╚═══════════════════════════════════════╝");
            
            return ai;
        }
        
        /// <summary>
        /// Example 6: REACTIVE COUNTER
        /// Uses: Counter System + Prediction + Learning
        /// Specializes in reacting to player actions
        /// </summary>
        public static AdvancedAIPattern CreateReactiveCounter()
        {
            var ai = new AdvancedAIPattern
            {
                // Base behavior
                BehaviorType = AIBehaviorType.Defensive,
                TargetPriority = TargetPriority.HighestThreat,
                Aggression = 35,
                SkillUsageRate = 75,
                
                // ═══ COUNTER SYSTEM ═══
                PredictsPlayerMoves = true,
                UsesPreemptiveCounters = true,
                BaitsPlayer = true,
                PredictionAccuracy = 85, // VERY HIGH
                CreatesFakePatterns = true,
                
                // ═══ LEARNING ═══
                LearnsPlayerPatterns = true,
                
                // Defensive focus
                UseDefensiveTactics = true,
                DefensiveThreshold = 70,
                GuardsAllies = true
            };
            
            GD.Print("═══════════════════════════════════════");
            GD.Print("    REACTIVE COUNTER LOADED");
            GD.Print("  • Predicts moves (85% accuracy!)");
            GD.Print("  • Preemptive counters");
            GD.Print("  • Baits player into traps");
            GD.Print("  • Creates fake patterns");
            GD.Print("═══════════════════════════════════════");
            
            return ai;
        }
        
        /// <summary>
        /// Example 7: RESOURCE HOARDER
        /// Uses: Resource Management + Strategy
        /// Conserves power for critical moments
        /// </summary>
        public static AdvancedAIPattern CreateResourceHoarder()
        {
            var ai = new AdvancedAIPattern
            {
                // Base behavior
                BehaviorType = AIBehaviorType.Balanced,
                TargetPriority = TargetPriority.LowestDefense,
                Aggression = 45,
                SkillUsageRate = 60, // LOW - conserves MP
                
                // ═══ RESOURCE MANAGEMENT ═══
                ManagesResourcesStrategically = true,
                MPConservationThreshold = 50, // HIGH threshold
                SavesForBossPhases = true,
                UsesItemsStrategically = true,
                
                // ═══ STRATEGY ═══
                UsesMultiTurnStrategy = true,
                PlanningDepth = 2,
                SavesResourcesForKey = true,
                
                // Thresholds
                DefensiveThreshold = 60
            };
            
            GD.Print("═══════════════════════════════════════");
            GD.Print("    RESOURCE HOARDER LOADED");
            GD.Print("  • Conserves MP carefully");
            GD.Print("  • Saves power for key moments");
            GD.Print("  • Strategic item usage");
            GD.Print("  • Plans resource expenditure");
            GD.Print("═══════════════════════════════════════");
            
            return ai;
        }
        
        /// <summary>
        /// Example 8: FORMATION TACTICIAN
        /// Uses: Formation + Positioning + Coordination
        /// Maintains tactical superiority through positioning
        /// </summary>
        public static AdvancedAIPattern CreateFormationTactician()
        {
            var ai = new AdvancedAIPattern
            {
                // Base behavior
                BehaviorType = AIBehaviorType.Tactical,
                TargetPriority = TargetPriority.Weakest,
                Aggression = 55,
                SkillUsageRate = 70,
                
                // ═══ FORMATION ═══
                UsesTacticalPositioning = true,
                PreferredFormation = FormationType.Flanking,
                MaintainsDistance = true,
                ProtectsBackline = true,
                FlankingBehavior = true,
                
                // ═══ COORDINATION ═══
                CoordinatesWithAllies = true,
                SynchronizedAttacks = true,
                TeamworkPriority = 80
            };
            
            GD.Print("═══════════════════════════════════════");
            GD.Print("    FORMATION TACTICIAN LOADED");
            GD.Print("  • Flanking formation");
            GD.Print("  • Maintains tactical distance");
            GD.Print("  • Protects vulnerable allies");
            GD.Print("  • Synchronized positioning");
            GD.Print("═══════════════════════════════════════");
            
            return ai;
        }
    }
}