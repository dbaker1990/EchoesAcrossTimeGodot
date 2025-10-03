# Limit Break System Documentation

## Overview
The **Limit Break System** adds character-specific ultimate abilities that charge up during battle. This is the final major feature of the battle system and provides epic, game-changing moments!

---

## Core Mechanics

### How Limit Gauge Fills

```
Gauge fills from:
✅ Taking damage (80% of damage taken → gauge)
✅ Dealing damage (20% of damage dealt → gauge)
✅ Ally death (+30 gauge instantly)
✅ Landing critical hits (+10 bonus gauge)

Maximum gauge: 100 points
When full: Limit Break becomes available!
```

### Gauge Fill Examples

```
Take 100 damage → +80 gauge
Deal 100 damage → +20 gauge
Critical hit → +10 bonus gauge
Ally dies → +30 gauge to all survivors

Full combo example:
- Take 50 damage → +40 gauge (40/100)
- Deal 100 damage with crit → +20 + 10 = +30 gauge (70/100)
- Ally dies → +30 gauge (100/100)
- LIMIT BREAK READY! ⚡
```

---

## Creating Limit Breaks

### Basic Offensive Limit Break

```csharp
var limitBreak = new LimitBreakData
{
    LimitBreakId = "reality_tear",
    DisplayName = "Reality Tear",
    Description = "Tears through reality itself",
    CharacterId = "Dominic",
    Type = LimitBreakType.Offensive,
    FlavorText = "The boundaries between worlds shatter!",
    
    // Damage
    BasePower = 600,
    PowerMultiplier = 4.5f, // Very powerful!
    Element = ElementType.Dark,
    HitsAllEnemies = false, // Single target
    CriticalBonus = 30, // +30% crit chance
    
    // Special
    IgnoresDefense = true,
    InstantKillBelow = true,
    InstantKillThreshold = 0.25f, // Kill if < 25% HP
    
    // Mechanics
    StopsTime = true,
    TimeStopDuration = 1
};
```

### Multi-Hit AoE Limit Break

```csharp
var limitBreak = new LimitBreakData
{
    LimitBreakId = "temporal_convergence",
    DisplayName = "Temporal Convergence",
    CharacterId = "Echo Walker",
    Type = LimitBreakType.Offensive,
    
    // Multi-hit AoE
    BasePower = 400,
    PowerMultiplier = 3.5f,
    HitsAllEnemies = true, // Hits all!
    HitCount = 5, // 5 hits per enemy!
    
    // Effects
    GrantsExtraTurn = true, // Bonus turn after!
    DispelsBuffs = true, // Remove enemy buffs
    
    // Status
    InflictsStatuses = new() { StatusEffect.Blind },
    StatusChance = 60
};
```

### Hybrid Support/Damage Limit Break

```csharp
var limitBreak = new LimitBreakData
{
    LimitBreakId = "frozen_eternity",
    DisplayName = "Frozen Eternity",
    CharacterId = "Aria",
    Type = LimitBreakType.Hybrid,
    
    // Damage
    BasePower = 450,
    PowerMultiplier = 3.8f,
    HitsAllEnemies = true,
    Element = ElementType.Ice,
    
    // Freeze enemies
    InflictsStatuses = new() { StatusEffect.Freeze },
    StatusChance = 100,
    
    // Heal allies
    HealPercent = 0.3f, // 30% HP heal
    
    // Buff party
    DefenseBuff = 30, // +30% defense
    BuffsEntireParty = true,
    BuffDuration = 3
};
```

### DUO Limit Break

```csharp
var duoLimitBreak = new LimitBreakData
{
    LimitBreakId = "eclipse_of_eternity",
    DisplayName = "Eclipse of Eternity",
    CharacterId = "Dominic",
    
    // DUO SPECIFIC
    IsDuoLimitBreak = true,
    RequiredPartnerId = "Echo Walker",
    DuoPowerBonus = 1.8f, // +80% extra damage!
    
    // Damage - Absolutely devastating
    BasePower = 700,
    PowerMultiplier = 5.0f, // × 1.8 = 9x total!
    HitsAllEnemies = true,
    IgnoresDefense = true,
    
    // Special
    InstantKillBelow = true,
    InstantKillThreshold = 0.4f, // Kill < 40%!
    PiercesImmunity = true,
    StopsTime = true,
    TimeStopDuration = 2
};
```

