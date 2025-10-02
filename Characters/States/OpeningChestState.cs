using Godot;

public partial class OpeningChestState : CharacterState
{
    private bool animationComplete = false;
    
    [Signal]
    public delegate void ChestOpenedEventHandler();
    
    public OpeningChestState()
    {
        StateType = CharacterStateType.OpeningChest;
    }
    
    public override void Enter(OverworldCharacter character)
    {
        base.Enter(character);
        animationComplete = false;
        character.Velocity = Vector2.Zero;
    }
    
    public override void Update(double delta)
    {
        // Wait for animation to complete
        if (!character.AnimatedSprite.IsPlaying() && !animationComplete)
        {
            animationComplete = true;
            EmitSignal(SignalName.ChestOpened);
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
        
        return $"open_chest_{direction}";
    }
}