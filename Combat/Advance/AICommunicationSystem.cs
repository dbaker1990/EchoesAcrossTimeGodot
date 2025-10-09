// Combat/Advanced/AICommunicationSystem.cs
// Makes AI thinking visible through dialogue and reactions
using Godot;
using System.Collections.Generic;
using System.Linq;

namespace EchoesAcrossTime.Combat.Advanced
{
    /// <summary>
    /// AI Communication System
    /// Makes AI decisions visible to players through dialogue,
    /// reactions, and visual indicators
    /// </summary>
    public partial class AICommunicationSystem : Node
    {
        private Dictionary<string, AIPersonality> personalities = new Dictionary<string, AIPersonality>();
        private Queue<AIMessage> messageQueue = new Queue<AIMessage>();
        
        [Signal]
        public delegate void AIDialogueEventHandler(string characterName, string dialogue, DialogueType type);
        
        [Signal]
        public delegate void AIReactionEventHandler(string characterName, string reaction);
        
        [Signal]
        public delegate void AITauntEventHandler(string characterName, string taunt);
        
        /// <summary>
        /// Register AI personality for a character
        /// </summary>
        public void RegisterPersonality(string characterId, AIPersonality personality)
        {
            personalities[characterId] = personality;
            GD.Print($"[AI COMMS] Registered personality for {characterId}: {personality.PersonalityType}");
        }
        
        /// <summary>
        /// AI announces its action
        /// </summary>
        public void AnnounceAction(string characterId, string characterName, AIDecision decision)
        {
            if (!personalities.ContainsKey(characterId)) return;
            
            var personality = personalities[characterId];
            string dialogue = GenerateActionDialogue(personality, decision);
            
            if (!string.IsNullOrEmpty(dialogue))
            {
                EmitDialogue(characterName, dialogue, DialogueType.Action);
            }
        }
        
        /// <summary>
        /// AI reacts to player action
        /// </summary>
        public void ReactToPlayerAction(string characterId, string characterName, string playerAction, bool wasEffective)
        {
            if (!personalities.ContainsKey(characterId)) return;
            
            var personality = personalities[characterId];
            string reaction = GenerateReactionDialogue(personality, playerAction, wasEffective);
            
            if (!string.IsNullOrEmpty(reaction))
            {
                EmitDialogue(characterName, reaction, DialogueType.Reaction);
                EmitSignal(SignalName.AIReaction, characterName, reaction);
            }
        }
        
        /// <summary>
        /// AI taunts player
        /// </summary>
        public void Taunt(string characterId, string characterName, TauntReason reason)
        {
            if (!personalities.ContainsKey(characterId)) return;
            
            var personality = personalities[characterId];
            string taunt = GenerateTaunt(personality, reason);
            
            if (!string.IsNullOrEmpty(taunt))
            {
                EmitDialogue(characterName, taunt, DialogueType.Taunt);
                EmitSignal(SignalName.AITaunt, characterName, taunt);
            }
        }
        
        /// <summary>
        /// AI explains its strategy (for teaching/narrative)
        /// </summary>
        public void ExplainStrategy(string characterId, string characterName, string strategyExplanation)
        {
            EmitDialogue(characterName, strategyExplanation, DialogueType.Strategy);
        }
        
        /// <summary>
        /// Generate action dialogue
        /// </summary>
        private string GenerateActionDialogue(AIPersonality personality, AIDecision decision)
        {
            var lines = new List<string>();
            
            switch (decision.ActionType)
            {
                case AIActionType.UseSkill:
                    if (decision.SelectedSkill != null)
                    {
                        lines = personality.SkillUseLines;
                    }
                    break;
                
                case AIActionType.Defend:
                    lines = personality.DefendLines;
                    break;
                
                case AIActionType.Attack:
                    lines = personality.AttackLines;
                    break;
            }
            
            // Add reasoning-specific lines
            if (decision.Reasoning.Contains("Technical"))
                lines.AddRange(personality.TechnicalLines);
            else if (decision.Reasoning.Contains("Weakness"))
                lines.AddRange(personality.WeaknessLines);
            else if (decision.Reasoning.Contains("Prediction"))
                lines.AddRange(personality.PredictionLines);
            else if (decision.Reasoning.Contains("Desperate"))
                lines.AddRange(personality.DesperateLines);
            
            return lines.Count > 0 ? lines[GD.RandRange(0, lines.Count - 1)] : "";
        }
        
        /// <summary>
        /// Generate reaction dialogue
        /// </summary>
        private string GenerateReactionDialogue(AIPersonality personality, string playerAction, bool wasEffective)
        {
            if (wasEffective)
            {
                return personality.HitReactions.Count > 0 
                    ? personality.HitReactions[GD.RandRange(0, personality.HitReactions.Count - 1)] 
                    : "";
            }
            else
            {
                return personality.MissReactions.Count > 0 
                    ? personality.MissReactions[GD.RandRange(0, personality.MissReactions.Count - 1)] 
                    : "";
            }
        }
        
