// SaveSystem/SaveData.cs
using Godot;
using System;
using System.Collections.Generic;
using EchoesAcrossTime.Database;
using EchoesAcrossTime.Combat;
using EchoesAcrossTime.Items;

namespace EchoesAcrossTime
{
    /// <summary>
    /// Complete save data structure
    /// </summary>
    [Serializable]
    public partial class SaveData
    {
        // Meta information
        public string SaveSlotName { get; set; } = "Save 1";
        public int SaveSlotIndex { get; set; } = 0;
        public DateTime SaveDateTime { get; set; }
        public string GameVersion { get; set; } = "1.0.0";
        public float PlayTimeSeconds { get; set; } = 0f;
        
        // Player state
        public PlayerSaveData Player { get; set; } = new PlayerSaveData();
        
        // Party data
        public List<PartyMemberSaveData> Party { get; set; } = new List<PartyMemberSaveData>();
        
        // Inventory
        public InventorySaveData Inventory { get; set; } = new InventorySaveData();
        
        // Map and position
        public string CurrentMapPath { get; set; } = "";
        public Vector2Data PlayerPosition { get; set; } = new Vector2Data();
        public int PlayerDirection { get; set; } = 0;
        
        // Variables and switches
        public Dictionary<string, object> Variables { get; set; } = new Dictionary<string, object>();
        public Dictionary<string, bool> Switches { get; set; } = new Dictionary<string, bool>();
        
        // Event state
        public List<string> CompletedEvents { get; set; } = new List<string>();
        
        // Screenshot for save file
        public byte[] ScreenshotData { get; set; }
        
        public SaveData()
        {
            SaveDateTime = DateTime.Now;
        }
        
        /// <summary>
        /// Initialize a new game save
        /// </summary>
        public void InitializeNewGame(GameDatabase database)
        {
            // Initialize player
            Player = new PlayerSaveData
            {
                CharacterId = "dominic"
            };
            
            // Add starting party member
            var dominicData = database.GetCharacter("dominic");
            if (dominicData != null)
            {
                var dominicStats = dominicData.CreateStatsInstance();
                Party.Add(PartyMemberSaveData.FromCharacterStats(dominicStats));
            }
            
            // Starting inventory
            Inventory.Gold = 1000;
            
            // Starting position
            CurrentMapPath = "res://Maps/StartingMap.tscn";
            PlayerPosition = new Vector2Data(100, 100);
            
            GD.Print("New game initialized");
        }
        
        /// <summary>
        /// Apply this save data to the current game state
        /// </summary>
        public void ApplyToGame()
        {
            // Apply inventory
            Inventory.ApplyToInventorySystem();
            
            // Apply party
            foreach (var memberData in Party)
            {
                memberData.ApplyToParty();
            }
            
            // Apply variables/switches
            if (Events.EventCommandExecutor.Instance != null)
            {
                foreach (var kvp in Variables)
                {
                    Events.EventCommandExecutor.Instance.SetVariable(kvp.Key, kvp.Value);
                }
            }
            
            GD.Print("Save data applied to game");
        }
        
        /// <summary>
        /// Capture current game state
        /// </summary>
        public void CaptureCurrentState()
        {
            SaveDateTime = DateTime.Now;
            
            // Capture player position
            var player = GameManager.Instance?.GetTree()?.GetFirstNodeInGroup("player") as PlayerCharacter;
            if (player != null)
            {
                PlayerPosition = new Vector2Data(player.GlobalPosition);
                PlayerDirection = (int)player.CurrentDirection;
            }
            
            // Capture current map
            var currentScene = GameManager.Instance?.GetTree()?.CurrentScene;
            if (currentScene != null)
            {
                CurrentMapPath = currentScene.SceneFilePath;
            }
            
            // Capture inventory
            Inventory.CaptureFromInventorySystem();
            
            // Capture party - FIXED
            Party.Clear();
            if (PartyManager.Instance != null)
            {
                // Get active followers
                var followers = PartyManager.Instance.GetActiveFollowerNames();
                // You'll need to implement getting their stats
                // For now, just capturing that they exist
                GD.Print($"Capturing {followers.Count} party members");
            }
            
            // Capture variables
            if (Events.EventCommandExecutor.Instance != null)
            {
                // You'll need to expose variables from EventCommandExecutor
            }
            
            GD.Print("Game state captured");
        }
    }
    
    #region Nested Save Data Classes
    
    [Serializable]
    public class Vector2Data
    {
        public float X { get; set; }
        public float Y { get; set; }
        
        public Vector2Data() { }
        
        public Vector2Data(Vector2 vec)
        {
            X = vec.X;
            Y = vec.Y;
        }
        
        public Vector2Data(float x, float y)
        {
            X = x;
            Y = y;
        }
        
        public Vector2 ToVector2() => new Vector2(X, Y);
    }
    
