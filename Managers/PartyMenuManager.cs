using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using EchoesAcrossTime.Combat;

/// <summary>
/// Manages party organization with main party (4 characters) and sub party system.
/// Sub party members gain 50% experience from battles.
/// </summary>
public partial class PartyMenuManager : Node
{
    public const int MAX_MAIN_PARTY = 4;
    public const float SUB_PARTY_EXP_MULTIPLIER = 0.5f;
    
    public static PartyMenuManager Instance { get; private set; }
    
    // Party data structures
    private List<PartyMemberData> mainParty = new List<PartyMemberData>();
    private List<PartyMemberData> subParty = new List<PartyMemberData>();
    
    [Signal]
    public delegate void MainPartyChangedEventHandler();
    
    [Signal]
    public delegate void SubPartyChangedEventHandler();
    
    [Signal]
    public delegate void PartyMemberLockedEventHandler(string characterId, bool isLocked);
    
    public override void _Ready()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }
        Instance = this;
    }
    
    /// <summary>
    /// Add a character to the party system. Goes to main party if space, otherwise sub party.
    /// </summary>
    public void AddCharacterToParty(string characterId, CharacterStats stats, bool isLocked = false)
    {
        // Check if already in party
        if (IsInParty(characterId))
        {
            GD.Print($"Character {characterId} is already in the party");
            return;
        }
        
        var memberData = new PartyMemberData
        {
            CharacterId = characterId,
            Stats = stats,
            IsLocked = isLocked
        };
        
        // Add to main party if space, otherwise sub party
        if (mainParty.Count < MAX_MAIN_PARTY)
        {
            mainParty.Add(memberData);
            EmitSignal(SignalName.MainPartyChanged);
            GD.Print($"Added {characterId} to main party");
        }
        else
        {
            subParty.Add(memberData);
            EmitSignal(SignalName.SubPartyChanged);
            GD.Print($"Added {characterId} to sub party (main party full)");
        }
        
        if (isLocked)
        {
            EmitSignal(SignalName.PartyMemberLocked, characterId, true);
        }
    }
    
    /// <summary>
    /// Swap a character from main party to sub party
    /// </summary>
    public bool SwapToSubParty(string characterId)
    {
        var member = mainParty.FirstOrDefault(m => m.CharacterId == characterId);
        
        if (member == null)
        {
            GD.PrintErr($"Character {characterId} not found in main party");
            return false;
        }
        
        if (member.IsLocked)
        {
            GD.Print($"Cannot swap {characterId} - character is locked in main party");
            return false;
        }
        
        mainParty.Remove(member);
        subParty.Add(member);
        
        EmitSignal(SignalName.MainPartyChanged);
        EmitSignal(SignalName.SubPartyChanged);
        
        GD.Print($"Swapped {characterId} to sub party");
        return true;
    }
    
    /// <summary>
    /// Swap a character from sub party to main party
    /// </summary>
    public bool SwapToMainParty(string characterId)
    {
        var member = subParty.FirstOrDefault(m => m.CharacterId == characterId);
        
        if (member == null)
        {
            GD.PrintErr($"Character {characterId} not found in sub party");
            return false;
        }
        
        if (mainParty.Count >= MAX_MAIN_PARTY)
        {
            GD.Print($"Cannot swap {characterId} to main party - party is full");
            return false;
        }
        
        subParty.Remove(member);
        mainParty.Add(member);
        
        EmitSignal(SignalName.MainPartyChanged);
        EmitSignal(SignalName.SubPartyChanged);
        
        GD.Print($"Swapped {characterId} to main party");
        return true;
    }
    
    /// <summary>
    /// Swap two characters between main and sub party
    /// </summary>
    public bool SwapCharacters(string mainPartyCharacterId, string subPartyCharacterId)
    {
        var mainMember = mainParty.FirstOrDefault(m => m.CharacterId == mainPartyCharacterId);
        var subMember = subParty.FirstOrDefault(m => m.CharacterId == subPartyCharacterId);
        
        if (mainMember == null || subMember == null)
        {
            GD.PrintErr("One or both characters not found for swap");
            return false;
        }
        
        if (mainMember.IsLocked)
        {
            GD.Print($"Cannot swap {mainPartyCharacterId} - character is locked in main party");
            return false;
        }
        
        // Perform the swap
        mainParty.Remove(mainMember);
        subParty.Remove(subMember);
        
        mainParty.Add(subMember);
        subParty.Add(mainMember);
        
        EmitSignal(SignalName.MainPartyChanged);
        EmitSignal(SignalName.SubPartyChanged);
        
        GD.Print($"Swapped {mainPartyCharacterId} with {subPartyCharacterId}");
        return true;
    }
    
    /// <summary>
    /// Lock or unlock a character in the main party
    /// </summary>
    public void SetCharacterLocked(string characterId, bool isLocked)
    {
        var member = mainParty.FirstOrDefault(m => m.CharacterId == characterId);
        
        if (member == null)
        {
            GD.PrintErr($"Character {characterId} not found in main party - can only lock main party members");
            return;
        }
        
        member.IsLocked = isLocked;
        EmitSignal(SignalName.PartyMemberLocked, characterId, isLocked);
        
        GD.Print($"{characterId} is now {(isLocked ? "locked" : "unlocked")} in main party");
    }
    
    /// <summary>
    /// Distribute experience to party members after battle
    /// </summary>
    public void DistributeExperience(int baseExp)
    {
        // Main party gets full exp
        foreach (var member in mainParty)
        {
            if (member.Stats.IsAlive)
            {
                int expGained = baseExp;
                member.Stats.AddExp(expGained);
                GD.Print($"{member.CharacterId} (main party) gained {expGained} EXP");
            }
        }
        
        // Sub party gets 50% exp
        int subPartyExp = Mathf.RoundToInt(baseExp * SUB_PARTY_EXP_MULTIPLIER);
        foreach (var member in subParty)
        {
            if (member.Stats.IsAlive)
            {
                member.Stats.AddExp(subPartyExp);
                GD.Print($"{member.CharacterId} (sub party) gained {subPartyExp} EXP (50%)");
            }
        }
    }
    
    /// <summary>
    /// Get all main party members
    /// </summary>
    public List<PartyMemberData> GetMainParty()
    {
        return new List<PartyMemberData>(mainParty);
    }
    
    /// <summary>
    /// Get all sub party members
    /// </summary>
    public List<PartyMemberData> GetSubParty()
    {
        return new List<PartyMemberData>(subParty);
    }
    
    /// <summary>
    /// Get main party as CharacterStats list (for battle system)
    /// </summary>
    public List<CharacterStats> GetMainPartyStats()
    {
        return mainParty.Select(m => m.Stats).ToList();
    }
    
    /// <summary>
    /// Check if character is in any party
    /// </summary>
    public bool IsInParty(string characterId)
    {
        return mainParty.Any(m => m.CharacterId == characterId) || 
               subParty.Any(m => m.CharacterId == characterId);
    }
    
    /// <summary>
    /// Check if character is in main party
    /// </summary>
    public bool IsInMainParty(string characterId)
    {
        return mainParty.Any(m => m.CharacterId == characterId);
    }
    
    /// <summary>
    /// Check if character is locked
    /// </summary>
    public bool IsCharacterLocked(string characterId)
    {
        var member = mainParty.FirstOrDefault(m => m.CharacterId == characterId);
        return member?.IsLocked ?? false;
    }
    
    /// <summary>
    /// Get total party size (main + sub)
    /// </summary>
    public int GetTotalPartySize()
    {
        return mainParty.Count + subParty.Count;
    }
    
    /// <summary>
    /// Remove a character from the party system entirely
    /// </summary>
    public void RemoveCharacter(string characterId)
    {
        var mainMember = mainParty.FirstOrDefault(m => m.CharacterId == characterId);
        if (mainMember != null)
        {
            if (mainMember.IsLocked)
            {
                GD.Print($"Cannot remove {characterId} - character is locked");
                return;
            }
            
            mainParty.Remove(mainMember);
            EmitSignal(SignalName.MainPartyChanged);
            GD.Print($"Removed {characterId} from main party");
            return;
        }
        
        var subMember = subParty.FirstOrDefault(m => m.CharacterId == characterId);
        if (subMember != null)
        {
            subParty.Remove(subMember);
            EmitSignal(SignalName.SubPartyChanged);
            GD.Print($"Removed {characterId} from sub party");
        }
    }
}

/// <summary>
/// Data container for party members
/// </summary>
public class PartyMemberData
{
    public string CharacterId { get; set; }
    public CharacterStats Stats { get; set; }
    public bool IsLocked { get; set; }
}