// Managers/GameManager.cs

using EchoesAcrossTime.Combat;
using Godot;
using EchoesAcrossTime.Database;
using EchoesAcrossTime.Managers;

namespace EchoesAcrossTime
{
    public partial class GameManager : Node
    {
        public static GameManager Instance { get; private set; }
        [Export] public GameDatabase Database { get; set; }
        [Export] public GameOverUI GameOverScreen { get; set; }

        /// <summary>
        /// Stores the last overworld scene path before entering battle
        /// </summary>
        public string LastMapScene { get; set; }

        /// <summary>
        /// Stores the player's position before entering battle
        /// </summary>
        public Vector2 LastPlayerPosition { get; set; }

        // Current game state - Changed to public set for SaveSystem
        public SaveData CurrentSave { get; set; }
        public bool IsGameActive { get; private set; }

        public override void _Ready()
        {
            if (Instance != null && Instance != this)
            {
                QueueFree();
                return;
            }

            Instance = this;
            
            ConnectMinimapEvents();

            // Load database if not assigned
            if (Database == null)
            {
                Database = GD.Load<GameDatabase>("res://Database/GameDatabase.tres");
            }

            if (Database == null)
            {
                GD.PrintErr("GameManager: No database assigned!");
            }
            else
            {
                GD.Print("GameManager initialized with database");
            }
        }

        public void StartNewGame()
        {
            CurrentSave = new SaveData();
            CurrentSave.InitializeNewGame(Database);
            IsGameActive = true;

            GD.Print("New game started");
        }

        public void LoadGame(SaveData saveData)
        {
            CurrentSave = saveData;
            CurrentSave.ApplyToGame();
            IsGameActive = true;

            GD.Print($"Game loaded: {saveData.SaveSlotName}");
        }

        public void QuitToTitle()
        {
            IsGameActive = false;
            CurrentSave = null;
            GetTree().ChangeSceneToFile("res://Scenes/TitleScreen.tscn");
        }

        public void TriggerGameOver()
        {
            // You might want to pause the game or stop player input here
            GetTree().Paused = true;
            GameOverScreen?.ShowScreen();
        }

        public void ContinueFromLastSave()
        {
            GetTree().Paused = false;

            // Get the index of the most recent save file.
            int lastSaveSlot = SaveSystem.Instance.GetMostRecentSaveSlot();

            if (lastSaveSlot != -1)
            {
                // Attempt to load the game from that slot.
                bool loadSuccessful = SaveSystem.Instance.LoadGame(lastSaveSlot);
                if (loadSuccessful)
                {
                    SaveData data = SaveSystem.Instance.CurrentSaveData;
                    LoadGame(data);
                    return;
                }
            }

            // If no save exists or it fails to load, go to the title screen.
            ReturnToTitleScreen();
        }

        /// <summary>
        /// Call this before starting a battle to save where the player was
        /// </summary>
        public void SaveOverworldState(string scenePath, Vector2 playerPosition)
        {
            LastMapScene = scenePath;
            LastPlayerPosition = playerPosition;
            GD.Print($"[GameManager] Saved overworld state: {scenePath} at {playerPosition}");
        }

        /// <summary>
        /// Start a battle encounter
        /// </summary>
        public void StartBattle(
            System.Collections.Generic.List<CharacterStats> playerParty,
            System.Collections.Generic.List<CharacterStats> enemies,
            bool isBossBattle = false)
        {
            // Save current location
            var currentScene = GetTree().CurrentScene;
            if (currentScene != null)
            {
                SaveOverworldState(currentScene.SceneFilePath, Vector2.Zero);
                // You can get actual player position if you have a player controller reference
            }

            // Load battle scene
            GetTree().ChangeSceneToFile("res://Scenes/BattleScene.tscn");
            
            CallDeferred(MethodName.RefreshMinimap);

            // You'll need to pass party/enemy data to the battle scene
            // This can be done through the GameManager or a dedicated BattleData class
        }

        /// <summary>
        /// Return to overworld after battle
        /// </summary>
        public void ReturnToOverworld()
        {
            if (!string.IsNullOrEmpty(LastMapScene))
            {
                GetTree().ChangeSceneToFile(LastMapScene);

                // After scene loads, you can restore player position
                // This would typically be done in the overworld scene's _Ready()
            }
            else
            {
                GD.PrintErr("[GameManager] No last map scene saved! Returning to default.");
                GetTree().ChangeSceneToFile("res://Maps/Veridia/VeridiaCapital.tscn");
            }
        }

        public void ReturnToTitleScreen()
        {
            GetTree().Paused = false;
            GetTree().ChangeSceneToFile("res://Scenes/TitleScreen.tscn");
        }
        
        #region Minimap Integration - NEW CODE ONLY
        
        /// <summary>
        /// Call this in your _Ready() method: ConnectMinimapEvents();
        /// Or call manually when needed
        /// </summary>
        public void ConnectMinimapEvents()
        {
            GetTree().NodeAdded += OnNodeAddedForMinimap;
        }

        /// <summary>
        /// Automatically setup minimap when player/tilemap loads
        /// </summary>
        private void OnNodeAddedForMinimap(Node node)
        {
            // Detect player
            if (node.IsInGroup("player") && node is Node2D player2D)
            {
                if (HUDManager.Instance != null)
                {
                    HUDManager.Instance.SetPlayer(player2D);
                }
            }
            
            // Detect tilemap
            if (node is TileMapLayer tileMapLayer)
            {
                if (HUDManager.Instance != null)
                {
                    HUDManager.Instance.SetTileMap(tileMapLayer);
                }
            }
        }

        /// <summary>
        /// Call this after changing scenes to refresh minimap
        /// Add to your existing scene transition code
        /// </summary>
        public void RefreshMinimap()
        {
            if (HUDManager.Instance != null)
            {
                HUDManager.Instance.OnMapChanged();
            }
        }

        /// <summary>
        /// Call this before battle starts - Add to your existing StartBattle method
        /// </summary>
        public void HideMinimapForBattle()
        {
            if (HUDManager.Instance != null)
            {
                HUDManager.Instance.Visible = false;
            }
        }

        /// <summary>
        /// Call this after battle ends - Add to your existing ReturnToOverworld method
        /// </summary>
        public void ShowMinimapAfterBattle()
        {
            if (HUDManager.Instance != null)
            {
                HUDManager.Instance.Visible = true;
            }
        }

        /// <summary>
        /// Toggle minimap on/off
        /// </summary>
        public void ToggleMinimap(bool visible)
        {
            if (HUDManager.Instance != null)
            {
                HUDManager.Instance.SetMinimapVisible(visible);
            }
        }

        #endregion

    }
}