using Godot;
using System.Collections.Generic;
using EchoesAcrossTime.Combat;

namespace EchoesAcrossTime.Testing
{
    /// <summary>
    /// Demonstrates Phase 2 advanced battle features:
    /// - Baton Pass system
    /// - Technical Damage
    /// - Showtime Attacks
    /// </summary>
    public partial class Phase2BattleTest : Node
    {
        private BattleManager battleManager;
        
        public override void _Ready()
        {
            // Create battle manager
            battleManager = new BattleManager();
            AddChild(battleManager);
            
            // Connect to battle signals
            battleManager.TurnStarted += OnTurnStarted;
            battleManager.WeaknessHit += OnWeaknessHit;
            battleManager.OneMoreTriggered += OnOneMoreTriggered;
            battleManager.BatonPassExecuted += OnBatonPassExecuted;
            battleManager.TechnicalDamage += OnTechnicalDamage;
            battleManager.ShowtimeTriggered += OnShowtimeTriggered;
            battleManager.BattleEnded += OnBattleEnded;
            
            // Start demo battle
            StartAdvancedBattle();
        }
        
        private void StartAdvancedBattle()
        {
            GD.Print("\n╔═════════════════════════════════════════════════════╗");
            GD.Print("║  Phase 2: Advanced Battle System Demonstration   ║");
            GD.Print("║  Features: Baton Pass • Technical • Showtime     ║");
            GD.Print("╚═════════════════════════════════════════════════════╝\n");
            
            // Create player party
            var playerStats = new List<CharacterStats>
            {
                CreateHero("Dominic", 180, 90, 40, 28, 42),
                CreateHero("Echo Walker", 140, 110, 35, 25, 48),
                CreateHero("Aria", 120, 130, 30, 30, 52)
            };
            
            // Create enemies with status vulnerabilities
            var enemyStats = new List<CharacterStats>
            {
                CreateEnemy("Flame Ogre", 150, 50, 38, 20, 22, ElementType.Fire, ElementType.Ice),
                CreateEnemy("Storm Elemental", 100, 80, 32, 18, 40, ElementType.Thunder, ElementType.Earth)
            };
            
            // Create showtime attacks
            var showtimes = new List<ShowtimeAttackData>
            {
                CreateShowtime("Dominic", "Echo Walker", "Twilight Judgment", 
                    "Light and darkness converge!", ElementType.Light, 400, 3.0f),
                CreateShowtime("Aria", "Echo Walker", "Frozen Eternity",
                    "Time freezes in endless ice!", ElementType.Ice, 380, 2.8f)
            };
            
            // Initialize battle with showtimes
            battleManager.InitializeBattle(playerStats, enemyStats, showtimes);
            
            // Simulate advanced battle
            CallDeferred(nameof(ExecuteAdvancedTurns));
        }
        
