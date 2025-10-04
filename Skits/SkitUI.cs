// Skits/SkitUI.cs
using Godot;
using System;
using System.Threading.Tasks;

namespace EchoesAcrossTime.Skits
{
    /// <summary>
    /// UI for displaying Tales-style skits with character portraits
    /// </summary>
    public partial class SkitUI : Control
    {
        [ExportGroup("Portrait Containers")]
        [Export] private Control leftPortraitContainer;
        [Export] private Control rightPortraitContainer;
        [Export] private TextureRect leftPortrait;
        [Export] private TextureRect rightPortrait;
        [Export] private Control leftHighlight;
        [Export] private Control rightHighlight;
        
        [ExportGroup("Dialogue Display")]
        [Export] private Control dialoguePanel;
        [Export] private Label speakerNameLabel;
        [Export] private Label dialogueLabel;
        [Export] private Label skipHintLabel;
        
        [ExportGroup("Background")]
        [Export] private Panel backgroundPanel;
        [Export] private ColorRect backgroundOverlay;
        
        [ExportGroup("Audio")]
        [Export] private AudioStreamPlayer voicePlayer;
        [Export] private AudioStreamPlayer textBlipPlayer;
        [Export] private AudioStreamPlayer skitStartSFX;
        [Export] private AudioStreamPlayer skitEndSFX;
        
        [ExportGroup("Settings")]
        [Export] private float textSpeed = 0.03f;
        [Export] private float portraitSlideSpeed = 0.3f;
        [Export] private bool autoAdvance = false;
        [Export] private float autoAdvanceDelay = 2.0f;
        
        // State
        private SkitData currentSkit;
        private int currentLineIndex = 0;
        private bool isTyping = false;
        private bool waitingForInput = false;
        private bool skipRequested = false;
        private string currentFullText = "";
        private float textTimer = 0f;
        private int currentCharIndex = 0;
        
        // Character tracking
        private string leftCharacterId = "";
        private string rightCharacterId = "";

        public override void _Ready()
        {
            Visible = false;
            
            if (skipHintLabel != null)
            {
                skipHintLabel.Text = "[Press X to Skip]";
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
                    dialogueLabel.Text = currentFullText.Substring(0, currentCharIndex);
                    
                    // Play text blip
                    if (currentCharIndex % 2 == 0 && textBlipPlayer != null)
                    {
                        textBlipPlayer.Play();
                    }
                    
                    textTimer -= textSpeed;
                }
                
                if (currentCharIndex >= currentFullText.Length)
                {
                    isTyping = false;
                    waitingForInput = true;
                }
            }
        }

        public override void _Input(InputEvent @event)
        {
            if (!Visible) return;

            // Skip entire skit
            if (@event.IsActionPressed("ui_cancel"))
            {
                skipRequested = true;
            }
            
            // Advance dialogue
            if (@event.IsActionPressed("ui_accept") || @event.IsActionPressed("interact"))
            {
                if (isTyping)
                {
                    // Complete current text immediately
                    CompleteCurrentText();
                }
                else if (waitingForInput)
                {
                    // Advance to next line
                    AdvanceToNextLine();
                }
            }
        }

        /// <summary>
        /// Play a complete skit
        /// </summary>
        public async Task PlaySkit(SkitData skit)
        {
            if (skit == null || skit.Lines.Count == 0)
                return;

            currentSkit = skit;
            currentLineIndex = 0;
            skipRequested = false;
            leftCharacterId = "";
            rightCharacterId = "";

            // Show UI
            await OpenSkitUI();

            // Play each line
            while (currentLineIndex < skit.Lines.Count && !skipRequested)
            {
                var line = skit.Lines[currentLineIndex];
                await DisplayLine(line);
                
                // Wait for input or auto-advance
                if (autoAdvance && !skipRequested)
                {
                    await ToSignal(GetTree().CreateTimer(autoAdvanceDelay), SceneTreeTimer.SignalName.Timeout);
                    AdvanceToNextLine();
                }
                else
                {
                    // Manual advance (handled in _Input)
                    while (waitingForInput && !skipRequested)
                    {
                        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
                    }
                }
            }

            // Close UI
            await CloseSkitUI();
        }

        /// <summary>
        /// Display a single line of dialogue
        /// </summary>
        private async Task DisplayLine(SkitLine line)
        {
            // Update portraits
            await UpdatePortraits(line.CharacterId, line.PortraitPath);
            
            // Update speaker name
            if (speakerNameLabel != null)
            {
                speakerNameLabel.Text = GetCharacterDisplayName(line.CharacterId);
            }
            
            // Start typing effect
            currentFullText = line.Text;
            currentCharIndex = 0;
            textTimer = 0f;
            isTyping = true;
            waitingForInput = false;
            
            if (dialogueLabel != null)
            {
                dialogueLabel.Text = "";
            }
            
            // Play voice clip if available
            if (voicePlayer != null && !string.IsNullOrEmpty(line.VoiceClipPath))
            {
                var voiceClip = GD.Load<AudioStream>(line.VoiceClipPath);
                if (voiceClip != null)
                {
                    voicePlayer.Stream = voiceClip;
                    voicePlayer.Play();
                }
            }
        }

