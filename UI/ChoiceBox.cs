// UI/ChoiceBox.cs
using Godot;
using System;
using System.Collections.Generic;

namespace EchoesAcrossTime.UI
{
    public partial class ChoiceBox : Control
    {
        [Export] public Panel BackgroundPanel { get; set; }
        [Export] public VBoxContainer ChoiceContainer { get; set; }
        [Export] public AnimationPlayer AnimationPlayer { get; set; }
        [Export] public AudioStream SelectSound { get; set; }
        [Export] public AudioStream ConfirmSound { get; set; }
        
        // Styling properties
        [Export] public int ButtonMinHeight { get; set; } = 50;
        [Export] public int ButtonFontSize { get; set; } = 24;
        [Export] public float FadeInDuration { get; set; } = 0.3f;
        [Export] public float FadeOutDuration { get; set; } = 0.2f;
        
        private List<Button> choiceButtons = new List<Button>();
        private int selectedIndex = 0;
        
        [Signal]
        public delegate void ChoiceSelectedEventHandler(int choiceIndex);
        
        public override void _Ready()
        {
            Visible = false;
            
            // Auto-find nodes if not assigned
            if (BackgroundPanel == null)
                BackgroundPanel = GetNode<Panel>("BackgroundPanel");
            if (ChoiceContainer == null)
                ChoiceContainer = GetNode<VBoxContainer>("BackgroundPanel/MarginContainer/ChoiceContainer");
            if (AnimationPlayer == null)
                AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        }
        
        public void ShowChoices(List<string> choices, int defaultChoice = 0)
        {
            ClearChoices();
            
            // Create buttons for each choice
            for (int i = 0; i < choices.Count; i++)
            {
                var button = CreateStyledChoiceButton(choices[i], i);
                choiceButtons.Add(button);
                ChoiceContainer.AddChild(button);
            }
            
            // Adapt panel size to number of choices
            AdaptPanelSize(choices.Count);
            
            // Set default selection
            selectedIndex = Mathf.Clamp(defaultChoice, 0, choiceButtons.Count - 1);
            if (choiceButtons.Count > 0)
            {
                choiceButtons[selectedIndex].GrabFocus();
            }
            
            // Show with fade-in animation
            FadeIn();
        }
        
        private Button CreateStyledChoiceButton(string text, int index)
        {
            var button = new Button();
            button.Text = text;
            button.CustomMinimumSize = new Vector2(0, ButtonMinHeight);
            
            // Style the button to match your game's theme
            var normalStyle = CreateButtonStyle(new Color(0.15f, 0.15f, 0.25f), new Color(1f, 0.75f, 0.36f));
            var hoverStyle = CreateButtonStyle(new Color(0.2f, 0.2f, 0.35f), new Color(1f, 0.8f, 0.5f));
            var pressedStyle = CreateButtonStyle(new Color(0.25f, 0.25f, 0.4f), new Color(1f, 0.9f, 0.7f));
            var focusStyle = CreateButtonStyle(new Color(0.2f, 0.3f, 0.45f), new Color(0.3f, 0.6f, 1f));
            
            button.AddThemeStyleboxOverride("normal", normalStyle);
            button.AddThemeStyleboxOverride("hover", hoverStyle);
            button.AddThemeStyleboxOverride("pressed", pressedStyle);
            button.AddThemeStyleboxOverride("focus", focusStyle);
            
            // Set font size
            button.AddThemeFontSizeOverride("font_size", ButtonFontSize);
            
            // Connect button press
            button.Pressed += () => OnChoiceSelected(index);
            
            return button;
        }
        
        private StyleBoxFlat CreateButtonStyle(Color bgColor, Color borderColor)
        {
            var style = new StyleBoxFlat();
            style.BgColor = bgColor;
            style.BorderColor = borderColor;
            style.BorderWidthLeft = 2;
            style.BorderWidthTop = 2;
            style.BorderWidthRight = 2;
            style.BorderWidthBottom = 2;
            style.CornerRadiusTopLeft = 5;
            style.CornerRadiusTopRight = 5;
            style.CornerRadiusBottomLeft = 5;
            style.CornerRadiusBottomRight = 5;
            style.ContentMarginLeft = 15;
            style.ContentMarginTop = 10;
            style.ContentMarginRight = 15;
            style.ContentMarginBottom = 10;
            return style;
        }
        
