// LoreCodex/ExampleLoreEntries.cs
using Godot;
using EchoesAcrossTime.LoreCodex;

namespace EchoesAcrossTime.Examples
{
    /// <summary>
    /// Example lore entries to demonstrate the system
    /// Use these as templates for your own lore!
    /// </summary>
    public static class ExampleLoreEntries
    {
        /// <summary>
        /// Create a character lore entry
        /// </summary>
        public static LoreEntryData CreateCharacterEntry()
        {
            var entry = new LoreEntryData
            {
                EntryId = "dominic_ashford",
                EntryName = "Dominic Ashford",
                Category = LoreCategory.Character,
                
                ShortDescription = "A young mage seeking to uncover his family's lost legacy.",
                
                DetailedDescription = @"Dominic Ashford is a 17-year-old mage from the coastal town of Rivermist. 
After discovering an ancient tome in his late father's study, he embarks on a journey to understand the 
mysterious powers awakening within him.

His family has a long history of magical research, particularly in the field of temporal magic. However, 
much of this knowledge was lost when his father, Marcus Ashford, mysteriously disappeared five years ago.

Dominic is determined, clever, and somewhat reckless in his pursuit of the truth. He often acts before 
thinking, which frequently lands him in dangerous situations. Despite this, his natural talent for magic 
and his unwavering resolve make him a formidable ally.",
                
                Era = "Modern Era (Year 1247)",
                Location = "Rivermist",
                Author = "Collected by Archivist Helena",
                DateWritten = "Spring, 1247",
                
                StartsUnlocked = true // Main character - available from start
            };

            // Add sections for progressive discovery
            entry.Sections.Add(new LoreSection
            {
                SectionTitle = "Early Life",
                Content = @"Born in Rivermist to Marcus and Eleanor Ashford, Dominic grew up surrounded by 
books and magical artifacts. His mother, a talented herbalist, taught him the basics of potion-making, while 
his father introduced him to the theoretical aspects of magic.

The disappearance of Marcus when Dominic was only 12 left a profound impact on the young boy. He spent 
his teenage years searching for any clues about his father's fate, neglecting his formal magical education 
in favor of independent research.",
                IsLocked = false
            });

            entry.Sections.Add(new LoreSection
            {
                SectionTitle = "The Awakening",
                Content = @"On his 17th birthday, Dominic experienced a powerful magical awakening. While 
examining his father's research notes, he accidentally activated an ancient spell circle hidden beneath 
the family manor.

The resulting surge of temporal magic revealed visions of the past and glimpses of possible futures. This 
event not only unlocked Dominic's latent magical abilities but also set him on the path that would lead 
to his greatest adventure.",
                IsLocked = true,
                UnlockCondition = "Complete Prologue"
            });

            entry.Sections.Add(new LoreSection
            {
                SectionTitle = "The Truth",
                Content = @"[This section contains major spoilers and will be unlocked later in the story]",
                IsLocked = true,
                UnlockCondition = "Complete Chapter 5"
            });

            // Related entries
            entry.RelatedEntryIds.Add("rivermist");
            entry.RelatedEntryIds.Add("marcus_ashford");
            entry.RelatedEntryIds.Add("temporal_magic");

            return entry;
        }

