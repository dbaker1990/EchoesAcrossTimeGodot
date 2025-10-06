// LoreCodex/LoreCodexUI.cs
using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using EchoesAcrossTime.LoreCodex;
using EchoesAcrossTime.Managers;

namespace EchoesAcrossTime.UI
{
    /// <summary>
    /// UI for browsing the lore codex
    /// </summary>
    public partial class LoreCodexUI : Control
    {
        #region Export Nodes
        // Main container
        [Export] public Control MainPanel { get; set; }
        
        // Top bar
        [Export] public Label TitleLabel { get; set; }
        [Export] public Label CompletionLabel { get; set; }
        [Export] public ProgressBar CompletionBar { get; set; }
        [Export] public Button CloseButton { get; set; }
        
        // Filter/Sort controls
        [Export] public LineEdit SearchBar { get; set; }
        [Export] public OptionButton CategoryFilter { get; set; }
        [Export] public OptionButton EraFilter { get; set; }
        [Export] public OptionButton SortOption { get; set; }
        
        // Entry list
        [Export] public ItemList EntryList { get; set; }
        
        // Detail view
        [Export] public Control DetailPanel { get; set; }
        [Export] public TextureRect EntryPortrait { get; set; }
        [Export] public TextureRect EntryBanner { get; set; }
        [Export] public Label EntryNameLabel { get; set; }
        [Export] public Label EntryCategoryLabel { get; set; }
        [Export] public Label EntryEraLabel { get; set; }
        [Export] public Label EntryLocationLabel { get; set; }
        [Export] public Label EntryAuthorLabel { get; set; }
        [Export] public Label ShortDescLabel { get; set; }
        [Export] public RichTextLabel DetailedDescLabel { get; set; }
        
        // Section navigation
        [Export] public Control SectionContainer { get; set; }
        [Export] public Button PrevSectionButton { get; set; }
        [Export] public Button NextSectionButton { get; set; }
        [Export] public Label SectionTitleLabel { get; set; }
        [Export] public RichTextLabel SectionContentLabel { get; set; }
        [Export] public TextureRect SectionImage { get; set; }
        
        // Related entries
        [Export] public VBoxContainer RelatedEntriesContainer { get; set; }
        
        // Status indicators
        [Export] public Label NewIndicator { get; set; }
        [Export] public Label UnreadCount { get; set; }
        #endregion

        #region State
        private List<(LoreEntryData data, LoreCodexEntry status)> currentEntries = new();
        private LoreEntryData selectedEntry;
        private LoreCodexEntry selectedStatus;
        private int currentSectionIndex = 0;
        private LoreCategory currentCategoryFilter = (LoreCategory)(-1); // -1 = All
        private string currentSearchText = "";
        #endregion

        public override void _Ready()
        {
            Visible = false;
            
            // Connect signals
            CloseButton?.Connect("pressed", new Callable(this, nameof(OnClosePressed)));
            SearchBar?.Connect("text_changed", new Callable(this, nameof(OnSearchTextChanged)));
            CategoryFilter?.Connect("item_selected", new Callable(this, nameof(OnCategorySelected)));
            EraFilter?.Connect("item_selected", new Callable(this, nameof(OnEraSelected)));
            SortOption?.Connect("item_selected", new Callable(this, nameof(OnSortChanged)));
            EntryList?.Connect("item_selected", new Callable(this, nameof(OnEntrySelected)));
            PrevSectionButton?.Connect("pressed", new Callable(this, nameof(OnPrevSection)));
            NextSectionButton?.Connect("pressed", new Callable(this, nameof(OnNextSection)));
            
            // Connect to LoreCodexManager signals
            if (LoreCodexManager.Instance != null)
            {
                LoreCodexManager.Instance.CodexUpdated += RefreshEntryList;
            }
            
            // Initialize filters
            InitializeFilters();
        }

