// Examples/SmartEnemySetup.cs
// Shows how to create enemies with smart AI behaviors
using Godot;
using EchoesAcrossTime.Combat;
using EchoesAcrossTime.Database;

namespace EchoesAcrossTime.Examples
{
    public partial class SmartEnemySetup : Node
    {
        /// <summary>
        /// Example 1: Tactical Mage - Exploits weaknesses and technicals
        /// </summary>
        public static AIPattern CreateTacticalMage()
        {
            var ai = new AIPattern
            {
                // Core Behavior
                BehaviorType = AIBehaviorType.Tactical,
                TargetPriority = TargetPriority.MostVulnerable,
                
                // Skill Usage
                PrefersMagic = true,
                SkillUsageRate = 80,
                
                // Smart AI Features - MAX EXPLOITATION
                LearnWeaknesses = true,
                ExploitTechnicals = true,
                UseDefensiveTactics = true,
                TechnicalPriority = 90,  // Prioritize technical combos
                WeaknessPriority = 85,   // Also prioritize weaknesses
                
                // Thresholds
                LowHPThreshold = 40,
                DefensiveThreshold = 60,
                
                // Preferred Skills (fire/ice spells for status setup)
                PreferredSkillIds = new Godot.Collections.Array<string> 
                { 
                    "agi",      // Fire spell (causes Burn)
                    "bufu",     // Ice spell (causes Freeze)
                    "zio"       // Thunder spell (technical with Burn)
                },
                
                // Emergency Skills
                EmergencySkillIds = new Godot.Collections.Array<string> 
                { 
                    "dia",      // Heal self
                    "recarm"    // Revive if available
                }
            };
            
            return ai;
        }

        /// <summary>
        /// Example 2: Defensive Tank - Guards allies, heals when needed
        /// </summary>
        public static AIPattern CreateDefensiveTank()
        {
            var ai = new AIPattern
            {
                // Core Behavior
                BehaviorType = AIBehaviorType.Defensive,
                TargetPriority = TargetPriority.HighestHP,
                
                // Defensive Focus
                Aggression = 20,
                Recklessness = 0,
                
                // Smart AI Features
                UseDefensiveTactics = true,
                LearnWeaknesses = false,  // Doesn't need to learn
                ExploitTechnicals = false,
                
                // Thresholds - Guards early
                LowHPThreshold = 50,
                DefensiveThreshold = 70,
                
                // Special Behaviors
                GuardsAllies = true,
                
                // Preferred Skills (heals and buffs)
                PreferredSkillIds = new Godot.Collections.Array<string> 
                { 
                    "dia",
                    "media",
                    "tarukaja",  // Attack buff
                    "rakukaja"   // Defense buff
                },
                
                EmergencySkillIds = new Godot.Collections.Array<string> 
                { 
                    "diarama",   // Strong heal
                    "mediarama"  // Party heal
                }
            };
            
            return ai;
        }

        /// <summary>
        /// Example 3: Berserker - Powerful but unpredictable
        /// </summary>
        public static AIPattern CreateBerserker()
        {
            var ai = new AIPattern
            {
                // Core Behavior
                BehaviorType = AIBehaviorType.Berserk,
                TargetPriority = TargetPriority.Random,
                
                // Maximum Aggression
                Aggression = 100,
                Recklessness = 90,
                SkillUsageRate = 70,
                
                // Smart AI Features - NO TACTICS
                LearnWeaknesses = false,
                ExploitTechnicals = false,
                UseDefensiveTactics = false,
                
                // Special Behaviors
                EnragesAtLowHP = true,  // Gets more dangerous
                
                // Thresholds
                LowHPThreshold = 20,
                DefensiveThreshold = 0,  // Never defends
                
                // No skill preferences - uses anything
                PreferredSkillIds = new Godot.Collections.Array<string>(),
                EmergencySkillIds = new Godot.Collections.Array<string>()
            };
            
            return ai;
        }

        /// <summary>
        /// Example 4: Boss with Turn Patterns - Predictable but challenging
        /// </summary>
        public static AIPattern CreatePatternBoss()
        {
            var ai = new AIPattern
            {
                // Core Behavior
                BehaviorType = AIBehaviorType.Balanced,
                TargetPriority = TargetPriority.Leader,
                
                // Smart AI
                LearnWeaknesses = true,
                ExploitTechnicals = true,
                UseDefensiveTactics = true,
                
                // Turn Pattern System
                HasTurnPattern = true,
                TurnPattern = new Godot.Collections.Array<string>
                {
                    "charge",      // Turn 1: Power up
                    "megidola",    // Turn 2: Heavy damage
                    "heat_riser",  // Turn 3: Buff self
                    "maziodyne",   // Turn 4: AOE attack
                    "diarahan"     // Turn 5: Full heal, then repeat
                },
                
                // Thresholds
                LowHPThreshold = 30,
                DefensiveThreshold = 50,
                
                // Emergency breaks pattern
                EmergencySkillIds = new Godot.Collections.Array<string>
                {
                    "megidolaon",  // Ultimate attack
                    "diarahan"     // Full heal
                }
            };
            
            return ai;
        }

