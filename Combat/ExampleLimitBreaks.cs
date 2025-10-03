using Godot;
using EchoesAcrossTime.Combat;

namespace EchoesAcrossTime.Database
{
    /// <summary>
    /// Factory class to create example limit breaks for main characters
    /// </summary>
    public static class ExampleLimitBreaks
    {
        /// <summary>
        /// Dominic's Limit Break: Reality Tear
        /// Massive dark damage + time stop
        /// </summary>
        public static LimitBreakData CreateRealityTear()
        {
            var limitBreak = new LimitBreakData
            {
                LimitBreakId = "reality_tear",
                DisplayName = "Reality Tear",
                Description = "Tears through the fabric of reality itself, dealing catastrophic Dark damage and freezing time.",
                CharacterId = "Dominic",
                Type = LimitBreakType.Offensive,
                FlavorText = "The boundaries between worlds shatter at his command!",
                
                // Damage - Very powerful single target
                BasePower = 600,
                PowerMultiplier = 4.5f,
                Element = ElementType.Dark,
                HitsAllEnemies = false, // Single target nuke
                IgnoresDefense = true, // Bypasses all defense
                CriticalBonus = 30,
                HitCount = 1,
                
                // Special effects
                InstantKillBelow = true,
                InstantKillThreshold = 0.25f, // Kills if under 25% HP
                PiercesImmunity = true, // Even dark-immune enemies take damage
                
                // Status effects
                InflictsStatuses = new Godot.Collections.Array<StatusEffect>
                {
                    StatusEffect.Stun
                },
                StatusChance = 100,
                StatusDuration = 2,
                
                // Unique mechanic - Time Stop
                StopsTime = true,
                TimeStopDuration = 1, // Enemy skips 1 turn
                GrantsExtraTurn = false,
                
                // VFX
                VFXColor = new Color(0.5f, 0, 0.8f), // Dark purple
                AnimationDuration = 3.0f
            };
            
            return limitBreak;
        }
        
        /// <summary>
        /// Echo Walker's Limit Break: Temporal Convergence
        /// Multi-hit all enemies with time manipulation
        /// </summary>
        public static LimitBreakData CreateTemporalConvergence()
        {
            var limitBreak = new LimitBreakData
            {
                LimitBreakId = "temporal_convergence",
                DisplayName = "Temporal Convergence",
                Description = "Summons echoes from multiple timelines to strike all enemies repeatedly.",
                CharacterId = "Echo Walker",
                Type = LimitBreakType.Offensive,
                FlavorText = "Past, present, and future converge as one!",
                
                // Damage - Multi-hit AoE
                BasePower = 400,
                PowerMultiplier = 3.5f,
                Element = ElementType.Light,
                HitsAllEnemies = true, // Hits all enemies
                IgnoresDefense = false,
                CriticalBonus = 40,
                HitCount = 5, // Hits 5 times!
                
                // Special effects
                InstantKillBelow = false,
                DispelsBuffs = true, // Removes enemy buffs
                PiercesImmunity = false,
                
                // Status effects
                InflictsStatuses = new Godot.Collections.Array<StatusEffect>
                {
                    StatusEffect.Blind
                },
                StatusChance = 60,
                StatusDuration = 3,
                
                // Unique mechanic
                GrantsExtraTurn = true, // Gets another turn!
                StopsTime = false,
                
                // VFX
                VFXColor = new Color(1f, 1f, 0.8f), // Bright light
                AnimationDuration = 3.5f
            };
            
            return limitBreak;
        }
        
        /// <summary>
        /// Aria's Limit Break: Frozen Eternity
        /// Freezes time and space, massive ice damage
        /// </summary>
        public static LimitBreakData CreateFrozenEternity()
        {
            var limitBreak = new LimitBreakData
            {
                LimitBreakId = "frozen_eternity",
                DisplayName = "Frozen Eternity",
                Description = "Freezes all enemies in time, dealing massive Ice damage.",
                CharacterId = "Aria",
                Type = LimitBreakType.Hybrid,
                FlavorText = "Time itself crystallizes into ice!",
                
                // Damage - AoE with freeze
                BasePower = 450,
                PowerMultiplier = 3.8f,
                Element = ElementType.Ice,
                HitsAllEnemies = true,
                IgnoresDefense = false,
                CriticalBonus = 35,
                HitCount = 1,
                
                // Status effects - Guaranteed freeze
                InflictsStatuses = new Godot.Collections.Array<StatusEffect>
                {
                    StatusEffect.Freeze
                },
                StatusChance = 100,
                StatusDuration = 2,
                
                // Support - Also heals party
                HealPercent = 0.3f, // 30% HP heal
                FullHealParty = false,
                
                // Buffs
                DefenseBuff = 30, // +30% defense
                BuffsEntireParty = true,
                BuffDuration = 3,
                
                // VFX
                VFXColor = new Color(0.5f, 0.8f, 1f), // Ice blue
                AnimationDuration = 3.0f
            };
            
            return limitBreak;
        }
        
        /// <summary>
        /// DUO Limit Break: Dominic + Echo Walker
        /// Eclipse of Eternity - Dark and Light collide
        /// </summary>
        public static LimitBreakData CreateEclipseOfEternity()
        {
            var limitBreak = new LimitBreakData
            {
                LimitBreakId = "eclipse_of_eternity",
                DisplayName = "Eclipse of Eternity",
                Description = "Dark and Light merge in a cataclysmic explosion.",
                CharacterId = "Dominic",
                Type = LimitBreakType.Offensive,
                FlavorText = "When darkness and light become one, reality itself trembles!",
                
                // DUO SPECIFIC
                IsDuoLimitBreak = true,
                RequiredPartnerId = "Echo Walker",
                DuoPowerBonus = 1.8f, // 80% extra damage!
                
                // Damage - Absolutely devastating
                BasePower = 700,
                PowerMultiplier = 5.0f, // Then x1.8 from duo = 9x total!
                Element = ElementType.None, // Non-elemental
                HitsAllEnemies = true,
                IgnoresDefense = true,
                CriticalBonus = 50,
                HitCount = 1,
                
                // Special effects
                InstantKillBelow = true,
                InstantKillThreshold = 0.4f, // Kills under 40%!
                PiercesImmunity = true,
                DispelsBuffs = true,
                
                // Mechanics
                StopsTime = true,
                TimeStopDuration = 2, // 2 turn freeze!
                
                // VFX
                VFXColor = new Color(0.8f, 0.5f, 1f), // Purple-white
                AnimationDuration = 4.0f
            };
            
            return limitBreak;
        }
    }
}