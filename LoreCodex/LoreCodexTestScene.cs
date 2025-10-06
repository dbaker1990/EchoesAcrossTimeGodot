// LoreCodex/LoreCodexTestScene.cs
using Godot;
using System;
using EchoesAcrossTime.LoreCodex;
using EchoesAcrossTime.Examples;
using EchoesAcrossTime.UI;

namespace EchoesAcrossTime.Testing
{
    /// <summary>
    /// Test scene for the Lore Codex system
    /// Create a new scene and attach this script to test the codex
    /// </summary>
    public partial class LoreCodexTestScene : Node2D
    {
        [Export] public Button OpenCodexButton { get; set; }
        [Export] public Button DiscoverCharacterButton { get; set; }
        [Export] public Button DiscoverLocationButton { get; set; }
        [Export] public Button DiscoverEventButton { get; set; }
        [Export] public Button DiscoverConceptButton { get; set; }
        [Export] public Button UnlockSectionButton { get; set; }
        [Export] public Button UnlockAllButton { get; set; }
        [Export] public Button ResetButton { get; set; }
        [Export] public Label StatusLabel { get; set; }

        private LoreCodexUI codexUI;
        private LoreCodexNotification notification;

        public override void _Ready()
        {
            GD.Print("=== Lore Codex Test Scene ===");
            
            // Initialize test entries
            ExampleLoreEntries.RegisterAllExamples();
            
            // Create UI instances
            SetupUI();
            
            // Connect buttons
            ConnectButtons();
            
            // Initial status update
            UpdateStatus();
            
            GD.Print("Test scene ready! Use buttons to test the system.");
        }

        private void SetupUI()
        {
            // Load and instantiate UI scenes
            var codexScene = GD.Load<PackedScene>("res://LoreCodex/LoreCodexUI.tscn");
            var notificationScene = GD.Load<PackedScene>("res://LoreCodex/LoreCodexNotification.tscn");

            if (codexScene != null)
            {
                codexUI = codexScene.Instantiate<LoreCodexUI>();
                AddChild(codexUI);
                GD.Print("✓ LoreCodexUI loaded");
            }
            else
            {
                GD.PrintErr("✗ Could not load LoreCodexUI.tscn");
            }

            if (notificationScene != null)
            {
                notification = notificationScene.Instantiate<LoreCodexNotification>();
                AddChild(notification);
                GD.Print("✓ LoreCodexNotification loaded");
            }
            else
            {
                GD.PrintErr("✗ Could not load LoreCodexNotification.tscn");
            }

            // Connect to manager signals
            if (LoreCodexManager.Instance != null)
            {
                LoreCodexManager.Instance.LoreEntryDiscovered += OnLoreDiscovered;
                LoreCodexManager.Instance.CodexUpdated += UpdateStatus;
            }
        }

        private void ConnectButtons()
        {
            OpenCodexButton?.Connect("pressed", Callable.From(OnOpenCodex));
            DiscoverCharacterButton?.Connect("pressed", Callable.From(OnDiscoverCharacter));
            DiscoverLocationButton?.Connect("pressed", Callable.From(OnDiscoverLocation));
            DiscoverEventButton?.Connect("pressed", Callable.From(OnDiscoverEvent));
            DiscoverConceptButton?.Connect("pressed", Callable.From(OnDiscoverConcept));
            UnlockSectionButton?.Connect("pressed", Callable.From(OnUnlockSection));
            UnlockAllButton?.Connect("pressed", Callable.From(OnUnlockAll));
            ResetButton?.Connect("pressed", Callable.From(OnReset));
        }

        private void OnOpenCodex()
        {
            GD.Print("Opening Lore Codex...");
            codexUI?.ShowCodex();
        }

        private void OnDiscoverCharacter()
        {
            GD.Print("Discovering character entry...");
            LoreCodexManager.Instance?.DiscoverEntry("dominic_ashford");
        }

        private void OnDiscoverLocation()
        {
            GD.Print("Discovering location entry...");
            LoreCodexManager.Instance?.DiscoverEntry("crystal_cove");
        }

        private void OnDiscoverEvent()
        {
            GD.Print("Discovering event entry...");
            LoreCodexManager.Instance?.DiscoverEntry("the_great_sundering");
        }

        private void OnDiscoverConcept()
        {
            GD.Print("Discovering concept entry...");
            LoreCodexManager.Instance?.DiscoverEntry("temporal_magic");
        }

        private void OnUnlockSection()
        {
            GD.Print("Unlocking section in Dominic's entry...");
            LoreCodexManager.Instance?.UnlockSection("dominic_ashford", 1);
        }

        private void OnUnlockAll()
        {
            GD.Print("Unlocking all entries...");
            LoreCodexManager.Instance?.UnlockAll();
        }

