# Smart AI Integration Guide

## 🎯 What's New

Your AI system now has **three major upgrades**:

### 1. **Technical Damage Awareness** ⚡
- AI detects when enemies have status effects (Burn, Freeze, Shock, etc.)
- AI knows which element creates a technical combo
- AI prioritizes technical opportunities for 1.5x damage

### 2. **Smart Weakness Exploitation** 🎯
- AI learns weaknesses mid-battle
- AI remembers which elements work against each character
- AI prioritizes hitting weaknesses for 1.5x damage + One More

### 3. **Defensive Behaviors** 🛡️
- AI guards when low on HP and no healing available
- AI adapts strategy based on threat level
- AI uses emergency skills when in danger

---

## 📦 Files Provided

1. **Enhanced_AIPattern.cs** - Core AI with new smart features
2. **AIIntegrationHelper.cs** - Connects AI to BattleManager
3. **SmartEnemySetup.cs** - 7 pre-configured AI templates
4. **SmartAI_TestDemo.cs** - Demonstration/testing script

---

## 🚀 Quick Setup (5 Minutes)

### Step 1: Replace AIPattern.cs
```
Replace: Combat/AIPattern.cs
With: Enhanced_AIPattern.cs
```

### Step 2: Add Integration Helper
```
Create: Combat/AIIntegrationHelper.cs
Add as child node to your BattleScene
```

### Step 3: Configure Enemies

#### Option A: Use Pre-configured Templates
```csharp
using EchoesAcrossTime.Examples;

// In your enemy setup code
var enemyData = ResourceLoader.Load<CharacterData>("res://Data/Enemies/Goblin.tres");
enemyData.AIBehavior = SmartEnemySetup.CreateTacticalMage();
```

#### Option B: Create Custom AI in Godot Editor
1. Open enemy CharacterData resource
2. Under "AI Behavior" section:
    - Set `BehaviorType` = Tactical
    - Set `LearnWeaknesses` = true
    - Set `ExploitTechnicals` = true
    - Set `TechnicalPriority` = 80
    - Set `WeaknessPriority` = 90

---

## 🎮 AI Behavior Types

### Tactical (Recommended)
- **Best for**: Mages, smart enemies, mini-bosses
- Exploits weaknesses
- Uses technical combos
- Learns from combat

### Defensive
- **Best for**: Tanks, healers, support enemies
- Guards when threatened
- Prioritizes healing
- Protects allies

### Aggressive
- **Best for**: Physical attackers, berserkers
- Maximum damage output
- High skill usage
- Reckless behavior

### Balanced
- **Best for**: Standard enemies
- Mix of offense/defense
- Adapts to situation
- Versatile

### Supportive
- **Best for**: Healer enemies, buff/debuff specialists
- Focuses on support skills
- Heals allies
- Debuffs players

### Berserk
- **Best for**: Wild enemies, chaos fights
- Random powerful attacks
- No strategy
- Unpredictable

### Cowardly
- **Best for**: Weak enemies, scouts
- Flees when threatened
- Low aggression
- Defensive focus

---

## ⚙️ Key Settings Explained

### Technical Priority (0-100)
```csharp
TechnicalPriority = 80
```
- **80-100**: Always seeks technicals (highly tactical)
- **50-79**: Frequently uses technicals (balanced)
- **0-49**: Rarely uses technicals (prefer other tactics)

### Weakness Priority (0-100)
```csharp
WeaknessPriority = 90
```
- **90-100**: Almost always hits weaknesses when possible
- **70-89**: Often hits weaknesses (smart)
- **50-69**: Sometimes hits weaknesses (average)
- **0-49**: Rarely exploits weaknesses (dumb)

### Defensive Threshold (0-100)
```csharp
DefensiveThreshold = 60
```
- HP % at which AI starts considering defense
- 60 = Defends when below 60% HP
- Lower = More aggressive (25-40)
- Higher = More cautious (70-90)

---

## 🎯 Example Configurations

### Boss Fight (Challenging)
```csharp
var bossAI = new AIPattern
{
    BehaviorType = AIBehaviorType.Tactical,
    LearnWeaknesses = true,
    ExploitTechnicals = true,
    UseDefensiveTactics = true,
    TechnicalPriority = 85,
    WeaknessPriority = 90,
    DefensiveThreshold = 40,
    
    // Turn pattern for predictability
    HasTurnPattern = true,
    TurnPattern = new() { "charge", "megidola", "heat_riser" },
    
    // Emergency skills
    EmergencySkillIds = new() { "diarahan", "megidolaon" }
};
```

### Mook Enemy (Easy)
```csharp
var mookAI = new AIPattern
{
    BehaviorType = AIBehaviorType.Aggressive,
    LearnWeaknesses = false,  // Too dumb to learn
    ExploitTechnicals = false,
    TechnicalPriority = 0,
    WeaknessPriority = 30,
    Aggression = 60
};
```

### Elite Enemy (Medium-Hard)
```csharp
var eliteAI = new AIPattern
{
    BehaviorType = AIBehaviorType.Tactical,
    LearnWeaknesses = true,
    ExploitTechnicals = true,
    TechnicalPriority = 70,
    WeaknessPriority = 75,
    UseDefensiveTactics = true,
    DefensiveThreshold = 50
};
```

