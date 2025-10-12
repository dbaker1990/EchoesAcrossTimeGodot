# Echoes Across Time - Complete System Documentation

## 📋 Table of Contents
1. [Project Overview](#project-overview)
2. [System Architecture](#system-architecture)
3. [Getting Started](#getting-started)
4. [Core Systems](#core-systems)
5. [Battle System](#battle-system)
6. [Advanced AI Systems](#advanced-ai-systems)
7. [RPG Systems](#rpg-systems)
8. [UI & Menu Systems](#ui--menu-systems)
9. [Audio & Visual Systems](#audio--visual-systems)
10. [Event & Dialogue System](#event--dialogue-system)
11. [Overworld Systems](#overworld-systems)
12. [Database System](#database-system)
13. [Save System](#save-system)
14. [API Reference](#api-reference)
15. [Development Guide](#development-guide)

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
- **Advanced AI systems** with pattern learning and adaptive behavior

#### RPG Systems
- 🏪 **Shop System** - Buy/sell with stock management
- 📜 **Quest System** - Main/side quests with objectives and rewards
- 📖 **Bestiary** - Progressive enemy discovery system
- 🎭 **Skit System** - Tales-style character conversations
- 📚 **Lore Codex** - World-building encyclopedia
- 🔗 **Bond/Relationship System** - Character connections and romance
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
- 📊 **Database system** for all game data
- 🎮 **HUD Manager** for on-screen display
- 🎨 **Retro Effects** with shader-based visual filters

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
PartyManager           - Party composition and roster
SkitManager            - Character skit conversations
AudioManager           - BGM, SFX, and voice management
BestiaryManager        - Enemy encyclopedia
EncounterManager       - Random battle encounters
BondManager            - Character bond progression
RelationshipManager    - Character relationships
ShopManager            - Shop system coordination
LoreCodexManager       - Lore encyclopedia management
HudManager             - HUD display coordination
RetroEffectsManager    - Visual shader effects
FPSCounter             - Performance monitoring
```

### Project Structure

```
EchoesAcrossTime/
│
├── Audio/                          # Audio system
│   ├── AudioManager.cs             # (Autoload)
│   ├── BGM/                        # Background music
│   └── SFX/                        # Sound effects
│
├── Beastiary/                      # Bestiary system
│   ├── BestiaryManager.cs          # (Autoload)
│   ├── BestiaryData.cs
│   ├── BestiaryUI.cs
│   └── BestiaryNotification.tscn
│
├── Bonds/                          # Bond/Relationship system
│   ├── BondManager.cs              # (Autoload)
│   ├── RelationshipManager.cs      # (Autoload)
│   ├── BondData.cs
│   └── BondConfig.tres
│
├── Combat/                         # Battle system
│   ├── BattleManager.cs
│   ├── CharacterStats.cs
│   ├── SkillData.cs
│   ├── TechnicalDamage.cs
│   ├── ShowtimeAttacks.cs
│   ├── LimitBreakSystem.cs
│   ├── BatonPassManager.cs
│   ├── AllOutAttackSystem.cs
│   └── Advance/                    # Advanced AI
│       ├── AdvancedAIPattern.cs
│       ├── AdvancedAIDebugger.cs
│       ├── AdvancedAIDirector.cs
│       └── AdvancedAIAchievements.cs
│
├── Crafting/                       # Crafting system
│   ├── CraftingSystem.cs
│   ├── CraftingRecipeData.cs
│   └── CraftingUI.cs
│
├── Database/                       # Database system
│   ├── GameDatabase.cs
│   ├── CharacterDatabase.cs
│   └── CharacterDatabase.tres
│
├── Encounters/                     # Encounter system
│   ├── EncounterManager.cs         # (Autoload)
│   └── EncounterZone.cs
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
│   ├── LoreCodexManager.cs         # (Autoload)
│   └── LoreCodexUI.cs
│
├── Managers/                       # Core managers
│   ├── SystemManager.cs            # (Autoload)
│   ├── GameManager.cs
│   └── PartyManager.cs             # (Autoload)
│
├── Quests/                         # Quest system
│   ├── QuestData.cs
│   ├── QuestManager.cs             # (Autoload)
│   └── QuestUI.cs
│
├── SaveSystem/                     # Save/Load system
│   ├── SaveManager.cs
│   ├── SaveData.cs
│   └── SaveData_Extensions.cs
│
├── Shops/                          # Shop system
│   ├── ShopData.cs
│   ├── ShopManager.cs              # (Autoload)
│   └── ShopUI.cs
│
├── Skits/                          # Character skits
│   ├── SkitData.cs
│   ├── SkitManager.cs              # (Autoload)
│   └── SkitUI.cs
│
├── UI/                             # User interface
│   ├── HUDManager.cs               # (Autoload)
│   ├── RetroEffectsManager.cs      # (Autoload)
│   ├── FPSCounter.cs               # (Autoload)
│   ├── MainMenuUI.cs
│   ├── ItemMenuUI.cs
│   ├── SkillMenuUI.cs
│   ├── EquipMenuUI.cs
│   ├── StatusMenuUI.cs
│   ├── OptionsMenuUI.cs
│   ├── SaveMenuUI.cs
│   ├── LoadMenuUI.cs
│   ├── PartyMenuUI.cs
│   └── TitleScreenOptionsUI.cs
│
└── Characters/                     # Overworld control
    ├── OverworldCharacter.cs
    ├── PlayerCharacter.cs
    ├── FollowerCharacter.cs
    └── States/
        ├── IdleState.cs
        ├── WalkingState.cs
        ├── RunningState.cs
        ├── JumpingState.cs
        ├── ClimbingState.cs
        ├── PullingState.cs
        ├── PushingState.cs
        ├── LockedState.cs
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

**Core Features:**
- Turn-based combat system
- Element system (8 types: Physical, Fire, Ice, Electric, Wind, Light, Dark, Almighty)
- Weakness exploitation → One More mechanic
- Status effects
- Battle flow management

**Key Methods:**
```csharp
// Initialize battle
BattleManager.InitializeBattle(
    List<CharacterStats> playerParty, 
    List<CharacterStats> enemies,
    List<ShowtimePair> showtimePairs
);

// Execute action
BattleManager.ExecuteAction(BattleAction action);

// Advance turn
BattleManager.NextTurn();

// Check battle end
bool isOver = BattleManager.IsBattleOver();
```

### Advanced Mechanics

#### 1. Baton Pass System
**Location:** `Combat/BatonPassManager.cs`

Strategic turn-passing system with stacking bonuses.

**Features:**
- Chain bonus multipliers (1.2x → 1.4x → 1.6x)
- Requires weakness exploitation or critical hit
- Can only pass to allies who haven't acted
- Visual chain indicators

**Usage:**
```csharp
// Check if can baton pass
bool canPass = BatonPassManager.CanBatonPass(currentChar, targetChar);

// Execute baton pass
BatonPassManager.ExecuteBatonPass(currentChar, targetChar);

// Get current bonus
float bonus = BatonPassManager.GetCurrentBonus();

// Reset chain
BatonPassManager.ResetChain();
```

#### 2. Technical Damage System
**Location:** `Combat/TechnicalDamage.cs`

Advanced combo system combining status effects with elements.

**Technical Combos:**
- **Burn** + Ice/Wind → Technical damage
- **Frozen** + Physical/Fire → Technical damage
- **Shocked** + Physical/Fire → Technical damage
- **Dizzy** + Any attack → Technical damage

**Usage:**
```csharp
// Check for technical
var result = TechnicalDamage.CheckTechnicalDamage(
    target,
    attackElement
);

if (result.IsTechnical)
{
    float damage = baseDamage * result.DamageMultiplier; // 1.5x
    // Apply technical effects
}
```

#### 3. Showtime Attacks
**Location:** `Combat/ShowtimeAttacks.cs`

Cinematic duo attacks between specific character pairs.

**Features:**
- Character pair relationships
- Gauge buildup system
- Unique animations per pair
- Powerful combined attacks

**Usage:**
```csharp
// Define showtime pair
var pair = new ShowtimePair
{
    CharacterA = "aria",
    CharacterB = "dominic",
    ShowtimeName = "Eternal Duet",
    ShowtimeId = "aria_dominic_showtime",
    RequiredGauge = 100
};

// Check availability
bool canUse = ShowtimeAttacks.CanUseShowtime(pair);

// Execute showtime
ShowtimeAttacks.ExecuteShowtime(pair, targets);
```

#### 4. Limit Break System
**Location:** `Combat/LimitBreakSystem.cs`

Ultimate attack system with solo and duo variants.

**Features:**
- Individual limit gauges (max 100)
- Solo Limit Breaks (requires 100 gauge)
- DUO Limit Breaks (requires both characters at 100)
- Gauge building from taking/dealing damage

**Usage:**
```csharp
// Build gauge
LimitBreakSystem.BuildLimitGauge(character, amount);

// Check if ready
bool ready = LimitBreakSystem.IsLimitBreakReady(character);

// Execute solo limit
LimitBreakSystem.ExecuteSoloLimit(character, target);

// Execute DUO limit
LimitBreakSystem.ExecuteDuoLimit(charA, charB, targets);
```

#### 5. All-Out Attack
**Location:** `Combat/AllOutAttackSystem.cs`

Finisher when all enemies are knocked down.

**Trigger Conditions:**
- All enemies must be in "Down" state
- At least one player character must have acted
- Player chooses to initiate

**Usage:**
```csharp
// Check if available
bool canAOA = AllOutAttackSystem.CanAllOutAttack(enemies);

// Execute All-Out Attack
AllOutAttackSystem.ExecuteAllOutAttack(
    playerParty, 
    enemies,
    leadCharacter
);
```

---

## Advanced AI Systems

### Overview

Sophisticated enemy AI that learns from player behavior and adapts strategies over time.

### 1. Advanced AI Pattern
**Location:** `Combat/Advance/AdvancedAIPattern.cs`

Core AI decision-making system.

**Features:**
- **Pattern Recognition** - Learns player habits
- **Adaptive Behavior** - Changes strategy based on battle conditions
- **Priority Targeting** - Intelligent target selection
- **Skill Selection** - Context-aware ability usage
- **Learning System** - Improves over multiple battles

**AI Systems:**
- Pattern learning and prediction
- Target priority analysis
- Skill effectiveness tracking
- Behavior adaptation
- Memory persistence

**Usage:**
```csharp
// Initialize AI
var ai = new AdvancedAIPattern();

// Record player action for learning
ai.RecordPlayerAction(character, skill, target);

// Get AI decision
var action = ai.DecideAction(
    aiCharacter,
    playerParty,
    enemyParty,
    turnNumber
);
```

### 2. Advanced AI Debugger
**Location:** `Combat/Advance/AdvancedAIDebugger.cs`

Real-time debugging overlay for AI systems.

**Features:**
- Visual AI state display
- Decision logging
- Pattern analysis
- System status tracking
- Performance metrics

**Usage:**
```csharp
// Attach debugger to AI
advancedAIDebugger.AttachToAI(aiPattern);

// Log decisions
advancedAIDebugger.LogDecision(
    "Attack Aria",
    "High HP target, vulnerable to fire"
);

// Toggle with F3 key in-game
```

### 3. AI Director
**Location:** `Combat/Advance/AdvancedAIDirector.cs`

Coordinates all AI behaviors and manages difficulty.

**Features:**
- Dynamic difficulty adjustment
- AI coordination between enemies
- Behavior scheduling
- Learning rate management

### 4. AI Achievements
**Location:** `Combat/Advance/AdvancedAIAchievements.cs`

Tracks player interactions with AI systems.

**Achievements:**
- "Predicted!" - Get countered by AI prediction
- "Read Like a Book" - Get countered 10 times
- "They're Learning!" - AI learns your patterns

---

## RPG Systems

### 1. Shop System
**Location:** `Shops/` (Autoload: ShopManager)

Complete merchant and trading system.

**Features:**
- Multiple shop types (General, Weapon, Armor, Item)
- Stock management and restocking
- Buy/sell mechanics
- Price adjustments
- Shop availability conditions

**Creating Shops:**
```csharp
var shop = new ShopData
{
    ShopId = "general_store",
    ShopName = "Merchant's Emporium",
    ShopType = ShopType.General,
    BuyPriceMultiplier = 1.0f,
    SellPriceMultiplier = 0.5f
};

// Add items to shop
shop.ItemsForSale.Add(new ShopItem
{
    ItemId = "health_potion",
    Stock = 10,
    RestockAmount = 5
});

ShopManager.Instance.RegisterShop(shop);
```

**Using Shops:**
```csharp
// Open shop
ShopManager.Instance.OpenShop("general_store");

// Buy item
bool success = ShopManager.Instance.BuyItem("health_potion", quantity: 3);

// Sell item
bool success = ShopManager.Instance.SellItem("old_sword", quantity: 1);

// Restock shop
ShopManager.Instance.RestockShop("general_store");
```

### 2. Quest System
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

### 3. Bestiary System
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

### 4. Skit System
**Location:** `Skits/` (Autoload: SkitManager)

Tales of Series-style optional character conversations.

**Features:**
- Triggered by events or locations
- Character portraits and expressions
- Branching dialogue
- Voice acting support
- Skippable content

**Creating Skits:**
```csharp
var skit = new SkitData
{
    SkitId = "skit_001",
    SkitTitle = "First Meeting",
    TriggerLocation = "town_square",
    RequiredCharacters = new List<string> { "aria", "dominic" }
};

// Add dialogue
skit.DialogueLines.Add(new SkitLine
{
    CharacterId = "aria",
    Text = "This town seems peaceful.",
    Expression = "happy",
    VoiceClip = "aria_skit001_01.ogg"
});

SkitManager.Instance.RegisterSkit(skit);
```

**Using Skits:**
```csharp
// Play skit
SkitManager.Instance.PlaySkit("skit_001");

// Check if available
bool canPlay = SkitManager.Instance.CanPlaySkit("skit_001");

// Mark as viewed
SkitManager.Instance.MarkSkitAsViewed("skit_001");
```

### 5. Lore Codex System
**Location:** `LoreCodex/` (Autoload: LoreCodexManager)

Comprehensive lore encyclopedia.

**Categories:**
- Characters
- Locations
- Events
- Items
- Monsters

**Features:**
- Progressive unlocking
- Category organization
- Search functionality
- Reading tracking

**Usage:**
```csharp
// Discover entry
LoreCodexManager.Instance.DiscoverEntry("char_aria_bio");

// Check if discovered
bool discovered = LoreCodexManager.Instance.IsDiscovered("char_aria_bio");

// Get entries by category
var entries = LoreCodexManager.Instance.GetEntriesByCategory(
    LoreCategory.Characters
);

// Open UI
var codexUI = GetNode<LoreCodexUI>("%LoreCodexUI");
codexUI.ShowCodex();
```

### 6. Crafting System
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

### 7. Bond System
**Location:** `Bonds/` (Autoload: BondManager, RelationshipManager)

Character relationship and bond progression system.

**Features:**
- Bond levels (1-10)
- Bond points accumulation
- Relationship tracking between characters
- Bond events and rewards
- Relationship types (Friend, Rival, Romance, etc.)
- Daily point caps
- Save/load integration

**Usage:**
```csharp
// Add bond points
BondManager.Instance.AddBondPoints("aria", "dominic", 10);

// Check bond level
int level = BondManager.Instance.GetBondLevel("aria", "dominic");

// Get bond rank
BondRank rank = BondManager.Instance.GetRank("aria", "dominic");

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

// Convenience hooks
BondManager.Instance.OnAllyHealed("aria", "dominic");
BondManager.Instance.OnGuarded("aria", "dominic");
BondManager.Instance.OnRevive("aria", "dominic");
```

### 8. Encounter System
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

### 9. Inventory & Equipment
**Location:** `Items/`

Complete item management system.

**Features:**
- Item storage with stacking
- Equipment management
- Consumable items
- Material items
- Gold tracking

**Usage:**
```csharp
// Inventory
InventorySystem.Instance.AddItem("health_potion", quantity: 5);
InventorySystem.Instance.RemoveItem("health_potion", quantity: 1);
InventorySystem.Instance.AddGold(100);

// Equipment
EquipmentManager.Instance.EquipFromInventory(
    "aria",
    "iron_sword",
    characterStats
);
EquipmentManager.Instance.UnequipItem("aria", EquipSlot.Weapon);
```

---

## Audio & Visual Systems

### 1. Audio Manager
**Location:** `Audio/AudioManager.cs` (Autoload)

Centralized audio playback system.

**Features:**
- BGM (Background Music) control
- SFX (Sound Effects) playback
- Voice clip playback
- Volume control
- Fade in/out
- Audio ducking

**Usage:**
```csharp
// Play BGM
AudioManager.Instance.PlayBGM("battle_theme");
AudioManager.Instance.StopBGM();
AudioManager.Instance.FadeBGM(targetVolume: 0.5f, duration: 2.0f);

// Play SFX
AudioManager.Instance.PlaySFX("sword_slash");
AudioManager.Instance.PlaySFX("explosion", volume: 0.8f);

// Play Voice
AudioManager.Instance.PlayVoice("aria_attack_01");

// Volume control
AudioManager.Instance.SetBGMVolume(0.7f);
AudioManager.Instance.SetSFXVolume(0.8f);
```

### 2. HUD Manager
**Location:** `UI/HUDManager.cs` (Autoload)

On-screen display management.

**Features:**
- Health/MP bars
- Minimap display
- Quest tracker
- Status indicators
- Notification system

**Usage:**
```csharp
// Show/hide HUD
HudManager.Instance.ShowHUD();
HudManager.Instance.HideHUD();

// Update party display
HudManager.Instance.UpdatePartyDisplay(partyMembers);

// Show notification
HudManager.Instance.ShowNotification("Quest Complete!");
```

### 3. Retro Effects Manager
**Location:** `UI/RetroEffectsManager.cs` (Autoload)

Shader-based visual filter system.

**Available Effects:**
- Pixelation
- Scanlines
- Dithering
- VHS distortion
- RGB Glitch
- Color Palette reduction
- Combined effects

**Usage:**
```csharp
// Set single effect
RetroEffectsManager.Instance.SetEffect(RetroEffectType.Pixelation);

// Set combined effects
RetroEffectsManager.Instance.SetCombinedEffect(
    RetroEffectType.Scanlines,
    RetroEffectType.ColorPalette
);

// Apply preset
RetroEffectsManager.Instance.ApplyPreset("retro_gaming");

// Adjust parameters
RetroEffectsManager.Instance.SetPixelationSize(4);
RetroEffectsManager.Instance.SetScanlinesParameters(0.5f, 480f);
RetroEffectsManager.Instance.SetRGBGlitchIntensity(0.8f, 0.03f);

// Trigger glitch burst
RetroEffectsManager.Instance.TriggerGlitchBurst(duration: 0.3f);

// Disable effects
RetroEffectsManager.Instance.SetEffect(RetroEffectType.None);
```

### 4. FPS Counter
**Location:** `UI/FPSCounter.cs` (Autoload)

Performance monitoring overlay.

**Features:**
- Real-time FPS display
- Frame time measurement
- Toggle visibility
- Performance statistics

**Usage:**
```csharp
// Toggle FPS counter
FPSCounter.Instance.ToggleVisibility();

// Get current FPS
int fps = FPSCounter.Instance.GetCurrentFPS();
```

---

## UI & Menu Systems

### Main Menu System
**Location:** `UI/MainMenuUI.cs`

Complete menu navigation system.

**Menu Screens:**
- **Items** - Inventory management
- **Skills** - Skill viewing and equipment
- **Equip** - Equipment management
- **Status** - Character status viewing
- **Party** - Party composition
- **Quests** - Quest log
- **Bestiary** - Enemy encyclopedia
- **Lore Codex** - Lore entries
- **Options** - Game settings
- **Save/Load** - Save management

**Usage:**
```csharp
// Open main menu
MainMenuUI.Instance.OpenMenu();

// Open specific submenu
MainMenuUI.Instance.OpenItemMenu();
MainMenuUI.Instance.OpenStatusMenu();
MainMenuUI.Instance.OpenQuestMenu();

// Close menu
MainMenuUI.Instance.CloseMenu();
```

### Individual Menu Systems

Each menu has dedicated UI classes:
- `ItemMenuUI.cs` - Item management
- `SkillMenuUI.cs` - Skill viewing
- `EquipMenuUI.cs` - Equipment screen
- `StatusMenuUI.cs` - Character stats
- `PartyMenuUI.cs` - Party management
- `SaveMenuUI.cs` - Save game
- `LoadMenuUI.cs` - Load game
- `OptionsMenuUI.cs` - Settings
- `TitleScreenOptionsUI.cs` - Title screen options

---

## Event & Dialogue System

### Event System
**Location:** `Events/`

Flexible command-based system for scripting cutscenes and story events.

**Event Commands:**
- **Show Message** - Display dialogue
- **Show Choices** - Player decisions
- **Transfer Player** - Scene transitions
- **Set Variable** - Modify game variables
- **Conditional Branch** - If/else logic
- **Control Switches** - Boolean flags
- **Wait** - Pause execution
- **Play BGM/SE** - Audio playback
- **Show Picture** - Display images
- **Move Character** - Character movement
- **Start Battle** - Initiate combat
- **Add/Remove Item** - Inventory changes
- **Add/Remove Party Member** - Party changes
- **Call Common Event** - Reusable events

**Event Object:**
```csharp
// EventObject in scene
var eventObject = new EventObject
{
    TriggerType = EventTriggerType.ActionButton,
    EventPages = new List<EventPage>
    {
        new EventPage
        {
            Commands = new List<EventCommand>
            {
                new ShowMessageCommand 
                { 
                    Text = "Hello, traveler!" 
                },
                new AddItemCommand 
                { 
                    ItemId = "health_potion", 
                    Quantity = 1 
                }
            }
        }
    }
};
```

**Trigger Types:**
- **Action Button:** Trigger on interact (E key)
- **Player Touch:** Trigger on collision
- **Autorun:** Trigger automatically once
- **Parallel:** Run continuously in background

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
```
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

## Database System

### GameDatabase
**Location:** `Database/GameDatabase.cs`

Central repository for all game data.

**Data Categories:**
- **Playable Characters** - Player party members
- **Enemies** - Regular enemies
- **Bosses** - Boss enemies
- **Skills** - All abilities
- **Items** - All items (consumables, equipment, materials)
- **Common Events** - Reusable event scripts
- **Dialogue Tables** - Dialogue data
- **Troops** - Predefined enemy groups

**Usage:**
```csharp
// Access database
var database = GameManager.Instance.Database;

// Get character
var character = database.GetCharacter("aria");

// Get skill
var skill = database.GetSkill("fireball");

// Get item
var item = database.GetItem("health_potion");

// Get all playable characters
var party = database.GetAllPlayableCharacters();
```

### CharacterDatabase
**Location:** `Database/CharacterDatabase.cs`

Specialized character data management.

**Features:**
- Character lookup by ID
- Character type filtering
- Skill management
- Validation

**Usage:**
```csharp
// Initialize
characterDatabase.Initialize();

// Get character
var character = characterDatabase.GetCharacter("aria");

// Get by type
var playable = characterDatabase.GetPlayableCharacters();
var enemies = characterDatabase.GetEnemies();
var bosses = characterDatabase.GetBosses();

// Get skill
var skill = characterDatabase.GetSkill("fireball");
```

---

## Save System

### SaveManager
**Location:** `SaveSystem/SaveManager.cs`

Complete save/load system with 10 save slots.

**Save Data Includes:**
- Player position and scene
- Party composition and stats
- Inventory and equipment
- Quest progress
- Switches and variables
- Bestiary discoveries
- Lore codex entries
- Bond progression
- Relationships
- Custom system data

**Save/Load Operations:**

```csharp
// Create new save
SaveManager.Instance.SaveGame(slotIndex: 0, "My Save");

// Load save
SaveManager.Instance.LoadGame(slotIndex: 0);

// Check if slot has save
bool hasSave = SaveManager.Instance.HasSave(slotIndex: 0);

// Get save info
var info = SaveManager.Instance.GetSaveInfo(slotIndex: 0);

// Delete save
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
    
    // Add bonds
    var bondData = BondManager.Instance.GetSaveData();
    saveData.CustomData["Bonds"] = Json.Stringify(bondData);
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
    
    if (saveData.CustomData.ContainsKey("Bonds"))
    {
        var json = saveData.CustomData["Bonds"].AsString();
        BondManager.Instance.LoadSaveData(json);
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
| `LimitBreakSystem.IsLimitBreakReady()` | Check if limit ready |
| `LimitBreakSystem.ExecuteSoloLimit()` | Use solo limit break |
| `LimitBreakSystem.ExecuteDuoLimit()` | Use DUO limit break |

#### Party & Character API

| Method | Description |
|--------|-------------|
| `PartyManager.GetMainParty()` | Get active party |
| `PartyManager.GetSubParty()` | Get reserve party |
| `PartyManager.AddToMainParty()` | Add character to main |
| `PartyManager.SwapToSubParty()` | Move to reserves |
| `PartyManager.LockCharacter()` | Lock/unlock character |
| `PartyManager.DistributeExperience()` | Give EXP to party |

#### Item & Equipment API

| Method | Description |
|--------|-------------|
| `InventorySystem.AddItem()` | Add item to inventory |
| `InventorySystem.RemoveItem()` | Remove item |
| `InventorySystem.AddGold()` | Add gold |
| `EquipmentManager.EquipFromInventory()` | Equip item |
| `EquipmentManager.UnequipItem()` | Unequip item |

#### Quest System API

| Method | Description |
|--------|-------------|
| `QuestManager.StartQuest()` | Begin a quest |
| `QuestManager.CompleteQuest()` | Finish quest |
| `QuestManager.UpdateObjective()` | Update progress |
| `QuestManager.GetActiveQuests()` | Get active quests |

#### Shop System API

| Method | Description |
|--------|-------------|
| `ShopManager.OpenShop()` | Open shop by ID |
| `ShopManager.BuyItem()` | Purchase item |
| `ShopManager.SellItem()` | Sell item |
| `ShopManager.RestockShop()` | Restock shop inventory |

#### Bestiary API

| Method | Description |
|--------|-------------|
| `BestiaryManager.RecordEncounter()` | Track enemy encounter |
| `BestiaryManager.RecordDefeat()` | Track enemy defeat |
| `BestiaryManager.RecordWeaknessDiscovered()` | Track weakness |
| `BestiaryManager.GetEntry()` | Get bestiary entry |

#### Bond System API

| Method | Description |
|--------|-------------|
| `BondManager.AddBondPoints()` | Add bond points |
| `BondManager.GetBondLevel()` | Get bond level |
| `BondManager.GetRank()` | Get bond rank |
| `BondManager.OnAllyHealed()` | Bond event: healing |
| `BondManager.OnGuarded()` | Bond event: guarding |
| `BondManager.OnRevive()` | Bond event: revival |
| `RelationshipManager.SetRelationship()` | Set relationship type |
| `RelationshipManager.GetRelationship()` | Get relationship |

#### Audio API

| Method | Description |
|--------|-------------|
| `AudioManager.PlayBGM()` | Play background music |
| `AudioManager.StopBGM()` | Stop BGM |
| `AudioManager.PlaySFX()` | Play sound effect |
| `AudioManager.PlayVoice()` | Play voice clip |
| `AudioManager.SetBGMVolume()` | Set BGM volume |

#### Save System API

| Method | Description |
|--------|-------------|
| `SaveManager.SaveGame()` | Save to slot |
| `SaveManager.LoadGame()` | Load from slot |
| `SaveManager.HasSave()` | Check if slot used |
| `SaveManager.DeleteSave()` | Delete save |

---

## Development Guide

### Adding New Systems

#### 1. Create New Autoload Manager

```csharp
// MyNewManager.cs
using Godot;

public partial class MyNewManager : Node
{
    public static MyNewManager Instance { get; private set; }
    
    public override void _Ready()
    {
        Instance = this;
        Initialize();
    }
    
    private void Initialize()
    {
        // Setup code here
    }
}
```

Then add to Autoload in Project Settings.

#### 2. Integrate with Save System

```csharp
// Create save data class
public class MySystemSaveData
{
    public Dictionary<string, int> MyData { get; set; }
    // Add properties
}

// In your manager
public MySystemSaveData GetSaveData()
{
    return new MySystemSaveData
    {
        MyData = currentData
    };
}

public void LoadSaveData(MySystemSaveData data)
{
    currentData = data.MyData;
}
```

Add to SaveData.cs CustomData:
```csharp
// In CaptureCurrentState()
saveData.CustomData["MySystem"] = Json.Stringify(
    MyNewManager.Instance.GetSaveData()
);

// In ApplyToGame()
if (saveData.CustomData.ContainsKey("MySystem"))
{
    var json = saveData.CustomData["MySystem"].AsString();
    MyNewManager.Instance.LoadSaveData(json);
}
```

### Best Practices

1. **Use Autoloads for Managers** - Singleton pattern for global access
2. **Implement Save/Load** - All persistent data should be saveable
3. **Document Public APIs** - Use XML comments
4. **Error Handling** - Check for null, validate parameters
5. **Events for Decoupling** - Use signals/events for communication
6. **Resource-Based Data** - Use Godot Resources for game data
7. **Scene Composition** - Break down complex scenes
8. **Testing** - Create test scenes for each system

### Common Patterns

#### Singleton Autoload
```csharp
public static MyManager Instance { get; private set; }

public override void _Ready()
{
    if (Instance != null)
    {
        QueueFree();
        return;
    }
    Instance = this;
}
```

#### Resource Data
```csharp
[GlobalClass]
public partial class MyData : Resource
{
    [Export] public string Id { get; set; }
    [Export] public string Name { get; set; }
}
```

#### Event Pattern
```csharp
[Signal]
public delegate void ItemAddedEventHandler(string itemId, int quantity);

// Emit signal
EmitSignal(SignalName.ItemAdded, itemId, quantity);
```

---

## Conclusion

**Echoes Across Time** is a **complete, production-ready JRPG framework** with professional-grade systems comparable to AAA titles like Persona 5 Royal and Tales series games. The architecture is modular, extensible, and well-documented.

### What You Have

✅ **Complete battle system** with advanced mechanics  
✅ **Advanced AI systems** with learning and adaptation  
✅ **Full RPG systems** (quests, shops, bestiary, bonds, etc.)  
✅ **Comprehensive UI** integrating all features  
✅ **Event/cutscene system** for storytelling  
✅ **Save/load system** with multiple slots  
✅ **Overworld character control** with state machine  
✅ **Audio management** with BGM, SFX, and voice  
✅ **Visual effects** with retro shader filters  
✅ **Database system** for all game data  
✅ **Well-documented codebase**  
✅ **Modular, extensible architecture**

### System Count: 30+ Major Systems

1. Battle Manager
2. Advanced AI Pattern
3. Advanced AI Debugger
4. Advanced AI Director
5. AI Achievements
6. Party Manager
7. Quest Manager
8. Shop Manager
9. Bestiary Manager
10. Skit Manager
11. Lore Codex Manager
12. Bond Manager
13. Relationship Manager
14. Encounter Manager
15. Crafting System
16. Inventory System
17. Equipment Manager
18. Audio Manager
19. HUD Manager
20. Retro Effects Manager
21. FPS Counter
22. Save Manager
23. Event System
24. Dialogue System
25. Character Control System
26. State Machine
27. Game Database
28. Character Database
29. Main Menu System
30. All submenu systems

### Quick Commands Cheat Sheet

```csharp
// Battle
BattleManager.InitializeBattle(party, enemies, showtimes);
BattleManager.ExecuteAction(action);

// Party
PartyManager.Instance.AddToMainParty(character);
PartyManager.Instance.DistributeExperience(1000);

// Inventory
InventorySystem.Instance.AddItem("item_id", quantity);
InventorySystem.Instance.AddGold(amount);

// Equipment
EquipmentManager.Instance.EquipFromInventory("char_id", "item_id", stats);

// Quests
QuestManager.Instance.StartQuest("quest_id");
QuestManager.Instance.CompleteQuest("quest_id");

// Shops
ShopManager.Instance.OpenShop("shop_id");
ShopManager.Instance.BuyItem("item_id", quantity);

// Bestiary
BestiaryManager.Instance.RecordEncounter("enemy_id");
BestiaryManager.Instance.RecordWeaknessDiscovered("enemy_id", element);

// Bonds
BondManager.Instance.AddBondPoints("char_a", "char_b", points);
BondManager.Instance.GetBondLevel("char_a", "char_b");

// Audio
AudioManager.Instance.PlayBGM("track_name");
AudioManager.Instance.PlaySFX("sound_name");

// Retro Effects
RetroEffectsManager.Instance.SetEffect(RetroEffectType.Pixelation);
RetroEffectsManager.Instance.ApplyPreset("retro_gaming");

// Save/Load
SaveManager.Instance.SaveGame(slotIndex, "Save Name");
SaveManager.Instance.LoadGame(slotIndex);

// Events
SkitManager.Instance.PlaySkit("skit_id");
LoreCodexManager.Instance.DiscoverEntry("entry_id");

// AI
advancedAI.RecordPlayerAction(character, skill, target);
var action = advancedAI.DecideAction(character, party, enemies, turn);
```

### You Have Everything You Need to Create an Amazing JRPG! 🎮⚔️✨

---

**Happy game development! 🚀**

*Documentation Version: 2.0.0*  
*Last Updated: 2025*  
*Project: Echoes Across Time (Godot 4.5 / C#)*