        private void ExecuteAdvancedTurns()
        {
            GD.Print("\n═══ PHASE 2 FEATURE DEMONSTRATION ═══\n");
            
            // DEMO 1: Technical Damage with Burn
            GD.Print("\n─── DEMO 1: Technical Damage ───");
            GD.Print("Strategy: Burn enemy, then hit with Thunder for Technical!\n");
            
            // Turn 1: Dominic applies Burn
            if (battleManager.IsPlayerTurn())
            {
                var dominic = battleManager.GetPlayerParty()[0];
                var flameOgre = battleManager.GetLivingEnemies()[0];
                
                // Create fire skill that applies burn
                var fireSkill = CreateStatusSkill("Inferno", ElementType.Fire, 60, 10, StatusEffect.Burn);
                var action = new BattleAction(dominic, BattleActionType.Skill)
                    .WithSkill(fireSkill)
                    .WithTargets(flameOgre);
                
                GD.Print(">>> Dominic uses Inferno to apply Burn <<<");
                battleManager.ExecuteAction(action);
            }
            
            // Turn 2: Echo Walker hits burned enemy with Thunder (TECHNICAL!)
            if (battleManager.IsPlayerTurn())
            {
                var echo = battleManager.GetPlayerParty()[1];
                var flameOgre = battleManager.GetLivingEnemies()[0];
                
                var thunderSkill = CreateSkill("Ziodyne", ElementType.Thunder, 90, 15);
                var action = new BattleAction(echo, BattleActionType.Skill)
                    .WithSkill(thunderSkill)
                    .WithTargets(flameOgre);
                
                GD.Print("\n>>> Echo Walker hits burned foe with Thunder (TECHNICAL!) <<<");
                battleManager.ExecuteAction(action);
            }
            
            // DEMO 2: Baton Pass Chain
            GD.Print("\n\n─── DEMO 2: Baton Pass Chain ───");
            GD.Print("Strategy: Hit weakness → Baton Pass → Hit weakness again → Baton Pass!\n");
            
            // Turn 3: Aria hits weakness and gets One More
            if (battleManager.IsPlayerTurn())
            {
                var aria = battleManager.GetPlayerParty()[2];
                var flameOgre = battleManager.GetLivingEnemies()[0];
                
                var iceSkill = CreateSkill("Bufudyne", ElementType.Ice, 85, 14);
                var action = new BattleAction(aria, BattleActionType.Skill)
                    .WithSkill(iceSkill)
                    .WithTargets(flameOgre);
                
                GD.Print(">>> Aria hits weakness with Ice <<<");
                var result = battleManager.ExecuteAction(action);
                
                // If got One More, demonstrate baton pass
                if (battleManager.CanBatonPass())
                {
                    GD.Print("\n>>> Aria can Baton Pass! Passing to Dominic... <<<");
                    var dominic = battleManager.GetPlayerParty()[0];
                    battleManager.ExecuteBatonPass(dominic);
                    
                    // Dominic acts with Baton Pass bonus
                    if (battleManager.IsPlayerTurn())
                    {
                        var stormElemental = battleManager.GetLivingEnemies().Count > 1 
                            ? battleManager.GetLivingEnemies()[1] 
                            : battleManager.GetLivingEnemies()[0];
                        
                        var darkSkill = CreateSkill("Maeigaon", ElementType.Dark, 95, 16);
                        var batonAction = new BattleAction(dominic, BattleActionType.Skill)
                            .WithSkill(darkSkill)
                            .WithTargets(stormElemental);
                        
                        GD.Print("\n>>> Dominic attacks with Baton Pass Lv.1 bonuses! <<<");
                        battleManager.ExecuteAction(batonAction);
                        
                        // If THAT hit weakness, chain baton pass!
                        if (battleManager.CanBatonPass())
                        {
                            GD.Print("\n>>> Another One More! Passing to Echo Walker... <<<");
                            var echo = battleManager.GetPlayerParty()[1];
                            battleManager.ExecuteBatonPass(echo);
                            
                            // Echo acts with Baton Pass Lv.2 (even stronger!)
                            if (battleManager.IsPlayerTurn())
                            {
                                var lightSkill = CreateSkill("Hamaon", ElementType.Light, 100, 18);
                                var finalAction = new BattleAction(echo, BattleActionType.Skill)
                                    .WithSkill(lightSkill)
                                    .WithTargets(battleManager.GetLivingEnemies()[0]);
                                
                                GD.Print("\n>>> Echo Walker attacks with Baton Pass Lv.2 bonuses! <<<");
                                GD.Print("    (Expect MASSIVE damage from x2.0 multiplier!)");
                                battleManager.ExecuteAction(finalAction);
                            }
                        }
                    }
                }
            }
            
            // DEMO 3: Showtime Attack
            GD.Print("\n\n─── DEMO 3: Showtime Attack ───");
            GD.Print("Strategy: Execute devastating duo attack!\n");
            
            var availableShowtimes = battleManager.GetAvailableShowtimes();
            if (availableShowtimes.Count > 0)
            {
                var showtime = availableShowtimes[0];
                GD.Print($">>> Executing Showtime: {showtime.AttackName} <<<");
                battleManager.ExecuteShowtime(showtime);
            }
            
            GD.Print("\n\n═══════════════════════════════════════");
            GD.Print("   Phase 2 Demonstration Complete!   ");
            GD.Print("═══════════════════════════════════════\n");
        }
        
        #region Helper Methods
        
