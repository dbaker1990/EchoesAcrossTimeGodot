// Scripts/System/Bonds/BondConfig.cs
using Godot;
using Godot.Collections;

[GlobalClass]
public partial class BondConfig : Resource
{
    // Rank letters purely cosmetic; points drive them.
    [Export] public int MaxPoints = 100;
    [Export] public Array<int> RankThresholds = new() { 0, 20, 45, 70, 90 }; // D,C,B,A,S
    [Export] public Array<string> RankLetters = new() { "D","C","B","A","S" };

    // Anti-grind & pacing
    [Export] public int DailyCapPerPair = 12;
    [Export] public float CampfireBonus = 1.25f; // applied to bond gains from camp scenes
}