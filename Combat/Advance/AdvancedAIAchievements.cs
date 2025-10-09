// Combat/Advanced/AdvancedAIAchievements.cs
// Tracks player achievements related to Advanced AI interactions
using Godot;
using System.Collections.Generic;
using System.Linq;

namespace EchoesAcrossTime.Combat.Advanced
{
    /// <summary>
    /// Tracks and awards achievements for interacting with Advanced AI
    /// Makes the AI system visible and rewarding to players
    /// </summary>
    public partial class AdvancedAIAchievements : Node
    {
        private Dictionary<string, AchievementProgress> achievements = new Dictionary<string, AchievementProgress>();
        
        [Signal]
        public delegate void AchievementUnlockedEventHandler(string achievementId, string achievementName);
        
        public override void _Ready()
        {
            InitializeAchievements();
        }
        
        private void InitializeAchievements()
        {
            // AI Prediction Achievements
            RegisterAchievement("predicted_once", "Predicted!", "Get countered by AI prediction", 1);
            RegisterAchievement("predicted_10", "Read Like a Book", "Get countered by AI prediction 10 times", 10);
            RegisterAchievement("predicted_50", "Completely Predictable", "Get countered by AI prediction 50 times", 50);
            
            // Pattern Learning Achievements
            RegisterAchievement("pattern_learned", "They're Learning!", "Have AI learn your pattern", 1);
            RegisterAchievement("pattern_learned_10", "Outsmarted", "Have AI learn your patterns 10 times", 10);
            
            // Combo Achievements
            RegisterAchievement("ai_combo_hit", "Calculated Combo", "Get hit by AI combo chain", 1);
            RegisterAchievement("ai_combo_10", "Strategic Genius", "Get hit by AI combo chains 10 times", 10);
            RegisterAchievement("ai_perfect_combo", "Perfect Setup", "Witness AI execute 3+ turn planned combo", 1);
            
            // Coordination Achievements
            RegisterAchievement("synchronized_attack", "In Sync", "Get hit by synchronized enemy attack", 1);
            RegisterAchievement("focus_fire", "Focus Fire", "All enemies target you at once", 1);
            RegisterAchievement("healer_protected", "Guardian", "Witness enemy protect their healer", 1);
            
            // Personality Shift Achievements
            RegisterAchievement("personality_shift", "Mood Swing", "Trigger AI personality shift", 1);
            RegisterAchievement("personality_all_phases", "Emotional Rollercoaster", "See all personality phases in one battle", 1);
            RegisterAchievement("enraged_boss", "You Made Them Angry", "Trigger boss enrage phase", 1);
            
            // Risk & Desperation Achievements
            RegisterAchievement("desperate_gamble", "All or Nothing", "Witness AI desperate gamble", 1);
            RegisterAchievement("risky_payoff", "Calculated Risk", "Get destroyed by AI risky move", 1);
            
            // Adaptive Difficulty Achievements
            RegisterAchievement("difficulty_increased", "Getting Harder", "Have AI increase its difficulty", 1);
            RegisterAchievement("difficulty_decreased", "Too Easy?", "Have AI decrease its difficulty", 1);
            RegisterAchievement("adaptive_master", "Adapts to You", "Battle AI that adapts 5 times", 1);
            
            // Resource Management Achievements
            RegisterAchievement("mp_conserved", "Saving for Later", "Notice AI conserving MP", 1);
            RegisterAchievement("phase_2_unleash", "Unleashed Power", "Witness boss unleash saved resources in phase 2", 1);
            
            // Victory Achievements
            RegisterAchievement("defeated_strategist", "Outsmarted the Mastermind", "Defeat Master Strategist AI", 1);
            RegisterAchievement("defeated_adaptive", "Beat the Adaptive", "Defeat Adaptive Champion", 1);
            RegisterAchievement("defeated_ultimate", "Ultimate Victor", "Defeat Ultimate Boss (all 8 systems)", 1);
            RegisterAchievement("no_damage_vs_advanced", "Untouchable", "Perfect battle vs Advanced AI", 1);
            
            // Collection Achievements
            RegisterAchievement("faced_all_systems", "System Scholar", "Face all 8 AI systems", 8);
            RegisterAchievement("faced_all_presets", "Enemy Connoisseur", "Battle all 8 preset AI types", 8);
        }
        