        /// <summary>
        /// Example 5: Support Healer - Keeps team alive
        /// </summary>
        public static AIPattern CreateSupportHealer()
        {
            var ai = new AIPattern
            {
                // Core Behavior
                BehaviorType = AIBehaviorType.Supportive,
                TargetPriority = TargetPriority.LowestHP,
                
                // Low Aggression
                Aggression = 10,
                SkillUsageRate = 95,  // Almost always uses skills
                
                // Smart AI
                UseDefensiveTactics = true,
                LearnWeaknesses = false,
                ExploitTechnicals = false,
                
                // Thresholds
                LowHPThreshold = 40,
                DefensiveThreshold = 60,
                
                // Preferred Skills - Healing priority
                PreferredSkillIds = new Godot.Collections.Array<string>
                {
                    "media",       // Party heal
                    "diarama",     // Strong single heal
                    "recarm",      // Revive
                    "makakaja"     // Magic buff for allies
                },
                
                EmergencySkillIds = new Godot.Collections.Array<string>
                {
                    "mediarama",   // Strong party heal
                    "samarecarm"   // Full revive
                }
            };
            
            return ai;
        }

        /// <summary>
        /// Example 6: Technical Specialist - Master of combos
        /// </summary>
        public static AIPattern CreateTechnicalSpecialist()
        {
            var ai = new AIPattern
            {
                // Core Behavior
                BehaviorType = AIBehaviorType.Tactical,
                TargetPriority = TargetPriority.MostVulnerable,
                
                // Skill-focused
                PrefersMagic = true,
                SkillUsageRate = 100,
                
                // Smart AI - MAXIMUM TECHNICAL FOCUS
                LearnWeaknesses = true,
                ExploitTechnicals = true,
                UseDefensiveTactics = false,
                TechnicalPriority = 100,  // ALWAYS seek technicals
                WeaknessPriority = 95,
                
                // Aggression
                Aggression = 70,
                
                // Skills that cause status for technicals
                PreferredSkillIds = new Godot.Collections.Array<string>
                {
                    "agi",         // Burn
                    "mabufu",      // Freeze all
                    "zionga",      // Shock
                    "pulinpa",     // Confusion
                    "dormina"      // Sleep
                },
                
                EmergencySkillIds = new Godot.Collections.Array<string>
                {
                    "megidola"     // Raw damage if low
                }
            };
            
            return ai;
        }

        /// <summary>
        /// Example 7: Coward - Flees when threatened
        /// </summary>
        public static AIPattern CreateCoward()
        {
            var ai = new AIPattern
            {
                // Core Behavior
                BehaviorType = AIBehaviorType.Cowardly,
                TargetPriority = TargetPriority.Random,
                
                // Low stats
                Aggression = 30,
                Recklessness = 5,
                
                // Smart AI - Defensive only
                UseDefensiveTactics = true,
                LearnWeaknesses = false,
                ExploitTechnicals = false,
                
                // Will flee
                WillFlee = true,
                FleeThreshold = 40,  // Flees at 40% HP
                
                // Thresholds
                LowHPThreshold = 50,
                DefensiveThreshold = 70,
                
                EmergencySkillIds = new Godot.Collections.Array<string>
                {
                    "dia"  // Emergency heal before fleeing
                }
            };
            
            return ai;
        }

        /// <summary>
        /// Complete Enemy Setup Example
        /// </summary>
        public static void SetupSmartEnemy(CharacterData enemyData, string enemyType)
        {
            // Assign appropriate AI based on enemy type
            enemyData.AIBehavior = enemyType switch
            {
                "Mage" => CreateTacticalMage(),
                "Tank" => CreateDefensiveTank(),
                "Berserker" => CreateBerserker(),
                "Boss" => CreatePatternBoss(),
                "Healer" => CreateSupportHealer(),
                "Technical" => CreateTechnicalSpecialist(),
                "Coward" => CreateCoward(),
                _ => CreateTacticalMage()  // Default
            };

            GD.Print($"[Setup] {enemyData.DisplayName} configured with {enemyType} AI");
        }
    }
}