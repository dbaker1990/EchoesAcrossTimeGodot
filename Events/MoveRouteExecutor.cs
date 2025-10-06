// Events/MoveRouteExecutor.cs
using Godot;
using System;
using System.Threading.Tasks;

namespace EchoesAcrossTime.Events
{
    /// <summary>
    /// Helper class to execute move routes on characters
    /// This gets attached to characters at runtime when SetMoveRouteCommand is used
    /// </summary>
    public partial class MoveRouteExecutor : Node
    {
        private Node2D character;
        private AnimatedSprite2D sprite;
        private CharacterBody2D body;
        private bool isExecuting = false;
        
        // Movement properties
        private float moveSpeed = 100f;
        private float moveFrequency = 1f;
        private Vector2 targetPosition;
        private bool isMoving = false;
        
        /// <summary>
        /// Execute a move route
        /// </summary>
        public async Task ExecuteRoute(Node2D targetCharacter, 
            Godot.Collections.Array<MoveCommand> route, 
            bool repeat, bool skipIfBlocked)
        {
            if (isExecuting)
            {
                GD.PrintErr("Move route already executing on this character");
                return;
            }
            
            character = targetCharacter;
            sprite = character.GetNodeOrNull<AnimatedSprite2D>("Sprite");
            body = character as CharacterBody2D;
            
            isExecuting = true;
            
            do
            {
                foreach (var command in route)
                {
                    bool success = await ExecuteCommand(command, skipIfBlocked);
                    
                    // If blocked and not skipping, stop the route
                    if (!success && !skipIfBlocked)
                    {
                        GD.Print("Move route blocked, stopping execution");
                        isExecuting = false;
                        return;
                    }
                }
            }
            while (repeat && isExecuting);
            
            isExecuting = false;
        }
        
        /// <summary>
        /// Stop the current route
        /// </summary>
        public void StopRoute()
        {
            isExecuting = false;
            isMoving = false;
        }
        
        /// <summary>
        /// Execute a single move command
        /// </summary>
        private async Task<bool> ExecuteCommand(MoveCommand command, bool skipIfBlocked)
        {
            switch (command)
            {
                // Directional movement
                case MoveCommand.MoveUp:
                    return await MoveInDirection(Vector2.Up);
                    
                case MoveCommand.MoveDown:
                    return await MoveInDirection(Vector2.Down);
                    
                case MoveCommand.MoveLeft:
                    return await MoveInDirection(Vector2.Left);
                    
                case MoveCommand.MoveRight:
                    return await MoveInDirection(Vector2.Right);
                
                // Turning
                case MoveCommand.TurnUp:
                    SetDirection(Vector2.Up);
                    return true;
                    
                case MoveCommand.TurnDown:
                    SetDirection(Vector2.Down);
                    return true;
                    
                case MoveCommand.TurnLeft:
                    SetDirection(Vector2.Left);
                    return true;
                    
                case MoveCommand.TurnRight:
                    SetDirection(Vector2.Right);
                    return true;
                
                // Advanced turns
                case MoveCommand.Turn90Left:
                    await Turn90Degrees(false);
                    return true;
                    
                case MoveCommand.Turn90Right:
                    await Turn90Degrees(true);
                    return true;
                    
                case MoveCommand.Turn180:
                    await Turn180Degrees();
                    return true;
                    
                case MoveCommand.TurnRandom:
                    TurnRandom();
                    return true;
                
                // Special moves
                case MoveCommand.Jump:
                    await ExecuteJump();
                    return true;
                    
                case MoveCommand.Wait:
                    await WaitCommand();
                    return true;
                    
                case MoveCommand.StepForward:
                    return await StepForward();
                    
                case MoveCommand.StepBackward:
                    return await StepBackward();
                
                // Property changes
                case MoveCommand.ChangeSpeed:
                    moveSpeed *= 1.5f; // Increase speed by 50%
                    return true;
                    
                case MoveCommand.ChangeFrequency:
                    moveFrequency *= 1.5f;
                    return true;
                
                // Visual changes
                case MoveCommand.ChangeGraphic:
                    // TODO: Implement graphic change
                    GD.Print("ChangeGraphic not yet implemented");
                    return true;
                    
                case MoveCommand.ChangeOpacity:
                    await ChangeOpacity(0.5f);
                    return true;
                    
                case MoveCommand.PlayAnimation:
                    PlayAnimation("default");
                    return true;
                
                // Switch operations
                case MoveCommand.SwitchOn:
                case MoveCommand.SwitchOff:
                    // These would affect the character's collision or visibility
                    GD.Print($"{command} not yet implemented");
                    return true;
                
                default:
                    GD.PrintErr($"Unknown move command: {command}");
                    return true;
            }
        }
        
        #region Movement Implementation
        