        private void RegisterAchievement(string id, string name, string description, int requirement)
        {
            achievements[id] = new AchievementProgress
            {
                Id = id,
                Name = name,
                Description = description,
                Requirement = requirement,
                Current = 0,
                IsUnlocked = false
            };
        }
        
        /// <summary>
        /// Track when AI predicts player move
        /// </summary>
        public void OnAIPrediction(bool accurate)
        {
            if (accurate)
            {
                IncrementAchievement("predicted_once");
                IncrementAchievement("predicted_10");
                IncrementAchievement("predicted_50");
            }
        }
        
        /// <summary>
        /// Track when AI learns pattern
        /// </summary>
        public void OnPatternLearned()
        {
            IncrementAchievement("pattern_learned");
            IncrementAchievement("pattern_learned_10");
        }
        
        /// <summary>
        /// Track AI combo execution
        /// </summary>
        public void OnAICombo(int comboLength)
        {
            IncrementAchievement("ai_combo_hit");
            IncrementAchievement("ai_combo_10");
            
            if (comboLength >= 3)
            {
                IncrementAchievement("ai_perfect_combo");
            }
        }
        
        /// <summary>
        /// Track synchronized attacks
        /// </summary>
        public void OnSynchronizedAttack()
        {
            IncrementAchievement("synchronized_attack");
        }
        
        /// <summary>
        /// Track focus fire
        /// </summary>
        public void OnFocusFire()
        {
            IncrementAchievement("focus_fire");
        }
        
        /// <summary>
        /// Track healer protection
        /// </summary>
        public void OnHealerProtected()
        {
            IncrementAchievement("healer_protected");
        }
        
        /// <summary>
        /// Track personality shifts
        /// </summary>
        public void OnPersonalityShift(int totalPhasesInBattle)
        {
            IncrementAchievement("personality_shift");
            
            if (totalPhasesInBattle >= 4)
            {
                IncrementAchievement("personality_all_phases");
            }
        }
        
        /// <summary>
        /// Track enrage
        /// </summary>
        public void OnEnrage()
        {
            IncrementAchievement("enraged_boss");
        }
        
        /// <summary>
        /// Track desperate gambles
        /// </summary>
        public void OnDesperateGamble()
        {
            IncrementAchievement("desperate_gamble");
        }
        
        /// <summary>
        /// Track risky moves
        /// </summary>
        public void OnRiskyMove(bool successful)
        {
            if (successful)
            {
                IncrementAchievement("risky_payoff");
            }
        }
        
        /// <summary>
        /// Track difficulty changes
        /// </summary>
        public void OnDifficultyAdjusted(bool increased)
        {
            if (increased)
            {
                IncrementAchievement("difficulty_increased");
            }
            else
            {
                IncrementAchievement("difficulty_decreased");
            }
            
            IncrementAchievement("adaptive_master");
        }
        
        /// <summary>
        /// Track resource conservation
        /// </summary>
        public void OnMPConserved()
        {
            IncrementAchievement("mp_conserved");
        }
        
        /// <summary>
        /// Track phase 2 unleash
        /// </summary>
        public void OnPhase2Unleash()
        {
            IncrementAchievement("phase_2_unleash");
        }
        
        /// <summary>
        /// Track victories
        /// </summary>
        public void OnVictory(string aiType, bool noDamage)
        {
            switch (aiType)
            {
                case "MasterStrategist":
                    IncrementAchievement("defeated_strategist");
                    break;
                case "AdaptiveChampion":
                    IncrementAchievement("defeated_adaptive");
                    break;
                case "UltimateBoss":
                    IncrementAchievement("defeated_ultimate");
                    break;
            }
            
            if (noDamage)
            {
                IncrementAchievement("no_damage_vs_advanced");
            }
            
            IncrementAchievement("faced_all_presets");
        }
        
        /// <summary>
        /// Track system encounters
        /// </summary>
        public void OnSystemEncountered(string systemName)
        {
            IncrementAchievement("faced_all_systems");
        }
        
