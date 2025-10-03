using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EchoesAcrossTime.UI
{
    /// <summary>
    /// UI for the crafting system
    /// Attach this to a Control node in your scene
    /// </summary>
    public partial class CraftingUI : Control
    {
        #region Node References
        // Main panels
        [Export] private Control recipeListPanel;
        [Export] private Control recipeDetailPanel;
        [Export] private Control craftingProgressPanel;

        // Recipe List
        [Export] private ItemList recipeList;
        [Export] private OptionButton categoryFilter;
        [Export] private LineEdit searchBox;

        // Recipe Details
        [Export] private Label recipeNameLabel;
        [Export] private Label recipeDescriptionLabel;
        [Export] private Label recipeDifficultyLabel;
        [Export] private Label recipeStationLabel;
        [Export] private Label recipeLevelRequirement;
        [Export] private Container ingredientsContainer;
        [Export] private Label goldCostLabel;
        [Export] private Label successRateLabel;
        [Export] private Button craftButton;

        // Crafting Progress (for timed recipes)
        [Export] private ProgressBar craftingProgressBar;
        [Export] private Label craftingStatusLabel;

        // Player Stats
        [Export] private Label craftingLevelLabel;
        [Export] private ProgressBar craftingExpBar;
        [Export] private Label craftingExpLabel;

        // Result popup
        [Export] private Control resultPopup;
        [Export] private Label resultMessageLabel;
        [Export] private TextureRect resultItemIcon;
        [Export] private Button resultCloseButton;
        #endregion

        private Crafting.CraftingRecipe selectedRecipe;
        private List<Crafting.CraftingRecipe> currentRecipes;

        public override void _Ready()
        {
            // Hide panels initially
            if (craftingProgressPanel != null)
                craftingProgressPanel.Visible = false;

            if (resultPopup != null)
                resultPopup.Visible = false;

            // Connect signals
            ConnectSignals();

            // Initialize UI
            RefreshUI();

            GD.Print("CraftingUI initialized");
        }

        private void ConnectSignals()
        {
            // Recipe list selection
            if (recipeList != null)
                recipeList.ItemSelected += OnRecipeSelected;

            // Craft button
            if (craftButton != null)
                craftButton.Pressed += OnCraftButtonPressed;

            // Category filter
            if (categoryFilter != null)
                categoryFilter.ItemSelected += OnCategoryFilterChanged;

            // Search box
            if (searchBox != null)
                searchBox.TextChanged += OnSearchTextChanged;

            // Result popup close
            if (resultCloseButton != null)
                resultCloseButton.Pressed += OnResultClosePressed;

            // Crafting system signals
            var craftingSystem = Crafting.CraftingSystem.Instance;
            if (craftingSystem != null)
            {
                craftingSystem.CraftingCompleted += OnCraftingCompleted;
                craftingSystem.CraftingLevelUp += OnCraftingLevelUp;
                craftingSystem.CraftingExpGained += OnCraftingExpGained;
                craftingSystem.RecipeUnlocked += OnRecipeUnlocked;
            }
        }

        #region UI Refresh

        /// <summary>
        /// Refresh the entire UI
        /// </summary>
        public void RefreshUI()
        {
            RefreshPlayerStats();
            RefreshCategoryFilter();
            RefreshRecipeList();
        }

        /// <summary>
        /// Refresh player crafting stats
        /// </summary>
        private void RefreshPlayerStats()
        {
            var craftingSystem = Crafting.CraftingSystem.Instance;
            if (craftingSystem == null) return;

            int level = craftingSystem.GetCraftingLevel();
            int exp = craftingSystem.GetCraftingExp();
            int expNeeded = craftingSystem.GetExpToNextLevel();

            if (craftingLevelLabel != null)
                craftingLevelLabel.Text = $"Crafting Level: {level}";

            if (craftingExpBar != null)
            {
                craftingExpBar.MaxValue = expNeeded;
                craftingExpBar.Value = exp;
            }

            if (craftingExpLabel != null)
                craftingExpLabel.Text = $"{exp} / {expNeeded} EXP";
        }

        /// <summary>
        /// Populate category filter dropdown
        /// </summary>
        private void RefreshCategoryFilter()
        {
            if (categoryFilter == null) return;

            categoryFilter.Clear();
            categoryFilter.AddItem("All Categories", 0);

            var craftingSystem = Crafting.CraftingSystem.Instance;
            if (craftingSystem == null) return;

            var categories = craftingSystem.GetUnlockedRecipes()
                .Select(r => r.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            int index = 1;
            foreach (var category in categories)
            {
                categoryFilter.AddItem(category, index);
                index++;
            }
        }

        /// <summary>
        /// Refresh the recipe list
        /// </summary>
        private void RefreshRecipeList()
        {
            if (recipeList == null) return;

            recipeList.Clear();

            var craftingSystem = Crafting.CraftingSystem.Instance;
            if (craftingSystem == null) return;

            // Get recipes for current station
            currentRecipes = craftingSystem.GetRecipesForCurrentStation();

            // Apply category filter
            if (categoryFilter != null && categoryFilter.Selected > 0)
            {
                string selectedCategory = categoryFilter.GetItemText(categoryFilter.Selected);
                currentRecipes = currentRecipes.Where(r => r.Category == selectedCategory).ToList();
            }

            // Apply search filter
            if (searchBox != null && !string.IsNullOrEmpty(searchBox.Text))
            {
                string search = searchBox.Text.ToLower();
                currentRecipes = currentRecipes.Where(r => 
                    r.DisplayName.ToLower().Contains(search) ||
                    r.Description.ToLower().Contains(search)
                ).ToList();
            }

            // Populate list
            foreach (var recipe in currentRecipes)
            {
                string displayText = recipe.DisplayName;

                // Add level requirement if not met
                int playerLevel = craftingSystem.GetCraftingLevel();
                if (playerLevel < recipe.RequiredCraftingLevel)
                {
                    displayText += $" [Lv.{recipe.RequiredCraftingLevel}]";
                }

                // Add difficulty indicator
                displayText += $" ({GetDifficultyString(recipe.Difficulty)})";

                recipeList.AddItem(displayText);
            }

            // Auto-select first if available
            if (currentRecipes.Count > 0)
            {
                recipeList.Select(0);
                OnRecipeSelected(0);
            }
            else
            {
                ClearRecipeDetails();
            }
        }

        /// <summary>
        /// Display selected recipe details
        /// </summary>
        private void DisplayRecipeDetails(Crafting.CraftingRecipe recipe)
        {
            if (recipe == null) return;

            selectedRecipe = recipe;

            // Basic info
            if (recipeNameLabel != null)
                recipeNameLabel.Text = recipe.DisplayName;

            if (recipeDescriptionLabel != null)
                recipeDescriptionLabel.Text = recipe.Description;

            if (recipeDifficultyLabel != null)
            {
                recipeDifficultyLabel.Text = $"Difficulty: {GetDifficultyString(recipe.Difficulty)}";
                recipeDifficultyLabel.AddThemeColorOverride("font_color", GetDifficultyColor(recipe.Difficulty));
            }

            if (recipeStationLabel != null)
            {
                string stationText = recipe.RequiredStation == Crafting.CraftingStationType.None 
                    ? "No station required" 
                    : $"Requires: {recipe.RequiredStation}";
                recipeStationLabel.Text = stationText;
            }

            if (recipeLevelRequirement != null)
                recipeLevelRequirement.Text = $"Required Level: {recipe.RequiredCraftingLevel}";

            // Gold cost
            if (goldCostLabel != null)
            {
                if (recipe.GoldCost > 0)
                {
                    goldCostLabel.Text = $"Cost: {recipe.GoldCost}g";
                    goldCostLabel.Visible = true;
                }
                else
                {
                    goldCostLabel.Visible = false;
                }
            }

            // Success rate
            if (successRateLabel != null)
            {
                var craftingSystem = Crafting.CraftingSystem.Instance;
                int successRate = recipe.CalculateSuccessRate(craftingSystem.GetCraftingLevel());
                successRateLabel.Text = $"Success Rate: {successRate}%";
                successRateLabel.AddThemeColorOverride("font_color", GetSuccessRateColor(successRate));
            }

            // Ingredients
            DisplayIngredients(recipe);

            // Craft button state
            UpdateCraftButton(recipe);
        }

        /// <summary>
        /// Display recipe ingredients
        /// </summary>
        private void DisplayIngredients(Crafting.CraftingRecipe recipe)
        {
            if (ingredientsContainer == null) return;

            // Clear existing children
            foreach (Node child in ingredientsContainer.GetChildren())
            {
                child.QueueFree();
            }

            var inventory = Items.InventorySystem.Instance;
            if (inventory == null) return;

            foreach (var ingredient in recipe.Ingredients)
            {
                var ingredientLabel = new Label();
                
                var item = inventory.GetItem(ingredient.ItemId);
                string itemName = item != null ? item.DisplayName : ingredient.ItemId;
                
                int have = inventory.GetItemCount(ingredient.ItemId);
                int need = ingredient.Quantity;

                ingredientLabel.Text = $"{itemName}: {have}/{need}";

                // Color code based on availability
                if (have >= need)
                {
                    ingredientLabel.AddThemeColorOverride("font_color", new Color(0.4f, 1.0f, 0.4f));
                }
                else
                {
                    ingredientLabel.AddThemeColorOverride("font_color", new Color(1.0f, 0.4f, 0.4f));
                }

                ingredientsContainer.AddChild(ingredientLabel);
            }
        }

        /// <summary>
        /// Update craft button enabled state
        /// </summary>
        private void UpdateCraftButton(Crafting.CraftingRecipe recipe)
        {
            if (craftButton == null) return;

            var craftingSystem = Crafting.CraftingSystem.Instance;
            var inventory = Items.InventorySystem.Instance;

            if (craftingSystem == null || inventory == null)
            {
                craftButton.Disabled = true;
                return;
            }

            // Check all requirements
            bool canCraft = true;
            string tooltip = "";

            // Check level
            if (craftingSystem.GetCraftingLevel() < recipe.RequiredCraftingLevel)
            {
                canCraft = false;
                tooltip = $"Requires Crafting Level {recipe.RequiredCraftingLevel}";
            }
            // Check ingredients and gold
            else if (!recipe.CanCraft(inventory))
            {
                canCraft = false;
                tooltip = "Missing required materials or gold";
            }

            craftButton.Disabled = !canCraft;
            craftButton.TooltipText = tooltip;
        }

        /// <summary>
        /// Clear recipe details panel
        /// </summary>
        private void ClearRecipeDetails()
        {
            selectedRecipe = null;

            if (recipeNameLabel != null)
                recipeNameLabel.Text = "No recipe selected";

            if (recipeDescriptionLabel != null)
                recipeDescriptionLabel.Text = "";

            if (craftButton != null)
                craftButton.Disabled = true;
        }

        #endregion

        #region Event Handlers

        private void OnRecipeSelected(long index)
        {
            if (index < 0 || index >= currentRecipes.Count) return;

            var recipe = currentRecipes[(int)index];
            DisplayRecipeDetails(recipe);
        }

        private void OnCraftButtonPressed()
        {
            if (selectedRecipe == null) return;

            var craftingSystem = Crafting.CraftingSystem.Instance;
            if (craftingSystem == null) return;

            // Attempt crafting
            var result = craftingSystem.CraftRecipe(selectedRecipe.RecipeId);

            // Show result
            ShowCraftingResult(result);

            // Refresh UI
            RefreshUI();
            if (selectedRecipe != null)
            {
                DisplayRecipeDetails(selectedRecipe);
            }
        }

        private void OnCategoryFilterChanged(long index)
        {
            RefreshRecipeList();
        }

        private void OnSearchTextChanged(string newText)
        {
            RefreshRecipeList();
        }

        private void OnCraftingCompleted(string recipeId, bool success)
        {
            // Could add animation or sound here
            GD.Print($"Crafting {(success ? "succeeded" : "failed")}: {recipeId}");
        }

        private void OnCraftingLevelUp(int newLevel)
        {
            GD.Print($"Crafting Level Up! Now level {newLevel}");
            RefreshPlayerStats();
            RefreshRecipeList(); // May unlock new recipes
        }

        private void OnCraftingExpGained(int exp, int currentExp, int expNeeded)
        {
            RefreshPlayerStats();
        }

        private void OnRecipeUnlocked(string recipeId)
        {
            GD.Print($"New recipe unlocked: {recipeId}");
            RefreshRecipeList();
        }

        #endregion

        #region Crafting Result Popup

        /// <summary>
        /// Show crafting result popup
        /// </summary>
        private void ShowCraftingResult(Crafting.CraftingResult result)
        {
            if (resultPopup == null) return;

            if (resultMessageLabel != null)
            {
                string message = result.Message;
                if (result.Success)
                {
                    message += $"\n\nCrafted: {result.ResultQuantity}x";
                    if (result.ExpGained > 0)
                    {
                        message += $"\nExp Gained: +{result.ExpGained}";
                    }
                }
                resultMessageLabel.Text = message;
            }

            // Could set result item icon here if you have icon references

            resultPopup.Visible = true;
        }

        private void OnResultClosePressed()
        {
            if (resultPopup != null)
                resultPopup.Visible = false;
        }

        #endregion

        #region Helper Methods

        private string GetDifficultyString(Crafting.RecipeDifficulty difficulty)
        {
            return difficulty switch
            {
                Crafting.RecipeDifficulty.Trivial => "★",
                Crafting.RecipeDifficulty.Easy => "★★",
                Crafting.RecipeDifficulty.Medium => "★★★",
                Crafting.RecipeDifficulty.Hard => "★★★★",
                Crafting.RecipeDifficulty.Master => "★★★★★",
                _ => "?"
            };
        }

        private Color GetDifficultyColor(Crafting.RecipeDifficulty difficulty)
        {
            return difficulty switch
            {
                Crafting.RecipeDifficulty.Trivial => new Color(0.7f, 0.7f, 0.7f),
                Crafting.RecipeDifficulty.Easy => new Color(0.4f, 1.0f, 0.4f),
                Crafting.RecipeDifficulty.Medium => new Color(1.0f, 1.0f, 0.4f),
                Crafting.RecipeDifficulty.Hard => new Color(1.0f, 0.6f, 0.2f),
                Crafting.RecipeDifficulty.Master => new Color(1.0f, 0.3f, 1.0f),
                _ => new Color(1, 1, 1)
            };
        }

        private Color GetSuccessRateColor(int successRate)
        {
            if (successRate >= 95)
                return new Color(0.4f, 1.0f, 0.4f);
            else if (successRate >= 75)
                return new Color(1.0f, 1.0f, 0.4f);
            else if (successRate >= 50)
                return new Color(1.0f, 0.6f, 0.2f);
            else
                return new Color(1.0f, 0.4f, 0.4f);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Open the crafting UI at a specific station
        /// </summary>
        public void OpenAtStation(Crafting.CraftingStationType? stationType)
        {
            var craftingSystem = Crafting.CraftingSystem.Instance;
            if (craftingSystem != null)
            {
                craftingSystem.SetCraftingStation(stationType);
            }

            Show();
            RefreshUI();
        }

        /// <summary>
        /// Close the crafting UI
        /// </summary>
        public void CloseCraftingUI()
        {
            Hide();
        }

        #endregion
    }
}