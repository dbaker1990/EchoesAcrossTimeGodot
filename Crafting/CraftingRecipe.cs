using Godot;
using System.Collections.Generic;
using System.Linq;

namespace EchoesAcrossTime.Crafting
{
    /// <summary>
    /// Represents an ingredient required for crafting
    /// </summary>
    public class CraftingIngredient
    {
        public string ItemId { get; set; }
        public int Quantity { get; set; }

        public CraftingIngredient(string itemId, int quantity)
        {
            ItemId = itemId;
            Quantity = quantity;
        }
    }

    /// <summary>
    /// Types of crafting stations
    /// </summary>
    public enum CraftingStationType
    {
        None,           // Can craft anywhere
        Blacksmith,     // For weapons and armor
        AlchemyTable,   // For potions and consumables
        Enchanter,      // For magical items and accessories
        Tailor,         // For cloth armor and robes
        Workbench       // For general items
    }

    /// <summary>
    /// Difficulty level of recipe
    /// </summary>
    public enum RecipeDifficulty
    {
        Trivial,
        Easy,
        Medium,
        Hard,
        Master
    }

    /// <summary>
    /// Defines a crafting recipe
    /// </summary>
    [GlobalClass]
    public partial class CraftingRecipe : Resource
    {
        [ExportCategory("Recipe Identity")]
        [Export] public string RecipeId { get; set; }
        [Export] public string DisplayName { get; set; }
        [Export(PropertyHint.MultilineText)] public string Description { get; set; }

        [ExportCategory("Crafting Requirements")]
        [Export] public string ResultItemId { get; set; }
        [Export] public int ResultQuantity { get; set; } = 1;
        [Export] public CraftingStationType RequiredStation { get; set; } = CraftingStationType.None;
        [Export] public RecipeDifficulty Difficulty { get; set; } = RecipeDifficulty.Easy;
        
        [ExportCategory("Recipe Unlock")]
        [Export] public bool IsUnlockedByDefault { get; set; } = false;
        [Export] public int RequiredCraftingLevel { get; set; } = 1;
        [Export] public string UnlockQuestId { get; set; } = ""; // Optional quest requirement
        
        [ExportCategory("Success")]
        [Export] public int BaseSuccessRate { get; set; } = 100; // 100 = guaranteed success
        [Export] public int CraftingTimeSeconds { get; set; } = 0; // 0 = instant
        [Export] public int GoldCost { get; set; } = 0; // Additional gold cost to craft

        // Ingredients (set via code or custom inspector)
        public List<CraftingIngredient> Ingredients { get; set; } = new List<CraftingIngredient>();

        // Category for organization
        [Export] public string Category { get; set; } = "General";

        /// <summary>
        /// Add an ingredient to this recipe
        /// </summary>
        public void AddIngredient(string itemId, int quantity)
        {
            Ingredients.Add(new CraftingIngredient(itemId, quantity));
        }

        /// <summary>
        /// Check if all ingredients are available in inventory
        /// </summary>
        public bool CanCraft(Items.InventorySystem inventory)
        {
            if (inventory == null) return false;

            // Check gold cost
            if (GoldCost > 0 && inventory.GetGold() < GoldCost)
                return false;

            // Check each ingredient
            foreach (var ingredient in Ingredients)
            {
                var availableQuantity = inventory.GetItemCount(ingredient.ItemId);
                if (availableQuantity < ingredient.Quantity)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Get missing ingredients for crafting
        /// </summary>
        public Dictionary<string, int> GetMissingIngredients(Items.InventorySystem inventory)
        {
            var missing = new Dictionary<string, int>();

            foreach (var ingredient in Ingredients)
            {
                var available = inventory.GetItemCount(ingredient.ItemId);
                var needed = ingredient.Quantity;
                
                if (available < needed)
                {
                    missing[ingredient.ItemId] = needed - available;
                }
            }

            return missing;
        }

        /// <summary>
        /// Calculate success rate based on crafting level
        /// </summary>
        public int CalculateSuccessRate(int craftingLevel)
        {
            int successRate = BaseSuccessRate;

            // Bonus for being over-leveled
            int levelDifference = craftingLevel - RequiredCraftingLevel;
            if (levelDifference > 0)
            {
                successRate += levelDifference * 2; // +2% per level above requirement
            }

            // Difficulty penalty
            switch (Difficulty)
            {
                case RecipeDifficulty.Hard:
                    successRate -= 10;
                    break;
                case RecipeDifficulty.Master:
                    successRate -= 20;
                    break;
            }

            return Mathf.Clamp(successRate, 5, 100); // Min 5%, max 100%
        }
    }
}