// UI/GameOverUI.cs

using EchoesAcrossTime;
using Godot;

public partial class GameOverUI : Control
{
    [Export] private Button _continueButton;
    [Export] private Button _titleButton;

    public override void _Ready()
    {
        Visible = false;

        // Safer way to connect signals
        if (_continueButton != null)
        {
            _continueButton.Pressed += OnContinuePressed;
        }
        else
        {
            GD.PrintErr("GameOverUI: Continue Button is not assigned in the Inspector!");
        }

        if (_titleButton != null)
        {
            _titleButton.Pressed += OnReturnToTitlePressed;
        }
        else
        {
            GD.PrintErr("GameOverUI: Title Button is not assigned in the Inspector!");
        }
    }

    public void ShowScreen()
    {
        Visible = true;
    }

    private void OnContinuePressed()
    {
        GameManager.Instance.ContinueFromLastSave();
    }

    private void OnReturnToTitlePressed()
    {
        GameManager.Instance.ReturnToTitleScreen();
    }
}