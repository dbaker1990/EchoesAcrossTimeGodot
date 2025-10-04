# Tales of Series Skit System

A complete recreation of the iconic skit conversation system from the Tales series for your Godot 4 JRPG!

## 🎭 What Are Skits?

Skits are optional character conversation scenes that feature:
- 2D character portrait displays
- Typewriter dialogue effect
- Voice acting support
- Emotion-based portrait variations
- Skippable content
- Automatic and manual triggering

## 📦 Files Included

### Core System Files
1. **SkitData.cs** - Skit data structure and definitions
2. **SkitManager.cs** - Main skit controller (singleton)
3. **SkitUI.cs** - UI display and animations
4. **SkitUI.tscn** - Godot scene for skit display

### Helper Files
5. **SkitTrigger.cs** - Automatic skit triggering component
6. **SkitNotification.cs** - "!" notification icon (Tales style)
7. **ExampleSkits.cs** - Sample skit implementations
8. **SkitTestScene.cs** - Testing and demo scene

### Documentation
9. **SkitSystemGuide.md** - Complete integration guide
10. **README.md** - This file

## 🚀 Quick Setup

### Step 1: Add to Project

Copy all files to your project:
```
YourProject/
├── Skits/
│   ├── SkitData.cs
│   ├── SkitManager.cs
│   ├── SkitUI.cs
│   ├── SkitUI.tscn
│   ├── SkitTrigger.cs
│   ├── SkitNotification.cs
│   ├── ExampleSkits.cs
│   └── SkitTestScene.cs
```

### Step 2: Setup Autoload

Add SkitManager as autoload singleton:

**Project Settings → Autoload:**
- Path: `res://Skits/SkitManager.cs`
- Name: `SkitManager`
- Enable: ✓

### Step 3: Add UI to Scene

Instance SkitUI.tscn in your main game scene:
```gdscript
var skit_ui = preload("res://Skits/SkitUI.tscn").instantiate()
add_child(skit_ui)
```

Mark it with unique name: `%SkitUI`

### Step 4: Create Portraits

Create character portraits:
```
Graphics/Portraits/
├── Dominic_Normal.png
├── Dominic_Happy.png
├── Dominic_Sad.png
├── Dominic_Angry.png
├── Dominic_Surprised.png
└── ... (other emotions)
```

Recommended size: 300x400 pixels

### Step 5: Test!

```csharp
// In your code
public override void _Ready()
{
    // Register a test skit
    var skit = ExampleSkits.CreateExampleSkit();
    SkitManager.Instance?.RegisterSkit(skit);
    
    // Play it!
    await SkitManager.Instance?.PlaySkit("skit_example_001");
}
```

## ✨ Features

### Character Portraits
- Left/right positioning
- Smooth slide-in animations
- Speaker highlighting
- Emotion-based expressions
- Automatic character swapping

### Dialogue Display
- Typewriter text effect
- Adjustable text speed
- Voice clip playback
- Text blip sounds
- Auto-advance option
- Manual advance with confirm

### Triggering System
- **Manual**: Play via code
- **Automatic**: Area-based triggers
- **Location**: Specific map triggers
- **Story Event**: After events
- **After Battle**: Post-combat
- **Item Obtained**: When getting items
- **Level Up**: On character level up

### Conditions
- Party composition requirements
- Game flag requirements
- Chapter/progress requirements
- Once-only viewing
- Custom conditions

### Notification System
- "!" icon when skits available
- Bobbing animation
- Pulse effect
- Count display
- Quick-access menu

## 🎮 Usage Examples

### Create a Simple Skit

```csharp
var skit = new SkitData
{
    SkitId = "my_first_skit",
    SkitTitle = "Meeting Aria",
    TriggerType = SkitTriggerType.Manual
};

skit.ParticipantIds.Add("dominic");
skit.ParticipantIds.Add("aria");

skit.Lines.Add(new SkitLine
{
    CharacterId = "dominic",
    Text = "So you're the Ice Princess everyone talks about.",
    PortraitPath = "res://Graphics/Portraits/Dominic_Normal.png",
    Emotion = SkitEmotion.Normal
});

skit.Lines.Add(new SkitLine
{
    CharacterId = "aria",
    Text = "And you must be the infamous Shadow Prince.",
    PortraitPath = "res://Graphics/Portraits/Aria_Normal.png",
    Emotion = SkitEmotion.Normal
});

SkitManager.Instance.RegisterSkit(skit);
```

