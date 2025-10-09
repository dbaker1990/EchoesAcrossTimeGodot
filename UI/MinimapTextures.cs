using Godot;
using System.Collections.Generic;

/// <summary>
/// Simpler texture-based minimap that uses a pre-rendered map texture
/// This is more performant and easier to set up for 2D games
/// </summary>
public partial class MinimapTexture : Control
{
    [ExportGroup("Map Settings")]
    [Export] private Texture2D mapTexture; // Your map background
    [Export] private Vector2 mapSize = new Vector2(200, 200);
    [Export] private Vector2 worldBounds = new Vector2(1000, 1000); // Size of your actual map in world units
    
    [ExportGroup("References")]
    [Export] private Node2D player;
    [Export] private TileMapLayer tileMapLayer; // If using TileMap, for world bounds
    
    [ExportGroup("Player Icon")]
    [Export] private Texture2D playerIcon;
    [Export] private Color playerBoxColor = Colors.Yellow;
    [Export] private Vector2 playerIconSize = new Vector2(8, 8);
    
    [ExportGroup("Icons")]
    [Export] private Texture2D doorIcon;
    [Export] private Texture2D transferIcon;
    [Export] private Color doorColor = Colors.Red;
    [Export] private Color transferColor = Colors.Cyan;
    
    // UI Elements
    private TextureRect mapBackground;
    private ColorRect playerBox;
    private List<MapMarker> markers = new List<MapMarker>();
    
    private class MapMarker
    {
        public Vector2 WorldPosition;
        public Control IconControl;
        public string Type;
    }

    public override void _Ready()
    {
        SetupMinimapUI();
        
        // Auto-detect world bounds from TileMap if available
        if (tileMapLayer != null)
        {
            DetectWorldBounds();
        }
        
        FindAndMarkMapObjects();
    }

    private void SetupMinimapUI()
    {
        // Set minimap position (top-right corner)
        AnchorLeft = 1.0f;
        AnchorTop = 0.0f;
        AnchorRight = 1.0f;
        AnchorBottom = 0.0f;
        OffsetLeft = -mapSize.X - 20;
        OffsetTop = 20;
        OffsetRight = -20;
        OffsetBottom = mapSize.Y + 20;
        CustomMinimumSize = mapSize;
        
        // Background border
        var border = new Panel();
        border.Size = mapSize + new Vector2(4, 4);
        border.Position = new Vector2(-2, -2);
        AddChild(border);
        
        // Map background texture
        mapBackground = new TextureRect();
        mapBackground.Texture = mapTexture;
        mapBackground.Size = mapSize;
        mapBackground.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
        mapBackground.StretchMode = TextureRect.StretchModeEnum.Scale;
        AddChild(mapBackground);
        
        // Player box icon
        if (playerIcon != null)
        {
            var iconRect = new TextureRect();
            iconRect.Texture = playerIcon;
            iconRect.Size = playerIconSize;
            playerBox = new ColorRect(); // This will be hidden, just for positioning
            playerBox.Size = playerIconSize;
            playerBox.Color = Colors.Transparent;
            AddChild(playerBox);
            playerBox.AddChild(iconRect);
        }
        else
        {
            // Simple colored box if no icon provided
            playerBox = new ColorRect();
            playerBox.Color = playerBoxColor;
            playerBox.Size = playerIconSize;
            
            // Add white border
            var boxBorder = new ReferenceRect();
            boxBorder.BorderColor = Colors.White;
            boxBorder.BorderWidth = 1.0f;
            boxBorder.Size = playerIconSize;
            playerBox.AddChild(boxBorder);
            
            AddChild(playerBox);
        }
    }

    private void DetectWorldBounds()
    {
        if (tileMapLayer == null) return;
        
        // Get the used rect of the tilemap
        Rect2I usedRect = tileMapLayer.GetUsedRect();
        Vector2I tileSize = tileMapLayer.TileSet.TileSize;
        
        worldBounds = new Vector2(
            usedRect.Size.X * tileSize.X,
            usedRect.Size.Y * tileSize.Y
        );
        
        GD.Print($"Minimap: Detected world bounds: {worldBounds}");
    }

