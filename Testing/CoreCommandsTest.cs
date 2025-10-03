using Godot;
using System.Collections.Generic;
using EchoesAcrossTime.Combat;
using EchoesAcrossTime.Items;

namespace EchoesAcrossTime.Testing
{
    /// <summary>
    /// Demonstrates Guard, Item, and Escape commands
    /// </summary>
    public partial class CoreCommandsTest : Node
    {
        private BattleManager battleManager;
        
        public override void _Ready()
        {
            battleManager = new BattleManager();
            AddChild(battleManager);
            
            battleManager.TurnStarted += OnTurnStarted;
            battleManager.BattleEnded += OnBattleEnded;
            
            StartDemo();
        }
        
        private void StartDemo()
        {
            GD.Print("\n╔═══════════════════════════════════════════════════╗");
            GD.Print("║  CORE COMMANDS DEMONSTRATION                   ║");
            GD.Print("║  Guard • Items • Escape                        ║");
            GD.Print("╚═══════════════════════════════════════════════════╝\n");
            
            // Create party
            var playerStats = new List<CharacterStats>
            {
                CreateHero("Dominic", 150, 80, 35, 25, 40),
                CreateHero("Echo Walker", 120, 100, 30, 20, 50),
                CreateHero("Aria", 100, 120, 25, 18, 45)
            };
            
            // Create enemies
            var enemyStats = new List<CharacterStats>
            {
                CreateEnemy("Goblin", 80, 30, 28, 15, 35),
                CreateEnemy("Orc", 120, 40, 35, 20, 25)
            };
            
            // NOT a boss battle - can escape!
            battleManager.InitializeBattle(playerStats, enemyStats, null, null, false, false);
            
            CallDeferred(nameof(DemonstrateCoreCommands));
        }
        
        private void DemonstrateCoreCommands()
        {
            GD.Print("\n═══ CORE COMMANDS DEMO ═══\n");
            
            // Demo 1: GUARD
            GD.Print("─── Demo 1: Guard Command ───");
            GD.Print("Dominic will guard and take reduced damage!\n");
            
            if (battleManager.IsPlayerTurn())
            {
                var dominic = battleManager.GetPlayerParty()[0];
                
                // Guard action
                var guardAction = new BattleAction(dominic, BattleActionType.Guard);
                GD.Print(">>> Dominic guards!");
                battleManager.ExecuteAction(guardAction);
                
                GD.Print($"    Guard state: {dominic.GuardState.IsGuarding}");
                GD.Print($"    Damage reduction: {dominic.GuardState.DamageReduction * 100}%");
                GD.Print($"    HP regen per turn: {dominic.GuardState.HPRegenPerTurn}\n");
            }
            
            // Simulate enemy hitting guarding character
            GD.Print(">>> Enemy attacks Dominic (who is guarding)...");
            if (battleManager.CurrentPhase != BattlePhase.Escaped)
            {
                // Skip to next turn for demo
                battleManager.StartNextTurn();
            }
            
            // Demo 2: ITEMS
            GD.Print("\n\n─── Demo 2: Item Usage ───");
            GD.Print("Using various items in battle!\n");
            
            // Damage Aria to show healing
            var aria = battleManager.GetPlayerParty()[2];
            aria.Stats.TakeDamage(50, ElementType.Physical);
            GD.Print($"Aria HP: {aria.Stats.CurrentHP}/{aria.Stats.MaxHP}");
            
            if (battleManager.IsPlayerTurn())
            {
                // Create a healing potion
                var healthPotion = CreateHealthPotion();
                
                var itemAction = new BattleAction(aria, BattleActionType.Item)
                {
                    ItemData = healthPotion
                };
                itemAction = itemAction.WithTargets(aria);
                
                GD.Print("\n>>> Using Health Potion on Aria!");
                battleManager.ExecuteAction(itemAction);
                GD.Print($"    Aria HP after: {aria.Stats.CurrentHP}/{aria.Stats.MaxHP}\n");
            }
            
            // Demo MP restoration
            var echo = battleManager.GetPlayerParty()[1];
            echo.Stats.ConsumeMP(30);
            GD.Print($"Echo Walker MP: {echo.Stats.CurrentMP}/{echo.Stats.MaxMP}");
            
            if (battleManager.IsPlayerTurn())
            {
                var manaPotion = CreateManaPotion();
                
                var itemAction = new BattleAction(echo, BattleActionType.Item)
                {
                    ItemData = manaPotion
                };
                itemAction = itemAction.WithTargets(echo);
                
                GD.Print("\n>>> Using Mana Potion on Echo Walker!");
                battleManager.ExecuteAction(itemAction);
                GD.Print($"    Echo Walker MP after: {echo.Stats.CurrentMP}/{echo.Stats.MaxMP}\n");
            }
            
            // Demo offensive item
            if (battleManager.IsPlayerTurn())
            {
                var bomb = CreateBomb();
                var goblin = battleManager.GetLivingEnemies()[0];
                
                var itemAction = new BattleAction(dominic, BattleActionType.Item)
                {
                    ItemData = bomb
                };
                itemAction = itemAction.WithTargets(goblin);
                
                GD.Print("\n>>> Dominic throws a Bomb at Goblin!");
                battleManager.ExecuteAction(itemAction);
            }
            
            // Demo 3: ESCAPE
            GD.Print("\n\n─── Demo 3: Escape Command ───");
            GD.Print("Attempting to flee from battle!\n");
            
            // Check if can escape
            bool canEscape = battleManager.CanEscape();
            int escapeChance = battleManager.GetEscapeChance();
            
            GD.Print($"Can escape: {canEscape}");
            GD.Print($"Escape chance: {escapeChance}%\n");
            
            if (battleManager.IsPlayerTurn() && canEscape)
            {
                var escapeAction = new BattleAction(dominic, BattleActionType.Escape);
                
                GD.Print(">>> Party attempts to escape!");
                var result = battleManager.ExecuteAction(escapeAction);
                
                if (!result.Success)
                {
                    GD.Print("\n>>> Failed! Trying again...");
                    
                    // Try again (with better odds)
                    if (battleManager.IsPlayerTurn())
                    {
                        int newChance = battleManager.GetEscapeChance();
                        GD.Print($"    New escape chance: {newChance}% (+10% from attempt)");
                        
                        var retryAction = new BattleAction(echo, BattleActionType.Escape);
                        battleManager.ExecuteAction(retryAction);
                    }
                }
            }
            
            GD.Print("\n\n═══════════════════════════════════════");
            GD.Print("   Core Commands Demo Complete!      ");
            GD.Print("═══════════════════════════════════════\n");
        }
        
