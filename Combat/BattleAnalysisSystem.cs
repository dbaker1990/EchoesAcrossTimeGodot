using Godot;
using System.Collections.Generic;
using System.Linq;

namespace EchoesAcrossTime.Combat
{
    /// <summary>
    /// Manages enemy analysis and information reveal (for Revealing Light spell)
    /// </summary>
    public partial class BattleAnalysisSystem : Node
    {
        public static BattleAnalysisSystem Instance { get; private set; }

        // Track which enemies have been analyzed in current battle
        private HashSet<string> analyzedEnemies = new HashSet<string>();

        [Signal]
        public delegate void EnemyAnalyzedEventHandler(string enemyId, CharacterStats enemyStats);

        public override void _Ready()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                QueueFree();
            }
        }

        /// <summary>
        /// Analyze an enemy to reveal their information
        /// </summary>
        public void AnalyzeEnemy(CharacterStats enemy)
        {
            if (enemy == null) return;

            string enemyId = enemy.CharacterId;

            if (!analyzedEnemies.Contains(enemyId))
            {
                analyzedEnemies.Add(enemyId);
                EmitSignal(SignalName.EnemyAnalyzed, enemyId, enemy);
                DisplayEnemyInfo(enemy);
            }
            else
            {
                GD.Print($"{enemy.CharacterName} has already been analyzed!");
                DisplayEnemyInfo(enemy);
            }
        }

        /// <summary>
        /// Check if an enemy has been analyzed
        /// </summary>
        public bool IsEnemyAnalyzed(string enemyId)
        {
            return analyzedEnemies.Contains(enemyId);
        }

        /// <summary>
        /// Display enemy information after analysis
        /// </summary>
        private void DisplayEnemyInfo(CharacterStats enemy)
        {
            GD.Print("=== ENEMY ANALYSIS ===");
            GD.Print($"Name: {enemy.CharacterName}");
            GD.Print($"Level: {enemy.Level}");
            GD.Print($"HP: {enemy.CurrentHP}/{enemy.MaxHP}");
            GD.Print($"MP: {enemy.CurrentMP}/{enemy.MaxMP}");
            GD.Print("");

            // Display stats
            GD.Print("--- Stats ---");
            GD.Print($"Attack: {enemy.Attack}");
            GD.Print($"Defense: {enemy.Defense}");
            GD.Print($"Magic Attack: {enemy.MagicAttack}");
            GD.Print($"Magic Defense: {enemy.MagicDefense}");
            GD.Print($"Speed: {enemy.Speed}");
            GD.Print($"Luck: {enemy.Luck}");
            GD.Print("");

            // Display elemental affinities
            if (enemy.ElementAffinities != null)
            {
                GD.Print("--- Elemental Affinities ---");
                DisplayAffinity("Physical", enemy.ElementAffinities.PhysicalAffinity);
                DisplayAffinity("Fire", enemy.ElementAffinities.FireAffinity);
                DisplayAffinity("Water", enemy.ElementAffinities.WaterAffinity);
                DisplayAffinity("Thunder", enemy.ElementAffinities.ThunderAffinity);
                DisplayAffinity("Ice", enemy.ElementAffinities.IceAffinity);
                DisplayAffinity("Earth", enemy.ElementAffinities.EarthAffinity);
                DisplayAffinity("Wind", enemy.ElementAffinities.WindAffinity);
                DisplayAffinity("Light", enemy.ElementAffinities.LightAffinity);
                DisplayAffinity("Dark", enemy.ElementAffinities.DarkAffinity);
                DisplayAffinity("Almighty", enemy.ElementAffinities.AlmightyAffinity);
                GD.Print("");
            }

            // Display weaknesses prominently
            var weaknesses = enemy.ElementAffinities?.GetWeaknesses() ?? new List<ElementType>();
            if (weaknesses.Count > 0)
            {
                GD.Print($"⚠️ WEAKNESSES: {string.Join(", ", weaknesses)}");
            }
            else
            {
                GD.Print("⚠️ WEAKNESSES: None");
            }

            // Display resistances
            var resistances = enemy.ElementAffinities?.GetResistances() ?? new List<ElementType>();
            if (resistances.Count > 0)
            {
                GD.Print($"🛡️ RESISTANCES: {string.Join(", ", resistances)}");
            }

            GD.Print("=====================");
        }

        /// <summary>
        /// Display single affinity
        /// </summary>
        private void DisplayAffinity(string elementName, ElementAffinity affinity)
        {
            string affinityText = affinity switch
            {
                ElementAffinity.Weak => "WEAK ⚠️",
                ElementAffinity.Resist => "Resist 🛡️",
                ElementAffinity.Immune => "Immune 🚫",
                ElementAffinity.Absorb => "Absorb ❤️",
                ElementAffinity.Null => "Null",
                _ => "Normal"
            };

            if (affinity != ElementAffinity.Normal)
            {
                GD.Print($"  {elementName}: {affinityText}");
            }
        }

        /// <summary>
        /// Clear analyzed enemies (call when battle ends)
        /// </summary>
        public void ClearAnalyzedEnemies()
        {
            analyzedEnemies.Clear();
            GD.Print("Battle analysis data cleared.");
        }

        /// <summary>
        /// Get all analyzed enemy IDs
        /// </summary>
        public List<string> GetAnalyzedEnemies()
        {
            return analyzedEnemies.ToList();
        }
    }
}