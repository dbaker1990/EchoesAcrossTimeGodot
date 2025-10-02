using Godot;

public interface IInteractable
{
    string GetInteractionType(); // "chest", "npc", "climbable", "pushable", etc.
    void Interact(OverworldCharacter character);
    bool CanInteract(OverworldCharacter character);
}