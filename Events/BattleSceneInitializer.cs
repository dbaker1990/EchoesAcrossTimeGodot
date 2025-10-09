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
            // Get battle manager if not assigned
            if (battleManager == null)
            {
                battleManager = GetNode<BattleManager>("BattleManager");
            }
            
            if (battleManager == null)
            {
                GD.PrintErr("[BattleSceneInitializer] BattleManager not found!");
                return;
            }
            
            // Check if this battle was started from an event
            InitializeFromEventBattle();
        }
        
        /// <summary>
        /// Initialize battle from event command parameters
        /// </summary>
        private void InitializeFromEventBattle()
        {
            // Try to get pending battle data from GameManager
            Godot.Collections.Dictionary battleParams = null;
            
            if (GameManager.Instance != null && GameManager.Instance.HasMeta("PendingBattleData"))
            {
                battleParams = GameManager.Instance.GetMeta("PendingBattleData").AsGodotDictionary();
                GameManager.Instance.RemoveMeta("PendingBattleData"); // Clear after reading
            }
            
            if (battleParams == null)
            {
                GD.Print("[BattleSceneInitializer] No pending battle data found");
                return;
            }
            
            GD.Print("[BattleSceneInitializer] Initializing battle from event parameters");
            
            // Extract parameters
            string troopId = battleParams["TroopId"].AsString();
            var enemyIds = battleParams["EnemyIds"].AsGodotArray<string>();
            bool isBossBattle = battleParams["IsBossBattle"].AsBool();
            bool canEscape = battleParams["CanEscape"].AsBool();
            bool canLose = battleParams["CanLose"].AsBool();
            string battleBGMPath = battleParams["BattleBGM"].AsString();
            string backgroundPath = battleParams["BattleBackground"].AsString();
            
            // Set battle background if provided
            if (!string.IsNullOrEmpty(backgroundPath))
            {
                SetBattleBackground(backgroundPath);
            }
            
            // Play battle BGM if provided
            if (!string.IsNullOrEmpty(battleBGMPath))
            {
                PlayBattleBGM(battleBGMPath);
            }
            else
            {
                // Use default battle music
                SystemManager.Instance?.PlayBattleMusic();
            }
            
            // Get player party
            var playerParty = GetPlayerParty();
            
            // Create enemies from IDs
            var enemies = CreateEnemiesFromIds(enemyIds);
            
            if (playerParty.Count == 0 || enemies.Count == 0)
            {
                GD.PrintErr("[BattleSceneInitializer] Failed to create party or enemies!");
                ReturnToOverworld();
                return;
            }
            
            // Initialize the battle
            battleManager.InitializeBattle(
                playerParty,
                enemies,
                availableShowtimes: GetAvailableShowtimes(),
                availableLimitBreaks: GetAvailableLimitBreaks(),
                isBossBattle: isBossBattle,
                isPinnedDown: !canEscape
            );
            
            // Connect to battle end signal
            battleManager.BattleEnded += OnBattleEnded;
            
            // Start the battle
            battleManager.StartBattle();
        }
        
        /// <summary>
        /// Get player party stats
        /// </summary>
        private List<CharacterStats> GetPlayerParty()
        {
            var partyStats = new List<CharacterStats>();
            
            if (PartyManager.Instance != null)
            {
                var party = PartyManager.Instance.GetMainParty();
                foreach (var member in party)
                {
                    if (member != null)
                    {
                        var stats = member.ToCharacterStats();
                        if (stats != null)
                        {
                            partyStats.Add(stats);
                        }
                    }
                }
            }
            
            // Fallback: Load from current save
            if (partyStats.Count == 0 && GameManager.Instance?.CurrentSave != null)
            {
                var saveData = GameManager.Instance.CurrentSave;
                foreach (var memberId in saveData.MainPartyIds)
                {
                    var memberData = saveData.PartyMembers.FirstOrDefault(m => m.CharacterId == memberId);
                    if (memberData != null && GameManager.Instance.Database != null)
                    {
                        var stats = memberData.ToCharacterStats(GameManager.Instance.Database);
                        if (stats != null)
                        {
                            partyStats.Add(stats);
                        }
                    }
                }
            }
            
            GD.Print($"[BattleSceneInitializer] Created party with {partyStats.Count} members");
            return partyStats;
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
                    var stats = enemyData.ToCharacterStats();
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
        /// Get available showtime attacks for this party
        /// </summary>
        private List<ShowtimeAttackData> GetAvailableShowtimes()
        {
            // TODO: Implement based on your showtime system
            // For now, return empty list
            return new List<ShowtimeAttackData>();
        }
        
        /// <summary>
        /// Get available limit breaks for this party
        /// </summary>
        private List<LimitBreakData> GetAvailableLimitBreaks()
        {
            // TODO: Implement based on your limit break system
            // For now, return empty list
            return new List<LimitBreakData>();
        }
        
        /// <summary>
        /// Set battle background image
        /// </summary>
        private void SetBattleBackground(string backgroundPath)
        {
            if (battleBackground == null || string.IsNullOrEmpty(backgroundPath))
                return;
            
            try
            {
                var texture = GD.Load<Texture2D>(backgroundPath);
                if (texture != null && battleBackground is Sprite2D sprite)
                {
                    sprite.Texture = texture;
                    GD.Print($"[BattleSceneInitializer] Set battle background: {backgroundPath}");
                }
            }
            catch (Exception e)
            {
                GD.PrintErr($"[BattleSceneInitializer] Failed to load background: {e.Message}");
            }
        }
        
        /// <summary>
        /// Play battle BGM
        /// </summary>
        private void PlayBattleBGM(string bgmPath)
        {
            if (string.IsNullOrEmpty(bgmPath)) return;
            
            try
            {
                var bgm = GD.Load<AudioStream>(bgmPath);
                if (bgm != null && AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayBGM(bgm);
                    GD.Print($"[BattleSceneInitializer] Playing battle BGM: {bgmPath}");
                }
            }
            catch (Exception e)
            {
                GD.PrintErr($"[BattleSceneInitializer] Failed to load BGM: {e.Message}");
            }
        }
        
        /// <summary>
        /// Called when battle ends
        /// </summary>
        private async void OnBattleEnded(bool victory)
        {
            GD.Print($"[BattleSceneInitializer] Battle ended - Victory: {victory}");
            
            // Set battle result for event branches
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
            if (victory && PartyManager.Instance != null)
            {
                var rewards = battleManager.GetBattleRewards();
                if (rewards != null)
                {
                    // Distribute EXP
                    PartyManager.Instance.DistributeExperience(rewards.TotalExp);
                    
                    // Add gold
                    if (Items.InventorySystem.Instance != null)
                    {
                        Items.InventorySystem.Instance.AddGold(rewards.TotalGold);
                    }
                    
                    // Add item drops
                    foreach (var drop in rewards.ItemDrops)
                    {
                        var itemData = GameManager.Instance?.Database?.GetItem(drop.itemId);
                        if (itemData != null && Items.InventorySystem.Instance != null)
                        {
                            Items.InventorySystem.Instance.AddItem(itemData, drop.quantity);
                        }
                    }
                }
            }
            
            // Wait a moment for rewards screen
            await ToSignal(GetTree().CreateTimer(2.0f), SceneTreeTimer.SignalName.Timeout);
            
            // Return to overworld
            ReturnToOverworld();
        }
        
        /// <summary>
        /// Return to the overworld scene
        /// </summary>
        private void ReturnToOverworld()
        {
            // Get return scene from GameManager
            string returnScene = "res://Maps/TestMap.tscn"; // Default fallback
            
            if (GameManager.Instance != null && !string.IsNullOrEmpty(GameManager.Instance.LastMapScene))
            {
                returnScene = GameManager.Instance.LastMapScene;
            }
            
            GD.Print($"[BattleSceneInitializer] Returning to: {returnScene}");
            
            // Change scene
            GetTree().ChangeSceneToFile(returnScene);
            
            // Player position restoration happens in the overworld scene's _Ready
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