// Scripts/System/Bonds/BondState.cs
using Godot;

public partial class BondState : RefCounted
{
    public string A;         // characterId
    public string B;         // characterId
    public int Points;
    public int LastGainDay;  // in-game day count
}