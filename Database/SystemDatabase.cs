using Godot;
using System;
using EchoesAcrossTime.Combat;

namespace EchoesAcrossTime.Database
{
    /// <summary>
    /// Global System Database - Similar to RPG Maker MZ System 1
    /// Handles all game-wide settings, initial conditions, and audio/SE configuration
    /// </summary>
    [GlobalClass]
    public partial class SystemDatabase : Resource
    {
        #region Game Title and Basic Info
        [ExportCategory("Game Information")]
        [Export] public string GameTitle { get; set; } = "Echoes Across Time";
        [Export] public Texture2D TitleScreenImage { get; set; }
        [Export] public string GameVersion { get; set; } = "1.0.0";
        #endregion

        #region Starting Conditions
        [ExportCategory("Starting Conditions")]
        [Export] public string[] StartingPartyIds { get; set; } = { "dominic", "echo_walker" };
        [Export] public int StartingGold { get; set; } = 500;
        [Export] public string StartingMapPath { get; set; } = "res://Maps/Veridia/VeridiaCapital.tscn";
        [Export] public Vector2 StartingPosition { get; set; } = new Vector2(320, 240);
        [Export] public int StartingDirection { get; set; } = 2; // 0=Up, 1=Right, 2=Down, 3=Left
        #endregion

        #region Damage Formula Configuration
        [ExportCategory("Battle System")]
        [Export] public bool UseGlobalFormulaOverride { get; set; } = false;
        [Export] public Combat.DamageFormulaType GlobalDamageFormula { get; set; } = Combat.DamageFormulaType.Simple;
        [Export] public float BaseDamageMultiplier { get; set; } = 1.0f;
        [Export] public bool UseCriticalHits { get; set; } = true;
        [Export] public float CriticalMultiplier { get; set; } = 2.0f;
        [Export] public bool UseElementalSystem { get; set; } = true;
        #endregion

        #region Music Configuration
        [ExportCategory("Music Tracks")]
        [Export] public AudioStream TitleMusic { get; set; }
        [Export] public AudioStream BattleMusic { get; set; }
        [Export] public AudioStream VictoryMusic { get; set; }
        [Export] public AudioStream DefeatMusic { get; set; }
        [Export] public AudioStream GameOverMusic { get; set; }
        
        [ExportSubgroup("Vehicle Music")]
        [Export] public AudioStream BoatMusic { get; set; }
        [Export] public AudioStream ShipMusic { get; set; }
        [Export] public AudioStream AirshipMusic { get; set; }
        #endregion

        #region Sound Effects - UI
        [ExportCategory("UI Sound Effects")]
        [Export] public AudioStream CursorSE { get; set; }
        [Export] public AudioStream OkSE { get; set; }
        [Export] public AudioStream BuzzerSE { get; set; }
        [Export] public AudioStream CancelSE { get; set; }
        [Export] public AudioStream EquipSE { get; set; }
        [Export] public AudioStream SaveSE { get; set; }
        [Export] public AudioStream LoadSE { get; set; }
        #endregion

        #region Sound Effects - Battle Start/End
        [ExportCategory("Battle Transition SFX")]
        [Export] public AudioStream BattleStartSE { get; set; }
        [Export] public AudioStream EscapeSE { get; set; }
        #endregion

        #region Sound Effects - Enemy Actions
        [ExportCategory("Enemy Sound Effects")]
        [Export] public AudioStream EnemyAttackSE { get; set; }
        [Export] public AudioStream EnemyDamageSE { get; set; }
        [Export] public AudioStream EnemyCollapseSE { get; set; }
        [Export] public AudioStream BossCollapse1SE { get; set; }
        [Export] public AudioStream BossCollapse2SE { get; set; }
        #endregion

        #region Sound Effects - Actor Actions
        [ExportCategory("Actor Sound Effects")]
        [Export] public AudioStream ActorDamageSE { get; set; }
        [Export] public AudioStream ActorCollapseSE { get; set; }
        [Export] public AudioStream RecoverySE { get; set; }
        #endregion

        #region Sound Effects - Battle Results
        [ExportCategory("Battle Result SFX")]
        [Export] public AudioStream MissSE { get; set; }
        [Export] public AudioStream EvasionSE { get; set; }
        [Export] public AudioStream MagicEvasionSE { get; set; }
        [Export] public AudioStream MagicReflectionSE { get; set; }
        #endregion

        #region Sound Effects - Items and Skills
        [ExportCategory("Item/Skill Sound Effects")]
        [Export] public AudioStream ShopSE { get; set; }
        [Export] public AudioStream UseItemSE { get; set; }
        [Export] public AudioStream UseSkillSE { get; set; }
        #endregion

        #region Volume Settings
        [ExportCategory("Default Volume Settings")]
        [Export(PropertyHint.Range, "0,100,1")] public float DefaultBGMVolume { get; set; } = 80f;
        [Export(PropertyHint.Range, "0,100,1")] public float DefaultBGSVolume { get; set; } = 80f;
        [Export(PropertyHint.Range, "0,100,1")] public float DefaultMEVolume { get; set; } = 80f;
        [Export(PropertyHint.Range, "0,100,1")] public float DefaultSEVolume { get; set; } = 80f;
        #endregion

