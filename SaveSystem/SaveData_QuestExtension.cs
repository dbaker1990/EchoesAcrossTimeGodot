// SaveSystem/SaveData_QuestExtension.cs
using System;
using System.Collections.Generic;
using EchoesAcrossTime.Quests;

namespace EchoesAcrossTime
{
    /// <summary>
    /// Extension to SaveData to include quest data
    /// Add this to your existing SaveData.cs file
    /// </summary>
    public partial class SaveData
    {
        // Add this property to your SaveData class
        public QuestSaveData Quests { get; set; } = new QuestSaveData();
        
        // Update InitializeNewGame to initialize quests
        public void InitializeNewGameWithQuests(Database.GameDatabase database)
        {
            // ... your existing initialization code ...
            
            // Initialize empty quest data
            Quests = new QuestSaveData
            {
                ActiveQuestIds = new List<string>(),
                CompletedQuestIds = new List<string>(),
                ObjectiveProgress = new Dictionary<string, Dictionary<string, int>>(),
                CurrentChapter = 1
            };
        }
        
        // Update ApplyToGame to load quest data
        public void ApplyToGameWithQuests()
        {
            // ... your existing apply code ...
            
            // Apply quest data
            if (Quests != null && QuestManager.Instance != null)
            {
                QuestManager.Instance.LoadSaveData(Quests);
            }
        }
        
        // Update CaptureCurrentState to save quest data
        public void CaptureCurrentStateWithQuests()
        {
            // ... your existing capture code ...
            
            // Capture quest data
            if (QuestManager.Instance != null)
            {
                Quests = QuestManager.Instance.GetSaveData();
            }
        }
    }
}

// Add to your SaveSystem.cs for convenience methods
namespace EchoesAcrossTime
{
    public partial class SaveSystem
    {
        /// <summary>
        /// Update save with current quest data before saving
        /// Call this in your SaveGame method
        /// </summary>
        private void UpdateQuestDataBeforeSave(SaveData saveData)
        {
            if (QuestManager.Instance != null)
            {
                saveData.Quests = QuestManager.Instance.GetSaveData();
            }
        }
        
        /// <summary>
        /// Load quest data after loading a save
        /// Call this in your LoadGame method
        /// </summary>
        private void LoadQuestDataAfterLoad(SaveData saveData)
        {
            if (saveData.Quests != null && QuestManager.Instance != null)
            {
                QuestManager.Instance.LoadSaveData(saveData.Quests);
            }
        }
    }
}