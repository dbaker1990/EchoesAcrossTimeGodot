using Godot;

public partial class JumpingState : CharacterState
{
    private Vector2 startPosition;
    private Vector2 endPosition;
    private float jumpTimer = 0f;
    private float jumpDuration;
    private float jumpHeight;
    
    public JumpingState()
    {
        StateType = CharacterStateType.Jumping;
    }
    
    public override void Enter(OverworldCharacter character)
    {
        base.Enter(character);
        
        startPosition = character.GlobalPosition;
        jumpDuration = character.JumpDuration;
        jumpHeight = character.JumpHeight;
        
        // Calculate end position based on direction
        Vector2 jumpDirection = character.GetDirectionVector();
        endPosition = startPosition + jumpDirection * character.JumpDistance;
        
        jumpTimer = 0f;
        
        // Disable collision during jump (optional)
        character.SetCollisionMaskValue(1, false);
    }
    
    public override void Exit()
    {
        // Re-enable collision
        character.SetCollisionMaskValue(1, true);
        
        // Ensure character lands properly
        character.GlobalPosition = endPosition;
        character.Velocity = Vector2.Zero;
    }
    
    public override void PhysicsUpdate(double delta)
    {
        jumpTimer += (float)delta;
        float progress = Mathf.Clamp(jumpTimer / jumpDuration, 0f, 1f);
        
        // Smooth arc motion
        float arcProgress = Mathf.Sin(progress * Mathf.Pi);
        
        // Horizontal movement
        character.GlobalPosition = startPosition.Lerp(endPosition, progress);
        
        // Vertical offset (jump arc)
        if (character.ShadowSprite != null)
        {
            character.AnimatedSprite.Position = new Vector2(0, -arcProgress * jumpHeight);
        }
        
        // Complete jump
        if (progress >= 1f)
        {
            if (character.ShadowSprite != null)
            {
                character.AnimatedSprite.Position = Vector2.Zero;
            }
            character.ChangeState(CharacterStateType.Idle);
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
        
        return $"jump_{direction}";
    }
}