# 📖 Bestiary System - Developer Cheat Sheet

## ⚡ Quick Commands

### Recording Events
```csharp
// Encounter enemy
BestiaryManager.Instance.RecordEncounter("slime", level: 5);

// Defeat enemy
BestiaryManager.Instance.RecordDefeat("slime");

// Discover weakness
BestiaryManager.Instance.RecordWeaknessDiscovered("slime", ElementType.Fire);

// Discover resistance
BestiaryManager.Instance.RecordResistanceDiscovered("slime", ElementType.Ice, ElementAffinity.Resist);

// Discover skill
BestiaryManager.Instance.RecordSkillDiscovered("slime", "acid_spray");
```

### Checking Data
```csharp
// Get entry
var entry = BestiaryManager.Instance.GetEntry("slime");

// Check if discovered
bool found = BestiaryManager.Instance.IsDiscovered("slime");

// Get stats
int discovered = BestiaryManager.Instance.TotalDiscovered;
int total = BestiaryManager.Instance.TotalEnemiesInGame;
float percent = BestiaryManager.Instance.CompletionPercentage;

// Get enemy data
var enemyData = BestiaryManager.Instance.GetEnemyData("slime");
```

### Filtering/Sorting
```csharp
// Filter
var bosses = BestiaryManager.Instance.GetFilteredEnemies(BestiaryFilterType.Bosses);
var discovered = BestiaryManager.Instance.GetFilteredEnemies(BestiaryFilterType.Discovered);

// Sort
var byLevel = BestiaryManager.Instance.GetSortedEnemies(BestiarySortType.ByLevel, descending: true);
var byName = BestiaryManager.Instance.GetSortedEnemies(BestiarySortType.ByName);
```

### UI Control
```csharp
// Show bestiary
var ui = GetNode<BestiaryUI>("%BestiaryUI");
ui.ShowBestiary();

// Custom notification
var notif = GetNode<BestiaryNotification>("%BestiaryNotification");
notif.ShowCustomNotification("dragon", "Special Enemy Spotted!");
```

---

## 🔌 Integration Points

### 1. Battle Start
```csharp
// In BattleManager.InitializeBattle()
foreach (var enemy in enemyParty)
{
    BestiaryManager.Instance?.RecordEncounter(
        enemy.Stats.CharacterId,
        enemy.Stats.Level
    );
}
```

### 2. Damage Dealt
```csharp
// In BattleManager.ExecuteAction() after damage
if (result.HitWeakness && IsPlayerAttackingEnemy())
{
    BestiaryManager.Instance?.RecordWeaknessDiscovered(
        target.Stats.CharacterId,
        result.Element
    );
}
```

### 3. Enemy Defeated
```csharp
// When enemy HP <= 0
BestiaryManager.Instance?.RecordDefeat(enemy.Stats.CharacterId);
```

### 4. Enemy Skill Used
```csharp
// In AI or skill execution
if (IsEnemy(user))
{
    BestiaryManager.Instance?.RecordSkillDiscovered(
        user.Stats.CharacterId,
        skill.SkillId
    );
}
```

---

## 📊 Enums Reference

### BestiaryFilterType
- `All` - All enemies
- `Discovered` - Only found enemies
- `Undiscovered` - Not yet found
- `Bosses` - Boss enemies only
- `RegularEnemies` - Non-boss enemies

### BestiarySortType
- `ByName` - Alphabetical
- `ByLevel` - Level order
- `ByType` - Boss/Enemy
- `ByTimesEncountered` - Most encountered
- `ByFirstEncounter` - First seen date
- `ByLastEncounter` - Last seen date

---

## 🎯 Common Patterns

### Check Completion & Reward
```csharp
BestiaryManager.Instance.BestiaryUpdated += () => {
    float completion = BestiaryManager.Instance.CompletionPercentage;
    
    if (completion >= 25f) UnlockReward("bestiary_25");
    if (completion >= 50f) UnlockReward("bestiary_50");
    if (completion >= 75f) UnlockReward("bestiary_75");
    if (completion >= 100f) UnlockReward("bestiary_master");
};
```

### Create Scan Skill
```csharp
if (skill.SkillId == "scan")
{
    // Reveal all info about target
    for (int i = 0; i < 8; i++)
    {
        var element = (ElementType)i;
        var affinity = target.ElementAffinities.GetAffinity(element);
        
        if (affinity == ElementAffinity.Weak)
            BestiaryManager.Instance.RecordWeaknessDiscovered(target.CharacterId, element);
        else if (affinity != ElementAffinity.Normal)
            BestiaryManager.Instance.RecordResistanceDiscovered(target.CharacterId, element, affinity);
    }
}
```

### Track Boss Battles
```csharp
private void OnBossDefeated(BattleMember boss)
{
    var entry = BestiaryManager.Instance.GetEntry(boss.Stats.CharacterId);
    
    if (entry.TimesDefeated == 1)
    {
        // First time defeating this boss
        ShowMessage("Boss data added to bestiary!");
        GiveReward("boss_data_complete");
    }
}
```

