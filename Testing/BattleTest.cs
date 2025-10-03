using Godot;
using System.Collections.Generic;
using EchoesAcrossTime.Combat;

namespace EchoesAcrossTime.Testing
{
    /// <summary>
    /// Example scene demonstrating Persona 5-style battle system
    /// Shows how to set up and execute battles with weaknesses, One More, and All-Out Attack
    /// </summary>
    public partial class BattleTest : Node
    {
        private BattleManager battleManager;
        
        public override void _Ready()
        {
            // Create battle manager
            battleManager = new BattleManager();
            AddChild(battleManager);
            
            // Connect to battle signals for demonstration
            battleManager.TurnStarted += OnTurnStarted;
            battleManager.ActionExecuted += OnActionExecuted;
            battleManager.WeaknessHit += OnWeaknessHit;
            battleManager.OneMoreTriggered += OnOneMoreTriggered;
            battleManager.AllOutAttackReady += OnAllOutAttackReady;
            battleManager.BattleEnded += OnBattleEnded;
            battleManager.Knockdown += OnKnockdown;
            
            // Set up a demo battle
            StartDemoBattle();
        }
        
        private void StartDemoBattle()
        {
            GD.Print("\n╔═══════════════════════════════════════════════════╗");
            GD.Print("║  Persona 5 Style Battle System - Demo Battle   ║");
            GD.Print("╚═══════════════════════════════════════════════════╝\n");
            
            // Create player party
            var playerStats = new List<CharacterStats>
            {
                CreateHero("Dominic", 150, 80, 35, 25, 40, ElementType.Dark),
                CreateHero("Echo Walker", 120, 100, 30, 20, 45, ElementType.Light),
                CreateHero("Aria", 100, 120, 25, 30, 50, ElementType.Ice)
            };
            
            // Create enemies with weaknesses
            var enemyStats = new List<CharacterStats>
            {
                CreateEnemy("Fire Demon", 80, 50, 30, 15, 25, ElementType.Fire, ElementType.Ice),
                CreateEnemy("Thunder Beast", 70, 40, 25, 20, 35, ElementType.Thunder, ElementType.Earth),
                CreateEnemy("Shadow Knight", 100, 60, 35, 25, 20, ElementType.Dark, ElementType.Light)
            };
            
            // Initialize battle
            battleManager.InitializeBattle(playerStats, enemyStats);
            
            // Simulate some turns
            SimulateBattleTurns();
        }
        
        private void SimulateBattleTurns()
        {
            // Wait a frame then start simulating
            CallDeferred(nameof(ExecuteTurns));
        }
        
        private void ExecuteTurns()
        {
            // Turn 1: Aria uses Ice spell on Fire Demon (weakness!)
            if (battleManager.IsPlayerTurn())
            {
                var aria = battleManager.GetPlayerParty()[2];
                var fireDemon = battleManager.GetLivingEnemies()[0];
                
                var iceSkill = CreateSkill("Bufu", ElementType.Ice, 80, 15);
                var action = new BattleAction(aria, BattleActionType.Skill)
                    .WithSkill(iceSkill)
                    .WithTargets(fireDemon);
                
                GD.Print("\n>>> PLAYER ACTION: Aria casts Bufu on Fire Demon <<<");
                battleManager.ExecuteAction(action);
            }
            
            // If Aria got One More, use it
            if (battleManager.IsPlayerTurn() && battleManager.GetPlayerParty()[2].HasExtraTurn)
            {
                var aria = battleManager.GetPlayerParty()[2];
                var thunderBeast = battleManager.GetLivingEnemies().Count > 1 
                    ? battleManager.GetLivingEnemies()[1] 
                    : battleManager.GetLivingEnemies()[0];
                
                var iceSkill = CreateSkill("Bufu", ElementType.Ice, 80, 15);
                var action = new BattleAction(aria, BattleActionType.Skill)
                    .WithSkill(iceSkill)
                    .WithTargets(thunderBeast);
                
                GD.Print("\n>>> ONE MORE ACTION: Aria casts Bufu on Thunder Beast <<<");
                battleManager.ExecuteAction(action);
            }
            
            // Turn 2: Echo Walker uses Light spell on Shadow Knight (weakness!)
            if (battleManager.IsPlayerTurn())
            {
                var echo = battleManager.GetPlayerParty()[1];
                var shadowKnight = battleManager.GetLivingEnemies().Count > 2 
                    ? battleManager.GetLivingEnemies()[2] 
                    : battleManager.GetLivingEnemies()[0];
                
                var lightSkill = CreateSkill("Hama", ElementType.Light, 85, 18);
                var action = new BattleAction(echo, BattleActionType.Skill)
                    .WithSkill(lightSkill)
                    .WithTargets(shadowKnight);
                
                GD.Print("\n>>> PLAYER ACTION: Echo Walker casts Hama on Shadow Knight <<<");
                battleManager.ExecuteAction(action);
            }
            
            // Check if All-Out Attack is available
            if (battleManager.CanUseAllOutAttack())
            {
                var dominic = battleManager.GetPlayerParty()[0];
                var action = new BattleAction(dominic, BattleActionType.AllOutAttack)
                    .WithTargets(battleManager.GetLivingEnemies().ToArray());
                
                GD.Print("\n>>> ALL-OUT ATTACK TRIGGERED! <<<");
                battleManager.ExecuteAction(action);
            }
        }
        