        private void InitializeFilters()
        {
            // Category filter
            if (CategoryFilter != null)
            {
                CategoryFilter.Clear();
                CategoryFilter.AddItem("All Categories", -1);
                
                foreach (LoreCategory category in Enum.GetValues(typeof(LoreCategory)))
                {
                    CategoryFilter.AddItem(category.ToString(), (int)category);
                }
            }

            // Era filter
            if (EraFilter != null)
            {
                EraFilter.Clear();
                EraFilter.AddItem("All Eras", -1);
                
                var eras = LoreCodexManager.Instance?.GetAllEras() ?? new List<string>();
                foreach (var era in eras)
                {
                    EraFilter.AddItem(era);
                }
            }

            // Sort options
            if (SortOption != null)
            {
                SortOption.Clear();
                SortOption.AddItem("Name (A-Z)", (int)LoreSortType.ByName);
                SortOption.AddItem("Category", (int)LoreSortType.ByCategory);
                SortOption.AddItem("Discovery Date", (int)LoreSortType.ByDiscoveryDate);
                SortOption.AddItem("Era", (int)LoreSortType.ByEra);
            }
        }

        public void ShowCodex()
        {
            Visible = true;
            GetTree().Paused = true;
            RefreshEntryList();
            UpdateCompletionDisplay();
            SystemManager.Instance?.PlayOkSE();
        }

        public void HideCodex()
        {
            Visible = false;
            GetTree().Paused = false;
            SystemManager.Instance?.PlayCancelSE();
        }

