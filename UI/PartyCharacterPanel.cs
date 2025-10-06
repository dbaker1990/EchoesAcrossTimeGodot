using Godot;
using EchoesAcrossTime.Combat;

namespace EchoesAcrossTime.UI
{
    public partial class PartyCharacterPanel : PanelContainer
    {
        [Export] private TextureRect portraitRect;
        [Export] private Label nameLabel;
        [Export] private Label levelLabel;
        [Export] private ProgressBar hpProgressBar;
        [Export] private Label hpValueLabel;
        [Export] private ProgressBar tpProgressBar;
        [Export] private Label tpValueLabel;
        [Export] private ProgressBar expProgressBar;
        [Export] private Label expValueLabel;
        
        private string characterId;
        
        public override void _Ready()
        {
            SetupProgressBars();
        }
        
        private void SetupProgressBars()
        {
            // HP Bar (Green)
            if (hpProgressBar != null)
            {
                var hpStyle = new StyleBoxFlat();
                hpStyle.BgColor = new Color(0.2f, 0.7f, 0.2f); // Green
                hpProgressBar.AddThemeStyleboxOverride("fill", hpStyle);
                
                var hpBgStyle = new StyleBoxFlat();
                hpBgStyle.BgColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
                hpProgressBar.AddThemeStyleboxOverride("background", hpBgStyle);
            }
            
            // TP Bar (Blue)
            if (tpProgressBar != null)
            {
                var tpStyle = new StyleBoxFlat();
                tpStyle.BgColor = new Color(0.2f, 0.4f, 0.9f); // Blue
                tpProgressBar.AddThemeStyleboxOverride("fill", tpStyle);
                
                var tpBgStyle = new StyleBoxFlat();
                tpBgStyle.BgColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
                tpProgressBar.AddThemeStyleboxOverride("background", tpBgStyle);
            }
            
            // EXP Bar (Yellow)
            if (expProgressBar != null)
            {
                var expStyle = new StyleBoxFlat();
                expStyle.BgColor = new Color(0.9f, 0.8f, 0.2f); // Yellow
                expProgressBar.AddThemeStyleboxOverride("fill", expStyle);
                
                var expBgStyle = new StyleBoxFlat();
                expBgStyle.BgColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
                expProgressBar.AddThemeStyleboxOverride("background", expBgStyle);
            }
        }
        
        public void Initialize(string charId, CharacterStats stats)
        {
            characterId = charId;
            UpdateDisplay(stats);
        }
        
        public void UpdateDisplay(CharacterStats stats)
        {
            if (stats == null) return;
            
            // Update name and level
            if (nameLabel != null)
                nameLabel.Text = stats.CharacterName;
            
            if (levelLabel != null)
                levelLabel.Text = $"Lv.  {stats.Level}";
            
            // Update HP
            if (hpProgressBar != null)
            {
                hpProgressBar.MaxValue = stats.MaxHP;
                hpProgressBar.Value = stats.CurrentHP;
            }
            if (hpValueLabel != null)
                hpValueLabel.Text = $"{stats.CurrentHP}/{stats.MaxHP}";
            
            // Update TP (MP)
            if (tpProgressBar != null)
            {
                tpProgressBar.MaxValue = stats.MaxMP;
                tpProgressBar.Value = stats.CurrentMP;
            }
            if (tpValueLabel != null)
                tpValueLabel.Text = $"{stats.CurrentMP}/{stats.MaxMP}";
            
            // Update EXP
            if (expProgressBar != null && expValueLabel != null)
            {
                if (stats.Level >= 99)
                {
                    expProgressBar.MaxValue = 100;
                    expProgressBar.Value = 100;
                    expValueLabel.Text = "MAX";
                }
                else
                {
                    // CurrentExp = progress toward next level
                    // ExpToNextLevel = total EXP needed for next level
                    int expRemaining = stats.ExpToNextLevel - stats.CurrentExp;
        
                    expProgressBar.MaxValue = stats.ExpToNextLevel;
                    expProgressBar.Value = stats.CurrentExp;
                    expValueLabel.Text = expRemaining.ToString();
                }
            }
            
            // Load portrait
            LoadPortrait(characterId);
        }
        
        private void LoadPortrait(string charId)
        {
            if (portraitRect == null) return;
            
            // Try menu portrait first
            string portraitPath = $"res://Characters/Portraits/{charId}_Menu.png";
            if (!ResourceLoader.Exists(portraitPath))
            {
                // Fallback to normal portrait
                portraitPath = $"res://Characters/Portraits/{charId}_Normal.png";
            }
            
            if (ResourceLoader.Exists(portraitPath))
            {
                portraitRect.Texture = GD.Load<Texture2D>(portraitPath);
            }
            else
            {
                GD.PrintErr($"Portrait not found for character: {charId}");
            }
        }
    }
}