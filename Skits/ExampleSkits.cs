// Skits/ExampleSkits.cs
using Godot;

namespace EchoesAcrossTime.Skits
{
    /// <summary>
    /// Example skits for testing and reference
    /// You can create these as .tres resources or programmatically
    /// </summary>
    public static class ExampleSkits
    {
        /// <summary>
        /// Create a simple two-character skit
        /// </summary>
        public static SkitData CreateExampleSkit()
        {
            var skit = new SkitData
            {
                SkitId = "skit_example_001",
                SkitTitle = "First Meeting",
                TriggerType = SkitTriggerType.Manual,
                RequiresAllParticipantsInParty = true,
                OnceOnly = true
            };

            // Add participants
            skit.ParticipantIds.Add("dominic");
            skit.ParticipantIds.Add("aria");

            // Line 1: Dominic speaks
            var line1 = new SkitLine
            {
                CharacterId = "dominic",
                Text = "So this is the famous Ice Princess of the Northern Kingdom...",
                PortraitPath = "res://Graphics/Portraits/Dominic_Normal.png",
                Emotion = SkitEmotion.Thoughtful
            };
            skit.Lines.Add(line1);

            // Line 2: Aria responds
            var line2 = new SkitLine
            {
                CharacterId = "aria",
                Text = "And you must be the Shadow Prince I've heard so much about.",
                PortraitPath = "res://Graphics/Portraits/Aria_Normal.png",
                Emotion = SkitEmotion.Normal
            };
            skit.Lines.Add(line2);

            // Line 3: Dominic again
            var line3 = new SkitLine
            {
                CharacterId = "dominic",
                Text = "I suppose our reputations precede us. Though I wonder if yours is as accurate as mine.",
                PortraitPath = "res://Graphics/Portraits/Dominic_Smirk.png",
                Emotion = SkitEmotion.Happy
            };
            skit.Lines.Add(line3);

            // Line 4: Aria (annoyed)
            var line4 = new SkitLine
            {
                CharacterId = "aria",
                Text = "Is that supposed to be charming? Because it's not working.",
                PortraitPath = "res://Graphics/Portraits/Aria_Annoyed.png",
                Emotion = SkitEmotion.Angry
            };
            skit.Lines.Add(line4);

            // Line 5: Dominic (embarrassed)
            var line5 = new SkitLine
            {
                CharacterId = "dominic",
                Text = "I... That's not what I meant. I was trying to—",
                PortraitPath = "res://Graphics/Portraits/Dominic_Embarrassed.png",
                Emotion = SkitEmotion.Embarrassed
            };
            skit.Lines.Add(line5);

            // Line 6: Aria (laughing)
            var line6 = new SkitLine
            {
                CharacterId = "aria",
                Text = "Relax, Shadow Prince. I'm just teasing. You should see your face right now!",
                PortraitPath = "res://Graphics/Portraits/Aria_Happy.png",
                Emotion = SkitEmotion.Happy
            };
            skit.Lines.Add(line6);

            return skit;
        }

        /// <summary>
        /// Create a three-character skit
        /// </summary>
        public static SkitData CreateThreeCharacterSkit()
        {
            var skit = new SkitData
            {
                SkitId = "skit_example_002",
                SkitTitle = "The Plan",
                TriggerType = SkitTriggerType.StoryEvent,
                RequiresAllParticipantsInParty = true,
                MinimumChapter = 2
            };

            skit.ParticipantIds.Add("dominic");
            skit.ParticipantIds.Add("aria");
            skit.ParticipantIds.Add("echo");

            // Dominic
            skit.Lines.Add(new SkitLine
            {
                CharacterId = "dominic",
                Text = "We need a better plan than just 'walk in and hope for the best.'",
                PortraitPath = "res://Graphics/Portraits/Dominic_Worried.png",
                Emotion = SkitEmotion.Worried
            });

            // Echo Walker
            skit.Lines.Add(new SkitLine
            {
                CharacterId = "echo",
                Text = "In my timeline, we tried that. It didn't end well.",
                PortraitPath = "res://Graphics/Portraits/Echo_Sad.png",
                Emotion = SkitEmotion.Sad
            });

            // Aria
            skit.Lines.Add(new SkitLine
            {
                CharacterId = "aria",
                Text = "Then we'll do something different this time. We have an advantage they don't expect.",
                PortraitPath = "res://Graphics/Portraits/Aria_Determined.png",
                Emotion = SkitEmotion.Determined
            });

            // Dominic
            skit.Lines.Add(new SkitLine
            {
                CharacterId = "dominic",
                Text = "Which is?",
                PortraitPath = "res://Graphics/Portraits/Dominic_Normal.png",
                Emotion = SkitEmotion.Normal
            });

            // Aria
            skit.Lines.Add(new SkitLine
            {
                CharacterId = "aria",
                Text = "Each other. Together, we're stronger than any of us alone.",
                PortraitPath = "res://Graphics/Portraits/Aria_Happy.png",
                Emotion = SkitEmotion.Happy
            });

            return skit;
        }

        /// <summary>
        /// Create a location-based skit
        /// </summary>
        public static SkitData CreateLocationSkit()
        {
            var skit = new SkitData
            {
                SkitId = "skit_location_temple",
                SkitTitle = "Ancient Temple",
                TriggerType = SkitTriggerType.Location,
                TriggerLocation = "AncientTemple",
                RequiresAllParticipantsInParty = false, // Only needs one participant
                OnceOnly = true
            };

            skit.ParticipantIds.Add("echo");

            skit.Lines.Add(new SkitLine
            {
                CharacterId = "echo",
                Text = "This place... I remember it from the other timeline. But it was in ruins there.",
                PortraitPath = "res://Graphics/Portraits/Echo_Surprised.png",
                Emotion = SkitEmotion.Surprised
            });

            skit.Lines.Add(new SkitLine
            {
                CharacterId = "echo",
                Text = "Seeing it intact like this... it gives me hope that we can change things.",
                PortraitPath = "res://Graphics/Portraits/Echo_Happy.png",
                Emotion = SkitEmotion.Happy
            });

            return skit;
        }

        /// <summary>
        /// Create a post-battle skit
        /// </summary>
        public static SkitData CreateBattleSkit()
        {
            var skit = new SkitData
            {
                SkitId = "skit_after_boss_001",
                SkitTitle = "Victory Celebration",
                TriggerType = SkitTriggerType.AfterBattle,
                RequiredFlag = "boss_defeated_temple",
                RequiresAllParticipantsInParty = true
            };

            skit.ParticipantIds.Add("dominic");
            skit.ParticipantIds.Add("aria");

            skit.Lines.Add(new SkitLine
            {
                CharacterId = "aria",
                Text = "That was incredible! Your shadow magic synced perfectly with my ice!",
                PortraitPath = "res://Graphics/Portraits/Aria_Happy.png",
                Emotion = SkitEmotion.Happy
            });

            skit.Lines.Add(new SkitLine
            {
                CharacterId = "dominic",
                Text = "We make a good team. Who would have thought?",
                PortraitPath = "res://Graphics/Portraits/Dominic_Happy.png",
                Emotion = SkitEmotion.Happy
            });

            skit.Lines.Add(new SkitLine
            {
                CharacterId = "aria",
                Text = "I did. From the moment I met you, I knew we'd be unstoppable together.",
                PortraitPath = "res://Graphics/Portraits/Aria_Determined.png",
                Emotion = SkitEmotion.Determined
            });

            return skit;
        }
    }
}