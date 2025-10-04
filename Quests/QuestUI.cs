using Godot;
using System.Collections.Generic;

namespace EchoesAcrossTime.Quests
{
    public partial class QuestUI : Control
    {
        private TabContainer tabContainer;
        private VBoxContainer mainQuestList;
        private VBoxContainer sideQuestList;
        private VBoxContainer completedQuestList;
        private Panel questDetailsPanel;
        private Label questTitleLabel;
        private Label questGiverLabel;
        private Label questDescriptionLabel;
        private VBoxContainer objectivesContainer;
        private VBoxContainer rewardsContainer;
        private Button trackButton;
        private Button abandonButton;
        private Button closeButton;
        
        private QuestData selectedQuest;
        
        public override void _Ready()
        {
            // Auto-find all child nodes
            FindNodes();
            
            // Connect signals
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.QuestStarted += OnQuestStarted;
                QuestManager.Instance.QuestCompleted += OnQuestCompleted;
                QuestManager.Instance.QuestObjectiveUpdated += OnObjectiveUpdated;
            }
            
            if (closeButton != null)
                closeButton.Pressed += CloseJournal;
            
            Hide();
            GD.Print("QuestUI initialized");
        }
        
        private void FindNodes()
        {
            // Find nodes by name
            tabContainer = FindChild("QuestTabs") as TabContainer;
            mainQuestList = FindChild("MainQuestList") as VBoxContainer;
            sideQuestList = FindChild("SideQuestList") as VBoxContainer;
            completedQuestList = FindChild("CompletedQuestList") as VBoxContainer;
            questDetailsPanel = FindChild("QuestDetailsPanel") as Panel;
            questTitleLabel = FindChild("QuestTitleLabel") as Label;
            questGiverLabel = FindChild("QuestGiverLabel") as Label;
            questDescriptionLabel = FindChild("QuestDescriptionLabel") as Label;
            objectivesContainer = FindChild("ObjectivesContainer") as VBoxContainer;
            rewardsContainer = FindChild("RewardsContainer") as VBoxContainer;
            trackButton = FindChild("TrackButton") as Button;
            abandonButton = FindChild("AbandonButton") as Button;
            closeButton = FindChild("CloseButton") as Button;
            
            GD.Print($"Found nodes: tabs={tabContainer != null}, mainList={mainQuestList != null}");
        }
        
        public void OpenJournal()
        {
            RefreshQuestLists();
            Show();
        }
        
        public void CloseJournal()
        {
            Hide();
        }
        
        private void RefreshQuestLists()
        {
            if (mainQuestList != null)
            {
                ClearContainer(mainQuestList);
                foreach (var quest in QuestManager.Instance.GetActiveMainQuests())
                {
                    AddSimpleQuestItem(quest, mainQuestList);
                }
            }
            
            if (sideQuestList != null)
            {
                ClearContainer(sideQuestList);
                foreach (var quest in QuestManager.Instance.GetActiveSideQuests())
                {
                    AddSimpleQuestItem(quest, sideQuestList);
                }
            }
            
            if (completedQuestList != null)
            {
                ClearContainer(completedQuestList);
                foreach (var quest in QuestManager.Instance.GetCompletedQuests())
                {
                    AddSimpleQuestItem(quest, completedQuestList);
                }
            }
        }
        
        private void AddSimpleQuestItem(QuestData quest, VBoxContainer container)
        {
            var button = new Button();
            button.Text = quest.QuestName;
            button.Pressed += () => DisplayQuestDetails(quest);
            container.AddChild(button);
        }
        
        private void DisplayQuestDetails(QuestData quest)
        {
            if (questTitleLabel != null)
                questTitleLabel.Text = quest.QuestName;
            
            if (questGiverLabel != null)
                questGiverLabel.Text = $"Given by: {quest.GiverName}";
            
            if (questDescriptionLabel != null)
                questDescriptionLabel.Text = quest.Description;
            
            if (questDetailsPanel != null)
                questDetailsPanel.Show();
        }
        
        private void OnQuestStarted(string questId)
        {
            RefreshQuestLists();
        }
        
        private void OnQuestCompleted(string questId)
        {
            RefreshQuestLists();
        }
        
        private void OnObjectiveUpdated(string questId, string objId, int current, int required)
        {
            RefreshQuestLists();
        }
        
        private void ClearContainer(VBoxContainer container)
        {
            if (container == null) return;
            foreach (Node child in container.GetChildren())
            {
                child.QueueFree();
            }
        }
        
        public override void _Input(InputEvent @event)
        {
            if (@event.IsActionPressed("ui_cancel") && Visible)
            {
                CloseJournal();
                GetViewport().SetInputAsHandled();
            }
        }
    }
}