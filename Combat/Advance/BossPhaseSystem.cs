// Combat/Advanced/BossPhaseSystem.cs
// Manages multi-phase boss battles with AI evolution
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EchoesAcrossTime.Combat.Advanced
{
    /// <summary>
    /// Advanced Boss Phase System
    /// Manages complex multi-phase boss battles with AI evolution,
    /// phase transitions, and special phase mechanics
    /// </summary>
    public partial class BossPhaseSystem : Node
    {
        private List<BossPhase> phases = new List<BossPhase>();
        private int currentPhaseIndex = 0;
        private BossPhase currentPhase;
        private CharacterStats bossStats;
        private AdvancedAIPattern bossAI;
        private bool isTransitioning = false;
        
        [Signal]
        public delegate void PhaseStartedEventHandler(int phaseNumber, string phaseName);
        
        [Signal]
        public delegate void PhaseTransitioningEventHandler(int fromPhase, int toPhase);
        
        [Signal]
        public delegate void PhaseCompletedEventHandler(int phaseNumber);
        
        [Signal]
        public delegate void BossEnragedEventHandler();
        
        [Signal]
        public delegate void SpecialMechanicTriggeredEventHandler(string mechanicName);
        
        /// <summary>
        /// Initialize boss phase system
        /// </summary>
        public void Initialize(CharacterStats boss, AdvancedAIPattern ai)
        {
            bossStats = boss;
            bossAI = ai;
            currentPhaseIndex = 0;
        }
        
        /// <summary>
        /// Add a boss phase
        /// </summary>
        public void AddPhase(BossPhase phase)
        {
            phases.Add(phase);
            GD.Print($"[BOSS PHASES] Added Phase {phases.Count}: {phase.PhaseName}");
        }
        
        /// <summary>
        /// Start the phase system
        /// </summary>
        public void Start()
        {
            if (phases.Count == 0)
            {
                GD.PrintErr("[BOSS PHASES] No phases configured!");
                return;
            }
            
            currentPhase = phases[0];
            ApplyPhase(currentPhase);
            EmitSignal(SignalName.PhaseStarted, 1, currentPhase.PhaseName);
        }
        
        /// <summary>
        /// Check for phase transitions
        /// </summary>
        public void CheckPhaseTransition()
        {
            if (bossStats == null || isTransitioning) return;
            
            float hpPercent = bossStats.HPPercent;
            
            // Check if we should transition to next phase
            if (currentPhaseIndex < phases.Count - 1)
            {
                var nextPhase = phases[currentPhaseIndex + 1];
                
                if (hpPercent <= nextPhase.HPThreshold)
                {
                    TransitionToPhase(currentPhaseIndex + 1);
                }
            }
        }
        
        /// <summary>
        /// Force transition to specific phase
        /// </summary>
        public void TransitionToPhase(int phaseIndex)
        {
            if (phaseIndex < 0 || phaseIndex >= phases.Count) return;
            if (isTransitioning) return;
            
            isTransitioning = true;
            int oldPhase = currentPhaseIndex;
            currentPhaseIndex = phaseIndex;
            currentPhase = phases[phaseIndex];
            
            GD.Print("\n╔═══════════════════════════════════════════════════════╗");
            GD.Print($"║         PHASE TRANSITION: {oldPhase + 1} → {currentPhaseIndex + 1}               ║");
            GD.Print("╚═══════════════════════════════════════════════════════╝");
            GD.Print($"  {currentPhase.PhaseName}");
            GD.Print($"  {currentPhase.Description}\n");
            
            EmitSignal(SignalName.PhaseTransitioning, oldPhase + 1, currentPhaseIndex + 1);
            
            // Show transition dialogue if available
            if (!string.IsNullOrEmpty(currentPhase.TransitionDialogue))
            {
                GD.Print($"  💬 {bossStats.CharacterName}: \"{currentPhase.TransitionDialogue}\"\n");
            }
            
            // Apply phase changes
            ApplyPhase(currentPhase);
            
            // Execute phase transition effects
            ExecutePhaseTransition(currentPhase);
            
            EmitSignal(SignalName.PhaseStarted, currentPhaseIndex + 1, currentPhase.PhaseName);
            
            isTransitioning = false;
        }
        
        /// <summary>
        /// Apply phase settings to boss
        /// </summary>
        private void ApplyPhase(BossPhase phase)
        {
            GD.Print($"[BOSS PHASES] Applying Phase {currentPhaseIndex + 1} settings:");
            
            // Apply AI changes
            if (phase.AIChanges != null)
            {
                bossAI.Aggression = phase.AIChanges.Aggression;
                bossAI.Recklessness = phase.AIChanges.Recklessness;
                bossAI.SkillUsageRate = phase.AIChanges.SkillUsageRate;
                
                // Enable new systems
                if (phase.AIChanges.EnableMultiTurnStrategy)
                {
                    bossAI.UsesMultiTurnStrategy = true;
                    bossAI.PlanningDepth = phase.AIChanges.PlanningDepth;
                    GD.Print("  ✓ Multi-Turn Strategy enabled");
                }
                
                if (phase.AIChanges.EnablePrediction)
                {
                    bossAI.PredictsPlayerMoves = true;
                    bossAI.PredictionAccuracy = phase.AIChanges.PredictionAccuracy;
                    GD.Print("  ✓ Prediction enabled");
                }
                
                if (phase.AIChanges.EnableCoordination)
                {
                    bossAI.CoordinatesWithAllies = true;
                    GD.Print("  ✓ Coordination enabled");
                }
                
                GD.Print($"  Aggression: {phase.AIChanges.Aggression}");
                GD.Print($"  Recklessness: {phase.AIChanges.Recklessness}");
            }
            
            // Apply stat changes
            if (phase.StatChanges != null)
            {
                if (phase.StatChanges.AttackMultiplier != 1f)
                {
                    int oldAtk = bossStats.Attack;
                    bossStats.Attack = Mathf.RoundToInt(bossStats.Attack * phase.StatChanges.AttackMultiplier);
                    GD.Print($"  Attack: {oldAtk} → {bossStats.Attack}");
                }
                
                if (phase.StatChanges.DefenseMultiplier != 1f)
                {
                    int oldDef = bossStats.Defense;
                    bossStats.Defense = Mathf.RoundToInt(bossStats.Defense * phase.StatChanges.DefenseMultiplier);
                    GD.Print($"  Defense: {oldDef} → {bossStats.Defense}");
                }
                
                if (phase.StatChanges.SpeedMultiplier != 1f)
                {
                    int oldSpd = bossStats.Speed;
                    bossStats.Speed = Mathf.RoundToInt(bossStats.Speed * phase.StatChanges.SpeedMultiplier);
                    GD.Print($"  Speed: {oldSpd} → {bossStats.Speed}");
                }
            }
        }
        
        /// <summary>
        /// Execute phase transition effects
        /// </summary>
        private void ExecutePhaseTransition(BossPhase phase)
        {
            // Heal boss
            if (phase.HealOnTransition > 0)
            {
                int healAmount = Mathf.RoundToInt(bossStats.MaxHP * phase.HealOnTransition);
                bossStats.Heal(healAmount);
                GD.Print($"  ❤️ Boss healed {healAmount} HP!");
            }
            
            // Restore MP
            if (phase.RestoreMPOnTransition > 0)
            {
                int mpRestore = Mathf.RoundToInt(bossStats.MaxMP * phase.RestoreMPOnTransition);
                bossStats.RestoreMP(mpRestore);
                GD.Print($"  ⚡ Boss restored {mpRestore} MP!");
            }
            
            // Enrage
            if (phase.EnragesOnTransition)
            {
                GD.Print("  🔥 BOSS ENRAGED!");
                EmitSignal(SignalName.BossEnraged);
            }
            
            // Special mechanics
            foreach (var mechanic in phase.SpecialMechanics)
            {
                GD.Print($"  ⚡ Special Mechanic: {mechanic}");
                EmitSignal(SignalName.SpecialMechanicTriggered, mechanic);
            }
            
            // Summon adds
            if (phase.SummonsAdds)
            {
                GD.Print($"  👥 Boss summons {phase.AddCount} allies!");
            }
        }
        
        /// <summary>
        /// Get current phase
        /// </summary>
        public BossPhase GetCurrentPhase() => currentPhase;
        
        /// <summary>
        /// Get phase number (1-indexed)
        /// </summary>
        public int GetCurrentPhaseNumber() => currentPhaseIndex + 1;
        
        /// <summary>
        /// Get total phases
        /// </summary>
        public int GetTotalPhases() => phases.Count;
        
        /// <summary>
        /// Check if in final phase
        /// </summary>
        public bool IsInFinalPhase() => currentPhaseIndex == phases.Count - 1;
        
        /// <summary>
        /// Get phase transition dialogue for current HP
        /// </summary>
        public string GetPhaseDialogue()
        {
            return currentPhase?.TransitionDialogue ?? "";
        }
    }
    
    /// <summary>
    /// Boss Phase Configuration
    /// </summary>
    public class BossPhase
    {
        // Identity
        public string PhaseName { get; set; }
        public string Description { get; set; }
        public float HPThreshold { get; set; } // When to trigger (0.0 - 1.0)
        
        // Dialogue
        public string TransitionDialogue { get; set; }
        
        // AI Changes
        public PhaseAIChanges AIChanges { get; set; }
        
        // Stat Changes
        public PhaseStatChanges StatChanges { get; set; }
        
        // Phase Mechanics
        public float HealOnTransition { get; set; } = 0f; // % of max HP
        public float RestoreMPOnTransition { get; set; } = 0f; // % of max MP
        public bool EnragesOnTransition { get; set; } = false;
        public bool SummonsAdds { get; set; } = false;
        public int AddCount { get; set; } = 0;
        public List<string> SpecialMechanics { get; set; } = new List<string>();
    }
    
    public class PhaseAIChanges
    {
        public int Aggression { get; set; } = 50;
        public int Recklessness { get; set; } = 30;
        public int SkillUsageRate { get; set; } = 70;
        
        // System enablement
        public bool EnableMultiTurnStrategy { get; set; } = false;
        public int PlanningDepth { get; set; } = 2;
        
        public bool EnablePrediction { get; set; } = false;
        public int PredictionAccuracy { get; set; } = 60;
        
        public bool EnableCoordination { get; set; } = false;
        public bool EnableRiskAssessment { get; set; } = false;
    }
    
    public class PhaseStatChanges
    {
        public float AttackMultiplier { get; set; } = 1.0f;
        public float DefenseMultiplier { get; set; } = 1.0f;
        public float SpeedMultiplier { get; set; } = 1.0f;
        public float MagicAttackMultiplier { get; set; } = 1.0f;
    }
    
    /// <summary>
    /// Pre-configured boss phase templates
    /// </summary>
    public static class BossPhaseTemplates
    {
        /// <summary>
        /// Classic 3-phase boss
        /// Phase 1: Normal
        /// Phase 2: Faster + More aggressive
        /// Phase 3: Enraged + All-out
        /// </summary>
        public static List<BossPhase> CreateClassic3Phase()
        {
            return new List<BossPhase>
            {
                // Phase 1: Testing the player
                new BossPhase
                {
                    PhaseName = "Phase 1: The Beginning",
                    Description = "Boss fights normally",
                    HPThreshold = 1.0f,
                    TransitionDialogue = "Let's see what you're capable of...",
                    AIChanges = new PhaseAIChanges
                    {
                        Aggression = 50,
                        Recklessness = 30,
                        SkillUsageRate = 60
                    },
                    StatChanges = new PhaseStatChanges()
                },
                
                // Phase 2: Getting serious
                new BossPhase
                {
                    PhaseName = "Phase 2: Escalation",
                    Description = "Boss gets faster and more aggressive",
                    HPThreshold = 0.6f,
                    TransitionDialogue = "Not bad... but now the real fight begins!",
                    AIChanges = new PhaseAIChanges
                    {
                        Aggression = 70,
                        Recklessness = 50,
                        SkillUsageRate = 75,
                        EnableMultiTurnStrategy = true,
                        PlanningDepth = 2
                    },
                    StatChanges = new PhaseStatChanges
                    {
                        SpeedMultiplier = 1.2f,
                        AttackMultiplier = 1.15f
                    },
                    HealOnTransition = 0.15f
                },
                
                // Phase 3: Enraged
                new BossPhase
                {
                    PhaseName = "Phase 3: ENRAGE",
                    Description = "Boss goes all-out with maximum power",
                    HPThreshold = 0.3f,
                    TransitionDialogue = "YOU DARE?! I'LL DESTROY YOU!",
                    AIChanges = new PhaseAIChanges
                    {
                        Aggression = 95,
                        Recklessness = 80,
                        SkillUsageRate = 90,
                        EnableMultiTurnStrategy = true,
                        PlanningDepth = 3,
                        EnablePrediction = true,
                        PredictionAccuracy = 70
                    },
                    StatChanges = new PhaseStatChanges
                    {
                        SpeedMultiplier = 1.4f,
                        AttackMultiplier = 1.3f,
                        MagicAttackMultiplier = 1.3f
                    },
                    EnragesOnTransition = true,
                    RestoreMPOnTransition = 0.5f
                }
            };
        }
        
        /// <summary>
        /// 4-phase epic boss
        /// Each phase unlocks new AI systems
        /// </summary>
        public static List<BossPhase> CreateEpic4Phase()
        {
            return new List<BossPhase>
            {
                // Phase 1: Calm strategist
                new BossPhase
                {
                    PhaseName = "Phase 1: Analysis",
                    Description = "Boss studies your tactics",
                    HPThreshold = 1.0f,
                    TransitionDialogue = "Interesting... Let me analyze your fighting style.",
                    AIChanges = new PhaseAIChanges
                    {
                        Aggression = 45,
                        Recklessness = 20,
                        SkillUsageRate = 65,
                        EnableMultiTurnStrategy = true,
                        PlanningDepth = 2
                    }
                },
                
                // Phase 2: Learning
                new BossPhase
                {
                    PhaseName = "Phase 2: Adaptation",
                    Description = "Boss learns and counters your patterns",
                    HPThreshold = 0.75f,
                    TransitionDialogue = "I see your patterns now... How predictable.",
                    AIChanges = new PhaseAIChanges
                    {
                        Aggression = 60,
                        Recklessness = 35,
                        SkillUsageRate = 72,
                        EnableMultiTurnStrategy = true,
                        PlanningDepth = 3,
                        EnablePrediction = true,
                        PredictionAccuracy = 65
                    },
                    StatChanges = new PhaseStatChanges
                    {
                        SpeedMultiplier = 1.1f,
                        AttackMultiplier = 1.1f
                    }
                },
                
                // Phase 3: Coordination
                new BossPhase
                {
                    PhaseName = "Phase 3: Reinforcements",
                    Description = "Boss summons allies and coordinates attacks",
                    HPThreshold = 0.5f,
                    TransitionDialogue = "You're stronger than expected... Minions, assist me!",
                    AIChanges = new PhaseAIChanges
                    {
                        Aggression = 75,
                        Recklessness = 50,
                        SkillUsageRate = 80,
                        EnableMultiTurnStrategy = true,
                        PlanningDepth = 3,
                        EnablePrediction = true,
                        PredictionAccuracy = 70,
                        EnableCoordination = true
                    },
                    StatChanges = new PhaseStatChanges
                    {
                        SpeedMultiplier = 1.25f,
                        AttackMultiplier = 1.2f,
                        DefenseMultiplier = 1.15f
                    },
                    SummonsAdds = true,
                    AddCount = 2,
                    HealOnTransition = 0.2f
                },
                
                // Phase 4: Desperation
                new BossPhase
                {
                    PhaseName = "Phase 4: FINAL FORM",
                    Description = "Boss unleashes ultimate power",
                    HPThreshold = 0.25f,
                    TransitionDialogue = "IF I FALL... I'M TAKING YOU WITH ME!",
                    AIChanges = new PhaseAIChanges
                    {
                        Aggression = 100,
                        Recklessness = 90,
                        SkillUsageRate = 95,
                        EnableMultiTurnStrategy = true,
                        PlanningDepth = 4,
                        EnablePrediction = true,
                        PredictionAccuracy = 80,
                        EnableCoordination = true,
                        EnableRiskAssessment = true
                    },
                    StatChanges = new PhaseStatChanges
                    {
                        SpeedMultiplier = 1.5f,
                        AttackMultiplier = 1.5f,
                        MagicAttackMultiplier = 1.5f
                    },
                    EnragesOnTransition = true,
                    RestoreMPOnTransition = 1.0f,
                    SpecialMechanics = new List<string> { "Damage Reflect", "Attack Twice Per Turn", "Immune to Debuffs" }
                }
            };
        }
        
        /// <summary>
        /// 2-phase simple boss
        /// </summary>
        public static List<BossPhase> CreateSimple2Phase()
        {
            return new List<BossPhase>
            {
                new BossPhase
                {
                    PhaseName = "Phase 1: Normal",
                    Description = "Standard combat",
                    HPThreshold = 1.0f,
                    TransitionDialogue = "Here I come!",
                    AIChanges = new PhaseAIChanges
                    {
                        Aggression = 55,
                        SkillUsageRate = 65
                    }
                },
                
                new BossPhase
                {
                    PhaseName = "Phase 2: Desperate",
                    Description = "All-out assault",
                    HPThreshold = 0.4f,
                    TransitionDialogue = "I won't lose!",
                    AIChanges = new PhaseAIChanges
                    {
                        Aggression = 85,
                        Recklessness = 70,
                        SkillUsageRate = 85
                    },
                    StatChanges = new PhaseStatChanges
                    {
                        AttackMultiplier = 1.25f,
                        SpeedMultiplier = 1.2f
                    },
                    EnragesOnTransition = true
                }
            };
        }
    }
}