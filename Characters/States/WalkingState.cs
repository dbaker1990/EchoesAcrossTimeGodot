using Godot;

public partial class WalkingState : CharacterState
{
    public WalkingState()
    {
        StateType = CharacterStateType.Walking;
    }
    
    public override void PhysicsUpdate(double delta)
    {
        if (character.InputDirection == Vector2.Zero)
        {
            character.ChangeState(CharacterStateType.Idle);
            return;
        }
        
        if (character.IsRunning)
        {
            character.ChangeState(CharacterStateType.Running);
            return;
        }
        
        character.UpdateDirectionFromInput();
        character.ApplyMovement(delta, character.WalkSpeed);
    }
}