using Godot;

public partial class Chest : Node2D, IInteractable
{
    [Export] public bool IsOpen { get; private set; } = false;
    [Export] public string ItemId { get; set; } = "potion";
    [Export] public AnimatedSprite2D ChestSprite { get; set; }
    
    private Area2D interactionArea;
    
    public override void _Ready()
    {
        interactionArea = GetNode<Area2D>("InteractionArea");
        
        if (ChestSprite == null)
            ChestSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        
        UpdateVisuals();
    }
    
    public string GetInteractionType() => "chest";
    
    public bool CanInteract(OverworldCharacter character)
    {
        return !IsOpen;
    }
    
    public void Interact(OverworldCharacter character)
    {
        if (IsOpen) return;
        
        IsOpen = true;
        UpdateVisuals();
        
        // Give item to player
        GD.Print($"Player received: {ItemId}");
    }
    
    private void UpdateVisuals()
    {
        if (ChestSprite != null)
        {
            ChestSprite.Animation = IsOpen ? "open" : "closed";
        }
    }
}