using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

#if TOOLS
namespace EchoesAcrossTime.Characters
{
    /// <summary>
    /// Editor tool to visually create and edit patrol paths for GuardNPC.
    /// Add this as a child of your Guard node in the editor.
    /// 
    /// USAGE:
    /// 1. Add this script to a Node2D child of your GuardNPC
    /// 2. Click "Add Waypoint At Guard Position" to create waypoints
    /// 3. Move the waypoint markers in the 2D editor
    /// 4. Click "Apply Path To Guard" to transfer waypoints
    /// 5. Remove this helper node before running the game
    /// </summary>
    [Tool]
    public partial class PatrolPathHelper : Node2D
    {
        [Export] public bool AddWaypointAtPosition { get; set; } = false;
        [Export] public bool ClearAllWaypoints { get; set; } = false;
        [Export] public bool ApplyPathToGuard { get; set; } = false;
        [Export] public bool ShowWaypointNumbers { get; set; } = true;
        [Export] public Color WaypointColor { get; set; } = Colors.Yellow;
        [Export] public float WaypointRadius { get; set; } = 8f;
        [Export] public Color PathLineColor { get; set; } = Colors.Cyan;
        
        private List<Marker2D> waypoints = new();
        private bool lastAddState = false;
        private bool lastClearState = false;
        private bool lastApplyState = false;
        
        public override void _Ready()
        {
            if (!Engine.IsEditorHint()) return;
            
            // Find existing waypoint markers
            RefreshWaypointList();
        }
        
        public override void _Process(double delta)
        {
            if (!Engine.IsEditorHint()) return;
            
            // Button: Add Waypoint
            if (AddWaypointAtPosition && !lastAddState)
            {
                CreateWaypoint();
                AddWaypointAtPosition = false;
            }
            lastAddState = AddWaypointAtPosition;
            
            // Button: Clear Waypoints
            if (ClearAllWaypoints && !lastClearState)
            {
                ClearWaypoints();
                ClearAllWaypoints = false;
            }
            lastClearState = ClearAllWaypoints;
            
            // Button: Apply to Guard
            if (ApplyPathToGuard && !lastApplyState)
            {
                ApplyToGuard();
                ApplyPathToGuard = false;
            }
            lastApplyState = ApplyPathToGuard;
            
            QueueRedraw();
        }
        
        public override void _Draw()
        {
            if (!Engine.IsEditorHint()) return;
            
            RefreshWaypointList();
            
            if (waypoints.Count == 0) return;
            
            // Draw path lines
            for (int i = 0; i < waypoints.Count - 1; i++)
            {
                Vector2 start = ToLocal(waypoints[i].GlobalPosition);
                Vector2 end = ToLocal(waypoints[i + 1].GlobalPosition);
                DrawLine(start, end, PathLineColor, 2f);
                
                // Draw arrow
                DrawArrow(start, end, PathLineColor);
            }
            
            // Draw waypoint circles and numbers
            for (int i = 0; i < waypoints.Count; i++)
            {
                Vector2 pos = ToLocal(waypoints[i].GlobalPosition);
                DrawCircle(pos, WaypointRadius, WaypointColor);
                DrawCircle(pos, WaypointRadius + 2f, Colors.Black, false, 2f);
                
                if (ShowWaypointNumbers)
                {
                    var font = ThemeDB.FallbackFont;
                    DrawString(font, pos + new Vector2(-4, -15), i.ToString(), 
                        HorizontalAlignment.Center, -1, 16, Colors.White);
                }
            }
        }
        
        private void DrawArrow(Vector2 from, Vector2 to, Color color)
        {
            Vector2 direction = (to - from).Normalized();
            Vector2 midPoint = (from + to) / 2f;
            
            float arrowSize = 10f;
            Vector2 perpendicular = new Vector2(-direction.Y, direction.X);
            
            Vector2 arrowPoint1 = midPoint - direction * arrowSize + perpendicular * (arrowSize / 2f);
            Vector2 arrowPoint2 = midPoint - direction * arrowSize - perpendicular * (arrowSize / 2f);
            
            DrawLine(midPoint, arrowPoint1, color, 2f);
            DrawLine(midPoint, arrowPoint2, color, 2f);
        }
        
        private void CreateWaypoint()
        {
            var guard = GetParentOrNull<GuardNPC>();
            if (guard == null)
            {
                GD.PrintErr("PatrolPathHelper must be a child of GuardNPC!");
                return;
            }
            
            // Create new waypoint marker at guard's current position
            var marker = new Marker2D();
            marker.Name = $"Waypoint{waypoints.Count}";
            marker.GlobalPosition = guard.GlobalPosition;
            
            AddChild(marker);
            marker.Owner = GetTree().EditedSceneRoot;
            
            waypoints.Add(marker);
            
            GD.Print($"Created waypoint {waypoints.Count - 1} at {marker.GlobalPosition}");
            QueueRedraw();
        }
        
        private void ClearWaypoints()
        {
            foreach (var waypoint in waypoints)
            {
                waypoint.QueueFree();
            }
            waypoints.Clear();
            
            GD.Print("Cleared all waypoints");
            QueueRedraw();
        }
        
        private void RefreshWaypointList()
        {
            waypoints.Clear();
            
            foreach (Node child in GetChildren())
            {
                if (child is Marker2D marker && child.Name.ToString().StartsWith("Waypoint"))
                {
                    waypoints.Add(marker);
                }
            }
            
            // Sort by name to maintain order
            waypoints = waypoints.OrderBy(w => w.Name.ToString()).ToList();
        }
        
        private void ApplyToGuard()
        {
            var guard = GetParentOrNull<GuardNPC>();
            if (guard == null)
            {
                GD.PrintErr("PatrolPathHelper must be a child of GuardNPC!");
                return;
            }
            
            if (waypoints.Count == 0)
            {
                GD.PrintErr("No waypoints to apply! Create some waypoints first.");
                return;
            }
            
            RefreshWaypointList();
            
            // Convert marker positions to world coordinates array
            Vector2[] path = waypoints.Select(w => w.GlobalPosition).ToArray();
            
            // Apply to guard
            guard.PatrolPath = path;
            
            GD.Print($"Applied {path.Length} waypoints to guard {guard.Name}:");
            for (int i = 0; i < path.Length; i++)
            {
                GD.Print($"  Waypoint {i}: {path[i]}");
            }
            
            GD.Print("✓ Patrol path applied successfully!");
            GD.Print("You can now remove this PatrolPathHelper node from the scene.");
        }
        
        public override string[] _GetConfigurationWarnings()
        {
            var warnings = new List<string>();
            
            if (GetParentOrNull<GuardNPC>() == null)
            {
                warnings.Add("This node should be a child of a GuardNPC node!");
            }
            
            if (waypoints.Count == 0)
            {
                warnings.Add("No waypoints created yet. Click 'Add Waypoint At Position' to start.");
            }
            else if (waypoints.Count == 1)
            {
                warnings.Add("Only 1 waypoint. Add at least 2 for a patrol path.");
            }
            
            return warnings.ToArray();
        }
    }
}
#endif