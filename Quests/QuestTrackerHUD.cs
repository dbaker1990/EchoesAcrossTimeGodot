using Godot;
using System.Collections.Generic;

namespace EchoesAcrossTime.Quests
{
    public partial class QuestTrackerHUD : Control
    {
        private VBoxContainer questContainer;
        private Label headerLabel;
        private Button minimizeButton;
        private bool isMinimized = false;
        
        public override void _Ready()
        {
            questContainer = FindChild("QuestContainer") as VBoxContainer;
            headerLabel = FindChild("HeaderLabel") as Label;
            minimizeButton = FindChild("MinimizeButton") as Button;
            
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.QuestStarted += (id) => RefreshTracker();
                QuestManager.Instance.QuestCompleted += (id) => RefreshTracker();
                QuestManager.Instance.QuestObjectiveUpdated += (id, obj, c, r) => RefreshTracker();
            }
            
            if (minimizeButton != null)
                minimizeButton.Pressed += ToggleMinimize;
            
            RefreshTracker();
            GD.Print("QuestTrackerHUD initialized");
        }
        
        private void RefreshTracker()
        {
            if (questContainer == null) return;
            
            foreach (Node child in questContainer.GetChildren())
                child.QueueFree();
            
            var activeQuests = QuestManager.Instance?.GetActiveQuests();
            if (activeQuests == null || activeQuests.Count == 0)
            {
                questContainer.Hide();
                return;
            }
            
            questContainer.Show();
            
            foreach (var quest in activeQuests)
            {
                if (questContainer.GetChildCount() >= 3) break;
                
                var label = new Label();
                label.Text = $"• {quest.QuestName}";
                questContainer.AddChild(label);
            }
        }
        
        private void ToggleMinimize()
        {
            isMinimized = !isMinimized;
            if (questContainer != null)
                questContainer.Visible = !isMinimized;
        }
    }
}