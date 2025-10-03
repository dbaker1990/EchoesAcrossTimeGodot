using Godot;
using System.Collections.Generic;
using System.Linq;

namespace EchoesAcrossTime.Crafting
{
    /// <summary>
    /// Database containing all crafting recipes
    /// </summary>
    [GlobalClass]
    public partial class CraftingDatabase : Resource
    {
        [Export] public Godot.Collections.Array<CraftingRecipe> Recipes { get; set; } = new Godot.Collections.Array<CraftingRecipe>();

        private Dictionary<string, CraftingRecipe> recipeLookup;
        private bool isInitialized = false;

        /// <summary>
        /// Initialize the database
        /// </summary>
        public void Initialize()
        {
            if (isInitialized) return;

            recipeLookup = new Dictionary<string, CraftingRecipe>();

            if (Recipes != null)
            {
                foreach (var recipe in Recipes)
                {
                    if (recipe != null && !string.IsNullOrEmpty(recipe.RecipeId))
                    {
                        recipeLookup[recipe.RecipeId] = recipe;
                    }
                }
            }

            isInitialized = true;
            GD.Print($"CraftingDatabase initialized with {recipeLookup.Count} recipes");
        }

        /// <summary>
        /// Get recipe by ID
        /// </summary>
        public CraftingRecipe GetRecipe(string recipeId)
        {
            if (!isInitialized) Initialize();

            if (string.IsNullOrEmpty(recipeId))
            {
                GD.PrintErr("CraftingDatabase: Cannot get recipe with empty ID");
                return null;
            }

            if (recipeLookup.TryGetValue(recipeId, out var recipe))
            {
                return recipe;
            }

            GD.PrintErr($"CraftingDatabase: Recipe '{recipeId}' not found");
            return null;
        }

        /// <summary>
        /// Get all recipes
        /// </summary>
        public List<CraftingRecipe> GetAllRecipes()
        {
            if (!isInitialized) Initialize();
            return recipeLookup.Values.ToList();
        }

        /// <summary>
        /// Get recipes by category
        /// </summary>
        public List<CraftingRecipe> GetRecipesByCategory(string category)
        {
            if (!isInitialized) Initialize();

            return recipeLookup.Values
                .Where(r => r.Category == category)
                .ToList();
        }

        /// <summary>
        /// Get recipes by station type
        /// </summary>
        public List<CraftingRecipe> GetRecipesByStation(CraftingStationType stationType)
        {
            if (!isInitialized) Initialize();

            return recipeLookup.Values
                .Where(r => r.RequiredStation == stationType)
                .ToList();
        }

        /// <summary>
        /// Get recipes by difficulty
        /// </summary>
        public List<CraftingRecipe> GetRecipesByDifficulty(RecipeDifficulty difficulty)
        {
            if (!isInitialized) Initialize();

            return recipeLookup.Values
                .Where(r => r.Difficulty == difficulty)
                .ToList();
        }

        /// <summary>
        /// Get all unique categories
        /// </summary>
        public List<string> GetAllCategories()
        {
            if (!isInitialized) Initialize();

            return recipeLookup.Values
                .Select(r => r.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToList();
        }

        /// <summary>
        /// Search recipes by name
        /// </summary>
        public List<CraftingRecipe> SearchRecipes(string searchTerm)
        {
            if (!isInitialized) Initialize();

            if (string.IsNullOrEmpty(searchTerm))
                return GetAllRecipes();

            searchTerm = searchTerm.ToLower();

            return recipeLookup.Values
                .Where(r => r.DisplayName.ToLower().Contains(searchTerm) || 
                           r.Description.ToLower().Contains(searchTerm))
                .ToList();
        }
    }

    /// <summary>
    /// Helper class to create common recipes programmatically
    /// </summary>
    public static class CraftingRecipePresets
    {
        /// <summary>
        /// Create a basic health potion recipe
        /// </summary>
        public static CraftingRecipe CreateHealthPotionRecipe()
        {
            var recipe = new CraftingRecipe
            {
                RecipeId = "recipe_health_potion",
                DisplayName = "Health Potion",
                Description = "Brew a restorative health potion using herbs and water.",
                ResultItemId = "potion_health",
                ResultQuantity = 1,
                RequiredStation = CraftingStationType.AlchemyTable,
                Difficulty = RecipeDifficulty.Easy,
                IsUnlockedByDefault = true,
                RequiredCraftingLevel = 1,
                BaseSuccessRate = 95,
                Category = "Potions",
                GoldCost = 10
            };

            recipe.AddIngredient("herb_healing", 2);
            recipe.AddIngredient("water_pure", 1);

            return recipe;
        }

