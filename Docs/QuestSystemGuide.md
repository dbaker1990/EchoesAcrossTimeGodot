# Quest System - Complete Guide

## 📋 Overview

A comprehensive quest system for your JRPG featuring:
- **Main Quests** and **Side Quests**
- Detailed **objective tracking**
- **Time-sensitive quests** based on story progression
- Multiple **quest giver types** (NPCs, quest boards, files)
- **Reward system** for side quests
- Full **save/load integration**
- **Event system integration**

---

## 📁 File Structure

```
Quests/
├── QuestData.cs                    # Quest definition resource
├── QuestManager.cs                 # Core quest management singleton
├── QuestUI.cs                      # Quest journal interface
├── QuestBoard.cs                   # Quest board in the world
├── QuestGivers.cs                  # NPC & file quest givers
├── QuestEventCommands.cs           # Event system integration
└── QuestSaveIntegration.cs         # Save system integration

Docs/
└── QuestSystemGuide.md            # This file
```

---

## 🚀 Setup Instructions

### 1. Add QuestManager as Autoload

**Project Settings → Autoload:**
- Name: `QuestManager`
- Path: `res://Quests/QuestManager.cs`
- Enable: ✓

### 2. Add QuestUI to Your Main Scene

```gdscript
# In your main game scene
var quest_ui = preload("res://Quests/QuestUI.tscn").instantiate()
add_child(quest_ui)
```

Make sure to mark it with unique name: `%QuestUI`

### 3. Update SaveData.cs

Add the quest save data property to your SaveData class:

```csharp
public QuestSaveData Quests { get; set; } = new QuestSaveData();
```

Update your save/load methods to include quest data (see QuestSaveIntegration.cs).

### 4. Load Quest Database

In your game initialization:

```csharp
public override void _Ready()
{
    // Load all quest resources from folder
    QuestManager.Instance?.LoadQuestsFromFolder("res://Data/Quests");
}
```

---

## 📝 Creating Quests

### Method 1: Create .tres Resource File

1. **Right-click in FileSystem** → New Resource
2. **Select QuestData**
3. **Configure the quest:**

#### Basic Info:
- **Quest ID**: `"quest_main_001"` (unique identifier)
- **Quest Name**: `"The Shadow Prince's Journey"`
- **Description**: Quest description text
- **Type**: MainQuest or SideQuest

#### Quest Giver:
- **Giver Type**: NPC, QuestBoard, ReadFile, or AutoReceived
- **Giver Name**: Name shown to player
- **Giver ID**: ID for tracking

#### Objectives:
Add QuestObjective resources:
- **Objective ID**: `"obj_collect_crystals"`
- **Description**: `"Collect 5 Shadow Crystals"`
- **Type**: CollectItems, DefeatEnemies, TalkToNPC, ReachLocation, UseItem, Custom
- **Target ID**: Item/enemy/NPC/location ID
- **Required Count**: How many needed
- **Is Optional**: If objective is optional

#### Rewards (Side Quests):
- **Gold Reward**: Amount of gold
- **Exp Reward**: Amount of experience
- **Item Rewards**: Array of QuestItemReward

#### Conditions:
- **Is Time Sensitive**: True if quest expires
- **Required Story Flag**: Flag needed to start
- **Expires After Flag**: Flag that makes quest unavailable
- **Minimum Chapter**: Earliest chapter available
- **Maximum Chapter**: Latest chapter available

### Method 2: Create via Code

```csharp
var quest = new QuestData
{
    QuestId = "quest_side_001",
    QuestName = "Help the Merchant",
    Description = "A merchant needs help gathering rare herbs.",
    Type = QuestType.SideQuest,
    GiverType = QuestGiverType.NPC,
    GiverName = "Merchant Marcus",
    GiverId = "npc_marcus",
    IsTimeSensitive = false,
    MinimumChapter = 1,
    MaximumChapter = 5
};

// Add objectives
var objective1 = new QuestObjective
{
    ObjectiveId = "obj_herbs",
    Description = "Collect 10 Rare Herbs",
    Type = QuestObjectiveType.CollectItems,
    TargetId = "item_rare_herb",
    RequiredCount = 10
};
quest.Objectives.Add(objective1);

// Add rewards
quest.GoldReward = 500;
quest.ExpReward = 200;

var itemReward = new QuestItemReward
{
    ItemId = "item_health_potion",
    Quantity = 3
};
quest.ItemRewards.Add(itemReward);

// Register quest
QuestManager.Instance?.RegisterQuest(quest);
```

---

## 🎮 Using the Quest System

### Starting Quests

#### Via Code:
```csharp
QuestManager.Instance.StartQuest("quest_main_001");
```

#### Via Event Command:
```csharp
var startQuest = new StartQuestCommand
{
    QuestId = "quest_main_001",
    ShowNotification = true
};
```

### Updating Objectives

