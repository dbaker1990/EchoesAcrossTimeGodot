using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EchoesAcrossTime.Combat
{
    /// <summary>
    /// Battle performance metrics for ranking
    /// </summary>
    public class BattleMetrics
    {
        public int TurnsElapsed { get; set; }
        public int TotalDamageDealt { get; set; }
        public int TotalDamageTaken { get; set; }
        public int WeaknessHits { get; set; }
        public int CriticalHits { get; set; }
        public int TechnicalHits { get; set; }
        public int AllOutAttacks { get; set; }
        public int ShowtimeAttacks { get; set; }
        public int LimitBreaksUsed { get; set; }
        public int CharactersKO { get; set; }
        public int ItemsUsed { get; set; }
        public int OverkillDamage { get; set; }
        
        public bool NoDamageTaken => TotalDamageTaken == 0;
        public bool NoItemsUsed => ItemsUsed == 0;
        public bool NoKOs => CharactersKO == 0;
        
        public void Reset()
        {
            TurnsElapsed = 0;
            TotalDamageDealt = 0;
            TotalDamageTaken = 0;
            WeaknessHits = 0;
            CriticalHits = 0;
            TechnicalHits = 0;
            AllOutAttacks = 0;
            ShowtimeAttacks = 0;
            LimitBreaksUsed = 0;
            CharactersKO = 0;
            ItemsUsed = 0;
            OverkillDamage = 0;
        }
    }
    
    /// <summary>
    /// Battle ranking from F to S+
    /// </summary>
    public enum BattleRank
    {
        F,      // Poor performance
        D,      // Below average
        C,      // Average
        B,      // Good
        A,      // Great
        S,      // Excellent
        SPlus   // Perfect
    }
    
    /// <summary>
    /// Results of a battle including rewards and ranking
    /// </summary>
    public class BattleRewardsResult
    {
        public BattleRank Rank { get; set; }
        public int RankScore { get; set; }
        public int TotalExp { get; set; }
        public int TotalGold { get; set; }
        public List<(string itemId, int quantity)> ItemDrops { get; set; }
        public Dictionary<string, int> ExpPerCharacter { get; set; }
        public List<string> CharactersLeveledUp { get; set; }
        public BattleMetrics Metrics { get; set; }
        
        // Bonus multipliers
        public float ExpBonus { get; set; } = 1.0f;
        public float GoldBonus { get; set; } = 1.0f;
        public float DropRateBonus { get; set; } = 1.0f;
        
        public BattleRewardsResult()
        {
            ItemDrops = new List<(string, int)>();
            ExpPerCharacter = new Dictionary<string, int>();
            CharactersLeveledUp = new List<string>();
        }
    }
    
    /// <summary>
    /// Manages battle rewards, exp distribution, and ranking
    /// </summary>
    public partial class BattleRewardsManager : Node
    {
        private BattleMetrics currentMetrics = new BattleMetrics();
        private RandomNumberGenerator rng = new RandomNumberGenerator();
        
        // Ranking thresholds
        private const int RANK_S_PLUS_THRESHOLD = 10000;
        private const int RANK_S_THRESHOLD = 7000;
        private const int RANK_A_THRESHOLD = 5000;
        private const int RANK_B_THRESHOLD = 3000;
        private const int RANK_C_THRESHOLD = 1500;
        private const int RANK_D_THRESHOLD = 500;
        
        // Score values
        private const int SCORE_PER_WEAKNESS = 200;
        private const int SCORE_PER_CRITICAL = 150;
        private const int SCORE_PER_TECHNICAL = 300;
        private const int SCORE_PER_ALL_OUT = 500;
        private const int SCORE_PER_SHOWTIME = 800;
        private const int SCORE_PER_LIMIT_BREAK = 600;
        private const int SCORE_NO_DAMAGE = 2000;
        private const int SCORE_NO_KO = 1500;
        private const int SCORE_NO_ITEMS = 1000;
        private const int SCORE_FAST_BATTLE = 100; // Per turn under threshold
        
        // Result property for accessing after calculation
        public BattleRewardsResult LastRewardsResult { get; private set; }
        
        [Signal]
        public delegate void RewardsCalculatedEventHandler();
        
        [Signal]
        public delegate void CharacterLeveledUpEventHandler(string characterName, int newLevel);
        
        [Signal]
        public delegate void ExpGainedEventHandler(string characterName, int expGained);
        
        [Signal]
        public delegate void ItemObtainedEventHandler(string itemId, int quantity);
        
        [Signal]
        public delegate void GoldObtainedEventHandler(int amount);
        
        public override void _Ready()
        {
            rng.Randomize();
        }
        
        /// <summary>
        /// Get current battle metrics
        /// </summary>
        public BattleMetrics GetMetrics()
        {
            return currentMetrics;
        }
        
        /// <summary>
        /// Reset metrics for new battle
        /// </summary>
        public void ResetMetrics()
        {
            currentMetrics.Reset();
        }
        
        /// <summary>
        /// Record battle event for metrics
        /// </summary>
        public void RecordEvent(string eventType)
        {
            switch (eventType.ToLower())
            {
                case "weakness_hit":
                    currentMetrics.WeaknessHits++;
                    break;
                case "critical_hit":
                    currentMetrics.CriticalHits++;
                    break;
                case "technical_hit":
                    currentMetrics.TechnicalHits++;
                    break;
                case "all_out_attack":
                    currentMetrics.AllOutAttacks++;
                    break;
                case "showtime":
                    currentMetrics.ShowtimeAttacks++;
                    break;
                case "limit_break":
                    currentMetrics.LimitBreaksUsed++;
                    break;
                case "character_ko":
                    currentMetrics.CharactersKO++;
                    break;
                case "item_used":
                    currentMetrics.ItemsUsed++;
                    break;
            }
        }
        
        /// <summary>
        /// Record damage dealt/taken
        /// </summary>
        public void RecordDamage(int damage, bool isPlayer)
        {
            if (isPlayer)
                currentMetrics.TotalDamageDealt += damage;
            else
                currentMetrics.TotalDamageTaken += damage;
        }
        
        /// <summary>
        /// Record turn elapsed
        /// </summary>
        public void RecordTurn()
        {
            currentMetrics.TurnsElapsed++;
        }
        
        /// <summary>
        /// Calculate final battle rewards and ranking
        /// </summary>
        public BattleRewardsResult CalculateRewards(
            List<CharacterStats> party,
            List<BattleMember> defeatedEnemies,
            bool isBossBattle = false)
        {
            var result = new BattleRewardsResult
            {
                Metrics = currentMetrics
            };
            
            // Calculate battle rank
            int rankScore = CalculateRankScore(currentMetrics, isBossBattle);
            result.RankScore = rankScore;
            result.Rank = GetRankFromScore(rankScore);
            
            // Apply rank bonuses
            ApplyRankBonuses(result);
            
            // Calculate total exp and gold from all enemies
            int totalExp = 0;
            int totalGold = 0;
            
            foreach (var enemy in defeatedEnemies)
            {
                if (enemy.Stats is CharacterData enemyData && enemyData.Rewards != null)
                {
                    totalExp += enemyData.Rewards.GetExpReward(rng);
                    totalGold += enemyData.Rewards.GetGoldReward(rng);
                    
                    // Roll for drops
                    var drops = enemyData.Rewards.RollAllDrops(rng);
                    foreach (var drop in drops)
                    {
                        result.ItemDrops.Add(drop);
                    }
                }
            }
            
            // Apply bonuses
            result.TotalExp = Mathf.RoundToInt(totalExp * result.ExpBonus);
            result.TotalGold = Mathf.RoundToInt(totalGold * result.GoldBonus);
            
            // Distribute exp to party
            DistributeExp(party, result.TotalExp, result);
            
            // Store result for access via property
            LastRewardsResult = result;
            
            // Emit signals
            EmitSignal(SignalName.RewardsCalculated);
            EmitSignal(SignalName.GoldObtained, result.TotalGold);
            
            foreach (var drop in result.ItemDrops)
            {
                EmitSignal(SignalName.ItemObtained, drop.itemId, drop.quantity);
            }
            
            return result;
        }
        
        /// <summary>
        /// Calculate battle rank score
        /// </summary>
        private int CalculateRankScore(BattleMetrics metrics, bool isBoss)
        {
            int score = 0;
            
            // Positive actions
            score += metrics.WeaknessHits * SCORE_PER_WEAKNESS;
            score += metrics.CriticalHits * SCORE_PER_CRITICAL;
            score += metrics.TechnicalHits * SCORE_PER_TECHNICAL;
            score += metrics.AllOutAttacks * SCORE_PER_ALL_OUT;
            score += metrics.ShowtimeAttacks * SCORE_PER_SHOWTIME;
            score += metrics.LimitBreaksUsed * SCORE_PER_LIMIT_BREAK;
            
            // Perfect performance bonuses
            if (metrics.NoDamageTaken)
                score += SCORE_NO_DAMAGE;
            
            if (metrics.NoKOs)
                score += SCORE_NO_KO;
            
            if (metrics.NoItemsUsed)
                score += SCORE_NO_ITEMS;
            
            // Speed bonus (battles should be 5-15 turns ideally)
            int turnThreshold = isBoss ? 20 : 10;
            if (metrics.TurnsElapsed < turnThreshold)
            {
                int turnsUnder = turnThreshold - metrics.TurnsElapsed;
                score += turnsUnder * SCORE_FAST_BATTLE;
            }
            
            // Damage efficiency bonus
            if (metrics.TotalDamageTaken > 0)
            {
                float damageRatio = (float)metrics.TotalDamageDealt / metrics.TotalDamageTaken;
                if (damageRatio > 5.0f) // Dealt 5x more than received
                    score += 1000;
                else if (damageRatio > 3.0f)
                    score += 500;
            }
            
            // Penalties
            score -= metrics.CharactersKO * 500;
            score -= Mathf.Max(0, (metrics.TurnsElapsed - turnThreshold) * 50); // Penalty for slow battles
            
            return Mathf.Max(0, score);
        }
        
        /// <summary>
        /// Convert score to rank
        /// </summary>
        private BattleRank GetRankFromScore(int score)
        {
            if (score >= RANK_S_PLUS_THRESHOLD) return BattleRank.SPlus;
            if (score >= RANK_S_THRESHOLD) return BattleRank.S;
            if (score >= RANK_A_THRESHOLD) return BattleRank.A;
            if (score >= RANK_B_THRESHOLD) return BattleRank.B;
            if (score >= RANK_C_THRESHOLD) return BattleRank.C;
            if (score >= RANK_D_THRESHOLD) return BattleRank.D;
            return BattleRank.F;
        }
        
        /// <summary>
        /// Apply bonuses based on battle rank
        /// </summary>
        private void ApplyRankBonuses(BattleRewardsResult result)
        {
            switch (result.Rank)
            {
                case BattleRank.SPlus:
                    result.ExpBonus = 2.0f;
                    result.GoldBonus = 2.0f;
                    result.DropRateBonus = 1.5f;
                    break;
                case BattleRank.S:
                    result.ExpBonus = 1.75f;
                    result.GoldBonus = 1.75f;
                    result.DropRateBonus = 1.3f;
                    break;
                case BattleRank.A:
                    result.ExpBonus = 1.5f;
                    result.GoldBonus = 1.5f;
                    result.DropRateBonus = 1.2f;
                    break;
                case BattleRank.B:
                    result.ExpBonus = 1.25f;
                    result.GoldBonus = 1.25f;
                    result.DropRateBonus = 1.1f;
                    break;
                case BattleRank.C:
                    result.ExpBonus = 1.0f;
                    result.GoldBonus = 1.0f;
                    result.DropRateBonus = 1.0f;
                    break;
                case BattleRank.D:
                    result.ExpBonus = 0.8f;
                    result.GoldBonus = 0.8f;
                    result.DropRateBonus = 0.9f;
                    break;
                case BattleRank.F:
                    result.ExpBonus = 0.5f;
                    result.GoldBonus = 0.5f;
                    result.DropRateBonus = 0.8f;
                    break;
            }
        }
        
        /// <summary>
        /// Distribute exp to all living party members
        /// </summary>
        private void DistributeExp(List<CharacterStats> party, int totalExp, BattleRewardsResult result)
        {
            var livingMembers = party.Where(p => p.IsAlive).ToList();
    
            if (livingMembers.Count == 0) return;
    
            int expPerMember = totalExp / livingMembers.Count;
    
            foreach (var member in livingMembers)
            {
                // Just store the exp - you'll apply it manually in your game code
                result.ExpPerCharacter[member.CharacterName] = expPerMember;
                EmitSignal(SignalName.ExpGained, member.CharacterName, expPerMember);
        
                GD.Print($"{member.CharacterName} gained {expPerMember} EXP");
        
                // TODO: Apply exp manually using your actual exp system
            }
        }
        
        /// <summary>
        /// Get rank display string with color
        /// </summary>
        public static string GetRankDisplay(BattleRank rank)
        {
            return rank switch
            {
                BattleRank.SPlus => "S+",
                BattleRank.S => "S",
                BattleRank.A => "A",
                BattleRank.B => "B",
                BattleRank.C => "C",
                BattleRank.D => "D",
                BattleRank.F => "F",
                _ => "?"
            };
        }
        
        /// <summary>
        /// Get rank color for UI
        /// </summary>
        public static Color GetRankColor(BattleRank rank)
        {
            return rank switch
            {
                BattleRank.SPlus => new Color(1.0f, 0.84f, 0.0f), // Gold
                BattleRank.S => new Color(1.0f, 0.4f, 0.4f), // Red
                BattleRank.A => new Color(0.2f, 0.8f, 1.0f), // Cyan
                BattleRank.B => new Color(0.2f, 1.0f, 0.2f), // Green
                BattleRank.C => new Color(1.0f, 1.0f, 0.4f), // Yellow
                BattleRank.D => new Color(1.0f, 0.6f, 0.2f), // Orange
                BattleRank.F => new Color(0.6f, 0.6f, 0.6f), // Gray
                _ => Colors.White
            };
        }
    }
}