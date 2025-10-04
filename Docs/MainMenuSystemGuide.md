# 📋 Main Menu System - Complete Implementation Guide

## 🎉 What You've Got

A **complete main menu (pause menu) system** that integrates all your existing game systems into one cohesive interface!

---

## 📦 Files Created

### Core Menu Files (6 files)
1. **MainMenuUI.cs** - Main hub menu that opens with ESC/Start
2. **ItemMenuUI.cs** - Browse and use items
3. **SkillMenuUI.cs** - View and equip skills
4. **EquipMenuUI.cs** - Manage character equipment
5. **StatusMenuUI.cs** - View character stats and info
6. **OptionsMenuUI.cs** - Game settings (audio, display, gameplay)

### Integration
- Works with your existing **Bestiary**, **Crafting**, **Party Menu**, **Quest** systems
- Connects to **Save/Load** system
- Uses **InventorySystem**, **EquipmentManager**, **PartyManager**

---

## 🚀 Quick Setup

### Step 1: Create Menu Scenes

Create these scene files in `res://UI/`:

**MainMenuUI.tscn** - Main menu structure:
```
MainMenuUI (Control) [Script: MainMenuUI.cs]
├── CanvasLayer
│   └── ColorRect (semi-transparent black background)
│       └── CenterContainer
│           └── PanelContainer (menuPanel)
│               └── MarginContainer
│                   └── VBoxContainer
│                       ├── Header (HBoxContainer)
│                       │   ├── LocationLabel (Label)
│                       │   ├── Spacer
│                       │   ├── PlaytimeLabel (Label)
│                       │   └── GoldLabel (Label)
│                       ├── HSeparator
│                       └── ButtonContainer (VBoxContainer)
│                           ├── ItemButton (Button) - "Items"
│                           ├── SkillButton (Button) - "Skills"
│                           ├── EquipButton (Button) - "Equipment"
│                           ├── StatusButton (Button) - "Status"
│                           ├── CraftingButton (Button) - "Crafting"
│                           ├── PartyButton (Button) - "Party"
│                           ├── BestiaryButton (Button) - "Bestiary"
│                           ├── QuestButton (Button) - "Quests"
│                           ├── OptionsButton (Button) - "Options"
│                           ├── SaveButton (Button) - "Save"
│                           ├── LoadButton (Button) - "Load"
│                           └── EndGameButton (Button) - "End Game"
```

**ItemMenuUI.tscn** - Item menu structure:
```
ItemMenuUI (Control) [Script: ItemMenuUI.cs]
├── Panel
│   └── VBoxContainer
│       ├── TopBar (HBoxContainer)
│       │   ├── CategoryFilter (OptionButton)
│       │   ├── GoldLabel (Label)
│       │   └── CloseButton (Button)
│       ├── MainContent (HBoxContainer)
│       │   ├── ItemList (ItemList) - Shows items
│       │   └── DetailPanel (Panel)
│       │       ├── ItemNameLabel (Label)
│       │       ├── ItemDescriptionLabel (Label)
│       │       └── ItemQuantityLabel (Label)
│       └── ButtonBar (HBoxContainer)
│           ├── UseButton (Button)
│           ├── DiscardButton (Button)
│           └── SortButton (Button)
├── CharacterSelectionPanel (Panel)
│   └── CharacterList (VBoxContainer)
```

**SkillMenuUI.tscn** - Skill menu structure:
```
SkillMenuUI (Control) [Script: SkillMenuUI.cs]
├── Panel
│   └── HBoxContainer
│       ├── CharacterList (ItemList)
│       └── VBoxContainer
│           ├── SkillTabs (TabContainer)
│           │   ├── EquippedSkillList (ItemList)
│           │   └── AvailableSkillList (ItemList)
│           ├── DetailPanel (Panel)
│           │   ├── SkillNameLabel
│           │   ├── SkillDescriptionLabel
│           │   ├── SkillMPCostLabel
│           │   ├── SkillPowerLabel
│           │   ├── SkillElementLabel
│           │   └── SkillTargetLabel
│           └── ButtonBar (HBoxContainer)
│               ├── EquipButton (Button)
│               ├── UnequipButton (Button)
│               └── CloseButton (Button)
```

