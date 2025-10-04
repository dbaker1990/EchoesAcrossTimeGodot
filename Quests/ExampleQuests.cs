// Quests/ExampleQuests.cs
using Godot;
using System;

namespace EchoesAcrossTime.Quests
{
    /// <summary>
    /// Example quest definitions for testing and reference
    /// </summary>
    public static class ExampleQuests
    {
        /// <summary>
        /// Main Quest Example: The Shadow Prince's Journey
        /// </summary>
        public static QuestData CreateMainQuest_ShadowPrince()
        {
            var quest = new QuestData
            {
                QuestId = "quest_main_001",
                QuestName = "The Shadow Prince's Journey",
                Description = "Prince Dominic must uncover the truth about the cosmic wound threatening both timelines. Begin by investigating the ancient archives.",
                Type = QuestType.MainQuest,
                GiverType = QuestGiverType.AutoReceived,
                GiverName = "Story Event",
                GiverId = "",
                IsTimeSensitive = false,
                MinimumChapter = 1,
                MaximumChapter = 999,
                TrackOnMap = true
            };
            
            // Objective 1: Visit the Archives
            var obj1 = new QuestObjective
            {
                ObjectiveId = "obj_visit_archives",
                Description = "Visit the Royal Archives",
                Type = QuestObjectiveType.ReachLocation,
                TargetId = "location_royal_archives",
                RequiredCount = 1,
                IsOptional = false
            };
            quest.Objectives.Add(obj1);
            
            // Objective 2: Talk to the Archivist
            var obj2 = new QuestObjective
            {
                ObjectiveId = "obj_talk_archivist",
                Description = "Speak with the Head Archivist",
                Type = QuestObjectiveType.TalkToNPC,
                TargetId = "npc_archivist",
                RequiredCount = 1,
                IsOptional = false
            };
            quest.Objectives.Add(obj2);
            
            // Objective 3: Find Ancient Texts
            var obj3 = new QuestObjective
            {
                ObjectiveId = "obj_find_texts",
                Description = "Locate 3 Ancient Texts about the Cosmic Cycle",
                Type = QuestObjectiveType.CollectItems,
                TargetId = "item_ancient_text",
                RequiredCount = 3,
                IsOptional = false
            };
            quest.Objectives.Add(obj3);
            
            return quest;
        }
        
        /// <summary>
        /// Side Quest Example: Help the Merchant
        /// </summary>
        public static QuestData CreateSideQuest_MerchantHelp()
        {
            var quest = new QuestData
            {
                QuestId = "quest_side_001",
                QuestName = "Merchant's Request",
                Description = "Merchant Marcus needs help gathering rare herbs from the Shadow Forest. The herbs are needed for healing potions.",
                Type = QuestType.SideQuest,
                GiverType = QuestGiverType.NPC,
                GiverName = "Merchant Marcus",
                GiverId = "npc_marcus",
                IsTimeSensitive = false,
                MinimumChapter = 1,
                MaximumChapter = 999,
                TrackOnMap = true,
                QuestLocation = new Vector2(500, 300)
            };
            
            // Objective: Collect herbs
            var obj1 = new QuestObjective
            {
                ObjectiveId = "obj_collect_herbs",
                Description = "Collect 10 Shadow Herbs",
                Type = QuestObjectiveType.CollectItems,
                TargetId = "item_shadow_herb",
                RequiredCount = 10,
                IsOptional = false
            };
            quest.Objectives.Add(obj1);
            
            // Objective: Return to Marcus
            var obj2 = new QuestObjective
            {
                ObjectiveId = "obj_return_marcus",
                Description = "Return to Merchant Marcus",
                Type = QuestObjectiveType.TalkToNPC,
                TargetId = "npc_marcus",
                RequiredCount = 1,
                IsOptional = false
            };
            quest.Objectives.Add(obj2);
            
            // Rewards
            quest.GoldReward = 500;
            quest.ExpReward = 250;
            
            var itemReward1 = new QuestItemReward
            {
                ItemId = "item_health_potion",
                Quantity = 5
            };
            quest.ItemRewards.Add(itemReward1);
            
            return quest;
        }
        
