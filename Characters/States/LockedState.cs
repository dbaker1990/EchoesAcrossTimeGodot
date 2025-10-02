using Godot;

public partial class LockedState : CharacterState
{
    public LockedState()
    {
        StateType = CharacterStateType.Locked;
    }
    
    public override void PhysicsUpdate(double delta)
    {
        character.ApplyFriction(delta);
    }
    
    public override string GetAnimationName()
    {
        // Maintain current animation direction
        string direction = character.CurrentDirection switch
        {
            OverworldCharacter.Direction.Down => "down",
            OverworldCharacter.Direction.Up => "up",
            OverworldCharacter.Direction.Left => "left",
            OverworldCharacter.Direction.Right => "right",
            _ => "down"
        };
        
        return $"idle_{direction}";
    }
}