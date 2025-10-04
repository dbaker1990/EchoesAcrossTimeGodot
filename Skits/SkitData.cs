// Skits/SkitData.cs
using Godot;
using System.Collections.Generic;

namespace EchoesAcrossTime.Skits
{
    /// <summary>
    /// Individual line in a skit conversation
    /// </summary>
    [GlobalClass]
    public partial class SkitLine : Resource
    {
        [Export] public string CharacterId { get; set; } = "";
        [Export(PropertyHint.MultilineText)] public string Text { get; set; } = "";
        [Export] public string PortraitPath { get; set; } = "";
        [Export] public SkitEmotion Emotion { get; set; } = SkitEmotion.Normal;
        [Export] public string VoiceClipPath { get; set; } = "";
        [Export] public float DisplayDuration { get; set; } = -1f; // -1 = wait for input
    }

    /// <summary>
    /// Emotions for portrait variations (Tales style)
    /// </summary>
    public enum SkitEmotion
    {
        Normal,
        Happy,
        Sad,
        Angry,
        Surprised,
        Worried,
        Embarrassed,
        Determined,
        Thoughtful
    }

    /// <summary>
    /// Complete skit data - Tales of Series style
    /// </summary>
    [GlobalClass]
    public partial class SkitData : Resource
    {
        [Export] public string SkitId { get; set; } = "skit_001";
        [Export] public string SkitTitle { get; set; } = "Untitled Skit";
        [Export] public Texture2D ThumbnailIcon { get; set; }
        
        [ExportGroup("Participants")]
        [Export] public Godot.Collections.Array<string> ParticipantIds { get; set; }
        
        [ExportGroup("Content")]
        [Export] public Godot.Collections.Array<SkitLine> Lines { get; set; }
        
        [ExportGroup("Trigger Conditions")]
        [Export] public SkitTriggerType TriggerType { get; set; } = SkitTriggerType.Manual;
        [Export] public string TriggerLocation { get; set; } = ""; // Map name or area
        [Export] public string RequiredFlag { get; set; } = ""; // Game flag requirement
        [Export] public int MinimumChapter { get; set; } = 1;
        [Export] public bool RequiresAllParticipantsInParty { get; set; } = true;
        [Export] public bool OnceOnly { get; set; } = true;
        
        [ExportGroup("Audio")]
        [Export] public AudioStream BackgroundMusic { get; set; }
        [Export] public bool StopCurrentMusic { get; set; } = false;
        
        public SkitData()
        {
            ParticipantIds = new Godot.Collections.Array<string>();
            Lines = new Godot.Collections.Array<SkitLine>();
        }

        /// <summary>
        /// Check if skit can be triggered
        /// </summary>
        public bool CanTrigger(List<string> currentParty, int currentChapter, List<string> clearedFlags)
        {
            // Check chapter requirement
            if (currentChapter < MinimumChapter)
                return false;

            // Check required flag
            if (!string.IsNullOrEmpty(RequiredFlag) && !clearedFlags.Contains(RequiredFlag))
                return false;

            // Check party composition
            if (RequiresAllParticipantsInParty)
            {
                foreach (var participantId in ParticipantIds)
                {
                    if (!currentParty.Contains(participantId))
                        return false;
                }
            }

            return true;
        }
    }

    /// <summary>
    /// How the skit is triggered
    /// </summary>
    public enum SkitTriggerType
    {
        Manual,          // Triggered by player action
        Automatic,       // Triggers automatically when conditions met
        Location,        // Triggers at specific location
        StoryEvent,      // Triggers after story event
        AfterBattle,     // Triggers after battle
        ItemObtained,    // Triggers when item obtained
        LevelUp          // Triggers on level up
    }
}