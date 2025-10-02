// Combat/AIPattern.cs
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
        Healer,         // Target healers first
        Mage,           // Target magic users
        Weakest,        // Lowest level/stats
        Leader          // Player character
    }

    /// <summary>
    /// AI behavior pattern for enemies
    /// </summary>
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
        [Export] public Godot.Collections.Array<string> EmergencySkillIds { get; set; }  // Used at low HP
        
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
        [Export] public Godot.Collections.Array<string> TurnPattern { get; set; }  // Skill IDs in order
        private int currentPatternIndex = 0;

        public AIPattern()
        {
            PreferredSkillIds = new Godot.Collections.Array<string>();
            EmergencySkillIds = new Godot.Collections.Array<string>();
            TurnPattern = new Godot.Collections.Array<string>();
        }

        /// <summary>
        /// Decide what action the AI should take
        /// </summary>
        public AIDecision DecideAction(CharacterStats actor, List<CharacterStats> allies, List<CharacterStats> enemies)
        {
            var decision = new AIDecision();

            // Check for emergency situations
            if (ShouldUseEmergencySkill(actor))
            {
                decision = DecideEmergencyAction(actor, allies, enemies);
                if (decision.ActionType != AIActionType.None) return decision;
            }

            // Check for special behaviors
            if (WillFlee && actor.HPPercent <= FleeThreshold / 100f)
            {
                decision.ActionType = AIActionType.Flee;
                return decision;
            }

            if (CallsForHelp && allies.Count(a => a.IsAlive) < 2 && actor.HPPercent < 0.5f)
            {
                decision.ActionType = AIActionType.CallForHelp;
                return decision;
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

        private bool ShouldUseEmergencySkill(CharacterStats actor)
        {
            return actor.HPPercent <= LowHPThreshold / 100f && EmergencySkillIds.Count > 0;
        }

        private AIDecision DecideEmergencyAction(CharacterStats actor, List<CharacterStats> allies, List<CharacterStats> enemies)
        {
            var usableEmergencySkills = new List<SkillData>();

            foreach (var skillId in EmergencySkillIds)
            {
                var skill = actor.Skills?.GetEquippedSkills().Find(s => s.SkillId == skillId);
                if (skill != null && skill.CanUse(actor))
                {
                    usableEmergencySkills.Add(skill);
                }
            }

            if (usableEmergencySkills.Count > 0)
            {
                var skill = usableEmergencySkills[GD.RandRange(0, usableEmergencySkills.Count - 1)];
                return new AIDecision
                {
                    ActionType = AIActionType.UseSkill,
                    SelectedSkill = skill,
                    Target = ChooseSkillTarget(skill, actor, allies, enemies)
                };
            }

            return new AIDecision { ActionType = AIActionType.None };
        }

        private AIDecision DecideAggressiveAction(CharacterStats actor, List<CharacterStats> enemies)
        {
            // High chance to use damage skills
            if (GD.Randf() * 100 < SkillUsageRate)
            {
                var damageSkills = actor.Skills?.GetEquippedSkills()
                    .Where(s => s.Type == SkillType.ActiveAttack && s.CanUse(actor))
                    .ToList();

                if (damageSkills != null && damageSkills.Count > 0)
                {
                    var skill = ChoosePreferredSkill(damageSkills);
                    return new AIDecision
                    {
                        ActionType = AIActionType.UseSkill,
                        SelectedSkill = skill,
                        Target = ChooseTarget(enemies, TargetPriority)
                    };
                }
            }

            // Default to basic attack
            return new AIDecision
            {
                ActionType = AIActionType.Attack,
                Target = ChooseTarget(enemies, TargetPriority)
            };
        }

        private AIDecision DecideDefensiveAction(CharacterStats actor, List<CharacterStats> allies)
        {
            // Check if anyone needs healing
            var woundedAllies = allies.Where(a => a.IsAlive && a.HPPercent < 0.7f).ToList();

            if (woundedAllies.Count > 0)
            {
                var healingSkills = actor.Skills?.GetEquippedSkills()
                    .Where(s => s.HealAmount > 0 && s.CanUse(actor))
                    .ToList();

                if (healingSkills != null && healingSkills.Count > 0)
                {
                    var mostWounded = woundedAllies.OrderBy(a => a.HPPercent).First();
                    return new AIDecision
                    {
                        ActionType = AIActionType.UseSkill,
                        SelectedSkill = healingSkills[0],
                        Target = mostWounded
                    };
                }
            }

            // Otherwise defend
            return new AIDecision
            {
                ActionType = AIActionType.Defend
            };
        }

        private AIDecision DecideBalancedAction(CharacterStats actor, List<CharacterStats> allies, List<CharacterStats> enemies)
        {
            // Mix of offense and defense
            if (actor.HPPercent < DefensiveThreshold / 100f)
            {
                return DecideDefensiveAction(actor, allies);
            }
            else
            {
                return DecideAggressiveAction(actor, enemies);
            }
        }

        private AIDecision DecideTacticalAction(CharacterStats actor, List<CharacterStats> enemies)
        {
            // Exploit weaknesses
            var skillsWithElements = actor.Skills?.GetEquippedSkills()
                .Where(s => s.Type == SkillType.ActiveAttack && s.Element != ElementType.Physical && s.CanUse(actor))
                .ToList();

            if (skillsWithElements != null && skillsWithElements.Count > 0)
            {
                // Find enemy with matching weakness
                foreach (var enemy in enemies.Where(e => e.IsAlive))
                {
                    foreach (var skill in skillsWithElements)
                    {
                        if (enemy.ElementAffinities.GetAffinity(skill.Element) == ElementAffinity.Weak)
                        {
                            return new AIDecision
                            {
                                ActionType = AIActionType.UseSkill,
                                SelectedSkill = skill,
                                Target = enemy
                            };
                        }
                    }
                }
            }

            // Fall back to aggressive
            return DecideAggressiveAction(actor, enemies);
        }

        private AIDecision DecideBerserkAction(CharacterStats actor, List<CharacterStats> enemies)
        {
            // Random powerful attacks
            var allSkills = actor.Skills?.GetUsableSkills(actor);

            if (allSkills != null && allSkills.Count > 0 && GD.Randf() < 0.7f)
            {
                var skill = allSkills[GD.RandRange(0, allSkills.Count - 1)];
                var target = enemies[GD.RandRange(0, enemies.Count - 1)];

                return new AIDecision
                {
                    ActionType = AIActionType.UseSkill,
                    SelectedSkill = skill,
                    Target = target
                };
            }

            return new AIDecision
            {
                ActionType = AIActionType.Attack,
                Target = enemies[GD.RandRange(0, enemies.Count - 1)]
            };
        }

        private AIDecision DecideSupportiveAction(CharacterStats actor, List<CharacterStats> allies, List<CharacterStats> enemies)
        {
            // Prioritize buffs and debuffs
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
                    Target = target
                };
            }

            return DecideDefensiveAction(actor, allies);
        }

        private AIDecision DecideCowardlyAction(CharacterStats actor, List<CharacterStats> allies, List<CharacterStats> enemies)
        {
            if (actor.HPPercent < 0.5f)
            {
                return new AIDecision { ActionType = AIActionType.Flee };
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
                    Target = ChooseSkillTarget(skill, actor, allies, enemies)
                };
            }

            return new AIDecision { ActionType = AIActionType.None };
        }

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
                TargetPriority.Random => aliveTargets[GD.RandRange(0, aliveTargets.Count - 1)],
                _ => aliveTargets[GD.RandRange(0, aliveTargets.Count - 1)]
            };
        }

        private CharacterStats ChooseSkillTarget(SkillData skill, CharacterStats actor, List<CharacterStats> allies, List<CharacterStats> enemies)
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

        private SkillData ChoosePreferredSkill(List<SkillData> skills)
        {
            if (PreferredSkillIds.Count > 0)
            {
                foreach (var preferredId in PreferredSkillIds)
                {
                    var preferred = skills.Find(s => s.SkillId == preferredId);
                    if (preferred != null) return preferred;
                }
            }

            return skills[GD.RandRange(0, skills.Count - 1)];
        }
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
    }
}