---

## Limit Break Properties

### Basic Info
```csharp
string LimitBreakId         // Unique identifier
string DisplayName          // "Reality Tear"
string Description          // Full description
string CharacterId          // "Dominic"
LimitBreakType Type         // Offensive/Support/Hybrid
string FlavorText           // Epic quote!
```

### Damage Properties
```csharp
int BasePower               // Base damage (400-700)
float PowerMultiplier       // 3.0-5.0x multiplier
ElementType Element         // Fire/Ice/etc
bool HitsAllEnemies         // AoE vs single target
bool IgnoresDefense         // Bypass defense stat
int CriticalBonus           // Extra crit chance
int HitCount                // Multi-hit (1-10)
```

### Special Mechanics
```csharp
bool InstantKillBelow       // One-shot low HP enemies
float InstantKillThreshold  // HP% threshold (0.2 = 20%)
bool DispelsBuffs           // Remove enemy buffs
bool DispelsDebuffs         // Remove ally debuffs
bool PiercesImmunity        // Hit immune enemies
```

### Unique Effects
```csharp
bool GrantsExtraTurn        // Act again after!
bool StopsTime              // Enemies skip turns
int TimeStopDuration        // How many turns
bool SummonsCopy            // Temporal clone
int CopyDuration            // Clone lifetime
```

### Status Effects
```csharp
Array<StatusEffect> InflictsStatuses
int StatusChance            // % chance (0-100)
int StatusDuration          // Turns
```

### Support Effects
```csharp
int HealAmount              // Flat HP heal
float HealPercent           // % HP heal (0-1)
bool RevivesAllies          // Resurrect dead
bool FullHealParty          // 100% HP/MP restore
```

### Buffs
```csharp
int AttackBuff              // % increase
int DefenseBuff             // % increase
int SpeedBuff               // % increase
bool BuffsEntireParty       // All allies vs self
int BuffDuration            // Turns
```

### DUO Properties
```csharp
bool IsDuoLimitBreak        // Requires partner
string RequiredPartnerId    // "Echo Walker"
float DuoPowerBonus         // Extra multiplier (1.5-2.0x)
```

---

## Usage in Battle

### Check if Ready

```csharp
// Check single character
if (battleManager.IsLimitBreakReady(member))
{
    var limitBreak = battleManager.GetLimitBreak(member);
    // Show Limit Break option in menu
}

// Get gauge percent for UI bar
float gaugePercent = battleManager.GetLimitGaugePercent(member);
// Display: 0.0 = 0%, 1.0 = 100%
```

### Execute Limit Break

```csharp
// Single Limit Break
var limitBreak = battleManager.GetLimitBreak(currentActor);
var action = new BattleAction(currentActor, BattleActionType.LimitBreak)
    .WithLimitBreak(limitBreak)
    .WithTargets(enemies);

battleManager.ExecuteAction(action);
```

### Execute DUO Limit Break

```csharp
// Check if duo partner is ready
var dominic = party[0];
var echo = party[1];

if (dominic.IsLimitBreakReady && echo.IsLimitBreakReady)
{
    var duoLB = GetDuoLimitBreak("Dominic", "Echo Walker");
    
    if (duoLB != null)
    {
        var action = new BattleAction(dominic, BattleActionType.LimitBreak)
            .WithLimitBreak(duoLB, echo) // Pass partner!
            .WithTargets(allEnemies);
        
        battleManager.ExecuteAction(action);
        // Both gauges consumed!
    }
}
```

---

## Integration

### Register Limit Breaks

