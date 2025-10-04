// Quests/QuestManager.cs
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EchoesAcrossTime.Quests
{
    /// <summary>
    /// Manages all active and completed quests
    /// </summary>
    public partial class QuestManager : Node
    {
        public static QuestManager Instance { get; private set; }
        
        // Quest database
        private Dictionary<string, QuestData> allQuests = new Dictionary<string, QuestData>();
        
        // Active quest tracking
        private List<string> activeQuestIds = new List<string>();
        private List<string> completedQuestIds = new List<string>();
        private Dictionary<string, Dictionary<string, int>> questObjectiveProgress = 
            new Dictionary<string, Dictionary<string, int>>();
        
        // Current game state for availability checking
        private int currentChapter = 1;
        
        [Signal]
        public delegate void QuestStartedEventHandler(string questId);
        
        [Signal]
        public delegate void QuestCompletedEventHandler(string questId);
        
        [Signal]
        public delegate void QuestObjectiveUpdatedEventHandler(string questId, string objectiveId, int current, int required);
        
        [Signal]
        public delegate void QuestFailedEventHandler(string questId);
        
        [Signal]
        public delegate void QuestExpiredEventHandler(string questId);
        
        public override void _Ready()
        {
            if (Instance != null)
            {
                QueueFree();
                return;
            }
            Instance = this;
            GD.Print("QuestManager initialized");
        }
        
        #region Quest Registration
        
        /// <summary>
        /// Register a quest to the database
        /// </summary>
        public void RegisterQuest(QuestData quest)
        {
            if (quest == null || string.IsNullOrEmpty(quest.QuestId))
            {
                GD.PrintErr("QuestManager: Cannot register null or invalid quest");
                return;
            }
            
            allQuests[quest.QuestId] = quest;
            GD.Print($"Registered quest: {quest.QuestId} - {quest.QuestName}");
        }
        
        /// <summary>
        /// Load all quests from a folder
        /// </summary>
        public void LoadQuestsFromFolder(string folderPath)
        {
            var dir = DirAccess.Open(folderPath);
            if (dir != null)
            {
                dir.ListDirBegin();
                string fileName = dir.GetNext();
                
                while (!string.IsNullOrEmpty(fileName))
                {
                    if (!dir.CurrentIsDir() && fileName.EndsWith(".tres"))
                    {
                        var quest = GD.Load<QuestData>($"{folderPath}/{fileName}");
                        if (quest != null)
                        {
                            RegisterQuest(quest);
                        }
                    }
                    fileName = dir.GetNext();
                }
                dir.ListDirEnd();
                GD.Print($"Loaded quests from {folderPath}");
            }
        }
        
        #endregion
        
        #region Quest Lifecycle
        
        /// <summary>
        /// Start a quest
        /// </summary>
        public bool StartQuest(string questId)
        {
            if (!allQuests.ContainsKey(questId))
            {
                GD.PrintErr($"QuestManager: Quest {questId} not found");
                return false;
            }
            
            if (activeQuestIds.Contains(questId))
            {
                GD.Print($"Quest {questId} is already active");
                return false;
            }
            
            if (completedQuestIds.Contains(questId))
            {
                GD.Print($"Quest {questId} is already completed");
                return false;
            }
            
            var quest = allQuests[questId];
            
            // Check availability
            var switches = GetCurrentSwitches();
            if (!quest.IsAvailable(currentChapter, switches))
            {
                GD.Print($"Quest {questId} is not available at this time");
                return false;
            }
            
            // Initialize quest
            activeQuestIds.Add(questId);
            questObjectiveProgress[questId] = new Dictionary<string, int>();
            
            // Initialize all objectives to 0
            foreach (var objective in quest.Objectives)
            {
                questObjectiveProgress[questId][objective.ObjectiveId] = 0;
            }
            
            EmitSignal(SignalName.QuestStarted, questId);
            GD.Print($"Started quest: {quest.QuestName}");
            
            return true;
        }
        
        /// <summary>
        /// Complete a quest and give rewards
        /// </summary>
        public bool CompleteQuest(string questId)
        {
            if (!activeQuestIds.Contains(questId))
            {
                GD.PrintErr($"Quest {questId} is not active");
                return false;
            }
            
            var quest = allQuests[questId];
            
            // Check if all objectives are complete
            if (!quest.AreAllObjectivesComplete(questObjectiveProgress[questId]))
            {
                GD.Print($"Cannot complete {questId}: objectives not finished");
                return false;
            }
            
            // Move to completed
            activeQuestIds.Remove(questId);
            completedQuestIds.Add(questId);
            
            // Give rewards (for side quests)
            if (quest.Type == QuestType.SideQuest)
            {
                GiveQuestRewards(quest);
            }
            
            EmitSignal(SignalName.QuestCompleted, questId);
            GD.Print($"Completed quest: {quest.QuestName}");
            
            return true;
        }
        
        /// <summary>
        /// Fail/abandon a quest
        /// </summary>
        public void FailQuest(string questId)
        {
            if (activeQuestIds.Contains(questId))
            {
                activeQuestIds.Remove(questId);
                questObjectiveProgress.Remove(questId);
                EmitSignal(SignalName.QuestFailed, questId);
                GD.Print($"Failed quest: {questId}");
            }
        }
        
        #endregion
        
        #region Objective Tracking
        
        /// <summary>
        /// Update objective progress
        /// </summary>
        public void UpdateObjective(string questId, string objectiveId, int amount = 1)
        {
            if (!activeQuestIds.Contains(questId))
                return;
            
            if (!questObjectiveProgress.ContainsKey(questId))
                return;
            
            if (!questObjectiveProgress[questId].ContainsKey(objectiveId))
                return;
            
            var quest = allQuests[questId];
            var objective = quest.Objectives.FirstOrDefault(o => o.ObjectiveId == objectiveId);
            
            if (objective == null)
                return;
            
            // Update progress
            questObjectiveProgress[questId][objectiveId] += amount;
            int current = questObjectiveProgress[questId][objectiveId];
            
            // Clamp to required count
            if (current > objective.RequiredCount)
            {
                questObjectiveProgress[questId][objectiveId] = objective.RequiredCount;
                current = objective.RequiredCount;
            }
            
            EmitSignal(SignalName.QuestObjectiveUpdated, questId, objectiveId, current, objective.RequiredCount);
            
            // Check if quest is complete
            if (quest.AreAllObjectivesComplete(questObjectiveProgress[questId]))
            {
                GD.Print($"All objectives complete for {quest.QuestName}!");
            }
        }
        
        /// <summary>
        /// Set objective progress directly
        /// </summary>
        public void SetObjectiveProgress(string questId, string objectiveId, int value)
        {
            if (!activeQuestIds.Contains(questId))
                return;
            
            if (!questObjectiveProgress.ContainsKey(questId))
                return;
            
            var quest = allQuests[questId];
            var objective = quest.Objectives.FirstOrDefault(o => o.ObjectiveId == objectiveId);
            
            if (objective == null)
                return;
            
            questObjectiveProgress[questId][objectiveId] = Mathf.Clamp(value, 0, objective.RequiredCount);
            int current = questObjectiveProgress[questId][objectiveId];
            
            EmitSignal(SignalName.QuestObjectiveUpdated, questId, objectiveId, current, objective.RequiredCount);
        }
        
        /// <summary>
        /// Convenience methods for common objective types
        /// </summary>
        public void OnItemCollected(string itemId, int amount)
        {
            foreach (var questId in activeQuestIds)
            {
                var quest = allQuests[questId];
                foreach (var objective in quest.Objectives)
                {
                    if (objective.Type == QuestObjectiveType.CollectItems && objective.TargetId == itemId)
                    {
                        UpdateObjective(questId, objective.ObjectiveId, amount);
                    }
                }
            }
        }
        
        public void OnEnemyDefeated(string enemyId)
        {
            foreach (var questId in activeQuestIds)
            {
                var quest = allQuests[questId];
                foreach (var objective in quest.Objectives)
                {
                    if (objective.Type == QuestObjectiveType.DefeatEnemies && objective.TargetId == enemyId)
                    {
                        UpdateObjective(questId, objective.ObjectiveId, 1);
                    }
                }
            }
        }
        
        public void OnNPCTalked(string npcId)
        {
            foreach (var questId in activeQuestIds)
            {
                var quest = allQuests[questId];
                foreach (var objective in quest.Objectives)
                {
                    if (objective.Type == QuestObjectiveType.TalkToNPC && objective.TargetId == npcId)
                    {
                        UpdateObjective(questId, objective.ObjectiveId, 1);
                    }
                }
            }
        }
        
        public void OnLocationReached(string locationId)
        {
            foreach (var questId in activeQuestIds)
            {
                var quest = allQuests[questId];
                foreach (var objective in quest.Objectives)
                {
                    if (objective.Type == QuestObjectiveType.ReachLocation && objective.TargetId == locationId)
                    {
                        UpdateObjective(questId, objective.ObjectiveId, 1);
                    }
                }
            }
        }
        
        #endregion
        
        #region Quest Queries
        
        public List<QuestData> GetActiveQuests()
        {
            return activeQuestIds.Select(id => allQuests[id]).ToList();
        }
        
        public List<QuestData> GetActiveMainQuests()
        {
            return activeQuestIds
                .Select(id => allQuests[id])
                .Where(q => q.Type == QuestType.MainQuest)
                .ToList();
        }
        
        public List<QuestData> GetActiveSideQuests()
        {
            return activeQuestIds
                .Select(id => allQuests[id])
                .Where(q => q.Type == QuestType.SideQuest)
                .ToList();
        }
        
        public List<QuestData> GetCompletedQuests()
        {
            return completedQuestIds.Select(id => allQuests[id]).ToList();
        }
        
        public QuestData GetQuest(string questId)
        {
            return allQuests.GetValueOrDefault(questId);
        }
        
        public bool IsQuestActive(string questId)
        {
            return activeQuestIds.Contains(questId);
        }
        
        public bool IsQuestCompleted(string questId)
        {
            return completedQuestIds.Contains(questId);
        }
        
        public Dictionary<string, int> GetQuestProgress(string questId)
        {
            return questObjectiveProgress.GetValueOrDefault(questId);
        }
        
        #endregion
        
        #region Time-Sensitive Quests
        
        /// <summary>
        /// Update current chapter - checks for expired quests
        /// </summary>
        public void SetCurrentChapter(int chapter)
        {
            currentChapter = chapter;
            CheckExpiredQuests();
        }
        
        /// <summary>
        /// Check and fail any time-sensitive quests that expired
        /// </summary>
        private void CheckExpiredQuests()
        {
            var switches = GetCurrentSwitches();
            var expiredQuests = new List<string>();
            
            foreach (var questId in activeQuestIds)
            {
                var quest = allQuests[questId];
                if (quest.IsTimeSensitive && !quest.IsAvailable(currentChapter, switches))
                {
                    expiredQuests.Add(questId);
                }
            }
            
            foreach (var questId in expiredQuests)
            {
                activeQuestIds.Remove(questId);
                questObjectiveProgress.Remove(questId);
                EmitSignal(SignalName.QuestExpired, questId);
                GD.Print($"Quest expired: {allQuests[questId].QuestName}");
            }
        }
        
        #endregion
        
        #region Rewards
        
        private void GiveQuestRewards(QuestData quest)
        {
            // Gold reward
            if (quest.GoldReward > 0)
            {
                Items.InventorySystem.Instance?.AddGold(quest.GoldReward);
                GD.Print($"Received {quest.GoldReward} gold");
            }
            
            // Experience reward
            if (quest.ExpReward > 0)
            {
                // Distribute to party if PartyMenuManager exists
                if (PartyMenuManager.Instance != null)
                {
                    PartyMenuManager.Instance.DistributeExperience(quest.ExpReward);
                }
                GD.Print($"Received {quest.ExpReward} experience");
            }
            
            // Item rewards
            foreach (var itemReward in quest.ItemRewards)
            {
                var item = GameManager.Instance?.Database?.GetItem(itemReward.ItemId);
                if (item != null)
                {
                    Items.InventorySystem.Instance?.AddItem(item, itemReward.Quantity);
                    GD.Print($"Received {itemReward.Quantity}x {item.DisplayName}");
                }
            }
        }
        
        #endregion
        
        #region Save/Load Integration
        
        public QuestSaveData GetSaveData()
        {
            return new QuestSaveData
            {
                ActiveQuestIds = new List<string>(activeQuestIds),
                CompletedQuestIds = new List<string>(completedQuestIds),
                ObjectiveProgress = new Dictionary<string, Dictionary<string, int>>(questObjectiveProgress),
                CurrentChapter = currentChapter
            };
        }
        
        public void LoadSaveData(QuestSaveData data)
        {
            if (data == null) return;
            
            activeQuestIds = new List<string>(data.ActiveQuestIds);
            completedQuestIds = new List<string>(data.CompletedQuestIds);
            questObjectiveProgress = new Dictionary<string, Dictionary<string, int>>(data.ObjectiveProgress);
            currentChapter = data.CurrentChapter;
            
            GD.Print("Quest data loaded from save");
        }
        
        #endregion
        
        private Dictionary<string, bool> GetCurrentSwitches()
        {
            // Get switches from event system
            var switches = new Dictionary<string, bool>();
            
            if (GameManager.Instance?.CurrentSave != null)
            {
                foreach (var kvp in GameManager.Instance.CurrentSave.Switches)
                {
                    switches[kvp.Key] = kvp.Value;
                }
            }
            
            return switches;
        }
    }
    
    /// <summary>
    /// Quest save data structure
    /// </summary>
    [Serializable]
    public class QuestSaveData
    {
        public List<string> ActiveQuestIds { get; set; } = new List<string>();
        public List<string> CompletedQuestIds { get; set; } = new List<string>();
        public Dictionary<string, Dictionary<string, int>> ObjectiveProgress { get; set; } = 
            new Dictionary<string, Dictionary<string, int>>();
        public int CurrentChapter { get; set; } = 1;
    }
}