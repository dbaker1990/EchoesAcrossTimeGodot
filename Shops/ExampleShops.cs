using Godot;
using System;

namespace EchoesAcrossTime.Shops
{
    /// <summary>
    /// Example shop configurations
    /// Use this to create shops programmatically for testing
    /// </summary>
    public static class ExampleShops
    {
        /// <summary>
        /// Create a basic general store
        /// </summary>
        public static ShopData CreateGeneralStore()
        {
            var shop = new ShopData
            {
                ShopId = "general_store_01",
                ShopName = "Nocturne General Store",
                ShopDescription = "Your one-stop shop for adventuring essentials!",
                ShopkeeperName = "Marcus",
                CanSellItems = true,
                SellPriceMultiplier = 0.5f,
                HasUnlimitedStock = true,
                IsUnlocked = true
            };

            // Basic consumables
            shop.ItemsForSale.Add(new ShopItem
            {
                ItemId = "potion",
                BuyPrice = 50,
                IsUnlocked = true,
                IsFeatured = false
            });

            shop.ItemsForSale.Add(new ShopItem
            {
                ItemId = "hi_potion",
                BuyPrice = 200,
                IsUnlocked = true
            });

            shop.ItemsForSale.Add(new ShopItem
            {
                ItemId = "ether",
                BuyPrice = 100,
                IsUnlocked = true
            });

            shop.ItemsForSale.Add(new ShopItem
            {
                ItemId = "antidote",
                BuyPrice = 30,
                IsUnlocked = true
            });

            shop.ItemsForSale.Add(new ShopItem
            {
                ItemId = "phoenix_down",
                BuyPrice = 500,
                IsUnlocked = true
            });

            return shop;
        }

        /// <summary>
        /// Create a weapon shop
        /// </summary>
        public static ShopData CreateWeaponShop()
        {
            var shop = new ShopData
            {
                ShopId = "weapon_shop_01",
                ShopName = "Steel & Edge Armory",
                ShopDescription = "Finest weapons in the kingdom! Forged with pride!",
                ShopkeeperName = "Grimbold the Smith",
                CanSellItems = false, // Weapon shops don't buy back
                HasUnlimitedStock = false,
                RestocksOnVisit = true,
                IsUnlocked = true
            };

            // Starter weapon
            shop.ItemsForSale.Add(new ShopItem
            {
                ItemId = "iron_sword",
                BuyPrice = 100,
                MaxStock = 5,
                CurrentStock = 5,
                IsUnlocked = true
            });

            // Mid-tier weapon
            shop.ItemsForSale.Add(new ShopItem
            {
                ItemId = "steel_sword",
                BuyPrice = 250,
                MaxStock = 3,
                CurrentStock = 3,
                IsUnlocked = true,
                RequiredLevel = 5
            });

            // High-tier weapon
            shop.ItemsForSale.Add(new ShopItem
            {
                ItemId = "mythril_sword",
                BuyPrice = 800,
                MaxStock = 2,
                CurrentStock = 2,
                IsUnlocked = false,
                RequiredQuestId = "unlock_forge",
                ShowIfLocked = true,
                IsNewItem = true
            });

            // Featured legendary weapon
            shop.ItemsForSale.Add(new ShopItem
            {
                ItemId = "legendary_blade",
                BuyPrice = 5000,
                MaxStock = 1,
                CurrentStock = 1,
                IsUnlocked = true,
                RequiredLevel = 25,
                IsFeatured = true
            });

            return shop;
        }

        /// <summary>
        /// Create an armor shop
        /// </summary>
        public static ShopData CreateArmorShop()
        {
            var shop = new ShopData
            {
                ShopId = "armor_shop_01",
                ShopName = "Ironclad Armory",
                ShopDescription = "Protection for the wise adventurer!",
                ShopkeeperName = "Helena",
                CanSellItems = false,
                HasUnlimitedStock = true,
                IsUnlocked = true
            };

            shop.ItemsForSale.Add(new ShopItem
            {
                ItemId = "leather_armor",
                BuyPrice = 80,
                IsUnlocked = true
            });

            shop.ItemsForSale.Add(new ShopItem
            {
                ItemId = "chainmail",
                BuyPrice = 200,
                IsUnlocked = true,
                RequiredLevel = 5
            });

            shop.ItemsForSale.Add(new ShopItem
            {
                ItemId = "plate_armor",
                BuyPrice = 500,
                IsUnlocked = true,
                RequiredLevel = 10
            });

            shop.ItemsForSale.Add(new ShopItem
            {
                ItemId = "dragon_scale_armor",
                BuyPrice = 2000,
                IsUnlocked = false,
                RequiredQuestId = "dragon_slayer",
                ShowIfLocked = true,
                IsFeatured = true
            });

            return shop;
        }

