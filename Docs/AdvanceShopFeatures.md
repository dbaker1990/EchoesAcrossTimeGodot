# 🌟 Advanced Shop Features

## Optional enhancements for your shop system

---

## 💰 Sales & Discounts

### Flash Sales
```csharp
// Add to ShopItem
[Export] public bool IsOnSale { get; set; } = false;
[Export] public float SaleDiscountPercent { get; set; } = 0.25f; // 25% off

// In ShopUI when displaying price
int displayPrice = shopItem.BuyPrice;
if (shopItem.IsOnSale)
{
    int salePrice = Mathf.RoundToInt(displayPrice * (1.0f - shopItem.SaleDiscountPercent));
    itemPriceLabel.Text = $"~~{displayPrice}G~~ {salePrice}G SALE!";
}
```

### Limited-Time Offers
```csharp
// Add to ShopItem
[Export] public bool IsLimitedTime { get; set; } = false;
[Export] public double OfferExpiresAt { get; set; } = 0; // Unix timestamp

// Check expiration
public bool IsOfferActive()
{
    if (!IsLimitedTime) return true;
    return Time.GetUnixTimeFromSystem() < OfferExpiresAt;
}

// Display countdown
TimeSpan timeLeft = TimeSpan.FromSeconds(OfferExpiresAt - Time.GetUnixTimeFromSystem());
expirationLabel.Text = $"Expires in: {timeLeft.Hours}h {timeLeft.Minutes}m";
```

---

## 📊 Shop Reputation System

### Implementation
```csharp
// Add to ShopData
[Export] public int ReputationLevel { get; set; } = 1;
[Export] public int ReputationPoints { get; set; } = 0;

// Reputation perks
public float GetReputationDiscount()
{
    return ReputationLevel switch
    {
        >= 10 => 0.25f, // 25% off
        >= 7 => 0.15f,  // 15% off
        >= 4 => 0.10f,  // 10% off
        _ => 0.0f
    };
}

// Gain reputation on purchase
public void OnItemBought(string itemId, int quantity, int cost)
{
    if (CurrentShop != null)
    {
        CurrentShop.ReputationPoints += cost / 100; // 1 point per 100G spent
        
        // Level up check
        while (CurrentShop.ReputationPoints >= GetRequiredPointsForLevel(CurrentShop.ReputationLevel + 1))
        {
            CurrentShop.ReputationLevel++;
            ShowReputationLevelUp();
        }
    }
}
```

### Reputation Tiers
```
Level 1 (0 pts)   - New Customer
Level 2 (100 pts) - Regular
Level 3 (250 pts) - Valued Customer - 5% discount
Level 4 (500 pts) - Preferred - 10% discount
Level 5 (1000 pts) - VIP - 15% discount, exclusive items
Level 10 (5000 pts) - Legendary Patron - 25% discount, special quest
```

---

## 🎁 Loyalty Rewards

### Points System
```csharp
// Add to SaveData
[Export] public int ShopLoyaltyPoints { get; set; } = 0;

// Earn points
public void OnPurchase(int goldSpent)
{
    int pointsEarned = goldSpent / 10; // 1 point per 10G
    ShopLoyaltyPoints += pointsEarned;
    ShowPointsEarned(pointsEarned);
}

// Redeem points
public bool RedeemPoints(int points, string reward)
{
    if (ShopLoyaltyPoints >= points)
    {
        ShopLoyaltyPoints -= points;
        GiveReward(reward);
        return true;
    }
    return false;
}
```

### Reward Catalog
```
100 pts → Potion
250 pts → Hi-Potion
500 pts → Phoenix Down
1000 pts → Rare weapon
2500 pts → Legendary item
5000 pts → Ultimate reward
```

---

## 🏷️ Dynamic Pricing

### Supply & Demand
```csharp
// Track purchases
[Export] public int TotalPurchases { get; set; } = 0;

public int GetDynamicPrice(int basePrice)
{
    // Price increases with demand
    float demandMultiplier = 1.0f + (TotalPurchases * 0.01f); // +1% per purchase
    demandMultiplier = Mathf.Clamp(demandMultiplier, 1.0f, 1.5f); // Max +50%
    
    return Mathf.RoundToInt(basePrice * demandMultiplier);
}
```

### Time-Based Pricing
```csharp
public float GetTimePriceMultiplier()
{
    int hour = Time.GetDatetimeDictFromSystem()["hour"].AsInt32();
    
    // Happy hour (cheap)
    if (hour >= 14 && hour <= 16)
        return 0.85f; // 15% off
    
    // Peak hours (expensive)
    if (hour >= 18 && hour <= 20)
        return 1.15f; // 15% markup
    
    return 1.0f;
}
```

