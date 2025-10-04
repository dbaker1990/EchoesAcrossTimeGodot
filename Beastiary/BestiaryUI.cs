using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using EchoesAcrossTime.Database;
using EchoesAcrossTime.Managers;

namespace EchoesAcrossTime.Bestiary
{
    /// <summary>
    /// Main UI for the bestiary system
    /// Shows all enemies, their stats, and discovery progress
    /// </summary>
    public partial class BestiaryUI : Control
    {
        #region Node References
        [Export] private Control mainPanel;
        [Export] private ScrollContainer enemyListScroll;
        [Export] private VBoxContainer enemyListContainer;
        [Export] private Control detailPanel;
        [Export] private Button closeButton;
        
        // Header
        [Export] private Label titleLabel;
        [Export] private Label completionLabel;
        [Export] private ProgressBar completionBar;
        
        // Filter/Sort controls
        [Export] private OptionButton filterDropdown;
        [Export] private OptionButton sortDropdown;
        [Export] private CheckButton sortDescending;
        [Export] private LineEdit searchBox;
        
        // Detail view
        [Export] private Label enemyNameLabel;
        [Export] private Label enemyTypeLabel;
        [Export] private TextureRect enemyPortrait;
        [Export] private Label levelLabel;
        [Export] private Label hpLabel;
        [Export] private Label encountersLabel;
        [Export] private Label defeatsLabel;
        [Export] private RichTextLabel loreText;
        
        // Stats display
        [Export] private GridContainer statsGrid;
        [Export] private VBoxContainer weaknessContainer;
        [Export] private VBoxContainer resistanceContainer;
        [Export] private VBoxContainer skillsContainer;
        [Export] private VBoxContainer dropsContainer;
        
        // Entry panel prefab
        [Export] private PackedScene enemyEntryPanelScene;
        #endregion
        
        private BestiaryFilterType currentFilter = BestiaryFilterType.All;
        private BestiarySortType currentSort = BestiarySortType.ByName;
        private bool sortDescendingValue = false;
        private string currentSearchTerm = "";
        
        private List<Control> entryPanels = new();
        private CharacterData selectedEnemy = null;
        
        public override void _Ready()
        {
            // Connect signals
            if (closeButton != null)
                closeButton.Pressed += OnClosePressed;
            
            if (filterDropdown != null)
                filterDropdown.ItemSelected += OnFilterChanged;
            
            if (sortDropdown != null)
                sortDropdown.ItemSelected += OnSortChanged;
            
            if (sortDescending != null)
                sortDescending.Toggled += OnSortDirectionToggled;
            
            if (searchBox != null)
                searchBox.TextChanged += OnSearchTextChanged;
            
            if (BestiaryManager.Instance != null)
            {
                BestiaryManager.Instance.BestiaryUpdated += RefreshBestiary;
                BestiaryManager.Instance.EnemyDiscovered += OnEnemyDiscovered;
            }
            
            // Setup dropdowns
            SetupFilterDropdown();
            SetupSortDropdown();
            
            // Initial display
            Hide();
        }
        
        #region Setup
        private void SetupFilterDropdown()
        {
            if (filterDropdown == null) return;
            
            filterDropdown.Clear();
            filterDropdown.AddItem("All Enemies", (int)BestiaryFilterType.All);
            filterDropdown.AddItem("Discovered", (int)BestiaryFilterType.Discovered);
            filterDropdown.AddItem("Undiscovered", (int)BestiaryFilterType.Undiscovered);
            filterDropdown.AddItem("Bosses", (int)BestiaryFilterType.Bosses);
            filterDropdown.AddItem("Regular Enemies", (int)BestiaryFilterType.RegularEnemies);
        }
        
        private void SetupSortDropdown()
        {
            if (sortDropdown == null) return;
            
            sortDropdown.Clear();
            sortDropdown.AddItem("Name", (int)BestiarySortType.ByName);
            sortDropdown.AddItem("Level", (int)BestiarySortType.ByLevel);
            sortDropdown.AddItem("Type", (int)BestiarySortType.ByType);
            sortDropdown.AddItem("Times Encountered", (int)BestiarySortType.ByTimesEncountered);
            sortDropdown.AddItem("First Encountered", (int)BestiarySortType.ByFirstEncounter);
            sortDropdown.AddItem("Last Encountered", (int)BestiarySortType.ByLastEncounter);
        }
        #endregion
        