        /// <summary>
        /// Update character portraits - Tales style positioning
        /// </summary>
        private async Task UpdatePortraits(string speakerId, string portraitPath)
        {
            bool isLeftSpeaker = leftCharacterId == speakerId;
            bool isRightSpeaker = rightCharacterId == speakerId;
            
            // If character not on screen, add them
            if (!isLeftSpeaker && !isRightSpeaker)
            {
                // Determine which side to place them
                if (string.IsNullOrEmpty(leftCharacterId))
                {
                    await SetPortrait(true, portraitPath);
                    leftCharacterId = speakerId;
                    isLeftSpeaker = true;
                }
                else if (string.IsNullOrEmpty(rightCharacterId))
                {
                    await SetPortrait(false, portraitPath);
                    rightCharacterId = speakerId;
                    isRightSpeaker = true;
                }
                else
                {
                    // Both sides occupied - replace the non-speaker
                    if (leftCharacterId != speakerId)
                    {
                        await SetPortrait(true, portraitPath);
                        leftCharacterId = speakerId;
                        isLeftSpeaker = true;
                    }
                    else
                    {
                        await SetPortrait(false, portraitPath);
                        rightCharacterId = speakerId;
                        isRightSpeaker = true;
                    }
                }
            }
            
            // Highlight the current speaker
            UpdateSpeakerHighlight(isLeftSpeaker);
        }

        /// <summary>
        /// Set portrait on left or right side
        /// </summary>
        private async Task SetPortrait(bool isLeft, string portraitPath)
        {
            var portraitRect = isLeft ? leftPortrait : rightPortrait;
            var container = isLeft ? leftPortraitContainer : rightPortraitContainer;
            
            if (portraitRect != null && !string.IsNullOrEmpty(portraitPath))
            {
                var texture = GD.Load<Texture2D>(portraitPath);
                if (texture != null)
                {
                    portraitRect.Texture = texture;
                    
                    // Slide in animation
                    if (container != null)
                    {
                        var tween = CreateTween();
                        container.Modulate = new Color(1, 1, 1, 0);
                        tween.TweenProperty(container, "modulate:a", 1.0f, portraitSlideSpeed);
                        await ToSignal(tween, Tween.SignalName.Finished);
                    }
                }
            }
        }

        /// <summary>
        /// Highlight the current speaker
        /// </summary>
        private void UpdateSpeakerHighlight(bool leftIsSpeaking)
        {
            if (leftHighlight != null)
            {
                leftHighlight.Visible = leftIsSpeaking;
            }
            if (rightHighlight != null)
            {
                rightHighlight.Visible = !leftIsSpeaking;
            }
            
            // Dim non-speaker
            if (leftPortraitContainer != null)
            {
                leftPortraitContainer.Modulate = leftIsSpeaking ? 
                    Colors.White : new Color(0.5f, 0.5f, 0.5f);
            }
            if (rightPortraitContainer != null)
            {
                rightPortraitContainer.Modulate = !leftIsSpeaking ? 
                    Colors.White : new Color(0.5f, 0.5f, 0.5f);
            }
        }

        /// <summary>
        /// Complete current text immediately
        /// </summary>
        private void CompleteCurrentText()
        {
            if (isTyping && dialogueLabel != null)
            {
                currentCharIndex = currentFullText.Length;
                dialogueLabel.Text = currentFullText;
                isTyping = false;
                waitingForInput = true;
            }
        }

        /// <summary>
        /// Advance to next line
        /// </summary>
        private void AdvanceToNextLine()
        {
            waitingForInput = false;
            currentLineIndex++;
        }

        /// <summary>
        /// Open skit UI with animation
        /// </summary>
        private async Task OpenSkitUI()
        {
            Visible = true;
            
            if (skitStartSFX != null)
            {
                skitStartSFX.Play();
            }
            
            // Fade in animation
            Modulate = new Color(1, 1, 1, 0);
            var tween = CreateTween();
            tween.TweenProperty(this, "modulate:a", 1.0f, 0.3f);
            await ToSignal(tween, Tween.SignalName.Finished);
        }

        /// <summary>
        /// Close skit UI with animation
        /// </summary>
        private async Task CloseSkitUI()
        {
            if (skitEndSFX != null)
            {
                skitEndSFX.Play();
            }
            
            // Fade out animation
            var tween = CreateTween();
            tween.TweenProperty(this, "modulate:a", 0.0f, 0.3f);
            await ToSignal(tween, Tween.SignalName.Finished);
            
            Visible = false;
            
            // Clear portraits
            if (leftPortrait != null) leftPortrait.Texture = null;
            if (rightPortrait != null) rightPortrait.Texture = null;
            leftCharacterId = "";
            rightCharacterId = "";
        }

        /// <summary>
        /// Skip the entire skit
        /// </summary>
        public void SkipSkit()
        {
            skipRequested = true;
            isTyping = false;
            waitingForInput = false;
        }

        /// <summary>
        /// Get character display name from ID
        /// </summary>
        private string GetCharacterDisplayName(string characterId)
        {
            // You can integrate with your CharacterDatabase here
            // For now, just return the ID capitalized
            if (string.IsNullOrEmpty(characterId))
                return "???";
            
            return char.ToUpper(characterId[0]) + characterId.Substring(1);
        }
    }
}