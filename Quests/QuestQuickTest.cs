using Godot;
using EchoesAcrossTime.Quests;

public partial class QuestQuickTest : Node
{
    private Label statusLabel;
    
    public override void _Ready()
    {
        GD.Print("=== QUEST QUICK TEST ===");
        
        // Create visible label
        var canvas = new CanvasLayer();
        AddChild(canvas);
        
        statusLabel = new Label();
        statusLabel.Position = new Vector2(50, 50);
        statusLabel.AddThemeFontSizeOverride("font_size", 20);
        statusLabel.Text = "Quest System Loading...";
        canvas.AddChild(statusLabel);
        
        // Register and test quests
        CallDeferred(nameof(TestQuests));
    }
    
    private void TestQuests()
    {
        GD.Print("Starting quest test...");
        
        // Register example quests
        ExampleQuests.RegisterAllExampleQuests();
        statusLabel.Text = "Quests Registered!\n\nPress SPACE to start quest\nPress ENTER to show active quests";
        
        // Connect to quest signals
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.QuestStarted += (id) => 
            {
                GD.Print($"QUEST STARTED: {id}");
                UpdateStatus();
            };
            
            QuestManager.Instance.QuestObjectiveUpdated += (qId, oId, current, required) =>
            {
                GD.Print($"OBJECTIVE UPDATED: {oId} = {current}/{required}");
                UpdateStatus();
            };
        }
    }
    
    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_accept")) // SPACE
        {
            GD.Print("Starting quest via SPACE...");
            bool success = QuestManager.Instance?.StartQuest("quest_main_001") ?? false;
            GD.Print($"Start quest result: {success}");
            UpdateStatus();
        }
        
        if (@event.IsActionPressed("ui_select")) // ENTER
        {
            GD.Print("Showing active quests...");
            UpdateStatus();
        }
        
        if (@event.IsActionPressed("ui_cancel")) // ESC
        {
            GD.Print("Updating objective...");
            QuestManager.Instance?.UpdateObjective("quest_main_001", "obj_visit_archives", 1);
        }
    }
    
    private void UpdateStatus()
    {
        var active = QuestManager.Instance?.GetActiveQuests();
        
        string text = $"QUEST SYSTEM TEST\n\n";
        text += $"Active Quests: {active?.Count ?? 0}\n\n";
        
        if (active != null && active.Count > 0)
        {
            foreach (var quest in active)
            {
                text += $"📜 {quest.QuestName}\n";
                
                var progress = QuestManager.Instance?.GetQuestProgress(quest.QuestId);
                foreach (var obj in quest.Objectives)
                {
                    int current = progress?.ContainsKey(obj.ObjectiveId) == true ? progress[obj.ObjectiveId] : 0;
                    text += $"  • {obj.Description}: {current}/{obj.RequiredCount}\n";
                }
                text += "\n";
            }
        }
        
        text += "\nControls:\n";
        text += "SPACE - Start Quest\n";
        text += "ENTER - Refresh Display\n";
        text += "ESC - Update Objective";
        
        statusLabel.Text = text;
    }
}