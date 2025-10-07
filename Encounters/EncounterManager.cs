using Godot;
using Godot.Collections;
using System.Linq;
using EchoesAcrossTime.Combat;
using EchoesAcrossTime.Characters;

namespace EchoesAcrossTime.Encounters
{
    /// <summary>
    /// Global encounter manager - handles step counting and battle transitions
    /// Add as AUTOLOAD in Project Settings
    /// </summary>
    public partial class EncounterManager : Node
    {
        #region Singleton
        
        public static EncounterManager Instance { get; private set; }
        
        #endregion
        
        #region Signals
        
        [Signal] public delegate void BattleStartingEventHandler();
        [Signal] public delegate void StepCountChangedEventHandler(int steps);
        
        #endregion
        
        #region Exports
        
        [ExportGroup("Encounter Settings")]
        [Export] public bool EncountersEnabled { get; set; } = true;
        
        [Export(PropertyHint.Range, "0.1,2.0")] 
        public float EncounterMultiplier { get; set; } = 1.0f; // Global encounter rate modifier
        
        [Export] public bool ShowDebugInfo { get; set; } = false;
        
        [ExportGroup("Battle Transition")]
        [Export] public float TransitionDuration { get; set; } = 1.0f;
        [Export] public Color FlashColor { get; set; } = Colors.White;
        [Export] public AudioStream EncounterSound { get; set; }
        
        #endregion
        
        #region Private Fields
        
        private int stepCount = 0;
        private EncounterZone currentZone;
        private bool inBattle = false;
        private Vector2 preBattlePosition;
        private string returnScenePath;
        
        // Items that affect encounter rate
        private bool hasRepelItem = false;
        private bool hasLureItem = false;
        
        #endregion
        
        #region Godot Lifecycle
        
        public override void _Ready()
        {
            if (Instance != null)
            {
                QueueFree();
                return;
            }
            
            Instance = this;
            ProcessMode = ProcessModeEnum.Always;
            
            var root = GetTree().Root;
            if (root.HasMeta("EncounterData"))
            {
                var encounterData = root.GetMeta("EncounterData").AsGodotDictionary();
        
                if (encounterData.ContainsKey("BattleBackground"))
                {
                    var bgPath = encounterData["BattleBackground"].AsString();
                    if (!string.IsNullOrEmpty(bgPath))
                    {
                        var bgTexture = GD.Load<Texture2D>(bgPath);
                        var bgTint = encounterData.ContainsKey("BackgroundTint") 
                            ? encounterData["BackgroundTint"].AsColor() 
                            : Colors.White;
                
                        SetBattleBackground(bgTexture, bgTint);
                    }
                }
            }
            
            GD.Print("[EncounterManager] Initialized");
        }
        
        #endregion
        
        #region Step Tracking
        
        /// <summary>
        /// Call this when the player takes a step
        /// </summary>
        public void OnPlayerStep()
        {
            if (!EncountersEnabled || inBattle || currentZone == null)
                return;
            
            stepCount++;
            EmitSignal(SignalName.StepCountChanged, stepCount);
            
            if (ShowDebugInfo)
            {
                GD.Print($"[EncounterManager] Step {stepCount}");
            }
            
            // Check for encounter
            CheckEncounter();
        }
        
        /// <summary>
        /// Reset step counter (called after battle or when entering safe zone)
        /// </summary>
        public void ResetStepCount()
        {
            stepCount = 0;
            if (ShowDebugInfo)
            {
                GD.Print("[EncounterManager] Step count reset");
            }
        }
        
        #endregion
        
        #region Zone Management
        
        /// <summary>
        /// Register zone when player enters
        /// </summary>
        public void RegisterZone(EncounterZone zone)
        {
            currentZone = zone;
            ResetStepCount(); // Reset steps when entering new zone
            
            GD.Print($"[EncounterManager] Registered zone: {zone.ZoneName}");
        }
        
        /// <summary>
        /// Unregister zone when player exits
        /// </summary>
        public void UnregisterZone(EncounterZone zone)
        {
            if (currentZone == zone)
            {
                currentZone = null;
                GD.Print($"[EncounterManager] Unregistered zone: {zone.ZoneName}");
            }
        }
        
        #endregion
        
        #region Encounter Logic
        
        /// <summary>
        /// Check if an encounter should trigger
        /// </summary>
        private void CheckEncounter()
        {
            if (currentZone == null || inBattle)
                return;
            
            // Apply encounter rate modifiers
            float modifiedChance = GetModifiedEncounterRate();
            
            // Check with current zone
            if (currentZone.CheckForEncounter(stepCount))
            {
                // Additional check with modifiers
                var rng = new RandomNumberGenerator();
                rng.Randomize();
                
                if (rng.Randf() * 100 <= modifiedChance * 100)
                {
                    TriggerEncounter();
                }
            }
        }
        
        /// <summary>
        /// Get modified encounter rate based on items/effects
        /// </summary>
        private float GetModifiedEncounterRate()
        {
            float rate = EncounterMultiplier;
            
            if (hasRepelItem) rate *= 0.5f; // Repel halves encounters
            if (hasLureItem) rate *= 2.0f;  // Lure doubles encounters
            
            return Mathf.Clamp(rate, 0.1f, 5.0f);
        }
        
