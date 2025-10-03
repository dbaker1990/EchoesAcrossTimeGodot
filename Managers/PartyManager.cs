using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class PartyManager : Node
{
    [ExportGroup("Party Settings")]
    [Export] public bool FollowersEnabled { get; set; } = true;
    [Export] public int MaxFollowers { get; set; } = 3;
    [Export] public PlayerCharacter Player { get; set; }
    [Export] public float FollowerSpacing { get; set; } = 32f;
    
    [ExportGroup("Party Formation")]
    [Export] public bool UseFormation { get; set; } = false;
    [Export] public FormationType Formation { get; set; } = FormationType.Line;
    
    public static PartyManager Instance { get; private set; }
    
    public enum FormationType
    {
        Line,      // Single file line
        VShape,    // V formation
        Diagonal,  // Diagonal line
        Box        // Box formation (for 4 members)
    }
    
    private List<FollowerCharacter> activeFollowers = new List<FollowerCharacter>();
    private Dictionary<string, FollowerCharacter> partyMembers = new Dictionary<string, FollowerCharacter>();
    
    [Signal]
    public delegate void FollowerAddedEventHandler(string characterName);
    
    [Signal]
    public delegate void FollowerRemovedEventHandler(string characterName);
    
    [Signal]
    public delegate void PartyStateChangedEventHandler(CharacterStateType newState);
    
    public override void _Ready()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }
        
        Instance = this;
        
        if (Player == null)
        {
            GD.PrintErr("PartyManager: Player character not assigned!");
            return;
        }
        
        // Connect to player state changes
        Player.StateChanged += OnPlayerStateChanged;
    }
    
    public void AddFollower(FollowerCharacter follower)
    {
        if (follower == null)
        {
            GD.PrintErr("PartyManager: Attempted to add null follower");
            return;
        }
        
        if (activeFollowers.Count >= MaxFollowers)
        {
            GD.Print($"PartyManager: Maximum followers ({MaxFollowers}) reached");
            return;
        }
        
        if (partyMembers.ContainsKey(follower.CharacterName))
        {
            GD.Print($"PartyManager: {follower.CharacterName} is already in party roster");
            
            // Reactivate if not already active
            if (!activeFollowers.Contains(follower))
            {
                ActivateFollower(follower);
            }
            return;
        }
        
        // Add to roster
        partyMembers[follower.CharacterName] = follower;
        
        // Activate follower
        ActivateFollower(follower);
        
        EmitSignal(SignalName.FollowerAdded, follower.CharacterName);
        GD.Print($"PartyManager: Added {follower.CharacterName} to party. Total active: {activeFollowers.Count}");
    }
    
    private void ActivateFollower(FollowerCharacter follower)
    {
        if (activeFollowers.Contains(follower)) return;
        
        // Set follow target based on formation
        if (UseFormation)
        {
            SetFormationTarget(follower);
        }
        else
        {
            // Chain following: each follower follows the one before
            if (activeFollowers.Count == 0)
            {
                follower.SetFollowTarget(Player);
            }
            else
            {
                follower.SetFollowTarget(activeFollowers[activeFollowers.Count - 1]);
            }
        }
        
        activeFollowers.Add(follower);
        
        // Apply settings
        follower.Visible = FollowersEnabled;
        follower.FollowDistance = FollowerSpacing;
        
        // Snap to position
        if (UseFormation)
        {
            follower.SnapToTarget();
        }
        else
        {
            follower.SnapBehindTarget(FollowerSpacing);
        }
        
        // Set collision layer to not collide with other party members
        if (follower.CanPassThroughLeader)
        {
            SetupFollowerCollision(follower);
        }
    }
    
    private void SetFormationTarget(FollowerCharacter follower)
    {
        // For formations, all followers target the player
        follower.SetFollowTarget(Player);
        // Formation offsets will be calculated in formation update
    }
    
    private void SetupFollowerCollision(FollowerCharacter follower)
    {
        // Set up collision layers so followers don't bump into each other
        // Layer 1: Environment
        // Layer 2: Player
        // Layer 3: Followers
        // Layer 4: NPCs
        
        follower.SetCollisionLayerValue(3, true);  // Follower is on layer 3
        follower.SetCollisionMaskValue(1, true);   // Collides with environment
        follower.SetCollisionMaskValue(2, false);  // Doesn't collide with player
        follower.SetCollisionMaskValue(3, false);  // Doesn't collide with other followers
        follower.SetCollisionMaskValue(4, true);   // Collides with NPCs
    }
    
    public void RemoveFollower(FollowerCharacter follower)
    {
        if (!activeFollowers.Contains(follower)) return;
        
        int index = activeFollowers.IndexOf(follower);
        activeFollowers.RemoveAt(index);
        
        // Update follow targets for remaining followers
        UpdateFollowChain();
        
        EmitSignal(SignalName.FollowerRemoved, follower.CharacterName);
        GD.Print($"PartyManager: Removed {follower.CharacterName} from active party");
    }
    
    public void RemoveFollowerByName(string characterName)
    {
        if (partyMembers.TryGetValue(characterName, out var follower))
        {
            RemoveFollower(follower);
        }
    }
    
    public void DeactivateFollower(string characterName)
    {
        if (partyMembers.TryGetValue(characterName, out var follower))
        {
            if (activeFollowers.Contains(follower))
            {
                RemoveFollower(follower);
                follower.Visible = false;
            }
        }
    }
    
    public void ActivateFollowerByName(string characterName)
    {
        if (partyMembers.TryGetValue(characterName, out var follower))
        {
            if (!activeFollowers.Contains(follower) && activeFollowers.Count < MaxFollowers)
            {
                ActivateFollower(follower);
            }
        }
    }
    
    public void ClearActiveFollowers()
    {
        foreach (var follower in activeFollowers.ToList())
        {
            follower.Visible = false;
        }
        
        activeFollowers.Clear();
        GD.Print("PartyManager: All active followers cleared");
    }
    
    public void ClearPartyRoster()
    {
        ClearActiveFollowers();
        partyMembers.Clear();
        GD.Print("PartyManager: Party roster cleared");
    }
    
    public void SetFollowersEnabled(bool enabled)
    {
        FollowersEnabled = enabled;
        
        foreach (var follower in activeFollowers)
        {
            follower.Visible = enabled;
        }
        
        GD.Print($"PartyManager: Followers {(enabled ? "enabled" : "disabled")}");
    }
    
    public void LockParty()
    {
        Player?.LockCharacter();
        
        foreach (var follower in activeFollowers)
        {
            follower.LockCharacter();
        }
        
        EmitSignal(SignalName.PartyStateChanged, (int)CharacterStateType.Locked);
    }
    
    public void UnlockParty()
    {
        Player?.UnlockCharacter();
        
        foreach (var follower in activeFollowers)
        {
            follower.UnlockCharacter();
        }
        
        EmitSignal(SignalName.PartyStateChanged, (int)CharacterStateType.Idle);
    }
    
    public void SetPartyState(CharacterStateType state)
    {
        Player?.ChangeState(state);
        
        foreach (var follower in activeFollowers)
        {
            follower.ChangeState(state);
        }
        
        EmitSignal(SignalName.PartyStateChanged, (int)state);
    }
    
    public void TeleportParty(Vector2 position, OverworldCharacter.Direction? direction = null)
    {
        if (Player != null)
        {
            Player.TeleportTo(position, direction);
        }
        
        if (UseFormation)
        {
            TeleportFormation(position, direction);
        }
        else
        {
            // Chain teleport with spacing
            foreach (var follower in activeFollowers)
            {
                follower.SnapToTarget();
            }
        }
    }
    
    private void TeleportFormation(Vector2 centerPosition, OverworldCharacter.Direction? direction = null)
    {
        for (int i = 0; i < activeFollowers.Count; i++)
        {
            Vector2 offset = CalculateFormationOffset(i, activeFollowers.Count);
            Vector2 finalPosition = centerPosition + offset;
            
            activeFollowers[i].TeleportToPosition(finalPosition, direction);
        }
    }
    
    private Vector2 CalculateFormationOffset(int index, int totalFollowers)
    {
        Vector2 playerDirection = Player.GetDirectionVector();
        Vector2 perpendicular = new Vector2(-playerDirection.Y, playerDirection.X);
        
        switch (Formation)
        {
            case FormationType.Line:
                return -playerDirection * FollowerSpacing * (index + 1);
            
            case FormationType.VShape:
                {
                    float side = (index % 2 == 0) ? 1f : -1f;
                    float row = Mathf.Floor(index / 2f) + 1;
                    return -playerDirection * FollowerSpacing * row + 
                           perpendicular * FollowerSpacing * side * row * 0.5f;
                }
            
            case FormationType.Diagonal:
                return -playerDirection * FollowerSpacing * (index + 1) + 
                       perpendicular * FollowerSpacing * (index + 1) * 0.5f;
            
            case FormationType.Box:
                {
                    // 2x2 formation
                    int row = index / 2;
                    int col = index % 2;
                    float xOffset = (col - 0.5f) * FollowerSpacing;
                    float yOffset = -FollowerSpacing * (row + 1);
                    return new Vector2(xOffset, yOffset);
                }
            
            default:
                return -playerDirection * FollowerSpacing * (index + 1);
        }
    }
    
    public void SetFormation(FormationType formationType)
    {
        Formation = formationType;
        UseFormation = formationType != FormationType.Line;
        
        UpdateFollowChain();
    }
    
    private void UpdateFollowChain()
    {
        if (UseFormation)
        {
            // All followers target player in formation
            foreach (var follower in activeFollowers)
            {
                follower.SetFollowTarget(Player);
            }
        }
        else
        {
            // Chain following
            for (int i = 0; i < activeFollowers.Count; i++)
            {
                if (i == 0)
                {
                    activeFollowers[i].SetFollowTarget(Player);
                }
                else
                {
                    activeFollowers[i].SetFollowTarget(activeFollowers[i - 1]);
                }
            }
        }
    }
    
    private void OnPlayerStateChanged(CharacterStateType newState)
    {
        // Handle special party-wide state changes
        switch (newState)
        {
            case CharacterStateType.Locked:
                // Auto-lock all followers
                foreach (var follower in activeFollowers)
                {
                    follower.LockCharacter();
                }
                break;
            
            case CharacterStateType.Idle:
                // If player unlocks from locked state, unlock followers too
                foreach (var follower in activeFollowers)
                {
                    if (follower.CurrentStateType == CharacterStateType.Locked)
                    {
                        follower.UnlockCharacter();
                    }
                }
                break;
        }
    }
    
    // Query methods
    public List<string> GetPartyMemberNames()
    {
        return new List<string>(partyMembers.Keys);
    }
    
    public List<string> GetActiveFollowerNames()
    {
        return activeFollowers.Select(f => f.CharacterName).ToList();
    }
    
    public int GetPartySize()
    {
        return activeFollowers.Count + 1; // +1 for player
    }
    
    public int GetTotalRosterSize()
    {
        return partyMembers.Count + 1; // +1 for player
    }
    
    public FollowerCharacter GetFollower(int index)
    {
        if (index >= 0 && index < activeFollowers.Count)
        {
            return activeFollowers[index];
        }
        return null;
    }
    
    public FollowerCharacter GetFollowerByName(string characterName)
    {
        if (partyMembers.TryGetValue(characterName, out var follower))
        {
            return follower;
        }
        return null;
    }
    
    public bool IsFollowerActive(string characterName)
    {
        if (partyMembers.TryGetValue(characterName, out var follower))
        {
            return activeFollowers.Contains(follower);
        }
        return false;
    }
    
    public bool IsFollowerInRoster(string characterName)
    {
        return partyMembers.ContainsKey(characterName);
    }
    
    public void SetFollowerSpacing(float spacing)
    {
        FollowerSpacing = spacing;
        
        foreach (var follower in activeFollowers)
        {
            follower.FollowDistance = spacing;
        }
    }
    
    public override void _ExitTree()
    {
        if (Player != null)
        {
            Player.StateChanged -= OnPlayerStateChanged;
        }
        
        base._ExitTree();
    }
}