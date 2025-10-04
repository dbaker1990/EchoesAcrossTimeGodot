// Skits/SkitNotification.cs
using Godot;
using System.Collections.Generic;

namespace EchoesAcrossTime.Skits
{
    /// <summary>
    /// Shows a notification icon (like Tales "!") when skits are available
    /// Attach to HUD or UI layer
    /// </summary>
    public partial class SkitNotification : Control
    {
        [ExportGroup("Notification Settings")]
        [Export] private Texture2D notificationIcon;
        [Export] private Color iconColor = new Color(1, 1, 0); // Yellow
        [Export] private float bobSpeed = 2.0f;
        [Export] private float bobHeight = 10.0f;
        [Export] private bool pulseEffect = true;
        
        [ExportGroup("UI References")]
        [Export] private TextureRect iconRect;
        [Export] private Label skitCountLabel;
        [Export] private Button openMenuButton;
        
        // State
        private List<string> availableSkitIds = new();
        private float bobTimer = 0f;
        private Vector2 originalPosition;
        private bool isVisible = false;

        public override void _Ready()
        {
            // Setup icon
            if (iconRect == null)
            {
                iconRect = GetNodeOrNull<TextureRect>("IconRect");
            }
            
            if (iconRect != null)
            {
                originalPosition = iconRect.Position;
                if (notificationIcon != null)
                {
                    iconRect.Texture = notificationIcon;
                }
                iconRect.Modulate = iconColor;
            }
            
            // Setup button
            if (openMenuButton == null)
            {
                openMenuButton = GetNodeOrNull<Button>("OpenMenuButton");
            }
            
            if (openMenuButton != null)
            {
                openMenuButton.Pressed += OnOpenMenuPressed;
            }
            
            // Connect to SkitManager
            if (SkitManager.Instance != null)
            {
                SkitManager.Instance.SkitAvailable += OnSkitAvailable;
                SkitManager.Instance.SkitEnded += OnSkitEnded;
            }
            
            // Initial state
            Hide();
        }

        public override void _Process(double delta)
        {
            if (!isVisible || iconRect == null)
                return;
            
            // Bob animation
            bobTimer += (float)delta * bobSpeed;
            float bobOffset = Mathf.Sin(bobTimer) * bobHeight;
            iconRect.Position = originalPosition + new Vector2(0, bobOffset);
            
            // Pulse effect
            if (pulseEffect)
            {
                float pulse = (Mathf.Sin(bobTimer * 2) + 1) / 2; // 0 to 1
                float scale = 1.0f + (pulse * 0.2f); // 1.0 to 1.2
                iconRect.Scale = new Vector2(scale, scale);
            }
        }

        /// <summary>
        /// Called when a new skit becomes available
        /// </summary>
        private void OnSkitAvailable(string skitId)
        {
            if (!availableSkitIds.Contains(skitId))
            {
                availableSkitIds.Add(skitId);
                UpdateDisplay();
            }
        }

        /// <summary>
        /// Called when a skit is viewed
        /// </summary>
        private void OnSkitEnded(string skitId)
        {
            availableSkitIds.Remove(skitId);
            UpdateDisplay();
        }

        /// <summary>
        /// Update the notification display
        /// </summary>
        private void UpdateDisplay()
        {
            if (availableSkitIds.Count > 0)
            {
                ShowNotification();
                
                // Update count label
                if (skitCountLabel != null)
                {
                    skitCountLabel.Text = availableSkitIds.Count > 1 
                        ? $"{availableSkitIds.Count}" 
                        : "";
                }
            }
            else
            {
                HideNotification();
            }
        }

        /// <summary>
        /// Show the notification icon
        /// </summary>
        private void ShowNotification()
        {
            if (isVisible)
                return;
            
            isVisible = true;
            Show();
            
            // Play show animation
            if (iconRect != null)
            {
                var tween = CreateTween();
                Modulate = new Color(1, 1, 1, 0);
                tween.TweenProperty(this, "modulate:a", 1.0f, 0.3f);
                
                // Scale pop
                iconRect.Scale = Vector2.Zero;
                tween.Parallel().TweenProperty(iconRect, "scale", Vector2.One, 0.3f)
                    .SetTrans(Tween.TransitionType.Back)
                    .SetEase(Tween.EaseType.Out);
            }
        }

        /// <summary>
        /// Hide the notification icon
        /// </summary>
        private void HideNotification()
        {
            if (!isVisible)
                return;
            
            isVisible = false;
            
            // Play hide animation
            var tween = CreateTween();
            tween.TweenProperty(this, "modulate:a", 0.0f, 0.3f);
            tween.TweenCallback(Callable.From(() => Hide()));
        }

        /// <summary>
        /// Open skit selection menu
        /// </summary>
        private void OnOpenMenuPressed()
        {
            // Get available skits
            var skits = SkitManager.Instance?.GetAvailableSkits();
            if (skits == null || skits.Count == 0)
                return;
            
            // If only one skit, play it directly
            if (skits.Count == 1)
            {
                PlaySkit(skits[0].SkitId);
            }
            else
            {
                // Show selection menu
                ShowSkitSelectionMenu(skits);
            }
        }

        /// <summary>
        /// Show a menu to select which skit to play
        /// </summary>
        private void ShowSkitSelectionMenu(List<SkitData> skits)
        {
            // Create simple popup menu
            var popup = new PopupMenu();
            AddChild(popup);
            
            for (int i = 0; i < skits.Count; i++)
            {
                var skit = skits[i];
                popup.AddItem(skit.SkitTitle, i);
            }
            
            popup.IdPressed += (id) => 
            {
                if (id >= 0 && id < skits.Count)
                {
                    PlaySkit(skits[(int)id].SkitId);
                }
                popup.QueueFree();
            };
            
            popup.PopupCentered();
        }

        /// <summary>
        /// Play a specific skit
        /// </summary>
        private async void PlaySkit(string skitId)
        {
            if (SkitManager.Instance != null)
            {
                await SkitManager.Instance.PlaySkit(skitId);
            }
        }

        /// <summary>
        /// Get number of available skits
        /// </summary>
        public int GetAvailableSkitCount()
        {
            return availableSkitIds.Count;
        }

        /// <summary>
        /// Check if any skits are available
        /// </summary>
        public bool HasAvailableSkits()
        {
            return availableSkitIds.Count > 0;
        }

        /// <summary>
        /// Manually refresh available skits
        /// </summary>
        public void RefreshAvailableSkits()
        {
            availableSkitIds.Clear();
            var availableSkits = SkitManager.Instance?.GetAvailableSkits();
            
            if (availableSkits != null)
            {
                foreach (var skit in availableSkits)
                {
                    availableSkitIds.Add(skit.SkitId);
                }
            }
            
            UpdateDisplay();
        }
    }
}