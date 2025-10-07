using Godot;
using Godot.Collections;
using System.Linq;
using EchoesAcrossTime.Combat;
using EchoesAcrossTime.Items;

namespace RPG.Items
{
    /// <summary>
    /// Target type for consumable items
    /// </summary>
    public enum ConsumableTargetType
    {
        OneAlly,
        AllAllies,
        OneEnemy,
        AllEnemies,
        RandomEnemy
    }

    [GlobalClass]
    public partial class ConsumableData : ItemData
    {
        [ExportGroup("Restoration")]
        [Export] public int HPRestore { get; set; } = 0;
        [Export(PropertyHint.Range, "0,1")] public float HPRestorePercent { get; set; } = 0f;
        [Export] public int MPRestore { get; set; } = 0;
        [Export(PropertyHint.Range, "0,1")] public float MPRestorePercent { get; set; } = 0f;

        [ExportGroup("Status Effects")]
        [Export] public Array<StatusEffect> CuresStatuses { get; set; }
        [Export] public bool CuresAllStatuses { get; set; } = false;

        [ExportGroup("Revival")]
        [Export] public bool CanRevive { get; set; } = false;
        [Export(PropertyHint.Range, "0,1")] public float ReviveHPPercent { get; set; } = 0.5f;

        [ExportGroup("Buffs")]
        [Export] public int AttackBuff { get; set; } = 0;
        [Export] public int DefenseBuff { get; set; } = 0;
        [Export] public int MagicBuff { get; set; } = 0;
        [Export] public int ResistanceBuff { get; set; } = 0;
        [Export] public int SpeedBuff { get; set; } = 0;
        [Export] public int BuffDuration { get; set; } = 3;

        // Legacy properties for backward compatibility
        public bool Revives { get => CanRevive; set => CanRevive = value; }
        public int RestoresHP { get => HPRestore; set => HPRestore = value; }
        public int RestoresMP { get => MPRestore; set => MPRestore = value; }
        public Array<StatusEffect> CuresStatus { get => CuresStatuses; set => CuresStatuses = value; }
        public int TemporaryAttackBoost { get => AttackBuff; set => AttackBuff = value; }
        public int TemporaryDefenseBoost { get => DefenseBuff; set => DefenseBuff = value; }
        public int TemporaryMagicBoost { get => MagicBuff; set => MagicBuff = value; }
        public int TemporaryResistanceBoost { get => ResistanceBuff; set => ResistanceBuff = value; }
        public int TemporarySpeedBoost { get => SpeedBuff; set => SpeedBuff = value; }

        [ExportGroup("Offensive Items")]
        [Export] public int DamageAmount { get; set; } = 0;
        [Export] public ElementType DamageElement { get; set; } = ElementType.Physical;
        [Export] public Array<StatusEffect> InflictsStatus { get; set; }
        [Export] public int StatusChance { get; set; } = 0;
        [Export] public int StatusDuration { get; set; } = 0;

        [ExportGroup("Target")]
        [Export] public ConsumableTargetType TargetType { get; set; } = ConsumableTargetType.OneAlly;

        [ExportGroup("Animation")]
        [Export] public string UseAnimationPath { get; set; } = "";

        // Legacy properties for backward compatibility
        public bool TargetAllies 
        { 
            get => TargetType == ConsumableTargetType.OneAlly || TargetType == ConsumableTargetType.AllAllies;
            set 
            {
                // When setting via legacy, maintain single/all distinction
                if (value)
                    TargetType = TargetAll ? ConsumableTargetType.AllAllies : ConsumableTargetType.OneAlly;
                else
                    TargetType = TargetAll ? ConsumableTargetType.AllEnemies : ConsumableTargetType.OneEnemy;
            }
        }

        public bool TargetAll 
        { 
            get => TargetType == ConsumableTargetType.AllAllies || TargetType == ConsumableTargetType.AllEnemies;
            set
            {
                // When setting via legacy, maintain ally/enemy distinction
                bool isAlly = TargetAllies;
                if (value)
                    TargetType = isAlly ? ConsumableTargetType.AllAllies : ConsumableTargetType.AllEnemies;
                else
                    TargetType = isAlly ? ConsumableTargetType.OneAlly : ConsumableTargetType.OneEnemy;
            }
        }

        public ConsumableData()
        {
            Type = ItemType.Consumable;
            IsConsumable = true;
            CuresStatuses = new Array<StatusEffect>();
            InflictsStatus = new Array<StatusEffect>();
        }

        /// <summary>
        /// Check if this item targets allies
        /// </summary>
        public bool IsAllyTargeting()
        {
            return TargetType == ConsumableTargetType.OneAlly || TargetType == ConsumableTargetType.AllAllies;
        }

        /// <summary>
        /// Check if this item targets enemies
        /// </summary>
        public bool IsEnemyTargeting()
        {
            return TargetType == ConsumableTargetType.OneEnemy || 
                   TargetType == ConsumableTargetType.AllEnemies || 
                   TargetType == ConsumableTargetType.RandomEnemy;
        }

        /// <summary>
        /// Check if this item targets all (allies or enemies)
        /// </summary>
        public bool TargetsAll()
        {
            return TargetType == ConsumableTargetType.AllAllies || TargetType == ConsumableTargetType.AllEnemies;
        }

        /// <summary>
        /// Check if this item targets randomly
        /// </summary>
        public bool TargetsRandom()
        {
            return TargetType == ConsumableTargetType.RandomEnemy;
        }

        /// <summary>
        /// Use the consumable item on a character
        /// NOTE: This is a simple implementation for basic HP/MP restoration.
        /// Status effects and buffs should be handled by BattleItemSystem in combat.
        /// </summary>
        public void Use(CharacterStats target)
        {
            if (target == null) return;

            // Restore HP
            if (HPRestore > 0)
            {
                target.Heal(HPRestore);
            }
            if (HPRestorePercent > 0)
            {
                int healAmount = Mathf.RoundToInt(target.MaxHP * HPRestorePercent);
                target.Heal(healAmount);
            }

            // Restore MP
            if (MPRestore > 0)
            {
                target.RestoreMP(MPRestore);
            }
            if (MPRestorePercent > 0)
            {
                int restoreAmount = Mathf.RoundToInt(target.MaxMP * MPRestorePercent);
                target.RestoreMP(restoreAmount);
            }

            // Revival
            if (CanRevive && !target.IsAlive)
            {
                int reviveHP = Mathf.RoundToInt(target.MaxHP * ReviveHPPercent);
                target.CurrentHP = reviveHP;
            }

            // Status effects and buffs are handled by BattleItemSystem during combat
            GD.Print($"Used {DisplayName} on {target.CharacterName}");
        }
    }
}