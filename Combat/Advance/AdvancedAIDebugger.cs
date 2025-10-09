// Combat/Advanced/AdvancedAIDebugger.cs
// Visual debugging and analysis tool for Advanced AI
using Godot;
using System.Collections.Generic;
using System.Linq;

namespace EchoesAcrossTime.Combat.Advanced
{
    /// <summary>
    /// Real-time debugging overlay for Advanced AI systems
    /// Shows what the AI is thinking and why it makes decisions
    /// </summary>
    public partial class AdvancedAIDebugger : CanvasLayer
    {
        private AdvancedAIPattern trackedAI;
        private VBoxContainer debugPanel;
        private Label statusLabel;
        private Label systemsLabel;
        private Label decisionsLabel;
        private Label patternsLabel;
        private RichTextLabel logLabel;
        
        private List<string> decisionLog = new List<string>();
        private const int MAX_LOG_ENTRIES = 15;
        
        private bool showDebugPanel = true;
        
        public override void _Ready()
        {
            CreateDebugUI();
        }
        
        public override void _Input(InputEvent @event)
        {
            // Toggle debug panel with F3 key
            if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
            {
                if (keyEvent.Keycode == Key.F3)
                {
                    showDebugPanel = !showDebugPanel;
                    debugPanel.Visible = showDebugPanel;
                }
            }
        }
        
        /// <summary>
        /// Attach debugger to an Advanced AI pattern
        /// </summary>
        public void AttachToAI(AdvancedAIPattern ai)
        {
            trackedAI = ai;
            GD.Print("[DEBUGGER] Attached to AI - Press F3 to toggle debug overlay");
        }
        
        /// <summary>
        /// Log an AI decision
        /// </summary>
        public void LogDecision(string decision, string reasoning)
        {
            string timestamp = $"[T{Engine.GetFramesDrawn() / 60}s]";
            string entry = $"{timestamp} {decision}: {reasoning}";
            
            decisionLog.Add(entry);
            if (decisionLog.Count > MAX_LOG_ENTRIES)
                decisionLog.RemoveAt(0);
            
            GD.Print($"[AI DEBUG] {entry}");
        }
        
        public override void _Process(double delta)
        {
            if (trackedAI == null || !showDebugPanel) return;
            
            UpdateDebugDisplay();
        }
        
        private void CreateDebugUI()
        {
            // Main container
            debugPanel = new VBoxContainer();
            debugPanel.Position = new Vector2(10, 10);
            debugPanel.CustomMinimumSize = new Vector2(500, 600);
            AddChild(debugPanel);
            
            // Background panel
            var panel = new Panel();
            panel.CustomMinimumSize = new Vector2(500, 600);
            debugPanel.AddChild(panel);
            panel.Modulate = new Color(0, 0, 0, 0.85f);
            
            // Title
            var titleLabel = new Label();
            titleLabel.Text = "🧠 ADVANCED AI DEBUGGER";
            titleLabel.AddThemeFontSizeOverride("font_size", 20);
            titleLabel.AddThemeColorOverride("font_color", Colors.Cyan);
            debugPanel.AddChild(titleLabel);
            
            var separator1 = new HSeparator();
            debugPanel.AddChild(separator1);
            
            // Status section
            statusLabel = new Label();
            statusLabel.Text = "Status: Not Attached";
            statusLabel.AddThemeFontSizeOverride("font_size", 14);
            debugPanel.AddChild(statusLabel);
            
            var separator2 = new HSeparator();
            debugPanel.AddChild(separator2);
            
            // Active systems
            var systemsTitle = new Label();
            systemsTitle.Text = "ACTIVE SYSTEMS:";
            systemsTitle.AddThemeColorOverride("font_color", Colors.Yellow);
            debugPanel.AddChild(systemsTitle);
            
            systemsLabel = new Label();
            systemsLabel.AutowrapMode = TextServer.AutowrapMode.Word;
            debugPanel.AddChild(systemsLabel);
            
            var separator3 = new HSeparator();
            debugPanel.AddChild(separator3);
            
            // Current decision info
            var decisionsTitle = new Label();
            decisionsTitle.Text = "CURRENT STATE:";
            decisionsTitle.AddThemeColorOverride("font_color", Colors.Yellow);
            debugPanel.AddChild(decisionsTitle);
            
            decisionsLabel = new Label();
            decisionsLabel.AutowrapMode = TextServer.AutowrapMode.Word;
            debugPanel.AddChild(decisionsLabel);
            
            var separator4 = new HSeparator();
            debugPanel.AddChild(separator4);
            
            // Learned patterns
            var patternsTitle = new Label();
            patternsTitle.Text = "LEARNED PATTERNS:";
            patternsTitle.AddThemeColorOverride("font_color", Colors.Yellow);
            debugPanel.AddChild(patternsTitle);
            
            patternsLabel = new Label();
            patternsLabel.AutowrapMode = TextServer.AutowrapMode.Word;
            debugPanel.AddChild(patternsLabel);
            
            var separator5 = new HSeparator();
            debugPanel.AddChild(separator5);
            
            // Decision log
            var logTitle = new Label();
            logTitle.Text = "DECISION LOG:";
            logTitle.AddThemeColorOverride("font_color", Colors.Yellow);
            debugPanel.AddChild(logTitle);
            
            logLabel = new RichTextLabel();
            logLabel.CustomMinimumSize = new Vector2(480, 200);
            logLabel.BbcodeEnabled = true;
            logLabel.ScrollFollowing = true;
            debugPanel.AddChild(logLabel);
            
            // Controls hint
            var controlsLabel = new Label();
            controlsLabel.Text = "Press F3 to toggle | F4 for stats dump";
            controlsLabel.AddThemeFontSizeOverride("font_size", 12);
            controlsLabel.AddThemeColorOverride("font_color", Colors.Gray);
            debugPanel.AddChild(controlsLabel);
        }
        
