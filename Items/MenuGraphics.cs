using Godot;

namespace EchoesAcrossTime.Items
{
    /// <summary>
    /// Graphics and visual data for menu/UI display
    /// </summary>
    [GlobalClass]
    public partial class MenuGraphics : Resource
    {
        [ExportGroup("Character Graphics")]
        [Export] public Texture2D Portrait { get; set; }  // Face for dialogue
        [Export] public Texture2D MenuPortrait { get; set; }  // Larger portrait for menu
        [Export] public Texture2D BattleSprite { get; set; }  // Battle sprite sheet
        [Export] public Texture2D OverworldSprite { get; set; }  // Overworld sprite sheet
        
        [ExportGroup("Status Icons")]
        [Export] public Texture2D HPIcon { get; set; }
        [Export] public Texture2D MPIcon { get; set; }
        [Export] public Texture2D LevelIcon { get; set; }
        [Export] public Texture2D ExpIcon { get; set; }
        
        [ExportGroup("Battle UI")]
        [Export] public Texture2D AttackIcon { get; set; }
        [Export] public Texture2D DefenseIcon { get; set; }
        [Export] public Texture2D MagicIcon { get; set; }
        [Export] public Texture2D SpeedIcon { get; set; }
        
        [ExportGroup("Colors")]
        [Export] public Color HPBarColor { get; set; } = new Color(0.2f, 0.8f, 0.2f);  // Green
        [Export] public Color MPBarColor { get; set; } = new Color(0.2f, 0.5f, 1.0f);  // Blue
        [Export] public Color ExpBarColor { get; set; } = new Color(1.0f, 0.8f, 0.2f);  // Yellow
        [Export] public Color BackgroundColor { get; set; } = new Color(0.1f, 0.1f, 0.2f, 0.9f);
        
        [ExportGroup("Fonts")]
        [Export] public Font DisplayFont { get; set; }
        [Export] public int FontSize { get; set; } = 16;
        
        [ExportGroup("Animation")]
        [Export] public float PortraitAnimationSpeed { get; set; } = 1.0f;
        [Export] public bool UseBattleAnimations { get; set; } = true;
    }
}