        /// <summary>
        /// Create a mana potion recipe
        /// </summary>
        public static CraftingRecipe CreateManaPotionRecipe()
        {
            var recipe = new CraftingRecipe
            {
                RecipeId = "recipe_mana_potion",
                DisplayName = "Mana Potion",
                Description = "Brew a magical elixir to restore MP.",
                ResultItemId = "potion_mana",
                ResultQuantity = 1,
                RequiredStation = CraftingStationType.AlchemyTable,
                Difficulty = RecipeDifficulty.Easy,
                IsUnlockedByDefault = true,
                RequiredCraftingLevel = 1,
                BaseSuccessRate = 95,
                Category = "Potions",
                GoldCost = 15
            };

            recipe.AddIngredient("herb_mana", 2);
            recipe.AddIngredient("crystal_shard", 1);

            return recipe;
        }

        /// <summary>
        /// Create an iron sword recipe
        /// </summary>
        public static CraftingRecipe CreateIronSwordRecipe()
        {
            var recipe = new CraftingRecipe
            {
                RecipeId = "recipe_iron_sword",
                DisplayName = "Iron Sword",
                Description = "Forge a sturdy iron sword.",
                ResultItemId = "sword_iron",
                ResultQuantity = 1,
                RequiredStation = CraftingStationType.Blacksmith,
                Difficulty = RecipeDifficulty.Medium,
                IsUnlockedByDefault = false,
                RequiredCraftingLevel = 3,
                BaseSuccessRate = 85,
                Category = "Weapons",
                GoldCost = 50
            };

            recipe.AddIngredient("ore_iron", 5);
            recipe.AddIngredient("wood_oak", 2);
            recipe.AddIngredient("leather_strip", 1);

            return recipe;
        }

        /// <summary>
        /// Create a steel armor recipe
        /// </summary>
        public static CraftingRecipe CreateSteelArmorRecipe()
        {
            var recipe = new CraftingRecipe
            {
                RecipeId = "recipe_steel_armor",
                DisplayName = "Steel Armor",
                Description = "Craft protective steel armor.",
                ResultItemId = "armor_steel_chest",
                ResultQuantity = 1,
                RequiredStation = CraftingStationType.Blacksmith,
                Difficulty = RecipeDifficulty.Hard,
                IsUnlockedByDefault = false,
                RequiredCraftingLevel = 5,
                BaseSuccessRate = 75,
                Category = "Armor",
                GoldCost = 100
            };

            recipe.AddIngredient("ore_steel", 8);
            recipe.AddIngredient("leather_thick", 3);
            recipe.AddIngredient("cloth_reinforced", 2);

            return recipe;
        }

        /// <summary>
        /// Create a magic ring recipe
        /// </summary>
        public static CraftingRecipe CreateMagicRingRecipe()
        {
            var recipe = new CraftingRecipe
            {
                RecipeId = "recipe_ring_magic",
                DisplayName = "Ring of Magic",
                Description = "Enchant a ring with magical power.",
                ResultItemId = "accessory_magic_ring",
                ResultQuantity = 1,
                RequiredStation = CraftingStationType.Enchanter,
                Difficulty = RecipeDifficulty.Hard,
                IsUnlockedByDefault = false,
                RequiredCraftingLevel = 6,
                BaseSuccessRate = 70,
                Category = "Accessories",
                GoldCost = 200
            };

            recipe.AddIngredient("metal_silver", 2);
            recipe.AddIngredient("gem_sapphire", 1);
            recipe.AddIngredient("essence_arcane", 3);

            return recipe;
        }

        /// <summary>
        /// Create a mage robe recipe
        /// </summary>
        public static CraftingRecipe CreateMageRobeRecipe()
        {
            var recipe = new CraftingRecipe
            {
                RecipeId = "recipe_robe_mage",
                DisplayName = "Mage's Robe",
                Description = "Tailor an enchanted robe for spellcasters.",
                ResultItemId = "armor_mage_robe",
                ResultQuantity = 1,
                RequiredStation = CraftingStationType.Tailor,
                Difficulty = RecipeDifficulty.Medium,
                IsUnlockedByDefault = false,
                RequiredCraftingLevel = 4,
                BaseSuccessRate = 80,
                Category = "Armor",
                GoldCost = 75
            };

            recipe.AddIngredient("cloth_silk", 5);
            recipe.AddIngredient("thread_magic", 3);
            recipe.AddIngredient("dye_azure", 1);

            return recipe;
        }

        /// <summary>
        /// Get all preset recipes
        /// </summary>
        public static List<CraftingRecipe> GetAllPresets()
        {
            return new List<CraftingRecipe>
            {
                CreateHealthPotionRecipe(),
                CreateManaPotionRecipe(),
                CreateIronSwordRecipe(),
                CreateSteelArmorRecipe(),
                CreateMagicRingRecipe(),
                CreateMageRobeRecipe()
            };
        }
    }
}