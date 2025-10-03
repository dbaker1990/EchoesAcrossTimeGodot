# Phase 2: Advanced Battle Features Documentation

## Overview
Phase 2 adds three sophisticated Persona 5 Royal mechanics that greatly increase strategic depth:
- **Baton Pass** - Pass your extra turn to allies with stat boosts
- **Technical Damage** - Exploit status effects for bonus damage
- **Showtime Attacks** - Devastating duo attacks between character pairs

---

## 1. Baton Pass System

### Concept
After getting a One More (from hitting weakness or crit), you can pass your turn to another party member. The recipient gets increasingly powerful bonuses that stack with each pass!

### How It Works

#### Requirements:
- Actor must have a One More turn (from weakness/crit)
- Target must be alive and not have acted yet this round
- Cannot pass to yourself

#### Bonuses Per Pass Level:
```
Pass Level 1: +50% damage, +50% healing, +10% crit chance
Pass Level 2: +100% damage, +100% healing, +20% crit chance
Pass Level 3: +150% damage, +150% healing, +30% crit chance
```

#### Strategy Example:
```
Aria hits Ice weakness → Gets One More
  ↓
Baton Pass to Dominic (Lv.1 bonuses)
  ↓
Dominic hits Dark weakness → Gets One More
  ↓
Baton Pass to Echo Walker (Lv.2 bonuses - MASSIVE damage!)
```

### Code Usage

```csharp
// Check if baton pass available
if (battleManager.CanBatonPass())
{
    // Get valid targets
    var targets = battleManager.GetBatonPassTargets();
    
    // Execute pass
    battleManager.ExecuteBatonPass(targetMember);
    
    // Target now acts with bonuses!
}
```

### Implementation Details

**BatonPassData.cs:**
- Tracks pass count and calculates bonuses
- Resets each round
- Stacks multiplicatively

**BatonPassManager.cs:**
- Validates pass conditions
- Transfers turns between members
- Applies damage/healing/crit bonuses

**Key Methods:**
```csharp
// Check if actor can pass
bool CanBatonPass(BattleMember actor)

// Check if target can receive
bool CanReceiveBatonPass(BattleMember actor, BattleMember target)

// Execute the pass
bool ExecuteBatonPass(BattleMember from, BattleMember to)

// Apply bonuses to damage/healing
int ApplyDamageBonus(BattleMember actor, int baseDamage)
int ApplyHealingBonus(BattleMember actor, int baseHealing)
int GetCriticalBonus(BattleMember actor)
```

---

## 2. Technical Damage System

### Concept
Hitting enemies with specific attack types while they have certain status effects deals bonus damage and creates satisfying combos!

### Technical Combos

| Status Effect | Attack Type | Result | Multiplier |
|--------------|-------------|---------|-----------|
| **Burn** | Thunder/Ice | Technical! | 1.5x |
| **Freeze** | Physical | Shatter! | 1.5x |
| **Paralysis/Shock** | Physical | Critical strike! | 1.5x |
| **Sleep** | ANY attack | Wake violently! | 1.5x |
| **Poison** | Fire/Thunder | Detonate! | 1.5x |
| **Confusion** | Light/Dark | Exploit! | 1.5x |

### Strategy Example:
```
1. Apply Burn to enemy with Fire spell
2. Hit with Thunder spell → TECHNICAL!
   - 150% damage
   - Satisfying combo message
   - Status may be removed
```

### Code Usage

```csharp
// Technical system checks automatically during skill execution
// Just apply status effects and hit with the right element!

// Example flow:
1. Use Fire skill with Burn status
2. Enemy now has Burn
3. Use Thunder skill → System detects Technical!
4. Bonus damage applied automatically
```

### Implementation Details

**TechnicalDamageSystem.cs:**
- Defines all valid combos
- Checks target status effects
- Applies bonus multiplier
- Handles status removal

