// Combat/Advanced/AdvancedAIIntegration.cs
// Integrates Advanced AI with your existing BattleManager
using Godot;
using System.Collections.Generic;
using System.Linq;

namespace EchoesAcrossTime.Combat.Advanced
{
    /// <summary>
    /// Extension to AIIntegrationHelper for advanced AI features
    /// </summary>
    public partial class AdvancedAIIntegration : Node
    {
        private BattleManager battleManager;
        private Dictionary<string, AdvancedAIPattern> advancedPatterns;
        private Dictionary<string, AIRole> characterRoles;
        private int globalTurnCount = 0;
        
        public void Initialize(BattleManager manager)
        {
            battleManager = manager;
            advancedPatterns = new Dictionary<string, AdvancedAIPattern>();
            characterRoles = new Dictionary<string, AIRole>();
            
            // Subscribe to battle events for learning
            battleManager.ActionExecuted += OnActionExecuted;
            battleManager.TurnStarted += (member) => { globalTurnCount++; };
            battleManager.BattleEnded += OnBattleEnded;
        }
        
        /// <summary>
        /// Register an advanced AI pattern for an enemy
        /// </summary>
        public void RegisterAdvancedAI(string characterId, AdvancedAIPattern pattern)
        {
            advancedPatterns[characterId] = pattern;
            GD.Print($"[ADVANCED AI] Registered for {characterId}");
            
            // Determine role based on stats/skills
            var role = DetermineRole(characterId);
            characterRoles[characterId] = role;
            GD.Print($"  Role: {role}");
        }
        
        /// <summary>
        /// Make an advanced AI decision
        /// </summary>
        public AIDecision MakeAdvancedDecision(BattleMember actor, List<BattleMember> allies, List<BattleMember> enemies)
        {
            if (!advancedPatterns.ContainsKey(actor.Stats.CharacterId))
            {
                GD.PrintErr($"No advanced AI registered for {actor.Stats.CharacterId}");
                return new AIDecision { ActionType = AIActionType.None };
            }
            
            var pattern = advancedPatterns[actor.Stats.CharacterId];
            
            // Convert BattleMembers to CharacterStats lists
            var allyStats = allies.Select(a => a.Stats).ToList();
            var enemyStats = enemies.Select(e => e.Stats).ToList();
            
            // Make decision using advanced AI
            return pattern.DecideAction(actor.Stats, allyStats, enemyStats);
        }
        
        /// <summary>
        /// Convert AI decision to battle action
        /// </summary>
        public BattleAction ConvertToAction(AIDecision decision, BattleMember actor, List<BattleMember> allMembers)
        {
            if (decision == null || decision.ActionType == AIActionType.None)
                return null;
            
            switch (decision.ActionType)
            {
                case AIActionType.Attack:
                    var attackTarget = FindMemberFromStats(decision.Target, allMembers);
                    if (attackTarget == null) return null;
                    return new BattleAction(actor, BattleActionType.Attack)
                        .WithTargets(attackTarget);
                
                case AIActionType.UseSkill:
                    if (decision.SelectedSkill == null || decision.Target == null)
                        return null;
                    
                    BattleMember[] skillTargets;
                    if (decision.SelectedSkill.Target == SkillTarget.AllEnemies)
                    {
                        skillTargets = allMembers.Where(m => 
                            !m.IsPlayerControlled && m.Stats.IsAlive).ToArray();
                    }
                    else if (decision.SelectedSkill.Target == SkillTarget.AllAllies)
                    {
                        skillTargets = allMembers.Where(m => 
                            m.IsPlayerControlled && m.Stats.IsAlive).ToArray();
                    }
                    else
                    {
                        skillTargets = new[] { FindMemberFromStats(decision.Target, allMembers) };
                    }
                    
                    return new BattleAction(actor, BattleActionType.Skill)
                        .WithSkill(decision.SelectedSkill)
                        .WithTargets(skillTargets);
                
                case AIActionType.Defend:
                    return new BattleAction(actor, BattleActionType.Guard);
                
                case AIActionType.Flee:
                    GD.Print($"{actor.Stats.CharacterName} attempts to flee!");
                    return null;
                
                default:
                    return null;
            }
        }
        
        /// <summary>
        /// Event handlers for learning
        /// </summary>
        private void OnActionExecuted(string actorName, string skillName, int damage, bool weakness, bool critical)
        {
            // Track player actions for learning
            foreach (var pattern in advancedPatterns.Values)
            {
                if (pattern.LearnsPlayerPatterns)
                {
                    // Determine action type
                    string actionType = skillName == "Attack" ? "Attack" : "Skill";
                    pattern.RecordPlayerAction(actionType, skillName);
                }
            }
        }
        
        private void OnBattleEnded(bool victory)
        {
            // Update adaptive difficulty for all patterns
            foreach (var pattern in advancedPatterns.Values)
            {
                if (pattern.AdaptsToPlayerSkill)
                {
                    if (victory)
                    {
                        GD.Print($"[ADAPTIVE] Player won - adjusting difficulty");
                    }
                    else
                    {
                        GD.Print($"[ADAPTIVE] Player lost - adjusting difficulty");
                    }
                }
            }
        }
        
        /// <summary>
        /// Determine AI role from character stats/skills
        /// </summary>
        private AIRole DetermineRole(string characterId)
        {
            var member = battleManager.GetEnemyParty()
                .FirstOrDefault(e => e.Stats.CharacterId == characterId);
            
            if (member == null) return AIRole.Hybrid;
            
            var stats = member.Stats;
            var skills = stats.Skills?.GetEquippedSkills() ?? new List<SkillData>();
            
            // Check for healing skills
            bool hasHealing = skills.Any(s => s.DisplayName.ToLower().Contains("heal"));
            if (hasHealing && stats.Defense > stats.Attack)
                return AIRole.Healer;
            
            // Check for high defense
            if (stats.Defense > stats.Attack * 1.5f && stats.MaxHP > 200)
                return AIRole.Tank;
            
            // Check for support skills
            bool hasBuffs = skills.Any(s => s.DisplayName.ToLower().Contains("buff") || 
                                           s.DisplayName.ToLower().Contains("kaja"));
            if (hasBuffs && stats.MagicAttack > stats.Attack)
                return AIRole.Support;
            
            // High damage dealers
            if (stats.Attack > stats.Defense * 1.3f || stats.MagicAttack > stats.Defense * 1.3f)
                return AIRole.DPS;
            
            return AIRole.Hybrid;
        }
        
        private BattleMember FindMemberFromStats(CharacterStats stats, List<BattleMember> members)
        {
            return members.FirstOrDefault(m => m.Stats == stats);
        }
        
        /// <summary>
        /// Get all coordinated actions for current turn
        /// </summary>
        public List<BattleAction> GetCoordinatedActions()
        {
            // Future: Implement synchronized attacks
            return new List<BattleAction>();
        }
        
        /// <summary>
        /// Check if any AI wants to coordinate
        /// </summary>
        public bool HasPendingCoordination()
        {
            return advancedPatterns.Values.Any(p => 
                p.CoordinatesWithAllies && 
                p.SynchronizedAttacks);
        }
    }
}