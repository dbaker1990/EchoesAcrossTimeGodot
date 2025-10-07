# Echoes Across Time - Complete Project Documentation

## 📋 Table of Contents
1. [Project Overview](#project-overview)
2. [Quick Start Guide](#quick-start-guide)
3. [Project Structure](#project-structure)
4. [Core Systems](#core-systems)
5. [Battle System](#battle-system)
6. [UI & Menu Systems](#ui--menu-systems)
7. [Game Systems](#game-systems)
8. [Save System](#save-system)
9. [Integration Guide](#integration-guide)
10. [API Reference](#api-reference)

---

## Project Overview

**Echoes Across Time** (formerly Nocturne Requiem) is a comprehensive **Godot 4 JRPG** built with C#, featuring:

- ⚔️ **Persona 5 Royal-style turn-based combat** with advanced mechanics
- 🎭 **Tales of Series-style skit system** for character interactions
- 🏪 **Complete shop system** with buy/sell functionality
- 📖 **Bestiary system** for enemy tracking
- 📜 **Quest system** with objectives and rewards
- 🎨 **Main menu system** integrating all game features
- 💾 **Full save/load system**
- 🔗 **Bond/Relationship system** for character connections
- 📚 **Lore Codex** for world-building
- ⚡ **Encounter system** for random battles

### Key Features
- **8 element types** (Fire, Ice, Thunder, Water, Earth, Light, Dark, Physical)
- **Weakness exploitation** with One More mechanic
- **Baton Pass chains** for strategic turn passing
- **Technical Damage** combos with status effects
- **Showtime attacks** between character pairs
- **Limit Break system** with solo and DUO attacks
- **All-Out Attack** finisher
- **Equipment management** with stat bonuses
- **Crafting system** for items and equipment
- **Party management** with main/sub party system

---

## Quick Start Guide

### Prerequisites
- Godot 4.5+
- .NET SDK
- C# support enabled

### Initial Setup

1. **Clone the repository**
```bash
git clone [your-repo-url]
cd EchoesAcrossTime
```

2. **Open in Godot 4**
    - Open project.godot in Godot Engine
    - Wait for initial C# compilation

3. **Verify Autoloads**
   Project Settings → Autoload should have:
    - SystemManager
    - QuestManager
    - PartyManager
    - SkitManager
    - AudioManager
    - BestiaryManager
    - EncounterManager
    - BondManager
    - RelationshipManager
    - ShopManager
    - LoreCodexManager

4. **Test the Project**
    - Run the main scene
    - Test battle system with Phase2BattleTest.cs
    - Test individual systems with their test scenes

---

## Project Structure

```
EchoesAcrossTime/
│
├── Audio/                          # Audio management
│   └── AudioManager.cs            # BGM, SFX, Voice management
│
├── Beastiary/                      # Enemy encyclopedia
│   ├── BestiaryData.cs            # Entry data structures
│   ├── BestiaryManager.cs         # Core tracking (Autoload)
│   ├── BestiaryUI.cs              # Browse interface
│   ├── BestiaryNotification.cs    # Discovery popups
│   └── BestiaryIntegration.cs     # Auto-tracking
│
├── Bonds/                          # Character relationships
│   ├── BondManager.cs             # Bond system (Autoload)
│   └── RelationshipManager.cs     # Relationships (Autoload)
│
├── Combat/                         # Battle system
│   ├── BattleMember.cs            # Battle participant
│   ├── BattleAction.cs            # Action structures
│   ├── BattleManager.cs           # Core battle controller
│   ├── BatonPassSystem.cs         # Baton pass mechanics
│   ├── TechnicalDamage.cs         # Technical combos
│   ├── ShowtimeAttacks.cs         # Duo attacks
│   ├── LimitBreakSystem.cs        # Ultimate attacks
│   ├── GuardSystem.cs             # Defensive mechanics
│   ├── BattleItemSystem.cs        # Item usage in battle
│   ├── EscapeSystem.cs            # Battle escape logic
│   ├── ElementType.cs             # Element definitions
│   ├── CharacterStats.cs          # Character stats
│   ├── SkillData.cs               # Skill definitions
│   ├── StatusEffectManager.cs     # Status effect handling
│   ├── BattleStats.cs             # Battle statistics
│   └── DamageFormula.cs           # Damage calculations
│
├── Crafting/                       # Item crafting
│   ├── CraftingRecipeData.cs      # Recipe definitions
│   ├── CraftingManager.cs         # Crafting logic
│   └── CraftingUI.cs              # Crafting interface
│
├── Database/                       # Game data
│   ├── CharacterData.cs           # Character definitions
│   ├── GameDatabase.cs            # Central database
│   ├── SystemDatabase.cs          # System settings
│   └── ItemData.cs                # Item definitions
│
├── Encounters/                     # Battle encounters
│   └── EncounterManager.cs        # Random encounters (Autoload)
│
├── Items/                          # Item system
│   ├── InventorySystem.cs         # Inventory management
│   ├── EquipmentManager.cs        # Equipment handling
│   └── EquipmentData.cs           # Equipment definitions
│
├── LoreCodex/                      # Lore encyclopedia
│   ├── LoreCodexData.cs           # Entry definitions
│   ├── LoreCodexManager.cs        # Lore tracking (Autoload)
│   ├── LoreCodexUI.cs             # Browse interface
│   └── LoreCodexNotification.cs   # Discovery popups
│
├── Managers/                       # Core managers
│   ├── SystemManager.cs           # System settings (Autoload)
│   ├── GameManager.cs             # Game state
│   ├── PartyManager.cs            # Party management (Autoload)
│   └── PartyMenuManager.cs        # Party UI management
│
├── Quests/                         # Quest system
│   ├── QuestData.cs               # Quest definitions
│   ├── QuestManager.cs            # Quest tracking (Autoload)
│   ├── QuestUI.cs                 # Quest journal
│   ├── QuestBoard.cs              # Quest boards
│   ├── QuestGivers.cs             # NPCs and triggers
│   ├── QuestEventCommands.cs      # Event integration
│   └── QuestSaveIntegration.cs    # Save/load support
│
├── SaveSystem/                     # Save/load
│   ├── SaveData.cs                # Save data structure
│   ├── SaveManager.cs             # Save/load operations
│   └── SaveSlot.cs                # Save slot management
│
├── Shops/                          # Shop system
│   ├── ShopData.cs                # Shop definitions
│   ├── ShopManager.cs             # Shop controller (Autoload)
│   ├── ShopUI.cs                  # Shop interface
│   ├── ShopTrigger.cs             # Shop triggers
│   ├── ShopNotification.cs        # Transaction popups
│   ├── ShopSaveIntegration.cs     # Save/load support
│   └── ExampleShops.cs            # Example shops
│
├── Skits/                          # Tales-style skits
│   ├── SkitData.cs                # Skit definitions
│   ├── SkitManager.cs             # Skit controller (Autoload)
│   ├── SkitUI.cs                  # Skit display
│   ├── SkitTrigger.cs             # Automatic triggers
│   ├── SkitNotification.cs        # Notification icons
│   └── ExampleSkits.cs            # Example skits
│
├── Testing/                        # Test scenes
│   ├── BattleTest.cs              # Phase 1 battle demo
│   ├── Phase2BattleTest.cs        # Phase 2 advanced demo
│   ├── CoreCommandsTest.cs        # Guard/Item/Escape demo
│   ├── QuestTestScene.cs          # Quest system demo
│   └── BestiaryTestScene.cs       # Bestiary demo
│
├── UI/                             # User interface
│   ├── MainMenuUI.cs              # Main pause menu
│   ├── ItemMenuUI.cs              # Item menu
│   ├── SkillMenuUI.cs             # Skill menu
│   ├── EquipMenuUI.cs             # Equipment menu
│   ├── StatusMenuUI.cs            # Status screen
│   ├── OptionsMenuUI.cs           # Settings menu
│   ├── SaveMenuUI.cs              # Save interface
│   └── LoadMenuUI.cs              # Load interface
│
└── Docs/                           # Documentation
    ├── CompleteIntegrationGuide.md
    ├── Persona5BattleSystem_Documentation.md
    ├── Phase2_Documentation.md
    ├── CoreCommandsDocumentation.md
    ├── MainMenuSystemGuide.md
    ├── MainMenuCompleteSummary.md
    ├── ShopSystemGuide.md
    ├── ShopSystemReadMe.md
    ├── QuestSystemGuide.md
    ├── QuestSystemReadme.md
    ├── BestiarySystemGuide.md
    ├── BestiarySystemComplete.md
    ├── BeastiaryReadMe.md
    └── SkitReadMe.md
```

---

## Core Systems

### 1. System Manager
**Location:** `Managers/SystemManager.cs` (Autoload)

Central configuration system for game-wide settings.

**Key Features:**
- Audio volume control (BGM, SFX, UI, Voice)
- Damage formula management
- System sound effects
- Critical hit settings
- Elemental system configuration
- New game initialization

**Usage:**
```csharp
// Play UI sound
SystemManager.Instance.PlayOkSE();
SystemManager.Instance.PlayCancelSE();

// Calculate damage
int damage = SystemManager.Instance.CalculateDamage(
    attacker, target, skill, isCritical
);

// Get settings
bool critEnabled = SystemManager.Instance.AreCriticalHitsEnabled();
float critMultiplier = SystemManager.Instance.GetCriticalMultiplier();
```

### 2. Game Manager
**Location:** `Managers/GameManager.cs`

Manages overall game state and database access.

**Key Features:**
- Game database reference
- Save data management
- Scene management
- Global game state

**Usage:**
```csharp
// Access database
var character = GameManager.Instance.Database.GetCharacter("char_id");
var item = GameManager.Instance.Database.GetItem("item_id");

// Access save data
var currentSave = GameManager.Instance.CurrentSave;
```

### 3. Party Manager
**Location:** `Managers/PartyManager.cs` (Autoload)

Manages party composition and character data.

**Key Features:**
- Main party (up to 4 members)
- Sub party (reserve members)
- Character locking
- Experience distribution
- Party-wide stat queries

**Usage:**
```csharp
// Get party
var mainParty = PartyManager.Instance.GetMainParty();
var subParty = PartyManager.Instance.GetSubParty();

// Manage party
PartyManager.Instance.AddToMainParty(characterData);
PartyManager.Instance.SwapToSubParty("char_id");
PartyManager.Instance.LockCharacter("char_id", true);

// Distribute exp
PartyManager.Instance.DistributeExperience(1000);
```

---

## Battle System

### Overview
A complete **Persona 5 Royal-style** turn-based combat system with advanced mechanics.

### Core Components

#### BattleManager.cs
**Location:** `Combat/BattleManager.cs`

Main battle controller handling all combat logic.

**Signals:**
```csharp
BattleStarted();
TurnStarted(BattleMember member);
ActionExecuted(BattleMember actor, BattleAction action, DamageResult result);
WeaknessExploited(BattleMember target, ElementType element);
EnemyKnockedDown(BattleMember enemy);
AllEnemiesKnockedDown();
OneMoreGranted(BattleMember member);
AllOutAttackAvailable();
BattleEnded(bool victory);
```

**Key Methods:**
```csharp
// Initialize battle
void InitializeBattle(
    List<CharacterStats> playerParty,
    List<CharacterStats> enemies,
    List<ShowtimeData> availableShowtimes = null
);

// Execute actions
void ExecuteAction(BattleAction action);

// Turn management
void NextTurn();
void SkipTurn();

// Special attacks
void ExecuteAllOutAttack();
bool CanAllOutAttack();
```

### Battle Mechanics

#### 1. **Weakness System**
- Hit enemy weakness → 1.5x damage
- Grants "One More" extra turn
- Knocks down enemy
- Enables All-Out Attack when all enemies down

**Implementation:**
```csharp
// Check weakness
var affinity = target.ElementAffinities.GetAffinity(skill.Element);
if (affinity == ElementAffinity.Weak)
{
    result.HitWeakness = true;
    result.DamageMultiplier *= 1.5f;
    // Grant One More
    actor.HasExtraTurn = true;
    // Knock down enemy
    target.KnockDown();
}
```

#### 2. **Baton Pass System**
**Location:** `Combat/BatonPassSystem.cs`

Pass "One More" turn to ally with stacking bonuses.

**Features:**
- Level 1: +50% damage
- Level 2: +100% damage
- Level 3: +150% damage
- Passes are stackable in a chain

**Usage:**
```csharp
// Check if can baton pass
bool canPass = BatonPassSystem.CanBatonPass(actor, target);

// Execute pass
BatonPassSystem.ExecuteBatonPass(actor, target);

// Clear at end of round
BatonPassSystem.ResetAllBatonPasses(allMembers);
```

#### 3. **Technical Damage**
**Location:** `Combat/TechnicalDamage.cs`

Bonus damage from combining status effects with specific elements.

**Combos:**
- Burn + Wind/Nuclear → Technical
- Freeze + Physical → Technical
- Shock + Physical → Technical
- Dizzy + Any Attack → Technical
- Sleep + Any Attack → Technical (wakes up)

**Usage:**
```csharp
// Apply status
TechnicalDamage.ApplyStatus(target, StatusEffectType.Burn);

// Check technical
bool isTech = TechnicalDamage.CheckTechnicalDamage(
    skill.Element, target
);

// Apply bonus
if (isTech)
{
    result.DamageMultiplier *= TechnicalDamage.TechnicalMultiplier;
    result.IsTechnical = true;
}
```

#### 4. **Showtime Attacks**
**Location:** `Combat/ShowtimeAttacks.cs`

Special duo attacks between character pairs.

**Features:**
- Random trigger chance
- Cooldown system
- High damage multiplier (3-4x)
- Requires both characters alive
- Consumes turn

**Usage:**
```csharp
// Create showtime data
var showtime = new ShowtimeData
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

// Check availability
bool canUse = ShowtimeAttacks.CanUseShowtime(showtime, member1, member2);

// Execute
ShowtimeAttacks.ExecuteShowtime(showtime, battleManager);
```

#### 5. **Limit Break System**
**Location:** `Combat/LimitBreakSystem.cs`

Ultimate attacks powered by Limit Gauge.

**Types:**
- **Solo Limit Break:** Single character ultimate
- **DUO Limit Break:** Pair ultimate (requires 2 full gauges)

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
LimitBreakSystem.BuildLimitGauge(member, 10);

// Check ready
bool ready = LimitBreakSystem.IsLimitBreakReady(member);

// Execute solo
LimitBreakSystem.ExecuteSoloLimitBreak(member, targetEnemy);

// Execute DUO
LimitBreakSystem.ExecuteDUOLimitBreak(member1, member2, enemies);
```

#### 6. **Guard System**
**Location:** `Combat/GuardSystem.cs`

Defensive stance reducing damage.

**Features:**
- 50% damage reduction
- Builds Limit Gauge
- Must reapply each turn
- Cannot act while guarding

**Usage:**
```csharp
// Apply guard
var guardAction = new BattleAction(member, BattleActionType.Guard);
battleManager.ExecuteAction(guardAction);

// Check if guarding
bool isGuarding = member.GuardState.IsGuarding;
```

#### 7. **Item System**
**Location:** `Combat/BattleItemSystem.cs`

Use consumable items in battle.

**Item Types:**
- Healing (restore HP)
- Recovery (restore MP)
- Revival (revive KO'd)
- Offensive (damage items)
- Status (cure/inflict status)

**Usage:**
```csharp
// Use item
var itemAction = new BattleAction(user, BattleActionType.Item)
{
    ItemData = healthPotion
};
itemAction = itemAction.WithTargets(ally);
battleManager.ExecuteAction(itemAction);
```

#### 8. **Escape System**
**Location:** `Combat/EscapeSystem.cs`

Flee from battle.

**Features:**
- Base 50% chance
- Modified by party speed vs enemy speed
- +10% per failed attempt
- Cannot escape boss battles

**Usage:**
```csharp
// Check if can escape
bool canEscape = battleManager.CanEscape();

// Attempt escape
var escapeAction = new BattleAction(member, BattleActionType.Escape);
battleManager.ExecuteAction(escapeAction);
```

### Battle Action Types

```csharp
public enum BattleActionType
{
    Attack,        // Basic physical attack
    Skill,         // Use skill/magic
    Guard,         // Defensive stance
    Item,          // Use consumable
    Escape,        // Flee battle
    AllOutAttack,  // Team finisher
    BatonPass,     // Pass turn to ally
    Showtime,      // Duo attack
    LimitBreak,    // Ultimate attack
    DUOLimit       // Pair ultimate
}
```

### Complete Battle Flow

```
1. Initialize Battle
   ↓
2. Calculate Turn Order (by Speed)
   ↓
3. ┌─→ Start Turn
   │   ↓
   │   Process Status Effects (poison, regen, etc.)
   │   ↓
   │   Player/Enemy Action Selection
   │   ↓
   │   Execute Action
   │   ↓
   │   Calculate Damage/Effects
   │   ↓
   │   Check Weakness/Critical
   │   ↓
   │   Grant One More? ──┐
   │   ↓                 │
   │   Check Technical   │
   │   ↓                 │
   │   Build Limit Gauge │
   │   ↓                 │
   │   Check Showtime    │
   │   ↓                 │
   │   All Enemies Down? │
   │   ↓                 │
   │   All-Out Attack?   │
   │   ↓                 │
   │   Check Battle End  │
   │   ↓                 │
   └───Next Turn ←───────┘
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

### Item Menu
**Location:** `UI/ItemMenuUI.cs`

Browse and use items from inventory.

**Features:**
- Filter by type (Consumable, Key, Material, Equipment)
- Use items on party members
- Sort inventory
- Item details display
- Discard items

### Skill Menu
**Location:** `UI/SkillMenuUI.cs`

View and equip character skills.

**Features:**
- Per-character skill management
- Equip/unequip skills
- Skill details (MP cost, power, element)
- Equipped vs available view
- Skill slot limits

### Equipment Menu
**Location:** `UI/EquipMenuUI.cs`

Manage character equipment.

**Features:**
- Equip weapons, armor, accessories
- Stat preview changes
- Per-character equipment
- Equipment requirements check
- Swap between inventory and character

### Status Menu
**Location:** `UI/StatusMenuUI.cs`

Detailed character information.

**Features:**
- Character portraits
- Level and experience
- Stats display
- Element affinities
- Equipment bonuses
- Active status effects

### Options Menu
**Location:** `UI/OptionsMenuUI.cs`

Game settings configuration.

**Features:**
- Audio volumes (BGM, SFX, Voice)
- Display settings (Fullscreen, VSync)
- Gameplay options (Text speed, Auto-save)
- Controls display
- Settings persistence

### Save/Load Menus
**Location:** `UI/SaveMenuUI.cs`, `UI/LoadMenuUI.cs`

Save and load game progress.

**Features:**
- 10 save slots
- Save previews
- Delete saves
- Confirmation dialogs
- Corrupted save handling

---

## Game Systems

### 1. Shop System
**Location:** `Shops/` (Autoload: ShopManager)

Complete buy/sell system with stock management.

**Features:**
- Buy/sell items
- Limited or unlimited stock
- Auto-restocking
- Item unlocking (level, quest, story)
- Featured items
- Transaction validation
- Save/load integration

**Creating Shops:**
```csharp
// Via resource file
// Create ShopData.tres in Godot editor
// Or via code:
var shop = new ShopData
{
    ShopId = "weapon_shop_01",
    ShopName = "Steel & Edge Armory",
    Description = "Fine weapons for discerning warriors",
    IsUnlocked = true
};

// Add items
shop.ItemsForSale.Add(new ShopItemData
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
ShopManager.Instance.BuyItem("weapon_shop_01", "iron_sword", 1);

// Sell item
ShopManager.Instance.SellItem("iron_sword", 5);
```

### 2. Quest System
**Location:** `Quests/` (Autoload: QuestManager)

Comprehensive quest tracking with objectives.

**Quest Types:**
- Main Quests
- Side Quests
- Time-limited quests

**Objective Types:**
- Defeat enemies
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

// Update objective
QuestManager.Instance.UpdateObjective("main_001", "obj_1", 1);

// Auto-tracking
QuestManager.Instance.OnEnemyDefeated("slime");
QuestManager.Instance.OnItemCollected("herb", 1);
QuestManager.Instance.OnNPCTalked("merchant");

// Complete quest
QuestManager.Instance.CompleteQuest("main_001");

// Open quest UI
var questUI = GetNode<QuestUI>("%QuestUI");
questUI.ShowQuestLog();
```

### 3. Bestiary System
**Location:** `Beastiary/` (Autoload: BestiaryManager)

Automatic enemy encyclopedia with progressive discovery.

**Features:**
- Auto-tracks encountered enemies
- Records weaknesses as discovered
- Tracks skills used by enemies
- Shows drop rates
- Completion percentage
- Discovery notifications
- Save/load integration

**Usage:**
```csharp
// Auto-tracking (via BestiaryIntegration)
// Enemies tracked automatically in battle

// Manual tracking
BestiaryManager.Instance.RecordEncounter("goblin", level: 5);
BestiaryManager.Instance.RecordDefeat("goblin");
BestiaryManager.Instance.RecordWeaknessDiscovered("goblin", ElementType.Fire);
BestiaryManager.Instance.RecordSkillDiscovered("goblin", "fireball");

// Query data
var entry = BestiaryManager.Instance.GetEntry("goblin");
bool discovered = entry.IsDiscovered;
int timesEncountered = entry.TimesEncountered;

// Completion
float completion = BestiaryManager.Instance.CompletionPercentage;

// Open UI
var bestiaryUI = GetNode<BestiaryUI>("%BestiaryUI");
bestiaryUI.ShowBestiary();
```

### 4. Skit System
**Location:** `Skits/` (Autoload: SkitManager)

Tales of Series-style character conversation scenes.

**Features:**
- 2D character portraits
- Emotion-based expressions
- Typewriter text effect
- Voice acting support
- Skippable scenes
- Auto and manual triggers
- Notification icons

**Creating Skits:**
```csharp
var skit = new SkitData
{
    SkitId = "skit_001",
    Title = "First Meeting",
    IsRepeatable = false
};

// Add dialogue
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

// Auto-trigger (place in world)
// Add SkitTrigger node to scene
// Set SkitId in inspector
// Trigger on enter/interact
```

### 5. Lore Codex
**Location:** `LoreCodex/` (Autoload: LoreCodexManager)

World-building encyclopedia for lore entries.

**Categories:**
- Characters
- Locations
- Events
- Concepts
- Items
- Organizations

**Features:**
- Progressive discovery
- Detailed entries with lore text
- Image support
- Search and filter
- Completion tracking
- Discovery notifications

**Usage:**
```csharp
// Create entry
var entry = new LoreCodexEntry
{
    EntryId = "ancient_city",
    Category = LoreCategory.Location,
    Title = "The Ancient City",
    ShortDescription = "A lost civilization",
    FullText = "Long ago, a great city...",
    IsDiscovered = false
};

LoreCodexManager.Instance.RegisterEntry(entry);

// Discover entry
LoreCodexManager.Instance.DiscoverEntry("ancient_city");

// Open UI
var codexUI = GetNode<LoreCodexUI>("%LoreCodexUI");
codexUI.ShowCodex();
```

### 6. Crafting System
**Location:** `Crafting/`

Item and equipment crafting with recipes.

**Features:**
- Recipe-based crafting
- Material requirements
- Skill level requirements
- Success rate system
- Batch crafting
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

// Add materials
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

### 7. Bond System
**Location:** `Bonds/` (Autoload: BondManager, RelationshipManager)

Character relationship and bond progression.

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

// Relationship between characters
RelationshipManager.Instance.SetRelationship(
    "aria", "dominic", RelationshipType.Friend
);
```

### 8. Encounter System
**Location:** `Encounters/` (Autoload: EncounterManager)

Random battle encounter system.

**Features:**
- Step-based encounters
- Encounter rate adjustment
- Area-specific enemy groups
- Encounter on/off toggle
- Custom encounter tables

**Usage:**
```csharp
// Configure encounters
EncounterManager.Instance.SetEncounterRate(20); // 20%
EncounterManager.Instance.SetEncountersEnabled(true);

// Add encounter table for area
var encounterTable = new List<EncounterData>
{
    new EncounterData { EnemyId = "goblin", Weight = 60 },
    new EncounterData { EnemyId = "slime", Weight = 30 },
    new EncounterData { EnemyId = "wolf", Weight = 10 }
};
EncounterManager.Instance.SetAreaEncounters("forest", encounterTable);

// On player movement
EncounterManager.Instance.OnPlayerMove();
```

---

## Save System

**Location:** `SaveSystem/`

Comprehensive save/load system with multiple slots.

### SaveData Structure

```csharp
public class SaveData
{
    // Core
    public string SaveName { get; set; }
    public DateTime SaveTime { get; set; }
    public float PlayTime { get; set; }
    
    // Player
    public string CurrentLocation { get; set; }
    public Vector2 PlayerPosition { get; set; }
    public int Gold { get; set; }
    
    // Party
    public List<PartyMemberSaveData> PartyMembers { get; set; }
    public List<string> MainPartyIds { get; set; }
    public List<string> SubPartyIds { get; set; }
    
    // Inventory
    public InventorySaveData Inventory { get; set; }
    
    // Equipment (per character)
    public Dictionary<string, CharacterEquipmentSaveData> Equipment { get; set; }
    
    // Systems
    public QuestSaveData Quests { get; set; }
    public Dictionary CustomData { get; set; } // For Bestiary, Lore, etc.
    
    // Flags
    public Dictionary<string, bool> StoryFlags { get; set; }
    public Dictionary<string, int> Variables { get; set; }
}
```

### Using Save System

```csharp
// Save game
SaveManager.Instance.SaveGame(slotIndex: 0, "My Save");

// Load game
SaveManager.Instance.LoadGame(slotIndex: 0);

// Check if save exists
bool exists = SaveManager.Instance.SaveExists(slotIndex: 0);

// Delete save
SaveManager.Instance.DeleteSave(slotIndex: 0);

// Get save info
SaveSlotInfo info = SaveManager.Instance.GetSaveSlotInfo(slotIndex: 0);
```

### Custom Save Data

For custom systems, use the `CustomData` dictionary:

```csharp
// Saving
public void CaptureCurrentState()
{
    var saveData = SaveManager.Instance.CurrentSave;
    
    // Add bestiary data
    var bestiaryData = BestiaryManager.Instance.GetSaveData();
    saveData.CustomData["Bestiary"] = Json.Stringify(bestiaryData);
    
    // Add lore codex
    var loreData = LoreCodexManager.Instance.GetSaveData();
    saveData.CustomData["LoreCodex"] = Json.Stringify(loreData);
}

// Loading
public void RestoreState()
{
    var saveData = SaveManager.Instance.CurrentSave;
    
    // Restore bestiary
    if (saveData.CustomData.ContainsKey("Bestiary"))
    {
        var json = saveData.CustomData["Bestiary"].AsString();
        BestiaryManager.Instance.LoadSaveData(json);
    }
    
    // Restore lore
    if (saveData.CustomData.ContainsKey("LoreCodex"))
    {
        var json = saveData.CustomData["LoreCodex"].AsString();
        LoreCodexManager.Instance.LoadSaveData(json);
    }
}
```

---

## Integration Guide

### Integrating Battle System with Your Game

1. **Add BattleManager to Scene**
```gdscript
# BattleScene.tscn
Node2D (BattleScene)
├── BattleManager (BattleManager.cs)
├── UI/
│   ├── BattleUI
│   └── ActionMenu
└── Visual/
    └── BattleBackground
```

2. **Start Battle from Overworld**
```csharp
// In your overworld/encounter code
private void StartBattle(List<CharacterData> enemies)
{
    // Get player party
    var playerParty = PartyManager.Instance.GetMainPartyStats();
    
    // Create enemy stats
    var enemyStats = enemies.Select(e => e.ToCharacterStats()).ToList();
    
    // Create showtime pairs
    var showtimes = GetAvailableShowtimes();
    
    // Load battle scene
    GetTree().ChangeSceneToFile("res://Battle/BattleScene.tscn");
    
    // After scene loaded, initialize battle
    var battleManager = GetNode<BattleManager>("/root/BattleScene/BattleManager");
    battleManager.InitializeBattle(playerParty, enemyStats, showtimes);
}
```

3. **Handle Battle End**
```csharp
// Connect to battle end signal
battleManager.BattleEnded += OnBattleEnded;

private void OnBattleEnded(bool victory)
{
    if (victory)
    {
        // Award exp, gold, items
        var totalExp = CalculateExpReward();
        PartyManager.Instance.DistributeExperience(totalExp);
        
        var gold = CalculateGoldReward();
        InventorySystem.Instance.AddGold(gold);
        
        // Award items
        foreach (var drop in battleDrops)
        {
            InventorySystem.Instance.AddItem(drop.ItemId, drop.Quantity);
        }
    }
    
    // Return to overworld
    GetTree().ChangeSceneToFile("res://Overworld/OverworldScene.tscn");
}
```

### Connecting Shop System

1. **Add Shop to NPC**
```gdscript
# NPC Scene
CharacterBody2D (NPC)
├── Sprite2D
├── InteractionArea (Area2D)
│   └── CollisionShape2D
└── ShopTrigger (ShopTrigger.cs)
    └── Export: ShopToOpen = "res://Data/Shops/WeaponShop.tres"
```

2. **Create Shop Resource**
```
1. Right-click in FileSystem
2. New Resource → ShopData
3. Configure:
   - Shop ID: "weapon_shop_01"
   - Shop Name: "Steel & Edge"
   - Items For Sale: [Add items]
4. Save as .tres file
```

### Implementing Bestiary Auto-Tracking

1. **Add BestiaryIntegration as Autoload**
   Already done in project.godot

2. **Connect Battle Signals**
```csharp
// In BattleManager._Ready()
BattleStarted += BestiaryIntegration.Instance.OnBattleStarted;
ActionExecuted += BestiaryIntegration.Instance.OnActionExecuted;
EnemyKnockedDown += BestiaryIntegration.Instance.OnEnemyKnockedDown;
```

3. **Initialize Database**
```csharp
// In game init
BestiaryManager.Instance.InitializeFromDatabase(
    GameManager.Instance.Database
);
```

### Quest Integration

1. **Place Quest Givers**
```gdscript
# NPC with quest
NPC (CharacterBody2D)
└── NPCQuestGiver (NPCQuestGiver.cs)
    └── Exports:
        - NPC ID: "merchant_marcus"
        - NPC Name: "Marcus"
        - Quests To Give: [quest_resources]
```

2. **Track Quest Progress**
```csharp
// When relevant events happen
QuestManager.Instance.OnEnemyDefeated("goblin");
QuestManager.Instance.OnItemCollected("herb", 1);
QuestManager.Instance.OnNPCTalked("merchant");
QuestManager.Instance.OnLocationReached("ancient_ruins");
```

3. **Quest Rewards**
```csharp
// Connect to quest completed signal
QuestManager.Instance.QuestCompleted += OnQuestCompleted;

private void OnQuestCompleted(QuestData quest)
{
    // Rewards automatically added by QuestManager
    GD.Print($"Quest completed: {quest.QuestName}");
    
    // Show reward notification
    ShowNotification($"Quest Complete! +{quest.Rewards.Gold} gold");
}
```

---

## API Reference

### Quick Reference Tables

#### Battle System

| Method | Description |
|--------|-------------|
| `BattleManager.InitializeBattle()` | Start new battle |
| `BattleManager.ExecuteAction()` | Execute battle action |
| `BattleManager.NextTurn()` | Advance to next turn |
| `BattleManager.CanAllOutAttack()` | Check if AOA available |
| `BattleManager.ExecuteAllOutAttack()` | Perform All-Out Attack |
| `BatonPassSystem.CanBatonPass()` | Check if can baton pass |
| `BatonPassSystem.ExecuteBatonPass()` | Pass turn to ally |
| `TechnicalDamage.CheckTechnicalDamage()` | Check for technical |
| `ShowtimeAttacks.CanUseShowtime()` | Check showtime availability |
| `ShowtimeAttacks.ExecuteShowtime()` | Use showtime attack |
| `LimitBreakSystem.BuildLimitGauge()` | Add to limit gauge |
| `LimitBreakSystem.ExecuteSoloLimitBreak()` | Use solo ultimate |
| `LimitBreakSystem.ExecuteDUOLimitBreak()` | Use pair ultimate |

#### Party System

| Method | Description |
|--------|-------------|
| `PartyManager.GetMainParty()` | Get main party members |
| `PartyManager.GetSubParty()` | Get reserve members |
| `PartyManager.AddToMainParty()` | Add to main party |
| `PartyManager.AddToSubParty()` | Add to reserve |
| `PartyManager.SwapToMainParty()` | Move to main party |
| `PartyManager.SwapToSubParty()` | Move to reserve |
| `PartyManager.RemoveFromParty()` | Remove from party |
| `PartyManager.LockCharacter()` | Lock/unlock character |
| `PartyManager.DistributeExperience()` | Award battle exp |

#### Inventory System

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
| `InventorySystem.IsFull()` | Check if full |

#### Equipment System

| Method | Description |
|--------|-------------|
| `EquipmentManager.EquipFromInventory()` | Equip item |
| `EquipmentManager.UnequipToInventory()` | Unequip item |
| `EquipmentManager.GetEquippedItem()` | Get equipped in slot |
| `EquipmentManager.GetCharacterEquipment()` | Get all equipment |
| `EquipmentManager.GetCharacterBonuses()` | Get stat bonuses |
| `EquipmentManager.ApplyEquipmentBonuses()` | Apply to stats |
| `EquipmentManager.UnequipAll()` | Remove all equipment |

#### Quest System

| Method | Description |
|--------|-------------|
| `QuestManager.RegisterQuest()` | Add quest to system |
| `QuestManager.StartQuest()` | Begin quest |
| `QuestManager.CompleteQuest()` | Complete quest |
| `QuestManager.FailQuest()` | Fail quest |
| `QuestManager.UpdateObjective()` | Manual objective update |
| `QuestManager.OnEnemyDefeated()` | Auto-track enemy |
| `QuestManager.OnItemCollected()` | Auto-track item |
| `QuestManager.OnNPCTalked()` | Auto-track NPC |
| `QuestManager.OnLocationReached()` | Auto-track location |
| `QuestManager.GetQuest()` | Get quest data |
| `QuestManager.GetActiveQuests()` | Get active quests |
| `QuestManager.GetCompletedQuests()` | Get completed |

#### Shop System

| Method | Description |
|--------|-------------|
| `ShopManager.RegisterShop()` | Add shop to system |
| `ShopManager.OpenShop()` | Open shop UI |
| `ShopManager.CloseShop()` | Close shop UI |
| `ShopManager.BuyItem()` | Purchase item |
| `ShopManager.SellItem()` | Sell item to shop |
| `ShopManager.CanAfford()` | Check if can buy |
| `ShopManager.UnlockShop()` | Unlock shop |
| `ShopManager.UnlockItem()` | Unlock item |
| `ShopManager.RestockShop()` | Refill stock |

#### Bestiary System

| Method | Description |
|--------|-------------|
| `BestiaryManager.RecordEncounter()` | Log enemy encounter |
| `BestiaryManager.RecordDefeat()` | Log enemy defeat |
| `BestiaryManager.RecordWeaknessDiscovered()` | Log weakness |
| `BestiaryManager.RecordSkillDiscovered()` | Log skill |
| `BestiaryManager.GetEntry()` | Get bestiary entry |
| `BestiaryManager.CompletionPercentage` | Get completion % |
| `BestiaryManager.UnlockAll()` | Debug: unlock all |

#### Skit System

| Method | Description |
|--------|-------------|
| `SkitManager.RegisterSkit()` | Add skit |
| `SkitManager.PlaySkit()` | Play skit scene |
| `SkitManager.CanPlaySkit()` | Check if can play |
| `SkitManager.MarkAsViewed()` | Mark skit viewed |
| `SkitManager.IsSkitAvailable()` | Check availability |

#### Save System

| Method | Description |
|--------|-------------|
| `SaveManager.SaveGame()` | Save to slot |
| `SaveManager.LoadGame()` | Load from slot |
| `SaveManager.SaveExists()` | Check if save exists |
| `SaveManager.DeleteSave()` | Delete save |
| `SaveManager.GetSaveSlotInfo()` | Get save preview |
| `SaveManager.AutoSave()` | Auto-save |

---

## Common Patterns & Examples

### Starting a New Game

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
    
    // Initialize party
    var startingCharacters = new[] { "aria", "dominic", "echo_walker" };
    foreach (var charId in startingCharacters)
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
    BestiaryManager.Instance.InitializeFromDatabase(GameManager.Instance.Database);
    
    // Start game
    GetTree().ChangeSceneToFile("res://Scenes/Overworld.tscn");
}
```

### Implementing Enemy AI

```csharp
public BattleAction DecideEnemyAction(BattleMember enemy, List<BattleMember> allies, List<BattleMember> players)
{
    // Check HP - heal if low
    if (enemy.Stats.CurrentHP < enemy.Stats.MaxHP * 0.3f)
    {
        var healSkill = enemy.Stats.Skills.GetEquippedSkills()
            .FirstOrDefault(s => s.TargetType == TargetType.Self && s.Power < 0);
        
        if (healSkill != null)
        {
            return new BattleAction(enemy, BattleActionType.Skill)
            {
                Skill = healSkill
            }.WithTargets(enemy);
        }
    }
    
    // Use strongest skill on player weakness
    var playerWeaknesses = players
        .Where(p => p.Stats.IsAlive)
        .SelectMany(p => GetPlayerWeaknesses(p))
        .ToList();
    
    if (playerWeaknesses.Any())
    {
        var skill = enemy.Stats.Skills.GetEquippedSkills()
            .Where(s => playerWeaknesses.Contains(s.Element))
            .OrderByDescending(s => s.Power)
            .FirstOrDefault();
        
        if (skill != null)
        {
            var target = players.First(p => p.Stats.IsAlive);
            return new BattleAction(enemy, BattleActionType.Skill)
            {
                Skill = skill
            }.WithTargets(target);
        }
    }
    
    // Default: basic attack random player
    var randomTarget = players
        .Where(p => p.Stats.IsAlive)
        .OrderBy(x => GD.Randf())
        .First();
    
    return new BattleAction(enemy, BattleActionType.Attack)
        .WithTargets(randomTarget);
}

private List<ElementType> GetPlayerWeaknesses(BattleMember player)
{
    var weaknesses = new List<ElementType>();
    
    for (int i = 0; i < 8; i++)
    {
        var element = (ElementType)i;
        var affinity = player.Stats.ElementAffinities.GetAffinity(element);
        
        if (affinity == ElementAffinity.Weak)
        {
            weaknesses.Add(element);
        }
    }
    
    return weaknesses;
}
```

### Creating Dynamic Quests

```csharp
public void CreateDynamicQuest(string enemyType, int count)
{
    var quest = new QuestData
    {
        QuestId = $"hunt_{enemyType}_{DateTime.Now.Ticks}",
        QuestName = $"Hunt {count} {enemyType}s",
        Description = $"The village needs help dealing with {enemyType}s!",
        Type = QuestType.Side,
        IsRepeatable = true,
        AutoTrack = true
    };
    
    // Add defeat objective
    quest.Objectives.Add(new QuestObjective
    {
        ObjectiveId = "defeat",
        Description = $"Defeat {count} {enemyType}s",
        Type = ObjectiveType.DefeatEnemy,
        TargetId = enemyType,
        RequiredCount = count,
        CurrentCount = 0
    });
    
    // Calculate rewards based on difficulty
    int baseGold = 50;
    int baseExp = 30;
    quest.Rewards.Gold = baseGold * count;
    quest.Rewards.Experience = baseExp * count;
    
    // Register and start
    QuestManager.Instance.RegisterQuest(quest);
    QuestManager.Instance.StartQuest(quest.QuestId);
    
    GD.Print($"Created dynamic quest: {quest.QuestName}");
}
```

### Custom Battle Victory Conditions

```csharp
private void CheckCustomVictoryCondition()
{
    // Example: Defeat boss within time limit
    if (battleType == BattleType.TimedBoss)
    {
        if (turnCount > MAX_TURNS)
        {
            // Time limit exceeded - player loses
            EndBattle(victory: false, reason: "Time limit exceeded!");
            return;
        }
    }
    
    // Example: Survive X turns
    if (battleType == BattleType.Survival)
    {
        if (turnCount >= SURVIVAL_TURNS)
        {
            // Survived long enough - win
            EndBattle(victory: true, reason: "Survived!");
            return;
        }
    }
    
    // Example: Protect NPC
    if (battleType == BattleType.Escort)
    {
        var npc = GetNPCToProtect();
        if (!npc.Stats.IsAlive)
        {
            // NPC died - lose
            EndBattle(victory: false, reason: "Failed to protect NPC");
            return;
        }
    }
    
    // Standard victory condition
    if (AreAllEnemiesDead())
    {
        EndBattle(victory: true);
    }
}
```

---

## Final Notes

### Project Statistics

- **Total C# Files:** 100+
- **Total Lines of Code:** ~15,000+
- **Battle Mechanics:** 17+
- **Game Systems:** 10+
- **UI Menus:** 12+
- **Test Scenes:** 5+
- **Documentation Files:** 15+

### Production Status

✅ **COMPLETE SYSTEMS:**
- Battle System (Persona 5 Royal-style)
- Main Menu & UI
- Shop System
- Quest System
- Bestiary System
- Skit System
- Save/Load System
- Inventory & Equipment
- Party Management

🔧 **NEEDS POLISH:**
- Battle UI animations
- Transition effects
- Balance tuning
- Content creation (enemies, quests, shops)

📝 **OPTIONAL ENHANCEMENTS:**
- AI system for enemies
- Skill trees
- Achievement system
- New Game+
- Difficulty modes
- Tutorial system

### Performance Considerations

- Battle system is event-driven (no polling)
- Inventory uses dictionary lookups (O(1))
- Quest system uses efficient objective tracking
- Save system uses JSON serialization
- All managers are singletons (efficient access)

### Best Practices

1. **Always initialize databases before systems**
2. **Connect signals in _Ready()**
3. **Use unique names for UI nodes (%NodeName)**
4. **Save frequently during gameplay**
5. **Test individual systems with test scenes**
6. **Use proper error handling**
7. **Follow consistent naming conventions**

### Next Steps for Development

1. **Create Battle UI** - Action menus, animations
2. **Design Content** - Enemies, quests, shops
3. **Build World** - Maps, NPCs, areas
4. **Balance Game** - Stats, prices, difficulty
5. **Add Polish** - Effects, sounds, transitions
6. **Playtest** - Iterate and refine

---

## Support & Resources

### Documentation Files
All documentation is in the `Docs/` folder:
- Battle system guides
- UI system guides
- Shop, Quest, Bestiary guides
- Integration examples

### Test Scenes
Use test scenes to understand systems:
- `BattleTest.cs` - Basic battle
- `Phase2BattleTest.cs` - Advanced battle
- `CoreCommandsTest.cs` - Guard/Item/Escape
- System-specific test scenes

### Debugging Tips

**Battle Issues:**
- Check signals are connected
- Verify character stats are valid
- Use GD.Print() to trace execution
- Test with simple scenarios first

**Save/Load Issues:**
- Verify all data is serializable
- Check file permissions
- Test with new saves first
- Use try-catch for error handling

**UI Issues:**
- Verify unique names are set
- Check node paths are correct
- Ensure scripts are attached
- Test input actions in Project Settings

---

## Conclusion

This project represents a **complete, production-ready JRPG framework** with professional-grade systems comparable to AAA titles like Persona 5 Royal. The architecture is modular, extensible, and well-documented.

**You have everything you need to create an amazing JRPG!** 🎮⚔️✨

### Quick Commands Cheat Sheet

```csharp
// Start battle
battleManager.InitializeBattle(party, enemies, showtimes);

// Party management
PartyManager.Instance.AddToMainParty(character);

// Inventory
InventorySystem.Instance.AddItem("item_id", quantity);

// Equipment
EquipmentManager.Instance.EquipFromInventory("char_id", "item_id", character);

// Quests
QuestManager.Instance.StartQuest("quest_id");

// Shops
ShopManager.Instance.OpenShop("shop_id");

// Bestiary
BestiaryManager.Instance.RecordEncounter("enemy_id");

// Save
SaveManager.Instance.SaveGame(slotIndex, "Save Name");
```

Happy game development! 🚀