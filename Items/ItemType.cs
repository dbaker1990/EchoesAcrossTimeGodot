using Godot;

namespace EchoesAcrossTime.Items
{
    public enum ItemType
    {
        Consumable,     // Potions, food, etc.
        Weapon,
        Armor,
        Accessory,
        KeyItem,        // Quest items, story items
        Material        // Crafting materials
    }

    public enum ItemRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary,
        Unique          // Story-specific items
    }

    public enum WeaponType
    {
        Staff,          // For mages
        Wand,
        Tome,           // Spell books
        Ceremonial,     // Court Mage weapons
        Sword,
        Bow,
        None
    }

    public enum ArmorType
    {
        LightArmor,     // Robes, cloth
        MediumArmor,    // Leather, chain
        HeavyArmor,     // Plate, heavy
        Clothing        // Diplomatic attire
    }

    public enum AccessoryType
    {
        Ring,
        Amulet,
        Cloak,
        Brooch,
        Crown,          // Royal accessories
        Badge           // Diplomatic insignia
    }
}