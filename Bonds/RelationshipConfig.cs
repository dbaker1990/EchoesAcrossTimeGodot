// Scripts/System/Relationship/RelationshipConfig.cs
using Godot;
using Godot.Collections;

[GlobalClass]
public partial class RelationshipConfig : Resource
{
    [Export] public int MaxPoints = 100;
    [Export] public Array<int> StageThresholds = new() { 0, 25, 55, 80 }; // Acquaintance, Confidant, Promise, Betrothal
    [Export] public string LockinOpensFlag = "act3_started";  // set this when Act 3 begins
    [Export] public string LockinClosedFlag = "final_march";  // after this, routes are locked
    [Export] public int SoftChoiceWindow = 10; // if top two within N points, show the final choice UI
}