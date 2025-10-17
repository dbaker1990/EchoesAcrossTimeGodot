// Skits/SkitTrigger.cs
using Godot;
using System.Collections.Generic;
using System.Linq;
using EchoesAcrossTime.Events;
using EchoesAcrossTime.Managers;

namespace EchoesAcrossTime.Skits
{
    public partial class SkitTrigger : Node2D
    {
        [Export] public SkitData SkitToPlay { get; set; }
        [Export] public SkitTriggerMode TriggerMode { get; set; } = SkitTriggerMode.PlayerEnter;
        [Export] public bool OneTimeOnly { get; set; } = true;
        [Export] public bool CanReplay { get; set; } = false;
        
        private Area2D triggerArea;
        private bool hasTriggered = false;
        private bool playerInRange = false;
        
        public override void _Ready()
        {
            if (SkitToPlay == null)
            {
                GD.PrintErr("[SkitTrigger] No SkitData assigned!");
                return;
            }
            
            // Create trigger area based on mode
            if (TriggerMode == SkitTriggerMode.PlayerEnter || TriggerMode == SkitTriggerMode.Interaction)
            {
                SetupTriggerArea();
            }
        }
        
        private void SetupTriggerArea()
        {
            triggerArea = new Area2D
            {
                Name = "TriggerArea"
            };
            AddChild(triggerArea);
            
            var shape = new CollisionShape2D();
            var rectShape = new RectangleShape2D { Size = new Vector2(64, 64) };
            shape.Shape = rectShape;
            triggerArea.AddChild(shape);
            
            triggerArea.BodyEntered += OnBodyEntered;
            triggerArea.BodyExited += OnBodyExited;
        }
        
        private void OnBodyEntered(Node2D body)
        {
            if (body.IsInGroup("player"))
            {
                playerInRange = true;
                
                if (TriggerMode == SkitTriggerMode.PlayerEnter)
                {
                    TryTriggerSkit();
                }
            }
        }
        
        private void OnBodyExited(Node2D body)
        {
            if (body.IsInGroup("player"))
            {
                playerInRange = false;
            }
        }
        
        public override void _Input(InputEvent @event)
        {
            if (TriggerMode == SkitTriggerMode.Interaction && playerInRange)
            {
                if (@event.IsActionPressed("interact"))
                {
                    TryTriggerSkit();
                }
            }
        }
        
        public void TryTriggerSkit()
        {
            // Check if already triggered and one-time only
            if (hasTriggered && OneTimeOnly && !CanReplay)
            {
                return;
            }
            
            // Check conditions
            if (!CheckConditions())
            {
                return;
            }
            
            // Trigger the skit
            PlaySkit();
        }
        
        private bool CheckConditions()
        {
            if (SkitToPlay == null) return false;
            
            // Get current party
            var currentParty = GetCurrentParty();
            
            // Check if all participants are in party (if required)
            if (SkitToPlay.RequiresAllParticipantsInParty && SkitToPlay.ParticipantIds != null)
            {
                foreach (var participantId in SkitToPlay.ParticipantIds)
                {
                    if (!currentParty.Contains(participantId))
                    {
                        return false;
                    }
                }
            }
            
            // Check required flag
            if (!string.IsNullOrEmpty(SkitToPlay.RequiredFlag))
            {
                if (!GetGameFlag(SkitToPlay.RequiredFlag))
                {
                    return false;
                }
            }
            
            return true;
        }
        
        private async void PlaySkit()
        {
            if (SkitManager.Instance != null)
            {
                await SkitManager.Instance.PlaySkit(SkitToPlay);  // ← Now properly awaited
                hasTriggered = true;
            }
            else
            {
                GD.PrintErr("[SkitTrigger] SkitManager not found!");
            }
        }
        
        /// <summary>
        /// Get current game flags from EventCommandExecutor
        /// </summary>
        private List<string> GetGameFlags()
        {
            var flags = new List<string>();
            
            if (EventCommandExecutor.Instance != null)
            {
                // Get all boolean variables that are set to true
                // These are treated as flags
                // Note: We can't directly access the private variables dictionary,
                // so we check common flag names or rely on a getter method
                
                // For now, we'll add a helper method to get all active flags
                // You can expand this by implementing GetAllActiveFlags() in EventCommandExecutor
                
                // Example flags to check (you can expand this list):
                var commonFlags = new List<string>
                {
                    "met_aria", "chapter_1_complete", "defeated_boss",
                    "unlocked_fast_travel", "learned_about_time_magic"
                };
                
                foreach (var flagName in commonFlags)
                {
                    if (EventCommandExecutor.Instance.GetVariable<bool>(flagName, false))
                    {
                        flags.Add(flagName);
                    }
                }
            }
            
            return flags;
        }
        
        /// <summary>
        /// Get current party composition from PartyManager
        /// </summary>
        private List<string> GetCurrentParty()
        {
            var partyIds = new List<string>();
            
            if (PartyMenuManager.Instance != null)
            {
                var mainParty = PartyMenuManager.Instance.GetMainParty();
                foreach (var member in mainParty)
                {
                    if (member?.CharacterId != null)
                    {
                        partyIds.Add(member.CharacterId);
                    }
                }
            }
            
            return partyIds;
        }
        
        public override void _ExitTree()
        {
            if (triggerArea != null)
            {
                triggerArea.BodyEntered -= OnBodyEntered;
                triggerArea.BodyExited -= OnBodyExited;
            }
        }
        
        private bool GetGameFlag(string flagName)
        {
            if (Events.EventCommandExecutor.Instance != null)
            {
                return Events.EventCommandExecutor.Instance.GetVariable<bool>(flagName, false);
            }
            
            return false;
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