        /// <summary>
        /// Generate taunt
        /// </summary>
        private string GenerateTaunt(AIPersonality personality, TauntReason reason)
        {
            List<string> lines = reason switch
            {
                TauntReason.LowPlayerHP => personality.TauntLowHP,
                TauntReason.PredictedMove => personality.TauntPrediction,
                TauntReason.PhaseTransition => personality.TauntPhaseChange,
                TauntReason.Victory => personality.TauntVictory,
                _ => personality.GenericTaunts
            };
            
            return lines.Count > 0 ? lines[GD.RandRange(0, lines.Count - 1)] : "";
        }
        
        private void EmitDialogue(string characterName, string dialogue, DialogueType type)
        {
            GD.Print($"💬 {characterName}: \"{dialogue}\"");
            EmitSignal(SignalName.AIDialogue, characterName, dialogue, (int)type);
        }
    }
    
    /// <summary>
    /// AI Personality defines dialogue style
    /// </summary>
    public class AIPersonality
    {
        public string PersonalityType { get; set; } = "Neutral";
        
        // Action lines
        public List<string> AttackLines { get; set; } = new List<string>();
        public List<string> SkillUseLines { get; set; } = new List<string>();
        public List<string> DefendLines { get; set; } = new List<string>();
        
        // Strategy lines
        public List<string> TechnicalLines { get; set; } = new List<string>();
        public List<string> WeaknessLines { get; set; } = new List<string>();
        public List<string> PredictionLines { get; set; } = new List<string>();
        public List<string> DesperateLines { get; set; } = new List<string>();
        
        // Reactions
        public List<string> HitReactions { get; set; } = new List<string>();
        public List<string> MissReactions { get; set; } = new List<string>();
        
        // Taunts
        public List<string> GenericTaunts { get; set; } = new List<string>();
        public List<string> TauntLowHP { get; set; } = new List<string>();
        public List<string> TauntPrediction { get; set; } = new List<string>();
        public List<string> TauntPhaseChange { get; set; } = new List<string>();
        public List<string> TauntVictory { get; set; } = new List<string>();
    }
    
    public enum DialogueType
    {
        Action,
        Reaction,
        Taunt,
        Strategy,
        PhaseChange
    }
    
    public enum TauntReason
    {
        LowPlayerHP,
        PredictedMove,
        PhaseTransition,
        Victory,
        Generic
    }
    
    public class AIMessage
    {
        public string CharacterName { get; set; }
        public string Message { get; set; }
        public DialogueType Type { get; set; }
        public float DisplayTime { get; set; } = 2.0f;
    }
    
    /// <summary>
    /// Pre-made personality templates
    /// </summary>
    public static class AIPersonalityTemplates
    {
        /// <summary>
        /// Confident Strategist - Calculating and intelligent
        /// </summary>
        public static AIPersonality CreateStrategist()
        {
            return new AIPersonality
            {
                PersonalityType = "Strategist",
                
                AttackLines = new List<string>
                {
                    "A calculated strike.",
                    "Exploiting your weakness.",
                    "This is all part of my plan."
                },
                
                SkillUseLines = new List<string>
                {
                    "Let me show you real power.",
                    "I've been saving this.",
                    "Time to execute my strategy."
                },
                
                DefendLines = new List<string>
                {
                    "I'll wait for the right moment.",
                    "Patience is key.",
                    "Building my strength..."
                },
                
                TechnicalLines = new List<string>
                {
                    "Perfect! A technical opportunity!",
                    "Status effect detected. Exploiting!",
                    "This combo will hurt!"
                },
                
                WeaknessLines = new List<string>
                {
                    "I found your weakness!",
                    "Striking your vulnerable point!",
                    "This element is your bane!"
                },
                
                PredictionLines = new List<string>
                {
                    "I knew you'd do that!",
                    "Your moves are so predictable.",
                    "Already three steps ahead of you.",
                    "Did you think I wouldn't see that coming?"
                },
                
                DesperateLines = new List<string>
                {
                    "Time for desperate measures!",
                    "Going all in!",
                    "This is my last gambit!"
                },
                
                HitReactions = new List<string>
                {
                    "Tch! That one connected...",
                    "Impressive... but not enough!",
                    "You're better than I thought."
                },
                
                MissReactions = new List<string>
                {
                    "Hah! Too slow!",
                    "Did you really think that would work?",
                    "Nice try."
                },
                
                TauntLowHP = new List<string>
                {
                    "You're barely standing!",
                    "I can see your end from here.",
                    "One more hit should do it."
                },
                
                TauntPrediction = new List<string>
                {
                    "I've memorized your pattern!",
                    "You're an open book to me.",
                    "I know your every move!"
                }
            };
        }
        
