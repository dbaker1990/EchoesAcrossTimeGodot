# Tales of Series Skit System - Integration Guide

## Overview

This skit system recreates the iconic conversation scenes from the Tales series, featuring:
- Character portraits with emotion variations
- Typewriter dialogue effect
- Voice clip support
- Automatic and manual triggering
- Skip functionality
- Save/load integration

## File Structure

```
Skits/
├── SkitData.cs              # Skit data definitions
├── SkitManager.cs           # Main skit controller
├── SkitUI.cs                # UI display logic
├── SkitTrigger.cs           # Automatic triggering
├── ExampleSkits.cs          # Sample skits
└── SkitUI.tscn              # UI scene file
```

## Setup Instructions

### 1. Add SkitManager to Your Scene

Add SkitManager as an autoload singleton:

**Project Settings > Autoload:**
- Name: `SkitManager`
- Path: `res://Skits/SkitManager.cs`
- Enable: ✓

### 2. Add SkitUI to Your Game

Instance the SkitUI scene in your main game scene:

```gdscript
# In your main scene
var skit_ui = preload("res://Skits/SkitUI.tscn").instantiate()
add_child(skit_ui)
```

Make sure it's marked with unique name: `%SkitUI`

### 3. Create Character Portraits

Create portrait variations for each character:

```
Graphics/Portraits/
├── Dominic_Normal.png
├── Dominic_Happy.png
├── Dominic_Sad.png
├── Dominic_Angry.png
├── Dominic_Surprised.png
├── Dominic_Worried.png
├── Dominic_Embarrassed.png
├── Dominic_Determined.png
└── Dominic_Thoughtful.png
```

Recommended portrait size: 300x400 pixels

### 4. Register Skits

**Option A: From Resources (.tres files)**

```csharp
// In your game initialization
public override void _Ready()
{
    SkitManager.Instance?.LoadSkitsFromFolder("res://Data/Skits");
}
```

**Option B: Programmatically**

```csharp
// Register individual skits
var skit = ExampleSkits.CreateExampleSkit();
SkitManager.Instance?.RegisterSkit(skit);
```

## Creating Skits

### Method 1: Create .tres Resource

1. In Godot Editor: Right-click in FileSystem
2. Create New Resource > SkitData
3. Fill in the properties:
    - Skit ID (unique identifier)
    - Skit Title
    - Participant IDs
    - Lines (add SkitLine resources)
    - Trigger conditions

### Method 2: Create via Code

```csharp
var skit = new SkitData
{
    SkitId = "my_skit_001",
    SkitTitle = "Character Banter",
    TriggerType = SkitTriggerType.Manual,
    RequiresAllParticipantsInParty = true
};

// Add participants
skit.ParticipantIds.Add("dominic");
skit.ParticipantIds.Add("aria");

// Add dialogue lines
skit.Lines.Add(new SkitLine
{
    CharacterId = "dominic",
    Text = "This is amazing!",
    PortraitPath = "res://Graphics/Portraits/Dominic_Happy.png",
    Emotion = SkitEmotion.Happy
});

// Register it
SkitManager.Instance.RegisterSkit(skit);
```

## Triggering Skits

### Manual Trigger (via code)

```csharp
// Play skit by ID
await SkitManager.Instance.PlaySkit("my_skit_001");

// Or play skit data directly
var skit = SkitManager.Instance.GetSkit("my_skit_001");
await SkitManager.Instance.PlaySkit(skit);
```

### Automatic Trigger (in scene)

1. Add a Node2D to your scene
2. Attach `SkitTrigger.cs` script
3. Configure in inspector:
    - Set SkitToTrigger or SkitId
    - Choose TriggerMode
    - Set conditions

```
Example:
PlayerEnters Area → SkitTrigger → Plays Skit
```

### Event-Based Trigger

Integrate with your event system:

```csharp
// In EventCommandExecutor or similar
public async Task TriggerSkit(string skitId)
{
    if (SkitManager.Instance != null)
    {
        await SkitManager.Instance.PlaySkit(skitId);
    }
}
```

## Skit Availability System

Update available skits based on game state:

```csharp
// Call this when game state changes
public void UpdateSkits()
{
    var currentParty = GetCurrentParty(); // Your party system
    var currentChapter = GetCurrentChapter(); // Your progress system
    var clearedFlags = GetClearedFlags(); // Your flag system
    
    SkitManager.Instance?.UpdateAvailableSkits(
        currentParty, 
        currentChapter, 
        clearedFlags
    );
}
```

Get available skits:

```csharp
var availableSkits = SkitManager.Instance?.GetAvailableSkits();
foreach (var skit in availableSkits)
{
    GD.Print($"Available: {skit.SkitTitle}");
}
```

## Skit Notification Icon (Tales Style)

Create a notification icon that appears when skits are available:

```csharp
// Listen for available skits
SkitManager.Instance.SkitAvailable += OnSkitAvailable;

private void OnSkitAvailable(string skitId)
{
    ShowSkitNotification(); // Show "!" icon
}
```

## Input Controls

Default controls:
- **Confirm/Interact**: Advance dialogue or complete text
- **Cancel**: Skip entire skit

Configure in SkitUI.cs:

```csharp
// In _Input method
if (@event.IsActionPressed("ui_accept")) // Advance
if (@event.IsActionPressed("ui_cancel"))  // Skip
```

## Voice Acting Integration

Add voice clips to skit lines:

