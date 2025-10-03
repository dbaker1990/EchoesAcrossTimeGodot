// UI/ScreenEffects.cs
using Godot;
using System;
using System.Threading.Tasks;

namespace EchoesAcrossTime.UI
{
    /// <summary>
    /// Handles screen-wide visual effects
    /// </summary>
    public partial class ScreenEffects : CanvasLayer
    {
        [Export] public ColorRect FadeRect { get; set; }
        [Export] public ColorRect TintRect { get; set; }
        [Export] public ColorRect FlashRect { get; set; }
        [Export] public Node2D ShakeContainer { get; set; }
        
        private Vector2 originalShakePosition = Vector2.Zero;
        private bool isShaking = false;
        
        public override void _Ready()
        {
            // Create fade rect if not assigned
            if (FadeRect == null)
            {
                FadeRect = new ColorRect
                {
                    Name = "FadeRect",
                    Color = new Color(0, 0, 0, 0),
                    MouseFilter = Control.MouseFilterEnum.Ignore
                };
                AddChild(FadeRect);
                FadeRect.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            }
            
            // Create tint rect if not assigned
            if (TintRect == null)
            {
                TintRect = new ColorRect
                {
                    Name = "TintRect",
                    Color = new Color(1, 1, 1, 0),
                    MouseFilter = Control.MouseFilterEnum.Ignore
                };
                AddChild(TintRect);
                TintRect.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            }
            
            // Create flash rect if not assigned
            if (FlashRect == null)
            {
                FlashRect = new ColorRect
                {
                    Name = "FlashRect",
                    Color = new Color(1, 1, 1, 0),
                    MouseFilter = Control.MouseFilterEnum.Ignore
                };
                AddChild(FlashRect);
                FlashRect.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            }
            
            if (ShakeContainer != null)
            {
                originalShakePosition = ShakeContainer.Position;
            }
        }
        
        /// <summary>
        /// Fade screen in or out
        /// </summary>
        public async Task Fade(bool fadeOut, float duration, Color? fadeColor = null)
        {
            if (FadeRect == null) return;
            
            Color targetColor = fadeColor ?? Colors.Black;
            Color startColor = fadeOut ? new Color(targetColor.R, targetColor.G, targetColor.B, 0) 
                                        : targetColor;
            Color endColor = fadeOut ? targetColor 
                                      : new Color(targetColor.R, targetColor.G, targetColor.B, 0);
            
            FadeRect.Color = startColor;
            
            var tween = CreateTween();
            tween.TweenProperty(FadeRect, "color", endColor, duration);
            
            await ToSignal(tween, Tween.SignalName.Finished);
        }
        
        /// <summary>
        /// Fade to a specific alpha
        /// </summary>
        public async Task FadeToAlpha(float targetAlpha, float duration, Color? fadeColor = null)
        {
            if (FadeRect == null) return;
            
            Color baseColor = fadeColor ?? Colors.Black;
            Color targetColor = new Color(baseColor.R, baseColor.G, baseColor.B, targetAlpha);
            
            var tween = CreateTween();
            tween.TweenProperty(FadeRect, "color", targetColor, duration);
            
            await ToSignal(tween, Tween.SignalName.Finished);
        }
        
        /// <summary>
        /// Tint the screen with a color
        /// </summary>
        public async Task Tint(Color tintColor, float duration)
        {
            if (TintRect == null) return;
            
            var tween = CreateTween();
            tween.TweenProperty(TintRect, "color", tintColor, duration);
            
            await ToSignal(tween, Tween.SignalName.Finished);
        }
        
        /// <summary>
        /// Remove screen tint
        /// </summary>
        public async Task RemoveTint(float duration)
        {
            if (TintRect == null) return;
            
            var tween = CreateTween();
            tween.TweenProperty(TintRect, "color", new Color(1, 1, 1, 0), duration);
            
            await ToSignal(tween, Tween.SignalName.Finished);
        }
        
        /// <summary>
        /// Flash the screen
        /// </summary>
        public async Task Flash(Color flashColor, float duration)
        {
            if (FlashRect == null) return;
            
            FlashRect.Color = flashColor;
            
            var tween = CreateTween();
            tween.TweenProperty(FlashRect, "color", new Color(flashColor.R, flashColor.G, flashColor.B, 0), duration);
            
            await ToSignal(tween, Tween.SignalName.Finished);
        }
        
        /// <summary>
        /// Shake the screen
        /// </summary>
        public async Task Shake(float intensity, float duration)
        {
            if (ShakeContainer == null)
            {
                GD.PrintErr("ScreenEffects: ShakeContainer not assigned");
                return;
            }
            
            isShaking = true;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                if (!isShaking) break;
                
                float progress = elapsed / duration;
                float currentIntensity = intensity * (1f - progress); // Decay over time
                
                Vector2 offset = new Vector2(
                    GD.Randf() * currentIntensity * 2 - currentIntensity,
                    GD.Randf() * currentIntensity * 2 - currentIntensity
                );
                
                ShakeContainer.Position = originalShakePosition + offset;
                
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
                elapsed += (float)GetProcessDeltaTime();
            }
            
            ShakeContainer.Position = originalShakePosition;
            isShaking = false;
        }
        
        /// <summary>
        /// Stop shake immediately
        /// </summary>
        public void StopShake()
        {
            if (ShakeContainer != null)
            {
                ShakeContainer.Position = originalShakePosition;
            }
            isShaking = false;
        }
    }
}