### Custom Notifications
```csharp
// On rare enemy encounter
if (enemyData.CharacterId == "rare_golden_slime")
{
    var notif = GetNode<BestiaryNotification>("%BestiaryNotification");
    notif.ShowCustomNotification("rare_golden_slime", "Rare Enemy Encountered!");
}
```

---

## 💾 Save/Load Pattern

### SaveData.cs
```csharp
[Export]
public Godot.Collections.Dictionary CustomData { get; set; } = new();

// Save
public void CaptureCurrentState()
{
    if (BestiaryManager.Instance != null)
    {
        var data = BestiaryManager.Instance.GetSaveData();
        CustomData["Bestiary"] = Json.Stringify(data);
    }
}

// Load
public void ApplyToGame()
{
    if (CustomData.ContainsKey("Bestiary") && BestiaryManager.Instance != null)
    {
        var json = CustomData["Bestiary"].AsString();
        var data = Json.ParseString(json);
        // Deserialize and apply
    }
}
```

---

## 🐛 Debug Commands

```csharp
// Unlock all for testing
BestiaryManager.Instance.UnlockAll();

// Check specific entry
var entry = BestiaryManager.Instance.GetEntry("dragon");
GD.Print($"Discovered: {entry?.IsDiscovered}");
GD.Print($"Encountered: {entry?.TimesEncountered}");
GD.Print($"Weaknesses: {entry?.DiscoveredWeaknesses.Count}");

// Print stats
GD.Print($"Completion: {BestiaryManager.Instance.CompletionPercentage:F1}%");
GD.Print($"Found: {BestiaryManager.Instance.TotalDiscovered}/{BestiaryManager.Instance.TotalEnemiesInGame}");
```

---

## 📱 Signals

### BestiaryManager Signals
```csharp
// New enemy discovered
BestiaryManager.Instance.EnemyDiscovered += (enemyId, data) => {
    GD.Print($"Discovered: {data.DisplayName}");
};

// Weakness found
BestiaryManager.Instance.WeaknessDiscovered += (enemyId, element) => {
    GD.Print($"Weak to: {element}");
};

// Skill learned
BestiaryManager.Instance.NewSkillDiscovered += (enemyId, skillId) => {
    GD.Print($"Skill learned: {skillId}");
};

// Any update
BestiaryManager.Instance.BestiaryUpdated += () => {
    RefreshUI();
};
```

---

## 🎨 UI Customization

### Change Notification Duration
```csharp
// In BestiaryNotification.cs
[Export] private float displayDuration = 3.0f; // seconds
```

### Modify Discovery Color
```csharp
// In BestiaryNotification.cs
[Export] private Color discoveryColor = Colors.Gold;
```

### Add Custom Filters
```csharp
// In BestiaryUI.cs SetupFilterDropdown()
filterDropdown.AddItem("Fire Enemies", 100);
filterDropdown.AddItem("Level 50+", 101);
```

---

## ⚠️ Gotchas

1. **Initialize Database First**
   ```csharp
   // MUST call before first battle!
   BestiaryManager.Instance.InitializeFromDatabase(GameManager.Instance.Database);
   ```

2. **Check for Null**
   ```csharp
   // Always check instance exists
   if (BestiaryManager.Instance == null) return;
   ```

3. **Player vs Enemy**
   ```csharp
   // Only track PLAYER attacks on ENEMIES
   if (IsPlayer(attacker) && IsEnemy(target))
   ```

4. **Unique Enemy IDs**
   ```csharp
   // Ensure CharacterData IDs are unique!
   "slime_green"  ✓
   "slime"        ✗ (if multiple slime types)
   ```

---

## 🚀 Performance Tips

- ✅ Use signals instead of polling
- ✅ Cache enemy data lookups
- ✅ Update UI only when visible
- ✅ Batch notifications (queue system)
- ✅ Lazy load portraits on demand

---

## 📝 File Locations

```
Bestiary/
├── BestiaryData.cs              # Data structures
├── BestiaryManager.cs           # Core (AUTOLOAD)
├── BestiaryUI.cs               # Main UI
├── BestiaryNotification.cs     # Popups
├── BestiaryIntegration.cs      # Auto-hooks (AUTOLOAD)
└── BestiaryBattleHook.cs       # Manual hooks

Docs/
├── BestiarySystemGuide.md      # Full guide
├── BestiaryReadMe.md          # Quick start
└── BestiaryCheatSheet.md      # This file
```

---

## ✅ Setup Checklist

- [ ] BestiaryManager in autoload
- [ ] BestiaryIntegration in autoload
- [ ] BestiaryUI in main scene (unique name: %BestiaryUI)
- [ ] BestiaryNotification in main scene
- [ ] InitializeFromDatabase() called
- [ ] Battle hooks connected
- [ ] Menu button added
- [ ] Save integration added
- [ ] Test scene verified

---

## 🎯 Quick Test

```csharp
// Test in _Ready()
BestiaryManager.Instance?.InitializeFromDatabase(testDB);
BestiaryManager.Instance?.RecordEncounter("slime", 5);
BestiaryManager.Instance?.RecordWeaknessDiscovered("slime", ElementType.Fire);
GetNode<BestiaryUI>("%BestiaryUI")?.ShowBestiary();
```

---

**Keep this handy while implementing! 🚀**