        /// <summary>
        /// Create a magic shop
        /// </summary>
        public static ShopData CreateMagicShop()
        {
            var shop = new ShopData
            {
                ShopId = "magic_shop_01",
                ShopName = "Arcane Emporium",
                ShopDescription = "Mystical items and spell tomes for the discerning mage.",
                ShopkeeperName = "Elara the Sage",
                CanSellItems = true,
                SellPriceMultiplier = 0.6f, // Magic items sell for more
                HasUnlimitedStock = false,
                RestocksOnVisit = false, // Rare items don't restock
                IsUnlocked = true
            };

            shop.ItemsForSale.Add(new ShopItem
            {
                ItemId = "spell_fire",
                BuyPrice = 150,
                MaxStock = 10,
                CurrentStock = 10,
                IsUnlocked = true
            });

            shop.ItemsForSale.Add(new ShopItem
            {
                ItemId = "spell_ice",
                BuyPrice = 150,
                MaxStock = 10,
                CurrentStock = 10,
                IsUnlocked = true
            });

            shop.ItemsForSale.Add(new ShopItem
            {
                ItemId = "spell_thunder",
                BuyPrice = 150,
                MaxStock = 10,
                CurrentStock = 10,
                IsUnlocked = true
            });

            shop.ItemsForSale.Add(new ShopItem
            {
                ItemId = "spell_ultima",
                BuyPrice = 5000,
                MaxStock = 1,
                CurrentStock = 1,
                IsUnlocked = false,
                RequiredQuestId = "master_mage",
                RequiredLevel = 30,
                ShowIfLocked = true,
                IsFeatured = true
            });

            return shop;
        }

        /// <summary>
        /// Create a black market (secret shop)
        /// </summary>
        public static ShopData CreateBlackMarket()
        {
            var shop = new ShopData
            {
                ShopId = "black_market_01",
                ShopName = "The Shadow Market",
                ShopDescription = "Shhh... you didn't hear this from me...",
                ShopkeeperName = "Mysterious Vendor",
                CanSellItems = true,
                SellPriceMultiplier = 0.75f, // Black market pays more
                HasUnlimitedStock = false,
                RestocksOnVisit = false,
                IsUnlocked = false,
                RequiredQuestId = "find_black_market"
            };

            // Rare materials
            shop.ItemsForSale.Add(new ShopItem
            {
                ItemId = "rare_gem",
                BuyPrice = 800,
                MaxStock = 3,
                CurrentStock = 3,
                IsUnlocked = true
            });

            shop.ItemsForSale.Add(new ShopItem
            {
                ItemId = "ancient_relic",
                BuyPrice = 2000,
                MaxStock = 1,
                CurrentStock = 1,
                IsUnlocked = true,
                IsFeatured = true
            });

            // Stolen goods (cheap!)
            shop.ItemsForSale.Add(new ShopItem
            {
                ItemId = "stolen_sword",
                BuyPrice = 50, // Normally 250
                MaxStock = 2,
                CurrentStock = 2,
                IsUnlocked = true
            });

            return shop;
        }

        /// <summary>
        /// Create an inn/rest stop shop
        /// </summary>
        public static ShopData CreateInnShop()
        {
            var shop = new ShopData
            {
                ShopId = "inn_shop_01",
                ShopName = "Traveler's Rest Inn",
                ShopDescription = "Rest, recover, and stock up for the road ahead!",
                ShopkeeperName = "Innkeeper Thomas",
                CanSellItems = true,
                SellPriceMultiplier = 0.4f, // Inn pays less
                HasUnlimitedStock = true,
                IsUnlocked = true
            };

            // Basic supplies
            shop.ItemsForSale.Add(new ShopItem
            {
                ItemId = "potion",
                BuyPrice = 60, // Slightly more expensive than general store
                IsUnlocked = true
            });

            shop.ItemsForSale.Add(new ShopItem
            {
                ItemId = "rations",
                BuyPrice = 20,
                IsUnlocked = true
            });

            shop.ItemsForSale.Add(new ShopItem
            {
                ItemId = "tent",
                BuyPrice = 300,
                IsUnlocked = true
            });

            return shop;
        }

