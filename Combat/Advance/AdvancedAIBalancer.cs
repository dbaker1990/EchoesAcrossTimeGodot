// Combat/Advanced/AdvancedAIBalancer.cs
// Automatic balancing and difficulty adjustment system
using Godot;
using System.Collections.Generic;
using System.Linq;

namespace EchoesAcrossTime.Combat.Advanced
{
    /// <summary>
    /// Automatically balances AI difficulty based on player performance
    /// Can create "Perfect Difficulty" that adapts to player skill
    /// </summary>
    public partial class AdvancedAIBalancer : Node
    {
        [Export] public bool EnableAutoBalancing { get; set; } = true;
        [Export] public float TargetWinRate { get; set; } = 0.6f; // Player should win 60% of time
        [Export] public int SampleSize { get; set; } = 10; // Battles to analyze
        
        private Queue<BattleResult> recentBattles = new Queue<BattleResult>();
        private Dictionary<string, AIBalanceProfile> aiProfiles = new Dictionary<string, AIBalanceProfile>();
        
        [Signal]
        public delegate void DifficultyAdjustedEventHandler(string aiId, int newDifficulty);
        
        /// <summary>
        /// Record a battle result for analysis
        /// </summary>
        public void RecordBattle(string aiId, bool playerWon, BattleMetrics metrics)
        {
            var result = new BattleResult
            {
                AIId = aiId,
                PlayerWon = playerWon,
                TurnsElapsed = metrics.TurnsElapsed,
                PlayerDamageTaken = metrics.TotalDamageTaken,
                PlayerDeaths = metrics.CharactersKO,
                PlayerPerformance = CalculatePlayerPerformance(metrics)
            };
            
            recentBattles.Enqueue(result);
            if (recentBattles.Count > SampleSize * 2)
                recentBattles.Dequeue();
            
            // Update AI profile
            if (!aiProfiles.ContainsKey(aiId))
                aiProfiles[aiId] = new AIBalanceProfile();
            
            var profile = aiProfiles[aiId];
            profile.TotalBattles++;
            if (playerWon)
                profile.PlayerWins++;
            else
                profile.AIWins++;
            
            // Auto-balance if enabled
            if (EnableAutoBalancing && profile.TotalBattles % 3 == 0)
            {
                AdjustAIDifficulty(aiId);
            }
        }
        
        /// <summary>
        /// Automatically adjust AI difficulty
        /// </summary>
        private void AdjustAIDifficulty(string aiId)
        {
            var profile = aiProfiles[aiId];
            float actualWinRate = (float)profile.PlayerWins / profile.TotalBattles;
            
            GD.Print($"\n[BALANCER] Analyzing {aiId}:");
            GD.Print($"  Battles: {profile.TotalBattles}");
            GD.Print($"  Player Win Rate: {actualWinRate * 100:F1}%");
            GD.Print($"  Target Win Rate: {TargetWinRate * 100:F1}%");
            
            // Calculate adjustment
            float difference = actualWinRate - TargetWinRate;
            int adjustment = 0;
            
            if (Mathf.Abs(difference) > 0.15f) // Significant difference
            {
                if (difference > 0) // Player winning too much - increase difficulty
                {
                    adjustment = Mathf.RoundToInt(difference * 50); // Up to +25 difficulty
                    GD.Print($"  ↑ Player winning too often - increasing difficulty by {adjustment}");
                }
                else // Player losing too much - decrease difficulty
                {
                    adjustment = Mathf.RoundToInt(difference * 50); // Up to -25 difficulty
                    GD.Print($"  ↓ Player struggling - decreasing difficulty by {adjustment}");
                }
                
                profile.CurrentDifficulty = Mathf.Clamp(profile.CurrentDifficulty + adjustment, 10, 100);
                
                EmitSignal(SignalName.DifficultyAdjusted, aiId, profile.CurrentDifficulty);
                GD.Print($"  New Difficulty: {profile.CurrentDifficulty}");
            }
            else
            {
                GD.Print($"  ✓ Difficulty is balanced!");
            }
        }
        