---

## 🔍 How It Works

### Turn Flow
```
1. Enemy Turn Starts
   ↓
2. Check Emergency Conditions
   - Low HP? → Use emergency skills
   ↓
3. Check Defensive Needs
   - Should guard? → Guard action
   ↓
4. Check Technical Opportunities (if enabled)
   - Scan all targets for status effects
   - Check if any skill creates technical
   - Priority check: Use if TechnicalPriority % succeeds
   ↓
5. Check Weakness Opportunities (if enabled)
   - Check learned weaknesses
   - Try to discover new weaknesses
   - Priority check: Use if WeaknessPriority % succeeds
   ↓
6. Follow Turn Pattern (if set)
   ↓
7. Use Behavior Type Logic
   - Aggressive → Damage skills
   - Defensive → Healing/buffs
   - Tactical → Smart targeting
   etc.
   ↓
8. Execute Action
```

### Learning System
```
First Time Hitting Target:
  Try Fire → Normal damage
  Try Ice → WEAKNESS! ★
  ↓
  Save to memory: "PlayerID" → Weak to Ice
  
Next Turn:
  Check memory → Player weak to Ice
  Priority: Use Ice skills
  Result: Consistent weakness hits!
```

---

## 🐛 Troubleshooting

### AI Not Using Technicals
**Problem**: AI never triggers technical combos
**Solutions**:
1. Check `ExploitTechnicals = true`
2. Increase `TechnicalPriority` (try 90-100)
3. Verify enemy has correct element skills
4. Ensure targets actually have status effects
5. Check TechnicalDamageSystem is initialized

### AI Not Learning Weaknesses
**Problem**: AI doesn't remember weaknesses
**Solutions**:
1. Check `LearnWeaknesses = true`
2. Increase `WeaknessPriority`
3. Verify enemy has multiple element skills to test
4. Check that affinity system is working

### AI Too Defensive
**Problem**: AI guards too often
**Solutions**:
1. Lower `DefensiveThreshold` (try 30-40)
2. Set `UseDefensiveTactics = false`
3. Increase `Aggression` stat (70-100)

### AI Not Smart Enough
**Problem**: AI feels random/dumb
**Solutions**:
1. Change to `Tactical` behavior type
2. Enable `LearnWeaknesses = true`
3. Enable `ExploitTechnicals = true`
4. Set high priorities: Technical=90, Weakness=85
5. Add preferred skill IDs

---

## 📊 AI Comparison

| Feature | Old AI | Smart AI |
|---------|--------|----------|
| Technical Awareness | ❌ No | ✅ Yes |
| Learns Weaknesses | ❌ No | ✅ Yes |
| Defensive Tactics | ❌ No | ✅ Yes |
| Priority System | ❌ No | ✅ Yes |
| Reasoning Debug | ❌ No | ✅ Yes |
| Adaptive | ❌ No | ✅ Yes |

---

## 🎓 Best Practices

### 1. **Match AI to Enemy Role**
```
Tank → Defensive
Mage → Tactical  
Berserker → Aggressive/Berserk
Healer → Supportive
```

### 2. **Scale Difficulty with AI**
```
Early Game: LearnWeaknesses = false, TechnicalPriority = 0
Mid Game: LearnWeaknesses = true, TechnicalPriority = 50
Late Game: LearnWeaknesses = true, TechnicalPriority = 90
```

### 3. **Give Boss Patterns**
```csharp
// Makes fights memorable and learnable
HasTurnPattern = true
TurnPattern = ["buff", "charge", "nuke", "heal"]
```

### 4. **Balance Emergency Skills**
```csharp
// Don't make unbeatable
LowHPThreshold = 30  // Not too high
EmergencySkillIds = ["heal"]  // Not instant-win
```

---

## 🚀 Next Steps

1. **Test the demos**: Run `SmartAI_TestDemo.cs`
2. **Create your first smart enemy**: Use a template
3. **Test in combat**: See how it feels
4. **Adjust priorities**: Fine-tune for your game
5. **Create custom patterns**: Make unique bosses

---

## 💡 Pro Tips

- **Debug Mode**: Check console for `[AI]` messages to see reasoning
- **Start Simple**: Use pre-configured templates first
- **Iterate**: Adjust priorities based on playtesting
- **Mix & Match**: Use different AI for enemy variety
- **Boss Gimmicks**: Combine patterns with emergency skills

---

## ✅ Quick Checklist

- [ ] Replaced AIPattern.cs with enhanced version
- [ ] Added AIIntegrationHelper to BattleScene
- [ ] Configured at least one enemy with smart AI
- [ ] Tested technical damage detection
- [ ] Tested weakness learning
- [ ] Tested defensive behavior
- [ ] Adjusted priorities to match desired difficulty
- [ ] Verified console logging for debugging

---

## 🎉 You're Done!

Your enemies are now **significantly smarter**! They will:
- ✅ Exploit technical combos
- ✅ Learn and remember weaknesses
- ✅ Defend when threatened
- ✅ Adapt their tactics mid-battle

**The player will notice the difference immediately!**