# 🎮 Menu System - Quick Reference

## 📝 Quick Commands

### Opening/Closing Menu
```csharp
// Open main menu
GetNode<MainMenuUI>("%MainMenuUI")?.OpenMenu();

// Close main menu
GetNode<MainMenuUI>("%MainMenuUI")?.CloseMenu();

// Open specific sub-menu directly
GetNode<ItemMenuUI>("%ItemMenuUI")?.OpenMenu();
GetNode<SkillMenuUI>("%SkillMenuUI")?.OpenMenu();
GetNode<EquipMenuUI>("%EquipMenuUI")?.OpenMenu();
GetNode<StatusMenuUI>("%StatusMenuUI")?.OpenMenu();
GetNode<OptionsMenuUI>("%OptionsUI")?.OpenMenu();
```

### Player Controller Integration
```csharp
public override void _Input(InputEvent @event)
{
    if (@event.IsActionPressed("ui_cancel") || @event.IsActionPressed("open_menu"))
    {
        var menu = GetNode<MainMenuUI>("%MainMenuUI");
        if (menu.Visible)
            menu.CloseMenu();
        else
            menu.OpenMenu();
    }
}
```

---

## 📋 Menu Files Checklist

- [x] MainMenuUI.cs - Hub menu
- [x] ItemMenuUI.cs - Item management
- [x] SkillMenuUI.cs - Skill management
- [x] EquipMenuUI.cs - Equipment management
- [x] StatusMenuUI.cs - Character status
- [x] OptionsMenuUI.cs - Game settings
- [x] SaveMenuUI.cs - Save game
- [x] LoadMenuUI.cs - Load game
- [ ] Your existing: CraftingUI
- [ ] Your existing: PartyMenuUI
- [ ] Your existing: BestiaryUI
- [ ] Your existing: QuestUI

---

## 🎯 Node Structure Template

```
MainMenuUI (Control)
├── Export: menuPanel (Control)
├── Export: buttonContainer (VBoxContainer)
├── Export: itemButton (Button)
├── Export: skillButton (Button)
├── Export: equipButton (Button)
├── Export: statusButton (Button)
├── Export: craftingButton (Button)
├── Export: partyButton (Button)
├── Export: bestiaryButton (Button)
├── Export: questButton (Button)
├── Export: optionsButton (Button)
├── Export: saveButton (Button)
├── Export: loadButton (Button)
├── Export: endGameButton (Button)
├── Export: locationLabel (Label)
├── Export: playtimeLabel (Label)
└── Export: goldLabel (Label)
```

---

## 🔑 Key Properties

### MainMenuUI
- **Process Mode**: `Inherit` (pauses with tree)
- **Unique Name**: `%MainMenuUI`
- **Visibility**: Hidden by default
- **Layer**: Should be on CanvasLayer above game

### All Sub-Menus
- **Process Mode**: `Inherit`
- **Unique Name**: Each menu has unique name (e.g., `%ItemMenuUI`)
- **Visibility**: Hidden by default
- **Parent**: Should be children of same CanvasLayer as MainMenuUI

---

## 🎨 Styling Tips

### Recommended Theme Overrides

**Buttons:**
- Font Size: 20-24
- Minimum Size: 200x50
- Custom Colors:
    - Normal: #FFFFFF
    - Hover: #FFD700
    - Pressed: #FFA500

**Labels:**
- Font Size: 16-18 (body text)
- Font Size: 24-32 (headers)

**Panels:**
- Background Color: #000000AA (semi-transparent)
- Border: 2px solid #FFFFFF44

**Lists (ItemList):**
- Fixed Column Width: 300
- Max Columns: 1
- Item Height: 40

---

## 🔊 Audio Integration

### Sound Effects
```csharp
// Cursor movement
Managers.SystemManager.Instance?.PlayCursorSE();

// Confirm selection
Managers.SystemManager.Instance?.PlayOkSE();

// Cancel/Back
Managers.SystemManager.Instance?.PlayCancelSE();

// Error/Invalid
Managers.SystemManager.Instance?.PlayBuzzerSE();

// Use item
Managers.SystemManager.Instance?.PlayUseItemSE();

// Equip item
Managers.SystemManager.Instance?.PlayEquipSE();

// Save game
Managers.SystemManager.Instance?.PlaySaveSE();

// Load game
Managers.SystemManager.Instance?.PlayLoadSE();
```

---

## 🎮 Input Actions Required

Add to **Project Settings → Input Map**:

| Action | Default Key | Gamepad |
|--------|-------------|---------|
| `ui_cancel` | Escape | B/Circle |
| `ui_accept` | Enter/Space | A/Cross |
| `ui_up` | Up Arrow | D-Pad Up |
| `ui_down` | Down Arrow | D-Pad Down |
| `ui_left` | Left Arrow | D-Pad Left |
| `ui_right` | Right Arrow | D-Pad Right |
| `open_menu` | Tab | Start |

