# 🎯 Quest System - Quick Start Guide

A complete, production-ready quest system for your JRPG!

## ✨ Features

✅ **Main & Side Quests** with different tracking and rewards  
✅ **Multiple Quest Givers**: NPCs, quest boards, readable files, auto-received  
✅ **Objective Types**: Collect items, defeat enemies, talk to NPCs, reach locations, custom  
✅ **Time-Sensitive Quests** that expire based on story progression  
✅ **Quest Journal UI** with tabs for active/completed quests  
✅ **On-Screen Tracker** showing current objectives  
✅ **Quest Notifications** for started/completed/updated quests  
✅ **Event System Integration** with custom event commands  
✅ **Save/Load Support** - all quest progress is saved  
✅ **Automatic Tracking** of items, enemies, NPCs, locations

---

## 📦 What's Included

### Core Files
- `QuestData.cs` - Quest resource definitions
- `QuestManager.cs` - Main quest system (singleton)
- `QuestUI.cs` - Quest journal interface
- `QuestTrackerHUD.cs` - On-screen quest tracker
- `QuestNotification.cs` - Quest popup notifications
- `QuestBoard.cs` - Interactive quest boards
- `QuestGivers.cs` - NPC and file quest givers
- `QuestEventCommands.cs` - Event system commands
- `QuestSaveIntegration.cs` - Save system integration

### Examples & Testing
- `ExampleQuests.cs` - 6 sample quest definitions
- `QuestTestScene.cs` - Full testing scene
- `QuestSystemGuide.md` - Complete documentation

---

## 🚀 Quick Setup (5 Steps)

### 1. Add QuestManager Autoload

**Project Settings → Autoload:**
```
Name: QuestManager
Path: res://Quests/QuestManager.cs
Enable: ✓
```

### 2. Add UI Components to Main Scene

```gdscript
# Add to your main game scene
var quest_ui = preload("res://Quests/QuestUI.tscn").instantiate()
add_child(quest_ui)

var tracker = preload("res://Quests/QuestTrackerHUD.tscn").instantiate()
add_child(tracker)

var notifications = preload("res://Quests/QuestNotification.tscn").instantiate()
add_child(notifications)
```

### 3. Update SaveData.cs

Add to your `SaveData` class:
```csharp
public QuestSaveData Quests { get; set; } = new QuestSaveData();
```

In `CaptureCurrentState()`:
```csharp
if (QuestManager.Instance != null)
    Quests = QuestManager.Instance.GetSaveData();
```

In `ApplyToGame()`:
```csharp
if (Quests != null && QuestManager.Instance != null)
    QuestManager.Instance.LoadSaveData(Quests);
```

### 4. Load Quests at Game Start

```csharp
public override void _Ready()
{
    // Option 1: Load from folder
    QuestManager.Instance?.LoadQuestsFromFolder("res://Data/Quests");
    
    // Option 2: Register example quests
    ExampleQuests.RegisterAllExampleQuests();
}
```

### 5. Test It!

Run `QuestTestScene.cs` to see all features in action.

---

## 🎮 Basic Usage

### Starting a Quest

```csharp
// Via code
QuestManager.Instance.StartQuest("quest_main_001");

// Via event command
var cmd = new StartQuestCommand 
{ 
    QuestId = "quest_main_001",
    ShowNotification = true 
};
```

### Updating Objectives

```csharp
// Manual update
QuestManager.Instance.UpdateObjective("quest_id", "objective_id", 1);

// Automatic tracking (call these in your systems)
QuestManager.Instance.OnItemCollected("item_id", amount);
QuestManager.Instance.OnEnemyDefeated("enemy_id");
QuestManager.Instance.OnNPCTalked("npc_id");
QuestManager.Instance.OnLocationReached("location_id");
```

### Completing Quests

```csharp
// Auto-complete when objectives done
QuestManager.Instance.CompleteQuest("quest_id");

// Or via event command
var cmd = new CompleteQuestCommand 
{ 
    QuestId = "quest_id",
    ShowRewards = true 
};
```