        #region Helper Methods
        /// <summary>
        /// Play a system sound effect through the AudioManager
        /// </summary>
        public void PlaySystemSE(SystemSoundEffect seType)
        {
            if (AudioManager.Instance == null) return;

            AudioStream se = seType switch
            {
                SystemSoundEffect.Cursor => CursorSE,
                SystemSoundEffect.Ok => OkSE,
                SystemSoundEffect.Buzzer => BuzzerSE,
                SystemSoundEffect.Cancel => CancelSE,
                SystemSoundEffect.Equip => EquipSE,
                SystemSoundEffect.Save => SaveSE,
                SystemSoundEffect.Load => LoadSE,
                SystemSoundEffect.BattleStart => BattleStartSE,
                SystemSoundEffect.Escape => EscapeSE,
                SystemSoundEffect.EnemyAttack => EnemyAttackSE,
                SystemSoundEffect.EnemyDamage => EnemyDamageSE,
                SystemSoundEffect.EnemyCollapse => EnemyCollapseSE,
                SystemSoundEffect.BossCollapse1 => BossCollapse1SE,
                SystemSoundEffect.BossCollapse2 => BossCollapse2SE,
                SystemSoundEffect.ActorDamage => ActorDamageSE,
                SystemSoundEffect.ActorCollapse => ActorCollapseSE,
                SystemSoundEffect.Recovery => RecoverySE,
                SystemSoundEffect.Miss => MissSE,
                SystemSoundEffect.Evasion => EvasionSE,
                SystemSoundEffect.MagicEvasion => MagicEvasionSE,
                SystemSoundEffect.MagicReflection => MagicReflectionSE,
                SystemSoundEffect.Shop => ShopSE,
                SystemSoundEffect.UseItem => UseItemSE,
                SystemSoundEffect.UseSkill => UseSkillSE,
                _ => null
            };

            if (se != null)
            {
                AudioManager.Instance.PlaySoundEffect(se);
            }
        }

        /// <summary>
        /// Initialize a new game with starting conditions
        /// </summary>
        public void InitializeNewGame(SaveData saveData, GameDatabase database)
        {
            // Set starting party members
            saveData.Party.Clear();
            foreach (string memberId in StartingPartyIds)
            {
                var characterData = database.GetCharacter(memberId);
                if (characterData != null)
                {
                    var stats = characterData.CreateStatsInstance();
                    var memberData = PartyMemberSaveData.FromCharacterStats(stats);
                    saveData.Party.Add(memberData);
                }
            }

            // Set starting gold
            saveData.Inventory.Gold = StartingGold;

            // Set starting position
            saveData.CurrentMapPath = StartingMapPath;
            saveData.PlayerPosition = new Vector2Data(StartingPosition);
            saveData.PlayerDirection = StartingDirection;

            GD.Print($"New game initialized: Party={StartingPartyIds.Length}, Gold={StartingGold}");
        }

        /// <summary>
        /// Calculate damage using system settings
        /// If UseGlobalFormulaOverride is true, forces all skills to use GlobalDamageFormula
        /// Otherwise uses each skill's individual formula
        /// </summary>
        public int CalculateDamage(CharacterStats attacker, CharacterStats target, SkillData skill, bool isCritical = false)
        {
            if (attacker == null || target == null || skill == null) return 0;

            // Handle Fixed and Percentage damage types (these ignore formulas)
            if (skill.DamageType == Combat.DamageType.Fixed)
            {
                return (int)(skill.BasePower * BaseDamageMultiplier);
            }

            if (skill.DamageType == Combat.DamageType.Percentage)
            {
                int percentDamage = Mathf.RoundToInt(target.MaxHP * (skill.BasePower / 100f));
                return (int)(percentDamage * BaseDamageMultiplier);
            }

            // Use global formula override if enabled, otherwise use skill's formula
            Combat.DamageFormulaType formulaToUse = UseGlobalFormulaOverride 
                ? GlobalDamageFormula 
                : skill.DamageFormula;

            // Calculate using the selected formula
            int baseDamage = Combat.DamageFormula.Calculate(formulaToUse, attacker, target, skill, isCritical);
            
            // Apply global damage multiplier
            return (int)(baseDamage * BaseDamageMultiplier);
        }
        #endregion
    }

    /// <summary>
    /// System sound effect types for easy reference
    /// </summary>
    public enum SystemSoundEffect
    {
        Cursor,
        Ok,
        Buzzer,
        Cancel,
        Equip,
        Save,
        Load,
        BattleStart,
        Escape,
        EnemyAttack,
        EnemyDamage,
        EnemyCollapse,
        BossCollapse1,
        BossCollapse2,
        ActorDamage,
        ActorCollapse,
        Recovery,
        Miss,
        Evasion,
        MagicEvasion,
        MagicReflection,
        Shop,
        UseItem,
        UseSkill
    }
}