        /// <summary>
        /// Time-Sensitive Quest Example: Investigate the Disturbance
        /// </summary>
        public static QuestData CreateTimeSensitiveQuest()
        {
            var quest = new QuestData
            {
                QuestId = "quest_time_001",
                QuestName = "Urgent: Investigate the Disturbance",
                Description = "Strange energy readings have been detected near the palace. This must be investigated before the summit begins!",
                Type = QuestType.SideQuest,
                GiverType = QuestGiverType.NPC,
                GiverName = "Royal Guard Captain",
                GiverId = "npc_guard_captain",
                IsTimeSensitive = true,
                RequiredStoryFlag = "chapter_2_started",
                ExpiresAfterFlag = "summit_began",
                MinimumChapter = 2,
                MaximumChapter = 2,
                TrackOnMap = true,
                QuestLocation = new Vector2(800, 600)
            };
            
            var obj1 = new QuestObjective
            {
                ObjectiveId = "obj_investigate_site",
                Description = "Investigate the Energy Disturbance Site",
                Type = QuestObjectiveType.ReachLocation,
                TargetId = "location_disturbance",
                RequiredCount = 1,
                IsOptional = false
            };
            quest.Objectives.Add(obj1);
            
            var obj2 = new QuestObjective
            {
                ObjectiveId = "obj_defeat_shadows",
                Description = "Defeat the Shadow Creatures",
                Type = QuestObjectiveType.DefeatEnemies,
                TargetId = "enemy_shadow_creature",
                RequiredCount = 5,
                IsOptional = false
            };
            quest.Objectives.Add(obj2);
            
            // Better rewards for time-sensitive quest
            quest.GoldReward = 1000;
            quest.ExpReward = 500;
            
            var itemReward = new QuestItemReward
            {
                ItemId = "item_rare_crystal",
                Quantity = 1
            };
            quest.ItemRewards.Add(itemReward);
            
            return quest;
        }
        
        /// <summary>
        /// Quest Board Quest Example
        /// </summary>
        public static QuestData CreateQuestBoardQuest_Slimes()
        {
            var quest = new QuestData
            {
                QuestId = "quest_board_001",
                QuestName = "Slime Extermination",
                Description = "The local farm is being overrun by slimes! Help clear them out.",
                Type = QuestType.SideQuest,
                GiverType = QuestGiverType.QuestBoard,
                GiverName = "Guild Quest Board",
                GiverId = "quest_board_guild",
                IsTimeSensitive = false,
                MinimumChapter = 1,
                MaximumChapter = 999,
                TrackOnMap = true
            };
            
            var obj1 = new QuestObjective
            {
                ObjectiveId = "obj_defeat_slimes",
                Description = "Defeat 15 Slimes",
                Type = QuestObjectiveType.DefeatEnemies,
                TargetId = "enemy_slime",
                RequiredCount = 15,
                IsOptional = false
            };
            quest.Objectives.Add(obj1);
            
            quest.GoldReward = 300;
            quest.ExpReward = 150;
            
            return quest;
        }
        
