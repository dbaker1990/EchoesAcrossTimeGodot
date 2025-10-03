using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EchoesAcrossTime.Crafting
{
    /// <summary>
    /// Result of a crafting attempt
    /// </summary>
    public class CraftingResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string ResultItemId { get; set; }
        public int ResultQuantity { get; set; }
        public bool CriticalSuccess { get; set; } // Bonus item/quality
        public int ExpGained { get; set; }
    }

    /// <summary>
    /// Global crafting system manager
    /// </summary>
    public partial class CraftingSystem : Node
    {
        public static CraftingSystem Instance { get; private set; }

        // Crafting progression
        private int craftingLevel = 1;
        private int craftingExp = 0;
        private int expToNextLevel = 100;

        // Current crafting station (can be null for recipes that don't need one)
        private CraftingStationType? currentStation = null;

        // Unlocked recipes
        private HashSet<string> unlockedRecipes = new HashSet<string>();

        // Reference to databases
        private CraftingDatabase craftingDatabase;

        [Signal]
        public delegate void RecipeUnlockedEventHandler(string recipeId);

        [Signal]
        public delegate void CraftingCompletedEventHandler(string recipeId, bool success);

        [Signal]
        public delegate void CraftingLevelUpEventHandler(int newLevel);

        [Signal]
        public delegate void CraftingExpGainedEventHandler(int exp, int currentExp, int expNeeded);

        public override void _Ready()
        {
            if (Instance != null)
            {
                QueueFree();
                return;
            }

            Instance = this;
            GD.Print("CraftingSystem initialized");

            // Load crafting database
            LoadDatabase();
        }

        private void LoadDatabase()
        {
            craftingDatabase = GD.Load<CraftingDatabase>("res://Database/CraftingDatabase.tres");
            
            if (craftingDatabase != null)
            {
                // Unlock default recipes
                foreach (var recipe in craftingDatabase.GetAllRecipes())
                {
                    if (recipe.IsUnlockedByDefault)
                    {
                        UnlockRecipe(recipe.RecipeId, silent: true);
                    }
                }
                
                GD.Print($"CraftingDatabase loaded with {craftingDatabase.GetAllRecipes().Count} recipes");
            }
            else
            {
                GD.PrintErr("Failed to load CraftingDatabase!");
            }
        }

        #region Recipe Management

        /// <summary>
        /// Unlock a recipe
        /// </summary>
        public bool UnlockRecipe(string recipeId, bool silent = false)
        {
            if (unlockedRecipes.Contains(recipeId))
                return false;

            unlockedRecipes.Add(recipeId);
            
            if (!silent)
            {
                EmitSignal(SignalName.RecipeUnlocked, recipeId);
                GD.Print($"Recipe unlocked: {recipeId}");
            }

            return true;
        }

        /// <summary>
        /// Check if recipe is unlocked
        /// </summary>
        public bool IsRecipeUnlocked(string recipeId)
        {
            return unlockedRecipes.Contains(recipeId);
        }

        /// <summary>
        /// Get all unlocked recipes
        /// </summary>
        public List<CraftingRecipe> GetUnlockedRecipes()
        {
            if (craftingDatabase == null) return new List<CraftingRecipe>();

            return craftingDatabase.GetAllRecipes()
                .Where(r => IsRecipeUnlocked(r.RecipeId))
                .ToList();
        }

        /// <summary>
        /// Get unlocked recipes by category
        /// </summary>
        public List<CraftingRecipe> GetUnlockedRecipesByCategory(string category)
        {
            return GetUnlockedRecipes()
                .Where(r => r.Category == category)
                .ToList();
        }

        /// <summary>
        /// Get unlocked recipes for current station
        /// </summary>
        public List<CraftingRecipe> GetRecipesForCurrentStation()
        {
            if (currentStation == null)
                return GetUnlockedRecipes().Where(r => r.RequiredStation == CraftingStationType.None).ToList();

            return GetUnlockedRecipes()
                .Where(r => r.RequiredStation == currentStation || r.RequiredStation == CraftingStationType.None)
                .ToList();
        }

        #endregion

        #region Crafting Station

        /// <summary>
        /// Set current crafting station
        /// </summary>
        public void SetCraftingStation(CraftingStationType? stationType)
        {
            currentStation = stationType;
            GD.Print($"Crafting station set to: {stationType}");
        }

        /// <summary>
        /// Get current station
        /// </summary>
        public CraftingStationType? GetCurrentStation()
        {
            return currentStation;
        }

        /// <summary>
        /// Check if at correct station for recipe
        /// </summary>
        private bool IsAtCorrectStation(CraftingRecipe recipe)
        {
            if (recipe.RequiredStation == CraftingStationType.None)
                return true;

            return currentStation == recipe.RequiredStation;
        }

        #endregion

        #region Crafting Actions

        /// <summary>
        /// Attempt to craft a recipe
        /// </summary>
        public CraftingResult CraftRecipe(string recipeId)
        {
            var result = new CraftingResult { Success = false };

            // Get recipe
            var recipe = craftingDatabase?.GetRecipe(recipeId);
            if (recipe == null)
            {
                result.Message = "Recipe not found!";
                return result;
            }

            // Check if unlocked
            if (!IsRecipeUnlocked(recipeId))
            {
                result.Message = "Recipe not unlocked!";
                return result;
            }

            // Check crafting level requirement
            if (craftingLevel < recipe.RequiredCraftingLevel)
            {
                result.Message = $"Requires Crafting Level {recipe.RequiredCraftingLevel}!";
                return result;
            }

            // Check if at correct station
            if (!IsAtCorrectStation(recipe))
            {
                result.Message = $"Requires {recipe.RequiredStation} station!";
                return result;
            }

            // Check if have ingredients
            var inventory = Items.InventorySystem.Instance;
            if (inventory == null)
            {
                result.Message = "Inventory system not found!";
                return result;
            }

            if (!recipe.CanCraft(inventory))
            {
                var missing = recipe.GetMissingIngredients(inventory);
                if (missing.Count > 0)
                {
                    result.Message = "Missing ingredients!";
                }
                else
                {
                    result.Message = $"Not enough gold! Need {recipe.GoldCost}g";
                }
                return result;
            }

            // Calculate success rate
            int successRate = recipe.CalculateSuccessRate(craftingLevel);
            int roll = GD.RandRange(1, 100);
            bool craftSuccess = roll <= successRate;

            // Critical success (10% chance on success)
            bool criticalSuccess = false;
            if (craftSuccess && GD.RandRange(1, 100) <= 10)
            {
                criticalSuccess = true;
            }

            // Consume ingredients and gold
            foreach (var ingredient in recipe.Ingredients)
            {
                inventory.RemoveItem(ingredient.ItemId, ingredient.Quantity);
            }

            if (recipe.GoldCost > 0)
            {
                inventory.RemoveGold(recipe.GoldCost);
            }

            // If successful, add result item
            if (craftSuccess)
            {
                int quantity = recipe.ResultQuantity;
                if (criticalSuccess)
                {
                    quantity += 1; // Bonus item on critical
                }

                // Get the item data from database
                var resultItem = GameManager.Instance?.Database?.GetItem(recipe.ResultItemId);
                if (resultItem != null)
                {
                    inventory.AddItem(resultItem, quantity);
                }
                else
                {
                    GD.PrintErr($"Result item not found: {recipe.ResultItemId}");
                }

                result.Success = true;
                result.CriticalSuccess = criticalSuccess;
                result.ResultItemId = recipe.ResultItemId;
                result.ResultQuantity = quantity;
                result.Message = criticalSuccess ? "Critical Success! Bonus item created!" : "Crafting successful!";

                // Grant experience
                int expGain = CalculateExpGain(recipe);
                GainCraftingExp(expGain);
                result.ExpGained = expGain;
            }
            else
            {
                result.Message = "Crafting failed! Materials lost...";
                
                // Small exp even on failure
                int failExp = CalculateExpGain(recipe) / 4;
                GainCraftingExp(failExp);
                result.ExpGained = failExp;
            }

            EmitSignal(SignalName.CraftingCompleted, recipeId, craftSuccess);
            return result;
        }

        /// <summary>
        /// Calculate exp gain from recipe
        /// </summary>
        private int CalculateExpGain(CraftingRecipe recipe)
        {
            int baseExp = 10;

            // More exp for harder recipes
            switch (recipe.Difficulty)
            {
                case RecipeDifficulty.Trivial:
                    baseExp = 5;
                    break;
                case RecipeDifficulty.Easy:
                    baseExp = 10;
                    break;
                case RecipeDifficulty.Medium:
                    baseExp = 20;
                    break;
                case RecipeDifficulty.Hard:
                    baseExp = 35;
                    break;
                case RecipeDifficulty.Master:
                    baseExp = 50;
                    break;
            }

            // Reduced exp if recipe is below level
            int levelDiff = craftingLevel - recipe.RequiredCraftingLevel;
            if (levelDiff > 3)
            {
                baseExp = Mathf.Max(1, baseExp / 2);
            }

            return baseExp;
        }

        #endregion

        #region Level System

        /// <summary>
        /// Gain crafting experience
        /// </summary>
        public void GainCraftingExp(int exp)
        {
            craftingExp += exp;
            EmitSignal(SignalName.CraftingExpGained, exp, craftingExp, expToNextLevel);

            // Check for level up
            while (craftingExp >= expToNextLevel)
            {
                LevelUp();
            }
        }

        /// <summary>
        /// Level up crafting skill
        /// </summary>
        private void LevelUp()
        {
            craftingExp -= expToNextLevel;
            craftingLevel++;
            expToNextLevel = CalculateExpForNextLevel();

            EmitSignal(SignalName.CraftingLevelUp, craftingLevel);
            GD.Print($"Crafting Level Up! Now level {craftingLevel}");

            // Unlock level-gated recipes
            UnlockLevelRecipes();
        }

        /// <summary>
        /// Calculate exp needed for next level
        /// </summary>
        private int CalculateExpForNextLevel()
        {
            // Exponential growth: 100, 150, 225, 340, 510...
            return Mathf.RoundToInt(100 * Mathf.Pow(1.5f, craftingLevel - 1));
        }

        /// <summary>
        /// Unlock recipes that become available at current level
        /// </summary>
        private void UnlockLevelRecipes()
        {
            if (craftingDatabase == null) return;

            foreach (var recipe in craftingDatabase.GetAllRecipes())
            {
                if (recipe.RequiredCraftingLevel == craftingLevel && !IsRecipeUnlocked(recipe.RecipeId))
                {
                    UnlockRecipe(recipe.RecipeId);
                }
            }
        }

        /// <summary>
        /// Get current crafting level
        /// </summary>
        public int GetCraftingLevel()
        {
            return craftingLevel;
        }

        /// <summary>
        /// Get current crafting exp
        /// </summary>
        public int GetCraftingExp()
        {
            return craftingExp;
        }

        /// <summary>
        /// Get exp needed for next level
        /// </summary>
        public int GetExpToNextLevel()
        {
            return expToNextLevel;
        }

        #endregion

        #region Save/Load Support

        public Dictionary<string, object> GetSaveData()
        {
            return new Dictionary<string, object>
            {
                { "craftingLevel", craftingLevel },
                { "craftingExp", craftingExp },
                { "expToNextLevel", expToNextLevel },
                { "unlockedRecipes", new List<string>(unlockedRecipes) }
            };
        }

        public void LoadSaveData(Dictionary<string, object> data)
        {
            if (data.ContainsKey("craftingLevel"))
                craftingLevel = Convert.ToInt32(data["craftingLevel"]);

            if (data.ContainsKey("craftingExp"))
                craftingExp = Convert.ToInt32(data["craftingExp"]);

            if (data.ContainsKey("expToNextLevel"))
                expToNextLevel = Convert.ToInt32(data["expToNextLevel"]);

            if (data.ContainsKey("unlockedRecipes"))
            {
                unlockedRecipes.Clear();
                var recipes = data["unlockedRecipes"] as List<string>;
                if (recipes != null)
                {
                    foreach (var recipeId in recipes)
                    {
                        unlockedRecipes.Add(recipeId);
                    }
                }
            }

            GD.Print($"CraftingSystem data loaded - Level {craftingLevel}, {unlockedRecipes.Count} recipes unlocked");
        }

        #endregion
    }
}