---

## ⚙️ Manager Dependencies

Required Autoloaded Singletons:

| Manager | Path | Purpose |
|---------|------|---------|
| SystemManager | `res://Managers/SystemManager.cs` | Audio, settings |
| InventorySystem | `res://Items/InventorySystem.cs` | Item management |
| EquipmentManager | `res://Items/EquipmentManager.cs` | Equipment |
| PartyManager | `res://Party/PartyManager.cs` | Party data |
| SaveManager | `res://SaveSystem/SaveManager.cs` | Save/Load |
| GameManager | `res://Managers/GameManager.cs` | Playtime tracking |

---

## 🐛 Common Issues & Fixes

### Menu won't open
**Problem**: MainMenuUI not found
**Fix**: Ensure unique name is set: `%MainMenuUI`

### Buttons don't work
**Problem**: Signals not connected
**Fix**: Check `_Ready()` connects all button.Pressed signals

### Sub-menu not showing
**Problem**: Menu not found by GetNode
**Fix**: All menus need unique names with `%` prefix

### Can't close menu
**Problem**: Input not handled correctly
**Fix**: Use `GetViewport().SetInputAsHandled()` after processing

### Stats not updating
**Problem**: Managers not initialized
**Fix**: Ensure all required managers are autoloaded

### No sound effects
**Problem**: SystemManager missing or audio buses not configured
**Fix**: Check audio bus layout has BGM, SFX, UI, Voice buses

---

## 💡 Pro Tips

### Disabling Menu Options
```csharp
// Hide option until unlocked
craftingButton.Visible = false;

// Disable in certain areas
saveButton.Disabled = isInBossBattle;

// Show as locked
questButton.Text = "Quests (Locked)";
questButton.Disabled = true;
```

### Custom Menu Opening Logic
```csharp
private bool CanOpenMenu()
{
    return !isInBattle 
        && !isInCutscene 
        && !isInDialogue
        && !isGameOver;
}
```

### Adding Menu Shortcuts
```csharp
// Press I to open items directly
if (@event.IsActionPressed("quick_items"))
{
    var itemMenu = GetNode<ItemMenuUI>("%ItemMenuUI");
    itemMenu?.OpenMenu();
}
```

### Progress Indicators
```csharp
// Show notification badge on quest button
questButton.Text = $"Quests ({activeQuestCount})";

// Change color if new items
if (hasNewBestiaryEntries)
    bestiaryButton.AddThemeColorOverride("font_color", Colors.Yellow);
```

---

## 📱 Controller Support

The menu system supports gamepad navigation automatically through Godot's UI focus system:

- **D-Pad/Left Stick**: Navigate menu items
- **A/Cross**: Confirm selection
- **B/Circle**: Go back/Cancel
- **Start**: Open menu

### Improving Gamepad Navigation
```csharp
// Set focus neighbors explicitly
itemButton.FocusNeighborBottom = skillButton.GetPath();
skillButton.FocusNeighborTop = itemButton.GetPath();

// Auto-focus first button
itemButton.GrabFocus();

// Show controller hints
if (Input.GetConnectedJoypads().Count > 0)
{
    confirmHint.Text = "[A] Confirm";
    cancelHint.Text = "[B] Back";
}
```

---

## 🔄 Save/Load Integration

### Save Menu
```csharp
// Open save menu
GetNode<SaveMenuUI>("%SaveUI")?.OpenMenu();

// Quick save (no menu)
SaveManager.Instance?.QuickSave();
```

### Load Menu
```csharp
// Open load menu
GetNode<LoadMenuUI>("%LoadUI")?.OpenMenu();

// Quick load (no menu)
SaveManager.Instance?.QuickLoad();
```

---

## 📊 Performance Notes

- Menus use lazy loading - only visible menu processes
- Tree paused when menu open (saves CPU on game logic)
- Stats only update when menu opened or character changes
- No continuous polling or updates

---

## ✅ Testing Checklist

- [ ] Menu opens with ESC/Tab
- [ ] Menu closes with ESC when open
- [ ] All buttons are clickable
- [ ] Keyboard navigation works (arrows + enter)
- [ ] Sound effects play correctly
- [ ] Item usage works
- [ ] Equipment changes apply
- [ ] Skills can be equipped/unequipped
- [ ] Status displays correctly
- [ ] Options save and persist
- [ ] Save system works
- [ ] Load system works
- [ ] Can return to title screen
- [ ] Game pauses when menu open
- [ ] Game resumes when menu closes

---

## 🎉 That's It!

You now have a complete reference for your menu system. Happy developing! 🚀