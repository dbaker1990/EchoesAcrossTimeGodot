// Skits/SkitTrigger.cs
using Godot;
using System.Collections.Generic;

namespace EchoesAcrossTime.Skits
{
    /// <summary>
    /// Place this in your scene to automatically trigger skits based on conditions
    /// Can be attached to Area2D nodes, events, or used standalone
    /// </summary>
    public partial class SkitTrigger : Node2D
    {
        [ExportGroup("Skit Configuration")]
        [Export] public SkitData SkitToTrigger { get; set; }
        [Export] public string SkitId { get; set; } = ""; // Alternative to SkitData
        
        [ExportGroup("Trigger Settings")]
        [Export] public SkitTriggerMode TriggerMode { get; set; } = SkitTriggerMode.PlayerEnter;
        [Export] public bool AutoRemoveAfterTrigger { get; set; } = true;
        [Export] public bool RequireInteraction { get; set; } = false;
        
        [ExportGroup("Conditions")]
        [Export] public bool CheckPartyComposition { get; set; } = true;
        [Export] public bool CheckFlags { get; set; } = true;
        [Export] public Godot.Collections.Array<string> RequiredFlags { get; set; }
        [Export] public Godot.Collections.Array<string> ForbiddenFlags { get; set; }
        
        // Internal
        private Area2D triggerArea;
        private bool hasTriggered = false;
        private bool playerInRange = false;

        public override void _Ready()
        {
            RequiredFlags = RequiredFlags ?? new Godot.Collections.Array<string>();
            ForbiddenFlags = ForbiddenFlags ?? new Godot.Collections.Array<string>();
            
            // Auto-create trigger area if not exists
            triggerArea = GetNodeOrNull<Area2D>("TriggerArea");
            if (triggerArea == null && TriggerMode == SkitTriggerMode.PlayerEnter)
            {
                CreateDefaultTriggerArea();
            }
            
            // Connect signals
            if (triggerArea != null)
            {
                triggerArea.BodyEntered += OnBodyEntered;
                triggerArea.BodyExited += OnBodyExited;
            }
        }

        public override void _Input(InputEvent @event)
        {
            if (RequireInteraction && playerInRange && !hasTriggered)
            {
                if (@event.IsActionPressed("interact"))
                {
                    TryTriggerSkit();
                }
            }
        }

        private void OnBodyEntered(Node2D body)
        {
            if (body.Name == "Player" || body is PlayerCharacter)
            {
                playerInRange = true;
                
                if (!RequireInteraction && !hasTriggered)
                {
                    TryTriggerSkit();
                }
            }
        }

        private void OnBodyExited(Node2D body)
        {
            if (body.Name == "Player" || body is PlayerCharacter)
            {
                playerInRange = false;
            }
        }

        /// <summary>
        /// Attempt to trigger the skit
        /// </summary>
        public async void TryTriggerSkit()
        {
            if (hasTriggered)
                return;

            if (SkitManager.Instance == null)
            {
                GD.PrintErr("SkitManager not found!");
                return;
            }

            // Get skit data
            SkitData skitData = SkitToTrigger;
            if (skitData == null && !string.IsNullOrEmpty(SkitId))
            {
                skitData = SkitManager.Instance.GetSkit(SkitId);
            }

            if (skitData == null)
            {
                GD.PrintErr("No skit data specified!");
                return;
            }

            // Check conditions
            if (!CheckConditions(skitData))
            {
                return;
            }

            // Mark as triggered
            hasTriggered = true;

            // Play skit
            await SkitManager.Instance.PlaySkit(skitData);

            // Auto-remove if set
            if (AutoRemoveAfterTrigger)
            {
                QueueFree();
            }
        }

        /// <summary>
        /// Check if all conditions are met
        /// </summary>
        private bool CheckConditions(SkitData skit)
        {
            // Check if already viewed
            if (skit.OnceOnly && SkitManager.Instance.HasViewedSkit(skit.SkitId))
            {
                return false;
            }

            // Check required flags
            if (CheckFlags && RequiredFlags.Count > 0)
            {
                var gameFlags = GetGameFlags();
                foreach (var flag in RequiredFlags)
                {
                    if (!gameFlags.Contains(flag))
                    {
                        GD.Print($"Skit blocked: Missing required flag '{flag}'");
                        return false;
                    }
                }
            }

            // Check forbidden flags
            if (CheckFlags && ForbiddenFlags.Count > 0)
            {
                var gameFlags = GetGameFlags();
                foreach (var flag in ForbiddenFlags)
                {
                    if (gameFlags.Contains(flag))
                    {
                        GD.Print($"Skit blocked: Forbidden flag '{flag}' is set");
                        return false;
                    }
                }
            }

            // Check party composition
            if (CheckPartyComposition && skit.RequiresAllParticipantsInParty)
            {
                var currentParty = GetCurrentParty();
                foreach (var participantId in skit.ParticipantIds)
                {
                    if (!currentParty.Contains(participantId))
                    {
                        GD.Print($"Skit blocked: '{participantId}' not in party");
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Create default trigger area
        /// </summary>
        private void CreateDefaultTriggerArea()
        {
            triggerArea = new Area2D { Name = "TriggerArea" };
            AddChild(triggerArea);
            
            var shape = new CollisionShape2D();
            var rectShape = new RectangleShape2D { Size = new Vector2(64, 64) };
            shape.Shape = rectShape;
            triggerArea.AddChild(shape);
        }

        /// <summary>
        /// Get current game flags (integrate with your save system)
        /// </summary>
        private List<string> GetGameFlags()
        {
            // TODO: Integrate with your game's flag/variable system
            // For now, return empty list
            return new List<string>();
        }

        /// <summary>
        /// Get current party composition (integrate with your party system)
        /// </summary>
        private List<string> GetCurrentParty()
        {
            // TODO: Integrate with your game's party system
            // For now, return example party
            return new List<string> { "dominic", "aria", "echo" };
        }
    }

    public enum SkitTriggerMode
    {
        PlayerEnter,      // Trigger when player enters area
        Interaction,      // Trigger on player interaction
        Script,           // Trigger via code
        Event            // Trigger from event system
    }
}