        /// <summary>
        /// Trigger a random encounter
        /// </summary>
        private void TriggerEncounter()
        {
            if (currentZone == null)
                return;
            
            var encounterData = currentZone.GetEncounterData();
            
            if (encounterData.EnemyIds == null || encounterData.EnemyIds.Count == 0)
            {
                GD.PrintErr("[EncounterManager] No enemies in zone!");
                return;
            }
            
            GD.Print($"[EncounterManager] Battle starting with {encounterData.EnemyIds.Count} enemies!");
            
            StartBattle(encounterData);
        }
        
        #endregion
        
        #region Battle Transition
        
        /// <summary>
        /// Start battle transition
        /// </summary>
        private async void StartBattle(EncounterData encounterData)
        {
            inBattle = true;
            EmitSignal(SignalName.BattleStarting);
            
            // Play encounter sound
            if (EncounterSound != null)
            {
                AudioManager.Instance?.PlaySoundEffect(EncounterSound, 1.0f);
            }
            
            // Store return position
            var player = GetTree().GetFirstNodeInGroup("player") as Node2D;
            if (player != null)
            {
                preBattlePosition = player.GlobalPosition;
            }
            
            // Store current scene path for return
            returnScenePath = GetTree().CurrentScene.SceneFilePath;
            
            // Flash screen effect
            await FlashScreen();
            
            // Change to battle scene
            LoadBattleScene(encounterData);
        }
        
        /// <summary>
        /// Flash screen transition effect
        /// </summary>
        private async System.Threading.Tasks.Task FlashScreen()
        {
            var flash = new ColorRect
            {
                Color = FlashColor,
                Size = GetViewport().GetVisibleRect().Size,
                Modulate = new Color(FlashColor, 0)
            };
            
            GetTree().Root.AddChild(flash);
            
            // Fade in
            var tween = CreateTween();
            tween.TweenProperty(flash, "modulate:a", 1.0f, TransitionDuration * 0.3f);
            await ToSignal(tween, Tween.SignalName.Finished);
            
            // Hold
            await ToSignal(GetTree().CreateTimer(TransitionDuration * 0.2f), SceneTreeTimer.SignalName.Timeout);
            
            // Fade out
            tween = CreateTween();
            tween.TweenProperty(flash, "modulate:a", 0.0f, TransitionDuration * 0.5f);
            await ToSignal(tween, Tween.SignalName.Finished);
            
            flash.QueueFree();
        }
        
        /// <summary>
        /// Load battle scene with encounter data
        /// </summary>
        private void LoadBattleScene(EncounterData encounterData)
        {
            // Convert enemy IDs list to Godot Array
            var enemyIdsArray = new Array();
            foreach (var id in encounterData.EnemyIds)
            {
                enemyIdsArray.Add(id);
            }
    
            // Store encounter data as dictionary for battle scene to access
            var encounterDict = new Dictionary
            {
                { "ZoneName", encounterData.ZoneName },
                { "EnemyIds", enemyIdsArray },
                { "IsBossBattle", encounterData.IsBossBattle },
                { "CanEscape", encounterData.CanEscape },
                { "BattleScenePath", encounterData.BattleScenePath },
        
                // ADD THESE:
                { "BattleBackground", encounterData.BattleBackground?.ResourcePath ?? "" },
                { "BackgroundTint", encounterData.BackgroundTint }
            };
    
            GetTree().Root.SetMeta("EncounterData", encounterDict);
    
            // Change scene
            GetTree().ChangeSceneToFile(encounterData.BattleScenePath);
        }
        
        /// <summary>
        /// Return to overworld after battle
        /// </summary>
        public async void ReturnToOverworld(bool victory)
        {
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            
            // Load overworld scene
            if (!string.IsNullOrEmpty(returnScenePath))
            {
                GetTree().ChangeSceneToFile(returnScenePath);
                
                // Wait for scene to load
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
                
                // Restore player position
                var player = GetTree().GetFirstNodeInGroup("player") as Node2D;
                if (player != null && player is OverworldCharacter character)
                {
                    character.TeleportTo(preBattlePosition);
                }
            }
            
            inBattle = false;
            ResetStepCount();
            
            GD.Print($"[EncounterManager] Returned to overworld - Victory: {victory}");
        }
        
        /// <summary>
        /// Set the battle background
        /// </summary>
        public void SetBattleBackground(Texture2D background, Color tint)
        {
            var bgNode = GetNode<TextureRect>("../Background");
            if (bgNode != null && background != null)
            {
                bgNode.Texture = background;
                bgNode.Modulate = tint;
                GD.Print($"[BattleManager] Background set: {background.ResourcePath}");
            }
        }
        
        #endregion
        
        #region Encounter Rate Modifiers
        
        /// <summary>
        /// Set repel item active (reduces encounters)
        /// </summary>
        public void SetRepelActive(bool active)
        {
            hasRepelItem = active;
            GD.Print($"[EncounterManager] Repel {(active ? "activated" : "deactivated")}");
        }
        
        /// <summary>
        /// Set lure item active (increases encounters)
        /// </summary>
        public void SetLureActive(bool active)
        {
            hasLureItem = active;
            GD.Print($"[EncounterManager] Lure {(active ? "activated" : "deactivated")}");
        }
        
        /// <summary>
        /// Disable encounters (for cutscenes, safe zones)
        /// </summary>
        public void SetEncountersEnabled(bool enabled)
        {
            EncountersEnabled = enabled;
            GD.Print($"[EncounterManager] Encounters {(enabled ? "enabled" : "disabled")}");
        }
        
        #endregion
        
        #region Public API
        
        public int CurrentStepCount => stepCount;
        public bool IsInBattle => inBattle;
        public EncounterZone CurrentZone => currentZone;
        
        #endregion
    }
}