// UI/MessageBox.cs
using Godot;
using System;
using EchoesAcrossTime.Events;

namespace EchoesAcrossTime.UI
{
    public partial class MessageBox : Control
    {
        [Export] public Panel BackgroundPanel { get; set; }
        [Export] public Label SpeakerNameLabel { get; set; }
        [Export] public RichTextLabel MessageLabel { get; set; }
        [Export] public TextureRect PortraitRect { get; set; }
        [Export] public AnimationPlayer AnimationPlayer { get; set; }
        [Export] public AudioStreamPlayer SoundEffectPlayer { get; set; }
        
        [ExportGroup("Styling")]
        [Export] public StyleBox MessageBoxStyle { get; set; }
        [Export] public Color SpeakerNameColor { get; set; } = Colors.White;
        [Export] public Color MessageTextColor { get; set; } = Colors.White;
        [Export] public int FontSize { get; set; } = 24;
        
        [ExportGroup("Animation")]
        [Export] public float FadeInDuration { get; set; } = 0.3f;
        [Export] public float FadeOutDuration { get; set; } = 0.2f;
        [Export] public AudioStream TextBlipSound { get; set; }
        
        
        private string currentFullText = "";
        private string currentDisplayText = "";
        private float textTimer = 0f;
        private float textSpeed = 0.05f;
        private int currentCharIndex = 0;
        private bool isTyping = false;
        private bool waitingForInput = false;
        
        [Signal]
        public delegate void MessageCompletedEventHandler();
        
        [Signal]
        public delegate void MessageAdvancedEventHandler();
        
        public override void _Ready()
        {
           // Visible = false; 
            
            if (BackgroundPanel != null && MessageBoxStyle != null)
            {
                BackgroundPanel.AddThemeStyleboxOverride("panel", MessageBoxStyle);
            }
            
            if (MessageLabel != null)
            {
                MessageLabel.AddThemeColorOverride("default_color", MessageTextColor);
                MessageLabel.AddThemeFontSizeOverride("normal_font_size", FontSize);
            }
            
            if (SpeakerNameLabel != null)
            {
                SpeakerNameLabel.AddThemeColorOverride("font_color", SpeakerNameColor);
            }
        }
        
        public override void _Process(double delta)
        {
            if (isTyping)
            {
                textTimer += (float)delta;
                
                while (textTimer >= textSpeed && currentCharIndex < currentFullText.Length)
                {
                    currentCharIndex++;
                    currentDisplayText = currentFullText.Substring(0, currentCharIndex);
                    MessageLabel.Text = currentDisplayText;
                    
                    // Play text blip sound
                    if (TextBlipSound != null && currentCharIndex % 2 == 0)
                    {
                        SoundEffectPlayer?.Play();
                    }
                    
                    textTimer -= textSpeed;
                }
                
                if (currentCharIndex >= currentFullText.Length)
                {
                    isTyping = false;
                    waitingForInput = true;
                    OnTypingComplete();
                }
            }
        }
        
        public override void _Input(InputEvent @event)
        {
            if (!Visible) return;
            
            if (@event.IsActionPressed("ui_accept") || @event.IsActionPressed("interact"))
            {
                if (isTyping)
                {
                    // Skip to end of text
                    CompleteText();
                }
                else if (waitingForInput)
                {
                    // Advance to next message
                    EmitSignal(SignalName.MessageAdvanced);
                }
            }
        }
        
