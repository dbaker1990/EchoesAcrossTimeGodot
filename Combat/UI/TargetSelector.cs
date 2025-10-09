using Godot;
using System.Collections.Generic;
using EchoesAcrossTime.Combat;

namespace EchoesAcrossTime.Combat.UI
{
    public partial class TargetSelector : Control
    {
        private BattleMember selectedTarget;
        private bool selectionMade;
        private List<BattleMember> availableTargets;
        private int selectedIndex = 0;
        private List<Panel> targetIndicators = new List<Panel>();
        private Label instructionLabel;
        private Sprite2D cursorArrow;
        private SkillData currentSkill; // NEW!
        
        public BattleMember GetSelectedTarget() => selectedTarget;
        public bool WasSelectionMade() => selectionMade;
        
        public void ClearSelection()
        {
            selectedTarget = null;
            selectionMade = false;
        }
        
        public override void _Ready()
        {
            instructionLabel = new Label();
            instructionLabel.Position = new Vector2(400, 50);
            instructionLabel.AddThemeFontSizeOverride("font_size", 24);
            instructionLabel.Modulate = Colors.Yellow;
            AddChild(instructionLabel);
            
            cursorArrow = new Sprite2D();
            cursorArrow.Modulate = Colors.Yellow;
            AddChild(cursorArrow);
            
            Hide();
        }
        
        // MODIFIED: Added skill parameter
        public void ShowSelection(List<BattleMember> targets, SkillData skill = null)
        {
            availableTargets = targets;
            currentSkill = skill; // NEW!
            selectedIndex = 0;
            selectionMade = false;
            selectedTarget = null;
            
            CreateTargetIndicators();
            UpdateSelection();
            
            instructionLabel.Text = "SELECT TARGET (Arrow Keys + Enter)";
            Show();
        }
        
        // MODIFIED: Added affinity checking
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
                var container = new VBoxContainer();
                container.AddThemeConstantOverride("separation", 5);
                
                if (target.IsPlayerControlled)
                {
                    container.Position = new Vector2(50, 150 + (index * 120));
                }
                else
                {
                    container.Position = new Vector2(600 + (index * 200), 180);
                }
                
                // Name and HP with affinity check
                var infoLabel = new Label();
                string infoText = $"{target.Stats.CharacterName}\nHP: {target.Stats.CurrentHP}/{target.Stats.MaxHP}";
                
                // NEW: Check elemental affinity
                if (currentSkill != null && currentSkill.Element != ElementType.None && target.SourceData.ElementAffinities != null)
                {
                    var affinity = target.SourceData.ElementAffinities.GetAffinity(currentSkill.Element);
                    
                    switch (affinity)
                    {
                        case ElementAffinity.Weak:
                            infoText += "\n⚡ WEAK!";
                            infoLabel.Modulate = Colors.Yellow;
                            break;
                            
                        case ElementAffinity.Resist:
                            infoText += "\n🛡 RESIST";
                            infoLabel.Modulate = Colors.Gray;
                            break;
                            
                        case ElementAffinity.Immune:
                            infoText += "\n🚫 IMMUNE";
                            infoLabel.Modulate = Colors.DarkGray;
                            break;
                            
                        case ElementAffinity.Absorb:
                            infoText += "\n💚 ABSORB";
                            infoLabel.Modulate = Colors.LightGreen;
                            break;
                            
                        default:
                            infoLabel.Modulate = Colors.White;
                            break;
                    }
                }
                else
                {
                    infoLabel.Modulate = Colors.White;
                }
                
                infoLabel.Text = infoText;
                infoLabel.HorizontalAlignment = HorizontalAlignment.Center;
                infoLabel.AddThemeFontSizeOverride("font_size", 12);
                container.AddChild(infoLabel);
                
                // Indicator panel
                var indicator = new Panel();
                indicator.CustomMinimumSize = new Vector2(180, 80);
                container.AddChild(indicator);
                
                AddChild(container);
                targetIndicators.Add(indicator);
                index++;
            }
        }
        
        private void UpdateSelection()
        {
            for (int i = 0; i < targetIndicators.Count; i++)
            {
                if (i == selectedIndex)
                {
                    targetIndicators[i].Modulate = Colors.Yellow;
                    AnimateIndicator(targetIndicators[i]);
                }
                else
                {
                    targetIndicators[i].Modulate = new Color(1, 1, 1, 0.3f);
                }
            }
            
            if (selectedIndex >= 0 && selectedIndex < targetIndicators.Count)
            {
                var targetPos = targetIndicators[selectedIndex].GlobalPosition;
                cursorArrow.Position = targetPos + new Vector2(0, -40);
            }
        }
        
        private void AnimateIndicator(Panel indicator)
        {
            var tween = CreateTween();
            tween.TweenProperty(indicator, "scale", new Vector2(1.1f, 1.1f), 0.3f)
                .SetEase(Tween.EaseType.InOut);
            tween.TweenProperty(indicator, "scale", Vector2.One, 0.3f)
                .SetEase(Tween.EaseType.InOut);
            tween.SetLoops();
        }
        
        public override void _Input(InputEvent @event)
        {
            if (!Visible) return;
            
            if (@event.IsActionPressed("ui_right"))
            {
                selectedIndex = (selectedIndex + 1) % availableTargets.Count;
                UpdateSelection();
                Managers.SystemManager.Instance?.PlayCursorSE();
                GetViewport().SetInputAsHandled();
            }
            else if (@event.IsActionPressed("ui_left"))
            {
                selectedIndex--;
                if (selectedIndex < 0) selectedIndex = availableTargets.Count - 1;
                UpdateSelection();
                Managers.SystemManager.Instance?.PlayCursorSE();
                GetViewport().SetInputAsHandled();
            }
            else if (@event.IsActionPressed("ui_accept"))
            {
                selectedTarget = availableTargets[selectedIndex];
                selectionMade = true;
                Managers.SystemManager.Instance?.PlayOkSE();
                Hide();
                GetViewport().SetInputAsHandled();
            }
            else if (@event.IsActionPressed("ui_cancel"))
            {
                selectedTarget = null;
                selectionMade = false;
                Managers.SystemManager.Instance?.PlayCancelSE();
                Hide();
                GetViewport().SetInputAsHandled();
            }
        }
    }
}