        private void RefreshEntryList()
        {
            if (EntryList == null || LoreCodexManager.Instance == null)
                return;

            EntryList.Clear();

            // Get filtered entries
            currentEntries = LoreCodexManager.Instance.GetFilteredEntries(
                LoreFilterType.All,
                currentCategoryFilter >= 0 ? (LoreCategory?)currentCategoryFilter : null
            );

            // Apply search filter
            if (!string.IsNullOrEmpty(currentSearchText))
            {
                currentEntries = currentEntries.Where(e => 
                    e.data.EntryName.Contains(currentSearchText, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            // Sort entries
            var sortType = (LoreSortType)(SortOption?.Selected ?? 0);
            currentEntries = LoreCodexManager.Instance.GetSortedEntries(sortType);

            // Populate list
            foreach (var (data, status) in currentEntries)
            {
                string displayName = status.IsDiscovered ? data.EntryName : "???";
                string prefix = "";
                
                if (status.IsDiscovered && !status.HasBeenRead)
                {
                    prefix = "[NEW] ";
                }

                EntryList.AddItem($"{prefix}{displayName}");
                
                // Set icon if available
                if (status.IsDiscovered && data.Portrait != null)
                {
                    EntryList.SetItemIcon(EntryList.ItemCount - 1, data.Portrait);
                }
            }

            UpdateUnreadCount();
        }

        private void UpdateCompletionDisplay()
        {
            if (LoreCodexManager.Instance == null)
                return;

            int discovered = LoreCodexManager.Instance.TotalDiscovered;
            int total = LoreCodexManager.Instance.TotalEntriesInGame;
            float percentage = LoreCodexManager.Instance.CompletionPercentage;

            if (CompletionLabel != null)
            {
                CompletionLabel.Text = $"Completion: {discovered}/{total} ({percentage:F1}%)";
            }

            if (CompletionBar != null)
            {
                CompletionBar.Value = percentage;
            }
        }

        private void UpdateUnreadCount()
        {
            if (UnreadCount != null && LoreCodexManager.Instance != null)
            {
                int unread = LoreCodexManager.Instance.TotalUnread;
                UnreadCount.Text = unread > 0 ? $"{unread} New" : "";
                UnreadCount.Visible = unread > 0;
            }
        }

        private void OnEntrySelected(long index)
        {
            if (index < 0 || index >= currentEntries.Count)
                return;

            var (data, status) = currentEntries[(int)index];
            
            if (!status.IsDiscovered)
            {
                SystemManager.Instance?.PlayBuzzerSE();
                return;
            }

            selectedEntry = data;
            selectedStatus = status;
            currentSectionIndex = 0;

            // Mark as read
            LoreCodexManager.Instance?.MarkAsRead(data.EntryId);

            DisplayEntry();
            SystemManager.Instance?.PlayCursorSE();
        }

        private void DisplayEntry()
        {
            if (selectedEntry == null || DetailPanel == null)
                return;

            DetailPanel.Visible = true;

            // Basic info
            if (EntryNameLabel != null)
                EntryNameLabel.Text = selectedEntry.EntryName;

            if (EntryCategoryLabel != null)
                EntryCategoryLabel.Text = $"Category: {selectedEntry.Category}";

            if (EntryEraLabel != null)
            {
                EntryEraLabel.Text = !string.IsNullOrEmpty(selectedEntry.Era) 
                    ? $"Era: {selectedEntry.Era}" 
                    : "";
                EntryEraLabel.Visible = !string.IsNullOrEmpty(selectedEntry.Era);
            }

            if (EntryLocationLabel != null)
            {
                EntryLocationLabel.Text = !string.IsNullOrEmpty(selectedEntry.Location) 
                    ? $"Location: {selectedEntry.Location}" 
                    : "";
                EntryLocationLabel.Visible = !string.IsNullOrEmpty(selectedEntry.Location);
            }

            if (EntryAuthorLabel != null)
            {
                EntryAuthorLabel.Text = !string.IsNullOrEmpty(selectedEntry.Author) 
                    ? $"Author: {selectedEntry.Author}" 
                    : "";
                EntryAuthorLabel.Visible = !string.IsNullOrEmpty(selectedEntry.Author);
            }

            // Images
            if (EntryPortrait != null)
            {
                EntryPortrait.Texture = selectedEntry.Portrait;
                EntryPortrait.Visible = selectedEntry.Portrait != null;
            }

            if (EntryBanner != null)
            {
                EntryBanner.Texture = selectedEntry.Banner;
                EntryBanner.Visible = selectedEntry.Banner != null;
            }

            // Description
            if (ShortDescLabel != null)
                ShortDescLabel.Text = selectedEntry.ShortDescription;

            if (DetailedDescLabel != null)
                DetailedDescLabel.Text = selectedEntry.DetailedDescription;

            // Sections
            DisplaySection();
            
            // Related entries
            DisplayRelatedEntries();

            // Hide "new" indicator
            if (NewIndicator != null)
                NewIndicator.Visible = false;

            RefreshEntryList(); // Refresh to update [NEW] markers
        }

        private void DisplaySection()
        {
            if (selectedEntry == null || SectionContainer == null)
                return;

            bool hasSections = selectedEntry.Sections != null && selectedEntry.Sections.Count > 0;
            SectionContainer.Visible = hasSections;

            if (!hasSections)
                return;

            // Clamp section index
            currentSectionIndex = Mathf.Clamp(currentSectionIndex, 0, selectedEntry.Sections.Count - 1);
            
            var section = selectedEntry.Sections[currentSectionIndex];

            // Check if section is unlocked
            bool isUnlocked = selectedStatus?.UnlockedSectionIndices.Contains(currentSectionIndex) ?? false;

            if (SectionTitleLabel != null)
            {
                SectionTitleLabel.Text = isUnlocked 
                    ? section.SectionTitle 
                    : "??? (Locked)";
            }

            if (SectionContentLabel != null)
            {
                SectionContentLabel.Text = isUnlocked 
                    ? section.Content 
                    : "This section has not been unlocked yet.";
            }

            if (SectionImage != null)
            {
                SectionImage.Texture = isUnlocked ? section.Image : null;
                SectionImage.Visible = isUnlocked && section.Image != null;
            }

            // Update navigation buttons
            if (PrevSectionButton != null)
                PrevSectionButton.Disabled = currentSectionIndex <= 0;

            if (NextSectionButton != null)
                NextSectionButton.Disabled = currentSectionIndex >= selectedEntry.Sections.Count - 1;
        }

        private void DisplayRelatedEntries()
        {
            if (selectedEntry == null || RelatedEntriesContainer == null)
                return;

            // Clear existing buttons
            foreach (var child in RelatedEntriesContainer.GetChildren())
            {
                child.QueueFree();
            }

            if (selectedEntry.RelatedEntryIds == null || selectedEntry.RelatedEntryIds.Count == 0)
            {
                RelatedEntriesContainer.Visible = false;
                return;
            }

            RelatedEntriesContainer.Visible = true;

            foreach (var relatedId in selectedEntry.RelatedEntryIds)
            {
                var relatedData = LoreCodexManager.Instance?.GetLoreEntry(relatedId);
                var relatedStatus = LoreCodexManager.Instance?.GetCodexEntry(relatedId);

                if (relatedData == null)
                    continue;

                var button = new Button();
                button.Text = relatedStatus?.IsDiscovered == true 
                    ? $"→ {relatedData.EntryName}" 
                    : "→ ???";
                button.Disabled = relatedStatus?.IsDiscovered != true;
                
                string capturedId = relatedId; // Capture for lambda
                button.Pressed += () => JumpToEntry(capturedId);
                
                RelatedEntriesContainer.AddChild(button);
            }
        }

        private void JumpToEntry(string entryId)
        {
            // Find entry in current list
            for (int i = 0; i < currentEntries.Count; i++)
            {
                if (currentEntries[i].data.EntryId == entryId)
                {
                    EntryList.Select(i);
                    OnEntrySelected(i);
                    SystemManager.Instance?.PlayOkSE();
                    return;
                }
            }

            // If not in current filter, show all and try again
            currentCategoryFilter = (LoreCategory)(-1);
            currentSearchText = "";
            if (CategoryFilter != null) CategoryFilter.Selected = 0;
            if (SearchBar != null) SearchBar.Text = "";
            
            RefreshEntryList();
            
            for (int i = 0; i < currentEntries.Count; i++)
            {
                if (currentEntries[i].data.EntryId == entryId)
                {
                    EntryList.Select(i);
                    OnEntrySelected(i);
                    SystemManager.Instance?.PlayOkSE();
                    return;
                }
            }
        }

        private void OnPrevSection()
        {
            if (currentSectionIndex > 0)
            {
                currentSectionIndex--;
                DisplaySection();
                SystemManager.Instance?.PlayCursorSE();
            }
        }

        private void OnNextSection()
        {
            if (selectedEntry != null && currentSectionIndex < selectedEntry.Sections.Count - 1)
            {
                currentSectionIndex++;
                DisplaySection();
                SystemManager.Instance?.PlayCursorSE();
            }
        }

        private void OnSearchTextChanged(string newText)
        {
            currentSearchText = newText;
            RefreshEntryList();
        }

        private void OnCategorySelected(long index)
        {
            currentCategoryFilter = (LoreCategory)(CategoryFilter.GetItemId((int)index));
            RefreshEntryList();
            SystemManager.Instance?.PlayCursorSE();
        }

        private void OnEraSelected(long index)
        {
            // Implement era filtering
            RefreshEntryList();
            SystemManager.Instance?.PlayCursorSE();
        }

        private void OnSortChanged(long index)
        {
            RefreshEntryList();
            SystemManager.Instance?.PlayCursorSE();
        }

        private void OnClosePressed()
        {
            HideCodex();
        }

        public override void _Input(InputEvent @event)
        {
            if (!Visible) return;

            if (@event.IsActionPressed("ui_cancel"))
            {
                HideCodex();
                GetViewport().SetInputAsHandled();
            }
        }
    }
}