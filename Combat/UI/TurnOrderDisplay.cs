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
            turnContainer.Position = new Vector2(400, 20);
            turnContainer.AddThemeConstantOverride("separation", 10);
            AddChild(turnContainer);
            
            currentTurnLabel = new Label();
            currentTurnLabel.Position = new Vector2(400, 80);
            currentTurnLabel.AddThemeFontSizeOverride("font_size", 24);
            currentTurnLabel.Modulate = Colors.Yellow;
            AddChild(currentTurnLabel);
            
            if (battleManager != null)
            {
                battleManager.TurnStarted += OnTurnStarted;
            }
        }
        
        private void OnTurnStarted(string characterName)
        {
            currentTurnLabel.Text = $"▼ {characterName}'S TURN ▼";
        }
    }
}