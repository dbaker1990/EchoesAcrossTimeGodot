using Godot;
using System.Collections.Generic;
using EchoesAcrossTime.Combat;

namespace EchoesAcrossTime.Database
{
    /// <summary>
    /// Simple manager that uses preset characters
    /// </summary>
    public partial class DatabaseManager : Node
    {
        public static DatabaseManager Instance { get; private set; }
        
        private Dictionary<string, CharacterData> characters = new();
        
        public override void _Ready()
        {
            if (Instance != null)
            {
                QueueFree();
                return;
            }
            
            Instance = this;
            InitializeCharacters();
        }
        
        private void InitializeCharacters()
        {
            // Add all playable characters
            AddCharacter(CharacterPresets.CreateDominic(1));
            AddCharacter(CharacterPresets.CreateEchoWalker(1));
            AddCharacter(CharacterPresets.CreateShadowsMirror(1));
            
            // Add some enemies
            AddCharacter(CharacterPresets.CreateBasicEnemy("Slime", 1));
            AddCharacter(CharacterPresets.CreateBasicEnemy("Goblin", 3));
            AddCharacter(CharacterPresets.CreateBasicEnemy("Dark Wolf", 5));
            
            // Add a boss
            AddCharacter(CharacterPresets.CreateBoss("Shadow Twin", 10));
            
            GD.Print($"DatabaseManager initialized with {characters.Count} characters");
        }
        
        private void AddCharacter(CharacterData characterData)
        {
            if (characterData != null && !string.IsNullOrEmpty(characterData.CharacterId))
            {
                characters[characterData.CharacterId] = characterData;
            }
        }
        
        public CharacterData GetCharacter(string characterId)
        {
            if (characters.TryGetValue(characterId, out var character))
            {
                return character;
            }
            
            GD.PrintErr($"Character '{characterId}' not found");
            return null;
        }
        
        public CharacterStats CreateCharacterStats(string characterId, int level = -1)
        {
            var characterData = GetCharacter(characterId);
            
            if (characterData == null)
                return null;
            
            if (level > 0)
            {
                return characterData.CreateStatsInstanceAtLevel(level);
            }
            
            return characterData.CreateStatsInstance();
        }
        
        public List<CharacterData> GetAllCharacters()
        {
            return new List<CharacterData>(characters.Values);
        }
        
        public List<CharacterData> GetPlayableCharacters()
        {
            var playable = new List<CharacterData>();
            foreach (var character in characters.Values)
            {
                if (character.Type == CharacterType.PlayableCharacter)
                {
                    playable.Add(character);
                }
            }
            return playable;
        }
        
        public List<CharacterData> GetEnemies()
        {
            var enemies = new List<CharacterData>();
            foreach (var character in characters.Values)
            {
                if (character.Type == CharacterType.Enemy)
                {
                    enemies.Add(character);
                }
            }
            return enemies;
        }
    }
}