        private void UpdateDebugDisplay()
        {
            if (trackedAI == null) return;
            
            // Update status
            statusLabel.Text = $"Status: ✓ Attached | Personality: {trackedAI.CurrentPersonality}";
            
            // Update active systems
            var systems = new List<string>();
            if (trackedAI.UsesMultiTurnStrategy) systems.Add("✓ Multi-Turn Strategy");
            if (trackedAI.CoordinatesWithAllies) systems.Add("✓ Party Coordination");
            if (trackedAI.AdaptsToPlayerSkill) systems.Add("✓ Adaptive Learning");
            if (trackedAI.UsesRiskCalculation) systems.Add("✓ Risk Assessment");
            if (trackedAI.UsesTacticalPositioning) systems.Add("✓ Formation/Positioning");
            if (trackedAI.ManagesResourcesStrategically) systems.Add("✓ Resource Management");
            if (trackedAI.HasPersonalityShifts) systems.Add("✓ Personality Shifts");
            if (trackedAI.PredictsPlayerMoves) systems.Add("✓ Counter/Prediction");
            
            systemsLabel.Text = systems.Count > 0 ? string.Join("\n", systems) : "No systems active";
            
            // Update current state
            var stateInfo = new List<string>();
            stateInfo.Add($"Behavior: {trackedAI.BehaviorType}");
            stateInfo.Add($"Aggression: {trackedAI.Aggression}");
            stateInfo.Add($"Recklessness: {trackedAI.Recklessness}");
            
            if (trackedAI.UsesMultiTurnStrategy)
            {
                stateInfo.Add($"Planning Depth: {trackedAI.PlanningDepth} turns");
            }
            
            if (trackedAI.AdaptsToPlayerSkill)
            {
                stateInfo.Add($"Difficulty: {trackedAI.BaseDifficultyLevel}%");
            }
            
            if (trackedAI.UsesRiskCalculation)
            {
                stateInfo.Add($"Risk Tolerance: {trackedAI.RiskTolerance}");
            }
            
            if (trackedAI.PredictsPlayerMoves)
            {
                stateInfo.Add($"Prediction Accuracy: {trackedAI.PredictionAccuracy}%");
            }
            
            decisionsLabel.Text = string.Join("\n", stateInfo);
            
            // Update patterns (mock for now - would need to expose from AdvancedAIPattern)
            patternsLabel.Text = "Tracking player actions...\n(Pattern data would appear here)";
            
            // Update log
            logLabel.Text = string.Join("\n", decisionLog.TakeLast(MAX_LOG_ENTRIES));
        }
        