        #region Helper Methods
        
        private CharacterStats CreateHero(string name, int hp, int mp, int atk, int def, int spd, ElementType resistance)
        {
            var stats = new CharacterStats
            {
                CharacterName = name,
                Level = 10,
                MaxHP = hp,
                CurrentHP = hp,
                MaxMP = mp,
                CurrentMP = mp,
                Attack = atk,
                Defense = def,
                MagicAttack = atk + 5,
                MagicDefense = def + 5,
                Speed = spd,
                BattleStats = new BattleStats
                {
                    AccuracyRate = 95,
                    EvasionRate = 5,
                    CriticalRate = 10,
                    CriticalDamageMultiplier = 200
                },
                ElementAffinities = new ElementAffinityData()
            };
            
            // Set resistance to one element
            stats.ElementAffinities.SetAffinity(resistance, ElementAffinity.Resist);
            
            GD.Print($"Hero created: {name} (HP:{hp}, ATK:{atk}, SPD:{spd}) - Resists {resistance}");
            return stats;
        }
        
        private CharacterStats CreateEnemy(string name, int hp, int mp, int atk, int def, int spd, 
            ElementType element, ElementType weakness)
        {
            var stats = new CharacterStats
            {
                CharacterName = name,
                Level = 8,
                MaxHP = hp,
                CurrentHP = hp,
                MaxMP = mp,
                CurrentMP = mp,
                Attack = atk,
                Defense = def,
                MagicAttack = atk,
                MagicDefense = def,
                Speed = spd,
                BattleStats = new BattleStats
                {
                    AccuracyRate = 85,
                    EvasionRate = 5,
                    CriticalRate = 5,
                    CriticalDamageMultiplier = 150
                },
                ElementAffinities = new ElementAffinityData()
            };
            
            // Set weakness
            stats.ElementAffinities.SetAffinity(weakness, ElementAffinity.Weak);
            
            // Set resistance to own element
            if (element != ElementType.None && element != ElementType.Physical)
            {
                stats.ElementAffinities.SetAffinity(element, ElementAffinity.Resist);
            }
            
            GD.Print($"Enemy created: {name} (HP:{hp}, ATK:{atk}, SPD:{spd}) - Weak to {weakness}");
            return stats;
        }
        
        private SkillData CreateSkill(string name, ElementType element, int power, int mpCost)
        {
            return new SkillData
            {
                SkillId = name.ToLower(),
                DisplayName = name,
                Description = $"{element} element spell",
                Element = element,
                BasePower = power,
                MPCost = mpCost,
                Accuracy = 95,
                DamageType = DamageType.Magical,
                DamageFormula = DamageFormulaType.Persona,
                CriticalBonus = 5
            };
        }
        
        #endregion
        
        #region Signal Handlers
        
        private void OnTurnStarted(string characterName)
        {
            GD.Print($"\n─────────────────────────────────────");
            GD.Print($"🎯 {characterName}'s turn begins!");
            GD.Print($"─────────────────────────────────────");
        }
        
        private void OnActionExecuted(string actorName, string actionName, int damageDealt, bool hitWeakness, bool wasCritical)
        {
            if (hitWeakness)
            {
                GD.Print("💥 WEAKNESS EXPLOITED!");
            }
            if (wasCritical)
            {
                GD.Print("⚡ CRITICAL HIT!");
            }
        }
        
        private void OnWeaknessHit(string attackerName, string targetName)
        {
            GD.Print($"✨ {attackerName} exploited {targetName}'s weakness!");
        }
        
        private void OnOneMoreTriggered(string characterName)
        {
            GD.Print($"⭐ {characterName} gets ONE MORE turn!");
        }
        
        private void OnAllOutAttackReady()
        {
            GD.Print("\n╔═══════════════════════════════════════╗");
            GD.Print("║  ★★★ ALL-OUT ATTACK READY! ★★★    ║");
            GD.Print("╚═══════════════════════════════════════╝");
        }
        
        private void OnKnockdown(string characterName)
        {
            GD.Print($"💫 {characterName} is knocked down!");
        }
        
        private void OnBattleEnded(bool victory)
        {
            GD.Print("\n╔═══════════════════════════════════════╗");
            if (victory)
            {
                GD.Print("║         ⭐ VICTORY! ⭐              ║");
            }
            else
            {
                GD.Print("║         💀 DEFEAT 💀               ║");
            }
            GD.Print("╚═══════════════════════════════════════╝\n");
        }
        
        #endregion
    }
}