// Combat/AIIntegrationHelper.cs
// Shows how to integrate the enhanced AI system with BattleManager
using Godot;
using System.Collections.Generic;
using System.Linq;

namespace EchoesAcrossTime.Combat
{
    /// <summary>
    /// Helper to integrate smart AI with BattleManager
    /// Add this to your BattleManager or create as separate node
    /// </summary>
    public partial class AIIntegrationHelper : Node
    {
        private BattleManager battleManager;
        private TechnicalDamageSystem technicalSystem;

        public override void _Ready()
        {
            // Get references
            battleManager = GetNode<BattleManager>("../BattleManager");
            
            // Connect to turn started signal
            battleManager.TurnStarted += OnTurnStarted;
        }

        private void OnTurnStarted(string characterName)
        {
            // Check if it's an enemy turn
            if (!battleManager.IsPlayerTurn())
            {
                ExecuteEnemyAI();
            }
        }

        /// <summary>
        /// Main enemy AI execution - ENHANCED VERSION
        /// </summary>
        private void ExecuteEnemyAI()
        {
            var enemy = battleManager.CurrentActor;
            if (enemy == null || !enemy.CanAct())
            {
                battleManager.StartNextTurn();
                return;
            }

            // Get enemy's AI pattern from their CharacterData
            var enemyData = GetEnemyCharacterData(enemy.Stats.CharacterId);
            
            if (enemyData?.AIBehavior == null)
            {
                // Fallback to simple AI
                ExecuteSimpleAI(enemy);
                return;
            }

            // INITIALIZE AI WITH TECHNICAL SYSTEM
            if (technicalSystem == null)
            {
                technicalSystem = new TechnicalDamageSystem();
            }
            enemyData.AIBehavior.Initialize(technicalSystem);

            // Get battle context
            var allies = battleManager.GetEnemyParty()
                .Select(e => e.Stats)
                .ToList();
            var targets = battleManager.GetLivingAllies()
                .Select(p => p.Stats)
                .ToList();

            if (targets.Count == 0)
            {
                battleManager.StartNextTurn();
                return;
            }

            // LET AI MAKE SMART DECISION
            var decision = enemyData.AIBehavior.DecideAction(
                enemy.Stats,
                allies,
                targets
            );

            // Log AI reasoning
            if (!string.IsNullOrEmpty(decision.Reasoning))
            {
                GD.Print($"[AI] {enemy.Stats.CharacterName} -> {decision.Reasoning}");
            }

            // Convert AI decision to battle action
            var action = ConvertAIDecisionToAction(decision, enemy);
            
            if (action != null)
            {
                battleManager.ExecuteAction(action);
            }
            else
            {
                // Fallback to basic attack - find BattleMember for first target
                var targetMember = FindBattleMemberFromStats(targets[0]);
                if (targetMember != null)
                {
                    var fallbackAction = new BattleAction(enemy, BattleActionType.Attack)
                        .WithTargets(targetMember);
                    battleManager.ExecuteAction(fallbackAction);
                }
                else
                {
                    battleManager.StartNextTurn();
                }
            }
        }

        /// <summary>
        /// Convert AI decision to executable BattleAction
        /// </summary>
        private BattleAction ConvertAIDecisionToAction(AIDecision decision, BattleMember actor)
        {
            if (decision == null || decision.ActionType == AIActionType.None)
                return null;

            switch (decision.ActionType)
            {
                case AIActionType.Attack:
                    var attackTarget = FindBattleMemberFromStats(decision.Target);
                    if (attackTarget == null) return null;
                    return new BattleAction(actor, BattleActionType.Attack)
                        .WithTargets(attackTarget);

                case AIActionType.UseSkill:
                    if (decision.SelectedSkill == null || decision.Target == null)
                        return null;
                    
                    // Handle multi-target skills
                    BattleMember[] skillTargets;
                    if (decision.SelectedSkill.Target == SkillTarget.AllEnemies)
                    {
                        skillTargets = battleManager.GetLivingAllies().ToArray();
                    }
                    else if (decision.SelectedSkill.Target == SkillTarget.AllAllies)
                    {
                        skillTargets = battleManager.GetEnemyParty()
                            .Where(e => e.Stats.IsAlive)
                            .ToArray();
                    }
                    else
                    {
                        skillTargets = new[] { FindBattleMemberFromStats(decision.Target) };
                    }

                    return new BattleAction(actor, BattleActionType.Skill)
                        .WithSkill(decision.SelectedSkill)
                        .WithTargets(skillTargets);

                case AIActionType.Defend:
                    return new BattleAction(actor, BattleActionType.Guard);

                case AIActionType.Flee:
                    // Handle flee if your system supports it
                    GD.Print($"{actor.Stats.CharacterName} attempts to flee!");
                    return null;

                case AIActionType.Item:
                    // Handle items if AI can use them
                    return null;

                default:
                    return null;
            }
        }

        /// <summary>
        /// Find BattleMember from CharacterStats
        /// </summary>
        private BattleMember FindBattleMemberFromStats(CharacterStats stats)
        {
            if (stats == null) return null;

            // Check player party
            var member = battleManager.GetPlayerParty()
                .FirstOrDefault(m => m.Stats == stats);
            
            if (member != null) return member;

            // Check enemy party
            return battleManager.GetEnemyParty()
                .FirstOrDefault(m => m.Stats == stats);
        }

        /// <summary>
        /// Get enemy CharacterData (implement based on your data management)
        /// </summary>
        private Database.CharacterData GetEnemyCharacterData(string characterId)
        {
            // Example: Load from ResourceLoader
            string path = $"res://Data/Characters/{characterId}.tres";
            
            if (ResourceLoader.Exists(path))
            {
                return ResourceLoader.Load<Database.CharacterData>(path);
            }

            GD.PrintErr($"Could not find CharacterData for {characterId}");
            return null;
        }

        /// <summary>
        /// Simple fallback AI (no AIPattern assigned)
        /// </summary>
        private void ExecuteSimpleAI(BattleMember enemy)
        {
            // Get living allies as BattleMember objects
            var targetMembers = battleManager.GetLivingAllies().ToList();
            if (targetMembers.Count == 0)
            {
                battleManager.StartNextTurn();
                return;
            }

            var skills = enemy.Stats.Skills?.GetUsableSkills(enemy.Stats);
            
            if (skills != null && skills.Count > 0 && GD.Randf() < 0.6f)
            {
                // Use random skill
                var skill = skills[GD.RandRange(0, skills.Count - 1)];
                var target = targetMembers[GD.RandRange(0, targetMembers.Count - 1)];

                var action = new BattleAction(enemy, BattleActionType.Skill)
                    .WithSkill(skill)
                    .WithTargets(target);
                
                battleManager.ExecuteAction(action);
            }
            else
            {
                // Basic attack
                var target = targetMembers[GD.RandRange(0, targetMembers.Count - 1)];
                var action = new BattleAction(enemy, BattleActionType.Attack)
                    .WithTargets(target);
                
                battleManager.ExecuteAction(action);
            }
        }
    }
}