**EquipMenuUI.tscn** - Equipment menu structure:
```
EquipMenuUI (Control) [Script: EquipMenuUI.cs]
├── Panel
│   └── HBoxContainer
│       ├── CharacterList (ItemList)
│       └── VBoxContainer
│           ├── CharacterInfo (VBoxContainer)
│           │   ├── CharacterNameLabel
│           │   ├── LevelLabel
│           │   └── CharacterPortrait (TextureRect)
│           ├── EquipmentSlots (VBoxContainer)
│           │   ├── WeaponSlot (Button)
│           │   ├── ArmorSlot (Button)
│           │   ├── Accessory1Slot (Button)
│           │   └── Accessory2Slot (Button)
│           ├── StatsPanel (Panel)
│           │   └── Stats labels (HP, MP, ATK, etc.)
│           └── CloseButton (Button)
├── EquipmentSelectionPanel (Panel)
│   └── VBoxContainer
│       ├── EquipmentList (ItemList)
│       ├── DetailPanel (Panel)
│       │   ├── EquipmentNameLabel
│       │   ├── EquipmentDescriptionLabel
│       │   └── EquipmentStatsLabel
│       └── ButtonBar (HBoxContainer)
│           ├── EquipConfirmButton (Button)
│           └── EquipCancelButton (Button)
```

**StatusMenuUI.tscn** - Status menu structure:
```
StatusMenuUI (Control) [Script: StatusMenuUI.cs]
├── Panel
│   └── HBoxContainer
│       ├── CharacterList (ItemList)
│       └── VBoxContainer
│           ├── CharacterInfo (HBoxContainer)
│           │   ├── CharacterPortrait (TextureRect)
│           │   └── BasicInfo (VBoxContainer)
│           │       ├── CharacterNameLabel
│           │       ├── LevelLabel
│           │       └── ClassLabel
│           ├── ExpPanel (Panel)
│           │   ├── CurrentExpLabel
│           │   ├── NextLevelExpLabel
│           │   └── ExpProgressBar (ProgressBar)
│           ├── StatsPanel (Panel)
│           │   └── All stat labels
│           ├── ElementAffinitiesContainer (VBoxContainer)
│           ├── EquipmentBonusesLabel (Label)
│           ├── StatusEffectsLabel (Label)
│           └── CloseButton (Button)
```

**OptionsMenuUI.tscn** - Options menu structure:
```
OptionsMenuUI (Control) [Script: OptionsMenuUI.cs]
├── Panel
│   └── VBoxContainer
│       ├── TabContainer
│       │   ├── Audio (VBoxContainer)
│       │   │   ├── BGMVolumeSlider (HSlider) + Label
│       │   │   ├── SFXVolumeSlider (HSlider) + Label
│       │   │   └── VoiceVolumeSlider (HSlider) + Label
│       │   ├── Display (VBoxContainer)
│       │   │   ├── FullscreenCheckbox (CheckButton)
│       │   │   ├── WindowSizeDropdown (OptionButton)
│       │   │   └── VSyncCheckbox (CheckButton)
│       │   ├── Gameplay (VBoxContainer)
│       │   │   ├── TextSpeedSlider (HSlider) + Label
│       │   │   ├── AutoSaveCheckbox (CheckButton)
│       │   │   ├── ShowTutorialsCheckbox (CheckButton)
│       │   │   └── BattleAnimationsCheckbox (CheckButton)
│       │   └── Controls (VBoxContainer)
│       │       └── ControlsDisplay (RichTextLabel)
│       └── ButtonBar (HBoxContainer)
│           ├── ApplyButton (Button)
│           ├── DefaultsButton (Button)
│           └── CloseButton (Button)
```

### Step 2: Add to Main Game Scene

Add all menu UIs to your main game scene:

```gdscript
# In your main game scene (e.g., GameWorld.tscn)

Node2D (Main Game)
├── ... your existing game nodes ...
├── UI (CanvasLayer)
│   ├── MainMenuUI [unique name: %MainMenuUI]
│   ├── ItemMenuUI [unique name: %ItemMenuUI]
│   ├── SkillMenuUI [unique name: %SkillMenuUI]
│   ├── EquipMenuUI [unique name: %EquipMenuUI]
│   ├── StatusMenuUI [unique name: %StatusMenuUI]
│   ├── OptionsUI [unique name: %OptionsUI]
│   ├── ... your existing UIs (CraftingUI, PartyMenuUI, etc.) ...
```

### Step 3: Connect Main Menu Node References

In MainMenuUI inspector, connect all the Export node paths to their respective nodes in the scene tree.

### Step 4: Enable Menu Opening

Add this to your **PlayerController** or **GameManager**:

```csharp
public override void _Input(InputEvent @event)
{
    // Open menu with Escape or Start button
    if (@event.IsActionPressed("ui_cancel"))
    {
        var mainMenu = GetNode<MainMenuUI>("%MainMenuUI");
        if (mainMenu != null)
        {
            if (mainMenu.Visible)
            {
                mainMenu.CloseMenu();
            }
            else
            {
                mainMenu.OpenMenu();
            }
            GetViewport().SetInputAsHandled();
        }
    }
}
```

