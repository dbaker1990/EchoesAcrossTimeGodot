// Combat/AIPattern.cs - Enhanced with Technical Awareness & Smart Tactics
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EchoesAcrossTime.Combat
{
    public enum AIBehaviorType
    {
        Aggressive,     // Focuses on dealing damage
        Defensive,      // Uses healing/buffs
        Balanced,       // Mix of offense and defense
        Tactical,       // Exploits weaknesses
        Berserk,        // Random but powerful
        Supportive,     // Buffs allies, debuffs enemies
        Cowardly        // Runs away at low HP
    }

    public enum TargetPriority
    {
        LowestHP,
        HighestHP,
        LowestDefense,
        HighestThreat,
        Random,
        Healer,
        Mage,
        Weakest,
        Leader,
        MostVulnerable  // NEW: Considers status + weaknesses
    }

    [GlobalClass]
    public partial class AIPattern : Resource
    {
        [ExportGroup("Behavior")]
        [Export] public AIBehaviorType BehaviorType { get; set; } = AIBehaviorType.Balanced;
        [Export] public TargetPriority TargetPriority { get; set; } = TargetPriority.Random;
        
        [ExportGroup("Aggression")]
        [Export(PropertyHint.Range, "0,100")] public int Aggression { get; set; } = 50;
        [Export(PropertyHint.Range, "0,100")] public int Recklessness { get; set; } = 30;
        
        [ExportGroup("Skill Usage")]
        [Export] public bool PrefersMagic { get; set; } = false;
        [Export] public bool PrefersMelee { get; set; } = false;
        [Export(PropertyHint.Range, "0,100")] public int SkillUsageRate { get; set; } = 60;
        [Export] public Godot.Collections.Array<string> PreferredSkillIds { get; set; }
        [Export] public Godot.Collections.Array<string> EmergencySkillIds { get; set; }
        
        [ExportGroup("Thresholds")]
        [Export(PropertyHint.Range, "0,100")] public int LowHPThreshold { get; set; } = 30;
        [Export(PropertyHint.Range, "0,100")] public int DefensiveThreshold { get; set; } = 50;
        [Export(PropertyHint.Range, "0,100")] public int FleeThreshold { get; set; } = 10;
        
        [ExportGroup("Special Behaviors")]
        [Export] public bool WillFlee { get; set; } = false;
        [Export] public bool CallsForHelp { get; set; } = false;
        [Export] public bool EnragesAtLowHP { get; set; } = false;
        [Export] public bool GuardsAllies { get; set; } = false;
        [Export] public bool FocusesOneTarget { get; set; } = false;
        
        [ExportGroup("Turn Patterns")]
        [Export] public bool HasTurnPattern { get; set; } = false;
        [Export] public Godot.Collections.Array<string> TurnPattern { get; set; }
        
        // NEW: Smart AI Features
        [ExportGroup("Smart AI")]
        [Export] public bool LearnWeaknesses { get; set; } = true;
        [Export] public bool ExploitTechnicals { get; set; } = true;
        [Export] public bool UseDefensiveTactics { get; set; } = true;
        [Export(PropertyHint.Range, "0,100")] public int TechnicalPriority { get; set; } = 80;
        [Export(PropertyHint.Range, "0,100")] public int WeaknessPriority { get; set; } = 90;
        
        private int currentPatternIndex = 0;
        private TechnicalDamageSystem technicalSystem;
        private Dictionary<string, ElementAffinity> learnedWeaknesses;

        public AIPattern()
        {
            PreferredSkillIds = new Godot.Collections.Array<string>();
            EmergencySkillIds = new Godot.Collections.Array<string>();
            TurnPattern = new Godot.Collections.Array<string>();
            learnedWeaknesses = new Dictionary<string, ElementAffinity>();
        }

        public void Initialize(TechnicalDamageSystem techSystem)
        {
            technicalSystem = techSystem;
        }

        /// <summary>
        /// Main decision making - NOW WITH TECHNICAL & WEAKNESS AWARENESS
        /// </summary>
        public AIDecision DecideAction(CharacterStats actor, List<CharacterStats> allies, List<CharacterStats> enemies)
        {
            var decision = new AIDecision();

            // Check for emergency situations FIRST
            if (ShouldUseEmergencySkill(actor))
            {
                decision = DecideEmergencyAction(actor, allies, enemies);
                if (decision.ActionType != AIActionType.None) return decision;
            }

            // NEW: Check for defensive needs
            if (UseDefensiveTactics && ShouldDefend(actor, enemies))
            {
                return new AIDecision { ActionType = AIActionType.Defend };
            }

            // Check for flee condition
            if (WillFlee && actor.HPPercent <= FleeThreshold / 100f)
            {
                return new AIDecision { ActionType = AIActionType.Flee };
            }

            // NEW: PRIORITIZE TECHNICAL DAMAGE OPPORTUNITIES
            if (ExploitTechnicals && GD.Randf() * 100 < TechnicalPriority)
            {
                decision = FindTechnicalOpportunity(actor, enemies);
                if (decision.ActionType != AIActionType.None) return decision;
            }

            // NEW: PRIORITIZE WEAKNESS EXPLOITATION
            if (LearnWeaknesses && GD.Randf() * 100 < WeaknessPriority)
            {
                decision = FindWeaknessOpportunity(actor, enemies);
                if (decision.ActionType != AIActionType.None) return decision;
            }

            // Follow turn pattern if set
            if (HasTurnPattern && TurnPattern.Count > 0)
            {
                decision = DecidePatternAction(actor, allies, enemies);
                if (decision.ActionType != AIActionType.None) return decision;
            }

            // Normal decision making based on behavior type
            decision = BehaviorType switch
            {
                AIBehaviorType.Aggressive => DecideAggressiveAction(actor, enemies),
                AIBehaviorType.Defensive => DecideDefensiveAction(actor, allies),
                AIBehaviorType.Balanced => DecideBalancedAction(actor, allies, enemies),
                AIBehaviorType.Tactical => DecideTacticalAction(actor, enemies),
                AIBehaviorType.Berserk => DecideBerserkAction(actor, enemies),
                AIBehaviorType.Supportive => DecideSupportiveAction(actor, allies, enemies),
                AIBehaviorType.Cowardly => DecideCowardlyAction(actor, allies, enemies),
                _ => DecideBalancedAction(actor, allies, enemies)
            };

            return decision;
        }

        #region NEW: Smart AI Functions

        /// <summary>
        /// NEW: Find and execute technical damage combos
        /// </summary>
        private AIDecision FindTechnicalOpportunity(CharacterStats actor, List<CharacterStats> enemies)
        {
            if (technicalSystem == null) return new AIDecision { ActionType = AIActionType.None };

            var skills = actor.Skills?.GetUsableSkills(actor);
            if (skills == null || skills.Count == 0) return new AIDecision { ActionType = AIActionType.None };

            // Check each enemy for technical opportunities
            foreach (var enemy in enemies.Where(e => e.IsAlive))
            {
                foreach (var skill in skills)
                {
                    if (technicalSystem.CanCreateTechnical(enemy, skill.Element))
                    {
                        GD.Print($"[AI] {actor.CharacterName} found TECHNICAL opportunity on {enemy.CharacterName}!");
                        return new AIDecision
                        {
                            ActionType = AIActionType.UseSkill,
                            SelectedSkill = skill,
                            Target = enemy,
                            Reasoning = "Technical Combo Opportunity"
                        };
                    }
                }
            }

            return new AIDecision { ActionType = AIActionType.None };
        }

        /// <summary>
        /// NEW: Find and exploit known/discovered weaknesses
        /// </summary>
        private AIDecision FindWeaknessOpportunity(CharacterStats actor, List<CharacterStats> enemies)
        {
            var skills = actor.Skills?.GetUsableSkills(actor);
            if (skills == null || skills.Count == 0) return new AIDecision { ActionType = AIActionType.None };

            // Try learned weaknesses first
            foreach (var enemy in enemies.Where(e => e.IsAlive))
            {
                string key = enemy.CharacterId;
                if (learnedWeaknesses.TryGetValue(key, out var weakElement))
                {
                    var skill = skills.FirstOrDefault(s => 
                        s.Element.ToString() == weakElement.ToString());
                    
                    if (skill != null)
                    {
                        GD.Print($"[AI] {actor.CharacterName} exploiting learned weakness!");
                        return new AIDecision
                        {
                            ActionType = AIActionType.UseSkill,
                            SelectedSkill = skill,
                            Target = enemy,
                            Reasoning = "Exploiting Known Weakness"
                        };
                    }
                }

                // Try to discover new weaknesses
                foreach (var skill in skills.Where(s => s.Element != ElementType.None))
                {
                    var affinity = enemy.ElementAffinities.GetAffinity(skill.Element);
                    if (affinity == ElementAffinity.Weak)
                    {
                        // Learn this weakness
                        learnedWeaknesses[key] = affinity;
                        
                        GD.Print($"[AI] {actor.CharacterName} discovered weakness to {skill.Element}!");
                        return new AIDecision
                        {
                            ActionType = AIActionType.UseSkill,
                            SelectedSkill = skill,
                            Target = enemy,
                            Reasoning = "Discovered New Weakness"
                        };
                    }
                }
            }

            return new AIDecision { ActionType = AIActionType.None };
        }

        /// <summary>
        /// NEW: Determine if AI should defend this turn
        /// </summary>
        private bool ShouldDefend(CharacterStats actor, List<CharacterStats> enemies)
        {
            // Defend if low HP and no healing available
            if (actor.HPPercent < DefensiveThreshold / 100f)
            {
                var healSkills = actor.Skills?.GetEquippedSkills()
                    .Where(s => s.DisplayName.ToLower().Contains("heal") && s.CanUse(actor))
                    .ToList();
                
                if (healSkills == null || healSkills.Count == 0)
                {
                    GD.Print($"[AI] {actor.CharacterName} guards defensively (Low HP: {actor.HPPercent * 100}%)");
                    return true;
                }
            }

            // Defend if heavily outnumbered
            if (GuardsAllies)
            {
                int livingAllies = enemies.Count(e => e.IsAlive);
                int livingEnemies = enemies.Count(e => e.IsAlive);
                
                if (livingEnemies >= livingAllies * 2)
                {
                    GD.Print($"[AI] {actor.CharacterName} guards strategically (Outnumbered!)");
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Emergency & Defensive Actions

        private bool ShouldUseEmergencySkill(CharacterStats actor)
        {
            return actor.HPPercent <= LowHPThreshold / 100f && EmergencySkillIds.Count > 0;
        }

        private AIDecision DecideEmergencyAction(CharacterStats actor, List<CharacterStats> allies, List<CharacterStats> enemies)
        {
            var skills = actor.Skills?.GetEquippedSkills();
            if (skills == null) return new AIDecision { ActionType = AIActionType.None };

            foreach (var skillId in EmergencySkillIds)
            {
                var skill = skills.Find(s => s.SkillId == skillId && s.CanUse(actor));
                if (skill != null)
                {
                    GD.Print($"[AI] {actor.CharacterName} uses EMERGENCY skill: {skill.DisplayName}");
                    return new AIDecision
                    {
                        ActionType = AIActionType.UseSkill,
                        SelectedSkill = skill,
                        Target = ChooseSkillTarget(skill, actor, allies, enemies),
                        Reasoning = "Emergency Skill"
                    };
                }
            }

            return new AIDecision { ActionType = AIActionType.None };
        }

        private AIDecision DecideDefensiveAction(CharacterStats actor, List<CharacterStats> allies)
        {
            var skills = actor.Skills?.GetUsableSkills(actor);
            if (skills == null) return new AIDecision { ActionType = AIActionType.Defend };

            // Prioritize healing wounded allies
            var woundedAlly = allies.Where(a => a.IsAlive && a.HPPercent < 0.6f)
                .OrderBy(a => a.HPPercent).FirstOrDefault();

            if (woundedAlly != null)
            {
                var healSkill = skills.FirstOrDefault(s => 
                    s.DisplayName.ToLower().Contains("heal"));
                
                if (healSkill != null)
                {
                    return new AIDecision
                    {
                        ActionType = AIActionType.UseSkill,
                        SelectedSkill = healSkill,
                        Target = woundedAlly,
                        Reasoning = "Healing Wounded Ally"
                    };
                }
            }

            // Use buff skills
            var buffSkills = skills.Where(s => 
                s.Type == SkillType.ActiveSupport && !s.IsDebuff).ToList();
            
            if (buffSkills.Count > 0)
            {
                var buff = buffSkills[GD.RandRange(0, buffSkills.Count - 1)];
                return new AIDecision
                {
                    ActionType = AIActionType.UseSkill,
                    SelectedSkill = buff,
                    Target = allies.OrderBy(a => a.HPPercent).First(),
                    Reasoning = "Buffing Ally"
                };
            }

            return new AIDecision { ActionType = AIActionType.Defend };
        }

        #endregion

        #region Behavior Types

        private AIDecision DecideAggressiveAction(CharacterStats actor, List<CharacterStats> enemies)
        {
            var skills = actor.Skills?.GetUsableSkills(actor);
            var damageSkills = skills?.Where(s => 
                s.Type == SkillType.ActiveAttack && s.BasePower > 0).ToList();

            if (damageSkills != null && damageSkills.Count > 0 && 
                GD.Randf() * 100 < SkillUsageRate)
            {
                // Choose highest damage skill
                var skill = damageSkills.OrderByDescending(s => s.BasePower).First();
                var target = ChooseTarget(enemies, TargetPriority);

                return new AIDecision
                {
                    ActionType = AIActionType.UseSkill,
                    SelectedSkill = skill,
                    Target = target,
                    Reasoning = "Aggressive Damage"
                };
            }

            return new AIDecision
            {
                ActionType = AIActionType.Attack,
                Target = ChooseTarget(enemies, TargetPriority),
                Reasoning = "Basic Attack"
            };
        }

        private AIDecision DecideBalancedAction(CharacterStats actor, List<CharacterStats> allies, List<CharacterStats> enemies)
        {
            // Check if healing needed
            if (allies.Any(a => a.IsAlive && a.HPPercent < 0.5f))
            {
                return DecideDefensiveAction(actor, allies);
            }

            // Otherwise be aggressive
            return DecideAggressiveAction(actor, enemies);
        }

        private AIDecision DecideTacticalAction(CharacterStats actor, List<CharacterStats> enemies)
        {
            var skills = actor.Skills?.GetUsableSkills(actor);
            var skillsWithElements = skills?.Where(s => 
                s.Element != ElementType.None && s.BasePower > 0).ToList();

            if (skillsWithElements != null && skillsWithElements.Count > 0)
            {
                // Find enemy with matching weakness
                foreach (var enemy in enemies.Where(e => e.IsAlive))
                {
                    foreach (var skill in skillsWithElements)
                    {
                        if (enemy.ElementAffinities.GetAffinity(skill.Element) == ElementAffinity.Weak)
                        {
                            GD.Print($"[AI] Tactical strike on weakness!");
                            return new AIDecision
                            {
                                ActionType = AIActionType.UseSkill,
                                SelectedSkill = skill,
                                Target = enemy,
                                Reasoning = "Tactical Weakness Hit"
                            };
                        }
                    }
                }
            }

            return DecideAggressiveAction(actor, enemies);
        }

        private AIDecision DecideBerserkAction(CharacterStats actor, List<CharacterStats> enemies)
        {
            var allSkills = actor.Skills?.GetUsableSkills(actor);

            if (allSkills != null && allSkills.Count > 0 && GD.Randf() < 0.7f)
            {
                var skill = allSkills[GD.RandRange(0, allSkills.Count - 1)];
                var target = enemies[GD.RandRange(0, enemies.Count - 1)];

                return new AIDecision
                {
                    ActionType = AIActionType.UseSkill,
                    SelectedSkill = skill,
                    Target = target,
                    Reasoning = "Berserk Attack"
                };
            }

            return new AIDecision
            {
                ActionType = AIActionType.Attack,
                Target = enemies[GD.RandRange(0, enemies.Count - 1)],
                Reasoning = "Berserk Attack"
            };
        }

        private AIDecision DecideSupportiveAction(CharacterStats actor, List<CharacterStats> allies, List<CharacterStats> enemies)
        {
            var supportSkills = actor.Skills?.GetEquippedSkills()
                .Where(s => s.Type == SkillType.ActiveSupport && s.CanUse(actor))
                .ToList();

            if (supportSkills != null && supportSkills.Count > 0)
            {
                var skill = supportSkills[GD.RandRange(0, supportSkills.Count - 1)];
                var target = skill.IsDebuff 
                    ? ChooseTarget(enemies, TargetPriority)
                    : allies.OrderBy(a => a.HPPercent).First();

                return new AIDecision
                {
                    ActionType = AIActionType.UseSkill,
                    SelectedSkill = skill,
                    Target = target,
                    Reasoning = "Support Action"
                };
            }

            return DecideBalancedAction(actor, allies, enemies);
        }

        private AIDecision DecideCowardlyAction(CharacterStats actor, List<CharacterStats> allies, List<CharacterStats> enemies)
        {
            if (actor.HPPercent < 0.5f)
            {
                return new AIDecision { ActionType = AIActionType.Flee, Reasoning = "Cowardly Flee" };
            }

            return DecideBalancedAction(actor, allies, enemies);
        }

        private AIDecision DecidePatternAction(CharacterStats actor, List<CharacterStats> allies, List<CharacterStats> enemies)
        {
            if (TurnPattern.Count == 0) return new AIDecision { ActionType = AIActionType.None };

            string skillId = TurnPattern[currentPatternIndex];
            currentPatternIndex = (currentPatternIndex + 1) % TurnPattern.Count;

            var skill = actor.Skills?.GetEquippedSkills().Find(s => s.SkillId == skillId);

            if (skill != null && skill.CanUse(actor))
            {
                return new AIDecision
                {
                    ActionType = AIActionType.UseSkill,
                    SelectedSkill = skill,
                    Target = ChooseSkillTarget(skill, actor, allies, enemies),
                    Reasoning = "Turn Pattern"
                };
            }

            return new AIDecision { ActionType = AIActionType.None };
        }

        #endregion

        #region Target Selection

        private CharacterStats ChooseTarget(List<CharacterStats> targets, TargetPriority priority)
        {
            var aliveTargets = targets.Where(t => t.IsAlive).ToList();
            if (aliveTargets.Count == 0) return null;

            return priority switch
            {
                TargetPriority.LowestHP => aliveTargets.OrderBy(t => t.CurrentHP).First(),
                TargetPriority.HighestHP => aliveTargets.OrderByDescending(t => t.CurrentHP).First(),
                TargetPriority.LowestDefense => aliveTargets.OrderBy(t => t.Defense).First(),
                TargetPriority.Weakest => aliveTargets.OrderBy(t => t.Level).First(),
                TargetPriority.MostVulnerable => FindMostVulnerable(aliveTargets),
                TargetPriority.Random => aliveTargets[GD.RandRange(0, aliveTargets.Count - 1)],
                _ => aliveTargets[GD.RandRange(0, aliveTargets.Count - 1)]
            };
        }

        private CharacterStats FindMostVulnerable(List<CharacterStats> targets)
        {
            // Prioritize targets with status effects or low HP
            return targets
                .OrderBy(t => t.ActiveStatuses.Count > 0 ? 0 : 1)
                .ThenBy(t => t.HPPercent)
                .First();
        }

        private CharacterStats ChooseSkillTarget(SkillData skill, CharacterStats actor, 
            List<CharacterStats> allies, List<CharacterStats> enemies)
        {
            return skill.Target switch
            {
                SkillTarget.SingleEnemy => ChooseTarget(enemies, TargetPriority),
                SkillTarget.SingleAlly => allies.OrderBy(a => a.HPPercent).FirstOrDefault(),
                SkillTarget.Self => actor,
                SkillTarget.DeadAlly => allies.FirstOrDefault(a => !a.IsAlive),
                _ => ChooseTarget(enemies, TargetPriority)
            };
        }

        #endregion
    }

    public enum AIActionType
    {
        None,
        Attack,
        UseSkill,
        Defend,
        Flee,
        CallForHelp,
        Item
    }

    public class AIDecision
    {
        public AIActionType ActionType { get; set; }
        public SkillData SelectedSkill { get; set; }
        public CharacterStats Target { get; set; }
        public string ItemId { get; set; }
        public string Reasoning { get; set; } // NEW: For debugging
    }
}