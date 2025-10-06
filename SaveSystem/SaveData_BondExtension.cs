// SaveSystem/SaveData_BondExtension.cs
using System;

namespace EchoesAcrossTime
{
    /// <summary>
    /// Extension to SaveData to include Bond and Relationship data
    /// This shows what to ADD to your existing SaveData.cs file
    /// </summary>
    public partial class SaveData
    {
        // ==============================================
        // ADD THESE PROPERTIES TO YOUR SaveData CLASS
        // ==============================================
        
        /// <summary>
        /// Party member bond data
        /// </summary>
        public BondSaveData Bonds { get; set; } = new BondSaveData();
        
        /// <summary>
        /// Romance relationship data
        /// </summary>
        public RelationshipSaveData Relationships { get; set; } = new RelationshipSaveData();
        
        
        // ==============================================
        // UPDATE InitializeNewGame METHOD
        // ==============================================
        // Add this code to your existing InitializeNewGame method:
        
        /*
        // Initialize empty bond data
        Bonds = new BondSaveData
        {
            BondPairs = new Dictionary<string, BondPairData>(),
            CurrentGameDay = 0
        };
        
        // Initialize relationship data with the three romance candidates
        Relationships = new RelationshipSaveData
        {
            Candidates = new Dictionary<string, RelationshipCandidateData>
            {
                { "elara", new RelationshipCandidateData { CandidateId = "elara", Points = 0, Stage = "Acquaintance" } },
                { "seraphine", new RelationshipCandidateData { CandidateId = "seraphine", Points = 0, Stage = "Acquaintance" } },
                { "naledi", new RelationshipCandidateData { CandidateId = "naledi", Points = 0, Stage = "Acquaintance" } }
            },
            LockedInCandidateId = "",
            LockinWindowOpen = false,
            LockinWindowClosed = false
        };
        */
        
        
        // ==============================================
        // UPDATE CaptureCurrentState METHOD
        // ==============================================
        // Add this code to your existing CaptureCurrentState method:
        
        /*
        // Capture bond data
        if (BondManager.Instance != null)
        {
            Bonds = BondManager.Instance.GetSaveData();
        }
        
        // Capture relationship data
        if (RelationshipManager.Instance != null)
        {
            Relationships = RelationshipManager.Instance.GetSaveData();
        }
        */
        
        
        // ==============================================
        // UPDATE ApplyToGame METHOD
        // ==============================================
        // Add this code to your existing ApplyToGame method:
        
        /*
        // Apply bond data
        if (Bonds != null && BondManager.Instance != null)
        {
            BondManager.Instance.LoadSaveData(Bonds);
        }
        
        // Apply relationship data
        if (Relationships != null && RelationshipManager.Instance != null)
        {
            RelationshipManager.Instance.LoadSaveData(Relationships);
        }
        */
    }
}

// ==============================================
// EXAMPLE OF COMPLETE UPDATED METHODS
// ==============================================

/*
// This is what your updated InitializeNewGame should look like:
public void InitializeNewGame(GameDatabase database)
{
    // ... your existing initialization code ...
    
    // Initialize bonds
    Bonds = new BondSaveData
    {
        BondPairs = new Dictionary<string, BondPairData>(),
        CurrentGameDay = 0
    };
    
    // Initialize relationships
    Relationships = new RelationshipSaveData
    {
        Candidates = new Dictionary<string, RelationshipCandidateData>
        {
            { "elara", new RelationshipCandidateData { CandidateId = "elara", Points = 0, Stage = "Acquaintance" } },
            { "seraphine", new RelationshipCandidateData { CandidateId = "seraphine", Points = 0, Stage = "Acquaintance" } },
            { "naledi", new RelationshipCandidateData { CandidateId = "naledi", Points = 0, Stage = "Acquaintance" } }
        },
        LockedInCandidateId = "",
        LockinWindowOpen = false,
        LockinWindowClosed = false
    };
    
    GD.Print("New game initialized with bonds and relationships");
}

// This is what your updated CaptureCurrentState should look like:
public void CaptureCurrentState()
{
    SaveDateTime = DateTime.Now;
    
    // ... your existing capture code ...
    
    // Capture bond data
    if (BondManager.Instance != null)
    {
        Bonds = BondManager.Instance.GetSaveData();
    }
    
    // Capture relationship data
    if (RelationshipManager.Instance != null)
    {
        Relationships = RelationshipManager.Instance.GetSaveData();
    }
}

// This is what your updated ApplyToGame should look like:
public void ApplyToGame()
{
    // ... your existing apply code ...
    
    // Apply bond data
    if (Bonds != null && BondManager.Instance != null)
    {
        BondManager.Instance.LoadSaveData(Bonds);
    }
    
    // Apply relationship data
    if (Relationships != null && RelationshipManager.Instance != null)
    {
        RelationshipManager.Instance.LoadSaveData(Relationships);
    }
    
    GD.Print("Save data applied to game with bonds and relationships");
}
*/