#### Manual Update:
```csharp
QuestManager.Instance.UpdateObjective("quest_id", "objective_id", 1);
```

#### Automatic Updates:
The system automatically tracks:
```csharp
// When item collected
QuestManager.Instance.OnItemCollected("item_id", amount);

// When enemy defeated
QuestManager.Instance.OnEnemyDefeated("enemy_id");

// When NPC talked to
QuestManager.Instance.OnNPCTalked("npc_id");

// When location reached
QuestManager.Instance.OnLocationReached("location_id");
```

#### Via Event Command:
```csharp
var updateQuest = new UpdateQuestObjectiveCommand
{
    QuestId = "quest_id",
    ObjectiveId = "objective_id",
    Amount = 1,
    SetDirectly = false // true = set value, false = add value
};
```

### Completing Quests

#### Auto-Complete:
Quests complete automatically when all objectives are done and player talks to quest giver.

#### Manual Complete:
```csharp
QuestManager.Instance.CompleteQuest("quest_id");
```

#### Via Event Command:
```csharp
var completeQuest = new CompleteQuestCommand
{
    QuestId = "quest_id",
    ShowRewards = true
};
```

### Quest Queries

```csharp
// Get quest data
var quest = QuestManager.Instance.GetQuest("quest_id");

// Check status
bool isActive = QuestManager.Instance.IsQuestActive("quest_id");
bool isCompleted = QuestManager.Instance.IsQuestCompleted("quest_id");

// Get progress
var progress = QuestManager.Instance.GetQuestProgress("quest_id");

// Get lists
var activeQuests = QuestManager.Instance.GetActiveQuests();
var mainQuests = QuestManager.Instance.GetActiveMainQuests();
var sideQuests = QuestManager.Instance.GetActiveSideQuests();
var completedQuests = QuestManager.Instance.GetCompletedQuests();
```

---

## 🌍 Quest Givers in the World

### NPC Quest Giver

Add `NPCQuestGiver` script to an NPC:

```csharp
// On NPC node
var questGiver = new NPCQuestGiver
{
    NPCId = "npc_marcus",
    NPCName = "Merchant Marcus",
    ShowQuestIndicator = true
};

// Assign quests to give
questGiver.QuestsToGive.Add(questResource);
```

The NPC will automatically:
- Show **!** when quest is available
- Show **?** when quest is in progress
- Show **✓** when quest can be turned in
- Handle quest offering and completion

### Quest Board

Add `QuestBoard` to your scene:

```csharp
var board = new QuestBoard
{
    BoardName = "Guild Quest Board",
    ShowNewQuestIndicator = true
};

// Add available quests
board.AvailableQuests.Add(questResource1);
board.AvailableQuests.Add(questResource2);
```

### File/Document Quest Giver

Add `FileQuestGiver` to readable objects:

```csharp
var file = new FileQuestGiver
{
    FileName = "Mysterious Letter",
    FileContent = "Help! The shadow creatures are invading...",
    QuestToGive = questResource,
    OneTimeUse = true
};
```

---

## 📱 Quest Journal UI

### Opening the Journal

```csharp
// Via code
var ui = GetNode<QuestUI>("%QuestUI");
ui.OpenJournal();

// Via input action
if (Input.IsActionJustPressed("open_quest_journal"))
{
    questUI.OpenJournal();
}
```

### Journal Features

- **Main Quests Tab**: Shows all active main quests
- **Side Quests Tab**: Shows all active side quests
- **Completed Tab**: Shows completed quests
- **Quest Details**: Click quest to see:
    - Quest name and description
    - Who gave the quest
    - All objectives with progress (✓/○)
    - Rewards (for side quests)
    - Track button (if enabled)
    - Abandon button (side quests only)

### Signals

```csharp
QuestManager.Instance.QuestStarted += OnQuestStarted;
QuestManager.Instance.QuestCompleted += OnQuestCompleted;
QuestManager.Instance.QuestObjectiveUpdated += OnObjectiveUpdated;
QuestManager.Instance.QuestFailed += OnQuestFailed;
QuestManager.Instance.QuestExpired += OnQuestExpired;
```

---

## ⏰ Time-Sensitive Quests

### Setting Up Time-Sensitive Quests

```csharp
var quest = new QuestData
{
    // ... other properties ...
    IsTimeSensitive = true,
    MinimumChapter = 2,
    MaximumChapter = 4,
    RequiredStoryFlag = "chapter_2_started",
    ExpiresAfterFlag = "main_boss_defeated"
};
```

### Chapter Management

```csharp
// Update current chapter
QuestManager.Instance.SetCurrentChapter(3);

// Or via event command
var setChapter = new SetChapterCommand { Chapter = 3 };
```

When chapter updates, the system automatically:
- Checks all active quests
- Fails any expired quests
- Emits `QuestExpired` signal

---

## 🎯 Integration Examples

### Battle Integration

