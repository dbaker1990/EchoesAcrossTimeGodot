// SaveSystem/BondSaveData.cs
using System;
using System.Collections.Generic;

namespace EchoesAcrossTime
{
    /// <summary>
    /// Save data for the Bond system (party member bonds)
    /// </summary>
    [Serializable]
    public class BondSaveData
    {
        /// <summary>
        /// Dictionary of bond pairs
        /// Key format: "{characterA}|{characterB}" (alphabetically sorted)
        /// </summary>
        public Dictionary<string, BondPairData> BondPairs { get; set; } = new Dictionary<string, BondPairData>();
        
        /// <summary>
        /// Current in-game day (for daily cap tracking)
        /// </summary>
        public int CurrentGameDay { get; set; } = 0;
    }

    /// <summary>
    /// Individual bond pair data
    /// </summary>
    [Serializable]
    public class BondPairData
    {
        public string CharacterA { get; set; }
        public string CharacterB { get; set; }
        public int Points { get; set; }
        public int LastGainDay { get; set; }
        public string CurrentRank { get; set; } = "E"; // E, D, C, B, A, S
    }

    /// <summary>
    /// Save data for the Relationship system (romance routes)
    /// </summary>
    [Serializable]
    public class RelationshipSaveData
    {
        /// <summary>
        /// Dictionary of relationship states for each candidate
        /// Key: candidateId (e.g., "elara", "seraphine", "naledi")
        /// </summary>
        public Dictionary<string, RelationshipCandidateData> Candidates { get; set; } = new Dictionary<string, RelationshipCandidateData>();
        
        /// <summary>
        /// ID of the locked-in romance route (if any)
        /// </summary>
        public string LockedInCandidateId { get; set; } = "";
        
        /// <summary>
        /// Whether the lock-in window is currently open
        /// </summary>
        public bool LockinWindowOpen { get; set; } = false;
        
        /// <summary>
        /// Whether the lock-in window has closed
        /// </summary>
        public bool LockinWindowClosed { get; set; } = false;
    }

    /// <summary>
    /// Individual relationship candidate data
    /// </summary>
    [Serializable]
    public class RelationshipCandidateData
    {
        public string CandidateId { get; set; }
        public int Points { get; set; }
        public string Stage { get; set; } = "Acquaintance"; // Acquaintance, Confidant, Promise, Betrothal
        public bool LockedIn { get; set; } = false;
    }
}