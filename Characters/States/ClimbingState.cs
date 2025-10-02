using Godot;

public partial class ClimbingState : CharacterState
{
    private Area2D climbableArea;
    private bool isClimbingUp = true;
    
    public ClimbingState()
    {
        StateType = CharacterStateType.Climbing;
    }
    
    public override void Enter(OverworldCharacter character)
    {
        base.Enter(character);
        
        // Disable horizontal collision while climbing
        character.SetCollisionLayerValue(1, false);
        character.Velocity = Vector2.Zero;
    }
    
    public override void Exit()
    {
        character.SetCollisionLayerValue(1, true);
    }
    
    public override void PhysicsUpdate(double delta)
    {
        // Only allow vertical movement while climbing
        float verticalInput = character.InputDirection.Y;
        
        if (Mathf.Abs(verticalInput) < 0.1f)
        {
            character.Velocity = Vector2.Zero;
        }
        else
        {
            character.Velocity = new Vector2(0, verticalInput * character.ClimbSpeed);
            
            // Update direction
            if (verticalInput < 0)
                character.SetDirection(OverworldCharacter.Direction.Up);
            else
                character.SetDirection(OverworldCharacter.Direction.Down);
        }
        
        character.MoveAndSlide();
        
        // Check if reached top or bottom
        if (ShouldExitClimbing())
        {
            character.ChangeState(CharacterStateType.Idle);
        }
    }
    
    private bool ShouldExitClimbing()
    {
        // Check if character pressed action to dismount
        // Or if they reached the end of the climbable
        return false; // Implement your logic
    }
    
    public override string GetAnimationName()
    {
        if (character.Velocity.Length() < 0.1f)
            return "climb_idle";
        
        return "climb";
    }
}