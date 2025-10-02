using Godot;

public partial class PushingState : CharacterState
{
    private Node2D pushableObject;
    
    public PushingState()
    {
        StateType = CharacterStateType.Pushing;
    }
    
    public override void Enter(OverworldCharacter character)
    {
        base.Enter(character);
        
        // Find pushable object in front
        pushableObject = FindPushableObject();
    }
    
    public override void PhysicsUpdate(double delta)
    {
        if (character.InputDirection == Vector2.Zero || pushableObject == null)
        {
            character.ChangeState(CharacterStateType.Idle);
            return;
        }
        
        // Check if still facing the object
        Vector2 directionToObject = (pushableObject.GlobalPosition - character.GlobalPosition).Normalized();
        Vector2 inputDirection = character.InputDirection.Normalized();
        
        if (directionToObject.Dot(inputDirection) < 0.5f)
        {
            character.ChangeState(CharacterStateType.Idle);
            return;
        }
        
        character.UpdateDirectionFromInput();
        
        // Move character and object together
        float moveSpeed = character.PushPullSpeed;
        Vector2 movement = character.InputDirection * moveSpeed * (float)delta;
        
        if (pushableObject is CharacterBody2D pushableBody)
        {
            pushableBody.Velocity = character.InputDirection * moveSpeed;
            pushableBody.MoveAndSlide();
            
            // If object stopped (hit wall), stop pushing
            if (pushableBody.Velocity.Length() < moveSpeed * 0.5f)
            {
                character.Velocity = Vector2.Zero;
                return;
            }
        }
        
        character.ApplyMovement(delta, moveSpeed);
    }
    
    private Node2D FindPushableObject()
    {
        // Raycast or area check in front of character
        Vector2 checkPosition = character.GlobalPosition + character.GetDirectionVector() * 32f;
        
        // Implement your own detection logic here
        // This is a placeholder
        return null;
    }
}