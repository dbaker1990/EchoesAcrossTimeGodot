using Godot;
using System.Collections.Generic;
using EchoesAcrossTime.Combat;
using EchoesAcrossTime.Database;

namespace EchoesAcrossTime.Testing
{
    /// <summary>
    /// Demonstrates the Limit Break system
    /// Shows gauge filling, single limit breaks, and duo limit breaks
    /// </summary>
    public partial class LimitBreakTest : Node
    {
        private BattleManager battleManager;
        
        public override void _Ready()
        {
            battleManager = new BattleManager();
            AddChild(battleManager);
            
            // Connect signals
            battleManager.TurnStarted += OnTurnStarted;
            battleManager.LimitBreakReady += OnLimitBreakReady;
            battleManager.LimitBreakUsed += OnLimitBreakUsed;
            battleManager.BattleEnded += OnBattleEnded;
            
            StartLimitBreakDemo();
        }
        
        private void StartLimitBreakDemo()
        {
            GD.Print("\n╔═══════════════════════════════════════════════════╗");
            GD.Print("║     LIMIT BREAK SYSTEM DEMONSTRATION           ║");
            GD.Print("║     Gauge fills from taking/dealing damage     ║");
            GD.Print("╚═══════════════════════════════════════════════════╝\n");
            
            // Create party
            var playerStats = new List<CharacterStats>
            {
                CreateHero("Dominic", 200, 100, 45, 30, 40),
                CreateHero("Echo Walker", 160, 120, 40, 25, 48),
                CreateHero("Aria", 140, 140, 35, 30, 50)
            };
            
            // Create tough enemies
            var enemyStats = new List<CharacterStats>
            {
                CreateEnemy("Ancient Dragon", 500, 100, 50, 40, 30),
                CreateEnemy("Dark Colossus", 600, 80, 55, 45, 25)
            };
            
            // Create limit breaks
            var limitBreaks = new List<LimitBreakData>
            {
                ExampleLimitBreaks.CreateRealityTear(),
                ExampleLimitBreaks.CreateTemporalConvergence(),
                ExampleLimitBreaks.CreateFrozenEternity(),
                ExampleLimitBreaks.CreateEclipseOfEternity() // DUO!
            };
            
            // Initialize battle
            battleManager.InitializeBattle(playerStats, enemyStats, null, limitBreaks);
            
            // Simulate battle
            CallDeferred(nameof(SimulateLimitBreakBattle));
        }
        
