# Echoes Across Time - Complete Project Documentation

## 📋 Table of Contents
1. [Project Overview](#project-overview)
2. [System Architecture](#system-architecture)
3. [Getting Started](#getting-started)
4. [Core Systems](#core-systems)
5. [Battle System](#battle-system)
6. [UI & Menu Systems](#ui--menu-systems)
7. [Game Systems](#game-systems)
8. [Event & Dialogue System](#event--dialogue-system)
9. [Overworld Systems](#overworld-systems)
10. [Save System](#save-system)
11. [API Reference](#api-reference)
12. [Development Guide](#development-guide)

---

## Project Overview

**Echoes Across Time** (formerly Nocturne Requiem) is a **production-ready Godot 4 JRPG framework** built with C#, featuring AAA-quality systems comparable to Persona 5 Royal and Tales series games.

### 🎮 Key Features

#### Battle System
- **Persona 5 Royal-style turn-based combat** with 8 element types
- **Weakness exploitation** with One More mechanic
- **Baton Pass chains** for strategic turn passing with stacking bonuses
- **Technical Damage** combos combining status effects with elements
- **Showtime attacks** - cinematic duo attacks between character pairs
- **Limit Break system** with solo and DUO ultimate attacks
- **All-Out Attack** finisher when all enemies are knocked down
- **Guard, Item, and Escape** mechanics

#### RPG Systems
- 🏪 **Shop System** - Buy/sell with stock management
- 📜 **Quest System** - Main/side quests with objectives and rewards
- 📖 **Bestiary** - Progressive enemy discovery system
- 🎭 **Skit System** - Tales-style character conversations
- 📚 **Lore Codex** - World-building encyclopedia
- 🔗 **Bond/Relationship System** - Character connections
- ⚡ **Encounter System** - Random battle encounters
- 🎒 **Inventory & Equipment** - Complete item management
- 🔨 **Crafting System** - Recipe-based item creation

#### Technical Features
- 💾 **Complete save/load system** with 10 save slots
- 🎨 **Comprehensive menu system** integrating all features
- 🎵 **Audio management** for BGM, SFX, and voice
- 🎬 **Event/cutscene system** with command-based scripting
- 🗺️ **Overworld character control** with state machine
- 🏃 **Party management** with main/sub party system

### Project Information
- **Engine:** Godot 4.5+
- **Language:** C# (.NET 8.0)
- **Project Name:** Echoes Across Time
- **Version:** v1.0.0
- **Resolution:** 1920x1080

---

## System Architecture

### Autoload Managers (Global Singletons)

The project uses several autoloaded manager scripts that provide global access:

```
SystemManager          - Core system coordinator
QuestManager           - Quest tracking and management
PartyManager           - Party composition and stats
SkitManager            - Character conversation system
AudioManager           - Music and sound effects
BestiaryManager        - Enemy encyclopedia tracking
EncounterManager       - Random battle encounters
BondManager            - Character bond levels
RelationshipManager    - Character relationships
ShopManager            - Shop system management
LoreCodexManager       - Lore/worldbuilding entries
HudManager             - HUD display management
```

### Project Structure

```
EchoesAcrossTime/
│
├── Audio/                          # Audio management
│   └── AudioManager.cs            # BGM, SFX, Voice
│
├── Beastiary/                      # Enemy encyclopedia
│   ├── BestiaryData.cs
│   ├── BestiaryManager.cs         # (Autoload)
│   ├── BestiaryUI.cs
│   ├── BestiaryNotification.cs
│   └── BestiaryIntegration.cs
│
├── Bonds/                          # Character relationships
│   ├── BondManager.cs             # (Autoload)
│   └── RelationshipManager.cs     # (Autoload)
│
├── Combat/                         # Battle system
│   ├── BattleManager.cs           # Main battle controller
│   ├── BattleMember.cs
│   ├── BattleAction.cs
│   ├── BatonPassSystem.cs
│   ├── TechnicalDamage.cs
│   ├── ShowtimeAttacks.cs
│   ├── LimitBreakSystem.cs
│   ├── GuardSystem.cs
│   ├── BattleItemSystem.cs
│   ├── EscapeSystem.cs
│   ├── ElementType.cs
│   ├── CharacterStats.cs
│   ├── SkillData.cs
│   └── StatusEffectManager.cs
│
├── Crafting/                       # Item crafting
│   ├── CraftingRecipeData.cs
│   ├── CraftingManager.cs
│   └── CraftingUI.cs
│
├── Database/                       # Game data
│   ├── CharacterData.cs
│   ├── GameDatabase.cs
│   └── ItemData.cs
│
├── Encounters/                     # Battle encounters
│   └── EncounterManager.cs        # (Autoload)
│
├── Events/                         # Cutscene system
│   ├── EventCommand.cs
│   ├── EventCommandExecutor.cs
│   ├── EventObject.cs
│   └── DialogueData.cs
│
├── Items/                          # Item system
│   ├── InventorySystem.cs
│   ├── EquipmentManager.cs
│   ├── EquipmentData.cs
│   └── ConsumableData.cs
│
├── LoreCodex/                      # Lore encyclopedia
│   ├── LoreCodexData.cs
│   ├── LoreCodexManager.cs        # (Autoload)
│   └── LoreCodexUI.cs
│
├── Managers/                       # Core managers
│   ├── SystemManager.cs           # (Autoload)
│   ├── GameManager.cs
│   └── PartyManager.cs            # (Autoload)
│
├── Quests/                         # Quest system
│   ├── QuestData.cs
│   ├── QuestManager.cs            # (Autoload)
│   └── QuestUI.cs
│
├── Shops/                          # Shop system
│   ├── ShopData.cs
│   ├── ShopManager.cs             # (Autoload)
│   └── ShopUI.cs
│
├── Skits/                          # Character skits
│   ├── SkitData.cs
│   ├── SkitManager.cs             # (Autoload)
│   └── SkitUI.cs
│
├── UI/                             # User interface
│   ├── MainMenuUI.cs
│   ├── ItemMenuUI.cs
│   ├── SkillMenuUI.cs
│   ├── EquipMenuUI.cs
│   ├── StatusMenuUI.cs
│   ├── OptionsMenuUI.cs
│   ├── SaveMenuUI.cs
│   └── LoadMenuUI.cs
│
└── Characters/                     # Overworld control
    ├── OverworldCharacter.cs
    ├── PlayerCharacter.cs
    ├── States/
    │   ├── IdleState.cs
    │   ├── WalkingState.cs
    │   ├── RunningState.cs
    │   ├── JumpingState.cs
    │   ├── ClimbingState.cs
    │   ├── PullingState.cs
    │   └── PushingState.cs
    └── FishingState.cs
```

---

## Getting Started

### Prerequisites
- **Godot 4.5+** with C# support
- **.NET 8.0 SDK**
- Basic knowledge of C# and Godot

### Initial Setup

1. **Clone the repository**
```bash
git clone [your-repo-url]
cd EchoesAcrossTime
```

2. **Open in Godot 4**
    - Launch Godot Engine
    - Import project.godot
    - Wait for C# compilation to complete

3. **Verify Autoloads**
    - Go to Project → Project Settings → Autoload
    - Ensure all manager scripts are properly loaded

4. **Test the Project**
    - Run the main scene
    - Test battle system with Phase2BattleTest.cs
    - Explore individual system test scenes

---

## Core Systems

### 1. System Manager
**Location:** `Managers/SystemManager.cs` (Autoload)

Central coordinator for all game systems.

**Features:**
- Database initialization
- Sound effect management
- System coordination
- Global event handling

**Usage:**
```csharp
// Play sound effect
SystemManager.Instance.PlayCursorSE();
SystemManager.Instance.PlayDecisionSE();
SystemManager.Instance.PlayCancelSE();
```

### 2. Game Manager
**Location:** `Managers/GameManager.cs`

Manages overall game state and scene transitions.

**Key Responsibilities:**
- Game database reference
- Save data management
- Scene transitions
- Battle initiation
- Global game state

**Usage:**
```csharp
// Access database
var character = GameManager.Instance.Database.GetCharacter("char_id");
var item = GameManager.Instance.Database.GetItem("item_id");

// Start battle
GameManager.Instance.StartBattle(playerParty, enemies, isBossBattle: false);

// Return to overworld
GameManager.Instance.ReturnToOverworld();
```

### 3. Party Manager
**Location:** `Managers/PartyManager.cs` (Autoload)

Manages party composition and character roster.

**Features:**
- Main party (up to 4 active members)
- Sub party (reserve members)
- Character locking
- Experience distribution
- Party-wide queries

**Usage:**
```csharp
// Get parties
var mainParty = PartyManager.Instance.GetMainParty();
var subParty = PartyManager.Instance.GetSubParty();

// Add character
PartyManager.Instance.AddToMainParty(characterData);

// Swap parties
PartyManager.Instance.SwapToSubParty("char_id");

// Lock/unlock character
PartyManager.Instance.LockCharacter("char_id", true);

// Distribute experience
PartyManager.Instance.DistributeExperience(1000);
```

---

## Battle System

### Overview

A complete **Persona 5 Royal-style** turn-based combat system with advanced strategic mechanics.

### BattleManager.cs
**Location:** `Combat/BattleManager.cs`

Main battle controller handling all combat logic.

#### Key Signals
```csharp
BattleStarted()
TurnStarted(string characterName)
ActionExecuted(string actor, BattleAction action)
WeaknessHit(string attacker, string target)
EnemyKnockedDown(string enemy)
AllEnemiesKnockedDown()
OneMoreGranted(string character)
AllOutAttackAvailable()
BatonPassExecuted(string from, string to, int level)
TechnicalDamage(string attacker, string target, string combo)
ShowtimeTriggered(string name, string char1, string char2)
LimitBreakReady(string character)
LimitBreakUsed(string character, string name, bool isDuo)
BattleEnded(bool victory)
```

#### Key Methods
```csharp
// Initialize battle
void InitializeBattle(
    List<CharacterStats> playerParty,
    List<CharacterStats> enemies,
    List<ShowtimeAttackData> showtimes = null,
    List<LimitBreakData> limitBreaks = null,
    bool isBossBattle = false,
    bool canEscape = true
);

// Execute action
void ExecuteAction(BattleAction action);

// Turn management
void NextTurn();
void SkipTurn();

// Special mechanics
void ExecuteAllOutAttack();
bool CanAllOutAttack();
bool CanBatonPass();
bool ExecuteBatonPass(BattleMember target);
```

### Battle Mechanics

#### 1. Elemental System

**8 Element Types:**
- Fire 🔥
- Ice ❄️
- Thunder ⚡
- Water 💧
- Earth 🌍
- Light ✨
- Dark 🌑
- Physical ⚔️

**Affinity Types:**
- **Weak** (150% damage) → Knockdown + One More
- **Normal** (100% damage)
- **Resist** (50% damage)
- **Immune** (0% damage)
- **Absorb** (Heals instead of damages)

**Weakness Exploitation:**
```csharp
// Hitting a weakness grants:
- 1.5x damage multiplier
- Knocks down the enemy
- Grants "One More" extra turn
- Enables All-Out Attack when all enemies down
```

#### 2. One More System

After hitting a weakness or landing a critical hit, the attacker gains an immediate extra turn.

**Features:**
- Can chain multiple times
- Resets at end of round
- Can be passed to allies via Baton Pass
- Enables strategic turn optimization

#### 3. Baton Pass System
**Location:** `Combat/BatonPassSystem.cs`

Pass your One More turn to an ally with stacking bonuses.

**Bonus Levels:**
- **Level 1:** +50% damage, +50% healing, +10% crit
- **Level 2:** +100% damage, +100% healing, +20% crit
- **Level 3:** +150% damage, +150% healing, +30% crit

**Restrictions:**
- Cannot pass to yourself
- Target must be alive and not acted yet
- Requires One More turn active

**Usage:**
```csharp
// Check if can baton pass
bool canPass = battleManager.CanBatonPass();

// Get available targets
var targets = battleManager.GetBatonPassTargets();

// Execute pass
bool success = battleManager.ExecuteBatonPass(targetAlly);
```

#### 4. Technical Damage
**Location:** `Combat/TechnicalDamage.cs`

Bonus damage from combining status effects with specific elements (1.5x multiplier).

**Combos:**
- **Burn** + Thunder/Ice → Technical!
- **Freeze** + Physical → Shatter!
- **Shock/Paralysis** + Physical → Critical strike!
- **Sleep** + Any attack → Wake violently!
- **Poison** + Fire/Thunder → Detonate!
- **Confusion** + Light/Dark → Exploit!

**Implementation:**
```csharp
// Check for technical
bool isTech = TechnicalDamage.CheckTechnicalDamage(
    skill.Element, 
    target
);

// Apply technical multiplier
if (isTech)
{
    result.DamageMultiplier *= 1.5f;
    result.IsTechnical = true;
}
```

#### 5. Showtime Attacks
**Location:** `Combat/ShowtimeAttacks.cs`

Cinematic duo attacks between specific character pairs.

**Features:**
- Random trigger chance (15% per turn)
- Very high damage multiplier (3.0-4.0x)
- High critical chance (50%)
- Hits all enemies
- Cooldown system (3-5 turns)
- Requires both characters alive

**Creating Showtimes:**
```csharp
var showtime = new ShowtimeAttackData
{
    Character1Id = "aria",
    Character2Id = "echo_walker",
    Name = "Frozen Echo",
    Description = "A devastating ice-lightning combo",
    Element = ElementType.Ice,
    BasePower = 300,
    Multiplier = 3.5f,
    TriggerChance = 15,
    CooldownTurns = 5
};
```

#### 6. Limit Break System
**Location:** `Combat/LimitBreakSystem.cs`

Ultimate attacks powered by the Limit Gauge (0-100).

**Types:**
- **Solo Limit Break:** Single character ultimate
- **DUO Limit Break:** Pair ultimate (requires both gauges full)

**Gauge Building:**
- Take damage: +3 per 10% HP lost
- Deal damage: +2 per hit
- Hit weakness: +5
- Knock down enemy: +5
- Critical hit: +5
- Baton Pass: +8
- Technical: +10
- Showtime: +15
- Ally KO'd: +20

**Usage:**
```csharp
// Build gauge
LimitBreakSystem.BuildLimitGauge(member, amount);

// Check if ready
bool ready = LimitBreakSystem.IsLimitBreakReady(member);

// Execute solo
LimitBreakSystem.ExecuteSoloLimitBreak(member, target);

// Execute DUO
LimitBreakSystem.ExecuteDUOLimitBreak(member1, member2, enemies);
```

#### 7. All-Out Attack

Team finisher when all enemies are knocked down.

**Features:**
- Massive damage (2x Attack stat per party member)
- Hits all enemies
- Consumes all party member turns
- Enemies stand back up after

**Trigger:**
```csharp
// Automatically available when all enemies knocked down
if (battleManager.CanAllOutAttack())
{
    battleManager.ExecuteAllOutAttack();
}
```

#### 8. Guard System
**Location:** `Combat/GuardSystem.cs`

Defensive stance reducing incoming damage.

**Effects:**
- 50% damage reduction
- Small HP/MP regeneration
- Builds Limit Gauge (+5)
- Must reapply each turn

**Usage:**
```csharp
var guardAction = new BattleAction(member, BattleActionType.Guard);
battleManager.ExecuteAction(guardAction);
```

#### 9. Item System
**Location:** `Combat/BattleItemSystem.cs`

Use consumable items during battle.

**Item Types:**
- **Healing:** Restore HP
- **Recovery:** Restore MP
- **Revival:** Revive KO'd allies
- **Offensive:** Damage items (bombs, etc.)
- **Status:** Cure or inflict status effects

**Usage:**
```csharp
var itemAction = new BattleAction(user, BattleActionType.Item)
{
    ItemData = healthPotion
};
itemAction = itemAction.WithTargets(ally);
battleManager.ExecuteAction(itemAction);
```

#### 10. Escape System
**Location:** `Combat/EscapeSystem.cs`

Flee from battle (not available in boss battles).

**Formula:**
- Base 50% chance
- Modified by party speed vs enemy speed
- +10% per failed attempt
- HP penalty on failure

**Usage:**
```csharp
// Check if can escape
bool canEscape = battleManager.CanEscape();

// Attempt escape
var escapeAction = new BattleAction(member, BattleActionType.Escape);
battleManager.ExecuteAction(escapeAction);
```

### Battle Flow Diagram

```
┌─────────────────────────────────┐
│   1. Initialize Battle          │
│   - Load characters & enemies   │
│   - Set up turn order           │
└────────────┬────────────────────┘
             │
             ▼
┌─────────────────────────────────┐
│   2. Start Turn                 │
│   - Process status effects      │
│   - Check for auto-actions      │
└────────────┬────────────────────┘
             │
             ▼
┌─────────────────────────────────┐
│   3. Action Selection           │
│   - Attack / Skill              │
│   - Guard / Item / Escape       │
│   - Baton Pass / Limit Break    │
└────────────┬────────────────────┘
             │
             ▼
┌─────────────────────────────────┐
│   4. Execute Action             │
│   - Calculate damage            │
│   - Apply status effects        │
└────────────┬────────────────────┘
             │
             ▼
┌─────────────────────────────────┐
│   5. Check for Special Events   │
│   - Weakness hit?               │
│   - Critical hit?               │
│   - Technical damage?           │
└────────────┬────────────────────┘
             │
             ▼
┌─────────────────────────────────┐
│   6. Grant One More?            │
│   - If yes, actor acts again    │
│   - Can Baton Pass              │
└────────────┬────────────────────┘
             │
             ▼
┌─────────────────────────────────┐
│   7. Check Battle State         │
│   - All enemies down? → AOA     │
│   - Showtime available?         │
│   - Battle ended?               │
└────────────┬────────────────────┘
             │
             ▼
┌─────────────────────────────────┐
│   8. Next Turn or End Battle    │
│   - Continue to next actor      │
│   - Or end with victory/defeat  │
└─────────────────────────────────┘
```

---

## UI & Menu Systems

### Main Menu System
**Location:** `UI/MainMenuUI.cs`

Comprehensive pause menu integrating all game systems.

**Features:**
- Opens with ESC/Tab
- Pauses game
- Shows location, playtime, gold
- Access to all sub-menus
- Sound effects
- Keyboard/controller navigation

**Sub-Menus:**
1. **Item Menu** - Browse/use items
2. **Skill Menu** - View/equip skills
3. **Equipment Menu** - Manage equipment
4. **Status Menu** - Character stats
5. **Crafting Menu** - Craft items
6. **Party Menu** - Party management
7. **Bestiary** - Enemy encyclopedia
8. **Quest Log** - Quest journal
9. **Options** - Game settings
10. **Save** - Save game
11. **Load** - Load game

**Usage:**
```csharp
// Open menu
var mainMenu = GetNode<MainMenuUI>("%MainMenuUI");
mainMenu.OpenMenu();

// Close menu
mainMenu.CloseMenu();
```

### Sub-Menu Systems

#### Item Menu
**Location:** `UI/ItemMenuUI.cs`

**Features:**
- Filter by type (Consumable, Key, Material, Equipment)
- Use items on party members
- Sort inventory
- Item details display
- Discard items

#### Skill Menu
**Location:** `UI/SkillMenuUI.cs`

**Features:**
- Per-character skill management
- Equip/unequip skills
- Skill details (MP cost, power, element)
- Equipped vs available view
- Skill slot limits

#### Equipment Menu
**Location:** `UI/EquipMenuUI.cs`

**Features:**
- Equip weapons, armor, accessories
- Stat preview with changes
- Per-character equipment
- Requirements checking
- Swap between inventory and character

#### Status Menu
**Location:** `UI/StatusMenuUI.cs`

**Features:**
- Character portraits
- Level and experience bars
- Detailed stats display
- Element affinities
- Equipment bonuses
- Active status effects

#### Options Menu
**Location:** `UI/OptionsMenuUI.cs`

**Features:**
- Audio volumes (BGM, SFX, Voice)
- Display settings (Fullscreen, VSync)
- Gameplay options (Text speed, Auto-save)
- Controls display
- Settings persistence

---

## Game Systems

### 1. Inventory System
**Location:** `Items/InventorySystem.cs`

Manages all items, gold, and consumables.

**Features:**
- Item stacking
- Maximum capacity
- Item categories
- Gold management
- Item usage

**Usage:**
```csharp
// Add items
InventorySystem.Instance.AddItem(itemData, quantity);

// Remove items
InventorySystem.Instance.RemoveItem("item_id", quantity);

// Check if has item
bool hasItem = InventorySystem.Instance.HasItem("item_id", quantity);

// Get item count
int count = InventorySystem.Instance.GetItemCount("item_id");

// Use item
InventorySystem.Instance.UseItem("item_id", target);

// Gold management
InventorySystem.Instance.AddGold(amount);
InventorySystem.Instance.RemoveGold(amount);
int gold = InventorySystem.Instance.GetGold();
```

### 2. Equipment System
**Location:** `Items/EquipmentManager.cs`

Manages character equipment and stat bonuses.

**Equipment Slots:**
- Weapon
- Armor
- Accessory 1
- Accessory 2

**Stat Bonuses:**
- MaxHP / MaxMP
- Attack / Defense
- Magic Attack / Magic Defense
- Speed / Luck
- Critical Rate / Evasion Rate

**Usage:**
```csharp
// Equip item from inventory
EquipmentManager.Instance.EquipFromInventory(
    "character_id", 
    "item_id", 
    characterData
);

// Unequip to inventory
EquipmentManager.Instance.UnequipToInventory(
    "character_id", 
    EquipSlot.Weapon
);

// Get equipped item
var weapon = EquipmentManager.Instance.GetEquippedItem(
    "character_id", 
    EquipSlot.Weapon
);

// Apply equipment bonuses to stats
EquipmentManager.Instance.ApplyEquipmentBonuses(
    "character_id", 
    characterStats
);

// Unequip all
EquipmentManager.Instance.UnequipAll("character_id");
```

### 3. Shop System
**Location:** `Shops/` (Autoload: ShopManager)

Complete buy/sell system with stock management.

**Features:**
- Buy/sell items
- Limited or unlimited stock
- Auto-restocking
- Item unlocking (level, quest, story requirements)
- Featured items
- Transaction validation
- Save/load integration

**Creating Shops:**
```csharp
var shop = new ShopData
{
    ShopId = "weapon_shop_01",
    ShopName = "Steel & Edge Armory",
    ShopDescription = "Fine weapons for discerning warriors",
    IsUnlocked = true,
    CanSellItems = true,
    SellPriceMultiplier = 0.5f // Sell for 50% of buy price
};

// Add items for sale
shop.ItemsForSale.Add(new ShopItem
{
    ItemId = "iron_sword",
    BuyPrice = 100,
    UnlimitedStock = true
});

ShopManager.Instance.RegisterShop(shop);
```

**Using Shops:**
```csharp
// Open shop
ShopManager.Instance.OpenShop("weapon_shop_01");

// Buy item
bool success = ShopManager.Instance.BuyItem("iron_sword", 1);

// Sell item
bool success = ShopManager.Instance.SellItem("old_sword", 1);

// Check if can afford
bool canAfford = ShopManager.Instance.CanAfford("iron_sword", 1);
```

### 4. Quest System
**Location:** `Quests/` (Autoload: QuestManager)

Comprehensive quest tracking with objectives.

**Quest Types:**
- Main Quests
- Side Quests
- Time-limited quests

**Objective Types:**
- Defeat enemies (specific type, quantity)
- Collect items
- Talk to NPCs
- Reach locations
- Custom objectives

**Creating Quests:**
```csharp
var quest = new QuestData
{
    QuestId = "main_001",
    QuestName = "The Beginning",
    Description = "Start your journey",
    Type = QuestType.Main,
    IsRepeatable = false
};

// Add objectives
quest.Objectives.Add(new QuestObjective
{
    ObjectiveId = "obj_1",
    Description = "Defeat 3 slimes",
    Type = ObjectiveType.DefeatEnemy,
    TargetId = "slime",
    RequiredCount = 3
});

// Add rewards
quest.Rewards.Gold = 100;
quest.Rewards.Experience = 50;
quest.Rewards.ItemRewards.Add(new ItemReward
{
    ItemId = "health_potion",
    Quantity = 3
});

QuestManager.Instance.RegisterQuest(quest);
```

**Using Quests:**
```csharp
// Start quest
QuestManager.Instance.StartQuest("main_001");

// Update objective manually
QuestManager.Instance.UpdateObjective("main_001", "obj_1", 1);

// Auto-tracking
QuestManager.Instance.OnEnemyDefeated("slime");
QuestManager.Instance.OnItemCollected("herb", 1);
QuestManager.Instance.OnNPCTalked("merchant");
QuestManager.Instance.OnLocationReached("old_ruins");

// Complete quest
QuestManager.Instance.CompleteQuest("main_001");

// Query quests
var activeQuests = QuestManager.Instance.GetActiveQuests();
var completedQuests = QuestManager.Instance.GetCompletedQuests();
```

### 5. Bestiary System
**Location:** `Beastiary/` (Autoload: BestiaryManager)

Automatic enemy encyclopedia with progressive discovery.

**Features:**
- Auto-tracks encountered enemies
- Records weaknesses as discovered in combat
- Tracks skills used by enemies
- Shows drop rates and item drops
- Completion percentage
- Discovery notifications
- Full save/load integration

**Usage:**
```csharp
// Auto-tracking (via BestiaryIntegration)
// Enemies automatically tracked when battle starts

// Manual tracking
BestiaryManager.Instance.RecordEncounter("goblin", level: 5);
BestiaryManager.Instance.RecordDefeat("goblin");

// Discovery during combat
BestiaryManager.Instance.RecordWeaknessDiscovered("goblin", ElementType.Fire);
BestiaryManager.Instance.RecordSkillDiscovered("goblin", "fireball");

// Query data
var entry = BestiaryManager.Instance.GetEntry("goblin");
bool discovered = entry.IsDiscovered;
int timesEncountered = entry.TimesEncountered;
var weaknesses = entry.DiscoveredWeaknesses;

// Completion percentage
float completion = BestiaryManager.Instance.CompletionPercentage;

// Open UI
var bestiaryUI = GetNode<BestiaryUI>("%BestiaryUI");
bestiaryUI.ShowBestiary();
```

### 6. Skit System
**Location:** `Skits/` (Autoload: SkitManager)

Tales of Series-style optional character conversations.

**Features:**
- 2D character portraits with emotions
- Typewriter text effect
- Voice acting support
- Skippable scenes
- Auto and manual triggers
- Notification icons
- Availability rules (party composition, story progress)

**Creating Skits:**
```csharp
var skit = new SkitData
{
    SkitId = "skit_001",
    Title = "First Meeting",
    IsRepeatable = false,
    RequiresAllCharacters = new[] { "aria", "dominic" }
};

// Add dialogue lines
skit.Dialogue.Add(new SkitDialogueLine
{
    CharacterId = "aria",
    Emotion = CharacterEmotion.Happy,
    Text = "Hey there! I'm Aria!",
    VoiceFile = "aria_greeting.ogg"
});

skit.Dialogue.Add(new SkitDialogueLine
{
    CharacterId = "dominic",
    Emotion = CharacterEmotion.Normal,
    Text = "Nice to meet you. I'm Dominic.",
    VoiceFile = "dominic_greeting.ogg"
});

SkitManager.Instance.RegisterSkit(skit);
```

**Playing Skits:**
```csharp
// Manual play
SkitManager.Instance.PlaySkit("skit_001");

// Check availability
bool canPlay = SkitManager.Instance.CanPlaySkit("skit_001");

// Get available skits
var available = SkitManager.Instance.GetAvailableSkits();
```

### 7. Lore Codex System
**Location:** `LoreCodex/` (Autoload: LoreCodexManager)

World-building encyclopedia for discovering lore.

**Categories:**
- Characters
- Locations
- Events
- Concepts
- Items
- Organizations

**Features:**
- Progressive discovery system
- Detailed entries with lore text
- Image support
- Search and filter functionality
- Completion tracking
- Discovery notifications

**Usage:**
```csharp
var entry = new LoreCodexEntry
{
    EntryId = "ancient_city",
    Category = LoreCategory.Location,
    Title = "The Ancient City",
    ShortDescription = "A lost civilization",
    FullText = "Long ago, a great city stood here...",
    IsDiscovered = false
};

LoreCodexManager.Instance.RegisterEntry(entry);

// Discover entry
LoreCodexManager.Instance.DiscoverEntry("ancient_city");

// Open UI
var codexUI = GetNode<LoreCodexUI>("%LoreCodexUI");
codexUI.ShowCodex();
```

### 8. Crafting System
**Location:** `Crafting/`

Recipe-based item and equipment crafting.

**Features:**
- Recipe-based crafting
- Material requirements
- Skill level requirements
- Success rate system
- Batch crafting support
- Recipe discovery

**Creating Recipes:**
```csharp
var recipe = new CraftingRecipeData
{
    RecipeId = "iron_sword_recipe",
    ResultItemId = "iron_sword",
    ResultQuantity = 1,
    RequiredLevel = 1,
    SuccessRate = 100
};

// Add required materials
recipe.RequiredMaterials.Add(new MaterialRequirement
{
    ItemId = "iron_ore",
    Quantity = 3
});

recipe.RequiredMaterials.Add(new MaterialRequirement
{
    ItemId = "wood",
    Quantity = 1
});

CraftingManager.Instance.RegisterRecipe(recipe);
```

**Crafting Items:**
```csharp
// Check if can craft
bool canCraft = CraftingManager.Instance.CanCraft("iron_sword_recipe");

// Craft item
bool success = CraftingManager.Instance.CraftItem("iron_sword_recipe");
```

### 9. Bond System
**Location:** `Bonds/` (Autoload: BondManager, RelationshipManager)

Character relationship and bond progression system.

**Features:**
- Bond levels (1-10)
- Relationship tracking between characters
- Bond points system
- Bond events and rewards
- Relationship types (Friend, Rival, Romance, etc.)

**Usage:**
```csharp
// Add bond points
BondManager.Instance.AddBondPoints("aria", 10);

// Check bond level
int level = BondManager.Instance.GetBondLevel("aria");

// Set relationship
RelationshipManager.Instance.SetRelationship(
    "aria", 
    "dominic", 
    RelationshipType.Friend
);

// Get relationship
var relationship = RelationshipManager.Instance.GetRelationship(
    "aria", 
    "dominic"
);
```

### 10. Encounter System
**Location:** `Encounters/` (Autoload: EncounterManager)

Random battle encounter system for overworld.

**Features:**
- Step-based encounters
- Encounter rate customization
- Zone-based enemy tables
- Encounter avoidance
- Advantage/disadvantage system

**Usage:**
```csharp
// Set encounter rate
EncounterManager.Instance.SetEncounterRate(0.05f); // 5% per step

// Register encounter zone
EncounterManager.Instance.RegisterZone("forest_zone", enemyList);

// Enable/disable encounters
EncounterManager.Instance.SetEncountersEnabled(true);

// Check for encounter
if (EncounterManager.Instance.CheckEncounter())
{
    // Start battle
    var enemies = EncounterManager.Instance.GetRandomEncounter("forest_zone");
    GameManager.Instance.StartBattle(playerParty, enemies);
}
```

---

## Event & Dialogue System

### Event System
**Location:** `Events/`

Flexible command-based system for scripting cutscenes and story events.

**Core Components:**
- **EventCommand.cs** - Base class for all event commands
- **EventCommandExecutor.cs** - Executes command sequences
- **EventObject.cs** - Scene objects that trigger events
- **DialogueData.cs** - Dialogue content structure

### Event Command Types

```csharp
public enum EventCommandType
{
    ShowText,              // Display dialogue
    ShowChoices,           // Player choices
    Wait,                  // Wait duration
    ChangeGold,            // Add/remove gold
    ChangeItems,           // Add/remove items
    ChangeWeapons,         // Equip/unequip
    HealCharacter,         // Heal party member
    ChangePartyMembers,    // Add/remove from party
    MovePlayer,            // Move character
    FadeScreen,            // Screen fade effect
    TintScreen,            // Screen tint
    FlashScreen,           // Screen flash
    ShakeScreen,           // Camera shake
    PlayBGM,               // Play background music
    StopBGM,               // Stop music
    PlaySE,                // Play sound effect
    TransferMap,           // Change scene
    ConditionalBranch,     // If/else logic
    SetVariable,           // Set variable
    CallCommonEvent,       // Call reusable event
    ControlSelfSwitch,     // Local switches
    NameInput,             // Character naming
    ChangeTransparency,    // Hide/show character
    ShowBalloonIcon,       // Emotion icon
    InitiateBattle,        // Start battle
    InitiateShop           // Open shop
}
```

### Creating Event Commands

**Show Dialogue:**
```csharp
var showText = new ShowTextCommand
{
    Dialogue = new DialogueData
    {
        SpeakerName = "Aria",
        Message = "Hello! Welcome to the village.",
        PortraitPath = "res://Characters/Portraits/aria.png",
        VoiceClip = voiceAudioStream
    }
};
```

**Show Player Choices:**
```csharp
var showChoices = new ShowChoicesCommand
{
    Choices = new[] { "Accept quest", "Decline quest", "Ask for details" },
    DefaultChoice = 0,
    VariableName = "quest_choice"
};
```

**Conditional Branch:**
```csharp
var conditional = new ConditionalBranchCommand
{
    ConditionType = ConditionType.Switch,
    SwitchName = "quest_complete",
    ExpectedValue = true,
    CommandsIfTrue = new[] { /* commands */ },
    CommandsIfFalse = new[] { /* commands */ }
};
```

**Initiate Battle:**
```csharp
var battle = new InitiateBattleCommand
{
    TroopId = "forest_encounter_01",
    CanEscape = true,
    CanLose = false,
    BattleBGM = battleMusicStream
};
```

**Transfer to Another Map:**
```csharp
var transfer = new TransferMapCommand
{
    MapPath = "res://Maps/Town.tscn",
    SpawnPosition = new Vector2(100, 200),
    FacingDirection = 2, // Down
    FadeOut = true,
    FadeIn = true
};
```

### Event Objects

**Location:** `Events/EventObject.cs`

Scene objects that trigger events in the world.

**Trigger Types:**
- **Action Button:** Trigger on interact (E key)
- **Player Touch:** Trigger on collision
- **Autorun:** Trigger automatically once
- **Parallel:** Run continuously in background

**Creating Event Objects:**
```gdscript
# In Godot scene
EventObject (Node2D)
├── Sprite2D (Character sprite)
├── Area2D (Collision detection)
│   └── CollisionShape2D
└── EventPages (Array of EventPage resources)
```

**Event Page Conditions:**
- Switch states
- Variable comparisons
- Item possession
- Party member presence
- Self switches (A, B, C, D)

---

## Overworld Systems

### Character Control
**Location:** `Characters/`

State machine-based character control system.

**Character Types:**
- **PlayerCharacter** - Player-controlled character
- **FollowerCharacter** - Party members following player
- **OverworldCharacter** - Base class for all characters

### Movement States

**Standard States:**
- **IdleState** - Standing still
- **WalkingState** - Normal movement
- **RunningState** - Fast movement
- **JumpingState** - Jumping mechanics

**Puzzle/Interaction States:**
- **ClimbingState** - Ladder/wall climbing
- **PushingState** - Pushing objects
- **PullingState** - Pulling objects
- **LockedState** - No movement (cutscenes/menus)

**Special States:**
- **FishingState** - Fishing mini-game

### Player Control

**Input Actions:**
```gdscript
# In Project Settings → Input Map
move_up
move_down
move_left
move_right
run (hold to run)
interact (E key)
menu (ESC key)
```

**Character States:**
```csharp
// Change state
playerCharacter.ChangeState(new RunningState());

// Lock for cutscene
playerCharacter.ChangeState(new LockedState());

// Unlock after cutscene
playerCharacter.ChangeState(new IdleState());
```

---

## Save System

### SaveManager
**Location:** `Managers/SaveManager.cs`

Complete save/load system with 10 save slots.

**Features:**
- 10 independent save slots
- Auto-save functionality
- Save preview information
- Corrupted save detection
- Custom data dictionary for extensions

### SaveData Structure

```csharp
public class SaveData
{
    // Meta info
    public string SaveName { get; set; }
    public DateTime SaveTime { get; set; }
    public float PlayTime { get; set; }
    public string CurrentLocation { get; set; }
    
    // Player data
    public int Gold { get; set; }
    public List<SavedCharacter> Party { get; set; }
    public Dictionary<string, int> Inventory { get; set; }
    public Dictionary<string, EquipmentSlot> Equipment { get; set; }
    
    // Progress
    public List<string> CompletedQuests { get; set; }
    public List<string> ActiveQuests { get; set; }
    public Dictionary<string, BestiaryEntryData> BestiaryEntries { get; set; }
    
    // Story
    public Dictionary<string, bool> StoryFlags { get; set; }
    public Dictionary<string, int> Variables { get; set; }
    
    // Custom data for extensions
    public Dictionary<string, Variant> CustomData { get; set; }
}
```

### Using Save System

**Save Game:**
```csharp
// Save to slot
SaveManager.Instance.SaveGame(slotIndex: 0, "My Save");

// Auto-save
SaveManager.Instance.AutoSave();
```

**Load Game:**
```csharp
// Check if save exists
bool exists = SaveManager.Instance.SaveExists(slotIndex: 0);

// Load game
SaveManager.Instance.LoadGame(slotIndex: 0);

// Get save info for preview
SaveSlotInfo info = SaveManager.Instance.GetSaveSlotInfo(slotIndex: 0);
// info.SaveName, info.PlayTime, info.Location, etc.
```

**Delete Save:**
```csharp
SaveManager.Instance.DeleteSave(slotIndex: 0);
```

### Custom Save Data

For custom systems, use the CustomData dictionary:

```csharp
// Saving custom data
public void CaptureCustomData()
{
    var saveData = SaveManager.Instance.CurrentSave;
    
    // Add bestiary data
    var bestiaryData = BestiaryManager.Instance.GetSaveData();
    saveData.CustomData["Bestiary"] = Json.Stringify(bestiaryData);
    
    // Add lore codex
    var loreData = LoreCodexManager.Instance.GetSaveData();
    saveData.CustomData["LoreCodex"] = Json.Stringify(loreData);
}

// Loading custom data
public void RestoreCustomData()
{
    var saveData = SaveManager.Instance.CurrentSave;
    
    if (saveData.CustomData.ContainsKey("Bestiary"))
    {
        var json = saveData.CustomData["Bestiary"].AsString();
        BestiaryManager.Instance.LoadSaveData(json);
    }
    
    if (saveData.CustomData.ContainsKey("LoreCodex"))
    {
        var json = saveData.CustomData["LoreCodex"].AsString();
        LoreCodexManager.Instance.LoadSaveData(json);
    }
}
```

---

## API Reference

### Quick Reference Tables

#### Battle System API

| Method | Description |
|--------|-------------|
| `BattleManager.InitializeBattle()` | Start new battle with parties |
| `BattleManager.ExecuteAction()` | Execute battle action |
| `BattleManager.NextTurn()` | Advance to next turn |
| `BattleManager.CanAllOutAttack()` | Check if AOA available |
| `BattleManager.ExecuteAllOutAttack()` | Perform All-Out Attack |
| `BattleManager.CanBatonPass()` | Check if can baton pass |
| `BattleManager.ExecuteBatonPass()` | Pass turn to ally |
| `TechnicalDamage.CheckTechnicalDamage()` | Check for technical combo |
| `ShowtimeAttacks.CanUseShowtime()` | Check showtime availability |
| `ShowtimeAttacks.ExecuteShowtime()` | Use showtime attack |
| `LimitBreakSystem.BuildLimitGauge()` | Add to limit gauge |
| `LimitBreakSystem.IsLimitBreakReady()` | Check if gauge full |
| `LimitBreakSystem.ExecuteSoloLimitBreak()` | Use solo ultimate |
| `LimitBreakSystem.ExecuteDUOLimitBreak()` | Use pair ultimate |

#### Party Management API

| Method | Description |
|--------|-------------|
| `PartyManager.GetMainParty()` | Get active party members |
| `PartyManager.GetSubParty()` | Get reserve members |
| `PartyManager.AddToMainParty()` | Add to active party |
| `PartyManager.AddToSubParty()` | Add to reserve |
| `PartyManager.SwapToMainParty()` | Move to active |
| `PartyManager.SwapToSubParty()` | Move to reserve |
| `PartyManager.RemoveFromParty()` | Remove character |
| `PartyManager.LockCharacter()` | Lock/unlock character |
| `PartyManager.DistributeExperience()` | Award battle exp |

#### Inventory API

| Method | Description |
|--------|-------------|
| `InventorySystem.AddItem()` | Add item to inventory |
| `InventorySystem.RemoveItem()` | Remove item |
| `InventorySystem.HasItem()` | Check if has item |
| `InventorySystem.GetItemCount()` | Get item quantity |
| `InventorySystem.UseItem()` | Use consumable item |
| `InventorySystem.AddGold()` | Add gold |
| `InventorySystem.RemoveGold()` | Remove gold |
| `InventorySystem.GetGold()` | Get gold amount |
| `InventorySystem.IsFull()` | Check if inventory full |

#### Equipment API

| Method | Description |
|--------|-------------|
| `EquipmentManager.EquipFromInventory()` | Equip item from inventory |
| `EquipmentManager.UnequipToInventory()` | Unequip item to inventory |
| `EquipmentManager.GetEquippedItem()` | Get equipped in slot |
| `EquipmentManager.GetCharacterEquipment()` | Get all equipment |
| `EquipmentManager.GetCharacterBonuses()` | Get stat bonuses |
| `EquipmentManager.ApplyEquipmentBonuses()` | Apply bonuses to stats |
| `EquipmentManager.UnequipAll()` | Remove all equipment |

#### Quest System API

| Method | Description |
|--------|-------------|
| `QuestManager.RegisterQuest()` | Add quest to system |
| `QuestManager.StartQuest()` | Begin quest |
| `QuestManager.CompleteQuest()` | Complete quest |
| `QuestManager.FailQuest()` | Fail quest |
| `QuestManager.UpdateObjective()` | Manual objective update |
| `QuestManager.OnEnemyDefeated()` | Auto-track enemy kill |
| `QuestManager.OnItemCollected()` | Auto-track item collection |
| `QuestManager.OnNPCTalked()` | Auto-track NPC talk |
| `QuestManager.OnLocationReached()` | Auto-track location |
| `QuestManager.GetQuest()` | Get quest data |
| `QuestManager.GetActiveQuests()` | Get active quests |
| `QuestManager.GetCompletedQuests()` | Get completed quests |

#### Shop System API

| Method | Description |
|--------|-------------|
| `ShopManager.RegisterShop()` | Add shop to system |
| `ShopManager.OpenShop()` | Open shop UI |
| `ShopManager.CloseShop()` | Close shop UI |
| `ShopManager.BuyItem()` | Purchase item |
| `ShopManager.SellItem()` | Sell item to shop |
| `ShopManager.CanAfford()` | Check if can buy |
| `ShopManager.UnlockShop()` | Unlock shop |
| `ShopManager.UnlockItem()` | Unlock shop item |
| `ShopManager.RestockShop()` | Refill stock |

#### Bestiary API

| Method | Description |
|--------|-------------|
| `BestiaryManager.RecordEncounter()` | Log enemy encounter |
| `BestiaryManager.RecordDefeat()` | Log enemy defeat |
| `BestiaryManager.RecordWeaknessDiscovered()` | Log weakness found |
| `BestiaryManager.RecordSkillDiscovered()` | Log skill seen |
| `BestiaryManager.GetEntry()` | Get bestiary entry |
| `BestiaryManager.CompletionPercentage` | Get completion % |

#### Save System API

| Method | Description |
|--------|-------------|
| `SaveManager.SaveGame()` | Save to slot |
| `SaveManager.LoadGame()` | Load from slot |
| `SaveManager.SaveExists()` | Check if save exists |
| `SaveManager.DeleteSave()` | Delete save |
| `SaveManager.GetSaveSlotInfo()` | Get save preview |
| `SaveManager.AutoSave()` | Auto-save game |

---

## Development Guide

### Best Practices

1. **Always initialize databases before systems**
    - Load GameDatabase first
    - Initialize managers in correct order
    - Verify autoloads are loaded

2. **Connect signals in _Ready()**
    - Use Connect() for code-based connections
    - Use inspector for scene-based connections
    - Always check null before emitting

3. **Use unique names for UI nodes**
   ```csharp
   // Mark nodes in inspector with unique name
   var healthBar = GetNode<ProgressBar>("%HealthBar");
   ```

4. **Save frequently during gameplay**
    - Auto-save at checkpoints
    - Save before major events
    - Implement save reminders

5. **Test individual systems with test scenes**
    - Create isolated test scenes
    - Test edge cases
    - Verify integrations

6. **Use proper error handling**
   ```csharp
   try
   {
       // Risky operation
   }
   catch (Exception ex)
   {
       GD.PrintErr($"Error: {ex.Message}");
   }
   ```

7. **Follow consistent naming conventions**
    - PascalCase for classes and methods
    - camelCase for fields
    - UPPER_CASE for constants

### Common Patterns

#### Starting a New Game

```csharp
public void StartNewGame()
{
    // Initialize save data
    var saveData = new SaveData
    {
        SaveName = "New Game",
        SaveTime = DateTime.Now,
        PlayTime = 0,
        CurrentLocation = "starting_village",
        Gold = 500
    };
    
    // Add starting characters
    var startingChars = new[] { "aria", "dominic", "echo_walker" };
    foreach (var charId in startingChars)
    {
        var charData = GameManager.Instance.Database.GetCharacter(charId);
        if (charData != null)
        {
            PartyManager.Instance.AddToMainParty(charData);
        }
    }
    
    // Give starting items
    InventorySystem.Instance.AddItem("health_potion", 5);
    InventorySystem.Instance.AddItem("mana_potion", 3);
    
    // Initialize systems
    QuestManager.Instance.LoadQuestsFromFolder("res://Data/Quests");
    ShopManager.Instance.LoadShopsFromFolder("res://Data/Shops");
    BestiaryManager.Instance.InitializeFromDatabase(
        GameManager.Instance.Database
    );
    
    // Start game
    GetTree().ChangeSceneToFile("res://Scenes/Overworld.tscn");
}
```

#### Implementing Enemy AI

```csharp
public BattleAction DecideEnemyAction(
    BattleMember enemy, 
    List<BattleMember> allies, 
    List<BattleMember> players)
{
    // Check HP - heal if low
    if (enemy.Stats.CurrentHP < enemy.Stats.MaxHP * 0.3f)
    {
        var healSkill = enemy.Skills.FirstOrDefault(s => s.HealsHP);
        if (healSkill != null)
        {
            return new BattleAction(enemy, BattleActionType.Skill)
            {
                Skill = healSkill
            }.WithTargets(enemy);
        }
    }
    
    // Target weakest player
    var weakestPlayer = players
        .Where(p => p.Stats.IsAlive)
        .OrderBy(p => p.Stats.CurrentHP)
        .FirstOrDefault();
    
    // Use strongest skill
    var bestSkill = enemy.Skills
        .Where(s => s.MPCost <= enemy.Stats.CurrentMP)
        .OrderByDescending(s => s.BasePower)
        .FirstOrDefault();
    
    if (bestSkill != null)
    {
        return new BattleAction(enemy, BattleActionType.Skill)
        {
            Skill = bestSkill
        }.WithTargets(weakestPlayer);
    }
    
    // Default to basic attack
    return new BattleAction(enemy, BattleActionType.Attack)
        .WithTargets(weakestPlayer);
}
```

#### Integrating Battle System

```csharp
// In overworld encounter
private void OnEncounterTriggered()
{
    // Get player party
    var playerParty = PartyManager.Instance.GetMainPartyStats();
    
    // Get random enemies
    var enemies = EncounterManager.Instance.GetRandomEncounter("forest_zone");
    
    // Get available showtimes
    var showtimes = GetAvailableShowtimes();
    
    // Save overworld state
    GameManager.Instance.SaveOverworldState(
        GetTree().CurrentScene.SceneFilePath,
        playerPosition
    );
    
    // Start battle
    GameManager.Instance.StartBattle(playerParty, enemies);
}

// In battle scene _Ready()
public override void _Ready()
{
    var battleManager = GetNode<BattleManager>("BattleManager");
    
    // Connect to battle ended signal
    battleManager.BattleEnded += OnBattleEnded;
    
    // Initialize battle
    battleManager.InitializeBattle(
        GameManager.Instance.PlayerParty,
        GameManager.Instance.Enemies,
        GameManager.Instance.AvailableShowtimes
    );
}

private void OnBattleEnded(bool victory)
{
    if (victory)
    {
        // Award rewards
        // Distribute experience
        // Return to overworld
        GameManager.Instance.ReturnToOverworld();
    }
    else
    {
        // Game over screen
        GetTree().ChangeSceneToFile("res://UI/GameOver.tscn");
    }
}
```

### Debugging Tips

#### Battle Issues
- Check signals are properly connected
- Verify character stats are valid (HP > 0, etc.)
- Use `GD.Print()` to trace execution flow
- Test with simple scenarios first
- Check turn order calculation

#### Save/Load Issues
- Verify all data is properly serializable
- Check file permissions
- Test with new saves before existing ones
- Use try-catch for error handling
- Validate data after loading

#### UI Issues
- Verify unique names are set (`%NodeName`)
- Check node paths are correct
- Ensure scripts are attached
- Test input actions in Project Settings
- Check for null references

#### Performance Issues
- Profile with Godot's performance monitor
- Check for memory leaks
- Optimize heavy loops
- Use object pooling for particles
- Minimize signal connections

### Testing Checklist

**Battle System:**
- [ ] Basic attack works
- [ ] Skills execute correctly
- [ ] Weaknesses grant One More
- [ ] Baton Pass chains work
- [ ] Technical damage triggers
- [ ] Showtimes activate
- [ ] Limit Breaks execute
- [ ] All-Out Attack works
- [ ] Guard reduces damage
- [ ] Items work in battle
- [ ] Escape succeeds/fails
- [ ] Victory rewards granted
- [ ] Defeat handled correctly

**UI Systems:**
- [ ] Main menu opens/closes
- [ ] All sub-menus accessible
- [ ] Item menu displays items
- [ ] Skill menu shows skills
- [ ] Equipment menu works
- [ ] Status menu shows stats
- [ ] Save/Load work correctly
- [ ] Options persist

**Game Systems:**
- [ ] Shops buy/sell correctly
- [ ] Quests track objectives
- [ ] Bestiary records enemies
- [ ] Skits play correctly
- [ ] Inventory manages items
- [ ] Equipment applies bonuses
- [ ] Crafting creates items
- [ ] Party management works

### Next Steps for Development

1. **Create Content**
    - Design characters with stats and skills
    - Create enemies and bosses
    - Write quests and dialogue
    - Design items and equipment
    - Create crafting recipes

2. **Build World**
    - Create maps and areas
    - Place NPCs and events
    - Set up encounters
    - Design dungeons
    - Create town shops

3. **Balance Game**
    - Adjust stat values
    - Balance skill costs and powers
    - Set appropriate prices
    - Tune encounter rates
    - Test difficulty curve

4. **Add Polish**
    - Create particle effects
    - Add sound effects
    - Implement transitions
    - Add battle animations
    - Create cutscenes

5. **Playtest**
    - Internal testing
    - Get feedback
    - Iterate on feedback
    - Fix bugs
    - Optimize performance

---

## Conclusion

**Echoes Across Time** is a **complete, production-ready JRPG framework** with professional-grade systems comparable to AAA titles like Persona 5 Royal and Tales series games. The architecture is modular, extensible, and well-documented.

### What You Have

✅ **Complete battle system** with advanced mechanics  
✅ **Full RPG systems** (quests, shops, bestiary, etc.)  
✅ **Comprehensive UI** integrating all features  
✅ **Event/cutscene system** for storytelling  
✅ **Save/load system** with multiple slots  
✅ **Overworld character control**  
✅ **Well-documented codebase**  
✅ **Modular, extensible architecture**

### You Have Everything You Need to Create an Amazing JRPG! 🎮⚔️✨

### Quick Commands Cheat Sheet

```csharp
// Start battle
battleManager.InitializeBattle(party, enemies, showtimes);

// Party management
PartyManager.Instance.AddToMainParty(character);
PartyManager.Instance.DistributeExperience(1000);

// Inventory
InventorySystem.Instance.AddItem("item_id", quantity);
InventorySystem.Instance.AddGold(amount);

// Equipment
EquipmentManager.Instance.EquipFromInventory("char_id", "item_id", character);

// Quests
QuestManager.Instance.StartQuest("quest_id");
QuestManager.Instance.CompleteQuest("quest_id");

// Shops
ShopManager.Instance.OpenShop("shop_id");
ShopManager.Instance.BuyItem("item_id", quantity);

// Bestiary
BestiaryManager.Instance.RecordEncounter("enemy_id");

// Save/Load
SaveManager.Instance.SaveGame(slotIndex, "Save Name");
SaveManager.Instance.LoadGame(slotIndex);

// Events
SkitManager.Instance.PlaySkit("skit_id");
LoreCodexManager.Instance.DiscoverEntry("entry_id");

// Bonds
BondManager.Instance.AddBondPoints("character_id", points);
```

### Support & Resources

**Documentation Files:**
- All documentation is in the `Docs/` folder
- Battle system guides
- UI system guides
- System integration examples

**Test Scenes:**
- `BattleTest.cs` - Basic battle mechanics
- `Phase2BattleTest.cs` - Advanced battle features
- `CoreCommandsTest.cs` - Guard/Item/Escape
- Individual system test scenes

**Community:**
- Report issues on GitHub
- Join Discord for discussions
- Check wiki for tutorials
- Contribute improvements

---

**Happy game development! 🚀**

*Documentation Version: 1.0.0*  
*Last Updated: 2025*  
*Project: Echoes Across Time (Godot 4.5 / C#)*