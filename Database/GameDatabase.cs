// Database/GameDatabase.cs
using System.Linq;
using EchoesAcrossTime.Combat;
using EchoesAcrossTime.Database;
using EchoesAcrossTime.Events;
using EchoesAcrossTime.Items;
using Godot;
using Godot.Collections;

namespace EchoesAcrossTime.Database
{
    /// <summary>
    /// Main game database - holds all game data
    /// Create this as a Resource in Godot and assign to autoload
    /// </summary>
    [GlobalClass]
    public partial class GameDatabase : Resource
    {
        [ExportGroup("Characters")]
        [Export] public Array<CharacterData> PlayableCharacters { get; set; }
        [Export] public Array<CharacterData> Enemies { get; set; }
        [Export] public Array<CharacterData> Bosses { get; set; }
        
        [ExportGroup("Combat")]
        [Export] public Array<SkillData> Skills { get; set; }
        
        [ExportGroup("Items")]
        [Export] public Array<ItemData> AllItems { get; set; }
        [Export] public Array<ConsumableData> Consumables { get; set; }
        [Export] public Array<EquipmentData> Equipment { get; set; }
        
        [ExportGroup("Events")]
        [Export] public Array<EventPage> CommonEvents { get; set; }
        [Export] public Dictionary DialogueTables { get; set; }
        
        public GameDatabase()
        {
            PlayableCharacters = new Array<CharacterData>();
            Enemies = new Array<CharacterData>();
            Bosses = new Array<CharacterData>();
            Skills = new Array<SkillData>();
            AllItems = new Array<ItemData>();
            Consumables = new Array<ConsumableData>();
            Equipment = new Array<EquipmentData>();
            CommonEvents = new Array<EventPage>();
            DialogueTables = new Dictionary();
        }
        
        #region Character Lookup
        public CharacterData GetCharacter(string id)
        {
            foreach (var character in PlayableCharacters)
            {
                if (character?.CharacterId == id) return character;
            }
            foreach (var enemy in Enemies)
            {
                if (enemy?.CharacterId == id) return enemy;
            }
            foreach (var boss in Bosses)
            {
                if (boss?.CharacterId == id) return boss;
            }
            return null;
        }
        
        public System.Collections.Generic.List<CharacterData> GetAllPlayableCharacters()
        {
            var list = new System.Collections.Generic.List<CharacterData>();
            foreach (var character in PlayableCharacters)
            {
                if (character != null) list.Add(character);
            }
            return list;
        }
        #endregion
        
        #region Skill Lookup
        public SkillData GetSkill(string id)
        {
            return Skills.FirstOrDefault(s => s?.SkillId == id);
        }
        #endregion
        
        #region Item Lookup
        public ItemData GetItem(string id)
        {
            return AllItems.FirstOrDefault(i => i?.ItemId == id);
        }
        
        public ConsumableData GetConsumable(string id)
        {
            return Consumables.FirstOrDefault(c => c?.ItemId == id);
        }
        
        public EquipmentData GetEquipment(string id)
        {
            return Equipment.FirstOrDefault(e => e?.ItemId == id);
        }
        #endregion
        
        #region Event Lookup
        public EventPage GetCommonEvent(string id)
        {
            return CommonEvents.FirstOrDefault(e => e?.EventId == id);
        }
        #endregion
        
        /// <summary>
        /// Initialize all preset characters into the database
        /// Call this when creating a new game
        /// </summary>
        public void InitializePresetCharacters()
        {
            // Add preset characters if they don't exist
            if (!HasCharacter("dominic"))
            {
                var dominic = CharacterPresets.CreateDominic(1);
                PlayableCharacters.Add(dominic);
            }
            
            if (!HasCharacter("echo_walker"))
            {
                var echo = CharacterPresets.CreateEchoWalker(1);
                PlayableCharacters.Add(echo);
            }
            
            if (!HasCharacter("shadows_mirror"))
            {
                var mirror = CharacterPresets.CreateShadowsMirror(1);
                PlayableCharacters.Add(mirror);
            }
            
            GD.Print($"GameDatabase initialized with {PlayableCharacters.Count} playable characters");
        }
        
        private bool HasCharacter(string id)
        {
            return GetCharacter(id) != null;
        }
    }
}