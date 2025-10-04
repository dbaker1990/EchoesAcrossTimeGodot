// Testing/QuestTestScene.cs
using Godot;
using System;
using EchoesAcrossTime.Quests;

namespace EchoesAcrossTime.Testing
{
    /// <summary>
    /// Test scene for the quest system
    /// Demonstrates all quest features
    /// </summary>
    public partial class QuestTestScene : Node2D
    {
        private VBoxContainer buttonContainer;
        private Label statusLabel;
        
        public override void _Ready()
        {
            GD.Print("=== Quest System Test Scene ===");
            
            // Create UI
            CreateTestUI();
            
            // Register example quests
            ExampleQuests.RegisterAllExampleQuests();
            
            // Connect to quest signals for logging
            ConnectQuestSignals();
            
            UpdateStatus("Quest Test Scene Ready! Use buttons to test features.");
        }
        
        private void CreateTestUI()
        {
            var canvas = new CanvasLayer();
            AddChild(canvas);
            
            var panel = new PanelContainer();
            panel.Position = new Vector2(20, 20);
            panel.CustomMinimumSize = new Vector2(400, 600);
            canvas.AddChild(panel);
            
            var vbox = new VBoxContainer();
            vbox.AddThemeConstantOverride("separation", 10);
            panel.AddChild(vbox);
            
            // Title
            var title = new Label();
            title.Text = "Quest System Test";
            title.AddThemeFontSizeOverride("font_size", 24);
            title.HorizontalAlignment = HorizontalAlignment.Center;
            vbox.AddChild(title);
            
            // Status label
            statusLabel = new Label();
            statusLabel.Text = "Status: Ready";
            statusLabel.AutowrapMode = TextServer.AutowrapMode.Word;
            statusLabel.CustomMinimumSize = new Vector2(0, 60);
            vbox.AddChild(statusLabel);
            
            var separator1 = new HSeparator();
            vbox.AddChild(separator1);
            
            // Button container
            buttonContainer = new VBoxContainer();
            buttonContainer.AddThemeConstantOverride("separation", 5);
            vbox.AddChild(buttonContainer);
            
            // Test buttons
            AddTestButton("Start Main Quest", () => TestStartQuest("quest_main_001"));
            AddTestButton("Start Side Quest (Merchant)", () => TestStartQuest("quest_side_001"));
            AddTestButton("Start Time-Sensitive Quest", () => TestStartQuest("quest_time_001"));
            AddTestButton("Start Quest Board Quest", () => TestStartQuest("quest_board_001"));
            
            var separator2 = new HSeparator();
            buttonContainer.AddChild(separator2);
            
            AddTestButton("Update Objective (Archives)", () => TestUpdateObjective("quest_main_001", "obj_visit_archives", 1));
            AddTestButton("Update Objective (Collect Herbs)", () => TestUpdateObjective("quest_side_001", "obj_collect_herbs", 5));
            AddTestButton("Complete All Objectives (Main)", () => TestCompleteAllObjectives("quest_main_001"));
            
            var separator3 = new HSeparator();
            buttonContainer.AddChild(separator3);
            
            AddTestButton("Complete Quest (Side Quest)", () => TestCompleteQuest("quest_side_001"));
            AddTestButton("Fail Quest (Time-Sensitive)", () => TestFailQuest("quest_time_001"));
            
            var separator4 = new HSeparator();
            buttonContainer.AddChild(separator4);
            
            AddTestButton("Set Chapter 2", () => TestSetChapter(2));
            AddTestButton("Set Chapter 3", () => TestSetChapter(3));
            
            var separator5 = new HSeparator();
            buttonContainer.AddChild(separator5);
            
            AddTestButton("Simulate Item Collected", () => TestItemCollected("item_shadow_herb", 1));
            AddTestButton("Simulate Enemy Defeated", () => TestEnemyDefeated("enemy_slime"));
            AddTestButton("Simulate NPC Talked", () => TestNPCTalked("npc_marcus"));
            
            var separator6 = new HSeparator();
            buttonContainer.AddChild(separator6);
            
            AddTestButton("Show Active Quests", () => TestShowActiveQuests());
            AddTestButton("Show Quest Progress", () => TestShowQuestProgress("quest_main_001"));
            AddTestButton("Open Quest Journal", () => TestOpenJournal());
        }
        
        private void AddTestButton(string text, Action callback)
        {
            var button = new Button();
            button.Text = text;
            button.Pressed += callback;
            buttonContainer.AddChild(button);
        }
        
        private void ConnectQuestSignals()
        {
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.QuestStarted += (questId) => 
                    GD.Print($"[SIGNAL] Quest Started: {questId}");
                    
                QuestManager.Instance.QuestCompleted += (questId) => 
                    GD.Print($"[SIGNAL] Quest Completed: {questId}");
                    
                QuestManager.Instance.QuestObjectiveUpdated += (questId, objId, current, required) => 
                    GD.Print($"[SIGNAL] Objective Updated: {questId}/{objId} - {current}/{required}");
                    
                QuestManager.Instance.QuestFailed += (questId) => 
                    GD.Print($"[SIGNAL] Quest Failed: {questId}");
                    
                QuestManager.Instance.QuestExpired += (questId) => 
                    GD.Print($"[SIGNAL] Quest Expired: {questId}");
            }
        }
        