    [Serializable]
    public class PlayerSaveData
    {
        public string CharacterId { get; set; }
        public bool IsVisible { get; set; } = true;
    }
    
    [Serializable]
    public class PartyMemberSaveData
    {
        public string CharacterId { get; set; }
        public int Level { get; set; }
        public int CurrentHP { get; set; }
        public int MaxHP { get; set; }
        public int CurrentMP { get; set; }
        public int MaxMP { get; set; }
        public int CurrentExp { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public int MagicAttack { get; set; }
        public int MagicDefense { get; set; }
        public int Speed { get; set; }
        
        public List<string> LearnedSkillIds { get; set; } = new List<string>();
        public List<string> EquippedSkillIds { get; set; } = new List<string>();
        
        public Dictionary<int, string> EquippedItems { get; set; } = new Dictionary<int, string>();
        
        public static PartyMemberSaveData FromCharacterStats(CharacterStats stats)
        {
            var data = new PartyMemberSaveData
            {
                CharacterId = stats.CharacterName, // You may need to store ID separately
                Level = stats.Level,
                CurrentHP = stats.CurrentHP,
                MaxHP = stats.MaxHP,
                CurrentMP = stats.CurrentMP,
                MaxMP = stats.MaxMP,
                CurrentExp = stats.CurrentExp,
                Attack = stats.Attack,
                Defense = stats.Defense,
                MagicAttack = stats.MagicAttack,
                MagicDefense = stats.MagicDefense,
                Speed = stats.Speed
            };
            
            // Save learned skills
            if (stats.Skills != null)
            {
                foreach (var skill in stats.Skills.GetLearnedSkills())
                {
                    data.LearnedSkillIds.Add(skill.SkillId);
                }
                
                foreach (var skill in stats.Skills.GetEquippedSkills())
                {
                    data.EquippedSkillIds.Add(skill.SkillId);
                }
            }
            
            return data;
        }
        
        public CharacterStats ToCharacterStats(GameDatabase database)
        {
            var characterData = database.GetCharacter(CharacterId);
            if (characterData == null)
            {
                GD.PrintErr($"Character {CharacterId} not found in database");
                return null;
            }
            
            var stats = new CharacterStats
            {
                CharacterName = characterData.DisplayName,
                Level = this.Level,
                CurrentHP = this.CurrentHP,
                MaxHP = this.MaxHP,
                CurrentMP = this.CurrentMP,
                MaxMP = this.MaxMP,
                CurrentExp = this.CurrentExp,
                Attack = this.Attack,
                Defense = this.Defense,
                MagicAttack = this.MagicAttack,
                MagicDefense = this.MagicDefense,
                Speed = this.Speed
            };
            
            // Restore skills
            stats.Skills = new CharacterSkills(CharacterId);
            foreach (var skillId in LearnedSkillIds)
            {
                var skill = database.GetSkill(skillId);
                if (skill != null)
                {
                    stats.Skills.LearnSkill(skill);
                }
            }
            
            foreach (var skillId in EquippedSkillIds)
            {
                var skill = database.GetSkill(skillId);
                if (skill != null)
                {
                    stats.Skills.EquipSkill(skill);
                }
            }
            
            return stats;
        }
        
        public void ApplyToParty()
        {
            // Apply this member to the actual party
            var stats = ToCharacterStats(GameManager.Instance.Database);
            // You'll need to implement adding to actual party
        }
    }
    
    [Serializable]
    public class InventorySaveData
    {
        public int Gold { get; set; }
        public Dictionary<string, int> Items { get; set; } = new Dictionary<string, int>();
        
        public void CaptureFromInventorySystem()
        {
            // FIXED - Use correct namespace
            if (InventorySystem.Instance == null) return;
            
            Gold = InventorySystem.Instance.GetGold();
            Items.Clear();
            
            foreach (var slot in InventorySystem.Instance.GetAllItems())
            {
                Items[slot.Item.ItemId] = slot.Quantity;
            }
        }
        
        public void ApplyToInventorySystem()
        {
            // FIXED - Use correct namespace
            if (InventorySystem.Instance == null) return;
            
            // Clear current inventory
            InventorySystem.Instance.ClearInventory();
            
            // Set gold
            var currentGold = InventorySystem.Instance.GetGold();
            if (Gold > currentGold)
            {
                InventorySystem.Instance.AddGold(Gold - currentGold);
            }
            else if (Gold < currentGold)
            {
                InventorySystem.Instance.RemoveGold(currentGold - Gold);
            }
            
            // Add items
            foreach (var kvp in Items)
            {
                var item = GameManager.Instance.Database.GetItem(kvp.Key);
                if (item != null)
                {
                    InventorySystem.Instance.AddItem(item, kvp.Value);
                }
            }
        }
    }
    
    #endregion
}