        #region Display Control
        /// <summary>
        /// Show the bestiary
        /// </summary>
        public void ShowBestiary()
        {
            Show();
            RefreshBestiary();
            
            if (mainPanel != null)
                mainPanel.Show();
        }
        
        /// <summary>
        /// Refresh the entire bestiary display
        /// </summary>
        public void RefreshBestiary()
        {
            if (!Visible) return;
            
            UpdateCompletionDisplay();
            UpdateEnemyList();
        }
        
        private void UpdateCompletionDisplay()
        {
            if (BestiaryManager.Instance == null) return;
            
            var completion = BestiaryManager.Instance.CompletionPercentage;
            var discovered = BestiaryManager.Instance.TotalDiscovered;
            var total = BestiaryManager.Instance.TotalEnemiesInGame;
            
            if (completionLabel != null)
                completionLabel.Text = $"Discovered: {discovered}/{total} ({completion:F1}%)";
            
            if (completionBar != null)
            {
                completionBar.MaxValue = 100;
                completionBar.Value = completion;
            }
        }
        
        private void UpdateEnemyList()
        {
            // Clear existing entries
            foreach (var panel in entryPanels)
            {
                panel.QueueFree();
            }
            entryPanels.Clear();
            
            if (BestiaryManager.Instance == null || enemyListContainer == null)
                return;
            
            // Get filtered and sorted enemies
            var enemies = BestiaryManager.Instance.GetFilteredEnemies(currentFilter);
            enemies = BestiaryManager.Instance.GetSortedEnemies(currentSort, sortDescendingValue)
                .Where(e => enemies.Contains(e))
                .ToList();
            
            // Apply search filter
            if (!string.IsNullOrWhiteSpace(currentSearchTerm))
            {
                enemies = enemies.Where(e => 
                    e.DisplayName.Contains(currentSearchTerm, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            
            // Create entry panels
            foreach (var enemy in enemies)
            {
                var entry = BestiaryManager.Instance.GetEntry(enemy.CharacterId);
                var panel = CreateEnemyEntryPanel(enemy, entry);
                
                if (panel != null)
                {
                    enemyListContainer.AddChild(panel);
                    entryPanels.Add(panel);
                }
            }
        }
        
        private Control CreateEnemyEntryPanel(CharacterData enemy, BestiaryEntry entry)
        {
            Control panel;
            
            if (enemyEntryPanelScene != null)
            {
                panel = enemyEntryPanelScene.Instantiate<Control>();
            }
            else
            {
                // Create simple panel if no prefab
                panel = CreateSimpleEntryPanel(enemy, entry);
            }
            
            // Make it clickable
            if (panel is Button button)
            {
                button.Pressed += () => OnEnemySelected(enemy);
            }
            else if (panel is Panel panelContainer)
            {
                var clickable = new Button();
                clickable.Flat = true;
                clickable.SizeFlagsHorizontal = SizeFlags.ExpandFill;
                clickable.SizeFlagsVertical = SizeFlags.ExpandFill;
                clickable.Pressed += () => OnEnemySelected(enemy);
                panelContainer.AddChild(clickable);
            }
            
            return panel;
        }
        
        private Control CreateSimpleEntryPanel(CharacterData enemy, BestiaryEntry entry)
        {
            var button = new Button();
            button.CustomMinimumSize = new Vector2(0, 60);
            
            var hbox = new HBoxContainer();
            button.AddChild(hbox);
            
            // Icon/Portrait
            var icon = new TextureRect();
            icon.CustomMinimumSize = new Vector2(50, 50);
            icon.ExpandMode = TextureRect.ExpandModeEnum.FitWidthProportional;
            icon.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
            
            if (entry?.IsDiscovered ?? false)
            {
                if (enemy.BattlePortrait != null)
                    icon.Texture = enemy.BattlePortrait;
            }
            else
            {
                // Show ??? for undiscovered
                icon.Modulate = new Color(0, 0, 0, 0.5f);
            }
            
            hbox.AddChild(icon);
            
            // Info
            var vbox = new VBoxContainer();
            vbox.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            hbox.AddChild(vbox);
            
            var nameLabel = new Label();
            nameLabel.Text = (entry?.IsDiscovered ?? false) ? enemy.DisplayName : "???";
            nameLabel.AddThemeColorOverride("font_color", (entry?.IsDiscovered ?? false) 
                ? Colors.White : Colors.Gray);
            vbox.AddChild(nameLabel);
            
            if (entry?.IsDiscovered ?? false)
            {
                var infoLabel = new Label();
                infoLabel.Text = $"Lv.{entry.HighestLevelSeen} | Encountered: {entry.TimesEncountered}x";
                infoLabel.AddThemeColorOverride("font_color", Colors.LightGray);
                infoLabel.AddThemeFontSizeOverride("font_size", 12);
                vbox.AddChild(infoLabel);
            }
            
            return button;
        }
        #endregion
        
        #region Detail View
        private void OnEnemySelected(CharacterData enemy)
        {
            selectedEnemy = enemy;
            UpdateDetailView();
            
            if (detailPanel != null)
                detailPanel.Show();
        }
        
        private void UpdateDetailView()
        {
            if (selectedEnemy == null) return;
            
            var entry = BestiaryManager.Instance?.GetEntry(selectedEnemy.CharacterId);
            bool isDiscovered = entry?.IsDiscovered ?? false;
            
            // Basic info
            if (enemyNameLabel != null)
                enemyNameLabel.Text = isDiscovered ? selectedEnemy.DisplayName : "???";
            
            if (enemyTypeLabel != null)
                enemyTypeLabel.Text = isDiscovered ? selectedEnemy.Type.ToString() : "Unknown";
            
            if (enemyPortrait != null)
            {
                if (isDiscovered && selectedEnemy.BattlePortrait != null)
                {
                    enemyPortrait.Texture = selectedEnemy.BattlePortrait;
                    enemyPortrait.Modulate = Colors.White;
                }
                else
                {
                    enemyPortrait.Modulate = new Color(0, 0, 0, 0.5f);
                }
            }
            
            // Stats
            if (levelLabel != null)
                levelLabel.Text = isDiscovered ? $"Level: {entry.HighestLevelSeen}" : "Level: ???";
            
            if (hpLabel != null)
                hpLabel.Text = isDiscovered ? $"HP: {selectedEnemy.MaxHP}" : "HP: ???";
            
            if (encountersLabel != null)
                encountersLabel.Text = isDiscovered ? $"Encountered: {entry.TimesEncountered}" : "Encountered: 0";
            
            if (defeatsLabel != null)
                defeatsLabel.Text = isDiscovered ? $"Defeated: {entry.TimesDefeated}" : "Defeated: 0";
            
            // Lore
            if (loreText != null)
            {
                if (isDiscovered)
                {
                    loreText.Text = !string.IsNullOrEmpty(selectedEnemy.Description) 
                        ? selectedEnemy.Description 
                        : "No additional information available.";
                }
                else
                {
                    loreText.Text = "Defeat this enemy to learn more about it.";
                }
            }
            
            // Detailed stats
            UpdateStatsDisplay(isDiscovered);
            UpdateWeaknessDisplay(isDiscovered, entry);
            UpdateSkillsDisplay(isDiscovered, entry);
            UpdateDropsDisplay(isDiscovered);
        }
        
        private void UpdateStatsDisplay(bool isDiscovered)
        {
            if (statsGrid == null) return;
            
            // Clear existing
            foreach (var child in statsGrid.GetChildren())
            {
                child.QueueFree();
            }
            
            if (!isDiscovered)
            {
                var label = new Label();
                label.Text = "Stats hidden until discovered";
                statsGrid.AddChild(label);
                return;
            }
            
            // Show stats
            AddStatRow("ATK", selectedEnemy.Attack.ToString());
            AddStatRow("DEF", selectedEnemy.Defense.ToString());
            AddStatRow("M.ATK", selectedEnemy.MagicAttack.ToString());
            AddStatRow("M.DEF", selectedEnemy.MagicDefense.ToString());
            AddStatRow("SPD", selectedEnemy.Speed.ToString());
        }
        
        private void AddStatRow(string statName, string value)
        {
            if (statsGrid == null) return;
            
            var nameLabel = new Label();
            nameLabel.Text = statName;
            statsGrid.AddChild(nameLabel);
            
            var valueLabel = new Label();
            valueLabel.Text = value;
            statsGrid.AddChild(valueLabel);
        }
        
        private void UpdateWeaknessDisplay(bool isDiscovered, BestiaryEntry entry)
        {
            if (weaknessContainer == null) return;
            
            // Clear
            foreach (var child in weaknessContainer.GetChildren())
            {
                child.QueueFree();
            }
            
            if (!isDiscovered || entry == null)
            {
                var label = new Label();
                label.Text = "Weaknesses: ???";
                weaknessContainer.AddChild(label);
                return;
            }
            
            var headerLabel = new Label();
            headerLabel.Text = "Weaknesses:";
            headerLabel.AddThemeColorOverride("font_color", Colors.Orange);
            weaknessContainer.AddChild(headerLabel);
            
            if (entry.DiscoveredWeaknesses.Count == 0)
            {
                var noneLabel = new Label();
                noneLabel.Text = "None discovered";
                weaknessContainer.AddChild(noneLabel);
            }
            else
            {
                foreach (var weakness in entry.DiscoveredWeaknesses)
                {
                    var label = new Label();
                    label.Text = $"• {weakness}";
                    weaknessContainer.AddChild(label);
                }
            }
        }
        
        private void UpdateSkillsDisplay(bool isDiscovered, BestiaryEntry entry)
        {
            if (skillsContainer == null) return;
            
            // Clear
            foreach (var child in skillsContainer.GetChildren())
            {
                child.QueueFree();
            }
            
            if (!isDiscovered || entry == null)
            {
                var label = new Label();
                label.Text = "Skills: ???";
                skillsContainer.AddChild(label);
                return;
            }
            
            var headerLabel = new Label();
            headerLabel.Text = "Known Skills:";
            headerLabel.AddThemeColorOverride("font_color", Colors.Cyan);
            skillsContainer.AddChild(headerLabel);
            
            if (entry.DiscoveredSkillIds.Count == 0)
            {
                var noneLabel = new Label();
                noneLabel.Text = "None discovered yet";
                skillsContainer.AddChild(noneLabel);
            }
            else
            {
                foreach (var skillId in entry.DiscoveredSkillIds)
                {
                    var skill = GameManager.Instance?.Database?.GetSkill(skillId);
                    var label = new Label();
                    label.Text = skill != null ? $"• {skill.DisplayName}" : $"• {skillId}";
                    skillsContainer.AddChild(label);
                }
            }
        }
        
        private void UpdateDropsDisplay(bool isDiscovered)
        {
            if (dropsContainer == null) return;
            
            // Clear
            foreach (var child in dropsContainer.GetChildren())
            {
                child.QueueFree();
            }
            
            if (!isDiscovered)
            {
                var label = new Label();
                label.Text = "Drops: ???";
                dropsContainer.AddChild(label);
                return;
            }
            
            var headerLabel = new Label();
            headerLabel.Text = "Drops:";
            headerLabel.AddThemeColorOverride("font_color", Colors.Gold);
            dropsContainer.AddChild(headerLabel);
            
            if (selectedEnemy.Rewards?.CommonDrops?.Count > 0)
            {
                foreach (var drop in selectedEnemy.Rewards.CommonDrops)
                {
                    var item = GameManager.Instance?.Database?.GetItem(drop.ItemId);
                    var label = new Label();
                    label.Text = item != null 
                        ? $"• {item.DisplayName} ({drop.DropChance}%)" 
                        : $"• {drop.ItemId} ({drop.DropChance}%)";
                    dropsContainer.AddChild(label);
                }
            }
            else
            {
                var noneLabel = new Label();
                noneLabel.Text = "No common drops";
                dropsContainer.AddChild(noneLabel);
            }
        }
        #endregion
        
        #region Event Handlers
        private void OnFilterChanged(long index)
        {
            currentFilter = (BestiaryFilterType)index;
            RefreshBestiary();
            SystemManager.Instance?.PlayCursorSE();
        }
        
        private void OnSortChanged(long index)
        {
            currentSort = (BestiarySortType)index;
            RefreshBestiary();
            SystemManager.Instance?.PlayCursorSE();
        }
        
        private void OnSortDirectionToggled(bool toggled)
        {
            sortDescendingValue = toggled;
            RefreshBestiary();
            SystemManager.Instance?.PlayCursorSE();
        }
        
        private void OnSearchTextChanged(string newText)
        {
            currentSearchTerm = newText;
            RefreshBestiary();
        }
        
        private void OnEnemyDiscovered(string enemyId, CharacterData enemyData)
        {
            // Could show a notification here
            GD.Print($"New enemy discovered: {enemyData.DisplayName}");
        }
        
        private void OnClosePressed()
        {
            Hide();
            SystemManager.Instance?.PlayCancelSE();
        }
        #endregion
        
        public override void _Input(InputEvent @event)
        {
            if (!Visible) return;
            
            if (@event.IsActionPressed("ui_cancel"))
            {
                OnClosePressed();
                GetViewport().SetInputAsHandled();
            }
        }
    }
}