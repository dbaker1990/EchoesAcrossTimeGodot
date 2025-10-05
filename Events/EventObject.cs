// Events/EventObject.cs
using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EchoesAcrossTime.Events
{
    /// <summary>
    /// An interactive event object on the map
    /// </summary>
    public partial class EventObject : Node2D, IInteractable
    {
        [Export] public string EventId { get; set; } = "event_001";
        [Export] public Godot.Collections.Array<EventPage> Pages { get; set; }
        [Export] public AnimatedSprite2D Sprite { get; set; }
        [Export] public Area2D InteractionArea { get; set; }
        
        private Dictionary<string, bool> _selfSwitches = new Dictionary<string, bool>()
        {
            { "A", false }, { "B", false }, { "C", false }, { "D", false }
        };
        
        private bool isRunning = false;
        private bool hasRun = false;
        private int currentPageIndex = 0;
        
        public override void _Ready()
        {
            if (Pages == null)
                Pages = new Godot.Collections.Array<EventPage>();
            
            if (InteractionArea != null)
            {
                InteractionArea.AreaEntered += OnAreaEntered;
            }
        }
        
        public string GetInteractionType() => "event";
        
        public bool CanInteract(OverworldCharacter character)
        {
            var activePage = GetActivePage();
            if (activePage == null) return false;
            
            // Check if already run (if set to run once)
            if (activePage.RunOnce && hasRun) return false;
            
            // Check trigger type
            return activePage.Trigger == EventTrigger.ActionButton;
        }
        
        public void Interact(OverworldCharacter character)
        {
            if (isRunning) return;
            
            var activePage = GetActivePage();
            if (activePage == null) return;
            
            RunEvent(activePage);
        }
        
        private void OnAreaEntered(Area2D area)
        {
            if (area.GetParent() is PlayerCharacter)
            {
                var activePage = GetActivePage();
                if (activePage != null && activePage.Trigger == EventTrigger.PlayerTouch)
                {
                    RunEvent(activePage);
                }
            }
        }
        
        public void SetSelfSwitch(string key, bool value)
        {
            if (_selfSwitches.ContainsKey(key))
            {
                _selfSwitches[key] = value;
            }
        }
        
        public bool GetSelfSwitch(string key)
        {
            return _selfSwitches.GetValueOrDefault(key, false);
        }
        
        public override void _Process(double delta)
        {
            // Check for autorun and parallel events
            if (!isRunning)
            {
                var activePage = GetActivePage();
                if (activePage != null)
                {
                    if (activePage.Trigger == EventTrigger.Autorun && !hasRun)
                    {
                        RunEvent(activePage);
                    }
                    else if (activePage.Trigger == EventTrigger.Parallel)
                    {
                        RunEvent(activePage);
                    }
                }
            }
        }
        
        private EventPage GetActivePage()
        {
            // Check pages in reverse order (higher priority first)
            for (int i = Pages.Count - 1; i >= 0; i--)
            {
                var page = Pages[i];
                if (page != null && page.CheckConditions(this))
                {
                    currentPageIndex = i;
                    return page;
                }
            }
            
            return null;
        }
        
        private async void RunEvent(EventPage page)
        {
            if (page == null || isRunning) return;
            
            isRunning = true;
            
            try
            {
                await EventCommandExecutor.Instance.ExecuteEvent(page);
                
                if (page.RunOnce)
                    hasRun = true;
            }
            catch (Exception e)
            {
                GD.PrintErr($"Error running event {EventId}: {e.Message}");
            }
            finally
            {
                isRunning = false;
            }
        }
        
        /// <summary>
        /// Reset the event (useful for testing or special cases)
        /// </summary>
        public void ResetEvent()
        {
            hasRun = false;
            isRunning = false;
        }
    }
}