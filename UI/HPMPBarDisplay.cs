using EchoesAcrossTime.Combat;
namespace EchoesAcrossTime.UI;
using Godot;

public partial class HPMPBarDisplay : Control
{
    [Export] private ProgressBar hpProgressBar;
    [Export] private Label hpValueLabel;
    [Export] private ProgressBar mpProgressBar;
    [Export] private Label mpValueLabel;
    
    private CharacterStats characterStats;
    
    public override void _Ready()
    {
        // Nodes are already connected via export in scene
    }
    
    /// 
    /// Initialize with a character's stats
    /// 
    public void Initialize(CharacterStats stats)
    {
        if (characterStats != null)
        {
            // Disconnect old signals
            characterStats.HPChanged -= OnHPChanged;
            characterStats.MPChanged -= OnMPChanged;
        }
        
        characterStats = stats;
        
        // Connect to signals
        characterStats.HPChanged += OnHPChanged;
        characterStats.MPChanged += OnMPChanged;
        
        // Set initial values
        UpdateHPBar(stats.CurrentHP, stats.MaxHP, false);
        UpdateMPBar(stats.CurrentMP, stats.MaxMP, false);
    }
    
    private void OnHPChanged(int oldHP, int newHP, int maxHP)
    {
        UpdateHPBar(newHP, maxHP, true);
    }
    
    private void OnMPChanged(int oldMP, int newMP, int maxMP)
    {
        UpdateMPBar(newMP, maxMP, true);
    }
    
    private void UpdateHPBar(int currentHP, int maxHP, bool animate)
    {
        hpProgressBar.MaxValue = maxHP;
        hpValueLabel.Text = $"{currentHP}/{maxHP}";
        
        if (animate)
        {
            // Smooth animation
            var tween = CreateTween();
            tween.TweenProperty(hpProgressBar, "value", currentHP, 0.3);
        }
        else
        {
            hpProgressBar.Value = currentHP;
        }
    }
    
    private void UpdateMPBar(int currentMP, int maxMP, bool animate)
    {
        mpProgressBar.MaxValue = maxMP;
        mpValueLabel.Text = $"{currentMP}/{maxMP}";
        
        if (animate)
        {
            var tween = CreateTween();
            tween.TweenProperty(mpProgressBar, "value", currentMP, 0.3);
        }
        else
        {
            mpProgressBar.Value = currentMP;
        }
    }
}