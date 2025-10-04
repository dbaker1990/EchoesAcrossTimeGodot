using Godot;
using System.Collections.Generic;
using EchoesAcrossTime.Quests;

public partial class QuestUIComplete : CanvasLayer
{
	private Control mainPanel;
	private TabContainer tabs;
	private bool isOpen = false;
	
	public override void _Ready()
	{
		BuildCompleteUI();
		
		if (QuestManager.Instance != null)
		{
			QuestManager.Instance.QuestStarted += (_) => RefreshUI();
			QuestManager.Instance.QuestCompleted += (_) => RefreshUI();
			QuestManager.Instance.QuestObjectiveUpdated += (_,_,_,_) => RefreshUI();
		}
		
		GD.Print("Quest UI Complete ready!");
	}
	
	private void BuildCompleteUI()
	{
		Layer = 100;
		
		// Main panel
		mainPanel = new Panel();
		mainPanel.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		mainPanel.Hide();
		AddChild(mainPanel);
		
		// Margin
		var margin = new MarginContainer();
		margin.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		margin.AddThemeConstantOverride("margin_left", 100);
		margin.AddThemeConstantOverride("margin_right", 100);
		margin.AddThemeConstantOverride("margin_top", 80);
		margin.AddThemeConstantOverride("margin_bottom", 80);
		mainPanel.AddChild(margin);
		
		var vbox = new VBoxContainer();
		vbox.AddThemeConstantOverride("separation", 20);
		margin.AddChild(vbox);
		
		// Title
		var title = new Label();
		title.Text = "QUEST JOURNAL";
		title.HorizontalAlignment = HorizontalAlignment.Center;
		title.AddThemeFontSizeOverride("font_size", 36);
		vbox.AddChild(title);
		
		// Tabs
		tabs = new TabContainer();
		tabs.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
		vbox.AddChild(tabs);
		
		// Create tabs
		CreateTab("Main Quests", QuestType.MainQuest);
		CreateTab("Side Quests", QuestType.SideQuest);
		CreateTab("Completed", null);
		
		// Close button
		var closeBtn = new Button();
		closeBtn.Text = "Close (ESC)";
		closeBtn.Pressed += Close;
		vbox.AddChild(closeBtn);
		
		RefreshUI();
	}
	
	private void CreateTab(string name, QuestType? type)
	{
		var scroll = new ScrollContainer();
		scroll.Name = name;
		scroll.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
		
		var list = new VBoxContainer();
		list.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		scroll.AddChild(list);
		
		tabs.AddChild(scroll);
	}
	
	private void RefreshUI()
	{
		if (tabs == null) return;
		
		// Main quests
		RefreshTab(0, QuestManager.Instance?.GetActiveMainQuests());
		
		// Side quests
		RefreshTab(1, QuestManager.Instance?.GetActiveSideQuests());
		
		// Completed
		RefreshTab(2, QuestManager.Instance?.GetCompletedQuests());
	}
	
	private void RefreshTab(int tabIndex, List<QuestData> quests)
	{
		var scroll = tabs.GetChild<ScrollContainer>(tabIndex);
		var list = scroll.GetChild<VBoxContainer>(0);
		
		foreach (Node child in list.GetChildren())
			child.QueueFree();
		
		if (quests == null || quests.Count == 0)
		{
			var empty = new Label();
			empty.Text = "No quests";
			empty.HorizontalAlignment = HorizontalAlignment.Center;
			list.AddChild(empty);
			return;
		}
		
		foreach (var quest in quests)
		{
			var panel = new PanelContainer();
			var margin = new MarginContainer();
			margin.AddThemeConstantOverride("margin_left", 10);
			margin.AddThemeConstantOverride("margin_right", 10);
			margin.AddThemeConstantOverride("margin_top", 10);
			margin.AddThemeConstantOverride("margin_bottom", 10);
			panel.AddChild(margin);
			
			var vb = new VBoxContainer();
			margin.AddChild(vb);
			
			// Quest name
			var nameLabel = new Label();
			nameLabel.Text = quest.QuestName;
			nameLabel.AddThemeFontSizeOverride("font_size", 20);
			vb.AddChild(nameLabel);
			
			// Giver
			var giverLabel = new Label();
			giverLabel.Text = $"From: {quest.GiverName}";
			giverLabel.AddThemeFontSizeOverride("font_size", 12);
			vb.AddChild(giverLabel);
			
			vb.AddChild(new HSeparator());
			
			// Objectives
			var progress = QuestManager.Instance?.GetQuestProgress(quest.QuestId);
			foreach (var obj in quest.Objectives)
			{
				int current = progress?.ContainsKey(obj.ObjectiveId) == true ? progress[obj.ObjectiveId] : 0;
				bool done = current >= obj.RequiredCount;
				
				var objLabel = new Label();
				objLabel.Text = $"{(done ? "✓" : "○")} {obj.Description}";
				if (obj.RequiredCount > 1)
					objLabel.Text += $" ({current}/{obj.RequiredCount})";
				
				if (done)
					objLabel.AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.5f));
				
				vb.AddChild(objLabel);
			}
			
			// Rewards
			if (quest.Type == QuestType.SideQuest && !QuestManager.Instance.IsQuestCompleted(quest.QuestId))
			{
				vb.AddChild(new HSeparator());
				var rewardLabel = new Label();
				rewardLabel.Text = $"Rewards: {quest.GoldReward}G, {quest.ExpReward} EXP";
				rewardLabel.AddThemeFontSizeOverride("font_size", 12);
				vb.AddChild(rewardLabel);
			}
			
			list.AddChild(panel);
		}
	}
	
	public void Toggle()
	{
		if (isOpen) Close();
		else Open();
	}
	
	public void Open()
	{
		RefreshUI();
		mainPanel.Show();
		isOpen = true;
		GD.Print("Quest UI opened");
	}
	
	public void Close()
	{
		mainPanel.Hide();
		isOpen = false;
	}
	
	public override void _Input(InputEvent @event)
	{
		// Debug all key presses
		if (@event is InputEventKey key && key.Pressed)
		{
			GD.Print($"Key pressed: {key.Keycode}");
		}
	
		if (@event.IsActionPressed("ui_cancel") && isOpen)
		{
			Close();
			GetViewport().SetInputAsHandled();
		}
	
		// Use a different key - try J for Journal
		if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.J)
		{
			GD.Print("J key detected - toggling UI");
			Toggle();
			GetViewport().SetInputAsHandled();
		}
	}
}