### Trigger a Skit

```csharp
// Play by ID
await SkitManager.Instance.PlaySkit("my_first_skit");

// Play skit data directly
await SkitManager.Instance.PlaySkit(skitData);
```

### Automatic Trigger in Scene

1. Add Node2D to scene
2. Attach SkitTrigger script
3. Set skit to trigger
4. Configure trigger mode and conditions

### Check Available Skits

```csharp
var available = SkitManager.Instance.GetAvailableSkits();
foreach (var skit in available)
{
    GD.Print($"Available: {skit.SkitTitle}");
}
```

## 🎨 Customization

### Text Speed
```csharp
// In SkitUI inspector or code
textSpeed = 0.03f; // Faster
textSpeed = 0.05f; // Slower
```

### Auto-Advance
```csharp
autoAdvance = true;
autoAdvanceDelay = 2.0f; // seconds
```

### Portrait Animations
```csharp
portraitSlideSpeed = 0.3f; // slide-in speed
```

### Colors
```csharp
iconColor = new Color(1, 1, 0); // notification icon color
```

## 🎯 Integration Points

### With Your Dialogue System
- Skits complement MessageBox for optional conversations
- Use MessageBox for story, Skits for character moments

### With Event System
```csharp
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
```csharp
public void OnPartyChanged()
{
    UpdateAvailableSkits();
}
```

### Save/Load
```csharp
// Save
var skitData = SkitManager.Instance.SaveData();
saveFile["skits"] = skitData;

// Load
SkitManager.Instance.LoadData(saveFile["skits"]);
```

## 📱 Controls

**During Skit:**
- **Confirm/Enter**: Advance dialogue or complete text
- **Cancel/Esc**: Skip entire skit

**Customizable via input actions:**
- `ui_accept` - Advance
- `ui_cancel` - Skip
- `interact` - Alternative advance

## 🐛 Troubleshooting

**Skits not appearing?**
- Verify SkitManager is autoloaded
- Check SkitUI is in scene tree
- Ensure skit is registered
- Verify trigger conditions

**Portraits not showing?**
- Check portrait file paths
- Verify textures imported correctly
- Ensure portraits are in project

**No audio?**
- Check AudioBus configuration
- Verify audio file paths
- Ensure AudioStreamPlayer nodes exist

## 📊 Performance Tips

1. **Lazy load portraits** - Load only when needed
2. **Limit registered skits** - Don't load all at once
3. **Use .ogg audio** - Compressed voice clips
4. **Object pooling** - Reuse UI elements

## 🎓 Advanced Features

### Skit Chains
```csharp
// Skit 1 unlocks Skit 2
skit1.OnEndFlag = "chain_part1_done";
skit2.RequiredFlag = "chain_part1_done";
```

### Dynamic Text
```csharp
var line = new SkitLine
{
    Text = $"We have {itemCount} crystals!",
    // Use string interpolation
};
```

### Conditional Branches
```csharp
skit.RequiredFlag = "romance_path_active";
// Only plays if flag is set
```

## 📝 Example Workflow

1. **Design your skit** - Write dialogue and choose emotions
2. **Create portraits** - Make expression variations
3. **Create skit data** - Either .tres or code
4. **Register skit** - Add to SkitManager
5. **Set trigger** - Automatic or manual
6. **Test** - Use SkitTestScene
7. **Integrate** - Add to your game flow

## 🏆 What's Included

✅ Complete skit system matching Tales series
✅ Portrait animations and effects
✅ Voice acting support
✅ Flexible trigger system
✅ Notification icon
✅ Save/load integration
✅ Skip functionality
✅ Example skits
✅ Test scene
✅ Full documentation

## 🎉 Ready to Use!

Your Tales-style skit system is complete! Just:
1. Add portraits
2. Create your skits
3. Integrate with your game systems
4. Enjoy character development moments!

---

**Need help?** Check SkitSystemGuide.md for complete integration instructions!

**Want to test?** Use SkitTestScene.cs to see everything in action!

**Happy game development!** 🎮✨