        /// <summary>
        /// File/Document Quest Example
        /// </summary>
        public static QuestData CreateFileQuest_MysteriousLetter()
        {
            var quest = new QuestData
            {
                QuestId = "quest_file_001",
                QuestName = "The Mysterious Letter",
                Description = "A strange letter was found in the abandoned mansion. It mentions a hidden treasure in the catacombs...",
                Type = QuestType.SideQuest,
                GiverType = QuestGiverType.ReadFile,
                GiverName = "Old Letter",
                GiverId = "",
                IsTimeSensitive = false,
                MinimumChapter = 1,
                MaximumChapter = 999,
                TrackOnMap = true
            };
            
            var obj1 = new QuestObjective
            {
                ObjectiveId = "obj_find_catacombs",
                Description = "Find the Hidden Catacombs",
                Type = QuestObjectiveType.ReachLocation,
                TargetId = "location_catacombs",
                RequiredCount = 1,
                IsOptional = false
            };
            quest.Objectives.Add(obj1);
            
            var obj2 = new QuestObjective
            {
                ObjectiveId = "obj_find_treasure",
                Description = "Locate the Hidden Treasure",
                Type = QuestObjectiveType.Custom,
                TargetId = "treasure_ancient_chest",
                RequiredCount = 1,
                IsOptional = false
            };
            quest.Objectives.Add(obj2);
            
            quest.GoldReward = 2000;
            quest.ExpReward = 800;
            
            var itemReward = new QuestItemReward
            {
                ItemId = "item_ancient_artifact",
                Quantity = 1
            };
            quest.ItemRewards.Add(itemReward);
            
            return quest;
        }
        
        /// <summary>
        /// Quest with Optional Objectives
        /// </summary>
        public static QuestData CreateQuestWithOptionalObjectives()
        {
            var quest = new QuestData
            {
                QuestId = "quest_optional_001",
                QuestName = "The Lost Expedition",
                Description = "Search for survivors of the lost expedition in the northern mountains.",
                Type = QuestType.SideQuest,
                GiverType = QuestGiverType.NPC,
                GiverName = "Explorer's Guild Leader",
                GiverId = "npc_explorer_leader",
                IsTimeSensitive = false,
                MinimumChapter = 3,
                MaximumChapter = 999
            };
            
            // Required objective
            var obj1 = new QuestObjective
            {
                ObjectiveId = "obj_find_camp",
                Description = "Find the Expedition Camp",
                Type = QuestObjectiveType.ReachLocation,
                TargetId = "location_expedition_camp",
                RequiredCount = 1,
                IsOptional = false
            };
            quest.Objectives.Add(obj1);
            
            // Required objective
            var obj2 = new QuestObjective
            {
                ObjectiveId = "obj_find_leader",
                Description = "Locate the Expedition Leader",
                Type = QuestObjectiveType.Custom,
                TargetId = "expedition_leader_found",
                RequiredCount = 1,
                IsOptional = false
            };
            quest.Objectives.Add(obj2);
            
            // Optional objective - bonus reward
            var obj3 = new QuestObjective
            {
                ObjectiveId = "obj_find_supplies",
                Description = "Recover Lost Expedition Supplies",
                Type = QuestObjectiveType.CollectItems,
                TargetId = "item_expedition_supplies",
                RequiredCount = 5,
                IsOptional = true
            };
            quest.Objectives.Add(obj3);
            
            quest.GoldReward = 1500;
            quest.ExpReward = 600;
            
            var itemReward = new QuestItemReward
            {
                ItemId = "item_explorer_badge",
                Quantity = 1
            };
            quest.ItemRewards.Add(itemReward);
            
            return quest;
        }
        
        /// <summary>
        /// Register all example quests
        /// </summary>
        public static void RegisterAllExampleQuests()
        {
            if (QuestManager.Instance == null)
            {
                GD.PrintErr("QuestManager not initialized!");
                return;
            }
            
            QuestManager.Instance.RegisterQuest(CreateMainQuest_ShadowPrince());
            QuestManager.Instance.RegisterQuest(CreateSideQuest_MerchantHelp());
            QuestManager.Instance.RegisterQuest(CreateTimeSensitiveQuest());
            QuestManager.Instance.RegisterQuest(CreateQuestBoardQuest_Slimes());
            QuestManager.Instance.RegisterQuest(CreateFileQuest_MysteriousLetter());
            QuestManager.Instance.RegisterQuest(CreateQuestWithOptionalObjectives());
            
            GD.Print("All example quests registered!");
        }
    }
}