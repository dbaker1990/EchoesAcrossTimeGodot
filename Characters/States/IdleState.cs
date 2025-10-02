using Godot;

public partial class IdleState : CharacterState
{
    public IdleState()
    {
        StateType = CharacterStateType.Idle;
    }
    
    public override void PhysicsUpdate(double delta)
    {
        character.ApplyFriction(delta);
        
        // Transition to walking/running if input detected
        if (character.IsPlayerControlled && character.InputDirection != Vector2.Zero)
        {
            if (character.IsRunning)
                character.ChangeState(CharacterStateType.Running);
            else
                character.ChangeState(CharacterStateType.Walking);
        }
    }
}