        private void AdaptPanelSize(int choiceCount)
        {
            if (BackgroundPanel == null) return;
            
            // Calculate height based on number of choices
            // Base padding + (button height + spacing) * count
            int spacing = 10; // VBoxContainer separation
            int margins = 40; // Top + Bottom margins (20 each)
            int calculatedHeight = margins + (ButtonMinHeight * choiceCount) + (spacing * (choiceCount - 1));
            
            // Set a reasonable width
            int width = 600;
            
            // Clamp to reasonable sizes
            calculatedHeight = Mathf.Clamp(calculatedHeight, 150, 500);
            
            BackgroundPanel.CustomMinimumSize = new Vector2(width, calculatedHeight);
            BackgroundPanel.Size = new Vector2(width, calculatedHeight);
            
            // Center the panel on screen
            var viewportSize = GetViewportRect().Size;
            BackgroundPanel.Position = new Vector2(
                (viewportSize.X - width) / 2,
                (viewportSize.Y - calculatedHeight) / 2
            );
        }
        
        private void OnChoiceSelected(int index)
        {
            // Play sound effect
            if (ConfirmSound != null)
            {
                // Assuming you have an AudioManager singleton
                // AudioManager.Instance?.PlaySoundEffect(ConfirmSound);
            }
            
            EmitSignal(SignalName.ChoiceSelected, index);
            HideChoiceBox();
        }
        
        private void ClearChoices()
        {
            foreach (var button in choiceButtons)
            {
                button.QueueFree();
            }
            choiceButtons.Clear();
        }
        
        public void HideChoiceBox()
        {
            FadeOut();
        }
        
        private void FadeIn()
        {
            Visible = true;
            if (AnimationPlayer != null && AnimationPlayer.HasAnimation("fade_in"))
            {
                AnimationPlayer.Play("fade_in");
            }
            else
            {
                // Fallback tween if no AnimationPlayer
                Modulate = new Color(1, 1, 1, 0);
                var tween = CreateTween();
                tween.TweenProperty(this, "modulate", Colors.White, FadeInDuration);
            }
        }
        
        private void FadeOut()
        {
            if (AnimationPlayer != null && AnimationPlayer.HasAnimation("fade_out"))
            {
                AnimationPlayer.Play("fade_out");
                AnimationPlayer.AnimationFinished += OnFadeOutComplete;
            }
            else
            {
                // Fallback tween
                var tween = CreateTween();
                tween.TweenProperty(this, "modulate", new Color(1, 1, 1, 0), FadeOutDuration);
                tween.TweenCallback(Callable.From(() => {
                    Visible = false;
                    ClearChoices();
                }));
            }
        }
        
        private void OnFadeOutComplete(StringName animName)
        {
            if (animName == "fade_out")
            {
                Visible = false;
                ClearChoices();
                AnimationPlayer.AnimationFinished -= OnFadeOutComplete;
            }
        }
        
        // Keyboard navigation support
        public override void _Input(InputEvent @event)
        {
            if (!Visible || choiceButtons.Count == 0) return;
            
            if (@event.IsActionPressed("ui_down"))
            {
                selectedIndex = (selectedIndex + 1) % choiceButtons.Count;
                choiceButtons[selectedIndex].GrabFocus();
                GetViewport().SetInputAsHandled();
            }
            else if (@event.IsActionPressed("ui_up"))
            {
                selectedIndex--;
                if (selectedIndex < 0) selectedIndex = choiceButtons.Count - 1;
                choiceButtons[selectedIndex].GrabFocus();
                GetViewport().SetInputAsHandled();
            }
        }
    }
}