---

## 🌍 Adding Quest Givers

### NPC Quest Giver

```csharp
// Add NPCQuestGiver component to NPC
var questGiver = new NPCQuestGiver
{
    NPCId = "npc_marcus",
    NPCName = "Merchant Marcus",
    ShowQuestIndicator = true
};
questGiver.QuestsToGive.Add(yourQuestResource);
```

Shows **!** for available, **?** for in-progress, **✓** for complete.

### Quest Board

```csharp
var board = new QuestBoard
{
    BoardName = "Guild Quest Board",
    ShowNewQuestIndicator = true
};
board.AvailableQuests.Add(quest1);
board.AvailableQuests.Add(quest2);
```

### Readable File

```csharp
var file = new FileQuestGiver
{
    FileName = "Mysterious Letter",
    FileContent = "Help! Shadow creatures...",
    QuestToGive = yourQuest,
    OneTimeUse = true
};
```

---

## 📝 Creating Quests

### Quick Example

```csharp
var quest = new QuestData
{
    QuestId = "my_quest",
    QuestName = "Help the Village",
    Description = "Defend the village from monsters",
    Type = QuestType.SideQuest,
    GiverType = QuestGiverType.NPC,
    GiverName = "Village Elder"
};

// Add objective
quest.Objectives.Add(new QuestObjective
{
    ObjectiveId = "defeat_monsters",
    Description = "Defeat 10 Monsters",
    Type = QuestObjectiveType.DefeatEnemies,
    TargetId = "enemy_monster",
    RequiredCount = 10
});

// Add rewards (side quests only)
quest.GoldReward = 500;
quest.ExpReward = 200;
quest.ItemRewards.Add(new QuestItemReward 
{ 
    ItemId = "health_potion", 
    Quantity = 3 
});

// Register
QuestManager.Instance.RegisterQuest(quest);
```

### Objective Types

- **CollectItems**: Collect X items
- **DefeatEnemies**: Defeat X enemies
- **TalkToNPC**: Talk to specific NPC
- **ReachLocation**: Reach a location
- **UseItem**: Use an item
- **Custom**: Manually tracked

---

## ⏰ Time-Sensitive Quests

```csharp
var quest = new QuestData
{
    // ... other settings ...
    IsTimeSensitive = true,
    MinimumChapter = 2,
    MaximumChapter = 4,
    RequiredStoryFlag = "chapter_2_started",
    ExpiresAfterFlag = "boss_defeated"
};

// Update chapter to check expiration
QuestManager.Instance.SetCurrentChapter(5); // Quest expires!
```

---

## 🎨 UI Components

### Quest Journal
- **Main Quests Tab**: Story quests
- **Side Quests Tab**: Optional quests
- **Completed Tab**: Finished quests
- **Quest Details**: Full info, objectives, rewards
- **Track/Abandon**: Buttons for quest management

Open with:
```csharp
GetNode<QuestUI>("%QuestUI").OpenJournal();
```

### Quest Tracker (HUD)
- Shows up to 3 active quests
- Displays objectives with progress
- Auto-tracks main quests
- Can minimize/expand

### Quest Notifications
- Popup when quest starts
- Popup when quest completes (with rewards)
- Popup when objectives complete
- Auto-queues multiple notifications

---

## 🔧 Integration Examples

### With Battle System

```csharp
// After battle victory
foreach (var enemy in defeatedEnemies)
{
    QuestManager.Instance.OnEnemyDefeated(enemy.EnemyId);
}
```

### With Inventory System

```csharp
// When adding items
public bool AddItem(ItemData item, int quantity)
{
    // ... add item code ...
    QuestManager.Instance.OnItemCollected(item.ItemId, quantity);
    return true;
}
```

### With Dialogue/Events

```csharp
// When talking to NPC
public void TalkToNPC(string npcId)
{
    QuestManager.Instance.OnNPCTalked(npcId);
    // ... show dialogue ...
}
```

