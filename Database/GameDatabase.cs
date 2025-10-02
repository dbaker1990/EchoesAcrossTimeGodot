// Database/GameDatabase.cs

using System.Linq;
using EchoesAcrossTime.Combat;
using EchoesAcrossTime.Database;
using EchoesAcrossTime.Items;
using Godot;
using Godot.Collections;

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
    [Export] public Array<ConsumableData> Consumables { get; set; }
    [Export] public Array<EquipmentData> Equipment { get; set; }
    
    public GameDatabase()
    {
        PlayableCharacters = new Array<CharacterData>();
        Enemies = new Array<CharacterData>();
        Bosses = new Array<CharacterData>();
        Skills = new Array<SkillData>();
        Consumables = new Array<ConsumableData>();
        Equipment = new Array<EquipmentData>();
    }
    
    // Helper methods
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
    
    public SkillData GetSkill(string id)
    {
        return Skills.FirstOrDefault(s => s?.SkillId == id);
    }
}