**Key Methods:**
```csharp
// Check if attack creates technical
TechnicalResult CheckTechnical(CharacterStats target, ElementType element)

// Apply technical multiplier
int ApplyTechnicalDamage(int baseDamage, TechnicalResult technical)

// Check if status should be removed
bool ShouldRemoveStatus(TechnicalComboType combo, StatusEffect status)

// Get possible technicals for planning
List<(StatusEffect, ElementType)> GetPossibleTechnicals(CharacterStats target)
```

### Adding New Combos

Edit the `TechnicalCombos` dictionary in `TechnicalDamageSystem.cs`:

```csharp
{
    StatusEffect.YourNewStatus, new List<(ElementType, TechnicalComboType)>
    {
        (ElementType.Fire, TechnicalComboType.YourNewCombo),
        (ElementType.Water, TechnicalComboType.YourNewCombo)
    }
}
```

---

## 3. Showtime Attacks

### Concept
Special cinematic duo attacks between specific character pairs that deal massive damage with high critical chance!

### Features
- **Pair-Specific** - Only certain characters can showtime together
- **Random Trigger** - Small chance to become available each turn
- **Cooldown** - Can't use immediately after triggering
- **Powerful** - Hits all enemies, high damage multiplier, high crit
- **Customizable** - Each showtime has unique properties

### Creating a Showtime

```csharp
var showtime = new ShowtimeAttackData
{
    Character1Id = "Dominic",
    Character2Id = "Echo Walker",
    AttackName = "Twilight Judgment",
    FlavorText = "Light and darkness converge!",
    
    // Damage
    BasePower = 400,
    Element = ElementType.Light,
    DamageMultiplier = 3.0f, // 300% damage!
    HitsAllEnemies = true,
    
    // Effects
    CriticalChance = 50,
    IgnoresDefense = false,
    
    // Trigger
    TriggerChance = 15, // 15% per turn
    CooldownTurns = 3,
    RequiresBothAlive = true,
    
    // Animation (optional)
    AnimationPath = "res://Animations/Showtimes/TwilightJudgment.tscn",
    AnimationDuration = 3.0f
};
```

### Registering Showtimes

```csharp
// During battle initialization
var showtimes = new List<ShowtimeAttackData>
{
    dominicEchoShowtime,
    ariaEchoShowtime,
    dominicAriaShowtime
};

battleManager.InitializeBattle(playerParty, enemies, showtimes);
```

### Executing Showtimes

```csharp
// Check if any showtimes available
var availableShowtimes = battleManager.GetAvailableShowtimes();

if (availableShowtimes.Count > 0)
{
    // Let player choose or auto-execute
    var showtime = availableShowtimes[0];
    battleManager.ExecuteShowtime(showtime);
}
```

### Showtime Properties

**ShowtimeAttackData.cs:**
```csharp
// Character Pair
string Character1Id
string Character2Id

// Damage
int BasePower              // Base damage (300-600)
float DamageMultiplier     // Multiplier (2.0x - 4.0x)
ElementType Element
bool HitsAllEnemies        // AoE vs single target
bool IgnoresDefense        // Bypass defense stat

// Effects  
int CriticalChance         // High crit (30-70%)
Array<StatusEffect> InflictsStatuses
int StatusChance
bool InstantKillChance     // Can one-shot!

// Trigger Conditions
int TriggerChance          // % per turn (10-20%)
int CooldownTurns          // Turns before can use again
float MinHPPercent         // HP required to trigger

// Animation
string AnimationPath       // Path to animation scene
float AnimationDuration
AudioStream SoundEffect
```

---

## Integration with Existing Systems

### All Three Systems Work Together!

**Ultimate Combo Example:**
```
1. Apply Burn status to enemy
2. Hit with Thunder → TECHNICAL! (1.5x damage)
3. Hit weakness → ONE MORE + KNOCKDOWN
4. BATON PASS to ally (Lv.1 bonuses)
5. Ally hits another weakness → ONE MORE
6. BATON PASS again (Lv.2 bonuses - 2x damage!)
7. All enemies knocked down → ALL-OUT ATTACK!
8. Next turn → SHOWTIME TRIGGERS! (3x damage finale!)
```

### Damage Calculation Order

