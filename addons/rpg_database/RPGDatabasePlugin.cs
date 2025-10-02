// addons/rpg_database/RPGDatabasePlugin.cs
#if TOOLS
using Godot;

[Tool]
public partial class RPGDatabasePlugin : EditorPlugin
{
    private Control databasePanel;
    
    public override void _EnterTree()
    {
        databasePanel = GD.Load<PackedScene>("res://addons/rpg_database/DatabasePanel.tscn").Instantiate<Control>();
        AddControlToBottomPanel(databasePanel, "RPG Database");
    }
    
    public override void _ExitTree()
    {
        RemoveControlFromBottomPanel(databasePanel);
        databasePanel.QueueFree();
    }
}
#endif