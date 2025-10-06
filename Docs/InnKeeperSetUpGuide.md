# 🏨 Inn Keeper System - Setup Guide

## 📋 Overview

Complete inn keeper system that allows players to:
- **Rest at inns** to restore HP and MP
- **Pay gold** for the service
- **See "not enough money"** message if broke
- **Fade to black** during rest
- **Restore all party members** (main + sub party)

---

## 📦 Files Created

1. **InnKeeperData.cs** - Configuration resource
2. **InnKeeperUI.cs** - Choice UI (Stay/Leave)
3. **InnKeeperNPC.cs** - NPC interaction logic

---

## 🚀 Setup Instructions

### Step 1: Create the UI Scene

Create `InnKeeperUI.tscn`:

```
InnKeeperUI (Control) [Script: InnKeeperUI.cs]
├── CanvasLayer
│   └── CenterContainer
│       └── Panel (DialoguePanel)
│           └── MarginContainer
│               └── VBoxContainer
│                   ├── Label (MessageLabel)
│                   │   └── Text: "Welcome message here"
│                   │   └── Autowrap: ON
│                   │   └── Horizontal Alignment: Center
│                   ├── HSeparator
│                   └── HBoxContainer
│                       ├── Button (StayButton)
│                       │   └── Text: "Stay"
│                       │   └── Min Size: 150x50
│                       └── Button (LeaveButton)
│                           └── Text: "Leave"
│                           └── Min Size: 150x50
```

**Node Exports:**
- Connect `DialoguePanel` to the Panel node
- Connect `MessageLabel` to the Label
- Connect `StayButton` to the Stay button
- Connect `LeaveButton` to the Leave button

**Mark with unique name:** `%InnKeeperUI`

---

### Step 2: Add UI to Main Scene

Instance `InnKeeperUI.tscn` in your main game scene:

```gdscript
# Add as child of your root scene or UI layer
var inn_ui = preload("res://Inn/InnKeeperUI.tscn").instantiate()
add_child(inn_ui)
```

Make sure it's marked with unique name `%InnKeeperUI`.

---

### Step 3: Create Inn Data Resource

Right-click in FileSystem → **Create New Resource** → **InnKeeperData**

Configure the resource:
- **Inn Name**: "Cozy Inn"
- **Inn Keeper ID**: "innkeeper_01"
- **Rest Cost**: 50 (or your desired price)
- **Welcome Message**: "Welcome to the inn! A good night's rest costs {cost} gold."
- **Not Enough Money Message**: "I'm sorry, but you don't have enough gold..."
- **After Rest Message**: "Have a good rest!"
- **Fade Duration**: 0.5
- **Rest Duration**: 3.0

Save as `res://Data/InnData/CozyInn.tres`

---

### Step 4: Place Inn Keeper NPC

In your map scene:

```
InnKeeperNPC (Area2D) [Script: InnKeeperNPC.cs]
├── CollisionShape2D (size: 64x64)
└── Sprite2D (your innkeeper sprite)
```

**Configure the NPC:**
1. Assign your inn data resource to **InnData**
2. Set **InnUIPath** to `%InnKeeperUI` (or leave empty to auto-find)
3. Set **ScreenEffectsPath** to `%ScreenEffects` (or leave empty to auto-find)

---

### Step 5: Ensure ScreenEffects Exists

Make sure you have a `ScreenEffects` node in your scene marked with unique name `%ScreenEffects`.

If you don't have one, create it:

```
ScreenEffects (CanvasLayer) [Script: ScreenEffects.cs]
├── FadeRect (ColorRect)
│   └── Layout: Full Rect
│   └── Color: Black with alpha 0
│   └── Mouse Filter: Ignore
```

---

## 🎮 How It Works

### Player Flow

1. **Player approaches innkeeper** → Prompt appears "[E] Talk"
2. **Player presses E** → Shows welcome message with cost
3. **Player sees two buttons:**
    - **Stay** → Continues to payment check
    - **Leave** → Closes dialogue and exits