        /// <summary>
        /// Register all example shops
        /// Call this in your game initialization
        /// </summary>
        public static void RegisterAllExampleShops()
        {
            if (ShopManager.Instance == null)
            {
                GD.PushError("[ExampleShops] ShopManager not found!");
                return;
            }

            ShopManager.Instance.RegisterShop(CreateGeneralStore());
            ShopManager.Instance.RegisterShop(CreateWeaponShop());
            ShopManager.Instance.RegisterShop(CreateArmorShop());
            ShopManager.Instance.RegisterShop(CreateMagicShop());
            ShopManager.Instance.RegisterShop(CreateBlackMarket());
            ShopManager.Instance.RegisterShop(CreateInnShop());

            GD.Print("[ExampleShops] Registered 6 example shops!");
        }
    }

    /// <summary>
    /// Test scene for trying out shops
    /// Add to a test scene and run to demo the shop system
    /// </summary>
    public partial class ShopTestScene : Node
    {
        public override void _Ready()
        {
            GD.Print("\n=== SHOP SYSTEM TEST ===\n");

            // Wait a frame for autoloads
            CallDeferred(nameof(RunTests));
        }

        private void RunTests()
        {
            // Register example shops
            ExampleShops.RegisterAllExampleShops();

            GD.Print("\n--- Registered Shops ---");
            GD.Print("1. general_store_01 - Nocturne General Store");
            GD.Print("2. weapon_shop_01 - Steel & Edge Armory");
            GD.Print("3. armor_shop_01 - Ironclad Armory");
            GD.Print("4. magic_shop_01 - Arcane Emporium");
            GD.Print("5. black_market_01 - The Shadow Market (locked)");
            GD.Print("6. inn_shop_01 - Traveler's Rest Inn");

            GD.Print("\n--- Testing Shop Open ---");
            bool opened = ShopManager.Instance.OpenShop("general_store_01");
            GD.Print($"Open general store: {(opened ? "SUCCESS" : "FAILED")}");

            if (opened)
            {
                GD.Print($"Current shop: {ShopManager.Instance.CurrentShop.ShopName}");
                GD.Print($"Items for sale: {ShopManager.Instance.CurrentShop.ItemsForSale.Count}");
            }

            GD.Print("\n--- Testing Buy Item ---");
            // This will fail without proper gold/inventory integration
            bool bought = ShopManager.Instance.BuyItem("potion", 1);
            GD.Print($"Buy potion: {(bought ? "SUCCESS" : "FAILED (expected - need to integrate gold/inventory)")}");

            GD.Print("\n--- Shop System Ready! ---");
            GD.Print("Press UI keys to test in your game:");
            GD.Print("  - Talk to NPCs with ShopTrigger attached");
            GD.Print("  - Or call ShopManager.Instance.OpenShop(\"shop_id\")");
            GD.Print("\nDon't forget to:");
            GD.Print("  1. Connect gold system (GetPlayerGold, SpendGold, AddGold)");
            GD.Print("  2. Connect inventory system (AddItem, RemoveItem, GetItemCount)");
            GD.Print("  3. Connect item database (GetItemData, GetItemValue)");
            GD.Print("  4. Add ShopUI.tscn to your main scene");
        }

        public override void _Input(InputEvent @event)
        {
            // Press 1-6 to open different shops for testing
            if (@event is InputEventKey keyEvent && keyEvent.Pressed)
            {
                switch ((Key)keyEvent.Keycode)
                {
                    case Key.Key1:
                        ShopManager.Instance?.OpenShop("general_store_01");
                        break;
                    case Key.Key2:
                        ShopManager.Instance?.OpenShop("weapon_shop_01");
                        break;
                    case Key.Key3:
                        ShopManager.Instance?.OpenShop("armor_shop_01");
                        break;
                    case Key.Key4:
                        ShopManager.Instance?.OpenShop("magic_shop_01");
                        break;
                    case Key.Key5:
                        ShopManager.Instance?.OpenShop("black_market_01");
                        break;
                    case Key.Key6:
                        ShopManager.Instance?.OpenShop("inn_shop_01");
                        break;
                    case Key.Escape:
                        ShopManager.Instance?.CloseShop();
                        break;
                }
            }
        }
    }
}