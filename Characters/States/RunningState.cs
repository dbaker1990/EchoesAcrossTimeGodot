using Godot;

public partial class RunningState : CharacterState
{
    public RunningState()
    {
        StateType = CharacterStateType.Running;
    }
    
    public override void PhysicsUpdate(double delta)
    {
        if (character.InputDirection == Vector2.Zero)
        {
            character.ChangeState(CharacterStateType.Idle);
            return;
        }
        
        if (!character.IsRunning)
        {
            character.ChangeState(CharacterStateType.Walking);
            return;
        }
        
        character.UpdateDirectionFromInput();
        character.ApplyMovement(delta, character.RunSpeed);
    }
}