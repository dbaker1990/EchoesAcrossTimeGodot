using Godot;
using System;

namespace EchoesAcrossTime.Quests
{
    public partial class QuestUIBuilder : Node
    {
        public override void _Ready()
        {
            BuildQuestUI();
            BuildQuestTracker();
            BuildNotificationManager();
        }
        
        private void BuildQuestUI()
        {
            var questUI = new Control();
            questUI.Name = "QuestUI";
            questUI.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            questUI.Visible = false;
            
            var canvas = new CanvasLayer();
            canvas.Layer = 10;
            questUI.AddChild(canvas);
            
            var panel = new Panel();
            panel.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            canvas.AddChild(panel);
            
            var margin = new MarginContainer();
            margin.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            margin.AddThemeConstantOverride("margin_left", 40);
            margin.AddThemeConstantOverride("margin_right", 40);
            margin.AddThemeConstantOverride("margin_top", 40);
            margin.AddThemeConstantOverride("margin_bottom", 40);
            panel.AddChild(margin);
            
            var vbox = new VBoxContainer();
            vbox.AddThemeConstantOverride("separation", 20);
            margin.AddChild(vbox);
            
            // Title
            var title = new Label();
            title.Text = "Quest Journal";
            title.HorizontalAlignment = HorizontalAlignment.Center;
            title.AddThemeFontSizeOverride("font_size", 32);
            vbox.AddChild(title);
            
            // Tab container
            var tabs = new TabContainer();
            tabs.Name = "QuestTabs";
            tabs.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            vbox.AddChild(tabs);
            
            // Main quests tab
            var mainTab = CreateQuestListTab("Main Quests", "MainQuestList");
            tabs.AddChild(mainTab);
            
            // Side quests tab
            var sideTab = CreateQuestListTab("Side Quests", "SideQuestList");
            tabs.AddChild(sideTab);
            
            // Completed tab
            var completedTab = CreateQuestListTab("Completed", "CompletedQuestList");
            tabs.AddChild(completedTab);
            
            // Details panel
            var detailsPanel = CreateDetailsPanel();
            detailsPanel.Name = "QuestDetailsPanel";
            vbox.AddChild(detailsPanel);
            
            // Attach script
            var script = GD.Load<Script>("res://Quests/QuestUI.cs");
            questUI.SetScript(script);
            
            GetTree().Root.AddChild(questUI);
            GD.Print("Quest UI created programmatically");
        }
        
        private Control CreateQuestListTab(string tabName, string listName)
        {
            var container = new VBoxContainer();
            container.Name = tabName;
            
            var scroll = new ScrollContainer();
            scroll.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            container.AddChild(scroll);
            
            var list = new VBoxContainer();
            list.Name = listName;
            list.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            scroll.AddChild(list);
            
            return container;
        }
        