        #region Helper Methods
        
        private CharacterStats CreateHero(string name, int hp, int mp, int atk, int def, int spd)
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
                    CriticalRate = 10,
                    CriticalDamageMultiplier = 200
                },
                ElementAffinities = new ElementAffinityData()
            };
            
            GD.Print($"Hero: {name} (HP:{hp}, SPD:{spd})");
            return stats;
        }
        
        private CharacterStats CreateEnemy(string name, int hp, int mp, int atk, int def, int spd)
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
                    CriticalRate = 5,
                    CriticalDamageMultiplier = 150
                },
                ElementAffinities = new ElementAffinityData()
            };
            
            GD.Print($"Enemy: {name} (HP:{hp}, SPD:{spd})");
            return stats;
        }
        
        private ConsumableData CreateHealthPotion()
        {
            return new ConsumableData
            {
                ItemId = "health_potion",
                DisplayName = "Health Potion",
                Description = "Restores 50 HP",
                RestoresHP = 50,
                ItemType = ItemType.Consumable
            };
        }
        
        private ConsumableData CreateManaPotion()
        {
            return new ConsumableData
            {
                ItemId = "mana_potion",
                DisplayName = "Mana Potion",
                Description = "Restores 30 MP",
                RestoresMP = 30,
                ItemType = ItemType.Consumable
            };
        }
        
        private ConsumableData CreateBomb()
        {
            return new ConsumableData
            {
                ItemId = "bomb",
                DisplayName = "Bomb",
                Description = "Deals 60 Fire damage",
                DamageAmount = 60,
                DamageElement = ElementType.Fire,
                ItemType = ItemType.Consumable,
                InflictsStatus = new Godot.Collections.Array<StatusEffect> { StatusEffect.Burn },
                StatusChance = 30,
                StatusDuration = 3
            };
        }
        
        #endregion
        
        #region Signal Handlers
        
        private void OnTurnStarted(string characterName)
        {
            GD.Print($"\n═══ {characterName}'s Turn ═══");
        }
        
        private void OnBattleEnded(bool victory)
        {
            string result = victory ? "⭐ VICTORY! ⭐" : "Escaped from battle!";
            GD.Print($"\n╔═══════════════════════════════════════╗");
            GD.Print($"║         {result}              ║");
            GD.Print($"╚═══════════════════════════════════════╝");
        }
        
        #endregion
    }
}