        /// <summary>
        /// Get recommended AI settings for balanced difficulty
        /// </summary>
        public AdvancedAIPattern GetBalancedAI(string aiId, int playerLevel)
        {
            var profile = aiProfiles.ContainsKey(aiId) ? aiProfiles[aiId] : new AIBalanceProfile();
            int difficulty = profile.CurrentDifficulty;
            
            // Scale systems based on difficulty
            var ai = new AdvancedAIPattern
            {
                BehaviorType = AIBehaviorType.Balanced,
                TargetPriority = TargetPriority.MostVulnerable,
                Aggression = difficulty,
                SkillUsageRate = Mathf.Clamp(40 + difficulty / 2, 40, 90),
                
                // Basic features always on
                LearnWeaknesses = true,
                ExploitTechnicals = true,
                UseDefensiveTactics = difficulty > 30
            };
            
            // Enable advanced systems based on difficulty tiers
            if (difficulty >= 30) // Easy-Medium
            {
                ai.UsesMultiTurnStrategy = true;
                ai.PlanningDepth = 2;
            }
            
            if (difficulty >= 50) // Medium
            {
                ai.LearnsPlayerPatterns = true;
                ai.ManagesResourcesStrategically = true;
                ai.MPConservationThreshold = 40;
            }
            
            if (difficulty >= 65) // Medium-Hard
            {
                ai.AdaptsToPlayerSkill = true;
                ai.BaseDifficultyLevel = difficulty;
                ai.UsesRiskCalculation = true;
                ai.RiskTolerance = 40;
            }
            
            if (difficulty >= 75) // Hard
            {
                ai.PlanningDepth = 3;
                ai.HasPersonalityShifts = true;
                ai.PhaseHPThresholds = new Godot.Collections.Array<float> { 0.7f, 0.4f };
                ai.CoordinatesWithAllies = true;
            }
            
            if (difficulty >= 85) // Very Hard
            {
                ai.PredictsPlayerMoves = true;
                ai.PredictionAccuracy = difficulty - 20; // 65% accuracy
                ai.UsesTacticalPositioning = true;
            }
            
            GD.Print($"[BALANCER] Generated AI for difficulty {difficulty}:");
            GD.Print($"  Systems active: {CountActiveSystems(ai)}/8");
            
            return ai;
        }
        
        /// <summary>
        /// Generate difficulty curve for progression
        /// </summary>
        public List<AIBalancePoint> GenerateDifficultyCurve(int startLevel, int endLevel)
        {
            var curve = new List<AIBalancePoint>();
            
            for (int level = startLevel; level <= endLevel; level++)
            {
                int difficulty = CalculateDifficultyForLevel(level, startLevel, endLevel);
                var point = new AIBalancePoint
                {
                    Level = level,
                    RecommendedDifficulty = difficulty,
                    EnabledSystems = GetSystemsForDifficulty(difficulty)
                };
                curve.Add(point);
            }
            
            return curve;
        }
        
        /// <summary>
        /// Print difficulty curve visualization
        /// </summary>
        public void PrintDifficultyCurve(int startLevel, int endLevel)
        {
            var curve = GenerateDifficultyCurve(startLevel, endLevel);
            
            GD.Print("\n╔═══════════════════════════════════════════════════════╗");
            GD.Print("║           DIFFICULTY CURVE VISUALIZATION             ║");
            GD.Print("╚═══════════════════════════════════════════════════════╝\n");
            
            GD.Print("Level  Difficulty  Systems  Graph");
            GD.Print("─────  ──────────  ───────  ─────────────────────────");
            
            foreach (var point in curve)
            {
                int barLength = point.RecommendedDifficulty / 2; // Scale to 50 chars max
                string bar = new string('█', barLength);
                GD.Print($"{point.Level,4}   {point.RecommendedDifficulty,3}%       {point.EnabledSystems}/8      {bar}");
            }
            
            GD.Print("\n");
        }
        
        private int CalculateDifficultyForLevel(int level, int startLevel, int endLevel)
        {
            // Logarithmic curve: starts slower, ramps up faster near end
            float progress = (float)(level - startLevel) / (endLevel - startLevel);
            float curveValue = Mathf.Pow(progress, 1.5f); // Exponential curve
            return Mathf.RoundToInt(Mathf.Lerp(30, 95, curveValue));
        }
        
        private int GetSystemsForDifficulty(int difficulty)
        {
            if (difficulty < 30) return 0;
            if (difficulty < 50) return 2;
            if (difficulty < 65) return 4;
            if (difficulty < 75) return 5;
            if (difficulty < 85) return 6;
            return 8;
        }
        
        private float CalculatePlayerPerformance(BattleMetrics metrics)
        {
            float score = 100f;
            
            // Penalties
            score -= metrics.CharactersKO * 10;
            score -= metrics.TotalDamageTaken / 10f;
            score -= Mathf.Max(0, metrics.TurnsElapsed - 10) * 2; // Penalty for slow battles
            
            // Bonuses
            score += metrics.WeaknessHits * 2;
            score += metrics.CriticalHits * 1.5f;
            score += metrics.TechnicalHits * 3;
            score += metrics.AllOutAttacks * 5;
            
            if (metrics.NoDamageTaken) score += 20;
            if (metrics.NoKOs) score += 15;
            
            return Mathf.Clamp(score, 0, 150);
        }
        
        private int CountActiveSystems(AdvancedAIPattern ai)
        {
            int count = 0;
            if (ai.UsesMultiTurnStrategy) count++;
            if (ai.CoordinatesWithAllies) count++;
            if (ai.AdaptsToPlayerSkill) count++;
            if (ai.UsesRiskCalculation) count++;
            if (ai.UsesTacticalPositioning) count++;
            if (ai.ManagesResourcesStrategically) count++;
            if (ai.HasPersonalityShifts) count++;
            if (ai.PredictsPlayerMoves) count++;
            return count;
        }
        
