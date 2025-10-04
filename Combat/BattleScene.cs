using Godot;
using Godot.Collections;
using System.Linq;
using EchoesAcrossTime.Combat;
using EchoesAcrossTime.Encounters;
using EchoesAcrossTime.Database;

/// <summary>
/// Attach this to the root Node2D of BattleScene.tscn
/// Handles encounter battles and returns to overworld
/// </summary>
public partial class BattleScene : Node2D
{
    #region Private Fields
    
    private BattleManager battleManager;
    private Dictionary encounterData;
    
    #endregion
    
    #region Godot Lifecycle
    
    public override void _Ready()
    {
        // Get battle manager (child of this scene)
        battleManager = GetNode<BattleManager>("BattleManager");
        
        if (battleManager == null)
        {
            GD.PrintErr("[BattleScene] BattleManager not found!");
            return;
        }
        
        // Connect signals
        battleManager.BattleEnded += OnBattleEnded;
        
        // Check if this is an encounter battle
        if (GetTree().Root.HasMeta("EncounterData"))
        {
            encounterData = GetTree().Root.GetMeta("EncounterData").AsGodotDictionary();
            GetTree().Root.RemoveMeta("EncounterData"); // Clean up
            
            InitializeEncounterBattle();
        }
        else
        {
            // Story battle or other type - you'll need to handle this yourself
            GD.Print("[BattleScene] No encounter data - this is a story battle");
            // For story battles, you'd set up the battle manually here
        }
    }
    
    #endregion
        
        #region Encounter Battle Setup
        
        /// <summary>
        /// Initialize battle from encounter data
        /// </summary>
        private void InitializeEncounterBattle()
        {
            if (encounterData == null)
                return;
            
            string zoneName = encounterData["ZoneName"].AsString();
            var enemyIds = encounterData["EnemyIds"].AsGodotArray();
            bool isBossBattle = encounterData["IsBossBattle"].AsBool();
            bool canEscape = encounterData["CanEscape"].AsBool();
            
            GD.Print($"[BattleScene] Setting up encounter: {zoneName}");
            
            // Get player party
            var playerParty = GetPlayerParty();
            
            // Create enemy party from encounter data
            var enemyParty = CreateEnemyParty(enemyIds);
            
            if (enemyParty.Count == 0)
            {
                GD.PrintErr("[BattleScene] No valid enemies created!");
                ReturnToOverworld(true); // Return immediately
                return;
            }
            
            // Play encounter music
            // If you have AudioManager singleton:
            // AudioManager.Instance?.PlayBGM(currentEncounter.BattleMusic, 1.0f, 1.0f);
            
            // Initialize battle (showtimes and limit breaks are optional - set to null for now)
            battleManager.InitializeBattle(
                playerParty,
                enemyParty,
                null, // showtimes - add later if you want
                null, // limit breaks - add later if you want
                isBossBattle,
                !canEscape
            );
            
            GD.Print($"[BattleScene] Battle started: {playerParty.Count} vs {enemyParty.Count}");
        }
        
        #endregion
        
        #region Party Creation
        
        /// <summary>
        /// Get the player's active party
        /// TODO: Replace this with your actual party system!
        /// </summary>
        private System.Collections.Generic.List<CharacterStats> GetPlayerParty()
        {
            // OPTION 1: If you have a PartyManager singleton
            // return PartyManager.Instance?.GetActivePartyStats();
            
            // OPTION 2: If you have a GameManager with save data
            // return GameManager.Instance?.CurrentSave?.GetPartyStats();
            
            // OPTION 3: Temporary fallback - just get Dominic for testing
            var party = new System.Collections.Generic.List<CharacterStats>();
            
            var database = GetNode<GameDatabase>("/root/GameDatabase");
            if (database != null)
            {
                var dominic = database.GetCharacter("dominic");
                if (dominic != null)
                {
                    party.Add(dominic.CreateStatsInstance());
                    GD.Print("[BattleScene] Created party with Dominic");
                }
            }
            
            return party;
        }
        
        /// <summary>
        /// Create enemy party from enemy IDs
        /// </summary>
        private System.Collections.Generic.List<CharacterStats> CreateEnemyParty(Array enemyIds)
        {
            var enemies = new System.Collections.Generic.List<CharacterStats>();
            
            if (enemyIds == null || enemyIds.Count == 0)
            {
                GD.PrintErr("[BattleScene] No enemy IDs provided!");
                return enemies;
            }
            
            // Get GameDatabase singleton
            var database = GetNode<GameDatabase>("/root/GameDatabase");
            if (database == null)
            {
                GD.PrintErr("[BattleScene] GameDatabase not found!");
                return enemies;
            }
            
            foreach (var enemyIdVariant in enemyIds)
            {
                string enemyId = enemyIdVariant.AsString();
                var enemyData = database.GetCharacter(enemyId);
                
                if (enemyData != null)
                {
                    var stats = enemyData.CreateStatsInstance();
                    enemies.Add(stats);
                    GD.Print($"  Added enemy: {stats.CharacterName} (Lv.{stats.Level})");
                }
                else
                {
                    GD.PrintErr($"[BattleScene] Enemy not found in database: {enemyId}");
                }
            }
            
            return enemies;
        }
        
        #endregion
        
        #region Battle End Handling
        
        /// <summary>
        /// Called when battle ends
        /// </summary>
        private void OnBattleEnded(bool victory)
        {
            GD.Print($"[BattleScene] Battle ended - Victory: {victory}");
            
            if (victory)
            {
                // Show victory screen, rewards, etc.
                ShowVictoryScreen();
            }
            else
            {
                // Game over or retry
                ShowDefeatScreen();
            }
            
            // After showing results, return to overworld
            GetTree().CreateTimer(2.0f).Timeout += () => ReturnToOverworld(victory);
        }
        
        /// <summary>
        /// Return to overworld after battle
        /// </summary>
        private void ReturnToOverworld(bool victory)
        {
            // Let EncounterManager handle the transition
            if (encounterData != null)
            {
                EncounterManager.Instance?.ReturnToOverworld(victory);
            }
            else
            {
                // Story battle - return to previous scene or specific location
                // Implement your own logic here
                GD.Print("[BattleScene] Returning from story battle");
            }
        }
        
        #endregion
        
        #region Victory/Defeat Screens
        
        /// <summary>
        /// Show victory screen with rewards
        /// </summary>
        private void ShowVictoryScreen()
        {
            // TODO: Implement victory screen
            // - Show EXP gained
            // - Show items obtained
            // - Level up notifications
            // - Play victory music
            
            GD.Print("[BattleScene] Victory!");
            
            // Get rewards from BattleRewardsManager if you have it
            // var rewardsManager = battleManager.GetNode<BattleRewardsManager>("BattleRewardsManager");
            // if (rewardsManager != null)
            // {
            //     GD.Print($"  EXP: {rewardsManager.TotalExpGained}");
            //     GD.Print($"  Gold: {rewardsManager.TotalGoldGained}");
            //     // Apply rewards to party
            // }
        }
        
        /// <summary>
        /// Show defeat screen
        /// </summary>
        private void ShowDefeatScreen()
        {
            // TODO: Implement defeat screen
            // - Show game over or retry options
            // - Play defeat music
            
            GD.Print("[BattleScene] Defeat...");
        }
        
        #endregion
}