4. **If Stay is chosen:**
    - **Check gold:**
        - **Not enough** → Show "not enough money" message → End
        - **Enough** → Deduct gold → Proceed to rest
5. **Rest sequence:**
    - Fade to black (0.5 seconds)
    - Wait 3 seconds
    - **Restore HP/MP** for all party members
    - Fade back in (0.5 seconds)

---

## 🔧 Customization

### Change Rest Cost

Edit the InnKeeperData resource:
- **Rest Cost**: 100 (for expensive inns)
- **Rest Cost**: 20 (for cheap inns)

### Custom Messages

Edit the InnKeeperData resource messages:
```
Welcome Message: "Welcome, traveler! Rest here for {cost} gold and wake refreshed!"
Not Enough Money Message: "Oh dear... you don't seem to have enough gold. Come back when you do!"
```

### Different Inn Locations

Create multiple InnKeeperData resources:
- `CheapInn.tres` - Cost: 20
- `StandardInn.tres` - Cost: 50
- `LuxuryInn.tres` - Cost: 100

Assign different data to different inn keepers!

---

## 🎯 Gold System Integration

The inn keeper will automatically find your gold system using this priority:

1. **ShopManager** → `GetGold()`, `SpendGold(amount)`
2. **GameManager** → `GetGold()`, `SpendGold(amount)`
3. **InventorySystem** → `GetGold()`, `RemoveGold(amount)`

**Make sure ONE of these systems exists** with the appropriate methods!

---

## 🐛 Troubleshooting

### Inn keeper doesn't respond
- ✅ InnKeeperData assigned?
- ✅ InnKeeperUI exists with unique name `%InnKeeperUI`?
- ✅ CollisionShape2D configured?
- ✅ Player is in "player" group?

### "Not enough money" always shows
- ✅ Gold system method found? (Check console for errors)
- ✅ Gold getter returning correct value?

### Screen doesn't fade
- ✅ ScreenEffects node exists?
- ✅ ScreenEffects marked with unique name `%ScreenEffects`?
- ✅ FadeRect configured properly?

### HP/MP not restoring
- ✅ PartyMenuManager in autoload?
- ✅ Characters added to party?
- ✅ Check console for restoration messages

---

## 🎨 UI Styling Tips

### Dialogue Panel
- Background: Semi-transparent black (#000000CC)
- Border: 2px white outline
- Corner Radius: 10px

### Buttons
- Normal Color: #4A4A4A
- Hover Color: #6A6A6A
- Pressed Color: #2A2A2A
- Min Size: 150x50

### Message Label
- Font Size: 18-20
- Center aligned
- Autowrap enabled
- Padding: 20px all sides

---

## ✨ Example Inn Data Presets

### Starting Town Inn
```
Inn Name: "Traveler's Rest"
Rest Cost: 30
Welcome: "Welcome! Rest for {cost} gold?"
```

### Mid-Game Inn
```
Inn Name: "Golden Bed Inn"
Rest Cost: 75
Welcome: "The finest beds in town! Only {cost} gold."
```

### Late-Game Inn
```
Inn Name: "Royal Suite"
Rest Cost: 150
Welcome: "Luxury accommodations for {cost} gold."
```

### Free Inn (Story Event)
```
Inn Name: "Hero's Welcome"
Rest Cost: 0
Welcome: "Please, rest for free! You've saved us!"
```

---

## 🎉 You're Done!

Your inn keeper system is ready! Players can now:
- ✅ Rest at inns
- ✅ Pay gold
- ✅ See error messages
- ✅ Experience fade transitions
- ✅ Fully restore party HP/MP

**Test it by:**
1. Placing an inn keeper in a test map
2. Walking up and pressing E
3. Trying to stay with/without enough gold
4. Confirming party HP/MP restoration

Enjoy your inn system! 🏨✨