---

## 🎯 Event Commands

Use in your event system:

```csharp
// Start quest
new StartQuestCommand { QuestId = "quest_id" }

// Update objective
new UpdateQuestObjectiveCommand 
{ 
    QuestId = "quest_id",
    ObjectiveId = "obj_id",
    Amount = 1 
}

// Complete quest
new CompleteQuestCommand { QuestId = "quest_id" }

// Conditional based on quest
new ConditionalQuestCommand
{
    QuestId = "quest_id",
    Condition = QuestStatusCheck.IsCompleted,
    TrueCommands = { ... },
    FalseCommands = { ... }
}

// Set chapter
new SetChapterCommand { Chapter = 2 }
```

---

## 📊 Quest Queries

```csharp
// Get quest data
var quest = QuestManager.Instance.GetQuest("quest_id");

// Check status
bool active = QuestManager.Instance.IsQuestActive("quest_id");
bool done = QuestManager.Instance.IsQuestCompleted("quest_id");

// Get lists
var activeQuests = QuestManager.Instance.GetActiveQuests();
var mainQuests = QuestManager.Instance.GetActiveMainQuests();
var sideQuests = QuestManager.Instance.GetActiveSideQuests();
var completed = QuestManager.Instance.GetCompletedQuests();

// Get progress
var progress = QuestManager.Instance.GetQuestProgress("quest_id");
int current = progress["objective_id"];
```

---

## 📱 Signals

```csharp
QuestManager.Instance.QuestStarted += (questId) => { };
QuestManager.Instance.QuestCompleted += (questId) => { };
QuestManager.Instance.QuestObjectiveUpdated += (questId, objId, current, required) => { };
QuestManager.Instance.QuestFailed += (questId) => { };
QuestManager.Instance.QuestExpired += (questId) => { };
```

---

## 🎁 Rewards System

Side quests can have:
- **Gold** - Added to inventory
- **Experience** - Distributed to party
- **Items** - Added to inventory

```csharp
quest.GoldReward = 1000;
quest.ExpReward = 500;
quest.ItemRewards.Add(new QuestItemReward 
{ 
    ItemId = "magic_sword", 
    Quantity = 1 
});
```

Rewards are automatically given when quest completes!

---

## 🐛 Troubleshooting

**Quest not starting?**
- Check quest ID is correct
- Verify chapter range
- Check required flags
- Make sure not already completed

**Objectives not updating?**
- Verify target IDs match exactly
- Ensure quest is active
- Check objective type is correct

**Not saving?**
- Add `QuestSaveData` to SaveData class
- Call `GetSaveData()` in CaptureCurrentState
- Call `LoadSaveData()` in ApplyToGame

---

## 📚 Example Quests Included

1. **Main Quest**: The Shadow Prince's Journey
2. **Side Quest**: Merchant's Request
3. **Time-Sensitive**: Investigate the Disturbance
4. **Quest Board**: Slime Extermination
5. **File Quest**: The Mysterious Letter
6. **Optional Objectives**: The Lost Expedition

Load them with:
```csharp
ExampleQuests.RegisterAllExampleQuests();
```

---

## ✅ Summary

You now have:
- ✨ Complete quest management system
- 📋 Quest journal with full UI
- 📍 On-screen quest tracker
- 🔔 Quest notifications
- 🌍 Multiple quest giver types
- ⏰ Time-sensitive quest support
- 💾 Full save/load integration
- 🎮 Event system integration
- 📝 6 example quests
- 🧪 Full test scene

**The quest system is production-ready and fully integrated with your game!**

---

## 🎉 Next Steps

1. Run `QuestTestScene.cs` to see everything in action
2. Create your own quests using the examples
3. Add quest givers (NPCs, boards, files) to your maps
4. Integrate with your battle, inventory, and dialogue systems
5. Design your main story quest line!

Happy questing! 🚀