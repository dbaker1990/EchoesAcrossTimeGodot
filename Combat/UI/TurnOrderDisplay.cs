using Godot;
using System.Collections.Generic;
using EchoesAcrossTime.Combat;

namespace EchoesAcrossTime.Combat.UI
{
    public partial class TurnOrderDisplay : Control
    {
        private BattleManager battleManager;
        private HBoxContainer turnContainer;
        private Label currentTurnLabel;
        
        public override void _Ready()
        {
            battleManager = GetNode<BattleManager>("/root/BattleScene/BattleManager");
    
            turnContainer = new HBoxContainer();
            turnContainer.Position = new Vector2(GetViewportRect().Size.X / 2 - 200, 20);turnContainer.Position = new Vector2(GetViewportRect().Size.X / 2 - 200, 20);
            turnContainer.AddThemeConstantOverride("separation", 10);
            AddChild(turnContainer);
    
            currentTurnLabel = new Label();
            currentTurnLabel.Position = new Vector2(GetViewportRect().Size.X / 2 - 150, 80);
            currentTurnLabel.AddThemeFontSizeOverride("font_size", 24);
            currentTurnLabel.Modulate = Colors.Yellow;
            AddChild(currentTurnLabel);
    
            if (battleManager != null)
            {
                battleManager.TurnStarted += OnTurnStarted;
                battleManager.BattleStarted += OnBattleStarted;
            }
        }
        
        private void OnTurnStarted(string characterName)
        {
            currentTurnLabel.Text = $"▼ {characterName}'S TURN ▼";
            UpdateTurnOrderDisplay();
            AnimateTurnLabel();
        }
        
        private void AnimateTurnLabel()
        {
            var tween = CreateTween();
    
            // Fade in from transparent
            currentTurnLabel.Modulate = new Color(1, 1, 0, 0);
            tween.TweenProperty(currentTurnLabel, "modulate", Colors.Yellow, 0.3f);
    
            // Scale bounce
            currentTurnLabel.Scale = new Vector2(0.7f, 0.7f);
            tween.Parallel().TweenProperty(currentTurnLabel, "scale", Vector2.One, 0.3f)
                .SetEase(Tween.EaseType.Out)
                .SetTrans(Tween.TransitionType.Back);
        }
        
        private void OnBattleStarted()
        {
            UpdateTurnOrderDisplay();
        }
        
        private void UpdateTurnOrderDisplay()
        {
            // Clear existing
            foreach (Node child in turnContainer.GetChildren())
            {
                child.QueueFree();
            }
    
            var turnOrder = battleManager.GetTurnOrder();
            if (turnOrder == null || turnOrder.Count == 0) return;
    
            foreach (var member in turnOrder)
            {
                var textureRect = new TextureRect();
                textureRect.CustomMinimumSize = new Vector2(60, 60);
                textureRect.ExpandMode = TextureRect.ExpandModeEnum.FitWidth;
                textureRect.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
        
                // Assuming CharacterStats has a Portrait property
                if (member.SourceData.BattlePortrait != null)
                {
                    textureRect.Texture = member.SourceData.BattlePortrait;
                }
        
                // Highlight current actor
                if (member == battleManager.CurrentActor)
                {
                    textureRect.Modulate = Colors.Yellow;
                }
                else
                {
                    textureRect.Modulate = Colors.White;
                }
        
                turnContainer.AddChild(textureRect);
            }
        }
    }
}