```csharp
var line = new SkitLine
{
    CharacterId = "dominic",
    Text = "Let's go!",
    PortraitPath = "res://Graphics/Portraits/Dominic_Determined.png",
    VoiceClipPath = "res://Audio/Voice/Dominic/LetsGo.ogg"
};
```

Audio settings in SkitUI:
- Voice clips play through "Voice" audio bus
- Text blips play through "SFX" audio bus

## Customization

### Adjust Text Speed

```csharp
// In SkitUI node
textSpeed = 0.03f; // Faster
textSpeed = 0.05f; // Slower
```

### Enable Auto-Advance

```csharp
// In SkitUI node
autoAdvance = true;
autoAdvanceDelay = 2.0f; // Seconds
```

### Portrait Animation Speed

```csharp
// In SkitUI node
portraitSlideSpeed = 0.3f; // Faster slide-in
```

## Save/Load Integration

Save viewed skits:

```csharp
// When saving game
var skitData = SkitManager.Instance?.SaveData();
saveFile["skits"] = skitData;
```

Load viewed skits:

```csharp
// When loading game
if (saveFile.ContainsKey("skits"))
{
    var skitData = saveFile["skits"] as Dictionary<string, object>;
    SkitManager.Instance?.LoadData(skitData);
}
```

## Example: Main Menu Skit Viewer

Create a skit viewer in your menu:

```csharp
public partial class SkitViewer : Control
{
    private ItemList skitList;
    
    public override void _Ready()
    {
        skitList = GetNode<ItemList>("SkitList");
        PopulateSkitList();
    }
    
    private void PopulateSkitList()
    {
        var viewedSkits = SkitManager.Instance.GetAllViewedSkits();
        foreach (var skitId in viewedSkits)
        {
            var skit = SkitManager.Instance.GetSkit(skitId);
            if (skit != null)
            {
                skitList.AddItem(skit.SkitTitle);
            }
        }
    }
    
    private async void OnSkitSelected(int index)
    {
        var skitId = GetSkitIdFromIndex(index);
        await SkitManager.Instance.PlaySkit(skitId);
    }
}
```

## Performance Tips

1. **Lazy Load Portraits**: Load portraits only when needed
2. **Limit Active Skits**: Don't register all skits at once
3. **Use Object Pooling**: Reuse UI elements
4. **Compress Audio**: Use .ogg for voice clips

## Troubleshooting

**Skit not triggering?**
- Check party composition requirements
- Verify flags are set correctly
- Ensure skit hasn't been viewed (if OnceOnly = true)

**Portraits not showing?**
- Verify portrait paths are correct
- Check texture import settings
- Ensure portraits are in project

**Text speed issues?**
- Adjust textSpeed value in SkitUI
- Check delta time scaling

**No audio?**
- Verify audio bus configuration
- Check audio file paths
- Ensure AudioStreamPlayer nodes exist

## Advanced Features

### Conditional Skit Branches

Create skits that branch based on flags:

```csharp
var skit = new SkitData();
skit.RequiredFlag = "dominic_romance_active";
// This skit only plays if romance flag is set
```

### Multi-Part Skit Chains

Create sequential skits:

```csharp
// Skit 1 sets flag
skit1.OnEndFlag = "skit_chain_part1_complete";

// Skit 2 requires that flag
skit2.RequiredFlag = "skit_chain_part1_complete";
```

### Dynamic Dialogue

Substitute variables in dialogue:

```csharp
var line = new SkitLine
{
    Text = $"We've collected {itemCount} crystals!",
    // Use string interpolation or variable system
};
```

## Integration with Your Systems

### With Dialogue System

Skits can complement your MessageBox system:
- Use MessageBox for story dialogue
- Use Skits for optional character moments

### With Event System

Trigger skits from events:

```csharp
// In EventCommandExecutor
public class TriggerSkitCommand : EventCommand
{
    [Export] public string SkitId { get; set; }
    
    public override async Task Execute(EventCommandExecutor executor)
    {
        await SkitManager.Instance.PlaySkit(SkitId);
    }
}
```

### With Party System

Update available skits when party changes:

```csharp
public void OnPartyChanged()
{
    UpdateAvailableSkits();
}
```

## Complete Example

```csharp
// GameManager.cs
public partial class GameManager : Node
{
    public override void _Ready()
    {
        // Load all skits
        SkitManager.Instance?.LoadSkitsFromFolder("res://Data/Skits");
        
        // Listen for skit events
        SkitManager.Instance.SkitAvailable += OnSkitAvailable;
        SkitManager.Instance.SkitStarted += OnSkitStarted;
        SkitManager.Instance.SkitEnded += OnSkitEnded;
        
        // Initial update
        UpdateAvailableSkits();
    }
    
    private void OnSkitAvailable(string skitId)
    {
        GD.Print($"New skit available: {skitId}");
        ShowSkitNotification();
    }
    
    private void OnSkitStarted(string skitId)
    {
        PauseGame(); // Optional
    }
    
    private void OnSkitEnded(string skitId)
    {
        ResumeGame(); // Optional
    }
}
```

---

## Summary

The Tales of Series Skit System provides:
✅ Character portrait conversations
✅ Emotion-based expressions
✅ Voice acting support
✅ Flexible triggering system
✅ Save/load integration
✅ Skip functionality
✅ Auto and manual modes

Perfect for adding optional character development moments to your JRPG!