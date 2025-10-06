// Scripts/System/Relationship/RelationshipManager.cs
using Godot;
using Godot.Collections;
using System.Linq;
using EchoesAcrossTime;

public partial class RelationshipManager : Node
{
    [Export] public RelationshipConfig Config;

    // Only these three candidates are “romanceable”
    [Export] public Array<string> CandidateIds = new() { "elara", "seraphine", "naledi" };

    private readonly Dictionary<string, RelationshipState> _map = new();

    [Signal] public delegate void RelationshipChangedEventHandler(string candidateId, int newPoints, string newStage, bool lockedIn);
    [Signal] public delegate void RelationshipStageUpEventHandler(string candidateId, string newStage);
    [Signal] public delegate void RouteLockedInEventHandler(string candidateId);

    public override void _Ready()
    {
        foreach (var id in CandidateIds)
            if (!_map.ContainsKey(id))
                _map[id] = new RelationshipState { CandidateId = id, Points = 0, Stage = "Acquaintance", LockedIn = false };
    }
    
    #region Save/Load

    /// <summary>
    /// Get current relationship data for saving
    /// </summary>
    public RelationshipSaveData GetSaveData()
    {
        var saveData = new RelationshipSaveData
        {
            Candidates = new System.Collections.Generic.Dictionary<string, RelationshipCandidateData>(),
            LockinWindowOpen = lockinWindowOpen,
            LockinWindowClosed = lockinWindowClosed
        };
        
        foreach (var kvp in _map)
        {
            var state = kvp.Value;
            saveData.Candidates[kvp.Key] = new RelationshipCandidateData
            {
                CandidateId = state.CandidateId,
                Points = state.Points,
                Stage = state.Stage,
                LockedIn = state.LockedIn
            };
            
            if (state.LockedIn)
            {
                saveData.LockedInCandidateId = state.CandidateId;
            }
        }
        
        return saveData;
    }

    /// <summary>
    /// Load relationship data from save
    /// </summary>
    public void LoadSaveData(RelationshipSaveData saveData)
    {
        if (saveData == null) return;
        
        _map.Clear();
        
        foreach (var kvp in saveData.Candidates)
        {
            var candidateData = kvp.Value;
            _map[kvp.Key] = new RelationshipState
            {
                CandidateId = candidateData.CandidateId,
                Points = candidateData.Points,
                Stage = candidateData.Stage,
                LockedIn = candidateData.LockedIn
            };
        }
        
        lockinWindowOpen = saveData.LockinWindowOpen;
        lockinWindowClosed = saveData.LockinWindowClosed;
        
        GD.Print($"Loaded {_map.Count} relationship candidates from save data");
    }

    #endregion

    // Add these fields to track lock-in window state
    private bool lockinWindowOpen = false;
    private bool lockinWindowClosed = false;

    // Update the OpenLockinWindow and CloseLockinWindow methods:
    public void OpenLockinWindow() 
    { 
        lockinWindowOpen = true;
        SetGameFlag(Config.LockinOpensFlag, true);
    }

    public void CloseLockinWindow() 
    { 
        lockinWindowClosed = true;
        SetGameFlag(Config.LockinClosedFlag, true);
    }

    public RelationshipState Get(string id) => _map[id];

    private string StageFor(int pts)
    {
        var t = Config.StageThresholds;
        if (pts >= t[3]) return "Betrothal";
        if (pts >= t[2]) return "Promise";
        if (pts >= t[1]) return "Confidant";
        return "Acquaintance";
    }

    public void AddPoints(string id, int amount, bool respectLock = true)
    {
        var s = _map[id];
        if (respectLock && s.LockedIn) return;

        var before = s.Stage;
        s.Points = Mathf.Clamp(s.Points + amount, 0, Config.MaxPoints);
        s.Stage = StageFor(s.Points);

        EmitSignal(SignalName.RelationshipChanged, id, s.Points, s.Stage, s.LockedIn);
        if (s.Stage != before) EmitSignal(SignalName.RelationshipStageUp, id, s.Stage);
    }

    // UI calls this when the player commits to a route during the lock window
    public void LockIn(string id)
    {
        foreach (var kv in _map) kv.Value.LockedIn = false;
        _map[id].LockedIn = true;
        EmitSignal(SignalName.RouteLockedIn, id);
    }

    // Endgame resolution: if no explicit lock-in was made, pick top; if within window, show a choice.
    public string ResolveSpouseId()
    {
        var top = _map.Values.OrderByDescending(v => v.Points).ToArray();
        if (top.Length == 0) return "";

        var topPts = top[0].Points;
        var secondPts = (top.Length > 1)? top[1].Points : -999;

        // If within SoftChoiceWindow, surface a choice UI; else take top (or the locked-in one if set)
        var locked = _map.Values.FirstOrDefault(v => v.LockedIn);
        if (locked != null) return locked.CandidateId;

        if (Mathf.Abs(topPts - secondPts) <= Config.SoftChoiceWindow)
        {
            // TODO: Present choice UI between the tied/top few and return that result.
            // For now return top[0] but your endgame scene should branch to a choice prompt.
            return top[0].CandidateId;
        }

        return top[0].CandidateId;
    }
    

    // Stub for your story flag system
    private void SetGameFlag(string flag, bool val) { /* hook into your save/state */ }
}
