using Godot;
using EchoesAcrossTime.Combat;

namespace EchoesAcrossTime.Database
{
    public static class CharacterPresets
    {
        public static CharacterData CreateDominic(int level = 1)
        {
            level = Mathf.Clamp(level, CharacterStats.MIN_LEVEL, CharacterStats.MAX_LEVEL);
    
            var dominic = new CharacterData
            {
                CharacterId = "dominic",
                DisplayName = "Prince Dominic Valebran",
                Type = CharacterType.PlayableCharacter,
                Class = CharacterClass.PrismShadow,
                Description = "Prince of Valebran with shadow magic abilities",
                Level = level,
                MaxHP = 120,
                MaxMP = 60,
                Attack = 12,
                Defense = 10,
                MagicAttack = 18,
                MagicDefense = 15,
                Speed = 14,
                HPGrowthRate = 0.06f,
                MPGrowthRate = 0.07f,
                AttackGrowthRate = 0.03f,
                DefenseGrowthRate = 0.03f,
                MagicAttackGrowthRate = 0.05f,
                MagicDefenseGrowthRate = 0.04f,
                SpeedGrowthRate = 0.03f
            };
    
            dominic.ElementAffinities.SetAffinity(ElementType.Dark, ElementAffinity.Absorb);
            dominic.ElementAffinities.SetAffinity(ElementType.Light, ElementAffinity.Weak);
            dominic.ElementAffinities.SetAffinity(ElementType.Ice, ElementAffinity.Resist);
    
            // Battle stats
            dominic.BattleStats.CriticalRate = 15;
            dominic.BattleStats.EvasionRate = 10;
            dominic.BattleStats.PreemptiveStrikeRate = 20;
    
            return dominic;
        }
        
        public static CharacterData CreateEchoWalker(int level = 1)
        {
            level = Mathf.Clamp(level, CharacterStats.MIN_LEVEL, CharacterStats.MAX_LEVEL);
            
            var echo = new CharacterData
            {
                CharacterId = "echo_walker",
                DisplayName = "Echo Walker",
                Type = CharacterType.PlayableCharacter,
                Description = "Timeline-sensitive guide with reality manipulation",
                Level = level,
                MaxHP = 90,
                MaxMP = 80,
                Attack = 8,
                Defense = 8,
                MagicAttack = 20,
                MagicDefense = 18,
                Speed = 18,
                HPGrowthRate = 0.04f,
                MPGrowthRate = 0.08f,
                AttackGrowthRate = 0.02f,
                DefenseGrowthRate = 0.02f,
                MagicAttackGrowthRate = 0.06f,
                MagicDefenseGrowthRate = 0.05f,
                SpeedGrowthRate = 0.04f
            };
            
            echo.ElementAffinities.SetAffinity(ElementType.Thunder, ElementAffinity.Resist);
            echo.ElementAffinities.SetAffinity(ElementType.Earth, ElementAffinity.Weak);
            
            return echo;
        }
        
        public static CharacterData CreateShadowsMirror(int level = 1)
        {
            level = Mathf.Clamp(level, CharacterStats.MIN_LEVEL, CharacterStats.MAX_LEVEL);
            
            var mirror = new CharacterData
            {
                CharacterId = "shadows_mirror",
                DisplayName = "Shadow's Mirror",
                Type = CharacterType.PlayableCharacter,
                Description = "Redeemed corrupted light entity",
                Level = level,
                MaxHP = 100,
                MaxMP = 70,
                Attack = 10,
                Defense = 12,
                MagicAttack = 22,
                MagicDefense = 16,
                Speed = 12,
                HPGrowthRate = 0.05f,
                MPGrowthRate = 0.06f,
                AttackGrowthRate = 0.03f,
                DefenseGrowthRate = 0.04f,
                MagicAttackGrowthRate = 0.06f,
                MagicDefenseGrowthRate = 0.04f,
                SpeedGrowthRate = 0.03f
            };
            
            mirror.ElementAffinities.SetAffinity(ElementType.Light, ElementAffinity.Absorb);
            mirror.ElementAffinities.SetAffinity(ElementType.Dark, ElementAffinity.Weak);
            mirror.ElementAffinities.SetAffinity(ElementType.Fire, ElementAffinity.Resist);
            
            return mirror;
        }
        
        public static CharacterData CreateBasicEnemy(string name, int level = 1)
        {
            level = Mathf.Clamp(level, CharacterStats.MIN_LEVEL, CharacterStats.MAX_LEVEL);
    
            var enemy = new CharacterData
            {
                CharacterId = $"enemy_{name.ToLower().Replace(" ", "_")}",
                DisplayName = name,
                Type = CharacterType.Enemy,
                Level = level,
                MaxHP = 50 + (level * 10),
                MaxMP = 20 + (level * 5),
                Attack = 8 + level,
                Defense = 6 + level,
                MagicAttack = 8 + level,
                MagicDefense = 6 + level,
                Speed = 10 + level
            };
    
            // Setup rewards
            enemy.Rewards = new EnemyRewards
            {
                BaseExpReward = 10 + (level * 2),
                BaseGoldReward = 5 + level,
                ExpVariance = 0.1f,
                GoldVariance = 0.2f
            };
    
            // Add common drops
            var commonDrop = new DropItem
            {
                ItemId = "potion",
                DropChance = 30,
                MinQuantity = 1,
                MaxQuantity = 1
            };
            enemy.Rewards.CommonDrops.Add(commonDrop);
    
            // Battle stats
            enemy.BattleStats.AccuracyRate = 85;
            enemy.BattleStats.EvasionRate = 5;
    
            return enemy;
        }
        
        public static CharacterData CreateBoss(string name, int level = 10)
        {
            level = Mathf.Clamp(level, CharacterStats.MIN_LEVEL, CharacterStats.MAX_LEVEL);
    
            var boss = new CharacterData
            {
                CharacterId = $"boss_{name.ToLower().Replace(" ", "_")}",
                DisplayName = name,
                Type = CharacterType.Boss,
                IsBoss = true,
                Level = level,
                MaxHP = 200 + (level * 50),
                MaxMP = 100 + (level * 20),
                Attack = 15 + (level * 2),
                Defense = 12 + (level * 2),
                MagicAttack = 18 + (level * 2),
                MagicDefense = 15 + (level * 2),
                Speed = 12 + level
            };
    
            // Setup boss rewards
            boss.Rewards = new EnemyRewards
            {
                BaseExpReward = 100 + (level * 10),
                BaseGoldReward = 50 + (level * 5),
                IsBoss = true,
                BossExpMultiplier = 5,
                BossGoldMultiplier = 10
            };
    
            // Boss guaranteed drops
            boss.Rewards.GuaranteedDrop = new DropItem
            {
                ItemId = "boss_key",
                DropChance = 100,
                MinQuantity = 1,
                MaxQuantity = 1
            };
    
            // Rare drops
            var rareDrop = new DropItem
            {
                ItemId = "legendary_sword",
                DropChance = 10,
                MinQuantity = 1,
                MaxQuantity = 1
            };
            boss.Rewards.RareDrops.Add(rareDrop);
    
            // Battle stats - bosses are tough
            boss.BattleStats.AccuracyRate = 95;
            boss.BattleStats.CriticalRate = 20;
            boss.BattleStats.DeathResistance = 100;  // Immune to instant death
            boss.BattleStats.ImmuneToInstantDeath = true;
            boss.BattleStats.ParalysisResistance = 75;
            boss.BattleStats.SleepResistance = 100;
    
            return boss;
        }
    }
}