// Bonds/SpouseSelectionUI.cs
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EchoesAcrossTime.Bonds
{
    /// <summary>
    /// UI for choosing spouse when multiple candidates are tied
    /// </summary>
    public partial class SpouseSelectionUI : Control
    {
        [Signal]
        public delegate void SpouseChosenEventHandler(string candidateId);
        
        [Export] public Panel MainPanel { get; set; }
        [Export] public Label TitleLabel { get; set; }
        [Export] public Label DescriptionLabel { get; set; }
        [Export] public VBoxContainer CandidatesContainer { get; set; }
        
        private List<RelationshipState> candidates;
        
        public override void _Ready()
        {
            Visible = false;
        }
        
        /// <summary>
        /// Show the spouse selection UI with tied candidates
        /// </summary>
        public void ShowSelection(List<RelationshipState> tiedCandidates)
        {
            if (tiedCandidates == null || tiedCandidates.Count == 0)
            {
                GD.PrintErr("[SpouseSelectionUI] No candidates provided!");
                return;
            }
            
            candidates = tiedCandidates;
            
            // Set title and description
            if (TitleLabel != null)
            {
                TitleLabel.Text = "Choose Your Life Partner";
            }
            
            if (DescriptionLabel != null)
            {
                DescriptionLabel.Text = "Multiple companions have deep bonds with you. Who will you choose to spend your life with?";
            }
            
            // Clear existing candidates
            if (CandidatesContainer != null)
            {
                foreach (Node child in CandidatesContainer.GetChildren())
                {
                    child.QueueFree();
                }
                
                // Create candidate buttons
                foreach (var candidate in candidates)
                {
                    CreateCandidateButton(candidate);
                }
            }
            
            // Show the UI
            Visible = true;
            
            // Pause the game
            GetTree().Paused = true;
        }
        
        private void CreateCandidateButton(RelationshipState candidate)
        {
            var panel = new Panel();
            panel.CustomMinimumSize = new Vector2(0, 120);
            
            var margin = new MarginContainer();
            margin.AddThemeConstantOverride("margin_left", 15);
            margin.AddThemeConstantOverride("margin_right", 15);
            margin.AddThemeConstantOverride("margin_top", 10);
            margin.AddThemeConstantOverride("margin_bottom", 10);
            panel.AddChild(margin);
            
            var hbox = new HBoxContainer();
            margin.AddChild(hbox);
            
            // Portrait
            var portrait = new TextureRect();
            portrait.CustomMinimumSize = new Vector2(100, 100);
            portrait.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
            portrait.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
            
            // Try to load portrait
            var portraitPath = $"res://Images/MenuPortraits/{candidate.CandidateId}Menu.png";
            if (ResourceLoader.Exists(portraitPath))
            {
                portrait.Texture = GD.Load<Texture2D>(portraitPath);
            }
            hbox.AddChild(portrait);
            
            // Info section
            var vbox = new VBoxContainer();
            vbox.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            hbox.AddChild(vbox);
            
            // Name
            var nameLabel = new Label();
            nameLabel.Text = FormatCandidateName(candidate.CandidateId);
            nameLabel.AddThemeFontSizeOverride("font_size", 24);
            vbox.AddChild(nameLabel);
            
            // Relationship points
            var pointsLabel = new Label();
            pointsLabel.Text = $"Bond Level: {candidate.Points}";
            vbox.AddChild(pointsLabel);
            
            // Description
            var descLabel = new Label();
            descLabel.Text = GetCandidateDescription(candidate.CandidateId);
            descLabel.AutowrapMode = TextServer.AutowrapMode.Word;
            vbox.AddChild(descLabel);
            
            // Choose button
            var chooseButton = new Button();
            chooseButton.Text = "Choose";
            chooseButton.CustomMinimumSize = new Vector2(100, 40);
            
            var candidateId = candidate.CandidateId; // Capture for lambda
            chooseButton.Pressed += () => OnCandidateChosen(candidateId);
            
            hbox.AddChild(chooseButton);
            
            CandidatesContainer.AddChild(panel);
        }
        
        private string FormatCandidateName(string candidateId)
        {
            // Convert ID to display name (e.g., "aria" -> "Aria")
            if (string.IsNullOrEmpty(candidateId)) return "Unknown";
            return char.ToUpper(candidateId[0]) + candidateId.Substring(1).ToLower();
        }
        
        private string GetCandidateDescription(string candidateId)
        {
            // You can expand this with actual character descriptions from CharacterData
            return candidateId.ToLower() switch
            {
                "elara" => "A loyal companion whose courage never wavers.",
                "seraphine" => "A brilliant mind with a heart of gold.",
                "naledi" => "A steadfast ally through thick and thin.",
                "aria" => "A fellow time traveler with a mysterious past.",
                "elena" => "The war mage whose fierce spirit matches her determination.",
                "aldric" => "Your childhood friend whose wisdom guides you.",
                "echo" => "The enigmatic figure who sees beyond time itself.",
                _ => "A cherished companion who has walked beside you through countless trials."
            };
        }
        
        private void OnCandidateChosen(string candidateId)
        {
            // Emit signal
            EmitSignal(SignalName.SpouseChosen, candidateId);
            
            // Hide UI
            Visible = false;
            
            // Unpause game
            GetTree().Paused = false;
            
            GD.Print($"[SpouseSelectionUI] Player chose: {candidateId}");
        }
    }
}