using Godot;
using System;

public enum CharacterStateType
{
    Idle,
    Walking,
    Running,
    Jumping,
    Fishing,
    Pushing,
    Pulling,
    Climbing,
    OpeningChest,
    Locked // For cutscenes
}

public abstract partial class CharacterState : Node
{
    protected OverworldCharacter character;
    public CharacterStateType StateType { get; protected set; }
    
    public virtual void Enter(OverworldCharacter character)
    {
        this.character = character;
    }
    
    public virtual void Exit() { }
    
    public virtual void Update(double delta) { }
    
    public virtual void PhysicsUpdate(double delta) { }
    
    public virtual void HandleInput(InputEvent @event) { }
    
    public virtual string GetAnimationName()
    {
        return "idle_down";
    }
}