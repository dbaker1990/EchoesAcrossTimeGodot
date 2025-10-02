using Godot;

public partial class PlayerCharacter : OverworldCharacter
{
    [ExportGroup("Input")]
    [Export] public string MoveUpAction { get; set; } = "move_up";
    [Export] public string MoveDownAction { get; set; } = "move_down";
    [Export] public string MoveLeftAction { get; set; } = "move_left";
    [Export] public string MoveRightAction { get; set; } = "move_right";
    [Export] public string RunAction { get; set; } = "run";
    [Export] public string InteractAction { get; set; } = "interact";
    [Export] public string JumpAction { get; set; } = "jump";
    
    public override void _Ready()
    {
        base._Ready();
        IsPlayerControlled = true;
    }
    
    public override void _PhysicsProcess(double delta)
    {
        // Update input
        if (CurrentStateType != CharacterStateType.Locked)
        {
            InputDirection = Input.GetVector(MoveLeftAction, MoveRightAction, MoveUpAction, MoveDownAction);
            IsRunning = Input.IsActionPressed(RunAction);
        }
        else
        {
            InputDirection = Vector2.Zero;
            IsRunning = false;
        }
        
        base._PhysicsProcess(delta);
    }
    
    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        
        if (CurrentStateType == CharacterStateType.Locked)
            return;
        
        // Interact action
        if (@event.IsActionPressed(InteractAction))
        {
            HandleInteraction();
        }
        
        // Jump action
        if (@event.IsActionPressed(JumpAction))
        {
            TryJump();
        }
    }
    
    private void HandleInteraction()
    {
        var interactable = GetCurrentInteractable();
        
        if (interactable != null && interactable.CanInteract(this))
        {
            string interactionType = interactable.GetInteractionType();
            
            switch (interactionType)
            {
                case "chest":
                    ChangeState(CharacterStateType.OpeningChest);
                    break;
                case "fishing_spot":
                    ChangeState(CharacterStateType.Fishing);
                    break;
                case "climbable":
                    ChangeState(CharacterStateType.Climbing);
                    break;
                case "pushable":
                    ChangeState(CharacterStateType.Pushing);
                    break;
                case "pullable":
                    ChangeState(CharacterStateType.Pulling);
                    break;
            }
            
            interactable.Interact(this);
        }
    }
    
    private void TryJump()
    {
        // Only jump if in appropriate state
        if (CurrentStateType == CharacterStateType.Idle || 
            CurrentStateType == CharacterStateType.Walking || 
            CurrentStateType == CharacterStateType.Running)
        {
            ChangeState(CharacterStateType.Jumping);
        }
    }
}