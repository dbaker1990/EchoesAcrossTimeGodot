using Godot;
using EchoesAcrossTime.Combat;

namespace EchoesAcrossTime.Combat.UI
{
    public partial class BatonPassIndicator : Control
    {
        private Label batonLabel;
        private Label multiplierLabel;
        private Panel indicatorPanel;
        private BattleManager battleManager;
        
        public override void _Ready()
        {
            BuildIndicator();
            
            battleManager = GetNode<BattleManager>("/root/BattleScene/BattleManager");
            if (battleManager != null)
            {
                battleManager.BatonPassExecuted += OnBatonPassExecuted;
                battleManager.TurnStarted += OnTurnStarted;
            }
            
            Hide();
        }
        
        private void BuildIndicator()
        {
            indicatorPanel = new Panel();
            indicatorPanel.Position = new Vector2(1000, 100);
            indicatorPanel.CustomMinimumSize = new Vector2(250, 120);
            AddChild(indicatorPanel);
            
            var vbox = new VBoxContainer();
            vbox.Position = new Vector2(15, 15);
            indicatorPanel.AddChild(vbox);
            
            batonLabel = new Label();
            batonLabel.Text = "🎯 BATON PASS!";
            batonLabel.AddThemeFontSizeOverride("font_size", 20);
            batonLabel.Modulate = Colors.Orange;
            vbox.AddChild(batonLabel);
            
            multiplierLabel = new Label();
            multiplierLabel.Text = "Damage: x1.5";
            multiplierLabel.AddThemeFontSizeOverride("font_size", 24);
            multiplierLabel.Modulate = Colors.Yellow;
            vbox.AddChild(multiplierLabel);
        }
        
        public void ShowBatonPass(int passLevel, float multiplier)
        {
            multiplierLabel.Text = $"LEVEL {passLevel}\nDamage: x{multiplier:F1}";
            multiplierLabel.Modulate = passLevel switch
            {
                1 => Colors.Yellow,
                2 => Colors.Orange,
                3 => Colors.Red,
                _ => Colors.White
            };
            
            Show();
        }
        
        private void OnBatonPassExecuted(string fromCharacter, string toCharacter, int passLevel)
        {
            float multiplier = 1.0f + (passLevel * 0.5f);
            ShowBatonPass(passLevel, multiplier);
        }
        
        private void OnTurnStarted(string characterName)
        {
            // Hide when turn changes
            Hide();
        }
    }
}