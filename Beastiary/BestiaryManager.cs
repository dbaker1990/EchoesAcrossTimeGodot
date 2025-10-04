using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using EchoesAcrossTime.Combat;
using EchoesAcrossTime.Database;

namespace EchoesAcrossTime.Bestiary
{
    /// <summary>
    /// Main bestiary system - tracks all enemy encounters and discoveries
    /// Set as Autoload singleton
    /// </summary>
    public partial class BestiaryManager : Node
    {
        public static BestiaryManager Instance { get; private set; }
        
        // Storage
        private Dictionary<string, BestiaryEntry> bestiaryEntries = new();
        private List<CharacterData> allEnemiesInGame = new();
        
        // Stats
        public int TotalEnemiesInGame => allEnemiesInGame.Count;
        public int TotalDiscovered => bestiaryEntries.Values.Count(e => e.IsDiscovered);
        public int TotalDefeated => bestiaryEntries.Values.Sum(e => e.TimesDefeated);
        public float CompletionPercentage => TotalEnemiesInGame > 0 
            ? (float)TotalDiscovered / TotalEnemiesInGame * 100f : 0f;
        
        #region Signals
        [Signal] public delegate void EnemyDiscoveredEventHandler(string enemyId, CharacterData enemyData);
        [Signal] public delegate void WeaknessDiscoveredEventHandler(string enemyId, ElementType weakness);
        [Signal] public delegate void BestiaryUpdatedEventHandler();
        [Signal] public delegate void NewSkillDiscoveredEventHandler(string enemyId, string skillId);
        #endregion
        
        public override void _Ready()
        {
            if (Instance != null)
            {
                QueueFree();
                return;
            }
            Instance = this;
            
            GD.Print("BestiaryManager initialized");
        }
        
        #region Initialization
        /// <summary>
        /// Load all enemies from the game database
        /// Call this during game initialization
        /// </summary>
        public void InitializeFromDatabase(GameDatabase database)
        {
            if (database == null)
            {
                GD.PrintErr("BestiaryManager: Cannot initialize, database is null");
                return;
            }
            
            allEnemiesInGame.Clear();
            
            // Load all enemies
            foreach (var enemy in database.Enemies)
            {
                if (enemy != null && !string.IsNullOrEmpty(enemy.CharacterId))
                {
                    allEnemiesInGame.Add(enemy);
                    
                    // Create entry if doesn't exist
                    if (!bestiaryEntries.ContainsKey(enemy.CharacterId))
                    {
                        bestiaryEntries[enemy.CharacterId] = new BestiaryEntry
                        {
                            EnemyId = enemy.CharacterId,
                            IsDiscovered = false
                        };
                    }
                }
            }
            
            // Load all bosses
            foreach (var boss in database.Bosses)
            {
                if (boss != null && !string.IsNullOrEmpty(boss.CharacterId))
                {
                    allEnemiesInGame.Add(boss);
                    
                    if (!bestiaryEntries.ContainsKey(boss.CharacterId))
                    {
                        bestiaryEntries[boss.CharacterId] = new BestiaryEntry
                        {
                            EnemyId = boss.CharacterId,
                            IsDiscovered = false
                        };
                    }
                }
            }
            
            GD.Print($"BestiaryManager: Loaded {allEnemiesInGame.Count} enemies");
        }
        #endregion
        
        #region Enemy Discovery
        /// <summary>
        /// Record an enemy encounter
        /// Call this when battle starts or enemy appears
        /// </summary>
        public void RecordEncounter(string enemyId, int level = 1)
        {
            if (string.IsNullOrEmpty(enemyId))
                return;
            
            var entry = GetOrCreateEntry(enemyId);
            bool wasNewDiscovery = !entry.IsDiscovered;
            
            entry.IsDiscovered = true;
            entry.TimesEncountered++;
            entry.LastEncounteredDate = DateTime.Now;
            
            if (wasNewDiscovery)
            {
                entry.FirstEncounteredDate = DateTime.Now;
                
                // Emit discovery signal
                var enemyData = GetEnemyData(enemyId);
                if (enemyData != null)
                {
                    EmitSignal(SignalName.EnemyDiscovered, enemyId, enemyData);
                }
            }
            
            // Track level range
            if (level > entry.HighestLevelSeen)
                entry.HighestLevelSeen = level;
            if (level < entry.LowestLevelSeen)
                entry.LowestLevelSeen = level;
            
            EmitSignal(SignalName.BestiaryUpdated);
        }
        
