using Godot;
using System.Collections.Generic;
using EchoesAcrossTime.Combat;

namespace NocturneRequiem
{
    /// <summary>
    /// Controls the battle scene initialization and background loading
    /// Attach this to the root BattleScene node
    /// </summary>
    public partial class BattleSceneController : Node2D
    {
        [Export] private Texture2D defaultBackground;
        [Export] private Color defaultTint = Colors.White;
        
        private TextureRect backgroundNode;
        private BattleManager battleManager;
        
        public override void _Ready()
        {
            // Get references
            backgroundNode = GetNode<TextureRect>("Background");
            battleManager = GetNode<BattleManager>("%BattleManager");
            
            // Load encounter data and apply background
            LoadEncounterData();
            
            // Initialize battle
            InitializeBattle();
        }
        
        /// <summary>
        /// Load encounter data passed from EncounterManager
        /// </summary>
        private void LoadEncounterData()
        {
            var root = GetTree().Root;
            
            if (!root.HasMeta("EncounterData"))
            {
                GD.PrintErr("[BattleScene] No encounter data found! Using defaults.");
                ApplyDefaultBackground();
                return;
            }
            
            var encounterData = root.GetMeta("EncounterData").AsGodotDictionary();
            
            // Load background
            if (encounterData.ContainsKey("BattleBackground"))
            {
                string bgPath = encounterData["BattleBackground"].AsString();
                
                if (!string.IsNullOrEmpty(bgPath))
                {
                    var bgTexture = GD.Load<Texture2D>(bgPath);
                    var bgTint = encounterData.ContainsKey("BackgroundTint") 
                        ? encounterData["BackgroundTint"].AsColor() 
                        : Colors.White;
                    
                    SetBackground(bgTexture, bgTint);
                    GD.Print($"[BattleScene] Background loaded: {bgPath}");
                }
                else
                {
                    ApplyDefaultBackground();
                }
            }
            else
            {
                ApplyDefaultBackground();
            }
            
            // Store encounter data for battle manager
            StoreEncounterData(encounterData);
        }
        
        /// <summary>
        /// Apply the background texture
        /// </summary>
        private void SetBackground(Texture2D texture, Color tint)
        {
            if (backgroundNode != null && texture != null)
            {
                backgroundNode.Texture = texture;
                backgroundNode.Modulate = tint;
            }
        }
        
        /// <summary>
        /// Apply default background if none provided
        /// </summary>
        private void ApplyDefaultBackground()
        {
            if (defaultBackground != null)
            {
                SetBackground(defaultBackground, defaultTint);
                GD.Print("[BattleScene] Using default background");
            }
            else
            {
                GD.PrintErr("[BattleScene] No default background set!");
            }
        }
        
        /// <summary>
        /// Store encounter data for battle manager to access
        /// </summary>
        private void StoreEncounterData(Godot.Collections.Dictionary data)
        {
            // Extract enemy IDs - keep as Godot Array
            var enemyIds = new Godot.Collections.Array<string>();
            if (data.ContainsKey("EnemyIds"))
            {
                var enemyArray = data["EnemyIds"].AsGodotArray();
                foreach (var id in enemyArray)
                {
                    enemyIds.Add(id.AsString());
                }
            }
            
            // Store for battle manager
            bool isBoss = data.ContainsKey("IsBossBattle") && data["IsBossBattle"].AsBool();
            bool canEscape = data.ContainsKey("CanEscape") && data["CanEscape"].AsBool();
            
            // You can pass this to BattleManager or store as metadata
            SetMeta("EnemyIds", enemyIds);
            SetMeta("IsBossBattle", isBoss);
            SetMeta("CanEscape", canEscape);
        }
        
        /// <summary>
        /// Initialize the battle with loaded data
        /// </summary>
        private void InitializeBattle()
        {
            if (battleManager == null)
            {
                GD.PrintErr("[BattleScene] BattleManager not found!");
                return;
            }
            
            // Get enemy IDs from metadata
            var enemyIdsVariant = GetMeta("EnemyIds");
            var enemyIds = enemyIdsVariant.AsGodotArray<string>();
            bool isBoss = GetMeta("IsBossBattle").AsBool();
            bool canEscape = GetMeta("CanEscape").AsBool();
            
            GD.Print($"[BattleScene] Initializing battle:");
            GD.Print($"  - Enemies: {string.Join(", ", enemyIds)}");
            GD.Print($"  - Boss Battle: {isBoss}");
            GD.Print($"  - Can Escape: {canEscape}");
            
            // TODO: Create party and enemy members from IDs
            // This depends on your GameManager/PartyManager implementation
            
            // Example:
            // var party = PartyManager.Instance.GetActiveParty();
            // var enemies = CreateEnemiesFromIds(enemyIds);
            // battleManager.InitializeBattle(party, enemies, ...);
        }
        
        /// <summary>
        /// Public method to change background mid-battle if needed
        /// </summary>
        public void ChangeBackground(Texture2D newBackground, Color tint)
        {
            SetBackground(newBackground, tint);
        }
    }
}