        private void SimulateLimitBreakBattle()
        {
            GD.Print("\n═══ LIMIT BREAK DEMONSTRATION ═══\n");
            
            // Phase 1: Take damage to fill gauge
            GD.Print("─── Phase 1: Building Limit Gauge ───");
            GD.Print("Strategy: Take hits to fill Limit gauge!\n");
            
            // Dominic attacks (builds gauge slowly from damage dealt)
            if (battleManager.IsPlayerTurn())
            {
                var dominic = battleManager.GetPlayerParty()[0];
                var dragon = battleManager.GetLivingEnemies()[0];
                
                var action = new BattleAction(dominic, BattleActionType.Attack)
                    .WithTargets(dragon);
                
                GD.Print(">>> Dominic attacks Ancient Dragon");
                battleManager.ExecuteAction(action);
                
                float gaugePercent = battleManager.GetLimitGaugePercent(dominic);
                GD.Print($"    Dominic's Limit Gauge: {gaugePercent * 100:F0}%\n");
            }
            
            // Let enemy hit us (builds gauge quickly from damage taken)
            GD.Print(">>> Ancient Dragon counterattacks!");
            GD.Print("    (Simulating heavy damage to build Limit gauge)\n");
            
            // Manually add gauge to speed up demo
            var dominicMember = battleManager.GetPlayerParty()[0];
            var echoMember = battleManager.GetPlayerParty()[1];
            var ariaMember = battleManager.GetPlayerParty()[2];
            
            // Simulate taking big hits
            dominicMember.LimitGauge = 100; // Ready!
            dominicMember.IsLimitBreakReady = true;
            
            echoMember.LimitGauge = 100; // Ready!
            echoMember.IsLimitBreakReady = true;
            
            ariaMember.LimitGauge = 75; // Almost ready
            
            GD.Print($"Dominic's Limit Gauge: READY! ⚡");
            GD.Print($"Echo Walker's Limit Gauge: READY! ⚡");
            GD.Print($"Aria's Limit Gauge: 75%\n");
            
            // Phase 2: Use Dominic's Limit Break
            GD.Print("\n─── Phase 2: Single Limit Break ───");
            GD.Print("Dominic unleashes Reality Tear!\n");
            
            var dominicLB = battleManager.GetLimitBreak(dominicMember);
            if (dominicLB != null)
            {
                var action = new BattleAction(dominicMember, BattleActionType.LimitBreak)
                    .WithLimitBreak(dominicLB)
                    .WithTargets(battleManager.GetLivingEnemies()[0]);
                
                battleManager.ExecuteAction(action);
                
                GD.Print($"\nDominic's gauge after: {battleManager.GetLimitGaugePercent(dominicMember) * 100:F0}%");
            }
            
            // Phase 3: Fill Aria's gauge and use her limit
            GD.Print("\n\n─── Phase 3: Hybrid Limit Break ───");
            GD.Print("Aria's gauge filled! Using Frozen Eternity!\n");
            
            ariaMember.LimitGauge = 100;
            ariaMember.IsLimitBreakReady = true;
            
            var ariaLB = battleManager.GetLimitBreak(ariaMember);
            if (ariaLB != null)
            {
                var action = new BattleAction(ariaMember, BattleActionType.LimitBreak)
                    .WithLimitBreak(ariaLB)
                    .WithTargets(battleManager.GetLivingEnemies().ToArray());
                
                battleManager.ExecuteAction(action);
            }
            
            // Phase 4: DUO LIMIT BREAK!
            GD.Print("\n\n─── Phase 4: DUO LIMIT BREAK! ───");
            GD.Print("Dominic and Echo Walker's gauges are ready!");
            GD.Print("Time for the ultimate attack!\n");
            
            // Fill gauges again
            dominicMember.LimitGauge = 100;
            dominicMember.IsLimitBreakReady = true;
            echoMember.LimitGauge = 100;
            echoMember.IsLimitBreakReady = true;
            
            // Get duo limit break
            var duoLB = ExampleLimitBreaks.CreateEclipseOfEternity();
            
            var duoAction = new BattleAction(dominicMember, BattleActionType.LimitBreak)
                .WithLimitBreak(duoLB, echoMember) // Pass duo partner!
                .WithTargets(battleManager.GetLivingEnemies().ToArray());
            
            battleManager.ExecuteAction(duoAction);
            
            GD.Print("\n\n═══════════════════════════════════════");
            GD.Print("   Limit Break Demo Complete!        ");
            GD.Print("═══════════════════════════════════════\n");
        }
        
        #region Helper Methods
        
        private CharacterStats CreateHero(string name, int hp, int mp, int atk, int def, int spd)
        {
            var stats = new CharacterStats
            {
                CharacterName = name,
                Level = 20,
                MaxHP = hp,
                CurrentHP = hp,
                MaxMP = mp,
                CurrentMP = mp,
                Attack = atk,
                Defense = def,
                MagicAttack = atk + 10,
                MagicDefense = def + 10,
                Speed = spd,
                BattleStats = new BattleStats
                {
                    AccuracyRate = 95,
                    CriticalRate = 15,
                    CriticalDamageMultiplier = 200
                },
                ElementAffinities = new ElementAffinityData()
            };
            
            GD.Print($"Hero: {name} (HP:{hp}, ATK:{atk})");
            return stats;
        }
        
        private CharacterStats CreateEnemy(string name, int hp, int mp, int atk, int def, int spd)
        {
            var stats = new CharacterStats
            {
                CharacterName = name,
                Level = 25,
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
                    AccuracyRate = 90,
                    CriticalRate = 10,
                    CriticalDamageMultiplier = 150
                },
                ElementAffinities = new ElementAffinityData()
            };
            
            GD.Print($"Enemy: {name} (HP:{hp}, ATK:{atk})");
            return stats;
        }
        
        #endregion
        
        #region Signal Handlers
        
        private void OnTurnStarted(string characterName)
        {
            GD.Print($"\n═══ {characterName}'s Turn ═══");
        }
        
        private void OnLimitBreakReady(string characterName)
        {
            GD.Print($"\n⚡⚡⚡ {characterName}'s LIMIT BREAK is READY! ⚡⚡⚡");
        }
        
        private void OnLimitBreakUsed(string characterName, string limitBreakName, bool isDuo)
        {
            if (isDuo)
            {
                GD.Print($"\n🌟 DUO LIMIT BREAK ACTIVATED! 🌟");
            }
            else
            {
                GD.Print($"\n⚡ {characterName} used {limitBreakName}! ⚡");
            }
        }
        
        private void OnBattleEnded(bool victory)
        {
            string result = victory ? "⭐ VICTORY! ⭐" : "💀 DEFEAT 💀";
            GD.Print($"\n╔═══════════════════════════════════════╗");
            GD.Print($"║         {result}              ║");
            GD.Print($"╚═══════════════════════════════════════╝");
        }
        
        #endregion
    }
}