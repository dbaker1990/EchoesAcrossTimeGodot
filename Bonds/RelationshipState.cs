// Scripts/System/Relationship/RelationshipState.cs
using Godot;

public partial class RelationshipState : RefCounted
{
    public string CandidateId;  // e.g., "elara", "seraphine", "naledi"
    public int Points;
    public string Stage;        // "Acquaintance","Confidant","Promise","Betrothal"
    public bool LockedIn;       // once true, others cannot reach Betrothal
}