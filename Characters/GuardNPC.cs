using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EchoesAcrossTime.Characters
{
    /// <summary>
    /// NPC Guard that patrols a predetermined path with line of sight detection.
    /// When player enters vision cone, shows message and teleports player to spawn point.
    /// All guards in scene reset to their starting positions when a catch occurs.
    /// </summary>
    public partial class GuardNPC : OverworldCharacter
    {
        #region Exports
        [ExportGroup("Guard Patrol Settings")]
        [Export] public Vector2[] PatrolPath { get; set; } = Array.Empty<Vector2>();
        [Export] public float PatrolSpeed { get; set; } = 80f;
        [Export] public float PatrolWaitTime { get; set; } = 2f;
        [Export] public bool LoopPatrol { get; set; } = true;
        [Export] public bool ReverseOnEnd { get; set; } = true;
        
        [ExportGroup("Vision Cone Settings")]
        [Export] public float VisionRange { get; set; } = 150f;
        [Export] public float VisionAngle { get; set; } = 60f;
        [Export] public Color VisionConeColor { get; set; } = new Color(1f, 0f, 0f, 0.3f);
        [Export] public bool ShowVisionCone { get; set; } = true;
        
        [ExportGroup("Detection Settings")]
        [Export] public string CatchMessage { get; set; } = "Hey! You there!";
        [Export] public float MessageDisplayTime { get; set; } = 1.5f;
        [Export] public NodePath PlayerSpawnPointPath { get; set; }
        [Export] public string PlayerGroupName { get; set; } = "player";
        
        [ExportGroup("References")]
        [Export] public Label MessageLabel { get; set; }
        #endregion
        
        #region Private Fields
        private int currentWaypointIndex = 0;
        private bool isWaiting = false;
        private float waitTimer = 0f;
        private bool isReturningToPath = false;
        private bool patrolForward = true;
        
        private Vector2 startingPosition;
        private Direction startingDirection;
        
        private Node2D playerSpawnPoint;
        private Node2D detectedPlayer;
        private bool hasDetectedPlayer = false;
        
        // Vision cone mesh for visualization
        private MeshInstance2D visionConeMesh;
        private const int VISION_CONE_SEGMENTS = 20;
        #endregion
        
        #region Initialization
        public override void _Ready()
        {
            base._Ready();
            
            // Store starting position and direction
            startingPosition = GlobalPosition;
            startingDirection = CurrentDirection;
            
            // Get player spawn point
            if (!PlayerSpawnPointPath.IsEmpty)
            {
                playerSpawnPoint = GetNode<Node2D>(PlayerSpawnPointPath);
            }
            
            // Create vision cone visualization
            CreateVisionCone();
            
            // Validate patrol path
            if (PatrolPath.Length == 0)
            {
                GD.PushWarning($"Guard {Name}: No patrol path set! Add waypoints in Inspector.");
                PatrolPath = new Vector2[] { GlobalPosition };
            }
            
            // Setup message label
            if (MessageLabel == null)
            {
                CreateMessageLabel();
            }
            
            GD.Print($"Guard {Name} initialized at {startingPosition} with {PatrolPath.Length} waypoints");
        }
        
        private void CreateMessageLabel()
        {
            MessageLabel = new Label();
            MessageLabel.AddThemeColorOverride("font_color", Colors.Red);
            MessageLabel.AddThemeFontSizeOverride("font_size", 24);
            MessageLabel.HorizontalAlignment = HorizontalAlignment.Center;
            MessageLabel.Position = new Vector2(-50, -80);
            MessageLabel.Size = new Vector2(100, 30);
            MessageLabel.Visible = false;
            AddChild(MessageLabel);
        }
        
        private void CreateVisionCone()
        {
            if (!ShowVisionCone) return;
            
            visionConeMesh = new MeshInstance2D();
            AddChild(visionConeMesh);
            
            UpdateVisionConeMesh();
        }
        
        private void UpdateVisionConeMesh()
        {
            if (visionConeMesh == null) return;
            
            var mesh = new ImmediateMesh();
            visionConeMesh.Mesh = mesh;
            
            // Create material for the cone
            var material = new StandardMaterial3D();
            
            // We'll redraw each frame in _Process
        }
        #endregion
        
        #region Main Loop
        public override void _Process(double delta)
        {
            base._Process(delta);
            
            if (hasDetectedPlayer) return;
            
            // Update vision cone visualization
            if (ShowVisionCone)
            {
                QueueRedraw();
            }
            
            // Check for player in vision cone
            CheckForPlayerInVision();
        }
        
        public override void _PhysicsProcess(double delta)
        {
            if (hasDetectedPlayer) return;
            
            base._PhysicsProcess(delta);
            
            // Handle patrol behavior
            if (isWaiting)
            {
                HandleWaiting(delta);
            }
            else
            {
                HandlePatrol(delta);
            }
        }
        
        public override void _Draw()
        {
            if (!ShowVisionCone) return;
            
            // Draw vision cone
            DrawVisionCone();
        }
        #endregion
        
        #region Patrol Logic
        private void HandlePatrol(double delta)
        {
            if (PatrolPath.Length == 0) return;
            
            Vector2 targetWaypoint = PatrolPath[currentWaypointIndex];
            Vector2 direction = (targetWaypoint - GlobalPosition).Normalized();
            float distance = GlobalPosition.DistanceTo(targetWaypoint);
            
            // Reached waypoint
            if (distance < 5f)
            {
                OnWaypointReached();
                return;
            }
            
            // Update facing direction
            UpdateDirectionFromMovement(direction);
            
            // Move towards waypoint
            Velocity = direction * PatrolSpeed;
            MoveAndSlide();
        }
        
        private void OnWaypointReached()
        {
            // Start waiting at this waypoint
            isWaiting = true;
            waitTimer = PatrolWaitTime;
            Velocity = Vector2.Zero;
            
            GD.Print($"Guard {Name} reached waypoint {currentWaypointIndex}");
        }
        
        private void HandleWaiting(double delta)
        {
            waitTimer -= (float)delta;
            
            if (waitTimer <= 0f)
            {
                isWaiting = false;
                AdvanceToNextWaypoint();
            }
        }
        
        private void AdvanceToNextWaypoint()
        {
            if (ReverseOnEnd)
            {
                // Ping-pong patrol
                if (patrolForward)
                {
                    currentWaypointIndex++;
                    if (currentWaypointIndex >= PatrolPath.Length)
                    {
                        currentWaypointIndex = PatrolPath.Length - 2;
                        patrolForward = false;
                    }
                }
                else
                {
                    currentWaypointIndex--;
                    if (currentWaypointIndex < 0)
                    {
                        currentWaypointIndex = 1;
                        patrolForward = true;
                    }
                }
            }
            else if (LoopPatrol)
            {
                // Loop patrol
                currentWaypointIndex = (currentWaypointIndex + 1) % PatrolPath.Length;
            }
            else
            {
                // One-way patrol
                currentWaypointIndex++;
                if (currentWaypointIndex >= PatrolPath.Length)
                {
                    currentWaypointIndex = PatrolPath.Length - 1;
                    Velocity = Vector2.Zero;
                }
            }
        }
        
        private void UpdateDirectionFromMovement(Vector2 moveDirection)
        {
            if (moveDirection.Length() < 0.1f) return;
            
            // Determine cardinal direction
            float angle = moveDirection.Angle();
            
            if (angle >= -Mathf.Pi/4 && angle < Mathf.Pi/4)
                CurrentDirection = Direction.Right;
            else if (angle >= Mathf.Pi/4 && angle < 3*Mathf.Pi/4)
                CurrentDirection = Direction.Down;
            else if (angle >= -3*Mathf.Pi/4 && angle < -Mathf.Pi/4)
                CurrentDirection = Direction.Up;
            else
                CurrentDirection = Direction.Left;
        }
        #endregion
        
        #region Vision Detection
        private void CheckForPlayerInVision()
        {
            var player = GetPlayerInScene();
            if (player == null) return;
            
            Vector2 toPlayer = player.GlobalPosition - GlobalPosition;
            float distanceToPlayer = toPlayer.Length();
            
            // Check if player is within range
            if (distanceToPlayer > VisionRange) return;
            
            // Get guard's facing direction as a vector
            Vector2 guardDirection = GetGuardDirectionVector();
            Vector2 toPlayerNormalized = toPlayer.Normalized();
            
            // Calculate angle between guard's direction and direction to player
            float angleToPlayer = Mathf.RadToDeg(guardDirection.AngleTo(toPlayerNormalized));
            float halfVisionAngle = VisionAngle / 2f;
            
            // Check if player is within vision cone angle
            if (Mathf.Abs(angleToPlayer) <= halfVisionAngle)
            {
                // Raycast to check line of sight (no obstacles)
                if (HasLineOfSight(player))
                {
                    OnPlayerDetected(player);
                }
            }
        }
        
        private bool HasLineOfSight(Node2D player)
        {
            var spaceState = GetWorld2D().DirectSpaceState;
            var query = PhysicsRayQueryParameters2D.Create(GlobalPosition, player.GlobalPosition);
            query.Exclude = new Godot.Collections.Array<Rid> { GetRid() };
            query.CollideWithAreas = false;
            query.CollideWithBodies = true;
            
            var result = spaceState.IntersectRay(query);
            
            // If raycast hits nothing, or hits the player, we have line of sight
            return result.Count == 0 || result["collider"].As<Node>() == player;
        }
        
        private Node2D GetPlayerInScene()
        {
            var players = GetTree().GetNodesInGroup(PlayerGroupName);
            return players.Count > 0 ? players[0] as Node2D : null;
        }
        
        private Vector2 GetGuardDirectionVector()
        {
            return CurrentDirection switch
            {
                Direction.Up => Vector2.Up,
                Direction.Down => Vector2.Down,
                Direction.Left => Vector2.Left,
                Direction.Right => Vector2.Right,
                _ => Vector2.Down
            };
        }
        #endregion
        
        #region Player Detection Response
        private async void OnPlayerDetected(Node2D player)
        {
            if (hasDetectedPlayer) return;
            
            hasDetectedPlayer = true;
            detectedPlayer = player;
            
            GD.Print($"Guard {Name} detected player!");
            
            // Stop movement
            Velocity = Vector2.Zero;
            
            // Show message
            await ShowCatchMessage();
            
            // Reset all guards in the scene
            ResetAllGuards();
            
            // Teleport player
            TeleportPlayerToSpawn();
            
            // Reset this guard
            await GetTree().CreateTimer(0.5f).ToSignal(GetTree().CreateTimer(0.5f), "timeout");
            hasDetectedPlayer = false;
        }
        
        private async System.Threading.Tasks.Task ShowCatchMessage()
        {
            if (MessageLabel == null) return;
            
            MessageLabel.Text = CatchMessage;
            MessageLabel.Visible = true;
            
            await ToSignal(GetTree().CreateTimer(MessageDisplayTime), "timeout");
            
            MessageLabel.Visible = false;
        }
        
        private void TeleportPlayerToSpawn()
        {
            if (detectedPlayer == null) return;
            
            if (playerSpawnPoint != null)
            {
                detectedPlayer.GlobalPosition = playerSpawnPoint.GlobalPosition;
                GD.Print($"Player teleported to spawn point: {playerSpawnPoint.GlobalPosition}");
            }
            else
            {
                GD.PushWarning($"Guard {Name}: No player spawn point set!");
            }
        }
        
        private void ResetAllGuards()
        {
            // Find all guards in the scene
            var allGuards = GetTree().GetNodesInGroup("guard");
            
            foreach (var node in allGuards)
            {
                if (node is GuardNPC guard)
                {
                    guard.ResetToStartPosition();
                }
            }
            
            GD.Print($"Reset {allGuards.Count} guards to starting positions");
        }
        #endregion
        
        #region Reset System
        public void ResetToStartPosition()
        {
            GlobalPosition = startingPosition;
            CurrentDirection = startingDirection;
            currentWaypointIndex = 0;
            isWaiting = false;
            waitTimer = 0f;
            patrolForward = true;
            Velocity = Vector2.Zero;
            hasDetectedPlayer = false;
            
            if (MessageLabel != null)
            {
                MessageLabel.Visible = false;
            }
            
            GD.Print($"Guard {Name} reset to position {startingPosition}");
        }
        #endregion
        
        #region Vision Cone Visualization
        private void DrawVisionCone()
        {
            Vector2 guardDir = GetGuardDirectionVector();
            float halfAngle = Mathf.DegToRad(VisionAngle / 2f);
            float guardAngle = guardDir.Angle();
            
            // Create polygon for vision cone
            var points = new List<Vector2> { Vector2.Zero };
            
            for (int i = 0; i <= VISION_CONE_SEGMENTS; i++)
            {
                float angle = guardAngle - halfAngle + (halfAngle * 2f * i / VISION_CONE_SEGMENTS);
                Vector2 point = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * VisionRange;
                points.Add(point);
            }
            
            // Draw filled polygon
            DrawColoredPolygon(points.ToArray(), VisionConeColor);
            
            // Draw outline
            DrawPolyline(points.ToArray(), Colors.Red, 2f);
        }
        #endregion
        
        #region Public API
        /// <summary>
        /// Call this to add the guard to the "guard" group for reset functionality
        /// </summary>
        public void RegisterGuard()
        {
            if (!IsInGroup("guard"))
            {
                AddToGroup("guard");
            }
        }
        
        /// <summary>
        /// Manually set patrol waypoints at runtime
        /// </summary>
        public void SetPatrolPath(Vector2[] waypoints)
        {
            PatrolPath = waypoints;
            currentWaypointIndex = 0;
            GD.Print($"Guard {Name} patrol path updated with {waypoints.Length} waypoints");
        }
        
        /// <summary>
        /// Pause/resume guard patrol
        /// </summary>
        public void SetPatrolActive(bool active)
        {
            SetPhysicsProcess(active);
            SetProcess(active);
        }
        #endregion
    }
}