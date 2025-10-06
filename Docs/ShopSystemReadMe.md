# 🏪 Complete Shop System for Nocturne Requiem

## 📦 What Was Created

A **production-ready shop system** with buy/sell functionality, stock management, unlock conditions, and full integration with your existing JRPG systems!

---

## 📋 Complete File List

### ✅ Core System Files (4 files)
1. **ShopData.cs** - Shop and item resource definitions
    - Define shops in Godot editor as .tres files
    - Just set items in an array - that's it!

2. **ShopManager.cs** - Singleton shop controller
    - Autoload manager for all shop operations
    - Buy/sell transactions with full validation

3. **ShopUI.cs** - Complete shop interface
    - Buy and sell tabs
    - Item details panel with quantity selector
    - Gold display and transaction feedback

4. **ShopTrigger.cs** - NPC/area shop triggers
    - Attach to NPCs to make them shopkeepers
    - Automatic or interaction-based opening

### ✅ Helper Files (5 files)
5. **ShopSceneBuilder.cs** - Auto-generate ShopUI.tscn
    - Run once to create complete UI structure
    - Saves hours of manual scene building

6. **ShopSaveIntegration.cs** - Save/load support
    - Persist shop stocks and unlock states
    - Easy integration with existing save system

7. **ShopNotification.cs** - Notification popups
    - "Item purchased!" messages
    - "Shop unlocked!" alerts
    - Animated slide-in notifications

8. **ShopManager_Additions.cs** - Extended functionality
    - Utility methods for shop management
    - Unlock shops/items programmatically
    - Reputation and advanced features

9. **ExampleShops.cs** - 6 ready-to-use shops
    - General Store, Weapon Shop, Armor Shop
    - Magic Shop, Black Market, Inn
    - Full examples with varied configurations

### ✅ Documentation (4 files)
10. **ShopSystemGuide.md** - Complete integration guide
    - Step-by-step setup instructions
    - Full feature documentation

11. **ShopSystemQuickReference.md** - Quick commands
    - Copy-paste code snippets
    - Common use cases

12. **AdvancedShopFeatures.md** - Optional enhancements
    - Sales, discounts, loyalty programs
    - Dynamic pricing, VIP systems

13. **ShopSystemComplete.md** - Summary and checklist
    - What's included overview
    - Integration checklist

### ✅ This File
14. **SHOP_SYSTEM_README.md** - You are here!

**Total: 14 files ready to integrate!** 🚀

---

## ⚡ Quick Start (15 Minutes)

### Step 1: Copy Files to Project
```
YourProject/
├── Shops/
│   ├── ShopData.cs
│   ├── ShopManager.cs
│   ├── ShopUI.cs
│   ├── ShopTrigger.cs
│   ├── ShopNotification.cs
│   ├── ShopSaveIntegration.cs
│   ├── ExampleShops.cs
│   └── ShopSceneBuilder.cs
└── Docs/
    └── (documentation files)
```

### Step 2: Add ShopManager to Autoload
```
Project Settings → Autoload
Name: ShopManager
Path: res://Shops/ShopManager.cs
✓ Enable
```

### Step 3: Build UI Scene
```
1. Create new scene
2. Attach ShopSceneBuilder.cs to any node
3. Check "Build Shop UI Scene" in inspector
4. Scene auto-generates at res://Shops/ShopUI.tscn
5. Open ShopUI.tscn and attach ShopUI.cs
6. Assign exported node references
```

**OR** manually create ShopUI.tscn using the structure in ShopSystemGuide.md

### Step 4: Add to Main Scene
```gdscript
# In your main game scene _ready()
var shop_ui = preload("res://Shops/ShopUI.tscn").instantiate()
shop_ui.unique_name_in_owner = true
add_child(shop_ui)
```

### Step 5: Create Shop Resources
```
1. Right-click in FileSystem → New Resource → ShopData
2. Set Shop ID, Name, Description
3. Add items to "Items For Sale" array
4. For each item, set Item ID, Buy Price, Stock
5. Save as .tres (e.g., GeneralStore.tres)
```

### Step 6: Add to NPCs
```
NPC Scene:
├── CharacterBody2D
├── Sprite2D
├── Area2D
│   └── CollisionShape2D
└── ShopTrigger (Script)
    Export: ShopToOpen = GeneralStore.tres
```

### Step 7: Connect to Your Systems

**In ShopManager.cs, update:**
```csharp
// Gold system
private int GetPlayerGold()
{
    return SaveManager.Instance.CurrentSave.Gold;
}

private bool SpendGold(int amount)
{
    if (SaveManager.Instance.CurrentSave.Gold >= amount)
    {
        SaveManager.Instance.CurrentSave.Gold -= amount;
        return true;
    }
    return false;
}

// Inventory system
private bool AddItemToInventory(string itemId, int quantity)
{
    return InventorySystem.Instance.AddItem(itemId, quantity);
}

// Item database
private int GetItemValue(string itemId)
{
    return GameDatabase.Instance.GetItem(itemId)?.Value ?? 100;
}
```

