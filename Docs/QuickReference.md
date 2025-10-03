# Battle System Quick Reference

## 🎯 One-Page Cheat Sheet

### Initialize Battle
```csharp
battleManager.InitializeBattle(playerStats, enemyStats, showtimes);
```

### Execute Player Action
```csharp
var action = new BattleAction(actor, BattleActionType.Skill)
    .WithSkill(skill)
    .WithTargets(target);
battleManager.ExecuteAction(action);
```

### Baton Pass
```csharp
if (battleManager.CanBatonPass())
{
    battleManager.ExecuteBatonPass(targetAlly);
}
```

### All-Out Attack
```csharp
if (battleManager.CanUseAllOutAttack())
{
    var action = new BattleAction(leader, BattleActionType.AllOutAttack)
        .WithTargets(allEnemies);
    battleManager.ExecuteAction(action);
}
```

### Showtime
```csharp
var showtimes = battleManager.GetAvailableShowtimes();
if (showtimes.Count > 0)
{
    battleManager.ExecuteShowtime(showtimes[0]);
}
```

---

## 📊 Element Weaknesses Table

| Element | Strong Against | Weak Against |
|---------|---------------|--------------|
| Fire | Ice, Earth | Water |
| Water | Fire | Thunder, Ice |
| Ice | Water, Earth | Fire |
| Thunder | Water | Earth |
| Earth | Thunder, Fire | Ice |
| Light | Dark | Dark |
| Dark | Light | Light |
| Physical | - | - |

---

## 🔥 Technical Combos

| Status | + Element | = Technical |
|--------|-----------|-------------|
| Burn | Thunder/Ice | ⚡ TECH! |
| Freeze | Physical | ⚡ TECH! |
| Shock | Physical | ⚡ TECH! |
| Sleep | ANY | ⚡ TECH! |
| Poison | Fire/Thunder | ⚡ TECH! |
| Confusion | Light/Dark | ⚡ TECH! |

**All Technical = 1.5x damage**

---

## 💪 Baton Pass Bonuses

| Level | Damage | Healing | Crit |
|-------|--------|---------|------|
| 1 | +50% | +50% | +10% |
| 2 | +100% | +100% | +20% |
| 3 | +150% | +150% | +30% |

---

## 🎬 Core Signals

```csharp
BattleStarted()
TurnStarted(string character)
ActionExecuted(string actor, string action, int dmg, bool weak, bool crit)
WeaknessHit(string attacker, string target)
OneMoreTriggered(string character)
Knockdown(string character)
AllOutAttackReady()
BatonPassExecuted(string from, string to, int level)
TechnicalDamage(string attacker, string target, string combo)
ShowtimeTriggered(string name, string char1, string char2)
BattleEnded(bool victory)
```

---

## 🎮 Battle Flow

```
1. InitializeBattle() → Calculate turn order
2. StartNextTurn() → Get next actor
3. Player/Enemy acts → ExecuteAction()
4. Check weakness/crit → Grant One More?
5. Baton Pass available? → Player choice
6. All enemies down? → All-Out Attack prompt
7. Check battle end → Victory/Defeat
8. Repeat from step 2
```

---

## 🛠️ Common Code Snippets

### Create Skill with Status
```csharp
var skill = new SkillData
{
    SkillId = "agi",
    DisplayName = "Agi",
    Element = ElementType.Fire,
    BasePower = 80,
    MPCost = 10,
    InflictsStatuses = new() { StatusEffect.Burn },
    StatusChances = new() { 30 }
};
```

### Set Enemy Weakness
```csharp
enemy.ElementAffinities.SetAffinity(ElementType.Ice, ElementAffinity.Weak);
```

### Create Showtime
```csharp
var showtime = new ShowtimeAttackData
{
    Character1Id = "Dominic",
    Character2Id = "Echo Walker",
    AttackName = "Twilight Judgment",
    BasePower = 400,
    DamageMultiplier = 3.0f,
    TriggerChance = 15
};
```

### Check Battle State
```csharp
bool playerTurn = battleManager.IsPlayerTurn();
bool canPass = battleManager.CanBatonPass();
bool canAllOut = battleManager.CanUseAllOutAttack();
List<BattleMember> targets = battleManager.GetBatonPassTargets();
```

---

## 📂 File Locations

```
Combat/
├── BattleManager.cs          ← Main controller
├── BattleMember.cs           ← Participant wrapper
├── BattleAction.cs           ← Action data
├── BatonPassSystem.cs        ← Baton pass
├── TechnicalDamage.cs        ← Technical combos
└── ShowtimeAttacks.cs        ← Showtimes

Testing/
├── BattleTest.cs             ← Phase 1 demo
└── Phase2BattleTest.cs       ← Phase 2 demo
```

---

## 🐛 Quick Troubleshooting

**No One More?**
- Check `result.HitWeakness` or `result.WasCritical`
- Verify `HandleOneMore()` is called

**Baton Pass Broken?**
- Target must not have acted yet
- Actor needs `HasExtraTurn = true`

**Technical Not Working?**
- Status must be in `ActiveStatuses`
- Check combo table in `TechnicalDamageSystem.cs`

**Showtime Never Triggers?**
- Increase `TriggerChance` to 100% for testing
- Check cooldown in `ShowtimeState`

---

## 📈 Damage Formula Order

```
1. Base Damage
2. × Baton Pass Bonus (if active)
3. × Technical Multiplier (if combo)
4. × Critical (if rolled)
5. × Elemental Affinity
6. = Final Damage
```

---

## 🎯 Best Practices

✅ **DO:**
- Connect all signals in _Ready()
- Use BattleActionType enum
- Check CanAct() before acting
- Cache frequently used nodes
- Test with GD.Print() liberally

❌ **DON'T:**
- Modify BattleMember directly during action
- Call ExecuteAction() during animations
- Forget to call EndTurn()
- Skip validation checks
- Mix up actor/target in signals

---

## 💡 Pro Tips

1. **Combo Chains**: Burn → Thunder → Knockdown → Baton Pass → repeat
2. **Max Damage**: Weakness + Crit + Baton 3 + Technical = 13.5x!
3. **Showtime Timing**: Save for boss phases or emergencies
4. **AI Strategy**: Prioritize weakness exploitation
5. **UI Feedback**: Always show multiplier stacking visually

---

## 🚀 Performance Checklist

- [ ] Cache BattleManager reference
- [ ] Pool damage numbers
- [ ] Batch UI updates per frame
- [ ] Use signals instead of polling
- [ ] Preload showtime animations
- [ ] Optimize status effect checks
- [ ] Limit particle effects

---

## 📚 Further Reading

- `Persona5BattleSystem_Documentation.md` - Phase 1 details
- `Phase2_Documentation.md` - Advanced features
- `CompleteIntegrationGuide.md` - Full integration

---

## 🎉 Quick Win Checklist

Starting from scratch? Follow this order:

1. ✅ Create BattleScene with BattleManager
2. ✅ Initialize with test party and enemies
3. ✅ Connect TurnStarted signal
4. ✅ Handle player input (Attack only)
5. ✅ Set enemy weakness (Ice weak to Fire)
6. ✅ Test hitting weakness → One More
7. ✅ All enemies knocked down → All-Out Attack
8. ✅ Add Baton Pass to action menu
9. ✅ Create skill with status effect
10. ✅ Test Technical combo (Burn + Thunder)
11. ✅ Create showtime resource
12. ✅ Test showtime trigger
13. ✅ Add animations and polish
14. ✅ Ship your game! 🚀

---

**Print this page and keep it next to your keyboard!** 📄✨