        // Test Methods
        
        private void TestStartQuest(string questId)
        {
            bool success = QuestManager.Instance?.StartQuest(questId) ?? false;
            var quest = QuestManager.Instance?.GetQuest(questId);
            
            if (success && quest != null)
            {
                UpdateStatus($"✓ Started: {quest.QuestName}");
            }
            else
            {
                UpdateStatus($"✗ Failed to start quest: {questId}");
            }
        }
        
        private void TestUpdateObjective(string questId, string objectiveId, int amount)
        {
            QuestManager.Instance?.UpdateObjective(questId, objectiveId, amount);
            
            var progress = QuestManager.Instance?.GetQuestProgress(questId);
            if (progress != null && progress.ContainsKey(objectiveId))
            {
                UpdateStatus($"✓ Updated objective: {objectiveId} = {progress[objectiveId]}");
            }
            else
            {
                UpdateStatus($"✗ Could not update objective");
            }
        }
        
        private void TestCompleteAllObjectives(string questId)
        {
            var quest = QuestManager.Instance?.GetQuest(questId);
            if (quest == null)
            {
                UpdateStatus($"✗ Quest not found: {questId}");
                return;
            }
            
            foreach (var objective in quest.Objectives)
            {
                QuestManager.Instance?.SetObjectiveProgress(questId, objective.ObjectiveId, objective.RequiredCount);
            }
            
            UpdateStatus($"✓ Completed all objectives for: {quest.QuestName}");
        }
        
        private void TestCompleteQuest(string questId)
        {
            bool success = QuestManager.Instance?.CompleteQuest(questId) ?? false;
            var quest = QuestManager.Instance?.GetQuest(questId);
            
            if (success && quest != null)
            {
                UpdateStatus($"✓ Completed: {quest.QuestName}");
            }
            else
            {
                UpdateStatus($"✗ Could not complete quest (objectives not finished?)");
            }
        }
        
        private void TestFailQuest(string questId)
        {
            QuestManager.Instance?.FailQuest(questId);
            UpdateStatus($"✓ Failed quest: {questId}");
        }
        
        private void TestSetChapter(int chapter)
        {
            QuestManager.Instance?.SetCurrentChapter(chapter);
            UpdateStatus($"✓ Set chapter to: {chapter}");
        }
        
        private void TestItemCollected(string itemId, int amount)
        {
            QuestManager.Instance?.OnItemCollected(itemId, amount);
            UpdateStatus($"✓ Item collected: {itemId} x{amount}");
        }
        
        private void TestEnemyDefeated(string enemyId)
        {
            QuestManager.Instance?.OnEnemyDefeated(enemyId);
            UpdateStatus($"✓ Enemy defeated: {enemyId}");
        }
        
        private void TestNPCTalked(string npcId)
        {
            QuestManager.Instance?.OnNPCTalked(npcId);
            UpdateStatus($"✓ Talked to NPC: {npcId}");
        }
        
        private void TestShowActiveQuests()
        {
            var activeQuests = QuestManager.Instance?.GetActiveQuests();
            
            if (activeQuests == null || activeQuests.Count == 0)
            {
                UpdateStatus("No active quests");
                return;
            }
            
            string output = $"Active Quests ({activeQuests.Count}):\n";
            foreach (var quest in activeQuests)
            {
                output += $"• {quest.QuestName} ({quest.Type})\n";
            }
            
            UpdateStatus(output);
            GD.Print(output);
        }
        
        private void TestShowQuestProgress(string questId)
        {
            var quest = QuestManager.Instance?.GetQuest(questId);
            var progress = QuestManager.Instance?.GetQuestProgress(questId);
            
            if (quest == null)
            {
                UpdateStatus($"Quest not found: {questId}");
                return;
            }
            
            if (progress == null)
            {
                UpdateStatus($"Quest not active: {questId}");
                return;
            }
            
            string output = $"{quest.QuestName} Progress:\n";
            foreach (var objective in quest.Objectives)
            {
                int current = progress.ContainsKey(objective.ObjectiveId) ? progress[objective.ObjectiveId] : 0;
                output += $"• {objective.Description}: {current}/{objective.RequiredCount}\n";
            }
            
            UpdateStatus(output);
            GD.Print(output);
        }
        
        private void TestOpenJournal()
        {
            var ui = GetTree().Root.GetNode<QuestUI>("%QuestUI");
            if (ui != null)
            {
                ui.OpenJournal();
                UpdateStatus("✓ Opened quest journal");
            }
            else
            {
                UpdateStatus("✗ QuestUI not found");
            }
        }
        
        private void UpdateStatus(string message)
        {
            if (statusLabel != null)
            {
                statusLabel.Text = $"Status: {message}";
            }
            GD.Print($"[TEST] {message}");
        }
        
        public override void _Input(InputEvent @event)
        {
            if (@event.IsActionPressed("ui_cancel"))
            {
                GetTree().Quit();
            }
        }
    }
}