---

## 📦 Bulk Discounts

### Implementation
```csharp
public float GetBulkDiscount(int quantity)
{
    return quantity switch
    {
        >= 20 => 0.20f, // 20% off
        >= 10 => 0.15f, // 15% off
        >= 5 => 0.10f,  // 10% off
        _ => 0.0f
    };
}

// Apply when buying
int basePrice = shopItem.BuyPrice * quantity;
float discount = GetBulkDiscount(quantity);
int finalPrice = Mathf.RoundToInt(basePrice * (1.0f - discount));

if (discount > 0)
{
    totalCostLabel.Text = $"~~{basePrice}G~~ {finalPrice}G ({discount * 100}% bulk discount!)";
}
```

---

## 🎲 Random Shop Events

### Daily Specials
```csharp
// In shop initialization
public void GenerateDailySpecial()
{
    int dayOfYear = Time.GetDatetimeDictFromSystem()["day"].AsInt32();
    var rng = new RandomNumberGenerator();
    rng.Seed = (ulong)dayOfYear; // Same special each day
    
    // Pick random item
    int index = rng.RandiRange(0, ItemsForSale.Count - 1);
    var specialItem = ItemsForSale[index];
    
    specialItem.IsFeatured = true;
    specialItem.IsOnSale = true;
    specialItem.SaleDiscountPercent = 0.3f; // 30% off
    
    GD.Print($"Today's special: {specialItem.ItemId}");
}
```

### Mystery Box
```csharp
// Add mystery item to shop
public ShopItem CreateMysteryBox()
{
    return new ShopItem
    {
        ItemId = "mystery_box",
        BuyPrice = 500,
        IsUnlocked = true,
        IsFeatured = true
    };
}

// When purchased
public void OpenMysteryBox()
{
    var rng = new RandomNumberGenerator();
    rng.Randomize();
    
    var possibleItems = new List<string> 
    { 
        "potion", "ether", "rare_gem", "legendary_sword" 
    };
    
    int index = rng.RandiRange(0, possibleItems.Count - 1);
    string wonItem = possibleItems[index];
    
    AddItemToInventory(wonItem, 1);
    ShowNotification($"You got: {wonItem}!");
}
```

---

## 🔄 Trade-In System

### Exchange Items for Discounts
```csharp
// Add to ShopItem
[Export] public Array<string> AcceptedTradeIns { get; set; } = new();
[Export] public int TradeInValue { get; set; } = 0;

// Trading logic
public bool TradeInItem(string itemId, string targetItemId)
{
    var shopItem = CurrentShop?.GetShopItem(targetItemId);
    if (shopItem == null) return false;
    
    if (!shopItem.AcceptedTradeIns.Contains(itemId))
    {
        ShowNotification("This item can't be traded in here!");
        return false;
    }
    
    if (!HasItemInInventory(itemId, 1))
        return false;
    
    // Remove trade-in item
    RemoveItemFromInventory(itemId, 1);
    
    // Calculate discount
    int discountedPrice = shopItem.BuyPrice - shopItem.TradeInValue;
    
    if (GetPlayerGold() >= discountedPrice)
    {
        SpendGold(discountedPrice);
        AddItemToInventory(targetItemId, 1);
        ShowNotification($"Trade-in successful! Paid only {discountedPrice}G");
        return true;
    }
    
    // Refund if can't afford
    AddItemToInventory(itemId, 1);
    return false;
}
```

---

## 🌟 VIP Shop System

### Exclusive Access
```csharp
// Add VIP-only shops
public partial class VIPShopData : ShopData
{
    [Export] public int RequiredVIPLevel { get; set; } = 1;
    [Export] public int VIPMembershipCost { get; set; } = 5000;
}

// Check access
public bool CanAccessVIPShop(VIPShopData shop)
{
    int playerVIPLevel = GetPlayerVIPLevel();
    return playerVIPLevel >= shop.RequiredVIPLevel;
}

// Purchase membership
public bool PurchaseVIPMembership(int level)
{
    int cost = level * 5000;
    if (GetPlayerGold() >= cost)
    {
        SpendGold(cost);
        SetPlayerVIPLevel(level);
        ShowNotification($"You are now VIP Level {level}!");
        return true;
    }
    return false;
}
```

---

## 📈 Shop Quests

