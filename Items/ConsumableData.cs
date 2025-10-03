using Godot;
using EchoesAcrossTime.Combat;

namespace EchoesAcrossTime.Items
{
    public enum ConsumableEffect
    {
        RestoreHP,
        RestoreMP,
        RestoreHPPercent,
        RestoreMPPercent,
        ReviveCharacter,
        CureStatus,
        BuffStat,
        DamageEnemy
    }

    [GlobalClass]
    public partial class ConsumableData : ItemData
    {
        [ExportGroup("Healing")]
        [Export] public int HPRestore { get; set; } = 0;
        [Export] public int MPRestore { get; set; } = 0;
        [Export] public float HPRestorePercent { get; set; } = 0f;  // 0-1
        [Export] public float MPRestorePercent { get; set; } = 0f;  // 0-1

        // Backward compatibility properties
        public int RestoresHP { get => HPRestore; set => HPRestore = value; }
        public int RestoresMP { get => MPRestore; set => MPRestore = value; }
        public float RestoresHPPercent { get => HPRestorePercent; set => HPRestorePercent = value; }
        public float RestoresMPPercent { get => MPRestorePercent; set => MPRestorePercent = value; }

        [ExportGroup("Revival")]
        [Export] public bool CanRevive { get; set; } = false;
        [Export] public float ReviveHPPercent { get; set; } = 0.25f;  // Revive with 25% HP

        // Backward compatibility
        public bool Revives { get => CanRevive; set => CanRevive = value; }

        [ExportGroup("Status Effects")]
        [Export] public Godot.Collections.Array<StatusEffect> CuresStatuses { get; set; }
        [Export] public bool CuresAllStatuses { get; set; } = false;

        // Backward compatibility
        public Godot.Collections.Array<StatusEffect> CuresStatus => CuresStatuses;
        public bool CuresAllDebuffs { get => CuresAllStatuses; set => CuresAllStatuses = value; }

        [ExportGroup("Buffs (Temporary)")]
        [Export] public int AttackBuff { get; set; } = 0;
        [Export] public int DefenseBuff { get; set; } = 0;
        [Export] public int MagicAttackBuff { get; set; } = 0;
        [Export] public int MagicDefenseBuff { get; set; } = 0;
        [Export] public int SpeedBuff { get; set; } = 0;
        [Export] public int BuffDuration { get; set; } = 3;  // Number of turns

        // Backward compatibility
        public int TemporaryAttackBoost { get => AttackBuff; set => AttackBuff = value; }
        public int TemporaryDefenseBoost { get => DefenseBuff; set => DefenseBuff = value; }
        public int TemporarySpeedBoost { get => SpeedBuff; set => SpeedBuff = value; }

        [ExportGroup("Offensive Items")]
        [Export] public int DamageAmount { get; set; } = 0;
        [Export] public ElementType DamageElement { get; set; } = ElementType.Physical;
        [Export] public Godot.Collections.Array<StatusEffect> InflictsStatus { get; set; }
        [Export] public int StatusChance { get; set; } = 0;
        [Export] public int StatusDuration { get; set; } = 0;

        [ExportGroup("Target")]
        [Export] public bool TargetAllies { get; set; } = true;
        [Export] public bool TargetAll { get; set; } = false;  // All allies or all enemies

        [ExportGroup("Animation")]
        [Export] public string UseAnimationPath { get; set; } = "";

        public ConsumableData()
        {
            Type = ItemType.Consumable;
            IsConsumable = true;
            CuresStatuses = new Godot.Collections.Array<StatusEffect>();
            InflictsStatus = new Godot.Collections.Array<StatusEffect>();
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