using Godot;
using EchoesAcrossTime.Combat;

namespace EchoesAcrossTime.Combat.UI
{
    public partial class TechnicalIndicator : Control
    {
        private BattleManager battleManager;
        
        public override void _Ready()
        {
            battleManager = GetNode<BattleManager>("/root/BattleScene/BattleManager");
            if (battleManager != null)
            {
                battleManager.TechnicalDamage += OnTechnicalDamage;
            }
        }
        
        private void OnTechnicalDamage(string attackerName, string targetName, string comboType)
        {
            ShowTechnicalPopup();
        }
        
        private void ShowTechnicalPopup()
        {
            var label = new Label();
            label.Text = "⚡ TECHNICAL! ⚡";
            label.AddThemeFontSizeOverride("font_size", 48);
            label.Modulate = Colors.Cyan;
            label.Position = new Vector2(500, 300);
            label.PivotOffset = label.Size / 2;
            
            AddChild(label);
            
            var tween = CreateTween();
            tween.SetParallel(true);
            tween.TweenProperty(label, "scale", Vector2.One * 1.5f, 0.2f);
            tween.TweenProperty(label, "scale", Vector2.One, 0.2f).SetDelay(0.2f);
            
            for (int i = 0; i < 5; i++)
            {
                tween.TweenProperty(label, "rotation", 0.1f, 0.05f).SetDelay(i * 0.1f);
                tween.TweenProperty(label, "rotation", -0.1f, 0.05f).SetDelay(i * 0.1f + 0.05f);
            }
            tween.TweenProperty(label, "rotation", 0, 0.05f).SetDelay(0.5f);
            
            tween.Chain().TweenProperty(label, "modulate:a", 0.0f, 0.5f).SetDelay(0.5f);
            tween.TweenCallback(Callable.From(() => label.QueueFree()));
        }
    }
}