# 📖 Bestiary System - Quick Reference

## 🎯 What Is This?

A complete enemy encyclopedia system for your JRPG! Automatically tracks all enemies encountered, their weaknesses, skills, and drops. Think Pokédex but for enemies! 🐉

---

## ✨ Features at a Glance

✅ **Auto-Discovery** - Enemies recorded when encountered  
✅ **Progressive Learning** - Weaknesses revealed through combat  
✅ **Beautiful UI** - Browse all enemies with detailed stats  
✅ **Notifications** - "New Enemy Registered!" popups  
✅ **Save System** - Progress persists across sessions  
✅ **Mystery Entries** - Undiscovered enemies show as ???

---

## 🚀 Quick Setup (5 Minutes)

### 1. Add to Autoload
```
Project Settings → Autoload
Name: BestiaryManager
Path: res://Bestiary/BestiaryManager.cs
```

### 2. Add Integration
```
Project Settings → Autoload
Name: BestiaryIntegration
Path: res://Bestiary/BestiaryIntegration.cs
```

### 3. Add UI to Main Scene
```gdscript
var bestiary_ui = preload("res://Bestiary/BestiaryUI.tscn").instantiate()
add_child(bestiary_ui)
```

### 4. Initialize Database
```csharp
BestiaryManager.Instance?.InitializeFromDatabase(GameManager.Instance.Database);
```

### 5. Add Menu Button
```csharp
private void OnBestiaryPressed()
{
	GetNode<BestiaryUI>("%BestiaryUI")?.ShowBestiary();
}
```

---

## 📚 Core Functions

### Recording Events
```csharp
// Record encounter (auto-called by integration)
BestiaryManager.Instance.RecordEncounter("slime_king", level: 15);

// Record defeat
BestiaryManager.Instance.RecordDefeat("slime_king");

// Record weakness discovery
BestiaryManager.Instance.RecordWeaknessDiscovered("slime_king", ElementType.Fire);

// Record skill discovery
BestiaryManager.Instance.RecordSkillDiscovered("slime_king", "acid_spray");
```

### Checking Progress
```csharp
// Get stats
int discovered = BestiaryManager.Instance.TotalDiscovered;
int total = BestiaryManager.Instance.TotalEnemiesInGame;
float percent = BestiaryManager.Instance.CompletionPercentage;

// Check specific enemy
bool found = BestiaryManager.Instance.IsDiscovered("slime_king");

// Get entry details
var entry = BestiaryManager.Instance.GetEntry("slime_king");
int encounters = entry?.TimesEncountered ?? 0;
```

### Filtering/Sorting
```csharp
// Get filtered enemies
var bosses = BestiaryManager.Instance.GetFilteredEnemies(BestiaryFilterType.Bosses);
var discovered = BestiaryManager.Instance.GetFilteredEnemies(BestiaryFilterType.Discovered);

// Get sorted enemies
var byLevel = BestiaryManager.Instance.GetSortedEnemies(BestiarySortType.ByLevel, descending: true);
```

---

## 🎮 How It Works

### Automatic Flow

```
Battle Starts
	↓
Enemy Appears → Recorded as "encountered"
	↓
Player Attacks with Fire
	↓
Enemy Weak to Fire? → Weakness recorded
	↓
Notification: "Weakness Discovered: Fire!"
	↓
Enemy Defeated → Defeat count +1
	↓
Bestiary Updated
```

### Manual Recording

For cutscenes or special events:
```csharp
// Boss reveals itself in cutscene
BestiaryManager.Instance.RecordEncounter("final_boss", 99);

// NPC tells you about weakness
BestiaryManager.Instance.RecordWeaknessDiscovered("dragon", ElementType.Ice);
```

---

## 📊 UI Features

### Main View
- Enemy list with portraits
- Search bar for filtering by name
- Dropdown filters (All/Discovered/Bosses/etc.)
- Sort options (Name/Level/Type/Encounters)
- Completion progress bar

### Detail View (when clicking enemy)
- **Portrait** - Enemy sprite
- **Basic Info** - Name, type, level
- **Stats** - HP, ATK, DEF, etc.
- **Encounter Info** - Times seen/defeated
- **Weaknesses** - Discovered elements
- **Resistances** - Immunities/absorbs
- **Skills** - Known abilities
- **Drops** - Items and drop rates
- **Lore** - Description text

---

## 🔧 Common Tasks

