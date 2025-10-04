# Party Menu System - API Reference

## Core Constants

```csharp
PartyMenuManager.MAX_MAIN_PARTY = 4
PartyMenuManager.SUB_PARTY_EXP_MULTIPLIER = 0.5f
```

## Main Methods

### Adding Characters

```csharp
// Add character to party (goes to main if space, else sub)
PartyMenuManager.Instance.AddCharacterToParty(
    string characterId, 
    CharacterStats stats, 
    bool isLocked = false
)

// Example:
var stats = CharacterPresets.CreateDominic(1).CreateStatsInstance();
PartyMenuManager.Instance.AddCharacterToParty("dominic", stats, isLocked: true);
```

### Swapping Characters

```csharp
// Move from main to sub party
bool SwapToSubParty(string characterId)

// Move from sub to main party
bool SwapToMainParty(string characterId)

// Swap two characters between parties
bool SwapCharacters(string mainPartyCharacterId, string subPartyCharacterId)

// Examples:
PartyMenuManager.Instance.SwapToSubParty("aria");
PartyMenuManager.Instance.SwapToMainParty("echo");
PartyMenuManager.Instance.SwapCharacters("aria", "echo");
```

### Locking Characters

```csharp
// Lock/unlock character in main party
void SetCharacterLocked(string characterId, bool isLocked)

// Check if locked
bool IsCharacterLocked(string characterId)

// Examples:
PartyMenuManager.Instance.SetCharacterLocked("dominic", true);
bool locked = PartyMenuManager.Instance.IsCharacterLocked("dominic");
```

### Experience Distribution

```csharp
// Distribute exp after battle (main party: 100%, sub party: 50%)
void DistributeExperience(int baseExp)

// Example:
PartyMenuManager.Instance.DistributeExperience(1000);
// Main party members get 1000 EXP
// Sub party members get 500 EXP
```

### Querying Party Data

```csharp
// Get party lists
List<PartyMemberData> GetMainParty()
List<PartyMemberData> GetSubParty()
List<CharacterStats> GetMainPartyStats() // For battle system

// Check membership
bool IsInParty(string characterId)
bool IsInMainParty(string characterId)

// Get counts
int GetTotalPartySize()

// Examples:
var mainParty = PartyMenuManager.Instance.GetMainParty();
bool inBattle = PartyMenuManager.Instance.IsInMainParty("aria");
int totalMembers = PartyMenuManager.Instance.GetTotalPartySize();
```

### Removing Characters

```csharp
// Remove character from party entirely
void RemoveCharacter(string characterId)

// Note: Cannot remove locked characters
PartyMenuManager.Instance.RemoveCharacter("aria");
```

## Signals

### PartyMenuManager Signals

```csharp
// Emitted when main party changes
[Signal] MainPartyChanged()

// Emitted when sub party changes
[Signal] SubPartyChanged()

// Emitted when character is locked/unlocked
[Signal] PartyMemberLocked(string characterId, bool isLocked)

// Connecting to signals:
PartyMenuManager.Instance.MainPartyChanged += OnMainPartyChanged;
PartyMenuManager.Instance.SubPartyChanged += OnSubPartyChanged;
PartyMenuManager.Instance.PartyMemberLocked += OnCharacterLocked;
```

### PartyMemberPanel Signals

```csharp
// Emitted when member is selected
[Signal] MemberSelected(string characterId, bool isMainParty)

// Emitted when swap button pressed
[Signal] SwapRequested(string characterId, bool isMainParty)
```

## Data Structures

### PartyMemberData

```csharp
public class PartyMemberData
{
    public string CharacterId { get; set; }
    public CharacterStats Stats { get; set; }
    public bool IsLocked { get; set; }
}
```

## UI Methods

### PartyMenuUI

```csharp
// Open the party menu
void OpenMenu()

// Refresh display
void RefreshPartyDisplay()

// Example:
var menu = GetNode<PartyMenuUI>("%PartyMenuUI");
menu.OpenMenu();
```

### PartyMemberPanel

```csharp
// Initialize panel with member data
void Initialize(PartyMemberData member, bool isMainParty)

// Update visual state
void SetSelected(bool selected)
void SetLocked(bool locked)
```

## Common Usage Patterns

### 1. Initialize Party at Game Start

```csharp
public override void _Ready()
{
    var dominic = CharacterPresets.CreateDominic(1).CreateStatsInstance();
    var aria = CharacterPresets.CreateAria(1).CreateStatsInstance();
    
    PartyMenuManager.Instance.AddCharacterToParty("dominic", dominic, true);
    PartyMenuManager.Instance.AddCharacterToParty("aria", aria);
}
```

### 2. Start Battle with Main Party

```csharp
public void StartBattle()
{
    var battleParty = PartyMenuManager.Instance.GetMainPartyStats();
    battleManager.InitializeBattle(battleParty, enemies, showtimes);
}
```

### 3. Award Experience After Battle

```csharp
private void OnBattleWon()
{
    int expReward = 1000;
    PartyMenuManager.Instance.DistributeExperience(expReward);
}
```

### 4. Story Event Locking

```csharp
// Before story event
PartyMenuManager.Instance.SetCharacterLocked("dominic", true);

// After story event
PartyMenuManager.Instance.SetCharacterLocked("dominic", false);
```

### 5. Character Recruitment

```csharp
public void RecruitCharacter(string characterId)
{
    var charData = GameDatabase.Instance.GetCharacter(characterId);
    var stats = charData.CreateStatsInstance();
    
    PartyMenuManager.Instance.AddCharacterToParty(characterId, stats);
    
    // Show message
    ShowMessage($"{charData.DisplayName} joined the party!");
}
```

### 6. React to Party Changes

```csharp
public override void _Ready()
{
    PartyMenuManager.Instance.MainPartyChanged += UpdateBattleUI;
    PartyMenuManager.Instance.MainPartyChanged += UpdateFollowerSprites;
}

private void UpdateBattleUI()
{
    var party = PartyMenuManager.Instance.GetMainParty();
    // Update UI to show current party
}
```

## Error Handling

### Common Errors and Solutions

**"Character is locked"**
- Check: `IsCharacterLocked()` before swapping
- Solution: Unlock character first or don't allow swap

**"Main party full"**
- Check: Main party count < MAX_MAIN_PARTY
- Solution: Character goes to sub party automatically

**"Character not found"**
- Check: `IsInParty()` before operations
- Solution: Add character first

**"Cannot swap locked character"**
- Check: `IsCharacterLocked()` before swap
- Solution: Disable swap UI for locked characters

## Best Practices

1. **Always lock story-critical characters** during important moments
2. **Check IsInMainParty()** before story events requiring specific characters
3. **Use GetMainPartyStats()** when initializing battles
4. **Connect to signals** to keep UI in sync with party changes
5. **Refresh UI** when returning from battles or cutscenes
6. **Save party configuration** with your save system
7. **Handle edge cases** like trying to swap when party is full

## Performance Notes

- Party operations are O(n) where n is party size (max ~10 characters)
- UI refreshes only when party changes (signal-driven)
- No polling or constant updates
- Minimal memory footprint

## Integration Checklist

- [ ] Add PartyMenuManager to Autoload
- [ ] Create PartyMenuUI scene with all nodes
- [ ] Connect UI node references in inspector
- [ ] Initialize party in game start
- [ ] Integrate with battle system
- [ ] Add exp distribution after battles
- [ ] Add party menu button to pause/main menu
- [ ] Test locking/unlocking
- [ ] Test all swap scenarios
- [ ] Test experience distribution
- [ ] Save/load party configuration