    public override void _Process(double delta)
    {
        if (player == null)
            return;
        
        UpdatePlayerPosition();
        UpdateMarkerPositions();
    }

    private void UpdatePlayerPosition()
    {
        Vector2 minimapPos = WorldToMinimapPosition(player.GlobalPosition);
        playerBox.Position = minimapPos - (playerIconSize / 2);
    }

    private Vector2 WorldToMinimapPosition(Vector2 worldPosition)
    {
        // Convert world coordinates to minimap coordinates
        // Normalize to 0-1 range based on world bounds
        Vector2 normalized = new Vector2(
            worldPosition.X / worldBounds.X,
            worldPosition.Y / worldBounds.Y
        );
        
        // Scale to minimap size
        return normalized * mapSize;
    }

    private void FindAndMarkMapObjects()
    {
        // Find all doors
        var doors = GetTree().GetNodesInGroup("door");
        foreach (Node door in doors)
        {
            if (door is Node2D door2D)
            {
                AddMarker(door2D.GlobalPosition, doorColor, "door", doorIcon);
            }
        }
        
        // Find all transfer/warp points
        var transfers = GetTree().GetNodesInGroup("transfer");
        foreach (Node transfer in transfers)
        {
            if (transfer is Node2D transfer2D)
            {
                AddMarker(transfer2D.GlobalPosition, transferColor, "transfer", transferIcon);
            }
        }
    }

    private void AddMarker(Vector2 worldPosition, Color color, string type, Texture2D icon = null)
    {
        Control markerControl;
        
        if (icon != null)
        {
            var iconRect = new TextureRect();
            iconRect.Texture = icon;
            iconRect.Size = new Vector2(6, 6);
            iconRect.Modulate = color;
            markerControl = iconRect;
        }
        else
        {
            // Simple colored square
            var colorRect = new ColorRect();
            colorRect.Color = color;
            colorRect.Size = new Vector2(4, 4);
            markerControl = colorRect;
        }
        
        AddChild(markerControl);
        
        markers.Add(new MapMarker
        {
            WorldPosition = worldPosition,
            IconControl = markerControl,
            Type = type
        });
    }

    private void UpdateMarkerPositions()
    {
        foreach (var marker in markers)
        {
            Vector2 minimapPos = WorldToMinimapPosition(marker.WorldPosition);
            
            // Check if within bounds
            if (minimapPos.X >= 0 && minimapPos.X <= mapSize.X &&
                minimapPos.Y >= 0 && minimapPos.Y <= mapSize.Y)
            {
                marker.IconControl.Position = minimapPos - (marker.IconControl.Size / 2);
                marker.IconControl.Visible = true;
            }
            else
            {
                marker.IconControl.Visible = false;
            }
        }
    }

    /// <summary>
    /// Call this when changing maps/areas to refresh markers
    /// </summary>
    public void RefreshMarkers()
    {
        // Clear existing markers
        foreach (var marker in markers)
        {
            marker.IconControl.QueueFree();
        }
        markers.Clear();
        
        // Redetect if using TileMap
        if (tileMapLayer != null)
        {
            DetectWorldBounds();
        }
        
        FindAndMarkMapObjects();
    }

    /// <summary>
    /// Manually set world bounds if not using TileMap
    /// </summary>
    public void SetWorldBounds(Vector2 bounds)
    {
        worldBounds = bounds;
    }

    /// <summary>
    /// Add a custom marker at runtime
    /// </summary>
    public void AddCustomMarker(Vector2 worldPosition, Color color, string type)
    {
        AddMarker(worldPosition, color, type);
    }

    /// <summary>
    /// Remove markers of a specific type
    /// </summary>
    public void RemoveMarkersByType(string type)
    {
        for (int i = markers.Count - 1; i >= 0; i--)
        {
            if (markers[i].Type == type)
            {
                markers[i].IconControl.QueueFree();
                markers.RemoveAt(i);
            }
        }
    }
}