### Shop-Specific Missions
```csharp
// Shop quest example
public class ShopQuest
{
    public string QuestId { get; set; }
    public string Description { get; set; }
    public int RequiredPurchases { get; set; }
    public int RewardGold { get; set; }
    public string RewardItem { get; set; }
}

// Example quest
var quest = new ShopQuest
{
    QuestId = "big_spender",
    Description = "Spend 10,000G at this shop",
    RequiredPurchases = 10000,
    RewardGold = 2000,
    RewardItem = "vip_card"
};

// Track progress
public void TrackShopQuest(int goldSpent)
{
    foreach (var quest in activeShopQuests)
    {
        quest.CurrentProgress += goldSpent;
        
        if (quest.CurrentProgress >= quest.RequiredPurchases)
        {
            CompleteShopQuest(quest);
        }
    }
}
```

---

## 🎯 Achievements

### Shop-Related Achievements
```
"First Purchase" - Buy your first item
"Big Spender" - Spend 100,000G total
"Bargain Hunter" - Buy 10 items on sale
"VIP Customer" - Reach max reputation at 5 shops
"Collector" - Buy every item from one shop
"Shop 'til You Drop" - Visit 20 different shops
"Mystery Master" - Open 50 mystery boxes
"Trade King" - Complete 100 trade-ins
```

---

## 💎 Premium Currency

### Gem Shop (Optional)
```csharp
// Add premium currency
[Export] public int PremiumGems { get; set; } = 0;

// Gem-exclusive items
public class GemShopItem : ShopItem
{
    [Export] public int GemCost { get; set; } = 0;
    [Export] public bool RequiresPremiumCurrency { get; set; } = true;
}

// Purchase with gems
public bool BuyWithGems(string itemId, int quantity)
{
    var item = CurrentShop?.GetShopItem(itemId) as GemShopItem;
    if (item == null) return false;
    
    int totalCost = item.GemCost * quantity;
    
    if (PremiumGems >= totalCost)
    {
        PremiumGems -= totalCost;
        AddItemToInventory(itemId, quantity);
        return true;
    }
    
    return false;
}
```

---

## 🎪 Seasonal Shops

### Limited-Time Shops
```csharp
public class SeasonalShop : ShopData
{
    [Export] public int StartMonth { get; set; } = 12; // December
    [Export] public int EndMonth { get; set; } = 1;    // January
    
    public bool IsSeasonActive()
    {
        int currentMonth = Time.GetDatetimeDictFromSystem()["month"].AsInt32();
        
        if (StartMonth <= EndMonth)
            return currentMonth >= StartMonth && currentMonth <= EndMonth;
        else
            return currentMonth >= StartMonth || currentMonth <= EndMonth;
    }
}

// Examples
// - Winter Holiday Shop (Dec-Jan)
// - Summer Festival Shop (Jul-Aug)
// - Halloween Shop (October)
// - Spring Sale (Mar-Apr)
```

---

## 🎨 Visual Enhancements

### Shop Themes
```csharp
public enum ShopTheme
{
    Modern,
    Medieval,
    Futuristic,
    Fantasy,
    Dark,
    Luxury
}

// Apply theme
public void ApplyShopTheme(ShopTheme theme)
{
    switch (theme)
    {
        case ShopTheme.Luxury:
            shopPanel.AddThemeColorOverride("bg_color", new Color(0.2f, 0.15f, 0.3f));
            // Gold accents, elegant fonts
            break;
        case ShopTheme.Dark:
            shopPanel.AddThemeColorOverride("bg_color", new Color(0.1f, 0.1f, 0.1f));
            // Dark theme with red accents
            break;
    }
}
```

---

## 📊 Analytics

### Track Shop Statistics
```csharp
public class ShopAnalytics
{
    public int TotalTransactions { get; set; }
    public int TotalGoldSpent { get; set; }
    public int TotalGoldEarned { get; set; }
    public Dictionary<string, int> ItemsSold { get; set; } = new();
    public Dictionary<string, int> ItemsBought { get; set; } = new();
    public string MostPopularItem { get; set; }
    public int AverageTransactionValue { get; set; }
}

// Display to player
public void ShowShopStats()
{
    GD.Print($"Total purchases: {analytics.TotalTransactions}");
    GD.Print($"Total spent: {analytics.TotalGoldSpent}G");
    GD.Print($"Favorite item: {analytics.MostPopularItem}");
}
```

---

## 🎉 Summary

These advanced features can transform your basic shop into a deep, engaging system! Pick and choose what fits your game:

**Easy to Add:**
- Sales & discounts
- Bulk discounts
- Daily specials
- Shop reputation

**Medium Complexity:**
- Loyalty points
- Dynamic pricing
- Trade-in system
- Shop quests

**Advanced:**
- VIP system
- Premium currency
- Seasonal shops
- Full analytics

Mix and match to create the perfect shop experience for your JRPG! 🏪✨