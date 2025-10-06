// LoreCodex/LoreCodexNotification.cs
using Godot;
using System;
using EchoesAcrossTime.LoreCodex;
using EchoesAcrossTime.Managers;

namespace EchoesAcrossTime.UI
{
    /// <summary>
    /// Displays notifications when new lore is discovered
    /// Add this scene to your main game scene
    /// </summary>
    public partial class LoreCodexNotification : Control
    {
        #region Export Nodes
        [Export] public Panel NotificationPanel { get; set; }
        [Export] public TextureRect IconRect { get; set; }
        [Export] public Label TitleLabel { get; set; }
        [Export] public Label DescriptionLabel { get; set; }
        [Export] public Label CategoryLabel { get; set; }
        #endregion

        #region Settings
        [Export] public float DisplayDuration { get; set; } = 3.0f;
        [Export] public float FadeInDuration { get; set; } = 0.3f;
        [Export] public float FadeOutDuration { get; set; } = 0.3f;
        [Export] public Vector2 HiddenPosition { get; set; } = new Vector2(0, -200);
        [Export] public Vector2 ShownPosition { get; set; } = new Vector2(0, 50);
        #endregion

        private Tween currentTween;
        private bool isShowing = false;

        public override void _Ready()
        {
            // Start hidden
            if (NotificationPanel != null)
            {
                NotificationPanel.Position = HiddenPosition;
                NotificationPanel.Modulate = new Color(1, 1, 1, 0);
            }

            // Connect to LoreCodexManager signals
            if (LoreCodexManager.Instance != null)
            {
                LoreCodexManager.Instance.LoreEntryDiscovered += OnLoreDiscovered;
                LoreCodexManager.Instance.LoreSectionUnlocked += OnSectionUnlocked;
            }
        }

        private void OnLoreDiscovered(string entryId)
        {
            var entry = LoreCodexManager.Instance?.GetLoreEntry(entryId);
            if (entry == null)
                return;

            ShowNotification(
                "New Lore Discovered!",
                entry.EntryName,
                entry.ShortDescription,
                entry.Portrait,
                entry.Category
            );

            // Play sound effect
            SystemManager.Instance?.PlayOkSE();
        }

        private void OnSectionUnlocked(string entryId, int sectionIndex)
        {
            var entry = LoreCodexManager.Instance?.GetLoreEntry(entryId);
            if (entry == null || sectionIndex >= entry.Sections.Count)
                return;

            var section = entry.Sections[sectionIndex];

            ShowNotification(
                "New Section Unlocked!",
                $"{entry.EntryName} - {section.SectionTitle}",
                "Check the Lore Codex for more information.",
                entry.Portrait,
                entry.Category
            );

            SystemManager.Instance?.PlayOkSE();
        }

        public void ShowNotification(
            string title, 
            string entryName, 
            string description, 
            Texture2D icon = null,
            LoreCategory? category = null)
        {
            if (isShowing)
            {
                // Queue this notification or skip it
                return;
            }

            isShowing = true;

            // Set content
            if (TitleLabel != null)
                TitleLabel.Text = title;

            if (DescriptionLabel != null)
                DescriptionLabel.Text = entryName;

            if (CategoryLabel != null && category.HasValue)
            {
                CategoryLabel.Text = $"[{category.Value}]";
                CategoryLabel.Visible = true;
            }
            else if (CategoryLabel != null)
            {
                CategoryLabel.Visible = false;
            }

            if (IconRect != null)
            {
                IconRect.Texture = icon;
                IconRect.Visible = icon != null;
            }

            // Animate in
            AnimateIn();
        }

        private void AnimateIn()
        {
            if (NotificationPanel == null)
                return;

            // Cancel existing tween
            currentTween?.Kill();
            currentTween = CreateTween();
            currentTween.SetParallel(true);

            // Slide in
            currentTween.TweenProperty(
                NotificationPanel, 
                "position", 
                ShownPosition, 
                FadeInDuration
            ).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);

            // Fade in
            currentTween.TweenProperty(
                NotificationPanel, 
                "modulate", 
                new Color(1, 1, 1, 1), 
                FadeInDuration
            );

            currentTween.SetParallel(false);

            // Wait
            currentTween.TweenInterval(DisplayDuration);

            // Animate out
            currentTween.TweenCallback(Callable.From(AnimateOut));
        }

        private void AnimateOut()
        {
            if (NotificationPanel == null)
                return;

            currentTween?.Kill();
            currentTween = CreateTween();
            currentTween.SetParallel(true);

            // Slide out
            currentTween.TweenProperty(
                NotificationPanel, 
                "position", 
                HiddenPosition, 
                FadeOutDuration
            ).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In);

            // Fade out
            currentTween.TweenProperty(
                NotificationPanel, 
                "modulate", 
                new Color(1, 1, 1, 0), 
                FadeOutDuration
            );

            currentTween.SetParallel(false);
            currentTween.TweenCallback(Callable.From(() => isShowing = false));
        }

        public override void _ExitTree()
        {
            // Disconnect signals
            if (LoreCodexManager.Instance != null)
            {
                LoreCodexManager.Instance.LoreEntryDiscovered -= OnLoreDiscovered;
                LoreCodexManager.Instance.LoreSectionUnlocked -= OnSectionUnlocked;
            }

            currentTween?.Kill();
        }
    }
}