        /// <summary>
        /// Generate comprehensive AI stats report
        /// </summary>
        public void GenerateStatsReport()
        {
            if (trackedAI == null)
            {
                GD.Print("[DEBUGGER] No AI attached");
                return;
            }
            
            GD.Print("\n╔═══════════════════════════════════════════════════════╗");
            GD.Print("║          ADVANCED AI STATISTICS REPORT               ║");
            GD.Print("╚═══════════════════════════════════════════════════════╝\n");
            
            GD.Print("CORE CONFIGURATION:");
            GD.Print($"  Behavior Type: {trackedAI.BehaviorType}");
            GD.Print($"  Target Priority: {trackedAI.TargetPriority}");
            GD.Print($"  Aggression: {trackedAI.Aggression}");
            GD.Print($"  Recklessness: {trackedAI.Recklessness}");
            GD.Print($"  Skill Usage Rate: {trackedAI.SkillUsageRate}%\n");
            
            GD.Print("ACTIVE SYSTEMS:");
            GD.Print($"  [{'✓'}] Multi-Turn Strategy: {trackedAI.UsesMultiTurnStrategy}");
            if (trackedAI.UsesMultiTurnStrategy)
                GD.Print($"      Planning Depth: {trackedAI.PlanningDepth} turns");
            
            GD.Print($"  [{'✓'}] Party Coordination: {trackedAI.CoordinatesWithAllies}");
            if (trackedAI.CoordinatesWithAllies)
                GD.Print($"      Teamwork Priority: {trackedAI.TeamworkPriority}%");
            
            GD.Print($"  [{'✓'}] Adaptive Learning: {trackedAI.AdaptsToPlayerSkill}");
            if (trackedAI.AdaptsToPlayerSkill)
                GD.Print($"      Base Difficulty: {trackedAI.BaseDifficultyLevel}%");
            
            GD.Print($"  [{'✓'}] Risk Assessment: {trackedAI.UsesRiskCalculation}");
            if (trackedAI.UsesRiskCalculation)
                GD.Print($"      Risk Tolerance: {trackedAI.RiskTolerance}");
            
            GD.Print($"  [{'✓'}] Formation/Positioning: {trackedAI.UsesTacticalPositioning}");
            GD.Print($"  [{'✓'}] Resource Management: {trackedAI.ManagesResourcesStrategically}");
            GD.Print($"  [{'✓'}] Personality Shifts: {trackedAI.HasPersonalityShifts}");
            GD.Print($"  [{'✓'}] Counter/Prediction: {trackedAI.PredictsPlayerMoves}\n");
            
            GD.Print("SPECIAL BEHAVIORS:");
            GD.Print($"  Learn Weaknesses: {trackedAI.LearnWeaknesses}");
            GD.Print($"  Exploit Technicals: {trackedAI.ExploitTechnicals}");
            GD.Print($"  Use Defensive Tactics: {trackedAI.UseDefensiveTactics}");
            GD.Print($"  Will Flee: {trackedAI.WillFlee}");
            GD.Print($"  Calls For Help: {trackedAI.CallsForHelp}");
            GD.Print($"  Enrages At Low HP: {trackedAI.EnragesAtLowHP}\n");
            
            GD.Print("THRESHOLDS:");
            GD.Print($"  Low HP: {trackedAI.LowHPThreshold}%");
            GD.Print($"  Defensive: {trackedAI.DefensiveThreshold}%");
            GD.Print($"  Flee: {trackedAI.FleeThreshold}%\n");
            
            GD.Print("═══════════════════════════════════════════════════════\n");
        }
    }
    
    /// <summary>
    /// AI Performance Analyzer - Tracks AI effectiveness
    /// </summary>
    public partial class AIPerformanceAnalyzer : Node
    {
        private Dictionary<string, AIPerformanceMetrics> metrics = new Dictionary<string, AIPerformanceMetrics>();
        
        public void TrackDecision(string aiId, AIDecision decision, bool wasEffective)
        {
            if (!metrics.ContainsKey(aiId))
                metrics[aiId] = new AIPerformanceMetrics();
            
            var aiMetrics = metrics[aiId];
            aiMetrics.TotalDecisions++;
            
            if (wasEffective)
                aiMetrics.EffectiveDecisions++;
            
            aiMetrics.DecisionTypes[decision.ActionType] = 
                aiMetrics.DecisionTypes.GetValueOrDefault(decision.ActionType, 0) + 1;
        }
        
        public void TrackPrediction(string aiId, bool accurate)
        {
            if (!metrics.ContainsKey(aiId))
                metrics[aiId] = new AIPerformanceMetrics();
            
            var aiMetrics = metrics[aiId];
            aiMetrics.PredictionAttempts++;
            
            if (accurate)
                aiMetrics.AccuratePredictions++;
        }
        
        public void PrintReport(string aiId)
        {
            if (!metrics.ContainsKey(aiId))
            {
                GD.Print($"No metrics found for {aiId}");
                return;
            }
            
            var aiMetrics = metrics[aiId];
            float effectiveness = (float)aiMetrics.EffectiveDecisions / aiMetrics.TotalDecisions * 100f;
            float predictionRate = aiMetrics.PredictionAttempts > 0 
                ? (float)aiMetrics.AccuratePredictions / aiMetrics.PredictionAttempts * 100f 
                : 0f;
            
            GD.Print($"\n=== AI Performance Report: {aiId} ===");
            GD.Print($"Total Decisions: {aiMetrics.TotalDecisions}");
            GD.Print($"Effectiveness: {effectiveness:F1}%");
            GD.Print($"Prediction Accuracy: {predictionRate:F1}%");
            GD.Print($"\nDecision Breakdown:");
            
            foreach (var kvp in aiMetrics.DecisionTypes)
            {
                float percent = (float)kvp.Value / aiMetrics.TotalDecisions * 100f;
                GD.Print($"  {kvp.Key}: {kvp.Value} ({percent:F1}%)");
            }
        }
    }
    
    public class AIPerformanceMetrics
    {
        public int TotalDecisions { get; set; } = 0;
        public int EffectiveDecisions { get; set; } = 0;
        public int PredictionAttempts { get; set; } = 0;
        public int AccuratePredictions { get; set; } = 0;
        public Dictionary<AIActionType, int> DecisionTypes { get; set; } = new Dictionary<AIActionType, int>();
    }
}