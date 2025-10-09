// Combat/SummonManager.cs
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EchoesAcrossTime.Combat
{
    /// <summary>
    /// Manages active summons in battle
    /// </summary>
    public partial class SummonManager : Node
    {
        [Signal]
        public delegate void SummonSpawnedEventHandler(string summonName, string summonerName);
        
        [Signal]
        public delegate void SummonDismissedEventHandler(string summonName, string reason);
        
        [Signal]
        public delegate void SummonExpiredEventHandler(string summonName);
        
        [Signal]
        public delegate void SummonExplodedEventHandler(string summonName, int damage);

        // Track active summons
        private Dictionary<string, ActiveSummon> activeSummons = new Dictionary<string, ActiveSummon>();
        
        // Track summon cooldowns per summoner
        private Dictionary<string, Dictionary<string, int>> summonCooldowns = new Dictionary<string, Dictionary<string, int>>();
        
        private int currentTurn = 0;

        public override void _Ready()
        {
            GD.Print("SummonManager initialized");
        }

        /// <summary>
        /// Attempt to summon a creature
        /// </summary>
        public BattleActionResult ExecuteSummon(
            BattleMember summoner, 
            SummonData summonData, 
            BattleManager battleManager)
        {
            var result = new BattleActionResult { Success = false };

            // Validate summon
            if (!CanSummon(summoner, summonData, out string failReason))
            {
                result.Message = failReason;
                return result;
            }

            // Consume resources
            summoner.Stats.ConsumeMP(summonData.MPCost);
            if (summonData.HPCost > 0)
            {
                summoner.Stats.TakeDamage(summonData.HPCost, ElementType.Physical);
            }

            // Handle instant summons
            if (summonData.DurationType == SummonDuration.Instant)
            {
                return ExecuteInstantSummon(summoner, summonData, battleManager);
            }

            // Create persistent summon
            var summonStats = summonData.CreateSummonStats(summoner.Stats.Level);
            var summonMember = new BattleMember(summonStats, summoner.IsPlayerControlled, 
                summoner.IsPlayerControlled ? battleManager.GetPlayerParty().Count : battleManager.GetEnemyParty().Count);

            // Store summon reference data
            var activeSummon = new ActiveSummon
            {
                SummonMember = summonMember,
                SummonData = summonData,
                SummonerName = summoner.Stats.CharacterName,
                RemainingTurns = summonData.TurnDuration,
                SpawnTurn = currentTurn
            };

            activeSummons[summonMember.Stats.CharacterName] = activeSummon;

            // Add to battle via the battle manager's party lists
            if (summoner.IsPlayerControlled)
            {
                // Add to player party through BattleManager's GetPlayerParty() list
                var playerParty = battleManager.GetPlayerParty();
                if (!playerParty.Contains(summonMember))
                {
                    playerParty.Add(summonMember);
                }
            }
            else
            {
                // Add to enemy party through BattleManager's GetEnemyParty() list
                var enemyParty = battleManager.GetEnemyParty();
                if (!enemyParty.Contains(summonMember))
                {
                    enemyParty.Add(summonMember);
                }
            }

            // Rebuild turn order (you'll need to expose this or call StartNextTurn)
            battleManager.RebuildTurnOrder();

            // Set cooldown
            SetCooldown(summoner.Stats.CharacterName, summonData.SummonId, summonData.Cooldown);

            // Apply passive buffs
            ApplyPassiveBuffs(activeSummon, battleManager);

            result.Success = true;
            result.Message = $"{summoner.Stats.CharacterName} summoned {summonData.DisplayName}!";
            
            EmitSignal(SignalName.SummonSpawned, summonData.DisplayName, summoner.Stats.CharacterName);
            
            GD.Print($"✨ Summon spawned: {summonData.DisplayName}");
            
            return result;
        }

        /// <summary>
        /// Execute instant summon (appears, does effect, disappears)
        /// </summary>
        private BattleActionResult ExecuteInstantSummon(
            BattleMember summoner,
            SummonData summonData,
            BattleManager battleManager)
        {
            var result = new BattleActionResult { Success = true };
            
            GD.Print($"💫 {summonData.DisplayName} appears briefly!");

            // Get targets
            var targets = GetInstantSummonTargets(summonData, summoner.IsPlayerControlled, battleManager);

            // Apply instant damage
            if (summonData.InstantDamage > 0)
            {
                int totalDamage = 0;
                foreach (var target in targets)
                {
                    if (!target.Stats.IsAlive) continue;

                    int damage = CalculateInstantDamage(summonData, summoner.Stats, target.Stats);
                    target.Stats.TakeDamage(damage, summonData.PrimaryElement);
                    totalDamage += damage;
                    
                    GD.Print($"  → {target.Stats.CharacterName} takes {damage} damage!");
                }
                result.DamageDealt = totalDamage;
            }

            // Apply instant healing
            if (summonData.InstantHealing > 0)
            {
                int totalHealing = 0;
                foreach (var target in targets)
                {
                    // Revive if needed
                    if (!target.Stats.IsAlive && summonData.InstantHealing > 0)
                    {
                        target.Stats.CurrentHP = summonData.InstantHealing;
                        GD.Print($"  → {target.Stats.CharacterName} was revived!");
                    }
                    else if (target.Stats.IsAlive)
                    {
                        int healing = summonData.InstantHealing;
                        target.Stats.Heal(healing);
                        totalHealing += healing;
                        GD.Print($"  → {target.Stats.CharacterName} healed for {healing} HP!");
                    }
                }
                result.HealingDone = totalHealing;
            }

            // Apply instant statuses
            foreach (var status in summonData.InstantStatuses)
            {
                foreach (var target in targets)
                {
                    if (target.Stats.IsAlive)
                    {
                        StatusEffectManager.Instance?.ApplyStatus(
                            target.Stats, status, 3, 10, summonData.SummonId);
                    }
                }
            }

            result.Message = $"{summonData.DisplayName} appeared and unleashed its power!";
            
            // Set cooldown
            SetCooldown(summoner.Stats.CharacterName, summonData.SummonId, summonData.Cooldown);
            
            return result;
        }

        /// <summary>
        /// Process summons at start of turn
        /// </summary>
        public void ProcessTurnStart(BattleManager battleManager)
        {
            currentTurn++;
            var toRemove = new List<string>();

            foreach (var kvp in activeSummons)
            {
                var summon = kvp.Value;
                
                // Decrease duration
                if (summon.SummonData.DurationType != SummonDuration.Permanent)
                {
                    summon.RemainingTurns--;
                    
                    if (summon.RemainingTurns <= 0)
                    {
                        DismissSummon(summon.SummonMember, "expired", battleManager);
                        toRemove.Add(kvp.Key);
                        continue;
                    }
                }

                // Apply passive effects
                ApplyPassiveEffects(summon, battleManager);
            }

            // Remove expired summons
            foreach (var key in toRemove)
            {
                activeSummons.Remove(key);
            }

            // Decrease cooldowns
            DecreaseCooldowns();
        }

        /// <summary>
        /// Handle summon death
        /// </summary>
        public void OnSummonDefeated(BattleMember summon, BattleManager battleManager)
        {
            if (!activeSummons.ContainsKey(summon.Stats.CharacterName))
                return;

            var activeSummon = activeSummons[summon.Stats.CharacterName];

            // Handle explosion
            if (activeSummon.SummonData.ExplodesOnDeath)
            {
                HandleExplosion(activeSummon, battleManager);
            }

            DismissSummon(summon, "defeated", battleManager);
            activeSummons.Remove(summon.Stats.CharacterName);
        }

        /// <summary>
        /// Handle summoner death
        /// </summary>
        public void OnSummonerDefeated(string summonerName, BattleManager battleManager)
        {
            var summonsToDismiss = activeSummons.Values
                .Where(s => s.SummonerName == summonerName && s.SummonData.DiesWhenSummonerDies)
                .ToList();

            foreach (var summon in summonsToDismiss)
            {
                DismissSummon(summon.SummonMember, "summoner_died", battleManager);
                activeSummons.Remove(summon.SummonMember.Stats.CharacterName);
            }
        }

        /// <summary>
        /// Dismiss a summon from battle
        /// </summary>
        private void DismissSummon(BattleMember summon, string reason, BattleManager battleManager)
        {
            // Remove passive buffs
            RemovePassiveBuffs(summon, battleManager);

            // Remove from party lists
            battleManager.GetPlayerParty().Remove(summon);
            battleManager.GetEnemyParty().Remove(summon);
            
            // Rebuild turn order
            battleManager.RebuildTurnOrder();

            EmitSignal(SignalName.SummonDismissed, summon.Stats.CharacterName, reason);
            
            GD.Print($"💨 {summon.Stats.CharacterName} has been dismissed ({reason})");
        }

        /// <summary>
        /// Handle summon explosion on death
        /// </summary>
        private void HandleExplosion(ActiveSummon summon, BattleManager battleManager)
        {
            int damage = summon.SummonData.ExplosionDamage;
            
            // Get targets (enemies if player summon, players if enemy summon)
            var targets = summon.SummonMember.IsPlayerControlled
                ? battleManager.GetLivingEnemies()
                : battleManager.GetLivingAllies();

            GD.Print($"💥 {summon.SummonMember.Stats.CharacterName} explodes!");
            
            foreach (var target in targets)
            {
                target.Stats.TakeDamage(damage, summon.SummonData.PrimaryElement);
                GD.Print($"  → {target.Stats.CharacterName} takes {damage} explosion damage!");
            }

            EmitSignal(SignalName.SummonExploded, summon.SummonMember.Stats.CharacterName, damage);
        }

        /// <summary>
        /// Check if summon can be used
        /// </summary>
        private bool CanSummon(BattleMember summoner, SummonData summonData, out string reason)
        {
            // Check basic requirements
            if (!summonData.CanSummon(summoner.Stats))
            {
                reason = "Insufficient resources or level too low!";
                return false;
            }

            // Check cooldown
            if (IsOnCooldown(summoner.Stats.CharacterName, summonData.SummonId))
            {
                int remaining = GetRemainingCooldown(summoner.Stats.CharacterName, summonData.SummonId);
                reason = $"{summonData.DisplayName} is on cooldown ({remaining} turns remaining)!";
                return false;
            }

            // Check if summon is already active (can't have duplicates)
            bool alreadyActive = activeSummons.Values.Any(s => 
                s.SummonData.SummonId == summonData.SummonId && 
                s.SummonerName == summoner.Stats.CharacterName);
            
            if (alreadyActive)
            {
                reason = $"{summonData.DisplayName} is already summoned!";
                return false;
            }

            reason = "";
            return true;
        }

        // Cooldown management
        private void SetCooldown(string summonerId, string summonId, int turns)
        {
            if (!summonCooldowns.ContainsKey(summonerId))
            {
                summonCooldowns[summonerId] = new Dictionary<string, int>();
            }
            summonCooldowns[summonerId][summonId] = turns;
        }

        private bool IsOnCooldown(string summonerId, string summonId)
        {
            return summonCooldowns.ContainsKey(summonerId) &&
                   summonCooldowns[summonerId].ContainsKey(summonId) &&
                   summonCooldowns[summonerId][summonId] > 0;
        }

        private int GetRemainingCooldown(string summonerId, string summonId)
        {
            if (IsOnCooldown(summonerId, summonId))
            {
                return summonCooldowns[summonerId][summonId];
            }
            return 0;
        }

        private void DecreaseCooldowns()
        {
            foreach (var summonerCooldowns in summonCooldowns.Values)
            {
                var keys = summonerCooldowns.Keys.ToList();
                foreach (var summonId in keys)
                {
                    if (summonerCooldowns[summonId] > 0)
                    {
                        summonerCooldowns[summonId]--;
                    }
                }
            }
        }

        // Passive effects
        private void ApplyPassiveEffects(ActiveSummon summon, BattleManager battleManager)
        {
            var data = summon.SummonData;
            var allies = summon.SummonMember.IsPlayerControlled
                ? battleManager.GetLivingAllies()
                : battleManager.GetLivingEnemies();

            // Passive healing
            if (data.PassiveHealPerTurn > 0)
            {
                foreach (var ally in allies)
                {
                    ally.Stats.Heal(data.PassiveHealPerTurn);
                }
            }

            // Passive damage
            if (data.PassiveDamagePerTurn > 0)
            {
                var enemies = summon.SummonMember.IsPlayerControlled
                    ? battleManager.GetLivingEnemies()
                    : battleManager.GetLivingAllies();
                
                foreach (var enemy in enemies)
                {
                    enemy.Stats.TakeDamage(data.PassiveDamagePerTurn, data.PrimaryElement);
                }
            }
        }

        private void ApplyPassiveBuffs(ActiveSummon summon, BattleManager battleManager)
        {
            // Would implement stat buffs here
            // This would need integration with your buff system
        }

        private void RemovePassiveBuffs(BattleMember summon, BattleManager battleManager)
        {
            // Would remove stat buffs here
        }

        // Helper methods
        private List<BattleMember> GetInstantSummonTargets(
            SummonData summonData, 
            bool isPlayerSummon,
            BattleManager battleManager)
        {
            return summonData.InstantTarget switch
            {
                SkillTarget.AllEnemies => isPlayerSummon 
                    ? battleManager.GetLivingEnemies() 
                    : battleManager.GetLivingAllies(),
                SkillTarget.AllAllies => isPlayerSummon 
                    ? battleManager.GetPlayerParty().Where(p => p.Stats.IsAlive || summonData.InstantHealing > 0).ToList()
                    : battleManager.GetEnemyParty().Where(e => e.Stats.IsAlive).ToList(),
                _ => new List<BattleMember>()
            };
        }

        private int CalculateInstantDamage(SummonData summonData, CharacterStats summoner, CharacterStats target)
        {
            // Base damage
            int damage = summonData.InstantDamage;
            
            // Scale with summoner's magic attack
            damage += summoner.MagicAttack / 2;
            
            // Apply element affinity
            var affinity = target.ElementAffinities.GetAffinity(summonData.PrimaryElement);
            float multiplier = target.ElementAffinities.GetDamageMultiplier(summonData.PrimaryElement);
            damage = Mathf.RoundToInt(damage * multiplier);
            
            return Mathf.Max(1, damage);
        }

        /// <summary>
        /// Get all active summons
        /// </summary>
        public List<ActiveSummon> GetActiveSummons()
        {
            return activeSummons.Values.ToList();
        }

        /// <summary>
        /// Get active summons for a specific summoner
        /// </summary>
        public List<ActiveSummon> GetSummonsBySummoner(string summonerName)
        {
            return activeSummons.Values
                .Where(s => s.SummonerName == summonerName)
                .ToList();
        }

        /// <summary>
        /// Clear all summons (for battle end)
        /// </summary>
        public void ClearAllSummons()
        {
            activeSummons.Clear();
            summonCooldowns.Clear();
            currentTurn = 0;
        }
    }

    /// <summary>
    /// Tracks an active summon in battle
    /// </summary>
    public class ActiveSummon
    {
        public BattleMember SummonMember { get; set; }
        public SummonData SummonData { get; set; }
        public string SummonerName { get; set; }
        public int RemainingTurns { get; set; }
        public int SpawnTurn { get; set; }
    }
}