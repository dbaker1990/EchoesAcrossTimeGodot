using Godot;
using EchoesAcrossTime.Combat;

namespace EchoesAcrossTime.Combat.UI
{
    public partial class AllOutAttackPrompt : Control
    {
        public bool UserConfirmed { get; private set; }
        public bool UserDeclined { get; private set; }
        
        private BattleManager battleManager;
        private ColorRect background;
        private Button yesButton;
        private Button noButton;
        
        public override void _Ready()
        {
            BuildPrompt();
            
            battleManager = GetNode<BattleManager>("/root/BattleScene/BattleManager");
            if (battleManager != null)
            {
                battleManager.AllOutAttackReady += OnAllOutAttackReady;
            }
            
            Hide();
        }
        
        private void BuildPrompt()
        {
            background = new ColorRect();
            background.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            background.Color = new Color(0, 0, 0, 0.7f);
            AddChild(background);
            
            var centerContainer = new CenterContainer();
            centerContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            AddChild(centerContainer);
            
            var panel = new PanelContainer();
            panel.CustomMinimumSize = new Vector2(500, 300);
            centerContainer.AddChild(panel);
            
            var vbox = new VBoxContainer();
            vbox.Alignment = BoxContainer.AlignmentMode.Center;
            panel.AddChild(vbox);
            
            var titleLabel = new Label();
            titleLabel.Text = "⚔️ CONVERGENCE STRIKE? ⚔️";
            titleLabel.AddThemeFontSizeOverride("font_size", 42);
            titleLabel.Modulate = Colors.Red;
            titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
            vbox.AddChild(titleLabel);
            
            var description = new Label();
            description.Text = "All enemies are disrupted!\nFinish them!";
            description.AddThemeFontSizeOverride("font_size", 18);
            description.HorizontalAlignment = HorizontalAlignment.Center;
            vbox.AddChild(description);
            
            var buttonBox = new HBoxContainer();
            buttonBox.Alignment = BoxContainer.AlignmentMode.Center;
            vbox.AddChild(buttonBox);
            
            yesButton = new Button();
            yesButton.Text = "YES!";
            yesButton.CustomMinimumSize = new Vector2(150, 60);
            yesButton.Pressed += OnYesPressed;
            buttonBox.AddChild(yesButton);
            
            noButton = new Button();
            noButton.Text = "No";
            noButton.CustomMinimumSize = new Vector2(150, 60);
            noButton.Pressed += OnNoPressed;
            buttonBox.AddChild(noButton);
        }
        
        private void OnAllOutAttackReady()
        {
            UserConfirmed = false;
            UserDeclined = false;
            Show();
            yesButton.GrabFocus();
        }
        
        private void OnYesPressed()
        {
            UserConfirmed = true;
            UserDeclined = false;
            Hide();
        }
        
        private void OnNoPressed()
        {
            UserConfirmed = false;
            UserDeclined = true;
            Hide();
        }
    }
}