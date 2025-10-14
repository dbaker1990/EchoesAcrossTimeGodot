#if TOOLS
using Godot;
using System;

[Tool]
public partial class SkillBuilderPlugin : EditorPlugin
{
    private Control dock;

    public override void _EnterTree()
    {
        // Load the scene file
        var scene = GD.Load<PackedScene>("res://addons/skill_builder/SkillBuilderUI.tscn");
        dock = scene.Instantiate<Control>();
        
        // Add to bottom panel
        AddControlToBottomPanel(dock, "Skill Builder");
        
        // Make it visible
        MakeBottomPanelItemVisible(dock);
        
        GD.Print("Skill Builder Plugin loaded with scene!");
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