        private Control CreateDetailsPanel()
        {
            var panel = new PanelContainer();
            panel.CustomMinimumSize = new Vector2(0, 300);
            
            var margin = new MarginContainer();
            margin.AddThemeConstantOverride("margin_left", 20);
            margin.AddThemeConstantOverride("margin_right", 20);
            margin.AddThemeConstantOverride("margin_top", 20);
            margin.AddThemeConstantOverride("margin_bottom", 20);
            panel.AddChild(margin);
            
            var vbox = new VBoxContainer();
            vbox.AddThemeConstantOverride("separation", 10);
            margin.AddChild(vbox);
            
            // Title
            var titleLabel = new Label();
            titleLabel.Name = "QuestTitleLabel";
            titleLabel.AddThemeFontSizeOverride("font_size", 24);
            vbox.AddChild(titleLabel);
            
            // Giver
            var giverLabel = new Label();
            giverLabel.Name = "QuestGiverLabel";
            vbox.AddChild(giverLabel);
            
            vbox.AddChild(new HSeparator());
            
            // Description
            var descLabel = new Label();
            descLabel.Name = "QuestDescriptionLabel";
            descLabel.AutowrapMode = TextServer.AutowrapMode.Word;
            vbox.AddChild(descLabel);
            
            vbox.AddChild(new HSeparator());
            
            // Objectives
            var objTitle = new Label();
            objTitle.Text = "Objectives:";
            objTitle.AddThemeFontSizeOverride("font_size", 18);
            vbox.AddChild(objTitle);
            
            var objContainer = new VBoxContainer();
            objContainer.Name = "ObjectivesContainer";
            vbox.AddChild(objContainer);
            
            vbox.AddChild(new HSeparator());
            
            // Rewards
            var rewardTitle = new Label();
            rewardTitle.Text = "Rewards:";
            rewardTitle.AddThemeFontSizeOverride("font_size", 18);
            vbox.AddChild(rewardTitle);
            
            var rewardContainer = new VBoxContainer();
            rewardContainer.Name = "RewardsContainer";
            vbox.AddChild(rewardContainer);
            
            // Buttons
            var btnBox = new HBoxContainer();
            vbox.AddChild(btnBox);
            
            var trackBtn = new Button();
            trackBtn.Name = "TrackButton";
            trackBtn.Text = "Track Quest";
            btnBox.AddChild(trackBtn);
            
            var abandonBtn = new Button();
            abandonBtn.Name = "AbandonButton";
            abandonBtn.Text = "Abandon Quest";
            btnBox.AddChild(abandonBtn);
            
            var closeBtn = new Button();
            closeBtn.Name = "CloseButton";
            closeBtn.Text = "Close";
            btnBox.AddChild(closeBtn);
            
            return panel;
        }
        
        private void BuildQuestTracker()
        {
            var tracker = new Control();
            tracker.Name = "QuestTrackerHUD";
            tracker.AnchorLeft = 1.0f;
            tracker.AnchorTop = 0.0f;
            tracker.AnchorRight = 1.0f;
            tracker.AnchorBottom = 0.0f;
            tracker.Position = new Vector2(-370, 20);
            tracker.CustomMinimumSize = new Vector2(350, 400);
            
            var panel = new PanelContainer();
            panel.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            tracker.AddChild(panel);
            
            var vbox = new VBoxContainer();
            vbox.AddThemeConstantOverride("separation", 10);
            panel.AddChild(vbox);
            
            // Header
            var header = new HBoxContainer();
            vbox.AddChild(header);
            
            var headerLabel = new Label();
            headerLabel.Name = "HeaderLabel";
            headerLabel.Text = "Active Quests";
            headerLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            header.AddChild(headerLabel);
            
            var minBtn = new Button();
            minBtn.Name = "MinimizeButton";
            minBtn.Text = "-";
            minBtn.CustomMinimumSize = new Vector2(30, 0);
            header.AddChild(minBtn);
            
            // Quest container
            var scroll = new ScrollContainer();
            scroll.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            vbox.AddChild(scroll);
            
            var questContainer = new VBoxContainer();
            questContainer.Name = "QuestContainer";
            questContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            scroll.AddChild(questContainer);
            
            // Attach script
            var script = GD.Load<Script>("res://Quests/QuestTrackerHUD.cs");
            tracker.SetScript(script);
            
            GetTree().Root.AddChild(tracker);
            GD.Print("Quest Tracker created programmatically");
        }
        
        private void BuildNotificationManager()
        {
            var notif = new Control();
            notif.Name = "QuestNotificationManager";
            notif.AnchorLeft = 0.5f;
            notif.AnchorTop = 0.0f;
            notif.AnchorRight = 0.5f;
            notif.AnchorBottom = 0.0f;
            notif.Position = new Vector2(-200, 20);
            notif.CustomMinimumSize = new Vector2(400, 100);
            
            // Attach script
            var script = GD.Load<Script>("res://Quests/QuestNotification.cs");
            notif.SetScript(script);
            
            GetTree().Root.AddChild(notif);
            GD.Print("Quest Notification Manager created programmatically");
        }
    }
}