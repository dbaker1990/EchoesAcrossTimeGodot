// Quests/QuestEventCommands.cs
using Godot;
using System;
using System.Threading.Tasks;

namespace EchoesAcrossTime.Quests
{
    /// <summary>
    /// Start a quest via event command
    /// </summary>
    [GlobalClass]
    public partial class StartQuestCommand : Events.EventCommand
    {
        [Export] public string QuestId { get; set; } = "";
        [Export] public bool ShowNotification { get; set; } = true;
        
        public override async Task Execute(Events.EventCommandExecutor executor)
        {
            if (string.IsNullOrEmpty(QuestId))
            {
                GD.PrintErr("StartQuestCommand: No quest ID specified");
                return;
            }
            
            bool success = QuestManager.Instance?.StartQuest(QuestId) ?? false;
            
            if (success && ShowNotification)
            {
                var quest = QuestManager.Instance?.GetQuest(QuestId);
                if (quest != null)
                {
                    GD.Print($"New Quest: {quest.QuestName}");
                }
            }
            
            await Task.CompletedTask;
        }
    }
    
    /// <summary>
    /// Complete a quest via event command
    /// </summary>
    [GlobalClass]
    public partial class CompleteQuestCommand : Events.EventCommand
    {
        [Export] public string QuestId { get; set; } = "";
        [Export] public bool ShowRewards { get; set; } = true;
        
        public override async Task Execute(Events.EventCommandExecutor executor)
        {
            if (string.IsNullOrEmpty(QuestId))
            {
                GD.PrintErr("CompleteQuestCommand: No quest ID specified");
                return;
            }
            
            var quest = QuestManager.Instance?.GetQuest(QuestId);
            if (quest == null)
            {
                GD.PrintErr($"CompleteQuestCommand: Quest {QuestId} not found");
                return;
            }
            
            bool success = QuestManager.Instance?.CompleteQuest(QuestId) ?? false;
            
            if (success && ShowRewards)
            {
                string rewardText = $"Quest Complete: {quest.QuestName}";
                
                if (quest.Type == QuestType.SideQuest)
                {
                    if (quest.GoldReward > 0)
                        rewardText += $"\n+{quest.GoldReward} Gold";
                    if (quest.ExpReward > 0)
                        rewardText += $"\n+{quest.ExpReward} EXP";
                }
                
                GD.Print(rewardText);
            }
            
            await Task.CompletedTask;
        }
    }
    
    /// <summary>
    /// Update a quest objective via event command
    /// </summary>
    [GlobalClass]
    public partial class UpdateQuestObjectiveCommand : Events.EventCommand
    {
        [Export] public string QuestId { get; set; } = "";
        [Export] public string ObjectiveId { get; set; } = "";
        [Export] public int Amount { get; set; } = 1;
        [Export] public bool SetDirectly { get; set; } = false;
        
        public UpdateQuestObjectiveCommand()
        {
            WaitForCompletion = false;
        }
        
        public override async Task Execute(Events.EventCommandExecutor executor)
        {
            if (string.IsNullOrEmpty(QuestId) || string.IsNullOrEmpty(ObjectiveId))
            {
                GD.PrintErr("UpdateQuestObjectiveCommand: Quest ID or Objective ID not specified");
                return;
            }
            
            if (SetDirectly)
            {
                QuestManager.Instance?.SetObjectiveProgress(QuestId, ObjectiveId, Amount);
            }
            else
            {
                QuestManager.Instance?.UpdateObjective(QuestId, ObjectiveId, Amount);
            }
            
            await Task.CompletedTask;
        }
    }
    
    /// <summary>
    /// Fail/abandon a quest via event command
    /// </summary>
    [GlobalClass]
    public partial class FailQuestCommand : Events.EventCommand
    {
        [Export] public string QuestId { get; set; } = "";
        [Export] public bool ShowMessage { get; set; } = true;
        
        public override async Task Execute(Events.EventCommandExecutor executor)
        {
            if (string.IsNullOrEmpty(QuestId))
            {
                GD.PrintErr("FailQuestCommand: No quest ID specified");
                return;
            }
            
            var quest = QuestManager.Instance?.GetQuest(QuestId);
            QuestManager.Instance?.FailQuest(QuestId);
            
            if (ShowMessage && quest != null)
            {
                GD.Print($"Quest Failed: {quest.QuestName}");
            }
            
            await Task.CompletedTask;
        }
    }
    
