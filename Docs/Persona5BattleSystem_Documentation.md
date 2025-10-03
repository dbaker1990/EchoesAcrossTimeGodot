# Persona 5 Royal-Style Battle System Documentation

## Overview
This battle system implements the core mechanics of Persona 5 Royal's combat:
- **Turn-based combat** with speed-based turn order
- **Elemental weakness system** with 8 element types
- **One More mechanic** - hitting weaknesses or crits grants an extra turn
- **Knock-down states** - enemies become vulnerable when weakness is exploited
- **All-Out Attack** - devastating team attack when all enemies are knocked down

## Core Components

### 1. BattleMember.cs
Wraps `CharacterStats` with battle-specific state tracking.

**Key Properties:**
- `IsKnockedDown` - Persona 5's knock-down state
- `HasExtraTurn` - One More turn granted
- `CanAllOutAttack` - Can participate in All-Out Attack
- `OneMoreCount` - Track chained One Mores

**Key Methods:**
- `KnockDown()` - Mark as knocked down and grant extra turn
- `StandUp()` - Recover from knocked down state
- `CanAct()` - Check if can take actions (considers status effects)

### 2. BattleAction.cs
Represents an action to be executed in battle.

**Action Types:**
- `Attack` - Basic physical attack
- `Skill` - Use a skill (magic, abilities)
- `Guard` - Defensive stance (TODO)
- `AllOutAttack` - Team finishing move
- `Item` - Use item (TODO)
- `Escape` - Flee from battle (TODO)

### 3. BattleManager.cs
Core battle controller implementing all Persona 5 mechanics.

**Battle Flow:**
```
Initialize Battle
    ↓
Calculate Turn Order (by Speed)
    ↓
┌─> Start Turn
│       ↓
│   Process Status Effects
│       ↓
│   Player/Enemy Action
│       ↓
│   Check Weakness/Crit
│       ↓
│   Grant One More? ───┐
│       ↓              │
│   All Enemies Down?  │
│       ↓              │
│   All-Out Attack? ───┤
│       ↓              │
│   Check Battle End   │
│       ↓              │
└───Next Turn <────────┘
```

## Persona 5 Mechanics Implementation

### Weakness System

#### How It Works:
1. Each enemy has elemental affinities (Weak, Normal, Resist, Immune, Absorb)
2. Hitting a weakness deals 1.5x damage
3. **Triggers One More** - attacker gets an extra turn
4. **Causes Knockdown** - target is knocked down and vulnerable

#### Example:
```csharp
// Fire Demon is weak to Ice
fireDemon.ElementAffinities.SetAffinity(ElementType.Ice, ElementAffinity.Weak);

// When hit with Ice spell:
// - 150% damage
// - Attacker gets One More
// - Fire Demon is knocked down
```

### One More System

#### How It Works:
- Hitting a **weakness** grants One More
- Landing a **critical hit** grants One More
- Actor can immediately take another action
- Can chain multiple One Mores in a single turn
- Resets at end of turn

#### Implementation:
```csharp
if (result.HitWeakness || result.WasCritical)
{
    actor.HasExtraTurn = true;
    actor.OneMoreCount++;
}
```

### Knock-Down State

#### How It Works:
- Enemy is knocked down when weakness is hit or crit lands
- Knocked down enemies **cannot act**
- Stand up at the start of their next turn (if not killed)
- Makes them vulnerable to All-Out Attack

#### State Transitions:
```
Normal → Hit Weakness → Knocked Down
   ↑                         ↓
   └──── Next Turn Start ────┘
```

### All-Out Attack

#### How It Works:
- Available when **ALL living enemies are knocked down**
- Prompts player before auto-proceeding to next turn
- All party members attack simultaneously
- Deals massive damage (2x Attack stat per party member)
- Ends all party members' turns
- Enemies stand back up after

#### Trigger Conditions:
```csharp
bool AreAllEnemiesKnockedDown()
{
    var livingEnemies = enemyParty.Where(e => e.Stats.IsAlive);
    return livingEnemies.All(e => e.IsKnockedDown);
}
```

## Element System

### Element Types:
```csharp
public enum ElementType
{
    None,
    Fire,      // Agi, Agilao, Agidyne
    Water,     // Not in P5, but in your system
    Thunder,   // Zio, Zionga, Ziodyne  
    Ice,       // Bufu, Bufula, Bufudyne
    Earth,     // Not in P5, but in your system
    Light,     // Hama, Hamaon (instant death)
    Dark,      // Mudo, Mudoon (instant death)
    Physical   // Physical attacks
}
```

### Affinity Types:
```csharp
public enum ElementAffinity
{
    Normal,    // 100% damage
    Weak,      // 150% damage + Knockdown + One More
    Resist,    // 50% damage
    Immune,    // 0% damage
    Absorb     // Heals instead of damages
}
```

## Integration Guide

### Step 1: Set Up Battle Scene

Create a new scene for battles:
```
BattleScene.tscn
├─ BattleManager (BattleManager.cs)
├─ BattleUI (your UI script)
├─ PlayerPartyContainer
├─ EnemyPartyContainer
└─ BattleCamera
```

### Step 2: Initialize Battle

