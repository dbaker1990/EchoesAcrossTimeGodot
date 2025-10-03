using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EchoesAcrossTime.Combat
{
    /// <summary>
    /// Result of an escape attempt
    /// </summary>
    public class EscapeResult
    {
        public bool Success { get; set; }
        public bool CanEscape { get; set; } = true;
        public string Message { get; set; }
        public int EscapeChance { get; set; }
    }
    
    /// <summary>
    /// Handles escaping from battles
    /// </summary>
    public class EscapeSystem
    {
        private const int BASE_ESCAPE_CHANCE = 50;
        private const int ESCAPE_CHANCE_INCREMENT = 10; // +10% per failed attempt
        private const int MAX_ESCAPE_CHANCE = 95;
        
        private int escapeAttempts = 0;
        private RandomNumberGenerator rng;
        
        public EscapeSystem()
        {
            rng = new RandomNumberGenerator();
        }
        
        /// <summary>
        /// Reset escape attempts (for new battle)
        /// </summary>
        public void ResetAttempts()
        {
            escapeAttempts = 0;
        }
        
        /// <summary>
        /// Attempt to escape from battle
        /// </summary>
        public EscapeResult AttemptEscape(
            List<BattleMember> party, 
            List<BattleMember> enemies,
            bool isBossBattle = false,
            bool isPinnedDown = false)
        {
            var result = new EscapeResult();
            
            // Check if escape is possible
            if (isBossBattle)
            {
                result.Success = false;
                result.CanEscape = false;
                result.Message = "Can't escape from a boss battle!";
                GD.Print("❌ " + result.Message);
                return result;
            }
            
            if (isPinnedDown)
            {
                result.Success = false;
                result.CanEscape = false;
                result.Message = "The party is pinned down! Can't escape!";
                GD.Print("❌ " + result.Message);
                return result;
            }
            
            // Calculate escape chance
            result.EscapeChance = CalculateEscapeChance(party, enemies);
            
            // Roll for escape
            float roll = rng.Randf() * 100;
            result.Success = roll < result.EscapeChance;
            
            escapeAttempts++;
            
            if (result.Success)
            {
                result.Message = "Successfully escaped from battle!";
                GD.Print($"\n✅ {result.Message}");
                GD.Print($"   (Rolled {roll:F0} vs {result.EscapeChance}% chance)");
            }
            else
            {
                result.Message = "Failed to escape!";
                GD.Print($"\n❌ {result.Message}");
                GD.Print($"   (Rolled {roll:F0} vs {result.EscapeChance}% chance)");
                GD.Print($"   Escape attempts: {escapeAttempts} (+10% next time)");
            }
            
            return result;
        }
        
        /// <summary>
        /// Calculate chance to escape based on party speed vs enemy speed
        /// </summary>
        private int CalculateEscapeChance(List<BattleMember> party, List<BattleMember> enemies)
        {
            // Get average party speed
            var livingParty = party.Where(p => p.Stats.IsAlive).ToList();
            if (livingParty.Count == 0)
                return 0;
            
            float avgPartySpeed = livingParty.Average(p => p.Stats.Speed);
            
            // Get average enemy speed
            var livingEnemies = enemies.Where(e => e.Stats.IsAlive).ToList();
            if (livingEnemies.Count == 0)
                return 100; // All enemies dead = auto escape
            
            float avgEnemySpeed = livingEnemies.Average(e => e.Stats.Speed);
            
            // Calculate base chance
            int escapeChance = BASE_ESCAPE_CHANCE;
            
            // Speed difference modifier
            float speedRatio = avgPartySpeed / avgEnemySpeed;
            
            if (speedRatio > 1.0f)
            {
                // Party is faster - bonus to escape
                int speedBonus = Mathf.RoundToInt((speedRatio - 1.0f) * 50);
                escapeChance += speedBonus;
            }
            else if (speedRatio < 1.0f)
            {
                // Enemies are faster - penalty to escape
                int speedPenalty = Mathf.RoundToInt((1.0f - speedRatio) * 30);
                escapeChance -= speedPenalty;
            }
            
            // Bonus for each failed attempt
            escapeChance += (escapeAttempts * ESCAPE_CHANCE_INCREMENT);
            
            // HP penalty - harder to escape when injured
            float avgHPPercent = livingParty.Average(p => p.Stats.HPPercent);
            if (avgHPPercent < 0.5f)
            {
                int hpPenalty = Mathf.RoundToInt((0.5f - avgHPPercent) * 40);
                escapeChance -= hpPenalty;
            }
            
            // Clamp between 10% and 95%
            escapeChance = Mathf.Clamp(escapeChance, 10, MAX_ESCAPE_CHANCE);
            
            return escapeChance;
        }
        
        /// <summary>
        /// Check if party is eligible to escape
        /// </summary>
        public bool CanAttemptEscape(
            List<BattleMember> party,
            bool isBossBattle,
            bool isPinnedDown)
        {
            if (isBossBattle || isPinnedDown)
                return false;
            
            // Need at least one living party member
            return party.Any(p => p.Stats.IsAlive);
        }
        
        /// <summary>
        /// Get escape chance for UI display
        /// </summary>
        public int GetEscapeChanceDisplay(List<BattleMember> party, List<BattleMember> enemies)
        {
            return CalculateEscapeChance(party, enemies);
        }
    }
}