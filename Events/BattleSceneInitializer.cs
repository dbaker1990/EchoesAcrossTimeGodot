// Events/BattleSceneInitializer.cs - COMPLETE FIX FOR ALL ERRORS
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using EchoesAcrossTime.Combat;
using EchoesAcrossTime.Events;
using EchoesAcrossTime.Managers;

namespace EchoesAcrossTime.Combat
{
    /// <summary>
    /// Attach this to your BattleScene root node
    /// It will automatically initialize the battle from event battle parameters
    /// </summary>
    public partial class BattleSceneInitializer : Node
    {
        [Export] private BattleManager battleManager;
        [Export] private Node2D battleBackground;
        [Export] private string defaultBattleScenePath = "res://Combat/BattleScene.tscn";
        
        public override void _Ready()
        {
            if (battleManager == null)
            {
                battleManager = GetNode<BattleManager>("BattleManager");
            }
            
            if (battleManager == null)
            {
                GD.PrintErr("[BattleSceneInitializer] BattleManager not found!");
                ReturnToOverworld();
                return;
            }
            
            // Get battle parameters
            var playerParty = GetPlayerParty();
            var enemies = GetEnemyParty();
            
            if (playerParty.Count == 0 || enemies.Count == 0)
            {
                GD.PrintErr("[BattleSceneInitializer] Failed to create party or enemies!");
                ReturnToOverworld();
                return;
            }
            
            // FIX #1: Use positional parameters, not named parameters
            battleManager.InitializeBattle(
                playerParty,                                    // playerStats
                enemies,                                        // enemyStats
                GetAvailableShowtimes(),                       // availableShowtimes
                GetAvailableLimitBreaks(),                     // availableLimitBreaks
                GetBattleParam("IsBossBattle", false),        // isBossBattle
                !GetBattleParam("CanEscape", true)            // isPinnedDown
            );
            
            battleManager.BattleEnded += OnBattleEnded;
        }
        
        /// <summary>
        /// Get player party stats
        /// </summary>
        private List<CharacterStats> GetPlayerParty()
        {
            var partyStats = new List<CharacterStats>();
            
            // Use PartyMenuManager for party members
            if (PartyMenuManager.Instance != null)
            {
                var party = PartyMenuManager.Instance.GetMainParty();
                foreach (var member in party)
                {
                    if (member != null && member.Stats != null)
                    {
                        partyStats.Add(member.Stats);
                    }
                }
            }
            
            // FIX #2 & #3: SaveData uses `Party` not `PartyMembers` or `MainPartyIds`
            // Fallback: Load from current save
            if (partyStats.Count == 0 && GameManager.Instance?.CurrentSave != null)
            {
                var saveData = GameManager.Instance.CurrentSave;
                
                // SaveData.Party is List<PartyMemberSaveData>, not MainPartyIds
                if (saveData.Party != null && GameManager.Instance.Database != null)
                {
                    foreach (var memberData in saveData.Party)
                    {
                        if (memberData != null)
                        {
                            var stats = memberData.ToCharacterStats(GameManager.Instance.Database);
                            if (stats != null)
                            {
                                partyStats.Add(stats);
                            }
                        }
                    }
                }
            }
            
            GD.Print($"[BattleSceneInitializer] Created party with {partyStats.Count} members");
            return partyStats;
        }
        
        /// <summary>
        /// Get enemy party from battle parameters
        /// </summary>
        private List<CharacterStats> GetEnemyParty()
        {
            var enemyIds = GetBattleParam<Godot.Collections.Array<string>>("EnemyIds", null);
            if (enemyIds == null || enemyIds.Count == 0)
            {
                GD.PrintErr("[BattleSceneInitializer] No enemy IDs in battle parameters!");
                return new List<CharacterStats>();
            }
            
            return CreateEnemiesFromIds(enemyIds);
        }
        
        /// <summary>
        /// Create enemy CharacterStats from enemy IDs
        /// </summary>
        private List<CharacterStats> CreateEnemiesFromIds(Godot.Collections.Array<string> enemyIds)
        {
            var enemies = new List<CharacterStats>();
            
            if (GameManager.Instance?.Database == null)
            {
                GD.PrintErr("[BattleSceneInitializer] GameDatabase not found!");
                return enemies;
            }
            
            foreach (var enemyId in enemyIds)
            {
                var enemyData = GameManager.Instance.Database.GetCharacter(enemyId);
                if (enemyData != null)
                {
                    var stats = enemyData.CreateStatsInstance();
                    if (stats != null)
                    {
                        enemies.Add(stats);
                    }
                }
                else
                {
                    GD.PrintErr($"[BattleSceneInitializer] Enemy '{enemyId}' not found in database!");
                }
            }
            
            GD.Print($"[BattleSceneInitializer] Created {enemies.Count} enemies");
            return enemies;
        }
        