```csharp
public void StartBattle(List<CharacterStats> party, List<CharacterStats> enemies)
{
    battleManager.InitializeBattle(party, enemies);
    
    // Connect signals
    battleManager.TurnStarted += OnTurnStarted;
    battleManager.WeaknessHit += OnWeaknessHit;
    battleManager.OneMoreTriggered += OnOneMore;
    battleManager.AllOutAttackReady += OnAllOutAttackPrompt;
}
```

### Step 3: Handle Player Input

```csharp
// During player turn
if (battleManager.IsPlayerTurn())
{
    var currentActor = battleManager.CurrentActor;
    
    // Show skill menu, get player choice
    var selectedSkill = ShowSkillMenu(currentActor);
    var target = ShowTargetSelection();
    
    // Create and execute action
    var action = new BattleAction(currentActor, BattleActionType.Skill)
        .WithSkill(selectedSkill)
        .WithTargets(target);
    
    battleManager.ExecuteAction(action);
}
```

### Step 4: Handle All-Out Attack

```csharp
private void OnAllOutAttackPrompt()
{
    // Show "All-Out Attack?" prompt
    bool useAllOutAttack = ShowAllOutAttackPrompt();
    
    if (useAllOutAttack)
    {
        var leadMember = battleManager.GetPlayerParty()[0];
        var action = new BattleAction(leadMember, BattleActionType.AllOutAttack)
            .WithTargets(battleManager.GetLivingEnemies().ToArray());
        
        battleManager.ExecuteAction(action);
    }
    else
    {
        // Continue to next turn
        battleManager.StartNextTurn();
    }
}
```

### Step 5: Create Skills with Weaknesses

```csharp
// Ice spell - effective against Fire enemies
var bufuSkill = new SkillData
{
    SkillId = "bufu",
    DisplayName = "Bufu",
    Description = "Light Ice damage to one foe",
    Element = ElementType.Ice,
    BasePower = 80,
    MPCost = 12,
    TargetType = TargetType.SingleEnemy,
    DamageType = DamageType.Magical,
    Accuracy = 95
};

// Set up enemy weakness
fireEnemy.ElementAffinities.SetAffinity(ElementType.Ice, ElementAffinity.Weak);
```

## UI Feedback Recommendations

### Visual Indicators:

1. **Weakness Hit:**
    - Flash screen with elemental color
    - Play "weakness" sound effect
    - Show "WEAK!" text
    - Display One More indicator

2. **Knocked Down:**
    - Enemy sprite falls/tilts
    - Show "DOWN!" icon above enemy
    - Grayed out or special shader

3. **One More:**
    - Show "ONE MORE!" banner
    - Highlight current actor
    - Extend turn indicator

4. **All-Out Attack:**
    - Cinematic cutscene
    - Show all party members
    - Rapid-fire attack animations
    - Finishing pose

### Sound Effects:
- Weakness hit: Sharp, satisfying sound
- Critical: Different from weakness
- One More: Upbeat, encouraging
- All-Out Attack: Epic, powerful theme

## Advanced Features (Future)

### Phase 2 - Baton Pass:
- Pass One More turn to another party member
- Boost their stats (attack, healing, etc.)
- Strategic resource management

### Phase 3 - Technical Damage:
- Combine status effects with attacks
- Burn → use Wind/Nuke for Technical
- Freeze → use Physical for Technical
- Shock → use Physical for Technical

### Phase 4 - Showtime Attacks:
- Special duo attacks between specific pairs
- Triggers randomly when both are in party
- Cinematic special animations
- Very high damage

## Testing the System

Run `BattleTest.cs` to see the system in action:

```
Battle initialized: 3 heroes vs 3 enemies

=== Aria's Turn ===
>>> Aria casts Bufu on Fire Demon <<<
💥 WEAKNESS EXPLOITED!
Fire Demon is knocked down!
⭐ Aria gets ONE MORE turn!

=== Aria's ONE MORE Turn ===
>>> Aria casts Bufu on Thunder Beast <<<
...

*** ALL ENEMIES KNOCKED DOWN ***
╔═══════════════════════════════════════╗
║       ★ ALL-OUT ATTACK! ★          ║
╚═══════════════════════════════════════╝

*** VICTORY ***
```

## Performance Notes

- Turn order calculated once per round
- Status effects processed efficiently
- Signals used for loose coupling with UI
- No frame-by-frame updates needed

## Troubleshooting

**Q: One More isn't triggering**
- Verify `result.HitWeakness` is set correctly
- Check `ElementAffinityData` is configured
- Ensure damage multiplier > 1.0f

**Q: All-Out Attack not available**
- All enemies must be knocked down
- At least one player must be alive
- Check `AreAllEnemiesKnockedDown()` logic

**Q: Turn order seems wrong**
- Verify Speed stats are set correctly
- Check `CalculateTurnOrder()` sorting
- Status effects may prevent action

## Next Steps

1. ✅ Core battle system complete
2. 🔲 Create battle UI
3. 🔲 Add battle animations
4. 🔲 Implement AI for enemies
5. 🔲 Add Baton Pass system
6. 🔲 Implement Technical damage
7. 🔲 Create Showtime attacks

Good luck with your Persona-style RPG!