        /// <summary>
        /// Record an enemy defeat
        /// </summary>
        public void RecordDefeat(string enemyId)
        {
            if (string.IsNullOrEmpty(enemyId))
                return;
            
            var entry = GetOrCreateEntry(enemyId);
            entry.TimesDefeated++;
            
            EmitSignal(SignalName.BestiaryUpdated);
        }
        
        /// <summary>
        /// Record discovering an enemy's weakness
        /// </summary>
        public void RecordWeaknessDiscovered(string enemyId, ElementType element)
        {
            if (string.IsNullOrEmpty(enemyId))
                return;
            
            var entry = GetOrCreateEntry(enemyId);
            
            if (!entry.DiscoveredWeaknesses.Contains(element))
            {
                entry.DiscoveredWeaknesses.Add(element);
                entry.WeaknessesDiscovered = true;
                
                EmitSignal(SignalName.WeaknessDiscovered, enemyId, (int)element);
                EmitSignal(SignalName.BestiaryUpdated);
            }
        }
        
        /// <summary>
        /// Record discovering an enemy's resistance
        /// </summary>
        public void RecordResistanceDiscovered(string enemyId, ElementType element, ElementAffinity affinity)
        {
            if (string.IsNullOrEmpty(enemyId))
                return;
            
            var entry = GetOrCreateEntry(enemyId);
            
            switch (affinity)
            {
                case ElementAffinity.Resist:
                    if (!entry.DiscoveredResistances.Contains(element))
                        entry.DiscoveredResistances.Add(element);
                    break;
                    
                case ElementAffinity.Immune:
                case ElementAffinity.Absorb:
                    if (!entry.DiscoveredImmunities.Contains(element))
                        entry.DiscoveredImmunities.Add(element);
                    break;
            }
            
            EmitSignal(SignalName.BestiaryUpdated);
        }
        
        /// <summary>
        /// Record discovering an enemy skill
        /// </summary>
        public void RecordSkillDiscovered(string enemyId, string skillId)
        {
            if (string.IsNullOrEmpty(enemyId) || string.IsNullOrEmpty(skillId))
                return;
            
            var entry = GetOrCreateEntry(enemyId);
            
            if (!entry.DiscoveredSkillIds.Contains(skillId))
            {
                entry.DiscoveredSkillIds.Add(skillId);
                entry.SkillsDiscovered = true;
                
                EmitSignal(SignalName.NewSkillDiscovered, enemyId, skillId);
                EmitSignal(SignalName.BestiaryUpdated);
            }
        }
        #endregion
        
        #region Data Access
        /// <summary>
        /// Get bestiary entry for an enemy
        /// </summary>
        public BestiaryEntry GetEntry(string enemyId)
        {
            return bestiaryEntries.TryGetValue(enemyId, out var entry) ? entry : null;
        }
        
        /// <summary>
        /// Get the actual enemy data from database
        /// </summary>
        public CharacterData GetEnemyData(string enemyId)
        {
            return allEnemiesInGame.FirstOrDefault(e => e.CharacterId == enemyId);
        }
        
        /// <summary>
        /// Check if enemy is discovered
        /// </summary>
        public bool IsDiscovered(string enemyId)
        {
            var entry = GetEntry(enemyId);
            return entry?.IsDiscovered ?? false;
        }
        
        /// <summary>
        /// Get all enemies (for bestiary UI)
        /// </summary>
        public List<CharacterData> GetAllEnemies()
        {
            return new List<CharacterData>(allEnemiesInGame);
        }
        
        /// <summary>
        /// Get filtered enemies based on criteria
        /// </summary>
        public List<CharacterData> GetFilteredEnemies(BestiaryFilterType filter)
        {
            var filtered = new List<CharacterData>();
            
            foreach (var enemy in allEnemiesInGame)
            {
                var entry = GetEntry(enemy.CharacterId);
                
                switch (filter)
                {
                    case BestiaryFilterType.All:
                        filtered.Add(enemy);
                        break;
                        
                    case BestiaryFilterType.Discovered:
                        if (entry?.IsDiscovered ?? false)
                            filtered.Add(enemy);
                        break;
                        
                    case BestiaryFilterType.Undiscovered:
                        if (!(entry?.IsDiscovered ?? false))
                            filtered.Add(enemy);
                        break;
                        
                    case BestiaryFilterType.Bosses:
                        if (enemy.Type == CharacterType.Boss && (entry?.IsDiscovered ?? false))
                            filtered.Add(enemy);
                        break;
                        
                    case BestiaryFilterType.RegularEnemies:
                        if (enemy.Type == CharacterType.Enemy && (entry?.IsDiscovered ?? false))
                            filtered.Add(enemy);
                        break;
                }
            }
            
            return filtered;
        }
        
