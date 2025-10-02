using Godot;

public partial class PullingState : CharacterState
{
    private Node2D pullableObject;
    
    public PullingState()
    {
        StateType = CharacterStateType.Pulling;
    }
    
    public override void Enter(OverworldCharacter character)
    {
        base.Enter(character);
        pullableObject = FindPullableObject();
    }
    
    public override void PhysicsUpdate(double delta)
    {
        if (character.InputDirection == Vector2.Zero || pullableObject == null)
        {
            character.ChangeState(CharacterStateType.Idle);
            return;
        }
        
        character.UpdateDirectionFromInput();
        
        // Move character backward, object follows
        float moveSpeed = character.PushPullSpeed;
        character.ApplyMovement(delta, moveSpeed);
        
        if (pullableObject is CharacterBody2D pullableBody)
        {
            Vector2 targetPosition = character.GlobalPosition + character.GetDirectionVector() * 32f;
            pullableBody.GlobalPosition = pullableBody.GlobalPosition.Lerp(targetPosition, 10f * (float)delta);
        }
    }
    
    private Node2D FindPullableObject()
    {
        // Similar to pushing, but object is in front initially
        return null;
    }
}