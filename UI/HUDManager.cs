using Godot;

namespace EchoesAcrossTime.Managers
{
    /// <summary>
    /// Fully automatic HUD Manager - No manual calls needed!
    /// Automatically detects player, tilemap, and refreshes minimap on map changes
    /// </summary>
    public partial class HUDManager : CanvasLayer
    {
        public static HUDManager Instance { get; private set; }
        
        private MinimapTexture minimap;
        private Node2D currentPlayer;
        private TileMapLayer currentTileMap;
        private string lastScenePath = "";

        public override void _Ready()
        {
            Instance = this;
            
            // Get minimap reference
            minimap = GetNodeOrNull<MinimapTexture>("Minimap");
            
            if (minimap != null)
            {
                GD.Print("[HUDManager] Minimap found and ready");
            }
            else
            {
                GD.PrintErr("[HUDManager] Minimap node not found! Make sure you have a Control node named 'Minimap' with MinimapTexture.cs attached");
            }
            
            // Automatically detect nodes as they're added to the scene tree
            GetTree().NodeAdded += OnNodeAdded;
            GetTree().NodeRemoved += OnNodeRemoved;
            
            // Detect scene changes
            GetTree().ProcessFrame += CheckForSceneChange;
        }

        public override void _ExitTree()
        {
            if (Instance == this)
                Instance = null;
                
            GetTree().NodeAdded -= OnNodeAdded;
            GetTree().NodeRemoved -= OnNodeRemoved;
        }

        /// <summary>
        /// Automatically detect important nodes when added to scene
        /// </summary>
        private void OnNodeAdded(Node node)
        {
            // Auto-detect player
            if (node.IsInGroup("player") && node is Node2D player2D)
            {
                SetPlayer(player2D);
            }
            
            // Auto-detect tilemap
            if (node is TileMapLayer tileMapLayer)
            {
                SetTileMap(tileMapLayer);
                // Automatically refresh markers when new tilemap loads
                CallDeferred(MethodName.OnMapChanged);
            }
        }

        /// <summary>
        /// Track when nodes are removed
        /// </summary>
        private void OnNodeRemoved(Node node)
        {
            if (node == currentPlayer)
            {
                currentPlayer = null;
            }
            
            if (node == currentTileMap)
            {
                currentTileMap = null;
            }
        }

        /// <summary>
        /// Detect scene changes and auto-refresh minimap
        /// </summary>
        private void CheckForSceneChange()
        {
            var currentScene = GetTree().CurrentScene;
            if (currentScene == null) return;
            
            string currentPath = currentScene.SceneFilePath;
            
            // Scene changed?
            if (!string.IsNullOrEmpty(currentPath) && currentPath != lastScenePath)
            {
                lastScenePath = currentPath;
                GD.Print($"[HUDManager] Scene changed to: {currentPath}");
                
                // Small delay to ensure all nodes are loaded
                GetTree().CreateTimer(0.1).Timeout += OnMapChanged;
            }
        }

        /// <summary>
        /// Set the player reference for the minimap
        /// </summary>
        public void SetPlayer(Node2D player)
        {
            currentPlayer = player;
            
            if (minimap != null)
            {
                minimap.Set("player", player);
                GD.Print($"[HUDManager] Player set for minimap: {player.Name}");
            }
        }

        /// <summary>
        /// Set the tilemap reference for the minimap
        /// </summary>
        public void SetTileMap(TileMapLayer tileMap)
        {
            currentTileMap = tileMap;
            
            if (minimap != null)
            {
                minimap.Set("tileMapLayer", tileMap);
                GD.Print($"[HUDManager] TileMap set for minimap: {tileMap.Name}");
            }
        }

        /// <summary>
        /// Refresh the minimap (finds new doors/transfers in current scene)
        /// This is called automatically when scene changes
        /// </summary>
        public void OnMapChanged()
        {
            if (minimap != null)
            {
                minimap.RefreshMarkers();
                GD.Print("[HUDManager] Minimap markers refreshed");
            }
        }

        /// <summary>
        /// Show or hide the minimap
        /// </summary>
        public void SetMinimapVisible(bool visible)
        {
            if (minimap != null)
            {
                minimap.Visible = visible;
            }
        }

        /// <summary>
        /// Manually set world bounds if needed
        /// </summary>
        public void SetWorldBounds(Vector2 bounds)
        {
            if (minimap != null)
            {
                minimap.SetWorldBounds(bounds);
            }
        }

        /// <summary>
        /// Add a custom marker at runtime
        /// </summary>
        public void AddCustomMarker(Vector2 worldPosition, Color color, string type)
        {
            if (minimap != null)
            {
                minimap.AddCustomMarker(worldPosition, color, type);
            }
        }
    }
}