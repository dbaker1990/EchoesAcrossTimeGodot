using Godot;
using System;

namespace EchoesAcrossTime.Shops
{
    /// <summary>
    /// Notification popup for shop events
    /// Shows messages like "New item available!" or "Shop unlocked!"
    /// </summary>
    public partial class ShopNotification : Control
    {
        [ExportGroup("UI References")]
        [Export] private PanelContainer notificationPanel;
        [Export] private Label messageLabel;
        [Export] private TextureRect iconRect;
        
        [ExportGroup("Animation Settings")]
        [Export] private float displayDuration = 3.0f;
        [Export] private float fadeInDuration = 0.3f;
        [Export] private float fadeOutDuration = 0.3f;
        
        private Tween currentTween;
        private Vector2 originalPosition;
        private bool isShowing = false;
        
        public override void _Ready()
        {
            // Connect to shop manager signals
            if (ShopManager.Instance != null)
            {
                ShopManager.Instance.ShopOpened += OnShopOpened;
                ShopManager.Instance.ItemBought += OnItemBought;
                ShopManager.Instance.TransactionFailed += OnTransactionFailed;
            }
            
            // Store original position
            if (notificationPanel != null)
            {
                originalPosition = notificationPanel.Position;
                notificationPanel.Modulate = new Color(1, 1, 1, 0); // Start invisible
            }
            
            // Hide by default
            Hide();
        }
        
        public override void _ExitTree()
        {
            if (ShopManager.Instance != null)
            {
                ShopManager.Instance.ShopOpened -= OnShopOpened;
                ShopManager.Instance.ItemBought -= OnItemBought;
                ShopManager.Instance.TransactionFailed -= OnTransactionFailed;
            }
        }
        
        #region Signal Handlers
        
        private void OnShopOpened(ShopData shop)
        {
            // Don't show notification for normal shop opens
            // Only for special cases
        }
        
        private void OnItemBought(string itemId, int quantity, int totalCost)
        {
            string itemName = GetItemName(itemId);
            ShowNotification($"Purchased {quantity}x {itemName}!", NotificationType.Success);
        }
        
        private void OnTransactionFailed(string reason)
        {
            ShowNotification(reason, NotificationType.Error);
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Show a notification message
        /// </summary>
        public void ShowNotification(string message, NotificationType type = NotificationType.Info)
        {
            if (isShowing)
            {
                // Cancel current notification
                currentTween?.Kill();
            }
            
            // Set message
            if (messageLabel != null)
                messageLabel.Text = message;
            
            // Set color based on type
            Color color = type switch
            {
                NotificationType.Success => new Color(0.2f, 0.8f, 0.2f),
                NotificationType.Error => new Color(0.8f, 0.2f, 0.2f),
                NotificationType.Warning => new Color(0.8f, 0.6f, 0.2f),
                _ => new Color(0.4f, 0.6f, 0.8f)
            };
            
            if (notificationPanel != null)
            {
                var styleBox = notificationPanel.GetThemeStylebox("panel");
                if (styleBox is StyleBoxFlat flatBox)
                {
                    flatBox.BgColor = color;
                }
            }
            
            Show();
            AnimateNotification();
        }
        
        /// <summary>
        /// Show notification for shop unlock
        /// </summary>
        public void ShowShopUnlocked(string shopName)
        {
            ShowNotification($"🔓 {shopName} is now available!", NotificationType.Success);
            PlaySuccessSound();
        }
        
        /// <summary>
        /// Show notification for new item available
        /// </summary>
        public void ShowNewItemAvailable(string itemName, string shopName)
        {
            ShowNotification($"✨ New item available at {shopName}: {itemName}!", NotificationType.Info);
            PlaySuccessSound();
        }
        
        /// <summary>
        /// Show notification for shop restock
        /// </summary>
        public void ShowShopRestocked(string shopName)
        {
            ShowNotification($"📦 {shopName} has been restocked!", NotificationType.Info);
        }
        
        #endregion
        
        #region Animation
        
        private void AnimateNotification()
        {
            if (notificationPanel == null) return;
            
            isShowing = true;
            
            // Kill existing tween
            currentTween?.Kill();
            currentTween = CreateTween();
            
            // Reset position and alpha
            notificationPanel.Position = originalPosition + new Vector2(0, -50);
            notificationPanel.Modulate = new Color(1, 1, 1, 0);
            
            // Fade in and slide down
            currentTween.SetParallel(true);
            currentTween.TweenProperty(notificationPanel, "modulate:a", 1.0f, fadeInDuration);
            currentTween.TweenProperty(notificationPanel, "position", originalPosition, fadeInDuration)
                .SetTrans(Tween.TransitionType.Back)
                .SetEase(Tween.EaseType.Out);
            
            // Hold
            currentTween.Chain();
            currentTween.TweenInterval(displayDuration);
            
            // Fade out and slide up
            currentTween.SetParallel(true);
            currentTween.TweenProperty(notificationPanel, "modulate:a", 0.0f, fadeOutDuration);
            currentTween.TweenProperty(notificationPanel, "position", originalPosition + new Vector2(0, -30), fadeOutDuration)
                .SetTrans(Tween.TransitionType.Back)
                .SetEase(Tween.EaseType.In);
            
            // Hide when done
            currentTween.Chain();
            currentTween.TweenCallback(Callable.From(() =>
            {
                Hide();
                isShowing = false;
            }));
        }
        
        #endregion
        
        #region Helpers
        
        private string GetItemName(string itemId)
        {
            // TODO: Get from your ItemData
            // For now, just return the ID
            return itemId;
        }
        
        private void PlaySuccessSound()
        {
            var systemManager = GetNodeOrNull<Node>("/root/SystemManager");
            systemManager?.Call("PlayOkSE");
        }
        
        #endregion
        
        public enum NotificationType
        {
            Info,
            Success,
            Warning,
            Error
        }
    }
    
