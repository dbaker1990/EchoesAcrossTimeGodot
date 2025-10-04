using Godot;
using EchoesAcrossTime.Encounters;

namespace EchoesAcrossTime.Characters.States
{
    /// <summary>
    /// Walking state with encounter step tracking
    /// </summary>
    public partial class WalkingState : CharacterState
    {
        private float stepTimer = 0f;
        private const float stepInterval = 0.3f; // Trigger step every 0.3 seconds of walking
        private Vector2 lastStepPosition;
        private const float minDistanceForStep = 16f; // Minimum pixels moved to count as step
        
        public WalkingState()
        {
            StateType = CharacterStateType.Walking;
        }
        
        public override void Enter(OverworldCharacter character)
        {
            base.Enter(character);
            stepTimer = 0f;
            lastStepPosition = character.GlobalPosition;
        }
        
        public override void PhysicsUpdate(double delta)
        {
            // Check if still moving
            if (character.InputDirection == Vector2.Zero)
            {
                character.ChangeState(CharacterStateType.Idle);
                return;
            }
            
            // Update facing direction
            character.UpdateDirectionFromInput();
            
            // Apply movement
            float moveSpeed = character.IsRunning ? character.RunSpeed : character.WalkSpeed;
            character.ApplyMovement(delta, moveSpeed);
            
            // Track steps for encounters (only for player)
            if (character.IsInGroup("player") && character.Velocity.Length() > 0)
            {
                TrackStep(delta);
            }
        }
        
        /// <summary>
        /// Track player steps for random encounters
        /// </summary>
        private void TrackStep(double delta)
        {
            stepTimer += (float)delta;
            
            // Method 1: Time-based stepping (every X seconds of walking)
            if (stepTimer >= stepInterval)
            {
                stepTimer = 0f;
                TriggerStep();
            }
            
            // Method 2: Distance-based stepping (uncomment to use instead)
            /*
            float distanceMoved = character.GlobalPosition.DistanceTo(lastStepPosition);
            if (distanceMoved >= minDistanceForStep)
            {
                lastStepPosition = character.GlobalPosition;
                TriggerStep();
            }
            */
        }
        
        /// <summary>
        /// Trigger a step event for encounter checking
        /// </summary>
        private void TriggerStep()
        {
            // Only count steps if encounters are enabled
            if (EncounterManager.Instance != null && EncounterManager.Instance.EncountersEnabled)
            {
                EncounterManager.Instance.OnPlayerStep();
            }
        }
        
        public override string GetAnimationName()
        {
            string direction = character.CurrentDirection switch
            {
                OverworldCharacter.Direction.Down => "down",
                OverworldCharacter.Direction.Up => "up",
                OverworldCharacter.Direction.Left => "left",
                OverworldCharacter.Direction.Right => "right",
                _ => "down"
            };
            
            string action = character.IsRunning ? "run" : "walk";
            return $"{action}_{direction}";
        }
    }
}