**Done!** Talk to NPCs to open shops! 🎉

---

## 🎮 How to Use

### Creating Shops in Godot Editor
```
1. Data/Shops/WeaponShop.tres:
   - Shop ID: "weapon_shop_01"
   - Shop Name: "Steel & Edge Armory"
   - Items For Sale:
     [0] Iron Sword (100G, unlimited)
     [1] Steel Sword (250G, level 5 required)
     [2] Mythril Sword (800G, quest locked)
```

### Opening Shops in Code
```csharp
// Via NPC trigger - automatic!
// Just talk to NPC with ShopTrigger attached

// Or manually:
ShopManager.Instance.OpenShop("weapon_shop_01");
ShopManager.Instance.CloseShop();
```

### Creating Shops in Code
```csharp
// In your game init
ExampleShops.RegisterAllExampleShops();

// Or load from folder
ShopManager.Instance.LoadShopsFromFolder("res://Data/Shops");
```

---

## ✨ Features Overview

### Core Features
- ✅ **Buy items** from shops with gold
- ✅ **Sell items** back to shops (configurable price %)
- ✅ **Stock management** (unlimited or limited)
- ✅ **Auto-restocking** on shop visit (optional)
- ✅ **Quantity selector** for bulk purchases
- ✅ **Transaction validation** (gold check, stock check, etc.)

### Item Features
- ✅ **Unlock conditions** (quests, levels, story progress)
- ✅ **Featured items** (★ marker in UI)
- ✅ **New items** ("NEW!" tag)
- ✅ **Show locked items** as ??? (optional)
- ✅ **Per-item pricing** - each shop sets own prices

### Shop Features
- ✅ **Multiple shop types** (weapons, armor, magic, etc.)
- ✅ **Shop unlock conditions** (quest-locked shops)
- ✅ **Shopkeeper info** (name, portrait, description)
- ✅ **Buy-only or buy/sell** shops
- ✅ **Customizable sell price** (default 50% of buy price)

### Integration
- ✅ **Save/Load support** - stocks and unlocks persist
- ✅ **Notification system** - transaction feedback
- ✅ **Signal-based** - easy to extend
- ✅ **Sound effects** - integrates with SystemManager
- ✅ **Quest system** - item unlocks via quests
- ✅ **Inventory system** - full integration
- ✅ **Gold/Economy** - connects to save system

---

## 🎯 Example Shops Included

### 1. General Store
- Potions, ethers, antidotes, phoenix downs
- Unlimited stock
- Can buy and sell
- Always available

### 2. Weapon Shop
- Iron/Steel/Mythril swords, legendary blade
- Limited stock, auto-restock on visit
- Level requirements on advanced items
- Buy-only (no selling)

### 3. Armor Shop
- Leather → Chainmail → Plate → Dragon Scale
- Unlimited stock
- Level-gated progression
- Quest-locked endgame armor

### 4. Magic Shop
- Spell tomes (Fire, Ice, Thunder, Ultima)
- Limited stock, no restock (rare items)
- Higher sell price (60% instead of 50%)
- Master mage quest requirement for Ultima

### 5. Black Market
- Rare gems, stolen goods, ancient relics
- Limited stock, no restock
- High sell prices (75%)
- Unlocked via secret quest
- Discounted "hot" items

### 6. Inn Shop
- Travel supplies, rations, tents
- Unlimited stock
- Slightly higher prices
- Low sell prices (40%)

---

## 🔗 System Integration Points

### Required Integration
```csharp
// MUST implement these in ShopManager.cs
GetPlayerGold()      → Get current gold amount
SpendGold(amount)    → Deduct gold, return success
AddGold(amount)      → Add gold to player
```

```csharp
// MUST implement these in ShopManager.cs  
AddItemToInventory(id, qty)    → Add item, return success
RemoveItemFromInventory(id, qty) → Remove item, return success
HasItemInInventory(id, qty)    → Check if player has item
```

### Optional Integration
```csharp
// ShopUI.cs - for item display
GetItemData(id)      → Get item name, desc, icon
GetPlayerInventory() → Get all items player owns

// ShopManager.cs - for sell prices
GetItemValue(id)     → Get item's base sell value

// Both - for sound effects
PlayPurchaseSound()
PlaySellSound()
PlayErrorSound()
```

### Save System Integration
```csharp
// In SaveData.cs
[Export] public ShopSaveData Shops { get; set; } = new();

// In CaptureCurrentState()
Shops = ShopSaveIntegration.CaptureShopData();

// In ApplyToGame()
ShopSaveIntegration.RestoreShopData(Shops);
```