        /// <summary>
        /// Create a location lore entry
        /// </summary>
        public static LoreEntryData CreateLocationEntry()
        {
            var entry = new LoreEntryData
            {
                EntryId = "crystal_cove",
                EntryName = "Crystal Cove",
                Category = LoreCategory.Location,
                
                ShortDescription = "A hidden bay where magical crystals grow naturally from the seafloor.",
                
                DetailedDescription = @"Crystal Cove is a secluded coastal area located south of Rivermist, 
accessible only during low tide through a narrow passage between towering cliffs. The cove is famous for 
its naturally occurring magical crystals that grow in shallow pools.

Legends say these crystals are the tears of the ancient sea goddess Meridia, solidified by time and infused 
with magical energy. Scholars debate this origin, but none can deny the potent magical properties of the 
cove's crystals.

The cove has become a gathering place for mages and alchemists seeking rare components. However, the 
local authorities strictly regulate crystal harvesting to prevent environmental damage.",
                
                Era = "Ancient Origin, Modern Discovery (Year 1180)",
                Location = "South Coast, Near Rivermist",
                Author = "Marine Geologist Thalen Wavecrest",
                
                StartsUnlocked = false,
                UnlockSwitchId = "visited_crystal_cove"
            };

            entry.Sections.Add(new LoreSection
            {
                SectionTitle = "Geological Features",
                Content = @"The cove spans approximately 200 meters in diameter, with crystal formations 
concentrated in three main tidal pools. The largest crystals can reach up to 2 meters in height and emit 
a soft blue-green luminescence, especially during full moons.

The crystal growth rate is remarkably slow - approximately 1 centimeter per century. This has led to 
strict conservation laws protecting the cove from over-harvesting.",
                IsLocked = false
            });

            entry.Sections.Add(new LoreSection
            {
                SectionTitle = "The Meridia Legend",
                Content = @"According to ancient texts, the sea goddess Meridia fell in love with a mortal 
sailor. When he perished in a storm, her tears fell into the ocean and crystallized, creating the first 
magical crystals.

Whether myth or reality, the legend has been passed down for over a thousand years, and many believe the 
cove holds a deeper connection to the divine than modern science can explain.",
                IsLocked = false
            });

            entry.RelatedEntryIds.Add("meridia_goddess");
            entry.RelatedEntryIds.Add("magical_crystals");
            entry.RelatedEntryIds.Add("rivermist");

            return entry;
        }

        /// <summary>
        /// Create a historical event entry
        /// </summary>
        public static LoreEntryData CreateEventEntry()
        {
            var entry = new LoreEntryData
            {
                EntryId = "the_great_sundering",
                EntryName = "The Great Sundering",
                Category = LoreCategory.Event,
                
                ShortDescription = "The cataclysmic event that split the continent 500 years ago.",
                
                DetailedDescription = @"Five hundred years ago, a magical catastrophe of unprecedented scale 
tore the continent of Aethoria in two. Known as the Great Sundering, this event created the Endless Rift - 
a mile-wide chasm of churning magical energy that separates the Eastern and Western lands to this day.

The exact cause of the Sundering remains a subject of intense scholarly debate. Most historians agree it 
was the result of a magical experiment gone catastrophically wrong, though details are scarce due to the 
loss of so many historical records during the event itself.

The Sundering claimed millions of lives and reshaped the political, geographical, and magical landscape 
of the world forever.",
                
                Era = "Age of Legends (Year 747)",
                Author = "Grand Historian Elara Timesong",
                DateWritten = "Year 1240",
                
                StartsUnlocked = false,
                UnlockQuestId = "learn_about_history"
            };

            entry.Sections.Add(new LoreSection
            {
                SectionTitle = "The Catalyst",
                Content = @"Historical evidence suggests the Sundering was triggered by a group of seven 
archmages known as the Concordat. They were attempting to create a new source of magical energy to power 
their civilization, but the ritual spiraled out of control.

Within moments, the fabric of reality itself began to tear. The archmages' tower, located at what is now 
the center of the Endless Rift, was completely annihilated. The magical shockwave spread across the 
continent, splitting it in two.",
                IsLocked = false
            });

            entry.Sections.Add(new LoreSection
            {
                SectionTitle = "Immediate Aftermath",
                Content = @"The days following the Sundering were chaotic. Entire cities found themselves 
split between the two new continents. Families were separated, trade routes destroyed, and governments 
collapsed.

The magical fallout from the event lasted for months. Strange phenomena occurred worldwide - spontaneous 
magical surges, temporal distortions, and the emergence of new magical creatures. Many believe the world's 
magic was fundamentally altered by the Sundering.",
                IsLocked = false
            });

            entry.Sections.Add(new LoreSection
            {
                SectionTitle = "Modern Impact",
                Content = @"Five centuries later, the Endless Rift remains impassable by conventional means. 
Several attempts to bridge the gap have failed, often catastrophically. The Eastern and Western continents 
have developed separately, with distinct cultures, governments, and magical traditions.

Recent developments in teleportation magic have allowed limited communication between the continents, but 
travel remains dangerous and rare. Some scholars believe the Rift may be gradually shrinking, while others 
fear it could expand further.",
                IsLocked = false
            });

            entry.Sections.Add(new LoreSection
            {
                SectionTitle = "The Concordat's Legacy",
                Content = @"The seven archmages responsible for the Sundering are reviled as history's 
greatest villains by some, while others view them as tragic heroes who sacrificed themselves trying to 
advance civilization.

Recent discoveries suggest not all members of the Concordat perished in the Sundering. There are whispers 
of survivors, transformed by the magical energies they unleashed, still walking the world today...",
                IsLocked = true,
                UnlockCondition = "Complete Chapter 7"
            });

            entry.RelatedEntryIds.Add("endless_rift");
            entry.RelatedEntryIds.Add("the_concordat");
            entry.RelatedEntryIds.Add("age_of_legends");
            entry.RelatedEntryIds.Add("eastern_continent");
            entry.RelatedEntryIds.Add("western_continent");

            return entry;
        }

