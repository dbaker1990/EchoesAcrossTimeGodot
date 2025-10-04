// Quests/QuestData.cs
using Godot;
using System;
using System.Collections.Generic;

namespace EchoesAcrossTime.Quests
{
    /// <summary>
    /// Defines a quest with objectives, rewards, and conditions
    /// </summary>
    [GlobalClass]
    public partial class QuestData : Resource
    {
        [ExportGroup("Basic Info")]
        [Export] public string QuestId { get; set; } = "quest_001";
        [Export] public string QuestName { get; set; } = "New Quest";
        [Export(PropertyHint.MultilineText)] public string Description { get; set; } = "";
        [Export] public QuestType Type { get; set; } = QuestType.SideQuest;
        
        [ExportGroup("Quest Giver")]
        [Export] public QuestGiverType GiverType { get; set; } = QuestGiverType.NPC;
        [Export] public string GiverName { get; set; } = ""; // NPC name, file name, or "Quest Board"
        [Export] public string GiverId { get; set; } = ""; // NPC ID for reference
        
        [ExportGroup("Objectives")]
        [Export] public Godot.Collections.Array<QuestObjective> Objectives { get; set; }
        
        [ExportGroup("Rewards (Side Quests Only)")]
        [Export] public int GoldReward { get; set; } = 0;
        [Export] public int ExpReward { get; set; } = 0;
        [Export] public Godot.Collections.Array<QuestItemReward> ItemRewards { get; set; }
        
        [ExportGroup("Conditions")]
        [Export] public bool IsTimeSensitive { get; set; } = false;
        [Export] public string RequiredStoryFlag { get; set; } = ""; // Must have this flag
        [Export] public string ExpiresAfterFlag { get; set; } = ""; // Quest unavailable after this flag
        [Export] public int MinimumChapter { get; set; } = 1;
        [Export] public int MaximumChapter { get; set; } = 999; // Quest expires after this chapter
        
        [ExportGroup("Tracking")]
        [Export] public bool TrackOnMap { get; set; } = true; // Show quest marker on map
        [Export] public Vector2 QuestLocation { get; set; } = Vector2.Zero;
        
        public QuestData()
        {
            Objectives = new Godot.Collections.Array<QuestObjective>();
            ItemRewards = new Godot.Collections.Array<QuestItemReward>();
        }
        
        /// <summary>
        /// Check if quest is available based on current game state
        /// </summary>
        public bool IsAvailable(int currentChapter, Dictionary<string, bool> switches)
        {
            // Check chapter range
            if (currentChapter < MinimumChapter || currentChapter > MaximumChapter)
                return false;
            
            // Check required flag
            if (!string.IsNullOrEmpty(RequiredStoryFlag))
            {
                if (!switches.GetValueOrDefault(RequiredStoryFlag, false))
                    return false;
            }
            
            // Check expiration flag
            if (!string.IsNullOrEmpty(ExpiresAfterFlag))
            {
                if (switches.GetValueOrDefault(ExpiresAfterFlag, false))
                    return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Check if all objectives are complete
        /// </summary>
        public bool AreAllObjectivesComplete(Dictionary<string, int> objectiveProgress)
        {
            foreach (var objective in Objectives)
            {
                int current = objectiveProgress.GetValueOrDefault(objective.ObjectiveId, 0);
                if (current < objective.RequiredCount)
                    return false;
            }
            return true;
        }
    }
    
    /// <summary>
    /// Individual quest objective
    /// </summary>
    [GlobalClass]
    public partial class QuestObjective : Resource
    {
        [Export] public string ObjectiveId { get; set; } = "obj_001";
        [Export(PropertyHint.MultilineText)] public string Description { get; set; } = "";
        [Export] public QuestObjectiveType Type { get; set; } = QuestObjectiveType.CollectItems;
        [Export] public string TargetId { get; set; } = ""; // Item ID, enemy ID, NPC ID, location ID
        [Export] public int RequiredCount { get; set; } = 1;
        [Export] public bool IsOptional { get; set; } = false;
    }
    
    /// <summary>
    /// Item reward for completing quest
    /// </summary>
    [GlobalClass]
    public partial class QuestItemReward : Resource
    {
        [Export] public string ItemId { get; set; } = "";
        [Export] public int Quantity { get; set; } = 1;
    }
    
    public enum QuestType
    {
        MainQuest,
        SideQuest
    }
    
    public enum QuestGiverType
    {
        NPC,
        QuestBoard,
        ReadFile,
        AutoReceived // Story progression
    }
    
    public enum QuestObjectiveType
    {
        CollectItems,
        DefeatEnemies,
        TalkToNPC,
        ReachLocation,
        UseItem,
        Custom // For special objectives tracked via events
    }
}