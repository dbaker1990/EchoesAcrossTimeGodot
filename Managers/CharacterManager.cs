// Managers/CharacterManager.cs
using Godot;
using System.Collections.Generic;
using EchoesAcrossTime.Database;
using EchoesAcrossTime.Combat;

namespace EchoesAcrossTime
{
    /// <summary>
    /// Manages character instances in the game world
    /// </summary>
    public partial class CharacterManager : Node
    {
        public static CharacterManager Instance { get; private set; }
        
        [ExportGroup("Templates")]
        [Export] public PackedScene GenericCharacterScene { get; set; }
        [Export] public PackedScene GenericFollowerScene { get; set; }
        [Export] public PackedScene BattleCharacterScene { get; set; }
        
        private Dictionary<string, OverworldCharacter> activeCharacters = new Dictionary<string, OverworldCharacter>();
        
        public override void _Ready()
        {
            if (Instance != null)
            {
                QueueFree();
                return;
            }
            
            Instance = this;
            
            // Load default scenes if not assigned
            if (GenericCharacterScene == null)
            {
                GenericCharacterScene = GD.Load<PackedScene>("res://Characters/GenericCharacter.tscn");
            }
            
            if (GenericFollowerScene == null)
            {
                GenericFollowerScene = GD.Load<PackedScene>("res://Characters/GenericFollower.tscn");
            }
            
            GD.Print("CharacterManager initialized");
        }
        
        /// <summary>
        /// Spawn a character in the world
        /// </summary>
        public OverworldCharacter SpawnCharacter(string characterId, Vector2 position, Node parent = null)
        {
            var characterData = GameManager.Instance?.Database?.GetCharacter(characterId);
            
            if (characterData == null)
            {
                GD.PrintErr($"CharacterManager: Character '{characterId}' not found in database");
                return null;
            }
            
            var character = characterData.CreateOverworldInstance(GenericCharacterScene);
            
            if (parent != null)
            {
                parent.AddChild(character);
            }
            else
            {
                GetTree().CurrentScene.AddChild(character);
            }
            
            character.GlobalPosition = position;
            activeCharacters[characterId] = character;
            
            GD.Print($"Spawned character: {characterData.DisplayName}");
            return character;
        }
        
        /// <summary>
        /// Spawn a follower character
        /// </summary>
        public FollowerCharacter SpawnFollower(string characterId, Vector2 position, Node parent = null)
        {
            var characterData = GameManager.Instance?.Database?.GetCharacter(characterId);
            
            if (characterData == null)
            {
                GD.PrintErr($"CharacterManager: Character '{characterId}' not found in database");
                return null;
            }
            
            var follower = characterData.CreateFollowerInstance(GenericFollowerScene);
            
            if (parent != null)
            {
                parent.AddChild(follower);
            }
            else
            {
                GetTree().CurrentScene.AddChild(follower);
            }
            
            follower.GlobalPosition = position;
            activeCharacters[characterId] = follower;
            
            GD.Print($"Spawned follower: {characterData.DisplayName}");
            return follower;
        }
        
        /// <summary>
        /// Get active character instance
        /// </summary>
        public OverworldCharacter GetCharacter(string characterId)
        {
            return activeCharacters.GetValueOrDefault(characterId);
        }
        
        /// <summary>
        /// Remove character from world
        /// </summary>
        public void RemoveCharacter(string characterId)
        {
            if (activeCharacters.TryGetValue(characterId, out var character))
            {
                character.QueueFree();
                activeCharacters.Remove(characterId);
            }
        }
        
        /// <summary>
        /// Add character to party and make them follow
        /// </summary>
        public void AddToParty(string characterId)
        {
            var follower = GetCharacter(characterId) as FollowerCharacter;
            
            if (follower != null && PartyManager.Instance != null)
            {
                PartyManager.Instance.AddFollower(follower);
            }
        }
    }
}