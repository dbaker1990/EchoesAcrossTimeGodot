using Godot;
using Godot.Collections;
using System.Collections.Generic;

namespace EchoesAcrossTime.Encounters
{
    /// <summary>
    /// Defines an encounter zone with specific enemies and encounter rates
    /// Place in world map areas to trigger random battles
    /// </summary>
    public partial class EncounterZone : Area2D
    {
        #region Exports
        
        [ExportCategory("Zone Settings")]
        [Export] public string ZoneName { get; set; } = "Wilderness";
        
        [ExportGroup("Encounter Configuration")]
        [Export(PropertyHint.Range, "1,100")] 
        public int BaseEncounterChance { get; set; } = 10; // % per step
        
        [Export(PropertyHint.Range, "10,200")] 
        public int StepsPerEncounterCheck { get; set; } = 30; // Check every X steps
        
        [Export] public bool DisableEncounters { get; set; } = false;
        
        [ExportGroup("Enemy Configuration")]
        [Export] public Array<string> CommonEnemyIds { get; set; } = new Array<string>();
        [Export] public Array<string> UncommonEnemyIds { get; set; } = new Array<string>();
        [Export] public Array<string> RareEnemyIds { get; set; } = new Array<string>();
        
        [Export(PropertyHint.Range, "1,8")] 
        public int MinEnemies { get; set; } = 1;
        
        [Export(PropertyHint.Range, "1,8")] 
        public int MaxEnemies { get; set; } = 3;
        
        [ExportGroup("Battle Settings")]
        [Export] public bool IsBossBattle { get; set; } = false;
        [Export] public bool CanEscape { get; set; } = true;
        [Export] public string BattleScenePath { get; set; } = "res://Scenes/BattleScene.tscn";
        [Export] public AudioStream BattleMusic { get; set; }
        
        [Export] public Texture2D BattleBackground { get; set; }
        [Export] public Color BackgroundTint { get; set; } = Colors.White;
        
        #endregion
        
        #region Private Fields
        
        private bool playerInZone = false;
        private RandomNumberGenerator rng;
        
        #endregion
        
        #region Godot Lifecycle
        
        public override void _Ready()
        {
            rng = new RandomNumberGenerator();
            rng.Randomize();
            
            // Connect signals
            BodyEntered += OnBodyEntered;
            BodyExited += OnBodyExited;
            
            // Set collision layers
            CollisionLayer = 0; // Don't collide with anything
            CollisionMask = 1; // Only detect player (layer 1)
            
            GD.Print($"[EncounterZone] '{ZoneName}' initialized - {CommonEnemyIds.Count} enemy types");
        }
        
        #endregion
        
        #region Zone Detection
        
        private void OnBodyEntered(Node2D body)
        {
            if (body.IsInGroup("player"))
            {
                playerInZone = true;
                EncounterManager.Instance?.RegisterZone(this);
                GD.Print($"[EncounterZone] Player entered '{ZoneName}'");
            }
        }
        
        private void OnBodyExited(Node2D body)
        {
            if (body.IsInGroup("player"))
            {
                playerInZone = false;
                EncounterManager.Instance?.UnregisterZone(this);
                GD.Print($"[EncounterZone] Player left '{ZoneName}'");
            }
        }
        
        #endregion
        
        #region Encounter Logic
        
        /// <summary>
        /// Check if an encounter should trigger
        /// </summary>
        public bool CheckForEncounter(int stepCount)
        {
            if (DisableEncounters || !playerInZone)
                return false;
            
            // Only check at specific step intervals
            if (stepCount % StepsPerEncounterCheck != 0)
                return false;
            
            int roll = rng.RandiRange(1, 100);
            bool shouldEncounter = roll <= BaseEncounterChance;
            
            if (shouldEncounter)
            {
                GD.Print($"[EncounterZone] Encounter triggered! (Rolled {roll}/{BaseEncounterChance})");
            }
            
            return shouldEncounter;
        }
        
        /// <summary>
        /// Get a random enemy party for this zone
        /// </summary>
        public System.Collections.Generic.List<string> GetRandomEnemyParty()
        {
            var enemies = new System.Collections.Generic.List<string>();
            int enemyCount = rng.RandiRange(MinEnemies, MaxEnemies);
            
            for (int i = 0; i < enemyCount; i++)
            {
                string enemy = GetRandomEnemy();
                if (!string.IsNullOrEmpty(enemy))
                {
                    enemies.Add(enemy);
                }
            }
            
            return enemies;
        }
        
        /// <summary>
        /// Get a random enemy based on rarity
        /// </summary>
        private string GetRandomEnemy()
        {
            // Roll for rarity (Common: 70%, Uncommon: 25%, Rare: 5%)
            int rarityRoll = rng.RandiRange(1, 100);
            
            if (rarityRoll <= 5 && RareEnemyIds.Count > 0)
            {
                return RareEnemyIds[rng.RandiRange(0, RareEnemyIds.Count - 1)];
            }
            else if (rarityRoll <= 30 && UncommonEnemyIds.Count > 0)
            {
                return UncommonEnemyIds[rng.RandiRange(0, UncommonEnemyIds.Count - 1)];
            }
            else if (CommonEnemyIds.Count > 0)
            {
                return CommonEnemyIds[rng.RandiRange(0, CommonEnemyIds.Count - 1)];
            }
            
            return null;
        }
        
        #endregion
        
        #region Public API
        
        public bool IsPlayerInZone => playerInZone;
        
        public EncounterData GetEncounterData()
        {
            return new EncounterData
            {
                ZoneName = ZoneName,
                EnemyIds = GetRandomEnemyParty(),
                IsBossBattle = IsBossBattle,
                CanEscape = CanEscape,
                BattleScenePath = BattleScenePath,
                BattleMusic = BattleMusic,
                BattleBackground = BattleBackground,
                BackgroundTint = BackgroundTint
            };
        }
        
        #endregion
    }
    
    /// <summary>
    /// Data for initiating a battle
    /// </summary>
    public class EncounterData
    {
        public string ZoneName { get; set; }
        public System.Collections.Generic.List<string> EnemyIds { get; set; }
        public bool IsBossBattle { get; set; }
        public bool CanEscape { get; set; }
        public string BattleScenePath { get; set; }
        public AudioStream BattleMusic { get; set; }
        
        public Texture2D BattleBackground { get; set; }
        public Color BackgroundTint { get; set; } = Colors.White;
    }
}