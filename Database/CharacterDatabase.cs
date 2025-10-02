using System.Collections.Generic;
using Godot;
using Godot.Collections;
using System.Linq;

namespace EchoesAcrossTime.Database
{
    public partial class CharacterDatabase
    {
        public List<CharacterData> Characters { get; set; } = new();
        
        private bool isInitialized = false;
        
        // Rest of your methods stay the same...
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
        
        private System.Collections.Generic.Dictionary<string, CharacterData> characterLookup;
        
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
    }
}