```csharp
// During battle initialization
var limitBreaks = new List<LimitBreakData>
{
    ExampleLimitBreaks.CreateRealityTear(),
    ExampleLimitBreaks.CreateTemporalConvergence(),
    ExampleLimitBreaks.CreateFrozenEternity(),
    ExampleLimitBreaks.CreateEclipseOfEternity() // DUO
};

battleManager.InitializeBattle(party, enemies, showtimes, limitBreaks);
```

### Create as Resources

```gdscript
# RealityTear.tres
[gd_resource type="Resource" script_class="LimitBreakData" format=3]

[resource]
script = preload("res://Combat/LimitBreakData.cs")
LimitBreakId = "reality_tear"
DisplayName = "Reality Tear"
CharacterId = "Dominic"
BasePower = 600
PowerMultiplier = 4.5
Element = 7  # Dark
IgnoresDefense = true
InstantKillBelow = true
InstantKillThreshold = 0.25
StopsTime = true
TimeStopDuration = 1
```

### UI Implementation

```csharp
// Limit gauge bar
public class LimitGaugeBar : ProgressBar
{
    private BattleMember member;
    
    public override void _Process(double delta)
    {
        if (member != null)
        {
            float percent = battleManager.GetLimitGaugePercent(member);
            Value = percent * 100;
            
            // Flash when ready
            if (member.IsLimitBreakReady)
            {
                Modulate = FlashColor(Time.GetTicksMsec());
            }
        }
    }
}

// Action menu
private void UpdateActionMenu()
{
    var limitBreakButton = actionMenu.GetNode<Button>("LimitBreak");
    
    bool canUse = battleManager.IsLimitBreakReady(currentActor);
    limitBreakButton.Disabled = !canUse;
    
    if (canUse)
    {
        limitBreakButton.AddThemeColorOverride("font_color", Colors.Yellow);
        limitBreakButton.Text = "⚡ LIMIT BREAK! ⚡";
    }
}
```

---

## Signals

```csharp
// When limit gauge fills to 100%
LimitBreakReady(string characterName)

// When limit break is executed
LimitBreakUsed(string characterName, string limitBreakName, bool isDuo)
```

### Signal Handlers

```csharp
private void OnLimitBreakReady(string characterName)
{
    // Play ready sound
    AudioManager.Instance.PlaySE("limit_ready");
    
    // Show notification
    ShowNotification($"{characterName}'s Limit Break is READY!");
    
    // Flash gauge bar
    FlashLimitGauge(characterName);
}

private void OnLimitBreakUsed(string character, string lbName, bool isDuo)
{
    if (isDuo)
    {
        // Play epic duo cutscene
        PlayDuoLimitBreakCutscene(lbName);
    }
    else
    {
        // Play character limit break animation
        PlayLimitBreakAnimation(character, lbName);
    }
}
```

---

## Balance Guidelines

### Gauge Fill Rates
```
Current rates:
- Take damage: 80% conversion (100 dmg = +80 gauge)
- Deal damage: 20% conversion (100 dmg = +20 gauge)
- Ally death: +30 gauge flat
- Critical hit: +10 bonus

Typical battle:
- Turn 3-5: First limit break available
- Turn 8-10: Second limit break available
- Boss battles: 2-3 limit breaks total
```

### Power Multipliers
```
Single target: 4.0-5.0x multiplier
Multi-target: 3.0-4.0x multiplier
Support: 2.0-3.0x multiplier

Base power ranges:
- Offensive: 500-700
- Hybrid: 400-500
- Support: 300-400
```

### DUO Balance
```
Duo bonus: 1.5-2.0x extra multiplier
Requires BOTH gauges full
Consumes BOTH gauges
Should feel truly ultimate!

Example DUO damage:
Base 700 × 5.0 × 1.8 = 6300 base damage!
With buffs/crits: 10,000+ possible
```

---

## Character-Specific Ideas