        private CharacterStats CreateHero(string name, int hp, int mp, int atk, int def, int spd)
        {
            var stats = new CharacterStats
            {
                CharacterName = name,
                Level = 15,
                MaxHP = hp,
                CurrentHP = hp,
                MaxMP = mp,
                CurrentMP = mp,
                Attack = atk,
                Defense = def,
                MagicAttack = atk + 8,
                MagicDefense = def + 8,
                Speed = spd,
                BattleStats = new BattleStats
                {
                    AccuracyRate = 95,
                    CriticalRate = 12,
                    CriticalDamageMultiplier = 200
                },
                ElementAffinities = new ElementAffinityData()
            };
            
            GD.Print($"Hero: {name} (HP:{hp}, ATK:{atk}, SPD:{spd})");
            return stats;
        }
        
        private CharacterStats CreateEnemy(string name, int hp, int mp, int atk, int def, int spd, 
            ElementType element, ElementType weakness)
        {
            var stats = new CharacterStats
            {
                CharacterName = name,
                Level = 12,
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
            
            stats.ElementAffinities.SetAffinity(weakness, ElementAffinity.Weak);
            if (element != ElementType.None)
            {
                stats.ElementAffinities.SetAffinity(element, ElementAffinity.Resist);
            }
            
            GD.Print($"Enemy: {name} (HP:{hp}, Weak:{weakness})");
            return stats;
        }
        
        private SkillData CreateSkill(string name, ElementType element, int power, int mpCost)
        {
            return new SkillData
            {
                SkillId = name.ToLower(),
                DisplayName = name,
                Element = element,
                BasePower = power,
                MPCost = mpCost,
                Accuracy = 95,
                DamageType = DamageType.Magical,
                CriticalBonus = 5
            };
        }
        
        private SkillData CreateStatusSkill(string name, ElementType element, int power, int mpCost, StatusEffect status)
        {
            var skill = CreateSkill(name, element, power, mpCost);
            skill.InflictsStatuses = new Godot.Collections.Array<StatusEffect> { status };
            skill.StatusChances = new Godot.Collections.Array<int> { 100 }; // Always applies for demo
            return skill;
        }
        
        private ShowtimeAttackData CreateShowtime(string char1, string char2, string name, 
            string flavorText, ElementType element, int power, float multiplier)
        {
            var showtime = new ShowtimeAttackData
            {
                Character1Id = char1,
                Character2Id = char2,
                AttackName = name,
                FlavorText = flavorText,
                BasePower = power,
                Element = element,
                DamageMultiplier = multiplier,
                HitsAllEnemies = true,
                CriticalChance = 50,
                TriggerChance = 100, // 100% for demo
                CooldownTurns = 3
            };
            
            GD.Print($"Showtime: {name} ({char1} + {char2})");
            return showtime;
        }
        
        #endregion
        
        #region Signal Handlers
        
        private void OnTurnStarted(string characterName)
        {
            GD.Print($"\n═══ {characterName}'s Turn ═══");
        }
        
        private void OnWeaknessHit(string attackerName, string targetName)
        {
            GD.Print($"💥 {attackerName} hit {targetName}'s WEAKNESS!");
        }
        
        private void OnOneMoreTriggered(string characterName)
        {
            GD.Print($"⭐ {characterName} gets ONE MORE!");
        }
        
        private void OnBatonPassExecuted(string fromChar, string toChar, int passLevel)
        {
            GD.Print($"\n★ BATON PASS! ★");
            GD.Print($"   {fromChar} → {toChar}");
            GD.Print($"   Baton Pass Level: {passLevel}");
        }
        
        private void OnTechnicalDamage(string attackerName, string targetName, string comboType)
        {
            GD.Print($"\n★★★ TECHNICAL DAMAGE! ★★★");
            GD.Print($"   {attackerName} exploited {targetName}'s status!");
            GD.Print($"   Combo: {comboType}");
        }
        
        private void OnShowtimeTriggered(string showtimeName, string char1, string char2)
        {
            GD.Print($"\n✨ SHOWTIME AVAILABLE! ✨");
            GD.Print($"   {showtimeName}");
            GD.Print($"   {char1} & {char2}");
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