// Events/EventPage.cs
using Godot;
using System;

namespace EchoesAcrossTime.Events
{
    /// <summary>
    /// Container for a sequence of event commands
    /// Similar to RPG Maker's event pages
    /// </summary>
    [GlobalClass]
    public partial class EventPage : Resource
    {
        [Export] public string EventId { get; set; } = "event_001";
        [Export] public string EventName { get; set; } = "Event";

        [ExportGroup("Conditions")] [Export] public bool HasConditions { get; set; } = false;
        [Export] public string VariableName { get; set; } = "";
        [Export] public int RequiredValue { get; set; } = 0;
        [Export] public bool RequireSwitch { get; set; } = false;
        [Export] public string SwitchName { get; set; } = "";

        [ExportGroup("Commands")] [Export] public Godot.Collections.Array<EventCommand> Commands { get; set; }

        [ExportGroup("Trigger")] [Export] public EventTrigger Trigger { get; set; } = EventTrigger.ActionButton;
        [Export] public bool RunOnce { get; set; } = false;
        [Export] public bool Parallel { get; set; } = false;
        [ExportGroup("Conditions")] [Export] public bool RequireSelfSwitch { get; set; } = false;
        [Export(PropertyHint.Enum, "A,B,C,D")] public string SelfSwitchName { get; set; } = "A";


        public EventPage()
        {
            Commands = new Godot.Collections.Array<EventCommand>();
        }

        // In EventPage.cs

        /// <summary>
        /// Check if conditions are met to run this event
        /// </summary>
        public bool CheckConditions(EventObject eventObject) // MODIFIED: Added EventObject parameter
        {
            if (!HasConditions) return true;

            // Add this new block to check the event's self switch
            if (RequireSelfSwitch)
            {
                // Now it can check the eventObject that was passed in
                if (eventObject == null || !eventObject.GetSelfSwitch(SelfSwitchName))
                {
                    return false;
                }
            }

            // Check variable condition
            if (!string.IsNullOrEmpty(VariableName))
            {
                var value = EventCommandExecutor.Instance?.GetVariable<int>(VariableName, 0) ?? 0;
                if (value != RequiredValue)
                    return false;
            }

            // Check switch condition
            if (RequireSwitch && !string.IsNullOrEmpty(SwitchName))
            {
                var switchValue = EventCommandExecutor.Instance?.GetVariable<bool>(SwitchName, false) ?? false;
                if (!switchValue)
                    return false;
            }

            return true;
        }
    }

    public enum EventTrigger
    {
        ActionButton,   // Triggered when player presses action button
        PlayerTouch,    // Triggered when player touches event
        EventTouch,     // Triggered when event touches player
        Autorun,        // Runs automatically when conditions met
        Parallel        // Runs in parallel with other events
    }
}