```
Base Damage
  ↓
Apply Baton Pass Bonus (if active)
  ↓
Apply Technical Damage (if combo)
  ↓
Apply Critical Hit (if rolled)
  ↓
Apply Elemental Multiplier
  ↓
Final Damage
```

---

## File Structure

```
Combat/
├── BatonPassSystem.cs       ← Baton pass data and manager
├── TechnicalDamage.cs        ← Technical combo system
├── ShowtimeAttacks.cs        ← Showtime data and manager
├── BattleManager.cs          ← Updated with all systems
├── BattleMember.cs           ← Updated with BatonPassData
└── ...existing files...

Testing/
├── BattleTest.cs             ← Basic Phase 1 test
└── Phase2BattleTest.cs       ← Advanced Phase 2 demo
```

---

## Signals

New signals for Phase 2 features:

```csharp
[Signal] public delegate void BatonPassExecutedEventHandler(
    string fromCharacter, 
    string toCharacter, 
    int passLevel
);

[Signal] public delegate void TechnicalDamageEventHandler(
    string attackerName, 
    string targetName, 
    string comboType
);

[Signal] public delegate void ShowtimeTriggeredEventHandler(
    string showtimeName, 
    string char1, 
    string char2
);
```

---

## Testing Phase 2 Features

Run `Phase2BattleTest.cs` to see all features in action:

```
═══ PHASE 2 FEATURE DEMONSTRATION ═══

─── DEMO 1: Technical Damage ───
Dominic uses Inferno → Applies Burn
Echo Walker uses Ziodyne → ★★★ TECHNICAL! ★★★

─── DEMO 2: Baton Pass Chain ───  
Aria hits weakness → ONE MORE!
  → Baton Pass to Dominic (Lv.1: x1.5 damage)
Dominic hits weakness → ONE MORE!
  → Baton Pass to Echo Walker (Lv.2: x2.0 damage!)

─── DEMO 3: Showtime Attack ───
★★★ SHOWTIME: Twilight Judgment ★★★
[Dominic + Echo Walker]
"Light and darkness converge!"
→ Hits all enemies for massive damage!
```

---

## Balance Recommendations

### Baton Pass:
- Cap at 3 passes maximum per turn
- Consider limiting certain party compositions
- High risk/reward - vulnerable if chain breaks

### Technical Damage:
- 1.5x multiplier is balanced
- Can increase to 1.75x for harder difficulties
- Some statuses (Sleep) remove on hit

### Showtime:
- 15% trigger rate feels good
- 3-turn cooldown prevents spam
- 3x damage multiplier appropriate for cinematic impact

---

## Future Enhancements

### Potential Phase 3 Features:
1. **Follow-Up Attacks** - Characters attack when ally crits
2. **Assist Skills** - Support actions during ally turns
3. **Fusion Spells** - Multiple characters cast together
4. **Ambush System** - Pre-emptive strikes and surprise attacks

---

## Troubleshooting

**Q: Baton pass bonuses not applying**
- Check `BatonPassData.IsActive` is true
- Verify pass executed successfully
- Bonuses reset each round

**Q: Technical not triggering**
- Confirm status effect is active on target
- Check element type matches combo table
- Verify not hitting weakness (different mechanic)

**Q: Showtime never triggers**
- Check both characters are alive
- Verify cooldown has expired
- Increase `TriggerChance` for testing

**Q: Baton pass breaks on status effects**
- Working as intended
- Sleep/Stun prevents action even with extra turn
- Strategic element!

---

## Performance Notes

All three systems are:
- ✅ Lightweight (minimal overhead)
- ✅ Event-driven (no polling)
- ✅ Easy to extend
- ✅ Compatible with future AI system

---

## Congratulations!

You now have a complete Persona 5 Royal-style battle system with:
- ✅ Turn-based combat
- ✅ Elemental weakness exploitation
- ✅ One More system
- ✅ Knockdown states
- ✅ All-Out Attack
- ✅ **Baton Pass chains**
- ✅ **Technical damage combos**
- ✅ **Showtime duo attacks**

This is a AAA-quality JRPG battle system! 🎮⚔️

Next steps: Create UI, add animations, integrate with AI!