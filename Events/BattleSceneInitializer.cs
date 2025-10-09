// Events/BattleSceneInitializer.cs - COMPLETE IMPLEMENTATION
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        
        // Paths to resource folders
        [Export] private string showtimesPath = "res://Data/Showtimes/";
        [Export] private string limitBreaksPath = "res://Data/LimitBreaks/";
        
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
            
            // Initialize battle with all systems
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
            
            // Fallback: Load from current save
            if (partyStats.Count == 0 && GameManager.Instance?.CurrentSave != null)
            {
                var saveData = GameManager.Instance.CurrentSave;
                
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
        /// Get available showtime attacks based on party composition
        /// </summary>
        private List<ShowtimeAttackData> GetAvailableShowtimes()
        {
            var showtimes = new List<ShowtimeAttackData>();
            
            // Get party members
            var partyMembers = GetPlayerParty();
            if (partyMembers.Count < 2)
            {
                return showtimes; // Need at least 2 members for showtimes
            }
            
            // Method 1: Load all showtime resources from folder
            if (DirAccess.DirExistsAbsolute(showtimesPath))
            {
                using var dir = DirAccess.Open(showtimesPath);
                if (dir != null)
                {
                    dir.ListDirBegin();
                    string fileName = dir.GetNext();
                    
                    while (fileName != "")
                    {
                        if (!dir.CurrentIsDir() && fileName.EndsWith(".tres"))
                        {
                            string fullPath = showtimesPath + fileName;
                            var showtime = GD.Load<ShowtimeAttackData>(fullPath);
                            
                            if (showtime != null)
                            {
                                // Check if both required characters are in party
                                bool hasChar1 = partyMembers.Any(p => 
                                    p.CharacterName.Equals(showtime.Character1Id, StringComparison.OrdinalIgnoreCase));
                                bool hasChar2 = partyMembers.Any(p => 
                                    p.CharacterName.Equals(showtime.Character2Id, StringComparison.OrdinalIgnoreCase));
                                
                                if (hasChar1 && hasChar2)
                                {
                                    showtimes.Add(showtime);
                                    GD.Print($"[BattleSceneInitializer] Loaded Showtime: {showtime.AttackName}");
                                }
                            }
                        }
                        fileName = dir.GetNext();
                    }
                    dir.ListDirEnd();
                }
            }
            
            // Method 2: Load from battle parameters if specified
            var showtimeIds = GetBattleParam<Godot.Collections.Array<string>>("ShowtimeIds", null);
            if (showtimeIds != null && showtimeIds.Count > 0)
            {
                foreach (var showtimeId in showtimeIds)
                {
                    string resourcePath = $"{showtimesPath}{showtimeId}.tres";
                    if (ResourceLoader.Exists(resourcePath))
                    {
                        var showtime = GD.Load<ShowtimeAttackData>(resourcePath);
                        if (showtime != null && !showtimes.Contains(showtime))
                        {
                            showtimes.Add(showtime);
                            GD.Print($"[BattleSceneInitializer] Loaded Showtime from params: {showtime.AttackName}");
                        }
                    }
                }
            }
            
            GD.Print($"[BattleSceneInitializer] Total Showtimes available: {showtimes.Count}");
            return showtimes;
        }
        
        /// <summary>
        /// Get available limit breaks for party members
        /// </summary>
        private List<LimitBreakData> GetAvailableLimitBreaks()
        {
            var limitBreaks = new List<LimitBreakData>();
            
            // Get party members
            var partyMembers = GetPlayerParty();
            if (partyMembers.Count == 0)
            {
                return limitBreaks;
            }
            
            // Method 1: Load limit breaks for each party member from folder
            if (DirAccess.DirExistsAbsolute(limitBreaksPath))
            {
                foreach (var member in partyMembers)
                {
                    // Try to load limit break for this character
                    string characterId = member.CharacterName.ToLower();
                    string[] possibleNames = new string[]
                    {
                        $"{characterId}_lb.tres",
                        $"{characterId}_limitbreak.tres",
                        $"{characterId}.tres",
                        $"LB_{characterId}.tres"
                    };
                    
                    foreach (var fileName in possibleNames)
                    {
                        string fullPath = limitBreaksPath + fileName;
                        if (ResourceLoader.Exists(fullPath))
                        {
                            var limitBreak = GD.Load<LimitBreakData>(fullPath);
                            if (limitBreak != null && !limitBreaks.Any(lb => lb.CharacterId == limitBreak.CharacterId))
                            {
                                limitBreaks.Add(limitBreak);
                                GD.Print($"[BattleSceneInitializer] Loaded Limit Break: {limitBreak.DisplayName} for {member.CharacterName}");
                                break;
                            }
                        }
                    }
                }
            }
            
            // Method 2: Load from battle parameters if specified
            var limitBreakIds = GetBattleParam<Godot.Collections.Array<string>>("LimitBreakIds", null);
            if (limitBreakIds != null && limitBreakIds.Count > 0)
            {
                foreach (var lbId in limitBreakIds)
                {
                    string resourcePath = $"{limitBreaksPath}{lbId}.tres";
                    if (ResourceLoader.Exists(resourcePath))
                    {
                        var limitBreak = GD.Load<LimitBreakData>(resourcePath);
                        if (limitBreak != null && !limitBreaks.Any(lb => lb.LimitBreakId == limitBreak.LimitBreakId))
                        {
                            limitBreaks.Add(limitBreak);
                            GD.Print($"[BattleSceneInitializer] Loaded Limit Break from params: {limitBreak.DisplayName}");
                        }
                    }
                }
            }
            
            // Method 3: Scan entire limit breaks folder if nothing found yet
            if (limitBreaks.Count == 0 && DirAccess.DirExistsAbsolute(limitBreaksPath))
            {
                using var dir = DirAccess.Open(limitBreaksPath);
                if (dir != null)
                {
                    dir.ListDirBegin();
                    string fileName = dir.GetNext();
                    
                    while (fileName != "")
                    {
                        if (!dir.CurrentIsDir() && fileName.EndsWith(".tres"))
                        {
                            string fullPath = limitBreaksPath + fileName;
                            var limitBreak = GD.Load<LimitBreakData>(fullPath);
                            
                            if (limitBreak != null)
                            {
                                // Check if this limit break belongs to a party member
                                bool belongsToPartyMember = partyMembers.Any(p => 
                                    p.CharacterName.Equals(limitBreak.CharacterId, StringComparison.OrdinalIgnoreCase));
                                
                                if (belongsToPartyMember && !limitBreaks.Any(lb => lb.LimitBreakId == limitBreak.LimitBreakId))
                                {
                                    limitBreaks.Add(limitBreak);
                                    GD.Print($"[BattleSceneInitializer] Loaded Limit Break: {limitBreak.DisplayName}");
                                }
                            }
                        }
                        fileName = dir.GetNext();
                    }
                    dir.ListDirEnd();
                }
            }
            
            GD.Print($"[BattleSceneInitializer] Total Limit Breaks available: {limitBreaks.Count}");
            return limitBreaks;
        }
        
        /// <summary>
        /// Get battle parameter with proper Godot variant handling
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
            var result = victory ? "victory" : "defeat";
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetMeta("BattleResult", result);
                
                // Award rewards on victory
                if (victory)
                {
                    await AwardBattleRewards();
                }
            }
            
            // Brief delay before returning
            await ToSignal(GetTree().CreateTimer(1.0), "timeout");
            
            ReturnToOverworld();
        }
        
        /// <summary>
        /// Award experience, gold, and items after battle victory
        /// </summary>
        private async Task AwardBattleRewards()
        {
            // Calculate total EXP from enemies
            int totalExp = CalculateExpReward();
            
            // Award EXP to party
            if (PartyMenuManager.Instance != null && totalExp > 0)
            {
                // Get main party
                var party = PartyMenuManager.Instance.GetMainParty();
                foreach (var member in party)
                {
                    if (member != null && member.Stats != null && member.Stats.IsAlive)
                    {
                        int expGained = totalExp;
                        member.Stats.AddExp(expGained);
                        GD.Print($"[BattleSceneInitializer] {member.Stats.CharacterName} gained {expGained} EXP");
                    }
                }
            }
            
            // TODO: Award gold and items based on enemy drops
            // This would require enemy reward data to be passed or calculated
            
            await Task.CompletedTask;
        }
        
        /// <summary>
        /// Calculate total EXP reward from defeated enemies
        /// </summary>
        private int CalculateExpReward()
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
                        // Calculate EXP based on enemy level
                        int enemyLevel = enemyData.Level > 0 ? enemyData.Level : 1;
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