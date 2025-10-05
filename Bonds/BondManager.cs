// Scripts/System/Bonds/BondManager.cs
using Godot;
using Godot.Collections;
using System;
using System.Linq;

public partial class BondManager : Node
{
    [Export] public BondConfig Config;
    // Key = $"{min(A,B)}|{max(A,B)}"
    private readonly Dictionary<string, BondState> _pairs = new();

    // Signals
    [Signal] public delegate void BondChangedEventHandler(string a, string b, int newPoints, string newRank);
    [Signal] public delegate void BondRankUpEventHandler(string a, string b, string newRank);

    public override void _Ready()
    {
        if (Config == null) GD.PushWarning("BondManager: Config not set.");
    }

    private string Key(string a, string b) => (string.Compare(a,b,StringComparison.Ordinal) < 0) ? $"{a}|{b}" : $"{b}|{a}";

    public int GetPoints(string a, string b)
    {
        if (_pairs.TryGetValue(Key(a,b), out var s)) return s.Points;
        return 0;
    }

    public string GetRank(int points)
    {
        var idx = 0;
        for (int i=0;i<Config.RankThresholds.Count;i++)
            if (points >= Config.RankThresholds[i]) idx = i;
        return Config.RankLetters[Math.Min(idx, Config.RankLetters.Count-1)];
    }

    public void AddPoints(string a, string b, int rawAmount, string reason, bool campfireBonus = false, int currentDay = 0)
    {
        if (a == b) return;
        var k = Key(a,b);
        if (!_pairs.TryGetValue(k, out var s))
        {
            s = new BondState { A = a, B = b, Points = 0, LastGainDay = -999 };
            _pairs[k] = s;
        }

        // Daily cap per pair
        var gainedToday = (s.LastGainDay == currentDay) ? 1 : 0; // track coarse; simple per-event cap gate
        if (gainedToday >= Config.DailyCapPerPair) return;

        var amount = rawAmount;
        if (campfireBonus) amount = Mathf.CeilToInt(amount * Config.CampfireBonus);

        var beforeRank = GetRank(s.Points);
        s.Points = Mathf.Clamp(s.Points + amount, 0, Config.MaxPoints);
        s.LastGainDay = currentDay;

        var afterRank = GetRank(s.Points);
        EmitSignal(SignalName.BondChanged, s.A, s.B, s.Points, afterRank);
        if (afterRank != beforeRank) EmitSignal(SignalName.BondRankUp, s.A, s.B, afterRank);
    }
    
    

    // Convenience hooks you’ll call from game systems
    public void OnAllyHealed(string healerId, string targetId)    => AddPoints(healerId,targetId, 2, "heal");
    public void OnGuarded(string guardId, string targetId)        => AddPoints(guardId,targetId, 2, "guard");
    public void OnAssistKill(string aId, string bId)              => AddPoints(aId,bId, 1, "assist");
    public void OnRevive(string rescuerId, string targetId)       => AddPoints(rescuerId,targetId, 4, "revive");
    public void OnCampDialogue(string aId, string bId, bool good) => AddPoints(aId,bId, good?3:1, "camp", campfireBonus:true);
}