---

## 📡 Signals Reference

```csharp
// Listen for shop events
ShopManager.Instance.ShopOpened += (shop) => {
    GD.Print($"Opened: {shop.ShopName}");
};

ShopManager.Instance.ShopClosed += () => {
    // Shop closed
};

ShopManager.Instance.ItemBought += (itemId, qty, cost) => {
    // Track purchases, achievements, etc.
};

ShopManager.Instance.ItemSold += (itemId, qty, value) => {
    // Track sales
};

ShopManager.Instance.TransactionFailed += (reason) => {
    // Show error to player
};
```

---

## 🎨 UI Customization

The ShopUI can be fully customized:
- Change themes, colors, fonts
- Add animations and transitions
- Modify layout and spacing
- Add category tabs
- Include search functionality
- Add item comparison features
- Show equipped items inline

See **ShopSystemGuide.md** for complete UI structure.

---

## 🔧 Advanced Features (Optional)

See **AdvancedShopFeatures.md** for:
- 💰 Sales & discounts
- 📊 Reputation system
- 🎁 Loyalty rewards
- 🏷️ Dynamic pricing
- 📦 Bulk discounts
- 🎲 Daily specials
- 🔄 Trade-in system
- 🌟 VIP memberships
- 📈 Shop quests
- 🎯 Achievements
- 💎 Premium currency
- 🎪 Seasonal shops

---

## ✅ Integration Checklist

### Setup
- [ ] All shop files copied to project
- [ ] ShopManager added to autoload
- [ ] ShopUI scene created
- [ ] ShopUI added to main scene
- [ ] At least one shop resource created
- [ ] ShopTrigger added to one NPC

### System Connections
- [ ] Gold system connected (Get/Spend/Add)
- [ ] Inventory system connected (Add/Remove/Has)
- [ ] Item database connected (GetItem, GetValue)
- [ ] Save system updated (ShopSaveData added)
- [ ] Sound effects connected (optional)
- [ ] Quest system connected (optional)

### Testing
- [ ] Can open shop via NPC
- [ ] Can buy item (gold deducted, item added)
- [ ] Can sell item (gold added, item removed)
- [ ] Out of gold shows error
- [ ] Out of stock shows error (if limited)
- [ ] Locked items show as ???
- [ ] Shop data saves/loads correctly

### Polish
- [ ] UI styled to match game
- [ ] Sound effects playing
- [ ] Notifications working
- [ ] All shops created and placed
- [ ] Prices balanced
- [ ] Unlock progression designed

---

## 🎓 Best Practices

### Shop Design
- Start with 1-2 shops to test
- Keep starter shops simple (unlimited stock)
- Add complexity gradually (limited stock, unlocks)
- Balance prices carefully
- Test the full buy → use → sell loop

### Economy Balance
- Early game: 50-200G items
- Mid game: 200-1000G items
- Late game: 1000-10000G items
- Sell price: 40-50% of buy price
- Ensure players can earn back money

### Item Progression
- Level 1-5: Basic equipment
- Level 5-10: Intermediate gear
- Level 10-20: Advanced equipment
- Level 20+: Legendary items
- Use quest unlocks for special items

---

## 🐛 Common Issues

**Shop won't open:**
- ShopManager in autoload? ✓
- ShopData assigned to trigger? ✓
- Shop IsUnlocked = true? ✓

**Can't buy items:**
- Enough gold? ✓
- Item unlocked? ✓
- Stock available? ✓
- Gold system connected? ✓

**Items not displaying:**
- ItemId matches database? ✓
- GetItemData() implemented? ✓
- Item in ItemsForSale array? ✓

**Save/Load not working:**
- ShopSaveData added to SaveData? ✓
- Capture/Restore methods called? ✓
- Shops registered before loading? ✓

---

## 📊 System Stats

- **Core Files:** 4
- **Helper Files:** 5
- **Documentation:** 5
- **Total Lines of Code:** ~2,000
- **Setup Time:** 15-30 minutes
- **Difficulty:** ⭐⭐☆☆☆ Easy-Medium

---

## 🎉 You're Done!

Your shop system is ready to use! You now have:

✅ Buy/sell functionality  
✅ Multiple shop support  
✅ Stock management  
✅ Item unlocking  
✅ Save/load integration  
✅ Full UI with notifications  
✅ 6 example shops  
✅ Complete documentation

**Next Steps:**
1. Create your shop resources
2. Place shops in your world
3. Design your game economy
4. Add advanced features as needed
5. Polish the UI to match your game's style

Happy shopkeeping! 🏪✨

---

**Questions or issues?** Check the documentation files or the inline comments in the code!