        /// <summary>
        /// Get sorted enemies
        /// </summary>
        public List<CharacterData> GetSortedEnemies(BestiarySortType sortType, bool descending = false)
        {
            var sorted = new List<CharacterData>(allEnemiesInGame);
            
            switch (sortType)
            {
                case BestiarySortType.ByName:
                    sorted = sorted.OrderBy(e => e.DisplayName).ToList();
                    break;
                    
                case BestiarySortType.ByLevel:
                    sorted = sorted.OrderBy(e => e.Level).ToList();
                    break;
                    
                case BestiarySortType.ByType:
                    sorted = sorted.OrderBy(e => e.Type).ThenBy(e => e.DisplayName).ToList();
                    break;
                    
                case BestiarySortType.ByTimesEncountered:
                    sorted = sorted.OrderBy(e => GetEntry(e.CharacterId)?.TimesEncountered ?? 0).ToList();
                    break;
                    
                case BestiarySortType.ByFirstEncounter:
                    sorted = sorted.OrderBy(e => GetEntry(e.CharacterId)?.FirstEncounteredDate ?? DateTime.MaxValue).ToList();
                    break;
                    
                case BestiarySortType.ByLastEncounter:
                    sorted = sorted.OrderBy(e => GetEntry(e.CharacterId)?.LastEncounteredDate ?? DateTime.MaxValue).ToList();
                    break;
            }
            
            if (descending)
                sorted.Reverse();
            
            return sorted;
        }
        #endregion
        
        #region Save/Load
        /// <summary>
        /// Get save data for persistence
        /// </summary>
        public BestiarySaveData GetSaveData()
        {
            var saveData = new BestiarySaveData
            {
                TotalEnemiesDiscovered = TotalDiscovered,
                TotalEnemiesDefeated = TotalDefeated
            };
            
            foreach (var kvp in bestiaryEntries)
            {
                saveData.Entries[kvp.Key] = kvp.Value;
            }
            
            return saveData;
        }
        
        /// <summary>
        /// Load save data
        /// </summary>
        public void LoadSaveData(BestiarySaveData saveData)
        {
            if (saveData == null)
                return;
            
            bestiaryEntries.Clear();
            
            foreach (var kvp in saveData.Entries)
            {
                bestiaryEntries[kvp.Key] = kvp.Value;
            }
            
            GD.Print($"BestiaryManager: Loaded {bestiaryEntries.Count} entries");
            EmitSignal(SignalName.BestiaryUpdated);
        }
        #endregion
        
        #region Helper Methods
        private BestiaryEntry GetOrCreateEntry(string enemyId)
        {
            if (!bestiaryEntries.ContainsKey(enemyId))
            {
                bestiaryEntries[enemyId] = new BestiaryEntry
                {
                    EnemyId = enemyId
                };
            }
            
            return bestiaryEntries[enemyId];
        }
        #endregion
        
        #region Debug
        /// <summary>
        /// Unlock all enemies (for testing)
        /// </summary>
        public void UnlockAll()
        {
            foreach (var enemy in allEnemiesInGame)
            {
                RecordEncounter(enemy.CharacterId, enemy.Level);
                RecordDefeat(enemy.CharacterId);
                
                // Discover all weaknesses
                var entry = GetEntry(enemy.CharacterId);
                if (entry != null && enemy.ElementAffinities != null)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        var element = (ElementType)i;
                        var affinity = enemy.ElementAffinities.GetAffinity(element);
                        
                        if (affinity == ElementAffinity.Weak)
                        {
                            RecordWeaknessDiscovered(enemy.CharacterId, element);
                        }
                        else if (affinity != ElementAffinity.Normal)
                        {
                            RecordResistanceDiscovered(enemy.CharacterId, element, affinity);
                        }
                    }
                }
            }
            
            GD.Print("BestiaryManager: All enemies unlocked!");
        }
        #endregion
    }
}