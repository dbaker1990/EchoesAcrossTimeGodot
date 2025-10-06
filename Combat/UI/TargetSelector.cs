using Godot;
using System.Collections.Generic;
using EchoesAcrossTime.Combat;

namespace EchoesAcrossTime.Combat.UI
{
    public partial class TargetSelector : Control
    {
        private BattleMember selectedTarget;
        private bool selectionMade;
        
        public BattleMember GetSelectedTarget()
        {
            return selectedTarget;
        }
        
        public bool WasSelectionMade()
        {
            return selectionMade;
        }
        
        public void ClearSelection()
        {
            selectedTarget = null;
            selectionMade = false;
        }
        
        private List<BattleMember> availableTargets;
        private int selectedIndex = 0;
        private List<Panel> targetIndicators = new List<Panel>();
        private Label instructionLabel;
        
        public override void _Ready()
        {
            instructionLabel = new Label();
            instructionLabel.Position = new Vector2(400, 50);
            instructionLabel.AddThemeFontSizeOverride("font_size", 24);
            instructionLabel.Modulate = Colors.Yellow;
            AddChild(instructionLabel);
            
            Hide();
        }
        
        public void ShowSelection(List<BattleMember> targets)
        {
            availableTargets = targets;
            selectedIndex = 0;
            selectionMade = false;
            selectedTarget = null;
            
            CreateTargetIndicators();
            UpdateSelection();
            
            instructionLabel.Text = "SELECT TARGET (Arrow Keys + Enter)";
            Show();
        }
        
        private void CreateTargetIndicators()
        {
            foreach (var indicator in targetIndicators)
            {
                indicator.QueueFree();
            }
            targetIndicators.Clear();
            
            int index = 0;
            foreach (var target in availableTargets)
            {
                var indicator = new Panel();
                indicator.CustomMinimumSize = new Vector2(180, 80);
                
                Vector2 position;
                if (target.IsPlayerControlled)
                {
                    position = new Vector2(50, 150 + (index * 120));
                }
                else
                {
                    position = new Vector2(600 + (index * 200), 180);
                }
                indicator.Position = position;
                
                AddChild(indicator);
                targetIndicators.Add(indicator);
                index++;
            }
        }
        
        private void UpdateSelection()
        {
            for (int i = 0; i < targetIndicators.Count; i++)
            {
                targetIndicators[i].Modulate = i == selectedIndex ? 
                    Colors.Yellow : new Color(1, 1, 1, 0.3f);
            }
        }
        
        public override void _Input(InputEvent @event)
        {
            if (!Visible) return;
            
            if (@event.IsActionPressed("ui_right"))
            {
                selectedIndex = (selectedIndex + 1) % availableTargets.Count;
                UpdateSelection();
                GetViewport().SetInputAsHandled();
            }
            else if (@event.IsActionPressed("ui_left"))
            {
                selectedIndex--;
                if (selectedIndex < 0) selectedIndex = availableTargets.Count - 1;
                UpdateSelection();
                GetViewport().SetInputAsHandled();
            }
            else if (@event.IsActionPressed("ui_accept"))
            {
                selectedTarget = availableTargets[selectedIndex];
                selectionMade = true;
                Hide();
                GetViewport().SetInputAsHandled();
            }
            else if (@event.IsActionPressed("ui_cancel"))
            {
                selectedTarget = null;
                selectionMade = false;
                Hide();
                GetViewport().SetInputAsHandled();
            }
        }
    }
}