        /// <summary>
        /// Create a magical concept entry
        /// </summary>
        public static LoreEntryData CreateConceptEntry()
        {
            var entry = new LoreEntryData
            {
                EntryId = "temporal_magic",
                EntryName = "Temporal Magic",
                Category = LoreCategory.Concept,
                
                ShortDescription = "The rare and dangerous art of manipulating time itself.",
                
                DetailedDescription = @"Temporal magic is one of the rarest and most dangerous branches of 
magical study. Unlike elemental magic or healing, which interact with the physical world, temporal magic 
reaches into the fundamental fabric of reality itself.

Only a handful of mages in each generation possess the aptitude for temporal magic. The practice requires 
not only immense magical power but also a unique mental discipline to perceive and manipulate time flows 
without becoming lost in paradox.

Due to the catastrophic potential of temporal magic gone wrong, its study is heavily regulated by magical 
councils worldwide. Unauthorized temporal experimentation is considered a capital offense in most nations.",
                
                Era = "First Documented: Age of Wonders (Year 200)",
                Author = "Chronomancer Aldric Stormweaver",
                
                StartsUnlocked = false,
                UnlockSwitchId = "learned_temporal_magic"
            };

            entry.Sections.Add(new LoreSection
            {
                SectionTitle = "Basic Principles",
                Content = @"Temporal magic operates on the principle that time is not a constant flow but 
a complex web of interconnected moments. Skilled practitioners can perceive this web and carefully adjust 
individual threads without unraveling the whole.

The simplest temporal spells involve minor time dilation - speeding up or slowing down localized areas. 
More advanced techniques include glimpsing possible futures, viewing the past, or even creating small 
temporal loops.",
                IsLocked = false
            });

            entry.Sections.Add(new LoreSection
            {
                SectionTitle = "Dangers and Paradoxes",
                Content = @"The primary danger of temporal magic is the creation of paradoxes. When the 
past is altered or the future is changed in ways that contradict itself, reality attempts to resolve the 
conflict. This resolution often manifests as violent magical backlash.

Lesser paradoxes might cause localized temporal distortions - areas where time flows incorrectly. Major 
paradoxes can create temporal rifts, tears in reality that leak entropic energy and can unmake matter itself.

The Great Sundering itself may have been the result of a catastrophic temporal paradox, though this remains 
unconfirmed.",
                IsLocked = true,
                UnlockCondition = "Reach Chronomancer Rank 3"
            });

            entry.RelatedEntryIds.Add("dominic_ashford");
            entry.RelatedEntryIds.Add("marcus_ashford");
            entry.RelatedEntryIds.Add("the_great_sundering");

            return entry;
        }

        /// <summary>
        /// Register all example lore entries
        /// </summary>
        public static void RegisterAllExamples()
        {
            if (LoreCodexManager.Instance == null)
            {
                GD.PrintErr("LoreCodexManager not initialized!");
                return;
            }

            LoreCodexManager.Instance.RegisterLoreEntry(CreateCharacterEntry());
            LoreCodexManager.Instance.RegisterLoreEntry(CreateLocationEntry());
            LoreCodexManager.Instance.RegisterLoreEntry(CreateEventEntry());
            LoreCodexManager.Instance.RegisterLoreEntry(CreateConceptEntry());

            GD.Print("Example lore entries registered!");
        }
    }
}