        public void ShowMessage(DialogueData dialogue)
        {
            if (dialogue == null) return;
            
            // Set speaker name
            if (SpeakerNameLabel != null)
            {
                SpeakerNameLabel.Text = dialogue.ShowSpeakerName ? dialogue.SpeakerName : "";
                SpeakerNameLabel.Visible = dialogue.ShowSpeakerName && !string.IsNullOrEmpty(dialogue.SpeakerName);
            }
            
            // Set portrait
            if (PortraitRect != null)
            {
                PortraitRect.Visible = dialogue.ShowPortrait && !string.IsNullOrEmpty(dialogue.PortraitPath);
                if (PortraitRect.Visible)
                {
                    PortraitRect.Texture = GD.Load<Texture2D>(dialogue.PortraitPath);
                }
            }
            
            // Set position
            SetMessageBoxPosition(dialogue.Position);
            
            // Start displaying text
            currentFullText = dialogue.Text;
            currentDisplayText = "";
            currentCharIndex = 0;
            textSpeed = dialogue.TextSpeed;
            textTimer = 0f;
            
            if (dialogue.UseTypewriterEffect)
            {
                isTyping = true;
                waitingForInput = false;
                MessageLabel.Text = "";
            }
            else
            {
                MessageLabel.Text = currentFullText;
                isTyping = false;
                waitingForInput = true;
            }
            
            // Show message box
            if (!Visible)
            {
                FadeIn();
            }
        }
        
        public void CompleteText()
        {
            if (isTyping)
            {
                currentCharIndex = currentFullText.Length;
                currentDisplayText = currentFullText;
                MessageLabel.Text = currentDisplayText;
                isTyping = false;
                waitingForInput = true;
                OnTypingComplete();
            }
        }
        
        private void OnTypingComplete()
        {
            // Could show a "continue" indicator here
        }
        
        public void HideMessageBox()  // Renamed from Hide to avoid conflict
        {
            FadeOut();
        }
        
        private void FadeIn()
        {
            Visible = true;
            Modulate = new Color(1, 1, 1, 0);
            
            var tween = CreateTween();
            tween.TweenProperty(this, "modulate", Colors.White, FadeInDuration);
        }
        
        private void FadeOut()
        {
            var tween = CreateTween();
            tween.TweenProperty(this, "modulate", new Color(1, 1, 1, 0), FadeOutDuration);
            tween.TweenCallback(Callable.From(() => {
                Visible = false;
                EmitSignal(SignalName.MessageCompleted);
            }));
        }
        
        private void SetMessageBoxPosition(MessageBoxPosition position)
        {
            if (BackgroundPanel == null) return;
    
            var viewportSize = GetViewportRect().Size;
    
            switch (position)
            {
                case MessageBoxPosition.Top:
                    // Anchor to top
                    BackgroundPanel.SetAnchorsPreset(Control.LayoutPreset.TopWide);
                    BackgroundPanel.OffsetLeft = 50;
                    BackgroundPanel.OffsetTop = 50;
                    BackgroundPanel.OffsetRight = -50;
                    BackgroundPanel.OffsetBottom = 350; // Height of message box
                    break;
            
                case MessageBoxPosition.Middle:
                    // Anchor to center
                    BackgroundPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
                    BackgroundPanel.SetSize(new Vector2(1720, 300));
                    BackgroundPanel.Position = new Vector2(
                        (viewportSize.X - 1720) / 2, 
                        (viewportSize.Y - 300) / 2
                    );
                    break;
            
                case MessageBoxPosition.Bottom:
                    // Anchor to bottom
                    BackgroundPanel.SetAnchorsPreset(Control.LayoutPreset.BottomWide);
                    BackgroundPanel.OffsetLeft = 50;
                    BackgroundPanel.OffsetTop = -350; // Negative to go up from bottom
                    BackgroundPanel.OffsetRight = -50;
                    BackgroundPanel.OffsetBottom = -50;
                    break;
            
                case MessageBoxPosition.AboveCharacter:
                    // This would need a character reference - for now, default to top
                    BackgroundPanel.SetAnchorsPreset(Control.LayoutPreset.TopWide);
                    BackgroundPanel.OffsetLeft = 50;
                    BackgroundPanel.OffsetTop = 50;
                    BackgroundPanel.OffsetRight = -50;
                    BackgroundPanel.OffsetBottom = 350;
                    break;
            }
        }
    }
}