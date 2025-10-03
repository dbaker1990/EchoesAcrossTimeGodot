using Godot;

namespace EchoesAcrossTime.Items
{
    /// <summary>
    /// Material quality tiers
    /// </summary>
    public enum MaterialQuality
    {
        Poor,
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    /// <summary>
    /// Types of crafting materials
    /// </summary>
    public enum MaterialCategory
    {
        Ore,            // Metal ores
        Herb,           // Plants and herbs
        Wood,           // Lumber and wood
        Cloth,          // Fabrics and textiles
        Leather,        // Animal hides
        Gem,            // Precious stones
        Essence,        // Magical essences
        Crystal,        // Magical crystals
        Reagent,        // Alchemy ingredients
        Component,      // Mechanical parts
        Monster         // Monster drops
    }

    /// <summary>
    /// Represents a crafting material item
    /// </summary>
    [GlobalClass]
    public partial class MaterialData : ItemData
    {
        [ExportCategory("Material Properties")]
        [Export] public MaterialCategory MaterialCategory { get; set; } = MaterialCategory.Component;
        [Export] public MaterialQuality Quality { get; set; } = MaterialQuality.Common;
        
        [ExportCategory("Crafting Value")]
        [Export] public int CraftingValue { get; set; } = 1; // For recipe calculations
        [Export] public bool IsRefinable { get; set; } = false; // Can be refined into better materials
        [Export] public string RefinedMaterialId { get; set; } = ""; // What it refines into
        
        [ExportCategory("Drop Info")]
        [Export] public bool IsMonsterDrop { get; set; } = false;
        [Export] public string[] DroppedByEnemies { get; set; } = new string[0];

        [ExportCategory("Gathering")]
        [Export] public bool IsGatherable { get; set; } = false; // Can be gathered in the world
        [Export] public string GatheringLocation { get; set; } = ""; // Where to find it

        public MaterialData()
        {
            Type = ItemType.Material;
        }

        /// <summary>
        /// Get material quality color
        /// </summary>
        public Color GetQualityColor()
        {
            return Quality switch
            {
                MaterialQuality.Poor => new Color(0.6f, 0.6f, 0.6f),
                MaterialQuality.Common => new Color(1.0f, 1.0f, 1.0f),
                MaterialQuality.Uncommon => new Color(0.3f, 1.0f, 0.3f),
                MaterialQuality.Rare => new Color(0.3f, 0.5f, 1.0f),
                MaterialQuality.Epic => new Color(0.8f, 0.3f, 1.0f),
                MaterialQuality.Legendary => new Color(1.0f, 0.6f, 0.0f),
                _ => new Color(1, 1, 1)
            };
        }

        /// <summary>
        /// Get quality display text
        /// </summary>
        public string GetQualityText()
        {
            return Quality switch
            {
                MaterialQuality.Poor => "Poor Quality",
                MaterialQuality.Common => "Common",
                MaterialQuality.Uncommon => "Uncommon",
                MaterialQuality.Rare => "Rare",
                MaterialQuality.Epic => "Epic",
                MaterialQuality.Legendary => "Legendary",
                _ => "Unknown"
            };
        }

        /// <summary>
        /// Get material category icon/description
        /// </summary>
        public string GetCategoryDescription()
        {
            return MaterialCategory switch
            {
                MaterialCategory.Ore => "Metal ore for smithing",
                MaterialCategory.Herb => "Herb for alchemy",
                MaterialCategory.Wood => "Wood for crafting",
                MaterialCategory.Cloth => "Cloth for tailoring",
                MaterialCategory.Leather => "Leather for armor",
                MaterialCategory.Gem => "Precious gemstone",
                MaterialCategory.Essence => "Magical essence",
                MaterialCategory.Crystal => "Magical crystal",
                MaterialCategory.Reagent => "Alchemical reagent",
                MaterialCategory.Component => "Crafting component",
                MaterialCategory.Monster => "Monster material",
                _ => "Crafting material"
            };
        }
    }

