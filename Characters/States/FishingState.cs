using Godot;

public partial class FishingState : CharacterState
{
    private bool isCasting = true;
    private bool isWaiting = false;
    private bool isReeling = false;
    private float waitTimer = 0f;
    private float biteWaitTime = 3f; // Random wait time for bite
    
    [Signal]
    public delegate void FishCaughtEventHandler();
    
    public FishingState()
    {
        StateType = CharacterStateType.Fishing;
    }
    
    public override void Enter(OverworldCharacter character)
    {
        base.Enter(character);
        isCasting = true;
        isWaiting = false;
        isReeling = false;
        waitTimer = 0f;
        biteWaitTime = GD.Randf() * 3f + 2f; // 2-5 seconds
    }
    
    public override void Update(double delta)
    {
        if (isCasting)
        {
            // Wait for cast animation to finish
            if (!character.AnimatedSprite.IsPlaying())
            {
                isCasting = false;
                isWaiting = true;
            }
        }
        else if (isWaiting)
        {
            waitTimer += (float)delta;
            
            if (waitTimer >= biteWaitTime)
            {
                // Fish bites!
                isWaiting = false;
                isReeling = true;
            }
        }
        else if (isReeling)
        {
            // Wait for reel animation
            if (!character.AnimatedSprite.IsPlaying())
            {
                // Fishing complete
                EmitSignal(SignalName.FishCaught);
                character.ChangeState(CharacterStateType.Idle);
            }
        }
    }
    
    public override void HandleInput(InputEvent @event)
    {
        // Allow canceling fishing
        if (@event.IsActionPressed("ui_cancel"))
        {
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
        
        if (isCasting)
            return $"fishing_cast_{direction}";
        else if (isWaiting)
            return $"fishing_wait_{direction}";
        else if (isReeling)
            return $"fishing_reel_{direction}";
        
        return $"fishing_wait_{direction}";
    }
}