        private void OnReset()
        {
            GD.Print("Resetting all progress...");
            LoreCodexManager.Instance?.ResetProgress();
        }

        private void OnLoreDiscovered(string entryId)
        {
            var entry = LoreCodexManager.Instance?.GetLoreEntry(entryId);
            GD.Print($"🎉 Discovered: {entry?.EntryName ?? entryId}");
        }

        private void UpdateStatus()
        {
            if (StatusLabel == null || LoreCodexManager.Instance == null)
                return;

            int discovered = LoreCodexManager.Instance.TotalDiscovered;
            int total = LoreCodexManager.Instance.TotalEntriesInGame;
            int unread = LoreCodexManager.Instance.TotalUnread;
            float completion = LoreCodexManager.Instance.CompletionPercentage;

            StatusLabel.Text = $@"Lore Codex Status:
Discovered: {discovered}/{total} ({completion:F1}%)
Unread: {unread}";
        }

        public override void _Input(InputEvent @event)
        {
            if (@event.IsActionPressed("ui_cancel"))
            {
                GetTree().Quit();
            }

            // Quick test shortcuts
            if (@event is InputEventKey keyEvent && keyEvent.Pressed)
            {
                switch (keyEvent.Keycode)
                {
                    case Key.Key1:
                        OnDiscoverCharacter();
                        break;
                    case Key.Key2:
                        OnDiscoverLocation();
                        break;
                    case Key.Key3:
                        OnDiscoverEvent();
                        break;
                    case Key.Key4:
                        OnDiscoverConcept();
                        break;
                    case Key.S:
                        OnUnlockSection();
                        break;
                    case Key.A:
                        OnUnlockAll();
                        break;
                    case Key.R:
                        OnReset();
                        break;
                    case Key.Space:
                        OnOpenCodex();
                        break;
                }
            }
        }

        public override void _ExitTree()
        {
            if (LoreCodexManager.Instance != null)
            {
                LoreCodexManager.Instance.LoreEntryDiscovered -= OnLoreDiscovered;
                LoreCodexManager.Instance.CodexUpdated -= UpdateStatus;
            }
        }
    }

    /// <summary>
    /// Scene builder helper - prints the structure for the test scene
    /// </summary>
    public static class LoreTestSceneBuilder
    {
        public static void PrintSceneStructure()
        {
            GD.Print(@"
=== LORE CODEX TEST SCENE STRUCTURE ===

LoreCodexTestScene (Node2D)
├── Camera2D (Camera2D) [enabled = true]
├── Background (ColorRect) [color = #2C3E50, anchors = Full Rect]
├── UI (CanvasLayer)
│   └── VBoxContainer [anchors = Center, custom_minimum_size = (400, 0)]
│       ├── TitleLabel (Label)
│       │   └── [text = 'Lore Codex Test Scene', align = Center, custom_font_size = 32]
│       ├── Separator1 (HSeparator)
│       ├── StatusLabel (Label)
│       │   └── [text = 'Status...', align = Center, custom_font_size = 16]
│       ├── Separator2 (HSeparator)
│       ├── Label1 (Label) [text = 'Discovery Tests:', custom_font_size = 20]
│       ├── OpenCodexButton (Button)
│       │   └── [text = 'Open Lore Codex (Space)', min_size = (300, 50)]
│       ├── DiscoverCharacterButton (Button)
│       │   └── [text = 'Discover Character (1)', min_size = (300, 40)]
│       ├── DiscoverLocationButton (Button)
│       │   └── [text = 'Discover Location (2)', min_size = (300, 40)]
│       ├── DiscoverEventButton (Button)
│       │   └── [text = 'Discover Event (3)', min_size = (300, 40)]
│       ├── DiscoverConceptButton (Button)
│       │   └── [text = 'Discover Concept (4)', min_size = (300, 40)]
│       ├── Separator3 (HSeparator)
│       ├── Label2 (Label) [text = 'Advanced Tests:', custom_font_size = 20]
│       ├── UnlockSectionButton (Button)
│       │   └── [text = 'Unlock Section (S)', min_size = (300, 40)]
│       ├── UnlockAllButton (Button)
│       │   └── [text = 'Unlock All Entries (A)', min_size = (300, 40)]
│       ├── ResetButton (Button)
│       │   └── [text = 'Reset Progress (R)', min_size = (300, 40)]
│       ├── Separator4 (HSeparator)
│       └── HelpLabel (Label)
│           └── [text = 'ESC to quit | Use numbers/letters for shortcuts', 
│                align = Center, custom_font_size = 12]

Attach LoreCodexTestScene.cs to the root node and connect all button exports!

IMPORTANT: Make sure you have:
1. LoreCodexManager in Autoload
2. LoreCodexUI.tscn exists
3. LoreCodexNotification.tscn exists
            ");
        }
    }
}