    /// <summary>
    /// Check if quest is active/completed - sets a variable
    /// </summary>
    [GlobalClass]
    public partial class CheckQuestStatusCommand : Events.EventCommand
    {
        [Export] public string QuestId { get; set; } = "";
        [Export] public string VariableName { get; set; } = "quest_status";
        [Export] public QuestStatusCheck CheckType { get; set; } = QuestStatusCheck.IsActive;
        
        public CheckQuestStatusCommand()
        {
            WaitForCompletion = false;
        }
        
        public override async Task Execute(Events.EventCommandExecutor executor)
        {
            if (string.IsNullOrEmpty(QuestId))
            {
                GD.PrintErr("CheckQuestStatusCommand: No quest ID specified");
                return;
            }
            
            bool result = false;
            
            switch (CheckType)
            {
                case QuestStatusCheck.IsActive:
                    result = QuestManager.Instance?.IsQuestActive(QuestId) ?? false;
                    break;
                case QuestStatusCheck.IsCompleted:
                    result = QuestManager.Instance?.IsQuestCompleted(QuestId) ?? false;
                    break;
                case QuestStatusCheck.ObjectivesComplete:
                    var quest = QuestManager.Instance?.GetQuest(QuestId);
                    var progress = QuestManager.Instance?.GetQuestProgress(QuestId);
                    if (quest != null && progress != null)
                    {
                        result = quest.AreAllObjectivesComplete(progress);
                    }
                    break;
            }
            
            executor.SetVariable(VariableName, result);
            await Task.CompletedTask;
        }
    }
    
    /// <summary>
    /// Set the current chapter for time-sensitive quests
    /// </summary>
    [GlobalClass]
    public partial class SetChapterCommand : Events.EventCommand
    {
        [Export] public int Chapter { get; set; } = 1;
        
        public SetChapterCommand()
        {
            WaitForCompletion = false;
        }
        
        public override async Task Execute(Events.EventCommandExecutor executor)
        {
            QuestManager.Instance?.SetCurrentChapter(Chapter);
            executor.SetVariable("current_chapter", Chapter);
            
            GD.Print($"Chapter set to: {Chapter}");
            await Task.CompletedTask;
        }
    }
    
    public enum QuestStatusCheck
    {
        IsActive,
        IsCompleted,
        ObjectivesComplete
    }
    
    /// <summary>
    /// Conditional branch based on quest status
    /// </summary>
    [GlobalClass]
    public partial class ConditionalQuestCommand : Events.EventCommand
    {
        [Export] public string QuestId { get; set; } = "";
        [Export] public QuestStatusCheck Condition { get; set; } = QuestStatusCheck.IsActive;
        [Export] public Godot.Collections.Array<Events.EventCommand> TrueCommands { get; set; }
        [Export] public Godot.Collections.Array<Events.EventCommand> FalseCommands { get; set; }
        
        public ConditionalQuestCommand()
        {
            TrueCommands = new Godot.Collections.Array<Events.EventCommand>();
            FalseCommands = new Godot.Collections.Array<Events.EventCommand>();
        }
        
        public override async Task Execute(Events.EventCommandExecutor executor)
        {
            bool condition = false;
            
            switch (Condition)
            {
                case QuestStatusCheck.IsActive:
                    condition = QuestManager.Instance?.IsQuestActive(QuestId) ?? false;
                    break;
                case QuestStatusCheck.IsCompleted:
                    condition = QuestManager.Instance?.IsQuestCompleted(QuestId) ?? false;
                    break;
                case QuestStatusCheck.ObjectivesComplete:
                    var quest = QuestManager.Instance?.GetQuest(QuestId);
                    var progress = QuestManager.Instance?.GetQuestProgress(QuestId);
                    if (quest != null && progress != null)
                    {
                        condition = quest.AreAllObjectivesComplete(progress);
                    }
                    break;
            }
            
            var commandsToExecute = condition ? TrueCommands : FalseCommands;
            
            foreach (var command in commandsToExecute)
            {
                if (command != null)
                {
                    await command.Execute(executor);
                }
            }
        }
    }
}