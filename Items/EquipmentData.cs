using Godot;
using System;
using System.Collections.Generic;
using EchoesAcrossTime.Combat;
using EchoesAcrossTime.Database;

namespace EchoesAcrossTime.Items
{
    public enum EquipSlot
    {
        Weapon,
        Armor,
        Accessory1,
        Accessory2
    }

    /// <summary>
    /// Equipment item data with stat bonuses
    /// </summary>
    [GlobalClass]
    public partial class EquipmentData : ItemData
    {
        [ExportGroup("Equipment Type")]
        [Export] public EquipSlot Slot { get; set; } = EquipSlot.Weapon;
        [Export] public WeaponType WeaponType { get; set; } = WeaponType.None;
        [Export] public ArmorType ArmorType { get; set; } = ArmorType.LightArmor;
        [Export] public AccessoryType AccessoryType { get; set; } = AccessoryType.Ring;

        [ExportGroup("Stat Bonuses")]
        [Export] public int MaxHPBonus { get; set; } = 0;
        [Export] public int MaxMPBonus { get; set; } = 0;
        [Export] public int AttackBonus { get; set; } = 0;
        [Export] public int DefenseBonus { get; set; } = 0;
        [Export] public int MagicAttackBonus { get; set; } = 0;
        [Export] public int MagicDefenseBonus { get; set; } = 0;
        [Export] public int SpeedBonus { get; set; } = 0;

        [ExportGroup("Battle Stats")]
        [Export] public int CriticalRateBonus { get; set; } = 0;
        [Export] public int EvasionRateBonus { get; set; } = 0;

        [ExportGroup("Element Properties")]
        [Export] public ElementType WeaponElement { get; set; } = ElementType.Physical;
        [Export] public ElementAffinityData ElementalResistances { get; set; }

        [ExportGroup("Equipment Restrictions")]
        [Export] public Godot.Collections.Array<CharacterClass> AllowedClasses { get; set; }
        [Export] public int MinimumLevel { get; set; } = 1;

        [ExportGroup("Special Effects")]
        [Export(PropertyHint.MultilineText)] public string SpecialEffect { get; set; } = "";
        [Export] public Godot.Collections.Array<StatusEffect> GrantsImmunityTo { get; set; }

        public EquipmentData()
        {
            Type = ItemType.Weapon;
            IsConsumable = false;
            UsableInBattle = false;
            UsableInField = false;
            MaxStack = 1;
            ElementalResistances = new ElementAffinityData();
            AllowedClasses = new Godot.Collections.Array<CharacterClass>();
            GrantsImmunityTo = new Godot.Collections.Array<StatusEffect>();
        }

        /// <summary>
        /// Check if character can equip this item
        /// </summary>
        public bool CanEquip(CharacterData character)
        {
            if (character == null) return false;

            // Check level requirement
            if (character.Level < MinimumLevel)
                return false;

            // Check class restriction
            if (AllowedClasses.Count > 0 && !AllowedClasses.Contains(character.Class))
                return false;

            return true;
        }

        public override bool IsValid()
        {
            return base.IsValid() && MinimumLevel >= 1;
        }
    }
}