```csharp
// After defeating enemy in battle
public void OnEnemyDefeated(string enemyId)
{
    QuestManager.Instance?.OnEnemyDefeated(enemyId);
}
```

### Inventory Integration

```csharp
// When item is added to inventory
public bool AddItem(ItemData item, int quantity)
{
    // ... add item code ...
    
    // Update quest objectives
    QuestManager.Instance?.OnItemCollected(item.ItemId, quantity);
    
    return true;
}
```

### Dialogue Integration

```csharp
// When talking to NPC
public void TalkToNPC(string npcId)
{
    QuestManager.Instance?.OnNPCTalked(npcId);
    
    // Then show dialogue
    ShowDialogue(npcId);
}
```

### Event System Integration

Use the quest event commands in your event pages:

```csharp
// Conditional based on quest
var conditional = new ConditionalQuestCommand
{
    QuestId = "quest_main_001",
    Condition = QuestStatusCheck.IsCompleted
};

// If quest completed
conditional.TrueCommands.Add(new ShowMessageCommand 
{ 
    Message = "Thank you for your help!" 
});

// If quest not completed
conditional.FalseCommands.Add(new ShowMessageCommand 
{ 
    Message = "Please complete the quest first." 
});
```

---

## 💾 Save/Load Integration

### Updating SaveData.cs

```csharp
// Add to SaveData class
public QuestSaveData Quests { get; set; } = new QuestSaveData();

// In InitializeNewGame
Quests = new QuestSaveData();

// In CaptureCurrentState
if (QuestManager.Instance != null)
{
    Quests = QuestManager.Instance.GetSaveData();
}

// In ApplyToGame
if (Quests != null && QuestManager.Instance != null)
{
    QuestManager.Instance.LoadSaveData(Quests);
}
```

The quest system automatically saves:
- Active quest IDs
- Completed quest IDs
- Objective progress for all quests
- Current chapter

---

## 🎨 Customization Tips

### Custom Objective Types

For special objectives not covered by the standard types:

```csharp
var objective = new QuestObjective
{
    ObjectiveId = "custom_puzzle",
    Description = "Solve the Ancient Puzzle",
    Type = QuestObjectiveType.Custom,
    TargetId = "puzzle_ancient_temple",
    RequiredCount = 1
};

// Then update manually when condition is met
QuestManager.Instance.UpdateObjective(questId, "custom_puzzle", 1);
```

### Quest Notifications

Customize the quest notification UI:

```csharp
private void OnQuestStarted(string questId)
{
    var quest = QuestManager.Instance.GetQuest(questId);
    
    // Show custom notification
    ShowCustomNotification(
        "New Quest!",
        quest.QuestName,
        quest.Type == QuestType.MainQuest ? Colors.Gold : Colors.White
    );
}
```

### Quest Markers on Map

```csharp
if (quest.TrackOnMap)
{
    // Add marker to map
    AddMapMarker(quest.QuestLocation, quest.QuestName);
}
```

---

## 📊 Example Quest Flow

### 1. Create Quest Resource
```
Quest: "Help the Merchant"
- Objective 1: Collect 10 Rare Herbs
- Objective 2: Return to Merchant
- Reward: 500 Gold, 3 Health Potions
```

### 2. Add to NPC
```csharp
merchantNPC.QuestsToGive.Add(helpMerchantQuest);
```

### 3. Player Interaction
1. Player talks to merchant
2. Merchant offers quest
3. Player accepts
4. Quest appears in journal

### 4. Collecting Items
```csharp
// Automatically tracked when items are collected
InventorySystem.Instance.AddItem(rareHerb, 1);
// → QuestManager.OnItemCollected("rare_herb", 1)
```

### 5. Returning to Merchant
1. Player talks to merchant
2. System detects quest objectives complete
3. Shows completion dialog with rewards
4. Gives rewards and marks quest complete

---

## 🐛 Troubleshooting

### Quest Not Starting
- Check if quest ID is correct
- Verify chapter range is valid
- Check required story flags
- Ensure quest hasn't been completed already

### Objectives Not Updating
- Verify target IDs match exactly
- Check if quest is active
- Ensure objective type is correct
- Make sure OnItemCollected/OnEnemyDefeated is being called

### Quest Not Saving
- Confirm SaveData has Quests property
- Check CaptureCurrentState includes quest data
- Verify QuestManager.GetSaveData is called

### Time-Sensitive Issues
- Make sure SetCurrentChapter is called
- Check MinimumChapter and MaximumChapter
- Verify ExpiresAfterFlag is set correctly

---

## 🎉 Summary

You now have a complete quest system with:
- ✅ Main and side quests
- ✅ Objective tracking with multiple types
- ✅ Time-sensitive quest support
- ✅ Multiple quest giver methods
- ✅ Reward system
- ✅ Full save/load integration
- ✅ Event system integration
- ✅ Quest journal UI

The system is ready to use and fully integrated with your existing game systems!