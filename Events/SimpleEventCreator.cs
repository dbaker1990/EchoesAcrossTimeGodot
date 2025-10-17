// Events/SimpleEventCreator.cs
// Create events with editable properties from the inspector

using Godot;
using EchoesAcrossTime.Events;

namespace EchoesAcrossTime.Events
{
    [Tool]
    public partial class SimpleEventCreator : Node
    {
        [ExportGroup("Target")]
        [Export] public NodePath TargetEventPath { get; set; }
        
        [ExportGroup("Dialogue Settings")]
        [Export] public string SpeakerName { get; set; } = "NPC";
        [Export(PropertyHint.MultilineText)] public string DialogueText { get; set; } = "Hello, traveler!";
        [Export] public string PortraitPath { get; set; } = "";
        [Export] public bool ShowPortrait { get; set; } = false;
        [Export] public MessageBoxPosition Position { get; set; } = MessageBoxPosition.Bottom;
        [Export] public float TextSpeed { get; set; } = 0.05f;
        [Export] public bool UseTypewriter { get; set; } = true;
        
        [ExportGroup("Event Settings")]
        [Export] public string EventId { get; set; } = "dialogue_event";
        [Export] public EventTrigger TriggerType { get; set; } = EventTrigger.ActionButton;
        [Export] public bool RunOnce { get; set; } = false;
        
        [ExportGroup("Actions")]
        [Export] public bool CreateEvent { get; set; } = false;
        [Export] public bool ClearAllPages { get; set; } = false;
        
        private bool lastCreateState = false;
        private bool lastClearState = false;
        
        public override void _Process(double delta)
        {
            if (!Engine.IsEditorHint()) return;
            
            // Create button
            if (CreateEvent && !lastCreateState)
            {
                CreateSimpleDialogue();
                CreateEvent = false;
            }
            lastCreateState = CreateEvent;
            
            // Clear button
            if (ClearAllPages && !lastClearState)
            {
                ClearPages();
                ClearAllPages = false;
            }
            lastClearState = ClearAllPages;
        }
        
        void CreateSimpleDialogue()
        {
            var targetEvent = GetNodeOrNull<EventObject>(TargetEventPath);
            
            if (targetEvent == null)
            {
                GD.PrintErr("❌ No EventObject found! Make sure Target Event Path is set.");
                return;
            }
            
            // Create dialogue data from inspector properties
            var dialogue = new DialogueData
            {
                SpeakerName = SpeakerName,
                Text = DialogueText,
                PortraitPath = PortraitPath,
                ShowSpeakerName = !string.IsNullOrEmpty(SpeakerName),
                ShowPortrait = ShowPortrait && !string.IsNullOrEmpty(PortraitPath),
                UseTypewriterEffect = UseTypewriter,
                TextSpeed = TextSpeed,
                Position = Position
            };
            
            // Create command
            var cmd = new ShowTextCommand
            {
                Dialogue = dialogue,
                Type = EventCommandType.ShowText,
                WaitForCompletion = true
            };
            
            // Create page
            var page = new EventPage
            {
                EventId = EventId,
                EventName = SpeakerName,
                Trigger = TriggerType,
                RunOnce = RunOnce,
                Commands = new Godot.Collections.Array<EventCommand>()
            };
            
            page.Commands.Add(cmd);
            
            // Add to event (don't clear, just append)
            targetEvent.Pages.Add(page);
            
            GD.Print($"✅ Created dialogue event: '{SpeakerName}: {DialogueText.Substring(0, System.Math.Min(30, DialogueText.Length))}...'");
            GD.Print($"📄 Event now has {targetEvent.Pages.Count} page(s)");
        }
        
        void ClearPages()
        {
            var targetEvent = GetNodeOrNull<EventObject>(TargetEventPath);
            
            if (targetEvent == null)
            {
                GD.PrintErr("❌ No EventObject found!");
                return;
            }
            
            targetEvent.Pages.Clear();
            GD.Print("🗑️ Cleared all pages from event");
        }
    }
}