// Combat/CharacterSkills.cs
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EchoesAcrossTime.Combat
{
    /// <summary>
    /// Manages a character's learned skills
    /// </summary>
    public partial class CharacterSkills : GodotObject
    {
        public string CharacterId { get; set; }
        
        private List<SkillData> learnedSkills = new List<SkillData>();
        private List<SkillData> equippedSkills = new List<SkillData>();
        private List<SkillData> passiveSkills = new List<SkillData>();
        
        [Export] public int MaxEquippedSkills { get; set; } = 8;
        
        [Signal]
        public delegate void SkillLearnedEventHandler(string skillId);
        
        [Signal]
        public delegate void SkillEquippedEventHandler(string skillId);
        
        [Signal]
        public delegate void SkillUnequippedEventHandler(string skillId);

        public CharacterSkills(string characterId)
        {
            CharacterId = characterId;
        }

        /// <summary>
        /// Learn a new skill
        /// </summary>
        public bool LearnSkill(SkillData skill)
        {
            if (skill == null)
            {
                GD.PrintErr("CharacterSkills: Cannot learn null skill");
                return false;
            }

            if (HasLearned(skill.SkillId))
            {
                GD.Print($"CharacterSkills: Already knows {skill.DisplayName}");
                return false;
            }

            learnedSkills.Add(skill);
            
            // Auto-equip passive skills
            if (skill.Type == SkillType.Passive)
            {
                passiveSkills.Add(skill);
            }
            
            // Auto-equip if there's space
            if (skill.Type != SkillType.Passive && equippedSkills.Count < MaxEquippedSkills)
            {
                EquipSkill(skill);
            }

            EmitSignal(SignalName.SkillLearned, skill.SkillId);
            GD.Print($"Learned skill: {skill.DisplayName}");
            
            return true;
        }

        /// <summary>
        /// Equip a skill for battle
        /// </summary>
        public bool EquipSkill(SkillData skill)
        {
            if (skill == null) return false;
            
            if (!HasLearned(skill.SkillId))
            {
                GD.PrintErr($"CharacterSkills: Cannot equip unlearned skill {skill.DisplayName}");
                return false;
            }

            if (skill.Type == SkillType.Passive)
            {
                GD.PrintErr("CharacterSkills: Passive skills are always active");
                return false;
            }

            if (equippedSkills.Contains(skill))
            {
                GD.Print($"CharacterSkills: {skill.DisplayName} already equipped");
                return false;
            }

            if (equippedSkills.Count >= MaxEquippedSkills)
            {
                GD.PrintErr($"CharacterSkills: Maximum equipped skills reached ({MaxEquippedSkills})");
                return false;
            }

            equippedSkills.Add(skill);
            EmitSignal(SignalName.SkillEquipped, skill.SkillId);
            
            return true;
        }

        /// <summary>
        /// Unequip a skill
        /// </summary>
        public bool UnequipSkill(SkillData skill)
        {
            if (skill == null) return false;

            if (equippedSkills.Remove(skill))
            {
                EmitSignal(SignalName.SkillUnequipped, skill.SkillId);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Unequip skill by ID
        /// </summary>
        public bool UnequipSkillById(string skillId)
        {
            var skill = equippedSkills.Find(s => s.SkillId == skillId);
            return UnequipSkill(skill);
        }

        /// <summary>
        /// Check if skill is learned
        /// </summary>
        public bool HasLearned(string skillId)
        {
            return learnedSkills.Any(s => s.SkillId == skillId);
        }

        /// <summary>
        /// Check if skill is equipped
        /// </summary>
        public bool IsEquipped(string skillId)
        {
            return equippedSkills.Any(s => s.SkillId == skillId);
        }

        /// <summary>
        /// Get all learned skills
        /// </summary>
        public List<SkillData> GetLearnedSkills()
        {
            return new List<SkillData>(learnedSkills);
        }

        /// <summary>
        /// Get equipped skills
        /// </summary>
        public List<SkillData> GetEquippedSkills()
        {
            return new List<SkillData>(equippedSkills);
        }

        /// <summary>
        /// Get passive skills
        /// </summary>
        public List<SkillData> GetPassiveSkills()
        {
            return new List<SkillData>(passiveSkills);
        }

        /// <summary>
        /// Get usable skills (equipped + not on cooldown)
        /// </summary>
        public List<SkillData> GetUsableSkills(CharacterStats user)
        {
            return equippedSkills.Where(s => s.CanUse(user)).ToList();
        }

        /// <summary>
        /// Get skills by type
        /// </summary>
        public List<SkillData> GetSkillsByType(SkillType type)
        {
            return learnedSkills.Where(s => s.Type == type).ToList();
        }

        /// <summary>
        /// Forget a skill
        /// </summary>
        public bool ForgetSkill(string skillId)
        {
            var skill = learnedSkills.Find(s => s.SkillId == skillId);
            
            if (skill == null) return false;

            learnedSkills.Remove(skill);
            equippedSkills.Remove(skill);
            passiveSkills.Remove(skill);

            GD.Print($"Forgot skill: {skill.DisplayName}");
            return true;
        }

        /// <summary>
        /// Get number of learned skills
        /// </summary>
        public int GetLearnedCount() => learnedSkills.Count;

        /// <summary>
        /// Get number of equipped skills
        /// </summary>
        public int GetEquippedCount() => equippedSkills.Count;

        /// <summary>
        /// Get number of free equip slots
        /// </summary>
        public int GetFreeSlots() => MaxEquippedSkills - equippedSkills.Count;
    }
}