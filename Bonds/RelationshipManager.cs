// Scripts/System/Relationship/RelationshipManager.cs
using Godot;
using Godot.Collections;
using System.Linq;
using System.Threading.Tasks;
using EchoesAcrossTime;
using EchoesAcrossTime.Bonds;

public partial class RelationshipManager : Node
{
    [Export] public RelationshipConfig Config;

    // Only these three candidates are “romanceable”
    [Export] public Array<string> CandidateIds = new() { "elara", "seraphine", "naledi" };

    private readonly Dictionary<string, RelationshipState> _map = new();
    
    public static RelationshipManager Instance { get; private set; }
    
    [Export] public NodePath SpouseSelectionUIPath { get; set; }
    
    private SpouseSelectionUI spouseSelectionUI;
    private string chosenSpouseId = null;

    [Signal] public delegate void RelationshipChangedEventHandler(string candidateId, int newPoints, string newStage, bool lockedIn);
    [Signal] public delegate void RelationshipStageUpEventHandler(string candidateId, string newStage);
    [Signal] public delegate void RouteLockedInEventHandler(string candidateId);

    public override void _Ready()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }
        Instance = this;
        
        // Initialize candidates
        foreach (var id in CandidateIds)
        {
            if (!_map.ContainsKey(id))
            {
                _map[id] = new RelationshipState 
                { 
                    CandidateId = id, 
                    Points = 0, 
                    Stage = "Acquaintance", 
                    LockedIn = false 
                };
            }
        }
        
        // Get SpouseSelectionUI reference
        if (SpouseSelectionUIPath != null && !SpouseSelectionUIPath.IsEmpty)
        {
            spouseSelectionUI = GetNode<SpouseSelectionUI>(SpouseSelectionUIPath);
        }
        else
        {
            // Try to find it in the scene tree
            spouseSelectionUI = GetTree().Root.GetNodeOrNull<SpouseSelectionUI>("%SpouseSelectionUI");
        }
        
        if (spouseSelectionUI != null)
        {
            spouseSelectionUI.SpouseChosen += OnSpouseChosenFromUI;
        }
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

    /// <summary>
    /// Resolve which candidate becomes the spouse.
    /// Shows choice UI if multiple candidates are tied.
    /// </summary>
    public async Task<string> ResolveSpouseId()
    {
        var top = _map.Values.OrderByDescending(v => v.Points).ToArray();
        if (top.Length == 0) return "";

        var topPts = top[0].Points;
        var secondPts = (top.Length > 1) ? top[1].Points : -999;

        // If a spouse is already locked in, return that
        var locked = _map.Values.FirstOrDefault(v => v.LockedIn);
        if (locked != null)
        {
            return locked.CandidateId;
        }

        // If within SoftChoiceWindow, present choice UI
        if (Mathf.Abs(topPts - secondPts) <= Config.SoftChoiceWindow)
        {
            // Get all candidates within the choice window
            var tiedCandidates = _map.Values
                .Where(v => Mathf.Abs(v.Points - topPts) <= Config.SoftChoiceWindow)
                .OrderByDescending(v => v.Points)
                .ToList();
            
            if (tiedCandidates.Count > 1)
            {
                // Show choice UI and wait for player decision
                if (spouseSelectionUI != null)
                {
                    chosenSpouseId = null;
                    spouseSelectionUI.ShowSelection(tiedCandidates);
                    
                    // Wait for player to make a choice
                    while (chosenSpouseId == null)
                    {
                        await Task.Delay(100);
                    }
                    
                    return chosenSpouseId;
                }
                else
                {
                    GD.PrintErr("[RelationshipManager] SpouseSelectionUI not found! Defaulting to top candidate.");
                    return top[0].CandidateId;
                }
            }
        }

        // Clear winner - return top candidate
        return top[0].CandidateId;
    }
    
    /// <summary>
    /// Synchronous version for backward compatibility
    /// Returns empty string if choice UI is needed
    /// </summary>
    public string ResolveSpouseIdSync()
    {
        var top = _map.Values.OrderByDescending(v => v.Points).ToArray();
        if (top.Length == 0) return "";

        var topPts = top[0].Points;
        var secondPts = (top.Length > 1) ? top[1].Points : -999;

        var locked = _map.Values.FirstOrDefault(v => v.LockedIn);
        if (locked != null) return locked.CandidateId;

        if (Mathf.Abs(topPts - secondPts) <= Config.SoftChoiceWindow)
        {
            // Needs UI choice - return empty to signal this
            return "";
        }

        return top[0].CandidateId;
    }
    
    /// <summary>
    /// Called when player chooses spouse from UI
    /// </summary>
    private void OnSpouseChosenFromUI(string candidateId)
    {
        chosenSpouseId = candidateId;
        
        // Lock in the choice
        if (_map.TryGetValue(candidateId, out var chosenCandidate))
        {
            chosenCandidate.LockedIn = true;
            SetGameFlag($"spouse_{candidateId}", true);
            SetGameFlag("spouse_chosen", true);
            
            GD.Print($"[RelationshipManager] Spouse locked in: {candidateId}");
        }
    }
    

    // Stub for your story flag system - integrate with EventCommandExecutor
    private void SetGameFlag(string flag, bool val)
    {
        if (EchoesAcrossTime.Events.EventCommandExecutor.Instance != null)
        {
            EchoesAcrossTime.Events.EventCommandExecutor.Instance.SetVariable(flag, val);
            GD.Print($"[RelationshipManager] Set flag: {flag} = {val}");
        }
        else
        {
            GD.PrintErr($"[RelationshipManager] Could not set flag {flag} - EventCommandExecutor not found");
        }
    }
    
    public override void _ExitTree()
    {
        if (spouseSelectionUI != null)
        {
            spouseSelectionUI.SpouseChosen -= OnSpouseChosenFromUI;
        }
    }
    
}
