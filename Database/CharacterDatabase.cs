using System.Collections.Generic;
using Godot;
using Godot.Collections;
using System.Linq;
using EchoesAcrossTime.Combat;
using EchoesAcrossTime.Items;

namespace EchoesAcrossTime.Database
{
    [GlobalClass]
    public partial class CharacterDatabase : Resource
    {
        [ExportGroup("Characters")]
        [Export] public Godot.Collections.Array<CharacterData> Characters { get; set; }
        
        [ExportGroup("Combat")]
        [Export] public Godot.Collections.Array<SkillData> Skills { get; set; }
        
        [ExportGroup("Items")]
        [Export] public Godot.Collections.Array<ItemData> AllItems { get; set; }
        
        private bool isInitialized = false;
        private System.Collections.Generic.Dictionary<string, CharacterData> characterLookup;
        
        public CharacterDatabase()
        {
            Characters = new Godot.Collections.Array<CharacterData>();
            Skills = new Godot.Collections.Array<SkillData>();
            AllItems = new Godot.Collections.Array<ItemData>();
        }
        
        #region Character Methods
        
        public void Initialize()
        {
            if (isInitialized) return;
            
            characterLookup = new System.Collections.Generic.Dictionary<string, CharacterData>();
            
            if (Characters == null || Characters.Count == 0)
            {
                GD.PrintErr("CharacterDatabase: No characters in database");
                isInitialized = true;
                return;
            }
            
            int validCount = 0;
            foreach (var character in Characters)
            {
                if (character == null)
                {
                    GD.PrintErr("CharacterDatabase: Null character entry found");
                    continue;
                }
                
                if (!character.IsValid())
                {
                    continue;
                }
                
                if (characterLookup.ContainsKey(character.CharacterId))
                {
                    GD.PrintErr($"CharacterDatabase: Duplicate character ID '{character.CharacterId}'");
                    continue;
                }
                
                characterLookup[character.CharacterId] = character;
                validCount++;
            }
            
            isInitialized = true;
            GD.Print($"CharacterDatabase initialized with {validCount} characters");
        }
        
        public CharacterData GetCharacter(string characterId)
        {
            if (!isInitialized)
            {
                Initialize();
            }
            
            if (string.IsNullOrEmpty(characterId))
            {
                GD.PrintErr("CharacterDatabase: Cannot get character with empty ID");
                return null;
            }
            
            if (characterLookup.TryGetValue(characterId, out var character))
            {
                return character;
            }
            
            GD.PrintErr($"CharacterDatabase: Character '{characterId}' not found");
            return null;
        }
        
        public System.Collections.Generic.List<CharacterData> GetCharactersByType(CharacterType type)
        {
            if (!isInitialized)
            {
                Initialize();
            }
            
            return characterLookup.Values.Where(c => c.Type == type).ToList();
        }
        
        public System.Collections.Generic.List<CharacterData> GetPlayableCharacters()
        {
            return GetCharactersByType(CharacterType.PlayableCharacter);
        }
        
        public System.Collections.Generic.List<CharacterData> GetEnemies()
        {
            return GetCharactersByType(CharacterType.Enemy);
        }
        
        public System.Collections.Generic.List<CharacterData> GetBosses()
        {
            if (!isInitialized)
            {
                Initialize();
            }
            
            return characterLookup.Values.Where(c => c.IsBoss).ToList();
        }
        
        public System.Collections.Generic.List<CharacterData> GetNPCs()
        {
            return GetCharactersByType(CharacterType.NPC);
        }
        
        public bool HasCharacter(string characterId)
        {
            if (!isInitialized)
            {
                Initialize();
            }
            
            return characterLookup.ContainsKey(characterId);
        }
        
        public int GetCharacterCount()
        {
            if (!isInitialized)
            {
                Initialize();
            }
            
            return characterLookup.Count;
        }
        
        public System.Collections.Generic.List<string> GetAllCharacterIds()
        {
            if (!isInitialized)
            {
                Initialize();
            }
            
            return characterLookup.Keys.ToList();
        }
        
        public bool ValidateDatabase()
        {
            if (Characters == null || Characters.Count == 0)
            {
                GD.PrintErr("CharacterDatabase: Database is empty");
                return false;
            }
            
            bool allValid = true;
            foreach (var character in Characters)
            {
                if (character == null || !character.IsValid())
                {
                    allValid = false;
                }
            }
            
            return allValid;
        }
        
        #endregion
        
        #region Skill Methods
        
        /// <summary>
        /// Get a skill by ID
        /// </summary>
        public SkillData GetSkill(string skillId)
        {
            if (Skills == null || Skills.Count == 0)
            {
                return null;
            }
            
            return Skills.FirstOrDefault(s => s?.SkillId == skillId);
        }
        
        /// <summary>
        /// Get all skills
        /// </summary>
        public System.Collections.Generic.List<SkillData> GetAllSkills()
        {
            var skillList = new System.Collections.Generic.List<SkillData>();
            if (Skills != null)
            {
                foreach (var skill in Skills)
                {
                    if (skill != null)
                        skillList.Add(skill);
                }
            }
            return skillList;
        }
        
        /// <summary>
        /// Check if a skill exists
        /// </summary>
        public bool HasSkill(string skillId)
        {
            return GetSkill(skillId) != null;
        }
        
        #endregion
        
        #region Item Methods
        
        /// <summary>
        /// Get an item by ID
        /// </summary>
        public ItemData GetItem(string itemId)
        {
            if (AllItems == null || AllItems.Count == 0)
            {
                return null;
            }
            
            return AllItems.FirstOrDefault(i => i?.ItemId == itemId);
        }
        
        /// <summary>
        /// Get all items
        /// </summary>
        public System.Collections.Generic.List<ItemData> GetAllItems()
        {
            var itemList = new System.Collections.Generic.List<ItemData>();
            if (AllItems != null)
            {
                foreach (var item in AllItems)
                {
                    if (item != null)
                        itemList.Add(item);
                }
            }
            return itemList;
        }
        
        /// <summary>
        /// Check if an item exists
        /// </summary>
        public bool HasItem(string itemId)
        {
            return GetItem(itemId) != null;
        }
        
        #endregion
    }
}