    /// <summary>
    /// Simple scene builder for ShopNotification
    /// </summary>
    [Tool]
    public partial class ShopNotificationBuilder : Node
    {
        [Export] public bool BuildScene { get; set; } = false;
        
        public override void _Process(double delta)
        {
            if (!Engine.IsEditorHint()) return;
            
            if (BuildScene)
            {
                BuildScene = false;
                CreateNotificationScene();
            }
        }
        
        private void CreateNotificationScene()
        {
            GD.Print("\n=== BUILDING SHOP NOTIFICATION SCENE ===\n");
            
            var root = new Control();
            root.Name = "ShopNotification";
            root.SetAnchorsPreset(Control.LayoutPreset.TopWide);
            root.OffsetTop = 50;
            root.OffsetBottom = 150;
            
            var centerContainer = new CenterContainer();
            centerContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            root.AddChild(centerContainer);
            centerContainer.Owner = root;
            
            var notificationPanel = new PanelContainer();
            notificationPanel.Name = "notificationPanel";
            notificationPanel.CustomMinimumSize = new Vector2(400, 80);
            centerContainer.AddChild(notificationPanel);
            notificationPanel.Owner = root;
            
            // Create StyleBox
            var styleBox = new StyleBoxFlat();
            styleBox.BgColor = new Color(0.2f, 0.3f, 0.5f, 0.95f);
            styleBox.CornerRadiusTopLeft = 10;
            styleBox.CornerRadiusTopRight = 10;
            styleBox.CornerRadiusBottomLeft = 10;
            styleBox.CornerRadiusBottomRight = 10;
            styleBox.BorderWidthBottom = 2;
            styleBox.BorderWidthLeft = 2;
            styleBox.BorderWidthRight = 2;
            styleBox.BorderWidthTop = 2;
            styleBox.BorderColor = new Color(0.8f, 0.8f, 0.8f, 0.5f);
            notificationPanel.AddThemeStyleboxOverride("panel", styleBox);
            
            var hbox = new HBoxContainer();
            hbox.AddThemeConstantOverride("separation", 15);
            notificationPanel.AddChild(hbox);
            hbox.Owner = root;
            
            var margin = new MarginContainer();
            margin.AddThemeConstantOverride("margin_left", 20);
            margin.AddThemeConstantOverride("margin_right", 20);
            margin.AddThemeConstantOverride("margin_top", 15);
            margin.AddThemeConstantOverride("margin_bottom", 15);
            notificationPanel.AddChild(margin);
            margin.Owner = root;
            
            var contentHBox = new HBoxContainer();
            contentHBox.AddThemeConstantOverride("separation", 15);
            margin.AddChild(contentHBox);
            contentHBox.Owner = root;
            
            var iconRect = new TextureRect();
            iconRect.Name = "iconRect";
            iconRect.CustomMinimumSize = new Vector2(48, 48);
            iconRect.ExpandMode = TextureRect.ExpandModeEnum.FitWidth;
            iconRect.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
            contentHBox.AddChild(iconRect);
            iconRect.Owner = root;
            
            var messageLabel = new Label();
            messageLabel.Name = "messageLabel";
            messageLabel.Text = "Notification message";
            messageLabel.AddThemeFontSizeOverride("font_size", 18);
            messageLabel.VerticalAlignment = VerticalAlignment.Center;
            messageLabel.AutowrapMode = TextServer.AutowrapMode.Word;
            contentHBox.AddChild(messageLabel);
            messageLabel.Owner = root;
            
            // Save scene
            var packedScene = new PackedScene();
            packedScene.Pack(root);
            
            string savePath = "res://Shops/ShopNotification.tscn";
            var error = ResourceSaver.Save(packedScene, savePath);
            
            if (error == Error.Ok)
            {
                GD.Print($"✅ ShopNotification scene saved to: {savePath}");
                GD.Print("\nNow:");
                GD.Print("1. Open ShopNotification.tscn");
                GD.Print("2. Attach ShopNotification.cs to root");
                GD.Print("3. Set exports in inspector");
                GD.Print("4. Add to main scene for global notifications");
            }
            else
            {
                GD.PushError($"Failed to save scene: {error}");
            }
        }
    }
}