        /// <summary>
        /// Get available showtime attacks
        /// </summary>
        private List<ShowtimeAttackData> GetAvailableShowtimes()
        {
            // Implement based on your showtime system
            return new List<ShowtimeAttackData>();
        }
        
        /// <summary>
        /// Get available limit breaks
        /// </summary>
        private List<LimitBreakData> GetAvailableLimitBreaks()
        {
            // Implement based on your limit break system
            return new List<LimitBreakData>();
        }
        
        /// <summary>
        /// Get battle parameter with proper Godot variant handling
        /// FIX #7: Add [MustBeVariant] attribute for generic parameter
        /// </summary>
        private T GetBattleParam<[MustBeVariant] T>(string key, T defaultValue)
        {
            if (GameManager.Instance != null && GameManager.Instance.HasMeta("PendingBattleData"))
            {
                var battleData = GameManager.Instance.GetMeta("PendingBattleData").As<Godot.Collections.Dictionary>();
                if (battleData != null && battleData.ContainsKey(key))
                {
                    return battleData[key].As<T>();
                }
            }
            return defaultValue;
        }
        
        /// <summary>
        /// Called when battle ends
        /// </summary>
        private async void OnBattleEnded(bool victory)
        {
            GD.Print($"[BattleSceneInitializer] Battle ended. Victory: {victory}");
            
            // Set battle result
            var result = victory ? 
                EventCommandExecutor.BattleResult.Victory : 
                EventCommandExecutor.BattleResult.Defeat;
            
            if (battleManager.CurrentPhase == BattlePhase.Escaped)
            {
                result = EventCommandExecutor.BattleResult.Escape;
            }
            
            if (EventCommandExecutor.Instance != null)
            {
                EventCommandExecutor.Instance.SetBattleResult(result);
            }
            
            // Distribute rewards if victory
            if (victory && PartyMenuManager.Instance != null)
            {
                // FIX #4: Don't use BattleStats.ExpReward - calculate directly from enemy data
                var totalExp = CalculateTotalExpFromEnemies();
                PartyMenuManager.Instance.DistributeExperience(totalExp);
                
                // Gold and items are handled by BattleRewardsManager signals
            }
            
            // Wait for rewards screen
            await ToSignal(GetTree().CreateTimer(2.0f), SceneTreeTimer.SignalName.Timeout);
            
            // Return to overworld
            ReturnToOverworld();
        }
        
        /// <summary>
        /// FIX #4: Calculate total EXP from defeated enemies
        /// BattleStats doesn't have ExpReward - rewards come from elsewhere
        /// </summary>
        private int CalculateTotalExpFromEnemies()
        {
            int totalExp = 0;
            var enemyIds = GetBattleParam<Godot.Collections.Array<string>>("EnemyIds", null);
            
            if (enemyIds != null && GameManager.Instance?.Database != null)
            {
                foreach (var enemyId in enemyIds)
                {
                    var enemyData = GameManager.Instance.Database.GetCharacter(enemyId);
                    if (enemyData != null)
                    {
                        // Use level-based calculation as fallback
                        // BattleStats doesn't have ExpReward property
                        int enemyLevel = enemyData.Level;
                        int expFromEnemy = enemyLevel * 10; // Base calculation
                        totalExp += expFromEnemy;
                        
                        GD.Print($"[BattleSceneInitializer] {enemyData.DisplayName} (Lv{enemyLevel}) gives {expFromEnemy} EXP");
                    }
                }
            }
            
            GD.Print($"[BattleSceneInitializer] Total EXP: {totalExp}");
            return totalExp;
        }
        
        /// <summary>
        /// Return to the overworld scene
        /// </summary>
        private void ReturnToOverworld()
        {
            string returnScene = GetBattleParam("ReturnScene", "res://Maps/TestMap.tscn");
            GD.Print($"[BattleSceneInitializer] Returning to: {returnScene}");
            GetTree().ChangeSceneToFile(returnScene);
        }
        
        public override void _ExitTree()
        {
            if (battleManager != null)
            {
                battleManager.BattleEnded -= OnBattleEnded;
            }
        }
    }
}