Or add a dedicated action:

```csharp
if (@event.IsActionPressed("open_menu")) // Add this to Input Map
{
    var mainMenu = GetNode<MainMenuUI>("%MainMenuUI");
    mainMenu?.OpenMenu();
}
```

### Step 5: Configure Input Map

Add these actions in **Project Settings → Input Map**:
- `open_menu` - Escape, Tab, or Start button
- `ui_cancel` - Escape or Back button
- `ui_accept` - Enter or Confirm button

---

## 🎮 How It Works

### Opening the Menu
1. Player presses **ESC** or **Tab** during gameplay
2. MainMenuUI appears, game pauses
3. Player can navigate between all sub-menus

### Menu Flow
```
Main Menu
├── Items → Use/Discard items, select character to use on
├── Skills → View skills, equip/unequip to characters
├── Equipment → Equip/unequip weapons, armor, accessories
├── Status → View character stats, affinities, effects
├── Crafting → Opens existing CraftingUI
├── Party → Opens existing PartyMenuUI
├── Bestiary → Opens existing BestiaryUI
├── Quests → Opens existing QuestUI
├── Options → Configure audio, display, gameplay settings
├── Save → Quick save or open save menu
├── Load → Open load game menu
└── End Game → Return to title screen (with confirmation)
```

### Closing the Menu
- Press **ESC** to go back from sub-menu to main menu
- Press **ESC** again to close the main menu and resume game

---

## 🔧 Customization

### Changing Menu Appearance

Edit the scene files to customize:
- **Colors**: Change PanelContainer themes
- **Fonts**: Add custom fonts to labels
- **Icons**: Add icons to buttons
- **Layout**: Rearrange button order

### Adding New Menu Options

To add a new menu button:

1. Add button to MainMenuUI.tscn
2. Export it in MainMenuUI.cs:
```csharp
[Export] private Button myNewButton;
```

3. Connect in `_Ready()`:
```csharp
myNewButton.Pressed += OnMyNewPressed;
```

4. Create handler:
```csharp
private void OnMyNewPressed()
{
    Managers.SystemManager.Instance?.PlayOkSE();
    menuPanel?.Hide();
    
    if (myNewMenuUI != null)
    {
        myNewMenuUI.Show();
        myNewMenuUI.Call("OpenMenu");
    }
}
```

### Disabling Options

To hide certain menu options:

```csharp
// In MainMenuUI._Ready()
craftingButton.Visible = false; // Hide crafting if not unlocked yet
saveButton.Disabled = true; // Disable saving in certain areas
```

---

## 💡 Integration Tips

### GameManager Integration

Add to your GameManager:

```csharp
private float playtime;

public override void _Process(double delta)
{
    if (!GetTree().Paused)
    {
        playtime += (float)delta;
    }
}

public float GetPlaytime() => playtime;
```

### Input Handling

The menu system properly handles input:
- Pauses game when open (`GetTree().Paused = true`)
- Prevents input passthrough
- Resumes game when closed

### Save Integration

The OptionsMenuUI saves settings to:
- `user://game_settings.cfg`

Settings persist across sessions automatically.

---

## 🐛 Troubleshooting

**Menu won't open?**
- Check that MainMenuUI is in scene tree
- Verify unique name is set (%MainMenuUI)
- Ensure Input Map has ui_cancel action

**Sub-menus not showing?**
- Check all sub-menu UIs are in scene with unique names
- Verify paths in GetSubMenuReferences()
- Make sure all UI scripts are attached

**Sound effects not playing?**
- Ensure SystemManager is autoloaded
- Check audio buses exist (BGM, SFX, UI, Voice)
- Verify sound effect resources are assigned

**Stats not updating?**
- Make sure PartyManager is initialized
- Check InventorySystem and EquipmentManager are working
- Verify character stats are being tracked properly

---

## ✅ Feature Checklist

- [x] Main menu with all game systems
- [x] Item browsing and usage
- [x] Skill management
- [x] Equipment management
- [x] Character status display
- [x] Game options (audio, display, gameplay)
- [x] Save/Load integration
- [x] Keyboard navigation
- [x] Controller support ready
- [x] Sound effects
- [x] Proper pause handling
- [x] Integration with existing systems

---

## 🎉 You're All Set!

Your complete main menu system is ready to use! Players can now:
- ✨ Access all game systems from one menu
- 🎮 Manage items, skills, and equipment
- 📊 View detailed character status
- ⚙️ Configure game settings
- 💾 Save and load their progress
- 📖 Browse bestiary, quests, and crafting

**The menu system integrates seamlessly with all your existing systems!**

---

**Need help?** Check the individual .cs files for detailed code comments!

**Happy game development!** 🚀✨