    /// <summary>
    /// Helper class to create common materials
    /// </summary>
    public static class MaterialPresets
    {
        public static MaterialData CreateIronOre()
        {
            return new MaterialData
            {
                ItemId = "ore_iron",
                DisplayName = "Iron Ore",
                Description = "A chunk of iron ore. Can be smelted into iron ingots.",
                MaterialCategory = MaterialCategory.Ore,
                Quality = MaterialQuality.Common,
                CraftingValue = 1,
                Rarity = ItemRarity.Common,
                MaxStack = 99,
                IsGatherable = true,
                GatheringLocation = "Mountain caves and mines"
            };
        }

        public static MaterialData CreateSteelIngot()
        {
            return new MaterialData
            {
                ItemId = "ore_steel",
                DisplayName = "Steel Ingot",
                Description = "Refined steel ingot, perfect for weapon crafting.",
                MaterialCategory = MaterialCategory.Ore,
                Quality = MaterialQuality.Uncommon,
                CraftingValue = 3,
                Rarity = ItemRarity.Uncommon,
                MaxStack = 99,
                IsRefinable = false
            };
        }

        public static MaterialData CreateHealingHerb()
        {
            return new MaterialData
            {
                ItemId = "herb_healing",
                DisplayName = "Healing Herb",
                Description = "A medicinal herb with restorative properties.",
                MaterialCategory = MaterialCategory.Herb,
                Quality = MaterialQuality.Common,
                CraftingValue = 1,
                Rarity = ItemRarity.Common,
                MaxStack = 99,
                IsGatherable = true,
                GatheringLocation = "Forest clearings and meadows"
            };
        }

        public static MaterialData CreateManaHerb()
        {
            return new MaterialData
            {
                ItemId = "herb_mana",
                DisplayName = "Mana Herb",
                Description = "A magical herb that restores magical energy.",
                MaterialCategory = MaterialCategory.Herb,
                Quality = MaterialQuality.Common,
                CraftingValue = 1,
                Rarity = ItemRarity.Common,
                MaxStack = 99,
                IsGatherable = true,
                GatheringLocation = "Near magical ley lines"
            };
        }

        public static MaterialData CreateCrystalShard()
        {
            return new MaterialData
            {
                ItemId = "crystal_shard",
                DisplayName = "Crystal Shard",
                Description = "A fragment of a magical crystal.",
                MaterialCategory = MaterialCategory.Crystal,
                Quality = MaterialQuality.Uncommon,
                CraftingValue = 2,
                Rarity = ItemRarity.Uncommon,
                MaxStack = 50,
                IsGatherable = true,
                GatheringLocation = "Crystal caves"
            };
        }

        public static MaterialData CreateDragonScale()
        {
            return new MaterialData
            {
                ItemId = "scale_dragon",
                DisplayName = "Dragon Scale",
                Description = "A shimmering scale from a dragon. Incredibly durable.",
                MaterialCategory = MaterialCategory.Monster,
                Quality = MaterialQuality.Epic,
                CraftingValue = 10,
                Rarity = ItemRarity.Epic,
                MaxStack = 20,
                IsMonsterDrop = true,
                DroppedByEnemies = new[] { "dragon_fire", "dragon_frost", "dragon_ancient" }
            };
        }

        public static MaterialData CreateArcaneEssence()
        {
            return new MaterialData
            {
                ItemId = "essence_arcane",
                DisplayName = "Arcane Essence",
                Description = "Pure magical energy in crystallized form.",
                MaterialCategory = MaterialCategory.Essence,
                Quality = MaterialQuality.Rare,
                CraftingValue = 5,
                Rarity = ItemRarity.Rare,
                MaxStack = 30,
                IsMonsterDrop = false
            };
        }

        public static MaterialData CreateSilkCloth()
        {
            return new MaterialData
            {
                ItemId = "cloth_silk",
                DisplayName = "Silk Cloth",
                Description = "Fine silk fabric, perfect for mage robes.",
                MaterialCategory = MaterialCategory.Cloth,
                Quality = MaterialQuality.Uncommon,
                CraftingValue = 2,
                Rarity = ItemRarity.Uncommon,
                MaxStack = 99,
                IsGatherable = false
            };
        }
    }
}