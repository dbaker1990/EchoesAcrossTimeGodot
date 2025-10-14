#if TOOLS
using Godot;
using System;

[Tool]
public partial class CharacterBuilderPlugin : EditorPlugin
{
    private CharacterBuilderDock dock;

    public override void _EnterTree()
    {
        dock = new CharacterBuilderDock();
        
        // Add to bottom panel
        AddControlToBottomPanel(dock, "Character Builder");
        
        // IMPORTANT: Make it visible!
        MakeBottomPanelItemVisible(dock);
        
        GD.Print("Character Builder Plugin loaded!");
    }

    public override void _ExitTree()
    {
        if (dock != null)
        {
            RemoveControlFromBottomPanel(dock);
            dock.QueueFree();
        }
    }
}
#endif