        /// <summary>
        /// Arrogant Champion - Overconfident and boastful
        /// </summary>
        public static AIPersonality CreateArrogant()
        {
            return new AIPersonality
            {
                PersonalityType = "Arrogant",
                
                AttackLines = new List<string>
                {
                    "Witness my superiority!",
                    "I'm simply better than you!",
                    "This is the power of a champion!"
                },
                
                SkillUseLines = new List<string>
                {
                    "Behold my ultimate technique!",
                    "This is what true power looks like!",
                    "I'll show you why I'm undefeated!"
                },
                
                DefendLines = new List<string>
                {
                    "I don't even need to try.",
                    "You're not worth my full effort.",
                    "Boring..."
                },
                
                WeaknessLines = new List<string>
                {
                    "Of COURSE you're weak to this!",
                    "So predictable!",
                    "Too easy!"
                },
                
                HitReactions = new List<string>
                {
                    "Lucky shot!",
                    "That was nothing!",
                    "Is that all you've got?!"
                },
                
                MissReactions = new List<string>
                {
                    "HAHAHAHA! Pathetic!",
                    "You couldn't hit me if I stood still!",
                    "Embarrassing!"
                },
                
                TauntLowHP = new List<string>
                {
                    "You're done! Accept defeat!",
                    "This was never a fair fight!",
                    "Know your place!"
                },
                
                TauntPhaseChange = new List<string>
                {
                    "Now I'm getting serious!",
                    "Time to stop holding back!",
                    "You haven't even seen my true power!"
                }
            };
        }
        
        /// <summary>
        /// Honorable Warrior - Respectful and disciplined
        /// </summary>
        public static AIPersonality CreateHonorable()
        {
            return new AIPersonality
            {
                PersonalityType = "Honorable",
                
                AttackLines = new List<string>
                {
                    "With honor!",
                    "Prepare yourself!",
                    "A fair strike!"
                },
                
                SkillUseLines = new List<string>
                {
                    "Witness my discipline!",
                    "I've trained for this!",
                    "My technique is perfected!"
                },
                
                DefendLines = new List<string>
                {
                    "I respect your strength.",
                    "A moment to recover.",
                    "I won't underestimate you."
                },
                
                WeaknessLines = new List<string>
                {
                    "I must exploit every advantage.",
                    "Forgive me, but I must win.",
                    "Tactics are part of combat."
                },
                
                HitReactions = new List<string>
                {
                    "Well struck!",
                    "You have skill!",
                    "An honorable blow!"
                },
                
                MissReactions = new List<string>
                {
                    "Your form needs work.",
                    "Too hasty.",
                    "Patience wins battles."
                },
                
                TauntPhaseChange = new List<string>
                {
                    "You've earned my full attention!",
                    "Now I show my true strength!",
                    "Let us both fight with everything we have!"
                }
            };
        }
        
        /// <summary>
        /// Unhinged Berserker - Chaotic and aggressive
        /// </summary>
        public static AIPersonality CreateBerserker()
        {
            return new AIPersonality
            {
                PersonalityType = "Berserker",
                
                AttackLines = new List<string>
                {
                    "RAAAAAAGH!",
                    "DESTROY!",
                    "BLOOD! DEATH!",
                    "MORE! MORE!"
                },
                
                SkillUseLines = new List<string>
                {
                    "MAXIMUM POWER!",
                    "OBLITERATE!",
                    "CHAOS! DESTRUCTION!"
                },
                
                DefendLines = new List<string>
                {
                    "Ugh... fine...",
                    "Just building rage...",
                    "This is boring!"
                },
                
                DesperateLines = new List<string>
                {
                    "IF I DIE, YOU DIE!",
                    "FINAL RAGE!",
                    "EVERYTHING BURNS!"
                },
                
                HitReactions = new List<string>
                {
                    "GRAAAGH!",
                    "PAIN! GLORIOUS PAIN!",
                    "MORE!"
                },
                
                TauntLowHP = new List<string>
                {
                    "I SMELL YOUR FEAR!",
                    "DIE! DIE! DIE!",
                    "YOU'RE ALREADY DEAD!"
                }
            };
        }
        
        /// <summary>
        /// Cold Tactician - Emotionless and efficient
        /// </summary>
        public static AIPersonality CreateTactician()
        {
            return new AIPersonality
            {
                PersonalityType = "Tactician",
                
                AttackLines = new List<string>
                {
                    "Executing attack protocol.",
                    "Target acquired.",
                    "Optimal strike angle calculated."
                },
                
                SkillUseLines = new List<string>
                {
                    "Deploying technique.",
                    "Special maneuver initiated.",
                    "Maximum efficiency mode."
                },
                
                TechnicalLines = new List<string>
                {
                    "Technical damage sequence initiated.",
                    "Status vulnerability detected and exploited.",
                    "Combo chain executing."
                },
                
                PredictionLines = new List<string>
                {
                    "Behavioral pattern recognized.",
                    "Movement predicted. Counter deployed.",
                    "Action anticipated with 98% certainty."
                },
                
                HitReactions = new List<string>
                {
                    "Damage registered.",
                    "Adjusting tactics.",
                    "Calculating countermeasure."
                },
                
                MissReactions = new List<string>
                {
                    "Attack failed. Recalibrating.",
                    "Miss registered. Data logged.",
                    "Ineffective approach noted."
                }
            };
        }
    }
}