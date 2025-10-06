using Godot;
using EchoesAcrossTime.Combat;

namespace EchoesAcrossTime.Combat.UI
{
    public partial class ShowtimePrompt : Control
    {
        private BattleManager battleManager;
        
        public override void _Ready()
        {
            battleManager = GetNode<BattleManager>("/root/BattleScene/BattleManager");
            if (battleManager != null)
            {
                battleManager.ShowtimeTriggered += OnShowtimeTriggered;
            }
        }
        
        private void OnShowtimeTriggered(string showtimeName, string char1, string char2)
        {
            ShowShowtimeAnimation(showtimeName, char1, char2);
        }
        
        private void ShowShowtimeAnimation(string showtimeName, string char1, string char2)
        {
            var cutscene = new ColorRect();
            cutscene.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            cutscene.Color = Colors.Black;
            AddChild(cutscene);
            
            var centerContainer = new CenterContainer();
            centerContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            cutscene.AddChild(centerContainer);
            
            var vbox = new VBoxContainer();
            centerContainer.AddChild(vbox);
            
            var showtimeLabel = new Label();
            showtimeLabel.Text = "✨ SHOWTIME! ✨";
            showtimeLabel.AddThemeFontSizeOverride("font_size", 64);
            showtimeLabel.Modulate = Colors.Gold;
            vbox.AddChild(showtimeLabel);
            
            var attackNameLabel = new Label();
            attackNameLabel.Text = showtimeName;
            attackNameLabel.AddThemeFontSizeOverride("font_size", 48);
            vbox.AddChild(attackNameLabel);
            
            var charactersLabel = new Label();
            charactersLabel.Text = $"{char1} & {char2}";
            charactersLabel.AddThemeFontSizeOverride("font_size", 32);
            vbox.AddChild(charactersLabel);
            
            var tween = CreateTween();
            cutscene.Modulate = new Color(1, 1, 1, 0);
            tween.TweenProperty(cutscene, "modulate:a", 1.0f, 0.5f);
            tween.TweenInterval(2.0f);
            tween.TweenProperty(cutscene, "modulate:a", 0.0f, 0.5f);
            tween.TweenCallback(Callable.From(() => cutscene.QueueFree()));
        }
    }
}