### Add Bestiary Button to Menu
```csharp
// In pause menu
[Export] private Button bestiaryButton;

public override void _Ready()
{
	bestiaryButton.Pressed += () => {
		GetNode<BestiaryUI>("%BestiaryUI")?.ShowBestiary();
	};
}
```

### Track Enemy Skills
```csharp
// In your battle manager when enemy uses skill
private void OnEnemyUseSkill(BattleMember enemy, SkillData skill)
{
	BestiaryManager.Instance?.RecordSkillDiscovered(
		enemy.Stats.CharacterId,
		skill.SkillId
	);
}
```

### Create "Scan" Skill
```csharp
// Skill that reveals all enemy info
if (skill.SkillId == "scan")
{
	var entry = BestiaryManager.Instance.GetEntry(target.CharacterId);
	
	// Reveal all weaknesses
	for (int i = 0; i < 8; i++)
	{
		var element = (ElementType)i;
		var affinity = target.ElementAffinities.GetAffinity(element);
		
		if (affinity == ElementAffinity.Weak)
		{
			BestiaryManager.Instance.RecordWeaknessDiscovered(
				target.CharacterId, element
			);
		}
	}
}
```

### Unlock All (Debug)
```csharp
// For testing
BestiaryManager.Instance.UnlockAll();
```

---

## 💾 Save Integration

Add to your `SaveData.cs`:

```csharp
[Export]
public Godot.Collections.Dictionary CustomData { get; set; } = new();

// When saving
public void CaptureCurrentState()
{
	// ... existing code ...
	
	if (BestiaryManager.Instance != null)
	{
		var bestiaryData = BestiaryManager.Instance.GetSaveData();
		CustomData["Bestiary"] = Json.Stringify(bestiaryData);
	}
}

// When loading
public void ApplyToGame()
{
	// ... existing code ...
	
	if (CustomData.ContainsKey("Bestiary"))
	{
		var json = CustomData["Bestiary"].AsString();
		var data = Json.ParseString(json);
		// Parse and load bestiary data
	}
}
```

---

## 🎨 Customization

### Change Notification Duration
```csharp
// In BestiaryNotification.cs
[Export] private float displayDuration = 3.0f;  // Seconds
```

### Add Custom Filters
```csharp
// In BestiaryUI.cs
filterDropdown.AddItem("Fire Enemies", (int)BestiaryFilterType.ByElement);
```

### Reward Milestones
```csharp
BestiaryManager.Instance.BestiaryUpdated += () => {
	float completion = BestiaryManager.Instance.CompletionPercentage;
	
	if (completion >= 50f)
		GiveReward("bestiary_50");
	
	if (completion >= 100f)
		GiveReward("bestiary_master");
};
```

---

## 📁 File Reference

```
Bestiary/
├── BestiaryData.cs              # Data structures
├── BestiaryManager.cs           # Core system (AUTOLOAD)
├── BestiaryUI.cs               # Main interface
├── BestiaryNotification.cs     # Discovery popups
└── BestiaryIntegration.cs      # Auto-tracking (AUTOLOAD)

Docs/
├── BestiarySystemGuide.md      # Full documentation
└── BestiaryReadMe.md          # This file
```

---

## 🐛 Troubleshooting

**Enemies not recording?**
- Check BestiaryManager is in autoload ✓
- Verify InitializeFromDatabase() was called ✓
- Ensure BestiaryIntegration is connected ✓

**UI not showing?**
- Check BestiaryUI scene is instantiated ✓
- Verify unique name `%BestiaryUI` is set ✓

**Save not working?**
- Add CustomData to SaveData.cs ✓
- Call GetSaveData() when saving ✓
- Call LoadSaveData() when loading ✓

---

## 🎯 Quick Test

Run `BestiaryTestScene.cs` to test all features:
1. Encounter enemies
2. Discover weaknesses
3. View UI
4. Test filters
5. Check save/load

---

## 🏆 You're All Set!

Your bestiary is ready to track all enemies! Players will love discovering and completing it! 📖✨

**For full documentation, see:** `BestiarySystemGuide.md`

---

## 📞 Integration Checklist

- [ ] BestiaryManager in autoload
- [ ] BestiaryIntegration in autoload
- [ ] BestiaryUI added to main scene
- [ ] Database initialized in game start
- [ ] Menu button connected
- [ ] Save integration added
- [ ] Test scene verified
- [ ] Notifications enabled

**Happy Hunting! 🐉⚔️**