        private async Task<bool> MoveInDirection(Vector2 direction)
        {
            if (character == null) return false;
            
            SetDirection(direction);
            
            Vector2 targetPos = character.GlobalPosition + direction * 16; // Tile size
            
            // Check if blocked (if CharacterBody2D)
            if (body != null)
            {
                var collision = body.MoveAndCollide(direction * 1f, true);
                if (collision != null)
                {
                    return false; // Blocked
                }
            }
            
            // Animate movement
            float distance = character.GlobalPosition.DistanceTo(targetPos);
            float duration = distance / moveSpeed;
            
            PlayAnimation("walk");
            
            Tween tween = character.CreateTween();
            tween.TweenProperty(character, "global_position", targetPos, duration);
            await character.ToSignal(tween, Tween.SignalName.Finished);
            
            PlayAnimation("idle");
            
            return true;
        }
        
        private async Task<bool> StepForward()
        {
            Vector2 direction = GetCurrentDirection();
            return await MoveInDirection(direction);
        }
        
        private async Task<bool> StepBackward()
        {
            Vector2 direction = -GetCurrentDirection();
            return await MoveInDirection(direction);
        }
        
        #endregion
        
        #region Turning Implementation
        
        private void SetDirection(Vector2 direction)
        {
            if (sprite == null) return;
            
            // Set animation based on direction
            if (direction == Vector2.Up)
                sprite.Animation = "idle_up";
            else if (direction == Vector2.Down)
                sprite.Animation = "idle_down";
            else if (direction == Vector2.Left)
            {
                sprite.Animation = "idle_side";
                sprite.FlipH = true;
            }
            else if (direction == Vector2.Right)
            {
                sprite.Animation = "idle_side";
                sprite.FlipH = false;
            }
        }
        
        private async Task Turn90Degrees(bool clockwise)
        {
            Vector2 currentDir = GetCurrentDirection();
            Vector2 newDir;
            
            if (clockwise)
            {
                if (currentDir == Vector2.Up) newDir = Vector2.Right;
                else if (currentDir == Vector2.Right) newDir = Vector2.Down;
                else if (currentDir == Vector2.Down) newDir = Vector2.Left;
                else newDir = Vector2.Up;
            }
            else
            {
                if (currentDir == Vector2.Up) newDir = Vector2.Left;
                else if (currentDir == Vector2.Left) newDir = Vector2.Down;
                else if (currentDir == Vector2.Down) newDir = Vector2.Right;
                else newDir = Vector2.Up;
            }
            
            SetDirection(newDir);
            await character.ToSignal(character.GetTree().CreateTimer(0.1f), SceneTreeTimer.SignalName.Timeout);
        }
        
        private async Task Turn180Degrees()
        {
            Vector2 currentDir = GetCurrentDirection();
            SetDirection(-currentDir);
            await character.ToSignal(character.GetTree().CreateTimer(0.1f), SceneTreeTimer.SignalName.Timeout);
        }
        
        private void TurnRandom()
        {
            var rng = new RandomNumberGenerator();
            rng.Randomize();
            int dir = rng.RandiRange(0, 3);
            
            Vector2 newDir = dir switch
            {
                0 => Vector2.Up,
                1 => Vector2.Down,
                2 => Vector2.Left,
                _ => Vector2.Right
            };
            
            SetDirection(newDir);
        }
        
        private Vector2 GetCurrentDirection()
        {
            if (sprite == null) return Vector2.Down;
            
            // Convert StringName to string for comparison
            string currentAnim = sprite.Animation.ToString();
            
            if (currentAnim.Contains("up"))
                return Vector2.Up;
            else if (currentAnim.Contains("down"))
                return Vector2.Down;
            else if (sprite.FlipH)
                return Vector2.Left;
            else
                return Vector2.Right;
        }
        
        #endregion
        
        #region Special Actions
        
        private async Task ExecuteJump()
        {
            if (character == null) return;
            
            Vector2 direction = GetCurrentDirection();
            Vector2 targetPos = character.GlobalPosition + direction * 32; // Jump 2 tiles
            
            // Create jump arc
            Tween tween = character.CreateTween();
            tween.SetParallel(true);
            
            // Horizontal movement
            tween.TweenProperty(character, "global_position", targetPos, 0.4f);
            
            // Vertical arc (if sprite exists)
            if (sprite != null)
            {
                tween.TweenProperty(sprite, "position:y", -20f, 0.2f);
                tween.Chain().TweenProperty(sprite, "position:y", 0f, 0.2f);
            }
            
            await character.ToSignal(tween, Tween.SignalName.Finished);
        }
        
        private async Task WaitCommand()
        {
            await character.ToSignal(character.GetTree().CreateTimer(0.5f), SceneTreeTimer.SignalName.Timeout);
        }
        
        private async Task ChangeOpacity(float targetOpacity)
        {
            if (sprite == null) return;
            
            Tween tween = character.CreateTween();
            tween.TweenProperty(sprite, "modulate:a", targetOpacity, 0.3f);
            await character.ToSignal(tween, Tween.SignalName.Finished);
        }
        
        private void PlayAnimation(string animName)
        {
            if (sprite == null) return;
            
            // This is a simplified version - adjust based on your sprite setup
            if (sprite.SpriteFrames.HasAnimation(animName))
            {
                sprite.Play(animName);
            }
        }
        
        #endregion
    }
}