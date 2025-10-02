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

        [ExportGroup("Revival")]
        [Export] public bool CanRevive { get; set; } = false;
        [Export] public float ReviveHPPercent { get; set; } = 0.25f;  // Revive with 25% HP

        [ExportGroup("Status Effects")]
        [Export] public Godot.Collections.Array<StatusEffect> CuresStatuses { get; set; }
        [Export] public bool CuresAllStatuses { get; set; } = false;

        [ExportGroup("Buffs (Temporary)")]
        [Export] public int AttackBuff { get; set; } = 0;
        [Export] public int DefenseBuff { get; set; } = 0;
        [Export] public int MagicAttackBuff { get; set; } = 0;
        [Export] public int MagicDefenseBuff { get; set; } = 0;
        [Export] public int SpeedBuff { get; set; } = 0;
        [Export] public int BuffDuration { get; set; } = 3;  // Number of turns

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
        }

        /// <summary>
        /// Use the consumable item on a character
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

            // Status cures would be implemented here
            // Buffs would be applied here

            GD.Print($"Used {DisplayName} on {target.CharacterName}");
        }
    }
}