        private void IncrementAchievement(string id)
        {
            if (!achievements.ContainsKey(id)) return;
            
            var achievement = achievements[id];
            if (achievement.IsUnlocked) return;
            
            achievement.Current++;
            
            if (achievement.Current >= achievement.Requirement)
            {
                UnlockAchievement(achievement);
            }
        }
        
        private void UnlockAchievement(AchievementProgress achievement)
        {
            achievement.IsUnlocked = true;
            
            GD.Print("\n╔═══════════════════════════════════════════════════════╗");
            GD.Print($"║  🏆 ACHIEVEMENT UNLOCKED: {achievement.Name,30} ║");
            GD.Print("╠═══════════════════════════════════════════════════════╣");
            GD.Print($"║  {achievement.Description,49} ║");
            GD.Print("╚═══════════════════════════════════════════════════════╝\n");
            
            EmitSignal(SignalName.AchievementUnlocked, achievement.Id, achievement.Name);
        }
        
        /// <summary>
        /// Get all achievements
        /// </summary>
        public List<AchievementProgress> GetAllAchievements()
        {
            return new List<AchievementProgress>(achievements.Values);
        }
        
        /// <summary>
        /// Get achievement progress
        /// </summary>
        public AchievementProgress GetAchievement(string id)
        {
            return achievements.ContainsKey(id) ? achievements[id] : null;
        }
        
        /// <summary>
        /// Print achievement summary
        /// </summary>
        public void PrintSummary()
        {
            int total = achievements.Count;
            int unlocked = achievements.Values.Where(a => a.IsUnlocked).ToList().Count;
            float percentage = (float)unlocked / total * 100f;
            
            GD.Print("\n╔═══════════════════════════════════════════════════════╗");
            GD.Print("║           ADVANCED AI ACHIEVEMENTS SUMMARY            ║");
            GD.Print("╚═══════════════════════════════════════════════════════╝\n");
            
            GD.Print($"Progress: {unlocked}/{total} ({percentage:F1}%)\n");
            
            // Group by category
            var categories = new Dictionary<string, List<AchievementProgress>>
            {
                ["Prediction"] = new List<AchievementProgress>(),
                ["Learning"] = new List<AchievementProgress>(),
                ["Combat"] = new List<AchievementProgress>(),
                ["Coordination"] = new List<AchievementProgress>(),
                ["Personality"] = new List<AchievementProgress>(),
                ["Victory"] = new List<AchievementProgress>(),
                ["Other"] = new List<AchievementProgress>()
            };
            
            foreach (var achievement in achievements.Values)
            {
                string category = "Other";
                if (achievement.Id.Contains("predicted")) category = "Prediction";
                else if (achievement.Id.Contains("pattern")) category = "Learning";
                else if (achievement.Id.Contains("combo") || achievement.Id.Contains("risky")) category = "Combat";
                else if (achievement.Id.Contains("synchronized") || achievement.Id.Contains("focus") || achievement.Id.Contains("healer")) category = "Coordination";
                else if (achievement.Id.Contains("personality") || achievement.Id.Contains("enraged")) category = "Personality";
                else if (achievement.Id.Contains("defeated") || achievement.Id.Contains("victory")) category = "Victory";
                
                categories[category].Add(achievement);
            }
            
            foreach (var category in categories)
            {
                if (category.Value.Count == 0) continue;
                
                GD.Print($"=== {category.Key} ===");
                foreach (var achievement in category.Value)
                {
                    string status = achievement.IsUnlocked ? "✓" : " ";
                    string progress = achievement.Requirement > 1 ? $" ({achievement.Current}/{achievement.Requirement})" : "";
                    GD.Print($"  [{status}] {achievement.Name}{progress}");
                    if (!achievement.IsUnlocked)
                        GD.Print($"      {achievement.Description}");
                }
                GD.Print("");
            }
        }
    }
    
    public class AchievementProgress
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Requirement { get; set; }
        public int Current { get; set; }
        public bool IsUnlocked { get; set; }
        
        public float Progress => Requirement > 0 ? (float)Current / Requirement : 0f;
    }
}