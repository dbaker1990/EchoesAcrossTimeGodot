// LoreCodex/LoreCodexSceneBuilder.cs
using Godot;

namespace EchoesAcrossTime.LoreCodex
{
    /// <summary>
    /// Helper class that prints the exact scene structure for Lore Codex UI scenes
    /// Attach to a node and run to see the structure in console
    /// </summary>
    public partial class LoreCodexSceneBuilder : Node
    {
        public override void _Ready()
        {
            GD.Print(@"
╔════════════════════════════════════════════════════════════════╗
║         LORE CODEX SCENE STRUCTURES - COPY & PASTE             ║
╚════════════════════════════════════════════════════════════════╝
");
            
            PrintLoreCodexUIStructure();
            GD.Print("\n" + new string('═', 70) + "\n");
            PrintNotificationStructure();
            GD.Print("\n" + new string('═', 70) + "\n");
            PrintTestSceneStructure();
        }

        private void PrintLoreCodexUIStructure()
        {
            GD.Print(@"
=== LORE CODEX UI STRUCTURE ===

LoreCodexUI (Control) [unique_name: %LoreCodexUI]
│   [anchors: Full Rect, visible: false]
│   [script: res://LoreCodex/LoreCodexUI.cs]
│
└── MainPanel (Panel) [anchors: Full Rect]
    │   [custom_minimum_size: (1000, 600)]
    │
    └── MarginContainer [margins: 20]
        └── VBoxContainer [separation: 10]
            │
            ├── TopBar (HBoxContainer) [custom_minimum_size: (0, 60)]
            │   │   [separation: 10]
            │   │
            │   ├── TitleLabel (Label)
            │   │   [text: 'Lore Codex', custom_font_size: 32]
            │   │
            │   ├── Spacer (Control) [size_flags_h: 3]
            │   │
            │   ├── CompletionLabel (Label)
            │   │   [text: 'Completion: 0/0 (0%)', custom_font_size: 16]
            │   │
            │   ├── CompletionBar (ProgressBar)
            │   │   [min_value: 0, max_value: 100, custom_minimum_size: (200, 0)]
            │   │
            │   └── CloseButton (Button)
            │       [text: 'Close', custom_minimum_size: (100, 40)]
            │
            ├── Separator1 (HSeparator)
            │
            ├── FilterBar (HBoxContainer) [custom_minimum_size: (0, 40)]
            │   │   [separation: 10]
            │   │
            │   ├── SearchBar (LineEdit)
            │   │   [placeholder_text: 'Search lore...', custom_minimum_size: (250, 0)]
            │   │
            │   ├── CategoryFilter (OptionButton)
            │   │   [custom_minimum_size: (150, 0)]
            │   │
            │   ├── EraFilter (OptionButton)
            │   │   [custom_minimum_size: (150, 0)]
            │   │
            │   ├── SortOption (OptionButton)
            │   │   [custom_minimum_size: (150, 0)]
            │   │
            │   └── UnreadCount (Label)
            │       [text: '', custom_font_size: 14, modulate: Color(1, 0.8, 0)]
            │
            ├── Separator2 (HSeparator)
            │
            └── Content (HSplitContainer) [size_flags_v: 3]
                │
                ├── LeftPanel (VBoxContainer) [custom_minimum_size: (300, 0)]
                │   │   [separation: 5]
                │   │
                │   ├── ListTitle (Label)
                │   │   [text: 'Entries', custom_font_size: 18]
                │   │
                │   └── EntryList (ItemList)
                │       [size_flags_v: 3, icon_mode: 1 (Left), fixed_icon_size: (48, 48)]
                │
                └── DetailPanel (ScrollContainer) [size_flags_h: 3]
                    │   [horizontal_scroll_mode: 0 (Disabled)]
                    │
                    └── DetailVBox (VBoxContainer)
                        │   [custom_minimum_size: (400, 0), separation: 10]
                        │
                        ├── EntryBanner (TextureRect)
                        │   [custom_minimum_size: (0, 200), expand_mode: 1 (Ignore Size)]
                        │   [stretch_mode: 5 (Keep Aspect Covered)]
                        │
                        ├── ProfileSection (HBoxContainer)
                        │   │
                        │   ├── EntryPortrait (TextureRect)
                        │   │   [custom_minimum_size: (150, 200), expand_mode: 1]
                        │   │   [stretch_mode: 4 (Keep Aspect Centered)]
                        │   │
                        │   └── InfoVBox (VBoxContainer) [size_flags_h: 3]
                        │       │
                        │       ├── EntryNameLabel (Label)
                        │       │   [text: 'Entry Name', custom_font_size: 28]
                        │       │
                        │       ├── EntryCategoryLabel (Label)
                        │       │   [text: 'Category: ', custom_font_size: 14]
                        │       │
                        │       ├── EntryEraLabel (Label)
                        │       │   [text: 'Era: ', custom_font_size: 14]
                        │       │
                        │       ├── EntryLocationLabel (Label)
                        │       │   [text: 'Location: ', custom_font_size: 14]
                        │       │
                        │       ├── EntryAuthorLabel (Label)
                        │       │   [text: 'Author: ', custom_font_size: 12, modulate: Color(0.8, 0.8, 0.8)]
                        │       │
                        │       └── NewIndicator (Label)
                        │           [text: '[NEW]', custom_font_size: 16]
                        │           [modulate: Color(1, 0.8, 0), visible: false]
                        │
                        ├── Sep3 (HSeparator)
                        │
                        ├── ShortDescLabel (Label)
                        │   [text: 'Short description...', autowrap_mode: 3 (Word)]
                        │   [custom_font_size: 16]
                        │
                        ├── Sep4 (HSeparator)
                        │
                        ├── DetailedDescLabel (RichTextLabel)
                        │   [text: 'Detailed description...', bbcode_enabled: true]
                        │   [custom_minimum_size: (0, 200), size_flags_v: 3]
                        │   [fit_content: true]
                        │
                        ├── Sep5 (HSeparator)
                        │
                        ├── SectionContainer (VBoxContainer) [separation: 10]
                        │   │   [visible: false]
                        │   │
                        │   ├── SectionHeader (Label)
                        │   │   [text: 'Additional Sections', custom_font_size: 20]
                        │   │
                        │   ├── SectionNav (HBoxContainer)
                        │   │   │
                        │   │   ├── PrevSectionButton (Button)
                        │   │   │   [text: '◀ Previous Section', custom_minimum_size: (150, 40)]
                        │   │   │
                        │   │   ├── SectionTitleLabel (Label) [size_flags_h: 3]
                        │   │   │   [text: 'Section Title', custom_font_size: 18, align: 1 (Center)]
                        │   │   │
                        │   │   └── NextSectionButton (Button)
                        │   │       [text: 'Next Section ▶', custom_minimum_size: (150, 40)]
                        │   │
                        │   ├── SectionImage (TextureRect)
                        │   │   [custom_minimum_size: (0, 200), expand_mode: 1]
                        │   │
                        │   └── SectionContentLabel (RichTextLabel)
                        │       [text: 'Section content...', bbcode_enabled: true]
                        │       [custom_minimum_size: (0, 150), fit_content: true]
                        │
                        ├── Sep6 (HSeparator)
                        │
                        └── RelatedSection (VBoxContainer)
                            │
                            ├── RelatedHeader (Label)
                            │   [text: 'Related Entries', custom_font_size: 18]
                            │
                            └── RelatedEntriesContainer (VBoxContainer)
                                [separation: 5]

EXPORT CONNECTIONS (in Inspector):
- MainPanel → MainPanel
- TitleLabel → TitleLabel
- CompletionLabel → CompletionLabel
- CompletionBar → CompletionBar
- CloseButton → CloseButton
- SearchBar → SearchBar
- CategoryFilter → CategoryFilter
- EraFilter → EraFilter
- SortOption → SortOption
- UnreadCount → UnreadCount
- EntryList → EntryList
- DetailPanel → DetailPanel
- EntryBanner → EntryBanner
- EntryPortrait → EntryPortrait
- EntryNameLabel → EntryNameLabel
- EntryCategoryLabel → EntryCategoryLabel
- EntryEraLabel → EntryEraLabel
- EntryLocationLabel → EntryLocationLabel
- EntryAuthorLabel → EntryAuthorLabel
- NewIndicator → NewIndicator
- ShortDescLabel → ShortDescLabel
- DetailedDescLabel → DetailedDescLabel
- SectionContainer → SectionContainer
- PrevSectionButton → PrevSectionButton
- NextSectionButton → NextSectionButton
- SectionTitleLabel → SectionTitleLabel
- SectionImage → SectionImage
- SectionContentLabel → SectionContentLabel
- RelatedEntriesContainer → RelatedEntriesContainer
            ");
        }

        private void PrintNotificationStructure()
        {
            GD.Print(@"
=== LORE CODEX NOTIFICATION STRUCTURE ===

LoreCodexNotification (Control) [unique_name: %LoreCodexNotification]
│   [anchors: Top Center, offset_left: -200, offset_right: 200]
│   [offset_top: -200, offset_bottom: -100]
│   [script: res://LoreCodex/LoreCodexNotification.cs]
│
└── NotificationPanel (Panel)
    │   [custom_minimum_size: (400, 100)]
    │
    └── MarginContainer [margins: 10]
        └── HBoxContainer [separation: 10]
            │
            ├── IconRect (TextureRect)
            │   [custom_minimum_size: (64, 64)]
            │   [expand_mode: 1 (Ignore Size)]
            │   [stretch_mode: 4 (Keep Aspect Centered)]
            │
            └── TextVBox (VBoxContainer) [size_flags_h: 3]
                │
                ├── TitleLabel (Label)
                │   [text: 'New Lore Discovered!', custom_font_size: 18]
                │   [modulate: Color(1, 0.8, 0)]
                │
                ├── DescriptionLabel (Label)
                │   [text: 'Entry Name', custom_font_size: 16]
                │
                └── CategoryLabel (Label)
                    [text: '[Category]', custom_font_size: 12]
                    [modulate: Color(0.7, 0.7, 0.7)]

EXPORT CONNECTIONS (in Inspector):
- NotificationPanel → NotificationPanel
- IconRect → IconRect
- TitleLabel → TitleLabel
- DescriptionLabel → DescriptionLabel
- CategoryLabel → CategoryLabel

IMPORTANT SETTINGS (in Inspector):
- HiddenPosition: (0, -200)
- ShownPosition: (0, 50)
- DisplayDuration: 3.0
- FadeInDuration: 0.3
- FadeOutDuration: 0.3
            ");
        }

        private void PrintTestSceneStructure()
        {
            GD.Print(@"
=== LORE CODEX TEST SCENE STRUCTURE ===

LoreCodexTestScene (Node2D)
│   [script: res://LoreCodex/LoreCodexTestScene.cs]
│
├── Camera2D (Camera2D)
│   [enabled: true, zoom: (1, 1)]
│
├── Background (ColorRect)
│   [anchors: Full Rect, color: Color(0.17, 0.24, 0.31)]
│
└── UI (CanvasLayer)
    └── CenterContainer [anchors: Full Rect]
        └── Panel [custom_minimum_size: (500, 700)]
            └── MarginContainer [margins: 20]
                └── VBoxContainer [separation: 10]
                    │
                    ├── TitleLabel (Label)
                    │   [text: 'Lore Codex Test Scene', custom_font_size: 32]
                    │   [horizontal_alignment: 1 (Center)]
                    │
                    ├── Sep1 (HSeparator)
                    │
                    ├── StatusLabel (Label)
                    │   [text: 'Status...', custom_font_size: 16, lines: 3]
                    │   [horizontal_alignment: 1 (Center)]
                    │
                    ├── Sep2 (HSeparator)
                    │
                    ├── DiscoveryHeader (Label)
                    │   [text: 'Discovery Tests', custom_font_size: 20]
                    │
                    ├── OpenCodexButton (Button)
                    │   [text: 'Open Lore Codex (Space)', custom_minimum_size: (0, 50)]
                    │
                    ├── DiscoverCharacterButton (Button)
                    │   [text: 'Discover Character Entry (1)', custom_minimum_size: (0, 40)]
                    │
                    ├── DiscoverLocationButton (Button)
                    │   [text: 'Discover Location Entry (2)', custom_minimum_size: (0, 40)]
                    │
                    ├── DiscoverEventButton (Button)
                    │   [text: 'Discover Event Entry (3)', custom_minimum_size: (0, 40)]
                    │
                    ├── DiscoverConceptButton (Button)
                    │   [text: 'Discover Concept Entry (4)', custom_minimum_size: (0, 40)]
                    │
                    ├── Sep3 (HSeparator)
                    │
                    ├── AdvancedHeader (Label)
                    │   [text: 'Advanced Tests', custom_font_size: 20]
                    │
                    ├── UnlockSectionButton (Button)
                    │   [text: 'Unlock Section in Entry (S)', custom_minimum_size: (0, 40)]
                    │
                    ├── UnlockAllButton (Button)
                    │   [text: 'Unlock All Entries (A)', custom_minimum_size: (0, 40)]
                    │
                    ├── ResetButton (Button)
                    │   [text: 'Reset All Progress (R)', custom_minimum_size: (0, 40)]
                    │
                    ├── Sep4 (HSeparator)
                    │
                    └── HelpLabel (Label)
                        [text: 'ESC to quit | Numbers/Letters for shortcuts']
                        [custom_font_size: 12, horizontal_alignment: 1]

EXPORT CONNECTIONS (in Inspector):
- OpenCodexButton → OpenCodexButton
- DiscoverCharacterButton → DiscoverCharacterButton
- DiscoverLocationButton → DiscoverLocationButton
- DiscoverEventButton → DiscoverEventButton
- DiscoverConceptButton → DiscoverConceptButton
- UnlockSectionButton → UnlockSectionButton
- UnlockAllButton → UnlockAllButton
- ResetButton → ResetButton
- StatusLabel → StatusLabel

KEYBOARD SHORTCUTS:
- Space: Open Codex
- 1: Discover Character
- 2: Discover Location
- 3: Discover Event
- 4: Discover Concept
- S: Unlock Section
- A: Unlock All
- R: Reset
- ESC: Quit
            ");
        }
    }
}