        /// <summary>
        /// Get current stats for an AI
        /// </summary>
        public AIBalanceProfile GetProfile(string aiId)
        {
            return aiProfiles.ContainsKey(aiId) ? aiProfiles[aiId] : null;
        }
        
        /// <summary>
        /// Reset all balance data
        /// </summary>
        public void Reset()
        {
            recentBattles.Clear();
            aiProfiles.Clear();
            GD.Print("[BALANCER] All balance data reset");
        }
    }
    
    public class BattleResult
    {
        public string AIId { get; set; }
        public bool PlayerWon { get; set; }
        public int TurnsElapsed { get; set; }
        public int PlayerDamageTaken { get; set; }
        public int PlayerDeaths { get; set; }
        public float PlayerPerformance { get; set; }
    }
    
    public class AIBalanceProfile
    {
        public int TotalBattles { get; set; } = 0;
        public int PlayerWins { get; set; } = 0;
        public int AIWins { get; set; } = 0;
        public int CurrentDifficulty { get; set; } = 50;
        public float AverageBattleLength { get; set; } = 0;
        
        public float WinRate => TotalBattles > 0 ? (float)PlayerWins / TotalBattles : 0f;
    }
    
    public class AIBalancePoint
    {
        public int Level { get; set; }
        public int RecommendedDifficulty { get; set; }
        public int EnabledSystems { get; set; }
    }
    
    /// <summary>
    /// Preset balance profiles for different game modes
    /// </summary>
    public static class BalancePresets
    {
        public static AdvancedAIPattern GetStoryMode()
        {
            return new AdvancedAIPattern
            {
                BehaviorType = AIBehaviorType.Balanced,
                Aggression = 40,
                
                // Fewer systems for casual play
                UsesMultiTurnStrategy = true,
                PlanningDepth = 2,
                HasPersonalityShifts = true,
                
                LearnWeaknesses = true,
                BaseDifficultyLevel = 40
            };
        }
        
        public static AdvancedAIPattern GetNormalMode()
        {
            return new AdvancedAIPattern
            {
                BehaviorType = AIBehaviorType.Tactical,
                Aggression = 55,
                
                // Standard challenge
                UsesMultiTurnStrategy = true,
                PlanningDepth = 2,
                LearnsPlayerPatterns = true,
                ManagesResourcesStrategically = true,
                HasPersonalityShifts = true,
                
                LearnWeaknesses = true,
                ExploitTechnicals = true,
                BaseDifficultyLevel = 55
            };
        }
        
        public static AdvancedAIPattern GetHardMode()
        {
            return new AdvancedAIPattern
            {
                BehaviorType = AIBehaviorType.Tactical,
                Aggression = 70,
                
                // Most systems active
                UsesMultiTurnStrategy = true,
                PlanningDepth = 3,
                CoordinatesWithAllies = true,
                AdaptsToPlayerSkill = true,
                LearnsPlayerPatterns = true,
                UsesRiskCalculation = true,
                ManagesResourcesStrategically = true,
                HasPersonalityShifts = true,
                PredictsPlayerMoves = true,
                PredictionAccuracy = 65,
                
                LearnWeaknesses = true,
                ExploitTechnicals = true,
                BaseDifficultyLevel = 70
            };
        }
        
        public static AdvancedAIPattern GetNightmareMode()
        {
            return new AdvancedAIPattern
            {
                BehaviorType = AIBehaviorType.Tactical,
                Aggression = 90,
                Recklessness = 60,
                
                // ALL SYSTEMS MAXIMUM
                UsesMultiTurnStrategy = true,
                PlanningDepth = 5,
                SetupsCombos = true,
                SavesResourcesForKey = true,
                
                CoordinatesWithAllies = true,
                CoverAlliesWeaknesses = true,
                SynchronizedAttacks = true,
                RoleAwareness = true,
                TeamworkPriority = 90,
                
                AdaptsToPlayerSkill = true,
                LearnsPlayerPatterns = true,
                BaseDifficultyLevel = 85,
                DifficultyAdjustmentRate = 0.2f,
                
                UsesRiskCalculation = true,
                RiskTolerance = 60,
                TakesCalculatedRisks = true,
                DesperateWhenLosing = true,
                
                UsesTacticalPositioning = true,
                PreferredFormation = FormationType.Flanking,
                FlankingBehavior = true,
                
                ManagesResourcesStrategically = true,
                MPConservationThreshold = 30,
                SavesForBossPhases = true,
                
                HasPersonalityShifts = true,
                EvolvesInBattle = true,
                PhaseHPThresholds = new Godot.Collections.Array<float> { 0.75f, 0.5f, 0.25f, 0.1f },
                
                PredictsPlayerMoves = true,
                UsesPreemptiveCounters = true,
                BaitsPlayer = true,
                PredictionAccuracy = 80,
                CreatesFakePatterns = true,
                
                LearnWeaknesses = true,
                ExploitTechnicals = true,
                UseDefensiveTactics = true,
                TechnicalPriority = 95,
                WeaknessPriority = 95
            };
        }
    }
}