### Dominic (Court Mage - Dark/Political)
- **Reality Tear** - Massive dark single target + time stop
- **Diplomatic Immunity** - Buff party, debuff enemies
- **Shadow Tribunal** - Multi-hit judgment attacks

### Echo Walker (Time Walker)
- **Temporal Convergence** - Multi-hit all enemies
- **Time Rewind** - Undo last turn, heal party
- **Chronos Strike** - Speed-based massive damage

### Aria (Ice/Support)
- **Frozen Eternity** - AoE ice + freeze + heal
- **Crystal Sanctuary** - Ultimate defense buff
- **Blizzard Queen** - Weather change + sustained damage

### Story Character Ideas
- **Politician** - "Veto Power" - Cancel enemy actions
- **Warrior** - "Last Stand" - Survive fatal blow + counter
- **Healer** - "Life Surge" - Full party revive + heal
- **Assassin** - "Inevitable Death" - Guaranteed kill
- **Mage** - "Meteor Strike** - Highest raw damage

---

## Advanced Features

### Conditional Limit Breaks
```csharp
// Only available at low HP
if (user.Stats.HPPercent <= 0.3f)
{
    // "Desperation" limit breaks
}

// Only in specific story moments
if (storyFlag == "FinalBoss")
{
    // Special story limit breaks
}
```

### Limit Break Chains
```csharp
// Track last used limit break
if (lastLimitBreak == "RealityTear")
{
    // Next limit break gets bonus
    bonusMultiplier = 1.2f;
}
```

### Limit Break Upgrades
```csharp
// Track usage count
limitBreakUseCount++;

if (limitBreakUseCount >= 10)
{
    // Unlock upgraded version
    UnlockLimitBreak2();
}
```

---

## Testing

Run `LimitBreakTest.cs` to see:

```
═══ LIMIT BREAK DEMONSTRATION ═══

Phase 1: Building Limit Gauge
- Take damage to fill gauge
- Gauge: 0% → 40% → 75% → 100%
- LIMIT BREAK READY! ⚡

Phase 2: Single Limit Break
- Dominic uses Reality Tear!
- Massive damage + time stop!
- Gauge reset to 0%

Phase 3: Hybrid Limit Break
- Aria uses Frozen Eternity!
- Damage + Freeze + Heal + Buff!

Phase 4: DUO LIMIT BREAK!
- Dominic & Echo Walker!
- Eclipse of Eternity!
- DEVASTATING POWER!
```

---

## Files Added

```
Combat/
├── LimitBreakData.cs           ← Limit break definitions
├── LimitBreakSystem.cs         ← Gauge & execution manager
├── BattleMember.cs             ← Updated with gauge
├── BattleAction.cs             ← Updated with LB support
└── BattleManager.cs            ← Updated integration

Database/
└── ExampleLimitBreaks.cs       ← Pre-made limit breaks

Testing/
└── LimitBreakTest.cs           ← Demo scene
```

---

## Quick Reference

### Create Limit Break
```csharp
var lb = new LimitBreakData
{
    CharacterId = "Dominic",
    DisplayName = "Reality Tear",
    BasePower = 600,
    PowerMultiplier = 4.5f,
    HitsAllEnemies = false
};
```

### Check if Ready
```csharp
bool ready = member.IsLimitBreakReady;
float percent = battleManager.GetLimitGaugePercent(member);
```

### Execute
```csharp
var action = new BattleAction(actor, BattleActionType.LimitBreak)
    .WithLimitBreak(limitBreak)
    .WithTargets(enemies);
battleManager.ExecuteAction(action);
```

---

## 🎉 Complete Battle System!

You now have **EVERYTHING**:

✅ Phase 1: Core mechanics (weakness, One More, All-Out Attack)
✅ Phase 2: Advanced (Baton Pass, Technical, Showtime)
✅ **Phase 3: Limit Breaks (Ultimate abilities!)**

**This is a AAA-quality JRPG battle system!** 🎮⚔️✨

---

**Next:** Create battle UI and make it look AMAZING! 🎨